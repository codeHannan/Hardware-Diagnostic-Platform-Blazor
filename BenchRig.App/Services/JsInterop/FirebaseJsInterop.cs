using System.Text.Json;
using Microsoft.JSInterop;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Auth;

namespace BenchRig.App.Services.JsInterop;

public sealed class FirebaseAuthJsInterop : ModuleInteropBase, IJsFirebaseAuthBridge
{
    public FirebaseAuthJsInterop(IJSRuntime js) : base(js, "./js/firebase-auth-interop.js") { }

    public async Task<UserProfile?> SignInWithEmailAsync(string email, string password)
        => MapUser(await (await ModuleAsync()).InvokeAsync<JsonElement?>("signInWithEmail", email, password));

    public async Task<UserProfile?> SignInWithGoogleAsync()
        => MapUser(await (await ModuleAsync()).InvokeAsync<JsonElement?>("signInWithGoogle"));

    public async Task<UserProfile?> RegisterWithEmailAsync(string email, string password, string displayName)
        => MapUser(await (await ModuleAsync()).InvokeAsync<JsonElement?>("registerWithEmail", email, password, displayName));

    public async Task SignOutAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("firebaseSignOut");

    public async Task RegisterAuthStateListenerAsync<T>(DotNetObjectReference<T> callbackRef, string callbackMethodName) where T : class
        => await (await ModuleAsync()).InvokeVoidAsync("registerAuthStateListener", callbackRef, callbackMethodName);

    public async Task<UserProfile?> UpdateDisplayNameAsync(string displayName)
        => MapUser(await (await ModuleAsync()).InvokeAsync<JsonElement?>("updateDisplayName", displayName));

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        => await (await ModuleAsync()).InvokeVoidAsync("changePassword", currentPassword, newPassword);

    public async Task DeleteAccountAsync(string? currentPassword)
        => await (await ModuleAsync()).InvokeVoidAsync("deleteAccount", currentPassword);

    internal static UserProfile? MapUser(JsonElement? element)
    {
        if (element is not { ValueKind: JsonValueKind.Object } e) return null;
        return new UserProfile
        {
            Uid = e.GetProperty("uid").GetString() ?? "",
            Email = e.TryGetProperty("email", out var em) ? em.GetString() ?? "" : "",
            DisplayName = e.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? "" : "",
            PhotoUrl = e.TryGetProperty("photoURL", out var pu) ? pu.GetString() : null,
            ProviderId = e.TryGetProperty("providerId", out var pi) ? pi.GetString() ?? "password" : "password",
        };
    }
}

public sealed class FirestoreJsInterop : ModuleInteropBase, IJsFirestoreBridge
{
    public FirestoreJsInterop(IJSRuntime js) : base(js, "./js/firestore-interop.js") { }

    public async Task<string> AddDocumentAsync(string collectionPath, object data)
        => await (await ModuleAsync()).InvokeAsync<string>("addDocument", collectionPath, data);

    public async Task SetDocumentAsync(string documentPath, object data)
        => await (await ModuleAsync()).InvokeVoidAsync("setDocument", documentPath, data);

    public async Task UpdateDocumentAsync(string documentPath, object partialData)
        => await (await ModuleAsync()).InvokeVoidAsync("updateDocument", documentPath, partialData);

    public async Task DeleteDocumentAsync(string documentPath)
        => await (await ModuleAsync()).InvokeVoidAsync("deleteDocument", documentPath);

    public async Task<T?> GetDocumentAsync<T>(string documentPath)
        => await (await ModuleAsync()).InvokeAsync<T?>("getDocument", documentPath);

    public async Task<List<T>> QueryCollectionAsync<T>(string collectionPath, QueryFilter? filter = null)
        => await (await ModuleAsync()).InvokeAsync<List<T>>("queryCollection", collectionPath, filter)
           ?? new List<T>();

    public async Task<int> CountCollectionAsync(string collectionPath)
        => await (await ModuleAsync()).InvokeAsync<int>("countCollection", collectionPath);

    public async Task<string> SubscribeToDocumentAsync<TCallback>(string documentPath, DotNetObjectReference<TCallback> callbackRef, string callbackMethodName) where TCallback : class
        => await (await ModuleAsync()).InvokeAsync<string>("subscribeToDocument", documentPath, callbackRef, callbackMethodName);

    public async Task<string> SubscribeToCollectionAsync<TCallback>(string collectionPath, QueryFilter? filter, DotNetObjectReference<TCallback> callbackRef, string callbackMethodName) where TCallback : class
        => await (await ModuleAsync()).InvokeAsync<string>("subscribeToCollection", collectionPath, filter, callbackRef, callbackMethodName);

    public async Task UnsubscribeAsync(string subscriptionId)
        => await (await ModuleAsync()).InvokeVoidAsync("unsubscribe", subscriptionId);
}
