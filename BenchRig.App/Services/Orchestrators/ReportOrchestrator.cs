using BenchRig.App.Core.Engines.Report;
using BenchRig.App.Core.Models.Compute;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Orchestrators;

/// <summary>Gathers results from every diagnostic state container into a single report.</summary>
public sealed class ReportOrchestrator
{
    private readonly NetworkDiagState _network;
    private readonly MouseDiagState _mouse;
    private readonly KeyboardDiagState _keyboard;
    private readonly MonitorDiagState _monitor;
    private readonly ComputeDiagState _compute;
    private readonly ReportState _report;
    private readonly AuthStateContainer _auth;
    private readonly ReportBuilder _builder = new();

    public ReportOrchestrator(
        NetworkDiagState network, MouseDiagState mouse, KeyboardDiagState keyboard,
        MonitorDiagState monitor, ComputeDiagState compute, ReportState report, AuthStateContainer auth)
    {
        _network = network;
        _mouse = mouse;
        _keyboard = keyboard;
        _monitor = monitor;
        _compute = compute;
        _report = report;
        _auth = auth;
    }

    public void Compile()
    {
        BenchmarkSuite? suite = (_compute.Cpu is not null || _compute.Gpu is not null)
            ? new BenchmarkSuite { Cpu = _compute.Cpu, Gpu = _compute.Gpu }
            : null;

        var report = _builder.Build(
            ownerUid: _auth.CurrentUser?.Uid,
            speed: _network.LatestSpeedTest,
            stability: _network.StabilityResult,
            click: _mouse.Click,
            mousePolling: _mouse.Polling,
            typing: _keyboard.Typing,
            refreshRate: _monitor.RefreshRate,
            benchmarks: suite);

        _report.SetReport(report);
    }

    public string ToMarkdown() => _report.Report is null ? "" : _builder.ToMarkdown(_report.Report);
}
