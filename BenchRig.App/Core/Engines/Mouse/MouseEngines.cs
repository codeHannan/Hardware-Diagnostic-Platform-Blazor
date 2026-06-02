using BenchRig.App.Core.Models.Mouse;

namespace BenchRig.App.Core.Engines.Mouse;

/// <summary>Clicks per second over a time window.</summary>
public sealed class CpsCalculator
{
    public double Calculate(int clicks, double durationSec)
        => durationSec <= 0 ? 0 : clicks / durationSec;
}

/// <summary>Estimates polling rate (Hz) from pointer event timestamp deltas.</summary>
public sealed class PollingRateAnalyzer
{
    public PollingRateResult Analyze(IReadOnlyList<double> timeStampsMs)
    {
        if (timeStampsMs.Count < 2)
            return new PollingRateResult { Hz = 0, SampleCount = timeStampsMs.Count };

        var deltas = new List<double>(timeStampsMs.Count - 1);
        for (int i = 1; i < timeStampsMs.Count; i++)
        {
            double d = timeStampsMs[i] - timeStampsMs[i - 1];
            if (d > 0) deltas.Add(d);
        }
        if (deltas.Count == 0)
            return new PollingRateResult { Hz = 0, SampleCount = timeStampsMs.Count };

        double avgDelta = deltas.Average();
        double hz = avgDelta <= 0 ? 0 : 1000.0 / avgDelta;

        double meanHz = hz;
        double devSum = deltas.Sum(d =>
        {
            double instHz = 1000.0 / d;
            return (instHz - meanHz) * (instHz - meanHz);
        });
        double deviation = Math.Sqrt(devSum / deltas.Count);

        return new PollingRateResult
        {
            Hz = hz,
            SampleCount = timeStampsMs.Count,
            DeviationHz = deviation,
        };
    }
}

/// <summary>Estimates DPI from pixel travel over a known physical distance.</summary>
public sealed class DpiEstimator
{
    public DpiEstimate Estimate(double pixelsTravelled, double physicalInches)
        => new()
        {
            DistancePx = pixelsTravelled,
            EstimatedDpi = physicalInches <= 0 ? 0 : pixelsTravelled / physicalInches,
        };
}

/// <summary>Std-dev of movement deltas — quantifies hand/sensor jitter.</summary>
public sealed class JitterAnalyzer
{
    public double Analyze(IReadOnlyList<PointerSample> samples)
    {
        if (samples.Count < 3) return 0;
        var deltas = new List<double>(samples.Count - 1);
        for (int i = 1; i < samples.Count; i++)
        {
            double dx = samples[i].X - samples[i - 1].X;
            double dy = samples[i].Y - samples[i - 1].Y;
            deltas.Add(Math.Sqrt(dx * dx + dy * dy));
        }
        double mean = deltas.Average();
        double variance = deltas.Sum(d => (d - mean) * (d - mean)) / deltas.Count;
        return Math.Sqrt(variance);
    }
}
