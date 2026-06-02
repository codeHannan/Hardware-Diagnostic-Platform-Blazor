namespace BenchRig.App.Core.Models.Monitor;

public enum GradientType { Horizontal, Vertical, Radial, Grayscale }

public record DeadPixelConfig
{
    public string ColorHex { get; init; } = "#000000";
    public bool Fullscreen { get; init; } = true;
}

public record ColorGradient
{
    public GradientType Type { get; init; } = GradientType.Horizontal;
    public IReadOnlyList<string> Stops { get; init; } = [];
}

public record GhostingConfig
{
    public double ObjectSpeedPxPerFrame { get; init; } = 8;
    public double ContrastLevel { get; init; } = 1.0;
}

public record RefreshRateResult
{
    public double MeasuredHz { get; init; }
    public double ExpectedHz { get; init; }
    public double DeltaHz { get; init; }
}

public record GammaTestConfig
{
    public double TargetGamma { get; init; } = 2.2;
    public string PatternType { get; init; } = "checker";
}

public record ResponseTimeResult
{
    public double GreyToGreyMs { get; init; }
    public double OvershootPercent { get; init; }
}
