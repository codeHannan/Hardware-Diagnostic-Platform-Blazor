using System.Text.Json;
using Microsoft.JSInterop;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Compute;

namespace BenchRig.App.Services.JsInterop;

public sealed class MouseJsInterop : ModuleInteropBase, IJsMouseBridge
{
    public MouseJsInterop(IJSRuntime js) : base(js, "./js/mouse-interop.js") { }

    public async Task StartPointerCaptureAsync<T>(string elementId, DotNetObjectReference<T> callbackRef) where T : class
        => await (await ModuleAsync()).InvokeVoidAsync("startPointerCapture", elementId, callbackRef);

    public async Task StopPointerCaptureAsync(string elementId)
        => await (await ModuleAsync()).InvokeVoidAsync("stopPointerCapture", elementId);

    public async Task StartPollingRateCaptureAsync<T>(string elementId, DotNetObjectReference<T> callbackRef) where T : class
        => await (await ModuleAsync()).InvokeVoidAsync("startPollingRateCapture", elementId, callbackRef);

    public async Task StopPollingRateCaptureAsync(string elementId)
        => await (await ModuleAsync()).InvokeVoidAsync("stopPollingRateCapture", elementId);
}

public sealed class KeyboardJsInterop : ModuleInteropBase, IJsKeyboardBridge
{
    public KeyboardJsInterop(IJSRuntime js) : base(js, "./js/keyboard-interop.js") { }

    public async Task StartKeyCaptureAsync<T>(DotNetObjectReference<T> callbackRef) where T : class
        => await (await ModuleAsync()).InvokeVoidAsync("startKeyCapture", callbackRef);

    public async Task StopKeyCaptureAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("stopKeyCapture");
}

public sealed class AudioJsInterop : ModuleInteropBase, IJsAudioBridge
{
    public AudioJsInterop(IJSRuntime js) : base(js, "./js/audio-interop.js") { }

    public async Task PlayTestToneAsync(string channelAudioPath, string outputChannelId)
        => await (await ModuleAsync()).InvokeVoidAsync("playTestTone", channelAudioPath, outputChannelId);
    public async Task StopPlaybackAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("stopPlayback");
    public async Task<bool> StartMicCaptureAsync()
        => await (await ModuleAsync()).InvokeAsync<bool>("startMicCapture");
    public async Task StopMicCaptureAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("stopMicCapture");
    public async Task StartLoopbackAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("startLoopback");
    public async Task StopLoopbackAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("stopLoopback");
    public async Task<double> MeasureEchoLatencyAsync()
        => await (await ModuleAsync()).InvokeAsync<double>("measureEchoLatency");
    public async Task<double> GetMicLevelDbAsync()
        => await (await ModuleAsync()).InvokeAsync<double>("getMicLevelDb");
}

public sealed class MonitorJsInterop : ModuleInteropBase, IJsMonitorBridge
{
    public MonitorJsInterop(IJSRuntime js) : base(js, "./js/monitor-interop.js") { }

    public async Task EnterFullscreenAsync(string elementId)
        => await (await ModuleAsync()).InvokeVoidAsync("enterFullscreen", elementId);
    public async Task ExitFullscreenAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("exitFullscreen");
    public async Task<double> MeasureFrameRateAsync(int sampleFrames)
        => await (await ModuleAsync()).InvokeAsync<double>("measureFrameRate", sampleFrames);
    public async Task StartGhostingAnimationAsync(string canvasId, double speedPxPerFrame)
        => await (await ModuleAsync()).InvokeVoidAsync("startGhosting", canvasId, speedPxPerFrame);
    public async Task StopGhostingAnimationAsync(string canvasId)
        => await (await ModuleAsync()).InvokeVoidAsync("stopGhosting", canvasId);
}

public sealed class ComputeJsInterop : ModuleInteropBase, IJsComputeBridge
{
    public ComputeJsInterop(IJSRuntime js) : base(js, "./js/compute-interop.js") { }

    public async Task<CpuBenchmarkResult> RunCpuBenchmarkAsync<T>(int threadCount, int durationSec, DotNetObjectReference<T> progressCallback) where T : class
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("runCpuBenchmark", threadCount, durationSec, progressCallback);
        return new CpuBenchmarkResult
        {
            Score = r.GetProperty("score").GetDouble(),
            ThreadsUsed = r.GetProperty("threadsUsed").GetInt32(),
            DurationMs = r.GetProperty("durationMs").GetDouble(),
            OpsPerSec = r.GetProperty("opsPerSec").GetDouble(),
        };
    }

    public async Task<GpuBenchmarkResult> RunGpuBenchmarkAsync<T>(string canvasId, int durationSec, DotNetObjectReference<T> progressCallback) where T : class
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("runGpuBenchmark", canvasId, durationSec, progressCallback);
        return new GpuBenchmarkResult
        {
            Score = r.GetProperty("score").GetDouble(),
            AvgFps = r.GetProperty("avgFps").GetDouble(),
            ShaderComplexity = r.GetProperty("shaderComplexity").GetInt32(),
            VramEstimateMb = r.GetProperty("vramEstimateMb").GetDouble(),
        };
    }
}
