namespace BenchRig.App.Core.Models.Mouse;

/// <summary>Mouse buttons tracked by the diagnostics.</summary>
public enum MouseButton { Left, Right, Middle, Back, Forward }

public record ClickTestResult
{
    public double Cps { get; init; }
    public int TotalClicks { get; init; }
    public double DurationSec { get; init; }
}

public record JitterClickResult
{
    public double JitterCps { get; init; }
    public double IrregularityScore { get; init; }
    public int TotalClicks { get; init; }
}

public record DoubleClickResult
{
    public double IntervalMs { get; init; }
    public bool IsAnomaly { get; init; }
}

public record ScrollTestResult
{
    public int StepsUp { get; init; }
    public int StepsDown { get; init; }
    public double ConsistencyPercent { get; init; }
}

public record ButtonState
{
    public MouseButton Button { get; init; }
    public bool IsPressed { get; init; }
}

public record PollingRateResult
{
    public double Hz { get; init; }
    public double MaxHz { get; init; }
    public int SampleCount { get; init; }
    public double DeviationHz { get; init; }
}

public record DpiEstimate
{
    public double EstimatedDpi { get; init; }
    public double DistancePx { get; init; }
}

public record DeadZoneResult
{
    public double DeadZoneRadiusPx { get; init; }
    public double JitterPx { get; init; }
}

public record AccuracyResult
{
    public double HitPercent { get; init; }
    public double MissPercent { get; init; }
    public double AvgDeviationPx { get; init; }
}

public record DragDropResult
{
    public bool DragIntegrity { get; init; }
    public double DropOffsetPx { get; init; }
}

/// <summary>Raw pointer sample marshaled from JS.</summary>
public record PointerSample
{
    public double X { get; init; }
    public double Y { get; init; }
    public double TimeStampMs { get; init; }
    public int Buttons { get; init; }
}
