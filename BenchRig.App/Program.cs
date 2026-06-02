using BenchRig.App;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Services.Auth;
using BenchRig.App.Services.JsInterop;
using BenchRig.App.Services.Orchestrators;
using BenchRig.App.Services.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// -- HttpClient --
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// == State Containers (Scoped = one per browser tab) ==
builder.Services.AddScoped<AuthStateContainer>();
builder.Services.AddScoped<NetworkDiagState>();
builder.Services.AddScoped<MouseDiagState>();
builder.Services.AddScoped<KeyboardDiagState>();
builder.Services.AddScoped<AudioDiagState>();
builder.Services.AddScoped<MonitorDiagState>();
builder.Services.AddScoped<ComputeDiagState>();
builder.Services.AddScoped<ReportState>();
builder.Services.AddScoped<ForumState>();
builder.Services.AddScoped<TicketState>();
builder.Services.AddScoped<ChatbotState>();

// == JS Interop Bridges (Interface -> Implementation) ==
builder.Services.AddScoped<IJsNetworkBridge, NetworkJsInterop>();
builder.Services.AddScoped<IJsMouseBridge, MouseJsInterop>();
builder.Services.AddScoped<IJsKeyboardBridge, KeyboardJsInterop>();
builder.Services.AddScoped<IJsAudioBridge, AudioJsInterop>();
builder.Services.AddScoped<IJsMonitorBridge, MonitorJsInterop>();
builder.Services.AddScoped<IJsComputeBridge, ComputeJsInterop>();
builder.Services.AddScoped<IJsFirebaseAuthBridge, FirebaseAuthJsInterop>();
builder.Services.AddScoped<IJsFirestoreBridge, FirestoreJsInterop>();
builder.Services.AddScoped<IJsGeoLocationBridge, GeoLocationJsInterop>();
builder.Services.AddScoped<IJsStripeBridge, StripeJsInterop>();
builder.Services.AddScoped<IJsChatbotBridge, ChatbotJsInterop>();
builder.Services.AddScoped<IJsLeafletBridge, LeafletJsInterop>();

// == Orchestrators ==
builder.Services.AddScoped<NetworkTestOrchestrator>();
builder.Services.AddScoped<MouseTestOrchestrator>();
builder.Services.AddScoped<KeyboardTestOrchestrator>();
builder.Services.AddScoped<AudioTestOrchestrator>();
builder.Services.AddScoped<MonitorTestOrchestrator>();
builder.Services.AddScoped<ComputeTestOrchestrator>();
builder.Services.AddScoped<ReportOrchestrator>();

// == Application Services ==
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminGuardService>();

await builder.Build().RunAsync();
