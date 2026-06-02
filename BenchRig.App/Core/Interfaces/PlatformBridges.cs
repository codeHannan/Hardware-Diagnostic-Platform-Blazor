using Microsoft.JSInterop;
using BenchRig.App.Core.Models.Auth;
using BenchRig.App.Core.Models.Network;
using BenchRig.App.Core.Models.Chatbot;

namespace BenchRig.App.Core.Interfaces;

public interface IJsFirebaseAuthBridge : IAsyncDisposable
{
    Task<UserProfile?> SignInWithEmailAsync(string email, string password);
    Task<UserProfile?> SignInWithGoogleAsync();
    Task<UserProfile?> RegisterWithEmailAsync(string email, string password, string displayName);
    Task SignOutAsync();
    Task RegisterAuthStateListenerAsync<T>(DotNetObjectReference<T> callbackRef, string callbackMethodName) where T : class;
}

public interface IJsFirestoreBridge : IAsyncDisposable
{
    Task<string> AddDocumentAsync(string collectionPath, object data);
    Task SetDocumentAsync(string documentPath, object data);
    Task UpdateDocumentAsync(string documentPath, object partialData);
    Task DeleteDocumentAsync(string documentPath);
    Task<T?> GetDocumentAsync<T>(string documentPath);
    Task<List<T>> QueryCollectionAsync<T>(string collectionPath, QueryFilter? filter = null);

    Task<string> SubscribeToDocumentAsync<TCallback>(string documentPath, DotNetObjectReference<TCallback> callbackRef, string callbackMethodName) where TCallback : class;
    Task<string> SubscribeToCollectionAsync<TCallback>(string collectionPath, QueryFilter? filter, DotNetObjectReference<TCallback> callbackRef, string callbackMethodName) where TCallback : class;
    Task UnsubscribeAsync(string subscriptionId);
}

public interface IJsGeoLocationBridge : IAsyncDisposable
{
    Task<NetworkInfo> LookupAsync();
    /// <summary>Haversine great-circle distance in km between two coordinates.</summary>
    double DistanceKm(double lat1, double lng1, double lat2, double lng2);
}

public interface IJsStripeBridge : IAsyncDisposable
{
    Task RedirectToCheckoutAsync(int amountCents);
}

public interface IJsChatbotBridge : IAsyncDisposable
{
    Task<string> SendMessageAsync(string userMessage, List<ChatMessage> history);
}

public interface IJsLeafletBridge : IAsyncDisposable
{
    Task InitMapAsync(string elementId, double lat, double lng, int zoom);
    Task PlotServersAsync(IEnumerable<DiagnosticServer> servers);
    Task PlotUserLocationAsync(double lat, double lng);
    Task DestroyMapAsync();
}
