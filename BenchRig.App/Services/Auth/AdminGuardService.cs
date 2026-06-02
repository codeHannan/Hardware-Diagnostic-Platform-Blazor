using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Auth;

/// <summary>
/// C#-layer admin gate (defense-in-depth). Firestore rules remain the
/// authoritative enforcement; this prevents wasted round-trips and guards UI flows.
/// </summary>
public sealed class AdminGuardService
{
    private readonly AuthStateContainer _auth;

    public AdminGuardService(AuthStateContainer auth) => _auth = auth;

    public bool IsAdmin => _auth.IsAdmin;

    public void EnsureAdmin()
    {
        if (!_auth.IsAdmin)
            throw new UnauthorizedAccessException("This operation requires an administrator account.");
    }
}
