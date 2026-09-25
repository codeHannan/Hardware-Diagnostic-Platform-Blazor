using Microsoft.JSInterop;
using BenchRig.App.Core.Constants;
using BenchRig.App.Core.Engines.Audio;
using BenchRig.App.Core.Engines.Monitor;
using BenchRig.App.Core.Engines.Compute;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Audio;
using BenchRig.App.Core.Models.Compute;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Orchestrators;

public sealed class AudioTestOrchestrator
{
    private readonly IJsAudioBridge _bridge;
    private readonly AudioDiagState _state;
    private readonly LatencyCalculator _latency = new();

    public AudioTestOrchestrator(IJsAudioBridge bridge, AudioDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    public Task<int> GetMaxChannelsAsync() => _bridge.GetMaxChannelsAsync();

    public async Task PlayChannelAsync(int channelIndex, int totalChannels, int freq = 440)
    {
        _state.SetPlaying(true);
        await _bridge.PlayChannelToneAsync(channelIndex, totalChannels, freq);
    }

    public async Task PlayPannedAsync(double pan, int freq = 440)
    {
        _state.SetPlaying(true);
        await _bridge.PlayPannedToneAsync(pan, freq);
    }

    public async Task StartBalanceToneAsync(int freq = 440)
    {
        _state.SetPlaying(true);
        await _bridge.StartBalanceToneAsync(freq);
    }

    public Task SetPanAsync(double pan) => _bridge.SetPanAsync(pan);

    public async Task PlaySpatialAsync()
    {
        _state.SetPlaying(true);
        await _bridge.StartSpatialSweepAsync();
    }

    public async Task StopAsync()
    {
        await _bridge.StopPlaybackAsync();
        _state.SetPlaying(false);
    }

    public async Task<bool> StartMicAsync()
    {
        bool ok = await _bridge.StartMicCaptureAsync();
        _state.SetMicState(new MicLoopbackState { IsCapturing = ok });
        return ok;
    }

    public async Task StopMicAsync()
    {
        await _bridge.StopMicCaptureAsync();
        _state.SetMicState(new MicLoopbackState { IsCapturing = false });
    }

    public Task StartLoopbackAsync() => _bridge.StartLoopbackAsync();
    public Task StopLoopbackAsync() => _bridge.StopLoopbackAsync();

    public async Task SampleMicLevelAsync() => _state.SetMicLevel(await _bridge.GetMicLevelDbAsync());
    public Task<int[]> SampleWaveformAsync() => _bridge.GetMicWaveformAsync();

    public async Task MeasureEchoAsync()
    {
        double rt = await _bridge.MeasureEchoLatencyAsync();
        _state.SetEcho(_latency.Calculate(rt, rt > 0 ? 0.8 : 0));
    }

    public Task<double> GetSystemLatencyAsync() => _bridge.GetSystemLatencyAsync();
}

public sealed class MonitorTestOrchestrator
{
    private readonly IJsMonitorBridge _bridge;
    private readonly MonitorDiagState _state;
    private readonly RefreshRateCalculator _refresh = new();

    public MonitorTestOrchestrator(IJsMonitorBridge bridge, MonitorDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    public async Task EnterFullscreenAsync(string elementId)
    {
        await _bridge.EnterFullscreenAsync(elementId);
        _state.SetFullscreen(true);
    }

    public async Task ExitFullscreenAsync()
    {
        await _bridge.ExitFullscreenAsync();
        _state.SetFullscreen(false);
    }

    public Task EnterFullscreenRawAsync(string elementId) => _bridge.EnterFullscreenAsync(elementId);
    public Task ExitFullscreenRawAsync() => _bridge.ExitFullscreenAsync();
    public Task WatchFullscreenExitAsync<T>(DotNetObjectReference<T> callbackRef, string method) where T : class
        => _bridge.WatchFullscreenExitAsync(callbackRef, method);

    /// <summary>Measures refresh rate + frame-time stats over the window. Returns (hz, minHz, maxHz, jitterMs, dropped).</summary>
    public async Task<RefreshStats> MeasureRefreshAsync(double expectedHz, int durationMs = 3000)
    {
        var deltas = (await _bridge.MeasureFrameDeltasAsync(durationMs)).Where(d => d > 0).ToList();
        if (deltas.Count == 0) return new RefreshStats(0, 0, 0, 0, 0, 0);

        double avg = deltas.Average();
        double hz = 1000.0 / avg;
        double minHz = 1000.0 / deltas.Max();   // longest frame -> lowest Hz
        double maxHz = 1000.0 / deltas.Min();
        // jitter = mean abs consecutive delta diff
        double jitter = 0;
        for (int i = 1; i < deltas.Count; i++) jitter += Math.Abs(deltas[i] - deltas[i - 1]);
        jitter = deltas.Count > 1 ? jitter / (deltas.Count - 1) : 0;
        // dropped = frames longer than 1.5x the median
        var sorted = deltas.OrderBy(d => d).ToList();
        double median = sorted[sorted.Count / 2];
        int dropped = deltas.Count(d => d > median * 1.5);

        _state.SetRefreshRate(_refresh.Calculate(hz, expectedHz));
        return new RefreshStats(hz, minHz, maxHz, jitter, dropped, deltas.Count);
    }
}

public readonly record struct RefreshStats(double Hz, double MinHz, double MaxHz, double JitterMs, int Dropped, int Frames);

public sealed class ComputeTestOrchestrator : IAsyncDisposable
{
    private readonly IJsComputeBridge _bridge;
    private readonly ComputeDiagState _state;
    private readonly CpuScoreCalculator _cpuScore = new();
    private readonly GpuScoreCalculator _gpuScore = new();
    private readonly MemoryScoreCalculator _memScore = new();
    private DotNetObjectReference<ComputeTestOrchestrator>? _selfRef;

    public ComputeTestOrchestrator(IJsComputeBridge bridge, ComputeDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    private DotNetObjectReference<ComputeTestOrchestrator> SelfRef => _selfRef ??= DotNetObjectReference.Create(this);

    public async Task DetectHardwareAsync()
    {
        try { _state.SetHardware(await _bridge.DetectHardwareAsync()); }
        catch { /* leave null */ }
    }

    public async Task RunCpuAsync(int durationMsPerPass)
    {
        int threads = _state.Hardware?.Threads is > 0 ? _state.Hardware!.Threads : Environment.ProcessorCount;
        _state.SetRunning(true);
        _state.SetPhase("Single-core then multi-core…");
        try
        {
            var r = await _bridge.RunCpuBenchmarkAsync(threads, durationMsPerPass, SelfRef);
            double single = _cpuScore.SingleScore(r.SingleUps);
            double multi = _cpuScore.MultiScore(r.MultiUps);
            _state.SetCpu(r with
            {
                SingleScore = single,
                MultiScore = multi,
                EstGeekbenchSingle = _cpuScore.EstGeekbench(single),
                EstGeekbenchMulti = _cpuScore.EstGeekbench(multi),
            });
        }
        finally { _state.SetRunning(false); _state.SetPhase(""); }
    }

    public async Task RunGpuAsync(string canvasId, int width, int height, int complexity, int durationMs)
    {
        _state.SetRunning(true);
        _state.SetPhase($"Rendering {width}×{height} @ complexity {complexity}…");
        try
        {
            var r = await _bridge.RunGpuBenchmarkAsync(canvasId, width, height, complexity, durationMs, SelfRef);
            _state.SetGpu(r with { Score = _gpuScore.Score(r.AvgFps, r.Width, r.Height, r.Complexity) });
        }
        finally { _state.SetRunning(false); _state.SetPhase(""); }
    }

    public async Task RunMemoryAsync(int targetMb)
    {
        _state.SetRunning(true);
        _state.SetPhase($"Allocating up to {targetMb} MB…");
        try
        {
            var r = await _bridge.RunMemoryTestAsync(targetMb, SelfRef);
            _state.SetMemory(r with { Score = _memScore.Score(r.WriteGBs, r.ReadGBs, r.PeakMb) });
        }
        finally { _state.SetRunning(false); _state.SetPhase(""); }
    }

    [JSInvokable]
    public void OnProgress(double percent) => _state.SetProgress(percent);

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _bridge.DisposeAsync();
    }
}
