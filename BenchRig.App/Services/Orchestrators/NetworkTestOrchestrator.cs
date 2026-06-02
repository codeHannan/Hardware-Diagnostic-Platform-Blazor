using Microsoft.JSInterop;
using BenchRig.App.Core.Engines.Network;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Network;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Orchestrators;

/// <summary>
/// Drives the full speedtest.net-style flow against Cloudflare's edge:
/// latency/jitter -> download -> upload, streaming live readings into state.
/// </summary>
public sealed class NetworkTestOrchestrator : IAsyncDisposable
{
    private const int LatencySamples = 20;
    private const int DownloadMs = 10_000;
    private const int UploadMs = 8_000;

    private readonly IJsNetworkBridge _bridge;
    private readonly NetworkDiagState _state;
    private readonly JitterCalculator _jitter = new();
    private readonly PacketLossCalculator _loss = new();
    private DotNetObjectReference<NetworkTestOrchestrator>? _selfRef;

    public NetworkTestOrchestrator(IJsNetworkBridge bridge, NetworkDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    private DotNetObjectReference<NetworkTestOrchestrator> SelfRef
        => _selfRef ??= DotNetObjectReference.Create(this);

    public async Task LoadNetworkInfoAsync()
    {
        try { _state.SetNetworkInfo(await _bridge.GetNetworkInfoAsync()); }
        catch { /* offline - leave info null */ }
    }

    public async Task LoadServersAsync()
    {
        try { _state.SetServers(await _bridge.GetServersAsync()); }
        catch { /* offline - leave empty */ }
    }

    public async Task<bool> UsePreciseLocationAsync()
    {
        try
        {
            var point = await _bridge.GetPreciseLocationAsync();
            if (point is null) return false;
            _state.ApplyPreciseLocation(point);
            return true;
        }
        catch { return false; }
    }

    public async Task AddCustomServerAsync(string name, string baseUrl, string kind = "ls")
    {
        var key = "custom-" + Guid.NewGuid().ToString("N")[..6];
        await _bridge.AddCustomServerAsync(key, name, baseUrl, kind);
        _state.AddServer(new DiagnosticServer { Key = key, Id = 100, Name = name, Location = "Custom", Server = baseUrl });
        _state.SelectServer(key);
    }

    public async Task RunFullTestAsync()
    {
        if (_state.IsRunning) return;
        string serverId = _state.SelectedServer?.Key ?? "cf";
        _state.BeginTest();

        try
        {
            // ── 1. Latency & jitter ──
            _state.SetPhase(SpeedPhase.Latency);
            var pings = (await _bridge.MeasureLatencyAsync(serverId, LatencySamples)).ToList();
            for (int i = 0; i < pings.Count; i++)
                _state.AppendLatency(new LatencyResult
                {
                    PingMs = pings[i],
                    JitterMs = _jitter.CalculateRunning(pings.Take(i + 1).ToList()),
                    PacketIndex = i,
                });

            double ping = Median(pings);
            double jitter = _jitter.CalculateRunning(pings);
            _state.SetLatency(ping, jitter);

            // ── 2. Download ──
            _state.SetPhase(SpeedPhase.Download);
            double down = await _bridge.MeasureDownloadAsync(serverId, DownloadMs, SelfRef, nameof(OnDownloadProgress));
            _state.SetDownload(down);

            // ── 3. Upload ──
            _state.SetPhase(SpeedPhase.Upload);
            double up = await _bridge.MeasureUploadAsync(serverId, UploadMs, SelfRef, nameof(OnUploadProgress));
            _state.SetUpload(up);

            // ── Complete ──
            string serverLabel = _state.SelectedServer?.Label ?? "Cloudflare";

            _state.CompleteTest(
                new SpeedTestResult
                {
                    DownloadMbps = down,
                    UploadMbps = up,
                    PingMs = ping,
                    JitterMs = jitter,
                    ServerLabel = serverLabel,
                    Timestamp = DateTime.UtcNow,
                },
                new StabilityResult
                {
                    PacketLossPercent = _loss.Calculate(LatencySamples, pings.Count),
                    TotalSamples = LatencySamples,
                });
        }
        catch
        {
            _state.Reset();
        }
    }

    [JSInvokable] public void OnDownloadProgress(double mbps) => _state.SetLiveMbps(mbps);
    [JSInvokable] public void OnUploadProgress(double mbps) => _state.SetLiveMbps(mbps);

    private static double Median(List<double> values)
    {
        if (values.Count == 0) return 0;
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
    }

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _bridge.DisposeAsync();
    }
}
