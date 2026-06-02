using System.Globalization;
using BenchRig.App.Core.Models.Report;
using BenchRig.App.Core.Models.Network;
using BenchRig.App.Core.Models.Mouse;
using BenchRig.App.Core.Models.Keyboard;
using BenchRig.App.Core.Models.Monitor;
using BenchRig.App.Core.Models.Compute;

namespace BenchRig.App.Core.Engines.Report;

/// <summary>Compiles individual diagnostic results into a master <see cref="DiagnosticReport"/>.</summary>
public sealed class ReportBuilder
{
    public DiagnosticReport Build(
        string? ownerUid = null,
        SpeedTestResult? speed = null,
        StabilityResult? stability = null,
        ClickTestResult? click = null,
        PollingRateResult? mousePolling = null,
        TypingTestResult? typing = null,
        RefreshRateResult? refreshRate = null,
        BenchmarkSuite? benchmarks = null)
    {
        var sections = new List<ReportSection>();

        if (speed is not null || stability is not null)
        {
            var m = new Dictionary<string, string>();
            if (speed is not null)
            {
                m["Download"] = $"{speed.DownloadMbps:F1} Mbps";
                m["Upload"] = $"{speed.UploadMbps:F1} Mbps";
                m["Server"] = speed.ServerLabel;
            }
            if (stability is not null)
                m["Packet Loss"] = $"{stability.PacketLossPercent:F1}%";
            sections.Add(new ReportSection { Kind = ReportSectionKind.Network, Title = "Network", Metrics = m });
        }

        if (click is not null || mousePolling is not null)
        {
            var m = new Dictionary<string, string>();
            if (click is not null) m["CPS"] = click.Cps.ToString("F1", CultureInfo.InvariantCulture);
            if (mousePolling is not null) m["Polling Rate"] = $"{mousePolling.Hz:F0} Hz";
            sections.Add(new ReportSection { Kind = ReportSectionKind.Mouse, Title = "Mouse", Metrics = m });
        }

        if (typing is not null)
            sections.Add(new ReportSection
            {
                Kind = ReportSectionKind.Keyboard,
                Title = "Keyboard",
                Metrics = new Dictionary<string, string>
                {
                    ["WPM"] = typing.Wpm.ToString("F0", CultureInfo.InvariantCulture),
                    ["Accuracy"] = $"{typing.AccuracyPercent:F1}%",
                },
            });

        if (refreshRate is not null)
            sections.Add(new ReportSection
            {
                Kind = ReportSectionKind.Monitor,
                Title = "Monitor",
                Metrics = new Dictionary<string, string>
                {
                    ["Refresh Rate"] = $"{refreshRate.MeasuredHz:F1} Hz",
                },
            });

        if (benchmarks is not null)
            sections.Add(new ReportSection
            {
                Kind = ReportSectionKind.Compute,
                Title = "Compute",
                Metrics = new Dictionary<string, string>
                {
                    ["CPU Score"] = (benchmarks.Cpu?.Score ?? 0).ToString("F0", CultureInfo.InvariantCulture),
                    ["GPU Score"] = (benchmarks.Gpu?.Score ?? 0).ToString("F0", CultureInfo.InvariantCulture),
                },
            });

        return new DiagnosticReport
        {
            OwnerUid = ownerUid,
            Speed = speed,
            Stability = stability,
            Click = click,
            MousePolling = mousePolling,
            Typing = typing,
            RefreshRate = refreshRate,
            Benchmarks = benchmarks,
            Sections = sections,
            Summary = BuildSummary(sections),
        };
    }

    private static string BuildSummary(IReadOnlyList<ReportSection> sections)
        => sections.Count == 0
            ? "No diagnostics have been run yet."
            : $"Compiled {sections.Count} diagnostic section(s): " +
              string.Join(", ", sections.Select(s => s.Title)) + ".";

    public string ToMarkdown(DiagnosticReport report)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# BenchRig Diagnostic Report");
        sb.AppendLine();
        sb.AppendLine($"_Generated: {report.GeneratedAt:u}_");
        sb.AppendLine();
        sb.AppendLine(report.Summary);
        sb.AppendLine();
        foreach (var section in report.Sections)
        {
            sb.AppendLine($"## {section.Title}");
            sb.AppendLine();
            sb.AppendLine("| Metric | Value |");
            sb.AppendLine("|---|---|");
            foreach (var (k, v) in section.Metrics)
                sb.AppendLine($"| {k} | {v} |");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
