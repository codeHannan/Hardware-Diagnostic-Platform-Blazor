namespace BenchRig.App.Core.Constants;

/// <summary>Default thresholds, durations, and sample sizes for diagnostics.</summary>
public static class DiagnosticDefaults
{
    // -- Network --
    public const int SpeedTestChunkKb = 2048;
    public const int SpeedTestChunkCount = 10;
    public const int UploadPayloadBytes = 2 * 1024 * 1024;
    public const int LatencyIterations = 50;
    public const int StressConcurrentStreams = 8;
    public const int StressDurationSec = 10;

    // -- Mouse --
    public const int CpsTestDurationSec = 5;
    public const double JitterThresholdPx = 2.0;
    public const int DoubleClickAnomalyMs = 80;

    // -- Keyboard --
    public const int TypingTestDurationSec = 60;
    public const int ChatterThresholdMs = 50;

    // -- Audio --
    public const double EchoConfidenceThreshold = 0.6;

    // -- Monitor --
    public const int RefreshRateSampleFrames = 240;

    // -- Compute --
    public const int CpuBenchDurationSec = 10;
    public const int GpuBenchDurationSec = 10;
    public const int DefaultCpuThreads = 4;

    // -- Validation limits (mirror Firestore rules) --
    public const int MaxPostTitle = 200;
    public const int MaxPostBody = 10_000;
    public const int MaxCommentBody = 2_000;
    public const int MaxTicketSubject = 200;
    public const int MaxTicketBody = 5_000;
    public const int MaxArticleTitle = 200;
    public const int MaxArticleBody = 50_000;
    public const int MaxCommentDepth = 3;
}
