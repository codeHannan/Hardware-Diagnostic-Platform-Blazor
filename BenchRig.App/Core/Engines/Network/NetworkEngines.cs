namespace BenchRig.App.Core.Engines.Network;

/// <summary>Converts transferred bytes + elapsed time into Mbps.</summary>
public sealed class SpeedCalculator
{
    public double CalculateMbps(long bytes, double elapsedMs)
    {
        if (elapsedMs <= 0) return 0;
        double bits = bytes * 8.0;
        double seconds = elapsedMs / 1000.0;
        return bits / seconds / 1_000_000.0;
    }
}

/// <summary>Computes statistical jitter from a ping series.</summary>
public sealed class JitterCalculator
{
    /// <summary>Mean absolute consecutive difference (RFC 3550-style running jitter).</summary>
    public double CalculateRunning(IReadOnlyList<double> pings)
    {
        if (pings.Count < 2) return 0;
        double sum = 0;
        for (int i = 1; i < pings.Count; i++)
            sum += Math.Abs(pings[i] - pings[i - 1]);
        return sum / (pings.Count - 1);
    }

    /// <summary>Standard deviation of the ping series.</summary>
    public double StandardDeviation(IReadOnlyList<double> pings)
    {
        if (pings.Count < 2) return 0;
        double mean = pings.Average();
        double variance = pings.Sum(p => (p - mean) * (p - mean)) / pings.Count;
        return Math.Sqrt(variance);
    }
}

/// <summary>Packet loss percentage from sent vs. received counts.</summary>
public sealed class PacketLossCalculator
{
    public double Calculate(int sent, int received)
    {
        if (sent <= 0) return 0;
        int lost = Math.Max(0, sent - received);
        return lost * 100.0 / sent;
    }
}

/// <summary>Aggregates concurrent-stream throughput.</summary>
public sealed class StressAnalyzer
{
    public double AggregateMbps(long totalBytes, double durationMs)
    {
        if (durationMs <= 0) return 0;
        return totalBytes * 8.0 / (durationMs / 1000.0) / 1_000_000.0;
    }
}
