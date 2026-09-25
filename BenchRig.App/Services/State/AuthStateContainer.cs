using BenchRig.App.Core.Models.Auth;
using BenchRig.App.Services.State.Base;

namespace BenchRig.App.Services.State;

public sealed class AuthStateContainer : StateContainerBase
{
    public UserProfile? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;
    public bool IsAdmin => CurrentUser?.IsAdmin == true;
    public bool IsInitialized { get; private set; }

    public void SetUser(UserProfile? user)
    {
        CurrentUser = user;
        IsInitialized = true;
        NotifyStateChanged();
    }

    public void Clear()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }
}
