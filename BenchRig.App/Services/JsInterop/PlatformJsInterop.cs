using System.Text.Json;
using Microsoft.JSInterop;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Network;
using BenchRig.App.Core.Models.Chatbot;
using BenchRig.App.Core.Models.Donations;

namespace BenchRig.App.Services.JsInterop;

public sealed class GeoLocationJsInterop : ModuleInteropBase, IJsGeoLocationBridge
{
    public GeoLocationJsInterop(IJSRuntime js) : base(js, "./js/network-interop.js") { }

    public async Task<NetworkInfo> LookupAsync()
    {
        var r = await (await ModuleAsync()).InvokeAsync<JsonElement>("getNetworkInfo");
        return new NetworkInfo
        {
            PublicIp = S(r, "query"), Isp = S(r, "isp"), Org = S(r, "org"),
            City = S(r, "city"), Region = S(r, "regionName"), Country = S(r, "country"),
            Timezone = S(r, "timezone"), Latitude = D(r, "lat"), Longitude = D(r, "lon"),
        };
    }

    public double DistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371.0;
        double dLat = Deg2Rad(lat2 - lat1), dLng = Deg2Rad(lng2 - lng1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2))
                 * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double Deg2Rad(double d) => d * Math.PI / 180.0;
    private static string S(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    private static double D(JsonElement e, string p) => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
}

public sealed class StripeJsInterop : ModuleInteropBase, IJsStripeBridge
{
    public StripeJsInterop(IJSRuntime js) : base(js, "./js/stripe-interop.js") { }

    public async Task<DonationOptions> GetOptionsAsync()
        => await (await ModuleAsync()).InvokeAsync<DonationOptions>("getDonationOptions");

    public async Task<bool> OpenAsync(string url)
        => await (await ModuleAsync()).InvokeAsync<bool>("openDonation", url);
}

public sealed class ChatbotJsInterop : ModuleInteropBase, IJsChatbotBridge
{
    public ChatbotJsInterop(IJSRuntime js) : base(js, "./js/chatbot-interop.js") { }

    public async Task<string> SendMessageAsync(string userMessage, List<ChatMessage> history)
    {
        var historyDto = history.Select(m => new { role = m.Role, content = m.Content }).ToArray();
        return await (await ModuleAsync()).InvokeAsync<string>("sendChatMessage", userMessage, historyDto);
    }
}

public sealed class LeafletJsInterop : ModuleInteropBase, IJsLeafletBridge
{
    public LeafletJsInterop(IJSRuntime js) : base(js, "./js/leaflet-interop.js") { }

    public async Task InitMapAsync(string elementId, double lat, double lng, int zoom)
        => await (await ModuleAsync()).InvokeVoidAsync("initMap", elementId, lat, lng, zoom);

    public async Task PlotServersAsync(IEnumerable<DiagnosticServer> servers)
    {
        var dto = servers.Select(s => new { name = s.Label, lat = s.Latitude, lng = s.Longitude, pingMs = s.PingMs }).ToArray();
        await (await ModuleAsync()).InvokeVoidAsync("plotServers", dto);
    }

    public async Task PlotUserLocationAsync(double lat, double lng)
        => await (await ModuleAsync()).InvokeVoidAsync("plotUserLocation", lat, lng);

    public async Task DestroyMapAsync()
        => await (await ModuleAsync()).InvokeVoidAsync("destroyMap");
}
