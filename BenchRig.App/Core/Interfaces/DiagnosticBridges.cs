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

    /// <summary>Streams downloads for the duration, reporting live Mbps to the callback. Returns the average Mbps.</summary>
    Task<double> MeasureDownloadAsync<T>(string serverId, int durationMs, DotNetObjectReference<T> progress, string methodName) where T : class;

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
    Task PlayTestToneAsync(string channelAudioPath, string outputChannelId);
    Task StopPlaybackAsync();
    Task<bool> StartMicCaptureAsync();
    Task StopMicCaptureAsync();
    Task StartLoopbackAsync();
    Task StopLoopbackAsync();
    Task<double> MeasureEchoLatencyAsync();
    Task<double> GetMicLevelDbAsync();
}

public interface IJsMonitorBridge : IAsyncDisposable
{
    Task EnterFullscreenAsync(string elementId);
    Task ExitFullscreenAsync();
    Task<double> MeasureFrameRateAsync(int sampleFrames);
    Task StartGhostingAnimationAsync(string canvasId, double speedPxPerFrame);
    Task StopGhostingAnimationAsync(string canvasId);
}

public interface IJsComputeBridge : IAsyncDisposable
{
    Task<CpuBenchmarkResult> RunCpuBenchmarkAsync<T>(int threadCount, int durationSec, DotNetObjectReference<T> progressCallback) where T : class;
    Task<GpuBenchmarkResult> RunGpuBenchmarkAsync<T>(string canvasId, int durationSec, DotNetObjectReference<T> progressCallback) where T : class;
}
