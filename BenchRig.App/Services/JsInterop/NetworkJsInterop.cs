using System.Text.Json;
using Microsoft.JSInterop;
using BenchRig.App.Core.Interfaces;
using BenchRig.App.Core.Models.Network;

namespace BenchRig.App.Services.JsInterop;

public sealed class NetworkJsInterop : ModuleInteropBase, IJsNetworkBridge
{
    public NetworkJsInterop(IJSRuntime js) : base(js, "./js/network-interop.js") { }

    public async Task<NetworkInfo> GetNetworkInfoAsync()
    {
        var module = await ModuleAsync();
        var r = await module.InvokeAsync<JsonElement>("getNetworkInfo");
        return new NetworkInfo
        {
            PublicIp = Str(r, "query"),
            Hostname = Str(r, "hostname"),
            Isp = Str(r, "isp"),
            Org = Str(r, "org"),
            City = Str(r, "city"),
            Region = Str(r, "regionName"),
            Country = Str(r, "country"),
            Postal = Str(r, "postal"),
            Timezone = Str(r, "timezone"),
            Latitude = Dbl(r, "lat"),
            Longitude = Dbl(r, "lon"),
            AccuracyKm = (int)Dbl(r, "accuracyKm"),
            Colo = Str(r, "colo"),
            Asn = (long)Dbl(r, "asn"),
            CountryCode = Str(r, "countryCode"),
            Continent = Str(r, "continent"),
            UtcOffset = Str(r, "utcOffset"),
            LocalTime = Str(r, "localTime"),
            HttpProtocol = Str(r, "httpProtocol"),
            ConnType = Str(r, "connType"),
            Downlink = Dbl(r, "downlink"),
            Rtt = Dbl(r, "rtt"),
            SaveData = Boolean(r, "saveData"),
            DohCloudflare = Boolean(r, "dohCloudflare"),
            Browser = Str(r, "browser"),
            Os = Str(r, "os"),
            Languages = Str(r, "languages"),
            Cores = (int)Dbl(r, "cores"),
            Memory = Dbl(r, "memory"),
            TouchPoints = (int)Dbl(r, "touchPoints"),
            Online = Boolean(r, "online"),
            CookiesEnabled = Boolean(r, "cookiesEnabled"),
            Screen = Str(r, "screen"),
        };
    }

    public async Task<GeoPoint?> GetPreciseLocationAsync()
    {
        var module = await ModuleAsync();
        var r = await module.InvokeAsync<JsonElement?>("getPreciseLocation");
        if (r is not { ValueKind: JsonValueKind.Object } e) return null;
        return new GeoPoint
        {
            Latitude = Dbl(e, "lat"),
            Longitude = Dbl(e, "lon"),
            City = Str(e, "city"),
            Region = Str(e, "regionName"),
            Country = Str(e, "country"),
        };
    }

    public async Task<IReadOnlyList<DiagnosticServer>> GetServersAsync()
    {
        var module = await ModuleAsync();
        var arr = await module.InvokeAsync<JsonElement>("getServers");
        var list = new List<DiagnosticServer>();
        int i = 0;
        foreach (var e in arr.EnumerateArray())
        {
            list.Add(new DiagnosticServer
            {
                Key = Str(e, "key"),
                Id = i++,
                Name = Str(e, "name"),
                Location = Str(e, "location"),
                Server = Str(e, "dl"),
                Latitude = Dbl(e, "lat"),
                Longitude = Dbl(e, "lng"),
            });
        }
        return list;
    }

    public async Task AddCustomServerAsync(string key, string name, string baseUrl, string kind)
    {
        var module = await ModuleAsync();
        await module.InvokeVoidAsync("addCustomServer", key, name, baseUrl, kind);
    }

    public async Task<IReadOnlyList<double>> MeasureLatencyAsync(string serverId, int count)
    {
        var module = await ModuleAsync();
        return await module.InvokeAsync<double[]>("measureLatency", serverId, count);
    }

    public async Task<(double Mbps, double BufferbloatMs)> MeasureDownloadAsync<T>(string serverId, int durationMs, DotNetObjectReference<T> progress, string methodName) where T : class
    {
        var module = await ModuleAsync();
        var r = await module.InvokeAsync<JsonElement>("measureDownload", serverId, durationMs, progress, methodName);
        return (Dbl(r, "mbps"), Dbl(r, "bloatMs"));
    }

    public async Task<double> MeasureUploadAsync<T>(string serverId, int durationMs, DotNetObjectReference<T> progress, string methodName) where T : class
    {
        var module = await ModuleAsync();
        return await module.InvokeAsync<double>("measureUpload", serverId, durationMs, progress, methodName);
    }

    private static string Str(JsonElement e, string p)
        => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    private static double Dbl(JsonElement e, string p)
        => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
    private static bool Boolean(JsonElement e, string p)
        => e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.True;
}
