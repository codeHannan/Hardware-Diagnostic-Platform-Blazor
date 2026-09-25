namespace BenchRig.App.Core.Models.Audio;

public enum PlaybackChannel { Stereo, FiveOne, SevenOne, Spatial }

public record BalanceResult
{
    public string Channel { get; init; } = "";
    public double FrequencyHz { get; init; }
    public double VolumeDb { get; init; }
}

public record MicLoopbackState
{
    public bool IsCapturing { get; init; }
    public double LatencyMs { get; init; }
    public double LevelDb { get; init; }
}

public record EchoLatencyResult
{
    public double RoundTripMs { get; init; }
    public double Confidence { get; init; }
}
