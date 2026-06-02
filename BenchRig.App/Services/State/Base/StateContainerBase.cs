namespace BenchRig.App.Services.State.Base;

/// <summary>
/// Thread-safe reactive state container for Blazor components.
/// Subclasses hold domain-specific mutable state and call
/// <see cref="NotifyStateChanged"/> (or <see cref="NotifyThrottled"/>) after mutations.
/// </summary>
public abstract class StateContainerBase : IDisposable
{
    /// <summary>Components subscribe: <c>container.OnChange += StateHasChanged;</c></summary>
    public event Action? OnChange;

    protected void NotifyStateChanged()
    {
        var handler = OnChange;
        handler?.Invoke();
    }

    // -- Throttling for high-frequency streams (~60fps) --
    private DateTime _lastNotify = DateTime.MinValue;
    private static readonly TimeSpan ThrottleInterval = TimeSpan.FromMilliseconds(16);

    protected void NotifyThrottled()
    {
        var now = DateTime.UtcNow;
        if (now - _lastNotify >= ThrottleInterval)
        {
            _lastNotify = now;
            NotifyStateChanged();
        }
    }

    public virtual void Dispose() => OnChange = null;
}
