using Microsoft.JSInterop;
using BenchRig.App.Core.Engines.Keyboard;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Keyboard;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Orchestrators;

/// <summary>Captures keyboard events and runs chatter / rollover / typing analysis.</summary>
public sealed class KeyboardTestOrchestrator : IAsyncDisposable
{
    private readonly IJsKeyboardBridge _bridge;
    private readonly KeyboardDiagState _state;
    private readonly ChatterDetector _chatter = new();
    private readonly RolloverAnalyzer _rollover = new();
    private readonly WpmCalculator _wpm = new();

    private DotNetObjectReference<KeyboardTestOrchestrator>? _selfRef;
    private readonly List<KeyEventSample> _events = new(2048);

    public KeyboardTestOrchestrator(IJsKeyboardBridge bridge, KeyboardDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    private DotNetObjectReference<KeyboardTestOrchestrator> SelfRef => _selfRef ??= DotNetObjectReference.Create(this);

    public async Task StartCaptureAsync()
    {
        _events.Clear();
        _state.ClearKeys();
        _state.SetRunning(true);
        await _bridge.StartKeyCaptureAsync(SelfRef);
    }

    public async Task StopCaptureAsync()
    {
        await _bridge.StopKeyCaptureAsync();
        _state.SetChatter(_chatter.Detect(_events));
        _state.SetRollover(_rollover.Analyze(_events));
        _state.SetRunning(false);
    }

    public TypingTestResult ComputeTyping(int correctChars, int totalChars, double durationSec)
    {
        var result = _wpm.Calculate(correctChars, totalChars, durationSec);
        _state.SetTyping(result);
        return result;
    }

    [JSInvokable]
    public void OnKeyEvent(KeyEventSample sample)
    {
        _events.Add(sample);
        if (sample.IsDown) _state.KeyDown(sample.Code);
        else _state.KeyUp(sample.Code);
    }

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _bridge.DisposeAsync();
    }
}
