namespace BenchRig.App.Core.Models.Network;

/// <summary>Result of a download/upload speed test.</summary>
public record SpeedTestResult
{
    public double DownloadMbps { get; init; }
    public double UploadMbps { get; init; }
    public double PingMs { get; init; }
    public double JitterMs { get; init; }
    public string ServerLabel { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>A single latency sample with running jitter.</summary>
public record LatencyResult
{
    public double PingMs { get; init; }
    public double JitterMs { get; init; }
    public int PacketIndex { get; init; }
}

/// <summary>Connection stability over a window.</summary>
public record StabilityResult
{
    public double PacketLossPercent { get; init; }
    public int DroppedFrames { get; init; }
    public int TotalSamples { get; init; }
}

/// <summary>Aggregate of a concurrent-stream stress test.</summary>
public record StressTestResult
{
    public int ConcurrentStreams { get; init; }
    public long TotalBytes { get; init; }
    public double DurationMs { get; init; }
    public double ThroughputMbps { get; init; }
}

/// <summary>Public IP / ISP / geo information (ipinfo.io style).</summary>
public record NetworkInfo
{
    public string PublicIp { get; init; } = "";
    public string Hostname { get; init; } = "";
    public string Isp { get; init; } = "";
    public string Org { get; init; } = "";
    public string City { get; init; } = "";
    public string Region { get; init; } = "";
    public string Country { get; init; } = "";
    public string Postal { get; init; } = "";
    public string Timezone { get; init; } = "";
    public string UtcOffset { get; init; } = "";
    public string LocalTime { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int AccuracyKm { get; init; }
    public string Colo { get; init; } = "";   // Cloudflare edge datacenter code
    public long Asn { get; init; }
    public string CountryCode { get; init; } = "";
    public string Continent { get; init; } = "";

    // Connection
    public string HttpProtocol { get; init; } = "";
    public string ConnType { get; init; } = "";        // effectiveType: 4g/3g/...
    public double Downlink { get; init; }              // Mbps estimate (NetworkInformation API)
    public double Rtt { get; init; }                   // ms estimate
    public bool SaveData { get; init; }
    public bool DohCloudflare { get; init; }           // DNS-over-HTTPS reachable

    // Device
    public string Browser { get; init; } = "";
    public string Os { get; init; } = "";
    public string Languages { get; init; } = "";
    public int Cores { get; init; }
    public double Memory { get; init; }
    public int TouchPoints { get; init; }
    public bool Online { get; init; }
    public bool CookiesEnabled { get; init; }
    public string Screen { get; init; } = "";

    public bool IsPrecise { get; init; }               // location refined via Geolocation API
    public bool HasLocation => Latitude != 0 || Longitude != 0;
}

/// <summary>Precise device location (Geolocation API + reverse geocode).</summary>
public record GeoPoint
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string City { get; init; } = "";
    public string Region { get; init; } = "";
    public string Country { get; init; } = "";
}

/// <summary>A selectable speed-test server (Cloudflare edge or a LibreSpeed instance).</summary>
public record DiagnosticServer
{
    public string Key { get; init; } = "";       // id passed to the JS layer
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Location { get; init; } = "";
    public string Server { get; init; } = "";
    public string Sponsor { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? DistanceKm { get; set; }
    public double? PingMs { get; set; }

    public string Label => string.IsNullOrWhiteSpace(Location) ? Name : $"{Name} — {Location}";
}
