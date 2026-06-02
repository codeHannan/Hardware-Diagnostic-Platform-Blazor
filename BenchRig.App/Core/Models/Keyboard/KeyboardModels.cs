namespace BenchRig.App.Core.Models.Keyboard;

public record KeyRegistration
{
    public string Code { get; init; } = "";
    public string Key { get; init; } = "";
    public double PressedTimeStampMs { get; init; }
    public bool IsPressed { get; init; }
}

public record ChatterResult
{
    public string Code { get; init; } = "";
    public int BounceCount { get; init; }
    public double IntervalMs { get; init; }
}

public record KeyPollingRate
{
    public double Hz { get; init; }
    public int SampleWindow { get; init; }
}

public record TypingTestResult
{
    public double Wpm { get; init; }
    public double RawWpm { get; init; }
    public double AccuracyPercent { get; init; }
    public int CorrectChars { get; init; }
    public int TotalChars { get; init; }
}

public record RolloverResult
{
    public int MaxSimultaneousKeys { get; init; }
    public IReadOnlyList<string> GhostedKeys { get; init; } = [];
}

public record StuckKeyEvent
{
    public string Code { get; init; } = "";
    public double StuckDurationMs { get; init; }
}

/// <summary>Raw key event marshaled from JS.</summary>
public record KeyEventSample
{
    public string Code { get; init; } = "";
    public string Key { get; init; } = "";
    public bool IsDown { get; init; }
    public double TimeStampMs { get; init; }
}
