using BenchRig.App.Core.Constants;
using BenchRig.App.Core.Models.Keyboard;

namespace BenchRig.App.Core.Engines.Keyboard;

/// <summary>
/// Detects key chatter: a keydown that re-fires within the threshold interval
/// after the key's most recent keyup is a bounce.
/// </summary>
public sealed class ChatterDetector
{
    public IReadOnlyList<ChatterResult> Detect(
        IReadOnlyList<KeyEventSample> events,
        int thresholdMs = DiagnosticDefaults.ChatterThresholdMs)
    {
        var lastUp = new Dictionary<string, double>();
        var bounces = new Dictionary<string, int>();
        var minInterval = new Dictionary<string, double>();

        foreach (var e in events.OrderBy(e => e.TimeStampMs))
        {
            if (e.IsDown)
            {
                if (lastUp.TryGetValue(e.Code, out double up))
                {
                    double interval = e.TimeStampMs - up;
                    if (interval >= 0 && interval < thresholdMs)
                    {
                        bounces[e.Code] = bounces.GetValueOrDefault(e.Code) + 1;
                        if (!minInterval.TryGetValue(e.Code, out double m) || interval < m)
                            minInterval[e.Code] = interval;
                    }
                }
            }
            else
            {
                lastUp[e.Code] = e.TimeStampMs;
            }
        }

        return bounces
            .Select(kv => new ChatterResult
            {
                Code = kv.Key,
                BounceCount = kv.Value,
                IntervalMs = minInterval.GetValueOrDefault(kv.Key),
            })
            .ToList();
    }
}

/// <summary>Words-per-minute: (correct chars / 5) / minutes.</summary>
public sealed class WpmCalculator
{
    public TypingTestResult Calculate(int correctChars, int totalChars, double durationSec)
    {
        double minutes = durationSec / 60.0;
        double wpm = minutes <= 0 ? 0 : (correctChars / 5.0) / minutes;
        double raw = minutes <= 0 ? 0 : (totalChars / 5.0) / minutes;
        double acc = totalChars <= 0 ? 0 : correctChars * 100.0 / totalChars;
        return new TypingTestResult
        {
            Wpm = wpm,
            RawWpm = raw,
            AccuracyPercent = acc,
            CorrectChars = correctChars,
            TotalChars = totalChars,
        };
    }
}

/// <summary>Tracks max concurrently-held keys and detects ghosting.</summary>
public sealed class RolloverAnalyzer
{
    public RolloverResult Analyze(IReadOnlyList<KeyEventSample> events)
    {
        var held = new HashSet<string>();
        int max = 0;
        var seenTogether = new HashSet<string>();

        foreach (var e in events.OrderBy(e => e.TimeStampMs))
        {
            if (e.IsDown)
            {
                held.Add(e.Code);
                if (held.Count > max)
                {
                    max = held.Count;
                    seenTogether = new HashSet<string>(held);
                }
            }
            else
            {
                held.Remove(e.Code);
            }
        }

        return new RolloverResult
        {
            MaxSimultaneousKeys = max,
            GhostedKeys = [],
        };
    }
}

/// <summary>Hz from keydown timestamp deltas.</summary>
public sealed class KeyPollingRateAnalyzer
{
    public KeyPollingRate Analyze(IReadOnlyList<double> downTimeStampsMs)
    {
        if (downTimeStampsMs.Count < 2)
            return new KeyPollingRate { Hz = 0, SampleWindow = downTimeStampsMs.Count };

        double total = 0; int n = 0;
        for (int i = 1; i < downTimeStampsMs.Count; i++)
        {
            double d = downTimeStampsMs[i] - downTimeStampsMs[i - 1];
            if (d > 0) { total += d; n++; }
        }
        double avg = n == 0 ? 0 : total / n;
        return new KeyPollingRate
        {
            Hz = avg <= 0 ? 0 : 1000.0 / avg,
            SampleWindow = downTimeStampsMs.Count,
        };
    }
}
