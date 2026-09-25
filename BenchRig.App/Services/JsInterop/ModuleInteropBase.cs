using Microsoft.JSInterop;

namespace BenchRig.App.Services.JsInterop;

/// <summary>
/// Base for JS-interop bridges that lazily import an ES module from wwwroot/js
/// and dispose it when the scope ends.
/// </summary>
public abstract class ModuleInteropBase : IAsyncDisposable
{
    protected readonly IJSRuntime Js;
    private readonly string _modulePath;
    private IJSObjectReference? _module;

    protected ModuleInteropBase(IJSRuntime js, string modulePath)
    {
        Js = js;
        _modulePath = modulePath;
    }

    protected async Task<IJSObjectReference> ModuleAsync()
        => _module ??= await Js.InvokeAsync<IJSObjectReference>("import", _modulePath);

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try { await _module.DisposeAsync(); }
            catch (JSDisconnectedException) { /* circuit gone */ }
            _module = null;
        }
        GC.SuppressFinalize(this);
    }
}
