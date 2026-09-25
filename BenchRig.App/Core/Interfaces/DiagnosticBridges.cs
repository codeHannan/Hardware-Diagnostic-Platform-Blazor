using Microsoft.JSInterop;
using BenchRig.App.Core.Models.Network;
using BenchRig.App.Core.Models.Compute;

namespace BenchRig.App.Core.Interfaces;

public interface IJsNetworkBridge : IAsyncDisposable
{
    /// <summary>Rich connection info (IP, hostname, ISP, ASN, geo, postal, timezone) via ipinfo.io.</summary>
    Task<NetworkInfo> GetNetworkInfoAsync();

    /// <summary>Refines location using the browser Geolocation API + reverse geocoding.</summary>
    Task<GeoPoint?> GetPreciseLocationAsync();

    /// <summary>Selectable servers (Cloudflare + any declared in network-servers.json).</summary>
    Task<IReadOnlyList<DiagnosticServer>> GetServersAsync();

    /// <summary>Registers a user-provided server. <paramref name="kind"/> is "ls" or "worker".</summary>
    Task AddCustomServerAsync(string key, string name, string baseUrl, string kind);

    /// <summary>Runs <paramref name="count"/> tiny round-trips to the server and returns each RTT in ms.</summary>
    Task<IReadOnlyList<double>> MeasureLatencyAsync(string serverId, int count);

    /// <summary>Streams downloads for the duration, reporting live Mbps to the callback. Returns steady-state Mbps + bufferbloat (added latency under load, ms).</summary>
    Task<(double Mbps, double BufferbloatMs)> MeasureDownloadAsync<T>(string serverId, int durationMs, DotNetObjectReference<T> progress, string methodName) where T : class;

    /// <summary>POSTs payloads for the duration, reporting live Mbps to the callback. Returns the average Mbps.</summary>
    Task<double> MeasureUploadAsync<T>(string serverId, int durationMs, DotNetObjectReference<T> progress, string methodName) where T : class;
}

public interface IJsMouseBridge : IAsyncDisposable
{
    Task StartPointerCaptureAsync<T>(string elementId, DotNetObjectReference<T> callbackRef) where T : class;
    Task StopPointerCaptureAsync(string elementId);
    Task StartPollingRateCaptureAsync<T>(string elementId, DotNetObjectReference<T> callbackRef) where T : class;
    Task StopPollingRateCaptureAsync(string elementId);
}

public interface IJsKeyboardBridge : IAsyncDisposable
{
    Task StartKeyCaptureAsync<T>(DotNetObjectReference<T> callbackRef) where T : class;
    Task StopKeyCaptureAsync();
}

public interface IJsAudioBridge : IAsyncDisposable
{
    /// <summary>Max output channels the audio device exposes (2 = stereo, 6 = 5.1, 8 = 7.1).</summary>
    Task<int> GetMaxChannelsAsync();
    /// <summary>Routes a tone to one discrete output channel of an N-channel layout.</summary>
    Task PlayChannelToneAsync(int channelIndex, int totalChannels, int freq);
    /// <summary>Plays a stereo-panned tone (-1 left .. +1 right).</summary>
    Task PlayPannedToneAsync(double pan, int freq);
    /// <summary>Starts a continuous tone whose pan can be adjusted live via <see cref="SetPanAsync"/>.</summary>
    Task StartBalanceToneAsync(int freq);
    /// <summary>Updates the live balance-tone pan (-1 left .. +1 right).</summary>
    Task SetPanAsync(double pan);
    /// <summary>Plays a tone that orbits the listener (HRTF immersive sweep).</summary>
    Task StartSpatialSweepAsync();
    Task StopPlaybackAsync();

    Task<bool> StartMicCaptureAsync();
    Task StopMicCaptureAsync();
    Task StartLoopbackAsync();
    Task StopLoopbackAsync();
    Task<double> GetMicLevelDbAsync();
    Task<int[]> GetMicWaveformAsync();

    Task<double> MeasureEchoLatencyAsync();
    Task<double> GetSystemLatencyAsync();
}

public interface IJsMonitorBridge : IAsyncDisposable
{
    Task EnterFullscreenAsync(string elementId);
    Task ExitFullscreenAsync();
    /// <summary>Registers a callback fired when the user leaves fullscreen (Esc).</summary>
    Task WatchFullscreenExitAsync<T>(DotNetObjectReference<T> callbackRef, string methodName) where T : class;
    /// <summary>Per-frame deltas (ms) over the window — used for refresh rate, jitter and dropped frames.</summary>
    Task<double[]> MeasureFrameDeltasAsync(int durationMs);
}

public interface IJsComputeBridge : IAsyncDisposable
{
    Task<HardwareInfo> DetectHardwareAsync();
    /// <summary>Runs single-core then multi-core passes; returns raw work-units/sec (scores computed in the engine).</summary>
    Task<CpuBenchmarkResult> RunCpuBenchmarkAsync<T>(int threads, int durationMs, DotNetObjectReference<T> progressCallback) where T : class;
    Task<GpuBenchmarkResult> RunGpuBenchmarkAsync<T>(string canvasId, int width, int height, int complexity, int durationMs, DotNetObjectReference<T> progressCallback) where T : class;
    Task<MemoryBenchmarkResult> RunMemoryTestAsync<T>(int targetMb, DotNetObjectReference<T> progressCallback) where T : class;
}
