using BenchRig.App.Core.Models.Mouse;
using BenchRig.App.Core.Models.Keyboard;
using BenchRig.App.Core.Models.Audio;
using BenchRig.App.Core.Models.Monitor;
using BenchRig.App.Core.Models.Compute;
using BenchRig.App.Services.State.Base;

namespace BenchRig.App.Services.State;

public sealed class MouseDiagState : StateContainerBase
{
    public ClickTestResult? Click { get; private set; }
    public PollingRateResult? Polling { get; private set; }
    public DpiEstimate? Dpi { get; private set; }
    public AccuracyResult? Accuracy { get; private set; }
    public IReadOnlyDictionary<MouseButton, bool> ButtonStates => _buttons;
    public double LiveCps { get; private set; }
    public bool IsRunning { get; private set; }

    private readonly Dictionary<MouseButton, bool> _buttons = new()
    {
        [MouseButton.Left] = false, [MouseButton.Right] = false, [MouseButton.Middle] = false,
        [MouseButton.Back] = false, [MouseButton.Forward] = false,
    };

    public void SetClick(ClickTestResult r) { Click = r; NotifyStateChanged(); }
    public void SetPolling(PollingRateResult r) { Polling = r; NotifyStateChanged(); }
    public void SetDpi(DpiEstimate r) { Dpi = r; NotifyStateChanged(); }
    public void SetAccuracy(AccuracyResult r) { Accuracy = r; NotifyStateChanged(); }
    public void SetLiveCps(double cps) { LiveCps = cps; NotifyThrottled(); }
    public void SetButton(MouseButton b, bool pressed) { _buttons[b] = pressed; NotifyThrottled(); }
    public void SetRunning(bool running) { IsRunning = running; NotifyStateChanged(); }
}

public sealed class KeyboardDiagState : StateContainerBase
{
    public IReadOnlyCollection<string> PressedKeys => _pressed;
    public IReadOnlyList<ChatterResult> Chatter => _chatter.AsReadOnly();
    public TypingTestResult? Typing { get; private set; }
    public RolloverResult? Rollover { get; private set; }
    public bool IsRunning { get; private set; }

    private readonly HashSet<string> _pressed = new();
    private readonly List<ChatterResult> _chatter = new();

    public void KeyDown(string code) { if (_pressed.Add(code)) NotifyThrottled(); }
    public void KeyUp(string code) { if (_pressed.Remove(code)) NotifyThrottled(); }
    public void SetChatter(IEnumerable<ChatterResult> c) { _chatter.Clear(); _chatter.AddRange(c); NotifyStateChanged(); }
    public void SetTyping(TypingTestResult r) { Typing = r; NotifyStateChanged(); }
    public void SetRollover(RolloverResult r) { Rollover = r; NotifyStateChanged(); }
    public void SetRunning(bool running) { IsRunning = running; NotifyStateChanged(); }
    public void ClearKeys() { _pressed.Clear(); NotifyStateChanged(); }
}

public sealed class AudioDiagState : StateContainerBase
{
    public PlaybackChannel Channel { get; private set; } = PlaybackChannel.Stereo;
    public MicLoopbackState? MicState { get; private set; }
    public EchoLatencyResult? Echo { get; private set; }
    public double MicLevelDb { get; private set; }
    public bool IsPlaying { get; private set; }

    public void SetChannel(PlaybackChannel c) { Channel = c; NotifyStateChanged(); }
    public void SetMicState(MicLoopbackState s) { MicState = s; NotifyStateChanged(); }
    public void SetEcho(EchoLatencyResult e) { Echo = e; NotifyStateChanged(); }
    public void SetMicLevel(double db) { MicLevelDb = db; NotifyThrottled(); }
    public void SetPlaying(bool playing) { IsPlaying = playing; NotifyStateChanged(); }
}

public sealed class MonitorDiagState : StateContainerBase
{
    public RefreshRateResult? RefreshRate { get; private set; }
    public string TestMode { get; private set; } = "none";
    public string CurrentColor { get; private set; } = "#000000";
    public bool IsFullscreen { get; private set; }

    public void SetRefreshRate(RefreshRateResult r) { RefreshRate = r; NotifyStateChanged(); }
    public void SetTestMode(string mode) { TestMode = mode; NotifyStateChanged(); }
    public void SetColor(string hex) { CurrentColor = hex; NotifyStateChanged(); }
    public void SetFullscreen(bool fs) { IsFullscreen = fs; NotifyStateChanged(); }
}

public sealed class ComputeDiagState : StateContainerBase
{
    public CpuBenchmarkResult? Cpu { get; private set; }
    public GpuBenchmarkResult? Gpu { get; private set; }
    public double ProgressPercent { get; private set; }
    public bool IsRunning { get; private set; }

    public void SetCpu(CpuBenchmarkResult r) { Cpu = r; NotifyStateChanged(); }
    public void SetGpu(GpuBenchmarkResult r) { Gpu = r; NotifyStateChanged(); }
    public void SetProgress(double p) { ProgressPercent = p; NotifyThrottled(); }
    public void SetRunning(bool running) { IsRunning = running; NotifyStateChanged(); }
}
