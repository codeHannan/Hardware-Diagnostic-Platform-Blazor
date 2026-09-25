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

    public async Task<int> GetMaxChannelsAsync()
        => await (await ModuleAsync()).InvokeAsync<int>("getMaxChannels");
    public async Task PlayChannelToneAsync(int channelIndex, int totalChannels, int freq)
        => await (await ModuleAsync()).InvokeVoidAsync("playChannelTone", channelIndex, totalChannels, freq);
    public async Task PlayPannedToneAsync(double pan, int freq)
        => await (await ModuleAsync()).InvokeVoidAsync("playPannedTone", pan, freq);
    public async Task StartBalanceToneAsync(int freq)
        => await (await ModuleAsync()).InvokeVoidAsync("startBalanceTone", freq);
    public async Task SetPanAsync(double pan)
        => await (await ModuleAsync()).InvokeVoidAsync("setPan", pan);
    public async Task StartSpatialSweepAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("startSpatialSweep");
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
    public async Task<double> GetMicLevelDbAsync()
        => await (await ModuleAsync()).InvokeAsync<double>("getMicLevelDb");
    public async Task<int[]> GetMicWaveformAsync()
        => await (await ModuleAsync()).InvokeAsync<int[]>("getMicWaveform") ?? [];

    public async Task<double> MeasureEchoLatencyAsync()
        => await (await ModuleAsync()).InvokeAsync<double>("measureEchoLatency");
    public async Task<double> GetSystemLatencyAsync()
        => await (await ModuleAsync()).InvokeAsync<double>("getSystemLatency");
}

public sealed class MonitorJsInterop : ModuleInteropBase, IJsMonitorBridge
{
    public MonitorJsInterop(IJSRuntime js) : base(js, "./js/monitor-interop.js") { }

    public async Task EnterFullscreenAsync(string elementId)
        => await (await ModuleAsync()).InvokeVoidAsync("enterFullscreen", elementId);
    public async Task ExitFullscreenAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("exitFullscreen");
    public async Task WatchFullscreenExitAsync<T>(DotNetObjectReference<T> callbackRef, string methodName) where T : class
        => await (await ModuleAsync()).InvokeVoidAsync("watchFullscreenExit", callbackRef, methodName);
    public async Task<double[]> MeasureFrameDeltasAsync(int durationMs)
        => await (await ModuleAsync()).InvokeAsync<double[]>("measureFrameDeltas", durationMs) ?? [];
}

public sealed class ComputeJsInterop : ModuleInteropBase, IJsComputeBridge
{
    public ComputeJsInterop(IJSRuntime js) : base(js, "./js/compute-interop.js") { }

    public async Task<HardwareInfo> DetectHardwareAsync()
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("detectHardware");
        return new HardwareInfo
        {
            Threads = I(r, "threads"),
            DeviceMemoryGb = D(r, "deviceMemoryGb"),
            GpuVendor = S(r, "gpuVendor"),
            GpuRenderer = S(r, "gpuRenderer"),
            Webgl2 = B(r, "webgl2"),
            Webgpu = B(r, "webgpu"),
            JsHeapLimitMb = I(r, "jsHeapLimitMb"),
            UserAgent = S(r, "userAgent"),
        };
    }

    public async Task<CpuBenchmarkResult> RunCpuBenchmarkAsync<T>(int threads, int durationMs, DotNetObjectReference<T> progressCallback) where T : class
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("runCpuBenchmark", threads, durationMs, progressCallback);
        return new CpuBenchmarkResult
        {
            SingleUps = D(r, "singleUps"),
            MultiUps = D(r, "multiUps"),
            ThreadsUsed = I(r, "threads"),
            DurationMs = D(r, "durationMs"),
        };
    }

    public async Task<GpuBenchmarkResult> RunGpuBenchmarkAsync<T>(string canvasId, int width, int height, int complexity, int durationMs, DotNetObjectReference<T> progressCallback) where T : class
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("runGpuBenchmark", canvasId, width, height, complexity, durationMs, progressCallback);
        return new GpuBenchmarkResult
        {
            AvgFps = D(r, "avgFps"),
            Frames = I(r, "frames"),
            Width = I(r, "width"),
            Height = I(r, "height"),
            Complexity = I(r, "complexity"),
            Supported = B(r, "supported"),
        };
    }

    public async Task<MemoryBenchmarkResult> RunMemoryTestAsync<T>(int targetMb, DotNetObjectReference<T> progressCallback) where T : class
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("runMemoryTest", targetMb, progressCallback);
        return new MemoryBenchmarkResult
        {
            PeakMb = D(r, "peakMb"),
            Oom = B(r, "oom"),
            WriteGBs = D(r, "writeGBs"),
            ReadGBs = D(r, "readGBs"),
            HeapLimitMb = I(r, "heapLimitMb"),
        };
    }

    private static string S(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    private static double D(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
    private static int I(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;
    private static bool B(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.True;
}
