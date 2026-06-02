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

    public async Task PlayToneAsync(string audioPath, string channelId)
    {
        _state.SetPlaying(true);
        await _bridge.PlayTestToneAsync(audioPath, channelId);
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

    public async Task SampleMicLevelAsync() => _state.SetMicLevel(await _bridge.GetMicLevelDbAsync());

    public async Task MeasureEchoAsync()
    {
        double rt = await _bridge.MeasureEchoLatencyAsync();
        _state.SetEcho(_latency.Calculate(rt, rt > 0 ? 0.8 : 0));
    }
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

    public async Task MeasureRefreshRateAsync(double expectedHz = 60)
    {
        double hz = await _bridge.MeasureFrameRateAsync(DiagnosticDefaults.RefreshRateSampleFrames);
        _state.SetRefreshRate(_refresh.Calculate(hz, expectedHz));
    }

    public Task StartGhostingAsync(string canvasId, double speed) => _bridge.StartGhostingAnimationAsync(canvasId, speed);
    public Task StopGhostingAsync(string canvasId) => _bridge.StopGhostingAnimationAsync(canvasId);
}

public sealed class ComputeTestOrchestrator : IAsyncDisposable
{
    private readonly IJsComputeBridge _bridge;
    private readonly ComputeDiagState _state;
    private readonly CpuScoreCalculator _cpuScore = new();
    private readonly GpuScoreCalculator _gpuScore = new();
    private DotNetObjectReference<ComputeTestOrchestrator>? _selfRef;

    public ComputeTestOrchestrator(IJsComputeBridge bridge, ComputeDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    private DotNetObjectReference<ComputeTestOrchestrator> SelfRef => _selfRef ??= DotNetObjectReference.Create(this);

    public async Task RunCpuAsync(int threads = DiagnosticDefaults.DefaultCpuThreads)
    {
        _state.SetRunning(true);
        _state.SetProgress(0);
        try
        {
            var r = await _bridge.RunCpuBenchmarkAsync(threads, DiagnosticDefaults.CpuBenchDurationSec, SelfRef);
            _state.SetCpu(r with { Score = _cpuScore.Score(r.OpsPerSec) });
        }
        finally { _state.SetRunning(false); }
    }

    public async Task RunGpuAsync(string canvasId)
    {
        _state.SetRunning(true);
        _state.SetProgress(0);
        try
        {
            var r = await _bridge.RunGpuBenchmarkAsync(canvasId, DiagnosticDefaults.GpuBenchDurationSec, SelfRef);
            _state.SetGpu(r with { Score = _gpuScore.Score(r.AvgFps) });
        }
        finally { _state.SetRunning(false); }
    }

    [JSInvokable]
    public void OnProgress(double percent) => _state.SetProgress(percent);

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _bridge.DisposeAsync();
    }
}
