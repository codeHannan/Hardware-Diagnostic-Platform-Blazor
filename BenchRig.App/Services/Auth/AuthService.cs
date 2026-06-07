using System.Text.Json;
using Microsoft.JSInterop;
using BenchRig.App.Core.Constants;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Auth;
using BenchRig.App.Services.JsInterop;
using BenchRig.App.Services.State;

namespace BenchRig.App.Services.Auth;

/// <summary>
/// Coordinates Firebase auth + Firestore user-profile sync and pushes the
/// resulting <see cref="UserProfile"/> into <see cref="AuthStateContainer"/>.
/// </summary>
public sealed class AuthService : IAsyncDisposable
{
    private readonly IJsFirebaseAuthBridge _auth;
    private readonly IJsFirestoreBridge _firestore;
    private readonly AuthStateContainer _state;
    private DotNetObjectReference<AuthService>? _selfRef;

    public AuthService(IJsFirebaseAuthBridge auth, IJsFirestoreBridge firestore, AuthStateContainer state)
    {
        _auth = auth;
        _firestore = firestore;
        _state = state;
    }

    /// <summary>Wires the Firebase onAuthStateChanged listener to this service.</summary>
    public async Task InitializeAsync()
    {
        _selfRef ??= DotNetObjectReference.Create(this);
        try
        {
            await _auth.RegisterAuthStateListenerAsync(_selfRef, nameof(OnAuthStateChanged));
        }
        finally
        {
            // Mark auth resolved (signed-out) so guards/UI don't hang if the listener
            // never fires (e.g. Firebase Auth not configured). The listener will update
            // CurrentUser later if a session exists.
            if (!_state.IsInitialized) _state.SetUser(null);
        }
    }

    public async Task SignInWithEmailAsync(string email, string password)
        => await SyncProfileAsync(await _auth.SignInWithEmailAsync(email, password));

    public async Task SignInWithGoogleAsync()
        => await SyncProfileAsync(await _auth.SignInWithGoogleAsync());

    public async Task RegisterWithEmailAsync(string email, string password, string displayName)
        => await SyncProfileAsync(await _auth.RegisterWithEmailAsync(email, password, displayName), isNew: true);

    public async Task SignOutAsync()
    {
        await _auth.SignOutAsync();
        _state.Clear();
    }

    /// <summary>Invoked from JS when Firebase auth state changes.</summary>
    [JSInvokable]
    public async Task OnAuthStateChanged(JsonElement? user)
    {
        var profile = FirebaseAuthJsInterop.MapUser(user);
        if (profile is null) { _state.SetUser(null); return; }
        await SyncProfileAsync(profile);
    }

    /// <summary>Ensures a Firestore users/{uid} doc exists and hydrates role into state.</summary>
    private async Task SyncProfileAsync(UserProfile? profile, bool isNew = false)
    {
        if (profile is null) { _state.SetUser(null); return; }

        var path = FirestoreCollections.User(profile.Uid);
        var existing = await _firestore.GetDocumentAsync<FirestoreUserDoc>(path);

        if (existing is null)
        {
            await _firestore.SetDocumentAsync(path, new
            {
                email = profile.Email,
                displayName = profile.DisplayName,
                photoURL = profile.PhotoUrl,
                role = "user",
                createdAt = DateTime.UtcNow.ToString("o"),
                lastLoginAt = DateTime.UtcNow.ToString("o"),
            });
            _state.SetUser(profile with { Role = UserRole.User });
        }
        else
        {
            await _firestore.UpdateDocumentAsync(path, new { lastLoginAt = DateTime.UtcNow.ToString("o") });
            var role = string.Equals(existing.Role, "admin", StringComparison.OrdinalIgnoreCase)
                ? UserRole.Admin : UserRole.User;
            _state.SetUser(profile with
            {
                Role = role,
                DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? existing.DisplayName : profile.DisplayName,
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _selfRef?.Dispose();
        await _auth.DisposeAsync();
    }

    private sealed record FirestoreUserDoc
    {
        public string Email { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public string Role { get; init; } = "user";
    }
}
