using BenchRig.App.Core.Models.Network;
using BenchRig.App.Services.State.Base;

namespace BenchRig.App.Services.State;

public enum SpeedPhase { Idle, Latency, Download, Upload, Done }

public sealed class NetworkDiagState : StateContainerBase
{
    public NetworkInfo? NetworkInfo { get; private set; }
    public SpeedTestResult? LatestSpeedTest { get; private set; }
    public StabilityResult? StabilityResult { get; private set; }
    public IReadOnlyList<LatencyResult> LatencyHistory => _latencyHistory.AsReadOnly();
    public IReadOnlyList<DiagnosticServer> Servers => _servers.AsReadOnly();
    public DiagnosticServer? SelectedServer { get; private set; }

    public SpeedPhase Phase { get; private set; } = SpeedPhase.Idle;
    public bool IsRunning { get; private set; }
    public double LiveMbps { get; private set; }          // current phase's live reading
    public double DownloadMbps { get; private set; }
    public double UploadMbps { get; private set; }
    public double PingMs { get; private set; }            // idle (min) latency
    public double AvgPingMs { get; private set; }
    public double MaxPingMs { get; private set; }
    public double JitterMs { get; private set; }
    public double Bufferbloat { get; private set; }       // added latency under download load (ms)
    public double LoadedPingMs => Bufferbloat > 0 && PingMs > 0 ? PingMs + Bufferbloat : 0;

    private readonly List<LatencyResult> _latencyHistory = new(64);
    private readonly List<DiagnosticServer> _servers = new(16);

    public void SetNetworkInfo(NetworkInfo info) { NetworkInfo = info; NotifyStateChanged(); }

    public void ApplyPreciseLocation(GeoPoint p)
    {
        if (NetworkInfo is null) return;
        NetworkInfo = NetworkInfo with
        {
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            City = string.IsNullOrWhiteSpace(p.City) ? NetworkInfo.City : p.City,
            Region = string.IsNullOrWhiteSpace(p.Region) ? NetworkInfo.Region : p.Region,
            Country = string.IsNullOrWhiteSpace(p.Country) ? NetworkInfo.Country : p.Country,
            IsPrecise = true,
        };
        NotifyStateChanged();
    }

    public void SetServers(IEnumerable<DiagnosticServer> servers)
    {
        _servers.Clear();
        _servers.AddRange(servers);
        SelectedServer ??= _servers.FirstOrDefault();
        NotifyStateChanged();
    }

    public void AddServer(DiagnosticServer server)
    {
        _servers.Add(server);
        NotifyStateChanged();
    }

    public void SelectServer(string key)
    {
        var match = _servers.FirstOrDefault(s => s.Key == key);
        if (match is not null) { SelectedServer = match; NotifyStateChanged(); }
    }

    public void BeginTest()
    {
        IsRunning = true;
        Phase = SpeedPhase.Latency;
        LiveMbps = DownloadMbps = UploadMbps = PingMs = AvgPingMs = MaxPingMs = JitterMs = Bufferbloat = 0;
        _latencyHistory.Clear();
        LatestSpeedTest = null;
        NotifyStateChanged();
    }

    public void SetPhase(SpeedPhase phase) { Phase = phase; LiveMbps = 0; NotifyStateChanged(); }
    public void SetLiveMbps(double mbps) { LiveMbps = mbps; NotifyThrottled(); }

    public void SetLatency(double minMs, double avgMs, double maxMs, double jitterMs)
    {
        PingMs = minMs;
        AvgPingMs = avgMs;
        MaxPingMs = maxMs;
        JitterMs = jitterMs;
        NotifyStateChanged();
    }

    public void SetBufferbloat(double ms) { Bufferbloat = ms; NotifyStateChanged(); }

    public void AppendLatency(LatencyResult r) { _latencyHistory.Add(r); NotifyThrottled(); }

    public void SetDownload(double mbps) { DownloadMbps = mbps; NotifyStateChanged(); }
    public void SetUpload(double mbps) { UploadMbps = mbps; NotifyStateChanged(); }

    public void CompleteTest(SpeedTestResult result, StabilityResult stability)
    {
        LatestSpeedTest = result;
        StabilityResult = stability;
        DownloadMbps = result.DownloadMbps;
        UploadMbps = result.UploadMbps;
        PingMs = result.PingMs;
        JitterMs = result.JitterMs;
        Phase = SpeedPhase.Done;
        IsRunning = false;
        NotifyStateChanged();
    }

    public void Reset()
    {
        Phase = SpeedPhase.Idle;
        IsRunning = false;
        LiveMbps = DownloadMbps = UploadMbps = PingMs = AvgPingMs = MaxPingMs = JitterMs = Bufferbloat = 0;
        _latencyHistory.Clear();
        LatestSpeedTest = null;
        StabilityResult = null;
        NotifyStateChanged();
    }
}
