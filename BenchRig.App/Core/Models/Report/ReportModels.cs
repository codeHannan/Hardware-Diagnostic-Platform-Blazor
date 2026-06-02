using BenchRig.App.Core.Models.Network;
using BenchRig.App.Core.Models.Mouse;
using BenchRig.App.Core.Models.Keyboard;
using BenchRig.App.Core.Models.Audio;
using BenchRig.App.Core.Models.Monitor;
using BenchRig.App.Core.Models.Compute;

namespace BenchRig.App.Core.Models.Report;

public enum ReportExportFormat { Json, Pdf, Markdown }

public enum ReportSectionKind { Network, Mouse, Keyboard, Audio, Monitor, Compute }

/// <summary>One section of metrics within a report.</summary>
public record ReportSection
{
    public ReportSectionKind Kind { get; init; }
    public string Title { get; init; } = "";
    public IReadOnlyDictionary<string, string> Metrics { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>Master diagnostic report aggregating all subsystem results.</summary>
public record DiagnosticReport
{
    public string? OwnerUid { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public string Summary { get; init; } = "";

    public SpeedTestResult? Speed { get; init; }
    public StabilityResult? Stability { get; init; }
    public ClickTestResult? Click { get; init; }
    public PollingRateResult? MousePolling { get; init; }
    public TypingTestResult? Typing { get; init; }
    public RefreshRateResult? RefreshRate { get; init; }
    public BenchmarkSuite? Benchmarks { get; init; }

    public IReadOnlyList<ReportSection> Sections { get; init; } = [];
}
