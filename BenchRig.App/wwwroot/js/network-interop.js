// Network diagnostics (Ookla-style).
//  - Rich connection info aggregated from GeoJS (accurate city) + ipinfo + Cloudflare + browser APIs.
//  - Multi-stream download/upload with warm-up + steady-state measurement (scales to ~1 Gbps).
//  - Idle + loaded latency (bufferbloat), jitter, packet loss.
//  - Selectable server: Cloudflare edge (default), a BenchRig Worker, or a LibreSpeed server.

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
async function getJson(url, opts, ms = 6000) {
    const a = new AbortController(); const t = setTimeout(() => a.abort(), ms);
    try { const r = await fetch(url, { ...(opts || {}), signal: a.signal, cache: "no-store" }); clearTimeout(t); return r.ok ? await r.json() : null; }
    catch { clearTimeout(t); return null; }
}
function parseBrowser(ua) {
    if (/edg\//i.test(ua)) return "Edge";
    if (/opr\/|opera/i.test(ua)) return "Opera";
    if (/firefox\//i.test(ua)) return "Firefox";
    if (/chrome\//i.test(ua)) return "Chrome";
    if (/safari\//i.test(ua)) return "Safari";
    return "Unknown";
}
function parseOs(ua) {
    if (/windows nt 10/i.test(ua)) return "Windows 10/11";
    if (/windows/i.test(ua)) return "Windows";
    if (/android/i.test(ua)) return "Android";
    if (/iphone|ipad/i.test(ua)) return "iOS";
    if (/mac os x/i.test(ua)) return "macOS";
    if (/linux/i.test(ua)) return "Linux";
    return "Unknown";
}

// ── Connection details (aggregated) ──
export async function getNetworkInfo() {
    const [geo, ipinfo, meta] = await Promise.all([
        getJson("https://get.geojs.io/v1/ip/geo.json"),
        getJson("https://ipinfo.io/json", { headers: { Accept: "application/json" } }),
        getJson("https://speed.cloudflare.com/meta")
    ]);

    // Prefer GeoJS for location (more accurate city), ipinfo for postal/hostname.
    let asn = 0, isp = "";
    if (geo?.asn) { asn = parseInt(geo.asn, 10) || 0; isp = geo.organization_name || ""; }
    if (!asn && ipinfo?.org) { const m = /^AS(\d+)\s+(.*)$/.exec(ipinfo.org); if (m) { asn = parseInt(m[1], 10); isp = m[2]; } else isp = ipinfo.org; }

    const lat = parseFloat(geo?.latitude ?? (ipinfo?.loc || "0,0").split(",")[0]) || 0;
    const lon = parseFloat(geo?.longitude ?? (ipinfo?.loc || "0,0").split(",")[1]) || 0;
    const tz = geo?.timezone || ipinfo?.timezone || Intl.DateTimeFormat().resolvedOptions().timeZone || "";

    let utcOffset = "", localTime = "";
    try {
        if (tz) {
            const parts = new Intl.DateTimeFormat("en-US", { timeZone: tz, timeZoneName: "shortOffset", hour: "2-digit", minute: "2-digit", hour12: false }).formatToParts(new Date());
            localTime = `${parts.find(p => p.type === "hour")?.value}:${parts.find(p => p.type === "minute")?.value}`;
            utcOffset = parts.find(p => p.type === "timeZoneName")?.value || "";
        }
    } catch { /* ignore */ }

    const ua = navigator.userAgent || "";
    const c = navigator.connection || navigator.mozConnection || navigator.webkitConnection || {};

    return {
        query: geo?.ip || ipinfo?.ip || meta?.clientIp || "Unavailable",
        hostname: ipinfo?.hostname || "",
        isp, org: geo?.organization || ipinfo?.org || isp,
        asn,
        city: geo?.city || ipinfo?.city || "",
        regionName: geo?.region || ipinfo?.region || "",
        country: geo?.country || ipinfo?.country || "",
        countryCode: geo?.country_code || ipinfo?.country || "",
        continent: geo?.continent_code || "",
        postal: ipinfo?.postal || "",
        timezone: tz, utcOffset, localTime,
        lat, lon,
        accuracyKm: geo?.accuracy ? parseInt(geo.accuracy, 10) : 0,
        colo: meta?.colo || "",
        httpProtocol: meta?.httpProtocol || "",
        dohCloudflare: await reachable("https://cloudflare-dns.com/dns-query?name=example.com&type=A", { headers: { Accept: "application/dns-json" } }),
        connType: c.effectiveType || "",
        downlink: c.downlink || 0,
        rtt: c.rtt || 0,
        saveData: !!c.saveData,
        browser: parseBrowser(ua),
        os: parseOs(ua),
        languages: (navigator.languages || []).join(", "),
        cores: navigator.hardwareConcurrency || 0,
        memory: navigator.deviceMemory || 0,
        touchPoints: navigator.maxTouchPoints || 0,
        online: navigator.onLine,
        cookiesEnabled: navigator.cookieEnabled,
        screen: `${screen.width} x ${screen.height} @${window.devicePixelRatio}x`,
        precise: false
    };
}

export async function getPreciseLocation() {
    const pos = await new Promise((resolve, reject) =>
        navigator.geolocation.getCurrentPosition(resolve, reject, { enableHighAccuracy: true, timeout: 12000, maximumAge: 0 }));
    const { latitude, longitude } = pos.coords;
    let city = "", region = "", country = "";
    const d = await getJson(`https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${latitude}&longitude=${longitude}&localityLanguage=en`);
    if (d) { city = d.city || d.locality || ""; region = d.principalSubdivision || ""; country = d.countryName || ""; }
    return { lat: latitude, lon: longitude, city, regionName: region, country };
}

// ── Servers ──
export async function getServers() {
    SERVERS = [{ key: "cf", name: "Cloudflare", location: "Auto - nearest edge", type: "cf", lat: 0, lng: 0 }];
    const cfg = await getJson("network-servers.json");
    if (Array.isArray(cfg)) { let i = 0; for (const s of cfg) if (s?.url) SERVERS.push(normalize(s, "cfg" + (i++))); }
    return SERVERS;
}
function normalize(s, fallbackKey) {
    const base = httpsify(s.url);
    const type = s.type === "worker" ? "worker" : "ls";
    return {
        key: s.key || fallbackKey, name: s.name || "Server", location: s.location || "", type,
        dl: type === "worker" ? join(base, "down") : join(base, s.dlURL || "backend/garbage.php"),
        ul: type === "worker" ? join(base, "up") : join(base, s.ulURL || "backend/empty.php"),
        ping: type === "worker" ? join(base, "ping") : join(base, s.pingURL || "backend/empty.php"),
        lat: s.lat || 0, lng: s.lng || 0
    };
}
export function addCustomServer(key, name, baseUrl, kind) {
    const s = normalize({ url: baseUrl, name, location: "Custom", type: kind || "ls" }, key);
    s.key = key; SERVERS.push(s); return s;
}
function find(id) { return SERVERS.find(s => s.key === id) || SERVERS[0] || { key: "cf", type: "cf" }; }
function downUrl(s) {
    if (s.type === "cf") return `${CF.down}?bytes=100000000&r=${Math.random()}`;
    if (s.type === "worker") return `${s.dl}?bytes=100000000&r=${Math.random()}`;
    return `${s.dl}?ckSize=100&r=${Math.random()}`;
}
function upUrl(s) { return s.type === "cf" ? CF.up : s.ul; }
function pingUrl(s) { return s.type === "cf" ? `${CF.down}?bytes=0` : s.ping; }

// ── Latency: returns array of RTT ms ──
export async function measureLatency(serverId, count) {
    const s = find(serverId);
    const url = pingUrl(s); const sep = url.includes("?") ? "&" : "?";
    const results = [];
    try { await fetch(`${url}${sep}r=${Math.random()}`, { cache: "no-store" }); } catch { /* warm-up */ }
    for (let i = 0; i < count; i++) {
        const t = performance.now();
        try { await fetch(`${url}${sep}r=${Math.random()}`, { cache: "no-store" }); results.push(performance.now() - t); }
        catch { /* dropped */ }
        await new Promise(r => setTimeout(r, 60));
    }
    return results;
}

// ── Download: multi-stream, warm-up + steady-state, sliding-window live speed, loaded latency ──
export async function measureDownload(serverId, durationMs, dotNetRef, methodName) {
    const s = find(serverId);
    const start = performance.now();
    const warmupMs = 1500;
    const deadline = start + durationMs;
    const controller = new AbortController();
    // Cap at 6: browsers allow ~6 connections per host, so more just queue.
    const streamCount = 6;
    let totalBytes = 0, warmupBytes = 0, warmupEnd = 0;

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

    // Bufferbloat: compare idle vs loaded RTT. Prefer a DIFFERENT host (Cloudflare
    // DoH) so the probe isn't starved by the download connection pool; fall back to
    // the test server's ping endpoint if DoH is blocked.
    const median = arr => { if (!arr.length) return 0; const s2 = [...arr].sort((a, b) => a - b); return s2[Math.floor(s2.length / 2)]; };
    const timedPing = async (url, opts) => {
        const a = new AbortController(); const id = setTimeout(() => a.abort(), 3000);
        const t0 = performance.now();
        try { await fetch(url, { ...(opts || {}), cache: "no-store", signal: a.signal }); clearTimeout(id); return performance.now() - t0; }
        catch { clearTimeout(id); const dt = performance.now() - t0; return dt >= 2800 ? dt : -1; }
    };
    const dohPing = () => timedPing(`https://cloudflare-dns.com/dns-query?name=r${Math.random()}.test&type=A`, { headers: { Accept: "application/dns-json" } });
    const pUrl = pingUrl(s); const pSep = pUrl.includes("?") ? "&" : "?";
    const srvPing = () => timedPing(`${pUrl}${pSep}r=${Math.random()}`);

    const idleDohArr = [], idleSrvArr = [];
    for (let i = 0; i < 4; i++) { const d = await dohPing(); if (d >= 0) idleDohArr.push(d); const v = await srvPing(); if (v >= 0) idleSrvArr.push(v); }
    const idleDoh = median(idleDohArr), idleSrv = median(idleSrvArr);

    const loadedDoh = [], loadedSrv = [];
    let pinging = true;
    const pingLoop = (async () => {
        await new Promise(r => setTimeout(r, warmupMs));
        while (performance.now() < deadline && pinging) {
            const d = await dohPing(); if (d >= 0) loadedDoh.push(d);
            else { const v = await srvPing(); if (v >= 0) loadedSrv.push(v); }
            await new Promise(r => setTimeout(r, 300));
        }
    })();

    const samples = [{ t: start, b: 0 }];
    const timer = setInterval(() => {
        const now = performance.now();
        if (warmupEnd === 0 && now - start >= warmupMs) { warmupEnd = now; warmupBytes = totalBytes; }
        samples.push({ t: now, b: totalBytes });
        while (samples.length > 2 && now - samples[0].t > 900) samples.shift();
        const f = samples[0], l = samples[samples.length - 1];
        const dt = (l.t - f.t) / 1000, db = l.b - f.b;
        dotNetRef.invokeMethodAsync(methodName, dt > 0 ? (db * 8) / (dt * 1e6) : 0);
    }, 100);

    const workers = Array.from({ length: streamCount }, worker);
    const stop = setTimeout(() => controller.abort(), durationMs + 1000);
    await Promise.allSettled(workers);
    // Capture the measurement endpoint NOW (workers stop counting bytes at the deadline).
    // Doing this before the bufferbloat ping loop unwinds is critical: its trailing pings
    // (up to ~3s) would otherwise inflate the elapsed time and make the rate read far too low.
    const measEnd = Math.min(performance.now(), deadline);
    const measEndBytes = totalBytes;
    pinging = false;
    await pingLoop.catch(() => { });
    clearInterval(timer); clearTimeout(stop);

    const measBytes = warmupEnd ? measEndBytes - warmupBytes : measEndBytes;
    const measSec = warmupEnd ? (measEnd - warmupEnd) / 1000 : (measEnd - start) / 1000;
    const mbps = measSec > 0 ? (measBytes * 8) / (measSec * 1e6) : 0;
    let bloatMs = 0;
    if (loadedDoh.length && idleDoh > 0) bloatMs = Math.max(0, median(loadedDoh) - idleDoh);
    else if (loadedSrv.length && idleSrv > 0) bloatMs = Math.max(0, median(loadedSrv) - idleSrv);
    return { mbps, bloatMs };
}

// ── Upload: multi-stream, warm-up + steady-state, sliding-window live speed ──
// Small 64 KB POSTs keep many completing inside the window, so a slow uplink averages
// cleanly; the measurement window is clamped to the deadline so trailing aborts don't
// inflate the elapsed time (which previously dragged the rate down).
export async function measureUpload(serverId, durationMs, dotNetRef, methodName) {
    const s = find(serverId);
    const start = performance.now();
    const warmupMs = 1200;
    const deadline = start + durationMs;
    const controller = new AbortController();
    const streamCount = 4;
    const payload = new Uint8Array(64_000);
    const url = upUrl(s);
    let totalBytes = 0, warmupBytes = 0, warmupEnd = 0;

    const worker = async () => {
        while (performance.now() < deadline && !controller.signal.aborted) {
            try { await fetch(url, { method: "POST", body: payload, cache: "no-store", signal: controller.signal }); totalBytes += payload.length; }
            catch { break; }
        }
    };
    const samples = [{ t: start, b: 0 }];
    const timer = setInterval(() => {
        const now = performance.now();
        if (warmupEnd === 0 && now - start >= warmupMs) { warmupEnd = now; warmupBytes = totalBytes; }
        samples.push({ t: now, b: totalBytes });
        while (samples.length > 2 && now - samples[0].t > 900) samples.shift();
        const f = samples[0], l = samples[samples.length - 1];
        const dt = (l.t - f.t) / 1000, db = l.b - f.b;
        dotNetRef.invokeMethodAsync(methodName, dt > 0 ? (db * 8) / (dt * 1e6) : 0);
    }, 100);

    const workers = Array.from({ length: streamCount }, worker);
    const stop = setTimeout(() => controller.abort(), durationMs + 1000);
    await Promise.allSettled(workers);
    const measEnd = Math.min(performance.now(), deadline);
    clearInterval(timer); clearTimeout(stop);

    const measBytes = warmupEnd ? totalBytes - warmupBytes : totalBytes;
    const measSec = warmupEnd ? (measEnd - warmupEnd) / 1000 : (measEnd - start) / 1000;
    return measSec > 0 ? (measBytes * 8) / (measSec * 1e6) : 0;
}
