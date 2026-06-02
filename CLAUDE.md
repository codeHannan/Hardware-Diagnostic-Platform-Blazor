# BenchRig — Build Progress Tracker

> Live progress log for implementing the BenchRig Diagnostic Platform per `implementation_plan.md`.
> Single-project Blazor WASM (.NET 10) Clean Architecture. Firebase via JS interop.

## Design System
- **Palette**: `#222831` (charcoal bg), `#393E46` (slate surface), `#948979` (taupe accent), `#DFD0B8` (cream text/light accent)
- **Inspiration**: chaingpt.org labs — dark, sleek dashboard, glass cards, soft glows (not a 1:1 clone)
- Design tokens live in `wwwroot/css/app.css` as CSS custom properties.

## Conventions
- Namespaces mirror folders under `BenchRig.App.*`.
- `Core/` has **no** `IJSRuntime`/browser deps. Bridges in `Services/JsInterop` implement `Core/Interfaces`.
- Components subscribe to state containers via `OnChange`, unsubscribe on `Dispose`.
- Build check: `dotnet build BenchRig.slnx -c Debug`.

## Phase Checklist

### Phase 0 — Foundation & Design System ✅
- [x] CLAUDE.md tracker
- [x] Folder restructure (Core / Services / Components / Pages)
- [x] Design tokens + global CSS (palette)
- [x] `_Imports.razor` global usings
- [x] Constants: `AppRoutes`, `FirestoreCollections`, `DiagnosticDefaults`

### Phase 1 — Core Domain Models ✅
- [x] Network, Mouse, Keyboard, Audio, Monitor, Compute
- [x] Report, Auth, Forum, Support, Solutions, Chatbot

### Phase 2 — Core Interfaces (JS bridges) ✅
- [x] Network, Mouse, Keyboard, Audio, Monitor, Compute
- [x] FirebaseAuth, Firestore, GeoLocation, Stripe, Chatbot, Leaflet

### Phase 3 — Core Engines (pure C#) ✅
- [x] Network, Mouse, Keyboard, Audio, Monitor, Compute, Report

### Phase 4 — Services / State Containers ✅
- [x] StateContainerBase + all domain states

### Phase 5 — Services / JsInterop bridge impls ✅
- [x] All bridges (ModuleInteropBase pattern)

### Phase 6 — Services / Orchestrators + Auth ✅
- [x] Orchestrators + AuthService + AdminGuardService

### Phase 7 — Program.cs DI registration ✅
- [x] Register state, bridges, orchestrators, services

### Phase 8 — JavaScript modules (wwwroot/js) ✅
- [x] firebase-config, auth, firestore, network, mouse, keyboard, audio, monitor, compute, leaflet, stripe, chatbot

### Phase 9 — Layout & Shared components ✅
- [x] MainLayout (wires Firebase auth listener), Sidebar, Topbar, FooterBar, ChatBubble/ChatWindow
- [x] MetricCard, GaugeChart, ProgressRing, StatusBadge, LoadingSpinner, MarkdownRenderer
- [ ] LiveGraph, ConfirmDialog, ToastNotification, FullscreenOverlay (not yet needed — inline equivalents used)

### Phase 10 — Pages & Panels ✅ (core complete)
- [x] Home dashboard (hero + suite tiles)
- [x] Network (ServerSelector, NetworkInfo, SpeedTest, Latency, Stress, ServerMap/Leaflet)
- [x] Mouse (CPS, Polling rate, Button test)
- [x] Keyboard (Key registration visualizer + chatter/rollover, Typing test)
- [x] Audio (Playback tones, Mic level + echo)
- [x] Monitor (Dead pixel fullscreen, Refresh rate, Ghosting canvas)
- [x] Compute (CPU worker bench, GPU WebGL bench)
- [x] Report (compile + Markdown export), Donate (Stripe links)
- [x] Auth (Login/Register), Forum (feed + post + comments + rating)
- [x] Support (ticket submit + list), Solutions (KB browser)
- [x] Admin (dashboard, tickets queue, article CRUD) with role guards
- [ ] Optional extras not built: Jitter/DoubleClick/Scroll/DPI/DeadZone/Accuracy/DragDrop mouse panels,
      ChatterDetect/Rollover/StuckKey/SpecialKey keyboard panels, Balance/EchoLatency audio panels,
      ColorGradient/Backlight/Gamma/ResponseTime monitor panels (engines/models/bridges already in place)

### Phase 11 — Host config ✅
- [x] index.html (Firebase importmap, Leaflet CDN, Stripe.js, fonts, scoped CSS, download helper)
- [x] firestore.rules at repo root (security rules from plan)
- [x] .claude/launch.json for `dotnet run` preview

## Verification
- Builds clean (0 warnings / 0 errors).
- Ran in browser: dashboard, Network (Leaflet map + LibreSpeed fallback servers + geo),
  and Mouse CPS test (live pointer-capture interop) all render and function with **no console errors**.
- Hardened all `async void` event handlers (ServerMapPanel, SupportPage, MicLoopbackPanel) so a
  transient JS error can never tear down the WASM runtime.

## Notes / Decisions
- **Network rework (Cloudflare backend)**: LibreSpeed/public speedtest servers block cross-origin
  browser fetches (CORS) and the fallback hosts didn't exist, so download/geo always failed.
  Replaced the whole network stack with **speed.cloudflare.com** (`/meta`, `/__down`, `/__up`),
  which is fully CORS-enabled. Now gives real download/upload/ping/jitter + accurate IP/ISP/ASN/geo.
  - New flow: `NetworkTestOrchestrator.RunFullTestAsync` → latency(20) → download(10s) → upload(8s)
    with live Mbps streamed to state via `[JSInvokable] OnDownloadProgress/OnUploadProgress`.
  - New UI: speedtest.net-style `SpeedTestPanel` + 270° `SpeedGauge` (GO button, live needle,
    Download/Upload/Ping/Jitter cards, provider+server strip). `NetworkInfoPanel` shows IP/ISP/ASN/
    location/edge; `LatencyPanel` shows idle ping + jitter + packet loss + RTT sparkline; `ServerMapPanel`
    pins the user's real location.
  - Upload uses 256 KB chunks (2 MB chunks never finished within the window on slow uplinks → 0 Mbps).
  - Removed `ServerSelector` and `StressTestPanel` (single Cloudflare edge replaces multi-server list).
  - Verified live in browser: real results (e.g. 4.3↓ / 2.6↑ Mbps, 126 ms ping), map pins location,
    no console errors.
- **Network polish pass** (per user feedback):
  1. Gauge redesigned: removed the glitchy needle + dense ticks; now a clean 270° arc with a glowing
     tip and eased (`cubic-bezier`) transitions → smooth, uncluttered.
  2. Connection info now uses **ipinfo.io** (`/json`) for rich "What Is My IP" detail: IP, hostname,
     ISP/Org, ASN, city/region/country, postal, coordinates, timezone. Cloudflare `/meta` is the fallback.
  3. **Server selection**: added a Test-Server dropdown. Cloudflare (anycast edge) is the default —
     it's the only public backend that reliably sends `Access-Control-Allow-Origin: *`. Public LibreSpeed
     servers and most CDNs block cross-origin browser fetches, so users can **Add** their own CORS-enabled
     LibreSpeed-compatible server (name + URL); the test then runs against it.
  4. Reorganized layout: server bar → centered gauge + GO → 4 metric cards → provider/IP/server strip;
     `What Is My IP` and map below. All verified live, no console errors.
- **Network Details pass** (per user feedback):
  - Renamed `What Is My IP` → **Network Details**, grouped into Identity / Connection / Location / Device.
  - Added more details: HTTP protocol + CF edge (cloudflare /meta), connection type/downlink/RTT
    (NetworkInformation API), **DNS-over-HTTPS reachability** (Cloudflare 1.1.1.1), browser/OS/languages.
    (WebRTC local IP omitted — modern browsers mDNS-obfuscate it; NextDNS/Google-DoH are CORS-blocked.)
  - **Location fix**: IP geo is ISP-level (showed Lahore). Added **"Use precise location"** → browser
    Geolocation API + BigDataCloud reverse-geocode (CORS-free) → updates city/region/country to the real
    location (e.g. Rawalpindi), re-centers the map, and flags it "precise". Falls back gracefully if denied.
  - **Cloudflare Worker** (`/cloudflare-worker/`): deployable free worker (`/down`,`/up`,`/ping`,`/meta`,
    `/proxy`) giving a CORS-enabled backend you control + a proxy for non-CORS upstreams. App loads optional
    `wwwroot/network-servers.json` to add worker/LibreSpeed servers to the dropdown; runtime **+ Add** form
    now supports both "LibreSpeed" and "BenchRig Worker" types.

## Build Status
- Baseline scaffold builds clean (.NET 10.0.300).
</content>
</invoke>
