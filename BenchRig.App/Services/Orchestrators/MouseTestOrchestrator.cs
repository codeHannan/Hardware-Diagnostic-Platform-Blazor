using Microsoft.JSInterop;
using BenchRig.App.Core.Engines.Mouse;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Mouse;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Orchestrators;

/// <summary>Drives mouse capture and feeds raw pointer samples into the mouse engines.</summary>
public sealed class MouseTestOrchestrator : IAsyncDisposable
{
    private readonly IJsMouseBridge _bridge;
    private readonly MouseDiagState _state;
    private readonly CpsCalculator _cps = new();
    private readonly PollingRateAnalyzer _polling = new();
    private readonly JitterAnalyzer _jitter = new();

    private DotNetObjectReference<MouseTestOrchestrator>? _selfRef;
    private readonly List<double> _pollTimestamps = new(4096);
    private int _clickCount;
    private DateTime _clickWindowStart;

    public MouseTestOrchestrator(IJsMouseBridge bridge, MouseDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    private DotNetObjectReference<MouseTestOrchestrator> SelfRef => _selfRef ??= DotNetObjectReference.Create(this);

    public async Task StartCpsTestAsync(string elementId)
    {
        _clickCount = 0;
        _clickWindowStart = DateTime.UtcNow;
        _state.SetRunning(true);
        await _bridge.StartPointerCaptureAsync(elementId, SelfRef);
    }

    public async Task StopCpsTestAsync(string elementId)
    {
        await _bridge.StopPointerCaptureAsync(elementId);
        double dur = Math.Max(0.001, (DateTime.UtcNow - _clickWindowStart).TotalSeconds);
        _state.SetClick(new ClickTestResult { Cps = _cps.Calculate(_clickCount, dur), TotalClicks = _clickCount, DurationSec = dur });
        _state.SetRunning(false);
    }

    public async Task StartPollingTestAsync(string elementId)
    {
        _pollTimestamps.Clear();
        _state.SetRunning(true);
        await _bridge.StartPollingRateCaptureAsync(elementId, SelfRef);
    }

    public async Task StopPollingTestAsync(string elementId)
    {
        await _bridge.StopPollingRateCaptureAsync(elementId);
        _state.SetPolling(_polling.Analyze(_pollTimestamps));
        _state.SetRunning(false);
    }

    [JSInvokable]
    public void OnPointerEvent(PointerSample sample)
    {
        // bit 1 = left button held; treat a fresh press as a click
        if (sample.Buttons != 0)
        {
            _clickCount++;
            double dur = Math.Max(0.001, (DateTime.UtcNow - _clickWindowStart).TotalSeconds);
            _state.SetLiveCps(_cps.Calculate(_clickCount, dur));
        }
    }

    [JSInvokable]
    public void OnPollSample(double timeStampMs) => _pollTimestamps.Add(timeStampMs);

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _bridge.DisposeAsync();
    }
}
