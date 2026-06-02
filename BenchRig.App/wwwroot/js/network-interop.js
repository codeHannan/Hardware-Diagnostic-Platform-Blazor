// Network diagnostics.
//  - Connection info via ipinfo.io (rich data) + Cloudflare meta + browser APIs.
//  - Speed/latency against a selectable server:
//      * Cloudflare's edge (default, anycast, CORS-enabled)
//      * a BenchRig speed Worker you deploy (see /cloudflare-worker, type "worker")
//      * any CORS-enabled LibreSpeed-compatible server (type "ls")
//    Extra servers can be declared in wwwroot/network-servers.json.

const CF = { down: "https://speed.cloudflare.com/__down", up: "https://speed.cloudflare.com/__up" };
let SERVERS = [];

function httpsify(url) {
    if (!url) return "";
    if (url.startsWith("//")) return "https:" + url;
    return url.replace(/^http:\/\//i, "https://");
}
function join(base, path) {
    base = httpsify(base).replace(/\/+$/, "");
    return `${base}/${(path || "").replace(/^\/+/, "")}`;
}
async function reachable(url, opts) {
    const a = new AbortController(); const t = setTimeout(() => a.abort(), 4000);
    try { const r = await fetch(url, { ...(opts || {}), signal: a.signal, cache: "no-store" }); clearTimeout(t); return r.ok; }
    catch { clearTimeout(t); return false; }
}
function parseBrowser(ua) {
    if (/edg\//i.test(ua)) return "Edge";
    if (/firefox\//i.test(ua)) return "Firefox";
    if (/chrome\//i.test(ua)) return "Chrome";
    if (/safari\//i.test(ua)) return "Safari";
    return "Unknown";
}
function parseOs(ua) {
    if (/windows nt 10/i.test(ua)) return "Windows";
    if (/windows/i.test(ua)) return "Windows";
    if (/android/i.test(ua)) return "Android";
    if (/iphone|ipad|ios/i.test(ua)) return "iOS";
    if (/mac os x/i.test(ua)) return "macOS";
    if (/linux/i.test(ua)) return "Linux";
    return "Unknown";
}

// ── Connection details (ipinfo.io + Cloudflare meta + browser APIs) ──
export async function getNetworkInfo() {
    let info = { query: "Unavailable", hostname: "", isp: "", org: "", city: "", regionName: "", country: "", postal: "", timezone: "", lat: 0, lon: 0, asn: 0, colo: "" };
    try {
        const d = await (await fetch("https://ipinfo.io/json", { headers: { Accept: "application/json" }, cache: "no-store" })).json();
        const [lat, lon] = (d.loc || "0,0").split(",");
        let asn = 0, isp = d.org || "";
        const m = /^AS(\d+)\s+(.*)$/.exec(d.org || "");
        if (m) { asn = parseInt(m[1], 10); isp = m[2]; }
        info = {
            query: d.ip ?? "", hostname: d.hostname ?? "", isp, org: d.org ?? "",
            city: d.city ?? "", regionName: d.region ?? "", country: d.country ?? "",
            postal: d.postal ?? "", timezone: d.timezone ?? "",
            lat: parseFloat(lat) || 0, lon: parseFloat(lon) || 0, asn, colo: ""
        };
    } catch { /* keep defaults; cloudflare meta below still enriches */ }

    // Cloudflare meta: HTTP protocol + edge colo (best effort).
    try {
        const meta = await (await fetch("https://speed.cloudflare.com/meta", { cache: "no-store" })).json();
        info.httpProtocol = meta.httpProtocol || "";
        info.colo = meta.colo || info.colo;
        if (!info.query || info.query === "Unavailable") info.query = meta.clientIp || info.query;
    } catch { info.httpProtocol = ""; }

    // NetworkInformation API.
    const c = navigator.connection || navigator.mozConnection || navigator.webkitConnection || {};
    info.connType = c.effectiveType || "";
    info.downlink = c.downlink || 0;
    info.rtt = c.rtt || 0;

    // DNS-over-HTTPS reachability (Cloudflare 1.1.1.1).
    info.dohCloudflare = await reachable("https://cloudflare-dns.com/dns-query?name=example.com&type=A", { headers: { Accept: "application/dns-json" } });

    // Device.
    const ua = navigator.userAgent || "";
    info.browser = parseBrowser(ua);
    info.os = parseOs(ua);
    info.languages = (navigator.languages || []).join(", ");

    info.precise = false;
    return info;
}

// ── Precise location via the Geolocation API + reverse geocode (fixes coarse IP geo) ──
export async function getPreciseLocation() {
    const pos = await new Promise((resolve, reject) =>
        navigator.geolocation.getCurrentPosition(resolve, reject, { enableHighAccuracy: true, timeout: 12000, maximumAge: 0 }));
    const { latitude, longitude } = pos.coords;
    let city = "", region = "", country = "";
    try {
        const d = await (await fetch(`https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${latitude}&longitude=${longitude}&localityLanguage=en`, { cache: "no-store" })).json();
        city = d.city || d.locality || "";
        region = d.principalSubdivision || "";
        country = d.countryCode || "";
    } catch { /* coords only */ }
    return { lat: latitude, lon: longitude, city, regionName: region, country };
}

// ── Server list (Cloudflare + any declared in network-servers.json) ──
export async function getServers() {
    SERVERS = [{ key: "cf", name: "Cloudflare", location: "Auto - nearest edge", type: "cf", lat: 0, lng: 0 }];
    try {
        const r = await fetch("network-servers.json", { cache: "no-store" });
        if (r.ok) {
            const cfg = await r.json();
            if (Array.isArray(cfg)) {
                let i = 0;
                for (const s of cfg) {
                    if (!s || !s.url) continue;
                    SERVERS.push(normalize(s, "cfg" + (i++)));
                }
            }
        }
    } catch { /* Cloudflare only */ }
    return SERVERS;
}

function normalize(s, fallbackKey) {
    const base = httpsify(s.url);
    const type = s.type === "worker" ? "worker" : "ls";
    return {
        key: s.key || fallbackKey,
        name: s.name || "Server",
        location: s.location || "",
        type,
        dl: type === "worker" ? join(base, "down") : join(base, s.dlURL || "backend/garbage.php"),
        ul: type === "worker" ? join(base, "up") : join(base, s.ulURL || "backend/empty.php"),
        ping: type === "worker" ? join(base, "ping") : join(base, s.pingURL || "backend/empty.php"),
        lat: s.lat || 0, lng: s.lng || 0
    };
}

// Registers a user-provided server at runtime (worker or LibreSpeed).
export function addCustomServer(key, name, baseUrl, kind) {
    const s = normalize({ url: baseUrl, name, location: "Custom", type: kind || "ls" }, key);
    s.key = key;
    SERVERS.push(s);
    return s;
}

function find(serverId) {
    return SERVERS.find(s => s.key === serverId) || SERVERS[0] || { key: "cf", type: "cf" };
}
function downUrl(s) {
    if (s.type === "cf") return `${CF.down}?bytes=26214400&r=${Math.random()}`;
    if (s.type === "worker") return `${s.dl}?bytes=26214400&r=${Math.random()}`;
    return `${s.dl}?ckSize=25&r=${Math.random()}`;
}
function upUrl(s) { return s.type === "cf" ? CF.up : s.ul; }
function pingUrl(s) {
    if (s.type === "cf") return `${CF.down}?bytes=0`;
    return s.ping;
}

// ── Latency ──
export async function measureLatency(serverId, count) {
    const s = find(serverId);
    const url = pingUrl(s);
    const sep = url.includes("?") ? "&" : "?";
    const results = [];
    try { await fetch(`${url}${sep}r=${Math.random()}`, { cache: "no-store" }); } catch { /* warm-up */ }
    for (let i = 0; i < count; i++) {
        const t = performance.now();
        try { await fetch(`${url}${sep}r=${Math.random()}`, { cache: "no-store" }); results.push(performance.now() - t); }
        catch { /* skip */ }
    }
    return results;
}

// ── Download (parallel streaming reads) ──
export async function measureDownload(serverId, durationMs, dotNetRef, methodName) {
    const s = find(serverId);
    const start = performance.now();
    const deadline = start + durationMs;
    const controller = new AbortController();
    const streamCount = 6;
    let totalBytes = 0;

    const worker = async () => {
        while (performance.now() < deadline && !controller.signal.aborted) {
            try {
                const res = await fetch(downUrl(s), { cache: "no-store", signal: controller.signal });
                const reader = res.body.getReader();
                while (true) {
                    const { done, value } = await reader.read();
                    if (done) break;
                    totalBytes += value.length;
                    if (performance.now() >= deadline) { controller.abort(); break; }
                }
            } catch { break; }
        }
    };
    const timer = setInterval(() => {
        const e = (performance.now() - start) / 1000;
        dotNetRef.invokeMethodAsync(methodName, e > 0 ? (totalBytes * 8) / (e * 1e6) : 0);
    }, 100);
    const workers = Array.from({ length: streamCount }, worker);
    const stop = setTimeout(() => controller.abort(), durationMs + 500);
    await Promise.allSettled(workers);
    clearInterval(timer); clearTimeout(stop);
    const e = (performance.now() - start) / 1000;
    return e > 0 ? (totalBytes * 8) / (e * 1e6) : 0;
}

// ── Upload (repeated small POSTs) ──
export async function measureUpload(serverId, durationMs, dotNetRef, methodName) {
    const s = find(serverId);
    const start = performance.now();
    const deadline = start + durationMs;
    const controller = new AbortController();
    const streamCount = 3;
    const payload = new Uint8Array(256_000);
    const url = upUrl(s);
    let totalBytes = 0;

    const worker = async () => {
        while (performance.now() < deadline && !controller.signal.aborted) {
            try { await fetch(url, { method: "POST", body: payload, cache: "no-store", signal: controller.signal }); totalBytes += payload.length; }
            catch { break; }
        }
    };
    const timer = setInterval(() => {
        const e = (performance.now() - start) / 1000;
        dotNetRef.invokeMethodAsync(methodName, e > 0 ? (totalBytes * 8) / (e * 1e6) : 0);
    }, 100);
    const workers = Array.from({ length: streamCount }, worker);
    const stop = setTimeout(() => controller.abort(), durationMs + 500);
    await Promise.allSettled(workers);
    clearInterval(timer); clearTimeout(stop);
    const e = (performance.now() - start) / 1000;
    return e > 0 ? (totalBytes * 8) / (e * 1e6) : 0;
}
