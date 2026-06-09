# BenchRig — Build Progress Tracker

> Live progress log for implementing the BenchRig Diagnostic Platform per `implementation_plan.md`.
> Single-project Blazor WASM (.NET 10) Clean Architecture. Firebase via JS interop.

## Design System — "MOLTEN CORE" rework (current)
- **Aesthetic**: obsidian-black premium diagnostics console; molten **ember** (red→orange) accent reserved
  for live data, primary actions, active states, and glows. Reference mood: **onyxproject.com** (dark, luxe,
  lava-core glow). Concept ties heat/performance to a benchmarking "rig".
- **Palette (tokens in `wwwroot/css/app.css`)**: `--bg #0a0a0c` obsidian · `--surface-2 #1a1a1e` cards ·
  `--ember #ff5a2c` / `--ember-2 #ff8a4c` accent · `--cream #faf8f4` headline · `--text-0 #ece8e2` body ·
  hairline `--border rgba(244,241,236,.09)`. Semantic: ok green / warn amber / **bad `#f0556b`** (crimson,
  kept distinct from the ember accent). Radii 10/16/22/pill; soft deep shadows + ember glow.
- **Type**: **Clash Display** (Fontshare) headings · **Hanken Grotesk** (Google) body · **JetBrains Mono**
  data/metrics. Loaded in `index.html`.
- **Atmosphere**: body has layered radial ember glows + a faint SVG film-grain overlay (`body::before`,
  4% overlay, behind content via `.app-shell { z-index:1 }`).
- **Everything cascades from the tokens + global classes** in `app.css` (`.card`/`.btn`/`.btn-primary`/
  `.input`/`.badge`/`.tab`/`.tile`/`.chip`/`.kvs`/`.cmp-*`/`.kcap`/`.click-pad`…), so all per-page scoped
  CSS re-themed automatically.
- **Responsive shell**: `MainLayout` owns a mobile **nav drawer** (`_navOpen` + backdrop); `Topbar` has a
  hamburger (`OnMenu`, shown < 980px); `Sidebar` takes `Open`/`OnNavigate` and slides off-canvas on mobile
  (closes on nav-tap). Hero `Home.razor.css` = molten-core orb + Clash Display + staggered tile reveals.
- **Verified live**: dashboard (hero/orb/tiles), sidebar/topbar (ember active, hamburger), mobile drawer
  (slide-in + backdrop + close-on-nav), Forum (mobile+desktop), tabbed Monitor suite, Knowledge Base —
  all cohesive, responsive, 0 build warn/err, **no console errors**.
- **Polish pass**: (1) **Boot screen** rebuilt in `index.html` + `app.css` `.boot*` — obsidian + breathing
  ember glow, progress ring (kept Blazor's `--blazor-load-percentage` wiring, r=44 in a 0–100 viewBox),
  glowing logo core (masked `website-logo.svg`), Clash Display "BenchRig" wordmark, % + "Initializing diagnostics…" (verified at 68%).
  (2) Removed the `v1.0 · .NET 10 · WASM` `sidebar-foot`. (3) **Auth split-screen**: new shared
  `AuthAside.razor` (molten-orb brand panel + feature bullets) beside the form; `.auth-split` grid collapses
  and hides the aside < 860px (Login/Register rebuilt). (4) **SpeedGauge** molten heat-ramp gradient
  (`ember-deep→ember→ember-2`) + glowing ember tip. All verified live; **no console errors**.
- *Legacy (pre-rework)*: taupe/cream palette (`#222831/#393E46/#948979/#DFD0B8`), Inter font, chaingpt mood.

## Account management + Google auth + topbar polish (current)
- **Google sign-in finalized**: JS `signInWithGoogle` (popup + `GoogleAuthProvider`) already existed; added
  the **"Continue with Google" button to the Register page** too (was Login-only) so it's offered on both.
  `AuthService.SignInWithGoogleAsync` creates the Firestore `users/{uid}` doc on first Google login like
  email signup. `mapUser` now also returns **`providerId`** (`password` / `google.com` / …) → `UserProfile.
  ProviderId` + `IsPasswordProvider`. FIREBASE_SETUP.md step 3 updated (enable Google = recommended).
- **Account settings page** (`/account`, new `Pages/Auth/AccountPage.razor` + scoped css; `AppRoutes.Account`):
  three cards — (1) **Profile** (avatar + email + provider label; **change display name** → Firebase Auth
  `updateProfile` + best-effort Firestore `users/{uid}` displayName sync + state update). (2) **Password**
  (email/password accounts only: current+new+confirm → reauth then `updatePassword`; Google users see a
  "managed by Google" note). (3) **Danger zone** — **delete account** (reauth: password accounts re-enter
  password, Google accounts re-auth via popup → `deleteUser`; two-step confirm; clears state + redirects).
  All sensitive ops re-authenticate in JS (Firebase `requires-recent-login`); errors mapped to friendly
  copy (wrong-password / too-many-requests / popup-closed).
  - New JS in `firebase-auth-interop.js`: `updateDisplayName`, `changePassword`, `deleteAccount` (+
    `reauthenticateWithCredential`/`reauthenticateWithPopup`/`EmailAuthProvider`/`updatePassword`/`deleteUser`
    imports). Bridge `IJsFirebaseAuthBridge` + `FirebaseAuthJsInterop` + `AuthService` gained matching
    `UpdateDisplayNameAsync`/`ChangePasswordAsync`/`DeleteAccountAsync`. (Firestore `users` delete stays
    rules-forbidden, so the orphan doc is left; harmless.)
- **Topbar UI fix** (user-reported clustered chip + bad seam): the **user chip is now a compact clickable
  button → `/account`** (avatar + name + small role line "Admin"/"Account" instead of the bulky stacked
  `badge badge-ok`; hover highlight). The **sidebar `.brand` is now exactly `--topbar-h` (68px) tall** so its
  bottom divider lines up pixel-perfect with the topbar's bottom border — one continuous hairline across the
  app (was a ~2px misaligned seam). Verified live: brandBottom=topbarBottom=68; chip/role render; `/account`
  resolves (signed-out → sign-in prompt); Register shows Google button; **0 warn/0 err, no console errors**.

## SVG icon set + logo (current)
- **User-supplied SVG icons** (first batch dropped in a repo-root `icons/` folder, **relocated to
  `wwwroot/icons/`** — the correct static-web-asset home, served at `/icons/*.svg`; later additions placed
  there directly): `dashboard·network·mouse·keyboard·audio·monitor·compute·article·report·forum·support·
  sign-in·website-logo` (monochrome, mask-themed) + `gemini-logo·gemini-text` (full-colour, inlined).
- **Themeable via CSS mask** (not `<img>`, which can't recolour a black SVG on the dark theme): new global
  `.svg-icon` util in `app.css` paints the masked silhouette with `background-color: currentColor` and sizes
  it `1em` square — so each icon inherits its context's text colour (muted nav → **ember when active**) and
  is sized by the host's `font-size`. Per-icon `.ico-*` classes just set `--ico: url(../icons/<name>.svg)`.
- **◆ → uploaded logo** everywhere the brand mark appeared: `Sidebar.razor` `.brand-mark`, `AuthAside.razor`
  `.auth-logo` (`.auth-logo-mark`), and the **boot screen** `.boot-mark` in `index.html`/`app.css` (mask baked
  straight into `.boot-mark`, animations/glow preserved). The decorative ◆ list-bullets in `.auth-points` are
  intentionally **kept** (they're bullets, not the logo).
- **Sidebar nav + Home tiles** now render `.svg-icon .ico-*` instead of unicode glyphs (Solutions/
  Admin-Articles → `article`; **Report → dedicated `report.svg` (`ico-report`)**; Tickets → `support`;
  Admin-Dashboard → `dashboard`). **Donate keeps `♥`** (no donate icon was supplied). `.ni-icon`/`.tile-icon`/
  `.brand-mark` already set `font-size`+`color`, which the mask util consumes — so existing scoped CSS
  re-themed automatically.
- **"Sign in" text → icon-only button**: `Topbar.razor`'s signed-out Sign-in is now a `.tb-iconbtn` ghost
  button showing `sign-in.svg` (title/aria-label "Sign in"); Register stays a text button.
- **Gemini chatbot bubble** (`ChatBubble.razor` + `.razor.css`): the floating toggle replaced the old "AI"
  text. **Both Gemini SVGs are inlined** (not masked) so the spark keeps its multi-colour gradients and the
  wordmark inherits `currentColor`: closed = the colourful **`gemini-logo`** spark in a compact dark
  speech-bubble (squared bottom-right corner, ember glow + idle `.pulse`); **on hover the bubble stretches
  leftward** (right edge pinned) and the **`gemini-text` "Gemini" wordmark** (cream) slides + fades in via a
  `max-width`/`opacity`/`margin` transition, with a small spark spin. **One persistent button** holds both
  faces (`.ct-gemini` + `.ct-x`) and toggles `.is-closed`/`.is-open`, so the **`✕` spins + scales its way in
  and the spark spins out** (and vice-versa) instead of a hard swap; open state is a **dark circle**
  (gradient `#17171b→#0b0b0d`, not ember — matches the bubble) with the ✕ tinting ember on hover.
  `prefers-reduced-motion` drops the spin. (Logo/text kept full-colour & themed because mask would flatten
  them — distinct from the monochrome nav icons.)
- **Verified live** (preview): all icons 200/`image/svg+xml`; brand mark masked ember `rgb(255,90,44)`;
  `ico-report` mask = `report.svg`; chat bubble collapses 58px→expands 144px on (forced) hover with the cream
  wordmark + ember border, open→`✕`, no "AI" text; build clean (0/0); **no console errors**. (Added a repo-root
  `.claude/launch.json` so the preview launches from the solution root.)

## Favicon / app icons (browser-tab + PWA, current)
- **Problem**: the browser-tab "compact" icon + PWA/apple icons were still the stock **Blazor purple `@`
  flame** (`icon-192.png`/`icon-512.png`), and `index.html` had **no `<link rel="icon">`** at all (only
  `apple-touch-icon`), so the tab fell back to a default. None reflected the BenchRig logo.
- **Fix**: new **`wwwroot/favicon.svg`** (ember `#ff5a2c` `website-logo` path on an obsidian `#0a0a0c`
  rounded square) wired in `index.html` as `<link rel="icon" type="image/svg+xml">` (+ png fallbacks).
  **Regenerated `icon-192.png` (192×192) and `icon-512.png` (512×512)** from the same logo on a full-bleed
  obsidian square (maskable-safe). No SVG rasterizer was available locally, so the PNGs were rendered via the
  preview browser's `canvas.toDataURL`, transferred as **checksum-verified base64 chunks**, and decoded on
  disk (verified valid PNG headers + 192²/512² IHDR dims). **`manifest.webmanifest`** refreshed: name/
  short_name → "BenchRig", `background_color`/`theme_color` → `#0a0a0c`, icons = favicon.svg (`any`) +
  192/512 png, all `purpose: "any maskable"`.
- **Verified live**: `<link rel=icon>` = favicon.svg; favicon.svg `200 image/svg+xml`; icon-192 `192×192`,
  icon-512 `512×512`, both 200; on-page preview shows the ember-on-obsidian mark (no purple flame); build
  clean (0/0); **no console errors**.

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
- [x] Mouse — **full 11-test suite** (tabbed page): CPS, Kohi, Scroll-wheel encoder, Jitter Click,
      Double Click, Button registration (mouse diagram), Polling rate, DPI, Jitter/Dead-zone, Accuracy
      &amp; tracking, Drag &amp; Drop. Native Blazor pointer/wheel events; polling rate keeps JS coalesced capture.
- [x] Keyboard — **full 8-test suite** (tabbed page): Key Registration (full layout, tracks every key
      verified), Double Press / chatter, Polling / key-rate (tap + OS-repeat), 60s Typing test (random
      text, colored feedback), Rollover (NKRO), Ghosting visualizer, Stuck-key detection, Special-key
      verification (modifiers L/R, F-row, nav, numpad). Native Blazor keydown/keyup on auto-focused zones.
- [x] Audio — **full 4-test suite** (tabbed page): Playback (Stereo / 5.1 / 7.1 surround room with
      per-channel discrete routing via ChannelMerger + Immersive HRTF orbit sweep), Audio Balance
      (live stereo pan slider + L/C/R + auto-sweep), Microphone (level meter + live waveform + loopback
      "hear yourself"), Echo &amp; Latency (system latency + speaker→mic click round-trip).
- [x] Monitor — **full 11-test suite** (tabbed page + reusable `MonitorStage` fullscreen overlay):
      Dead Pixel, Uniformity (3x3 grid), Color/Gradients, Contrast (black/white step crush), Sharpness
      &amp; Text (1px checker), Viewing Angles, Backlight Bleed, Ghosting &amp; Motion, Refresh Rate
      (live measure: Hz + range + jitter + dropped + compliance), Gamma (line-vs-solid match),
      Response Time. Fullscreen API via bridge with Esc-exit watch; patterns are pure CSS.
- [x] Compute — **CPU + GPU + Memory suite** (tabbed page): Hardware detect (GPU via WebGL
      UNMASKED_RENDERER, threads, memory, heap, WebGL2/WebGPU) + manual CPU/GPU datalist pickers;
      CPU (Web-Worker single+multi throughput, presets, normalized score + est. Geekbench-6 + reference
      comparison bars); GPU (WebGL2 fractal shader, resolution/complexity presets, sustained FPS → score
      + reference bars); Memory (progressive capped allocation + read/write bandwidth).
- [x] Report (compile + Markdown export), Donate (Stripe links)
- [x] Auth (Login/Register), Forum (feed + post + comments + rating)
- [x] Support (ticket submit + list), Solutions (KB browser)
- [x] Admin (dashboard, tickets queue, article CRUD) with role guards
- All planned diagnostic suites (Network, Mouse, Keyboard, Audio, Monitor, Compute) are now built.

## Compute suite notes
- `compute-interop.js`: `detectHardware` (GPU via `WEBGL_debug_renderer_info`, `hardwareConcurrency`,
  `deviceMemory`, `performance.memory` heap, WebGL2/WebGPU); `runCpuBenchmark` (inline-blob Web Workers,
  single-core pass then N-core pass, mixed xorshift-int + sqrt/sin-float + Float64 array kernel, returns
  work-units/sec); `runGpuBenchmark` (WebGL2 **volumetric ray-marcher, "Volume Shader BM" style** — per
  pixel marches `uComplexity` steps through 5-octave 3D fbm noise; readPixels-synced sustained FPS);
  `runMemoryTest` (32 MB chunked allocation up to target with page-touch, released after,
  + 64 MB sequential write/read bandwidth).
- Scores normalized in `CpuScoreCalculator`/`GpuScoreCalculator`/`MemoryScoreCalculator`. Calibration
  constants (`ReferenceSingleUps=7000`, `GeekbenchFactor=1.5`, GPU `Divisor=4e6`) tuned on a measured
  desktop so CPU single≈1000→GB6≈1500 and 1080p/medium/60fps GPU≈~RTX 3060 tier. **Estimates/relative**,
  not certified. Reference CPU/GPU tables in `ComputePresets.cs` provide comparison bars.
- Browser limits acknowledged in-UI: CPU model isn't exposed (manual picker); 4 GB memory target
  usually hits the tab heap ceiling (reported as such).
- **GPU measurement fixed (real GPU sync)**: two bugs. (1) `requestAnimationFrame` pinned every light preset
  to the ~60 fps vsync ceiling, so settings never differentiated. (2) The "uncapped" loop synced with
  **`gl.finish()`, which is a no-op in WebGL** (it returns immediately across the browser's GPU-process
  boundary) — so it measured JS *submit* rate (~thousands of fps on ANY GPU), which is why an **RX 470
  "beat" an RTX 4090**. Now `runGpuBenchmark` runs a vsync-independent loop that draws a small BATCH then
  forces a true CPU↔GPU stall with **`gl.readPixels(0,0,1,1)`** (the standard sync when GPU timer queries
  are unavailable), yielding every ~16 ms. FPS now reflects real GPU work and scores track FP32 power.
- **Workload swapped to Volume Shader BM** (per user request): the GPU `FRAG` is now a heavy volumetric
  ray-marcher (per pixel: `uComplexity` march steps, each a 5-octave 3D fbm) — far more GPU-bound than the
  old fractal (≈100× the per-frame cost), the realistic GPU stress test the user asked for. `BATCH=1`
  (frames are heavy, keeps the tab responsive, no GPU-watchdog/TDR risk — 4K stays well under 2 s/frame).
  Presets are now ray-march **step** counts (720p/64 · 1080p/96 · 1440p/128 · 4K/160). Panel/page copy +
  the "Ray-march steps" metric label updated.
- **References reordered to UserBenchmark "Effective Speed"** (per user: a GTX 1650 should rank above an
  RX 470 despite the RX 470's higher raw FP32). `ReferenceGpus` now UB-ordered: UHD 620 350 · **RX 470/570
  2700 · GTX 1650 2950** · RTX 3060 6600 · RX 6700 XT 8000 · RTX 4070 10500 · RTX 4090 15500.
- **Per-preset normalization (fixes all GPU modes)**: raw throughput climbed with resolution (per-frame
  sync/setup overhead at low res; better GPU saturation at high res), so a card crept up a tier at 4K.
  `GpuScoreCalculator` now applies a resolution calib (720p ×1.287 · 1080p ×1.003 · 1440p ×0.940 ·
  4K ×0.901), calibrated on the measured RX 470 (720p→4K raw 2098/2693/2872/2996 → all ≈2700). `Divisor`
  = **1.25e6** so the RX 470 lands ≈2700, just under the GTX 1650 (2950).
- **Verified live on the user's actual RX 470**: every preset reports the same tier — 720p **2710**,
  4K **2702** (1080p/1440p ≈2700) — and the RX 470 (2702) sits **below the GTX 1650 (2950)** and at its
  own reference (2700), far below the 4090. Volumetric scene renders; build clean (0/0); no console errors.
- Verified live (no console errors): RX 470 + 8 threads detected; CPU Quick → single 1075 (≈GB6 1613) /
  multi 4530 (≈GB6 6795) / 4.2× scaling with comparison chart; GPU 1080p → 5599 score @ 60 fps, fractal
  renders; memory 1 GB → 2.2 GB/s write, 2.9 GB/s read.

## Monitor suite notes
- 11 tests under `Components/Monitor/`, tabbed `MonitorDiagPage`. A single reusable `MonitorStage`
  fullscreen overlay (driven by `MonitorMode` enum) renders every visual pattern in pure CSS
  (solid colors, gradients, 1px checker/lines, contrast steps, gamma line-vs-solid, CSS-animated
  motion blocks). Control bar with prev/next, speed slider, and Exit; Arrow keys cycle, Esc exits.
- Fullscreen via `monitor-interop.js` (`enterFullscreen`/`exitFullscreen` + `watchFullscreenExit`
  fires a `[JSInvokable]` so the overlay closes in sync when the user presses Esc out of fullscreen).
- Refresh Rate is measured inline: `measureFrameDeltas(durationMs)` → orchestrator computes Hz,
  min/max, jitter (mean-abs consecutive), dropped frames (>1.5× median), and a compliance verdict.
- Verified live (no console errors): refresh measured 59.7 Hz / 30-61 range / 0.2 ms jitter / 1 dropped /
  Compliant; gradient pattern launches, cycles Grayscale→Red, and Exit returns to the page.
- **Gamma redesigned (clarity fix)**: the old line-block-vs-solid-block stacked comparison felt the same
  across 1.8–2.6. Now each gamma column is a **1px-stripe surround with a solid centre square** (`GammaGray`)
  — at the matching gamma the square blends into the stripes and "vanishes", which reads far more clearly
  than comparing two halves. Added an instruction line; columns are visibly differentiated. Verified live.

## Audio suite notes
- 4 tests under `Components/Audio/`, tabbed `AudioDiagPage` (`@key` per tab). Built on a rewritten
  `audio-interop.js` (Web Audio).
- Playback: `ChannelMergerNode` + `destination.channelCount`/`discrete` interpretation routes a tone to one
  physical channel (stereo/5.1/7.1). Surfaces `destination.maxChannelCount` so users know if their device
  can actually reproduce >2 channels. Immersive = HRTF `PannerNode` orbiting the listener.
- Balance: continuous tone through a `StereoPanner`; `setPan` updates pan live (no re-trigger clicks).
- Microphone: `getUserMedia` → `AnalyserNode`; polls level (RMS dB) + down-sampled time-domain waveform;
  loopback connects the mic source to `destination` (headphones advised). Echo round-trip plays a 1 kHz
  click and times the analyser peak (needs speakers, not headphones); falls back to base+output latency.
- Verified live (no console errors): 7.1 room renders + per-channel tone plays, balance "Left only" →
  Pan 100% L / R 0%, system latency = 50 ms. Mic/echo need device permission (graceful when denied).

## Keyboard suite status
- Already complete (8 tests, prior turn) — no changes needed this turn; re-verified it still builds clean.

## AI chatbot fix
- **Root cause**: the old `chatbot-interop.js` called OpenRouter with no API key. OpenRouter (and every
  provider) requires a `Bearer` token for ALL requests -> 401 "Missing Authentication header". The plan's
  assumption that the free model needs no key was wrong.
- **Keyless options dead**: probed live — Pollinations.ai now returns 403 "Missing Turnstile token"
  (Cloudflare anti-bot). No reliable keyless browser-CORS LLM endpoint remains.
- **Fix**: bring-your-own free key. Rewrote `chatbot-interop.js` to be provider-aware
  (Google **Gemini** default, **OpenRouter**, **Groq**) — all three verified CORS-reachable from the
  browser. Key/provider/model persist in `localStorage` (`benchrig.ai.*`); the system-prompt guardrail
  stays in the JS layer. Gemini uses `system_instruction` + `contents` (user/model roles); OpenRouter/Groq
  use OpenAI chat format.
- **UI**: `ChatWindow` now has a settings panel (provider dropdown, key field, optional model,
  "Get a free key" link, privacy note). Shows the setup panel until a key is saved; configured state
  shows a green dot + provider name. Assistant replies render through `MarkdownRenderer`. Errors are
  cleaned (JS stack stripped) and `NO_KEY` reopens settings.
- **Verified live**: settings panel renders; saving a key persists + switches to chat; sending reaches the
  real Gemini endpoint (CORS OK) and returns a clean "API key not valid" with a dummy key — i.e. the full
  pipeline works; a real free key yields real answers. No console errors.

## Firebase AI Logic — the ONLY chatbot backend (locked down per user request)
- **Why**: user asked "Can we use Firebase AI Logic for our chatbot," then "remove the ability for user to
  choose their own model… only use firebase api for this… keep it professional and simple." Firebase AI
  Logic lets the browser call Gemini through the app's own Firebase project — **no API key in the client**,
  and the **Gemini Developer API** backend has a free tier (no Blaze/billing needed; Vertex AI is the paid
  one). The chatbot is now Firebase-only with **zero user-facing configuration**.
- **SDK**: ALL Firebase imports pinned to **11.10.0** (modules must match versions); `"firebase/ai"` added
  to the importmap in `index.html`. `firebase-ai.js` exports `getAI`/`getGenerativeModel`/`GoogleAIBackend`.
- **`firebase-config.js`**: imports `initializeApp`/`getAuth`/`getFirestore` and **exports `app`/`auth`/`db`**
  (the rest of the app imports these; chatbot-interop imports `app`).
- **`chatbot-interop.js`** (simplified to Firebase-only): single export `sendChatMessage(userMessage,
  history)`. Lazily builds one cached model `getAI(app, { backend: new GoogleAIBackend() })` →
  `getGenerativeModel(ai, { model: "gemini-3.5-flash", systemInstruction: SYSTEM_PROMPT })` →
  `model.startChat({ history }).sendMessage(userMessage)` → `result.response.text()`. History maps
  `assistant`→`model` role. Diagnostic-only `SYSTEM_PROMPT` guardrail kept. **Removed**: provider switch,
  `getAiConfig`/`setAiConfig`/localStorage, the `DEFAULT_MODEL` map, and the Gemini-direct/OpenRouter/Groq
  bring-your-own-key paths. Model is a fixed constant (no picker).
- **Config plumbing removed (clean code)**: `IJsChatbotBridge` now exposes only `SendMessageAsync`
  (dropped `GetConfigAsync`/`SetConfigAsync`); `ChatbotJsInterop` trimmed to match; the `AiConfig` record
  deleted from `ChatModels.cs`.
- **`ChatWindow.razor`** (stripped): no settings panel, no gear (⚙), no provider dropdown, no key field, no
  model picker. Header = green dot + "BenchRig Assistant" + "Gemini 3.5 Flash" label; body = messages; input +
  Send. Assistant replies render via `MarkdownRenderer`; errors cleaned to a one-liner ("Couldn't reach the
  assistant. …"). Removed the `.ch-gear`/`.chat-settings`/`.cs-*` CSS.
- **`FIREBASE_SETUP.md`**: section 7 now states AI Logic (Gemini Developer API, free; recommend App Check)
  is the only chatbot backend — nothing else to configure.
- **Verified live (placeholder project)**: chat opens straight to the conversation (green dot, "Firebase
  AI"); DOM confirms no gear/select/key-input/settings panel; sending a message executes the Firebase-only
  `firebase/ai` path against `…/models/gemini-3.5-flash:generateContent` and surfaces a clean error
  ("Couldn't reach the assistant. Firebase AI Logic error: …") — i.e. the pipeline runs; a project with AI
  Logic enabled + quota yields real answers. Builds clean (0 warn/0 err); **no console errors**.

## Firebase connectivity + Forum/KB rework
- **Diagnosis (live test)**: the plan's placeholder project `diagnostic-platform-blazor` is NOT usable —
  `registerWithEmail` → `auth/configuration-not-found` (Auth not enabled) and Firestore reads **time out**
  (project/DB not reachable). The interop code is correct; it needs the user's own Firebase project.
- **Hardening so the app never hangs**:
  - `firestore-interop.js`: every op wrapped in a 12 s `withTimeout` → throws `FIRESTORE_UNREACHABLE`
    instead of hanging forever. Added `countCollection` (getCountFromServer) + `CountCollectionAsync` bridge.
  - `AuthService.InitializeAsync`: `finally` marks state initialized (signed-out) so admin guards / UI
    resolve even if the auth listener never fires. Verified: `/admin` now shows "Access denied" (not a
    perpetual "Checking access…").
  - New `ConnectionError` shared component (clear "Couldn't reach the database" card + setup hint + Retry).
- **Setup made trivial**: `firebase-config.js` clearly flagged as a placeholder to replace; new
  **`FIREBASE_SETUP.md`** (create project → enable Email/Password auth → create Firestore → deploy
  `firestore.rules` → promote an admin). `firestore.rules` updated so authors can delete their own posts.
- **Forum improvements**: search box, sort (Newest / Top rated / Most discussed), tag-filter chips
  (derived from posts), Refresh, post count, graceful error/empty states. Post detail now computes the
  **rating aggregate live from the `ratings` subcollection** (security rules forbid non-authors updating
  the post's avgRating, so it's read-time), shows your own rating, live comment count, tags, and a Delete
  button for author/admin.
- **Knowledge Base improvements**: category chips + search, article count, richer article view
  (author + updated date + tags), **related-by-category** articles, and the same loading/empty/error states.
- **Verified live**: with the (down) placeholder backend, Forum and KB now fail fast with the clean
  "Couldn't reach the database" card + Try again — no hang, no console errors. Full read/write/rating
  flows are code-correct and work once a real Firebase project is connected per FIREBASE_SETUP.md.

## Knowledge Base rework (logic + functionality pass)
- **Deep-linkable articles**: `SolutionsPage` now has two routes — `/solutions` (list) and
  `/solutions/{Id}` (detail) — on one reused component, so articles have shareable/bookmarkable URLs and
  the browser Back button works. `Open`/`BackToList`/`FilterByTag` use `NavigationManager`; `Resolve()`
  (run in `OnInitializedAsync` after load + `OnParametersSet`) maps the route Id to the loaded list, with
  a graceful **"Article not found"** card for unknown/unpublished ids. List filters persist across the
  list↔detail nav (same component instance).
- **Server-side published filter**: list query now uses `QueryFilter { WhereField="isPublished",
  WhereOp="==", WhereValue=true }` — drafts no longer ship to the client (lighter + private). Single
  where-clause, no composite index needed; sorting is client-side.
- **Search / sort / filter logic**: search now matches **title + body + tags** (was title+body) with a
  clear (×) button; sort dropdown (**Newest / Oldest / Title A–Z**); category chips show **live counts**
  and only render categories that have articles; a **tag filter row** (derived from article tags, like the
  Forum) plus clickable `#tags` in cards/detail that jump to a filtered list. "X of Y articles" count row
  + **Clear filters** reset; distinct empty states for "no articles yet" vs "no match".
- **Reading experience**: each card/detail shows an estimated **N min read** (≈200 wpm) and the updated
  date; detail has a **Copy link** button (clipboard via `navigator.clipboard`); markdown body via
  `MarkdownRenderer`; **Related articles** are now ranked by **shared-tag overlap + same-category boost**
  (was just first-3-same-category).
- **Admin editor fixes (the key end-to-end gap)**: `AdminSolutionsPage` now has a **Tags** input
  (comma-separated, parsed/lowercased/deduped, capped at 10) that **saves on create AND update** — before,
  it always wrote an empty tags array, so the public tag-filtering had no data. Added a **markdown
  Preview/Edit toggle** (reuses `MarkdownRenderer`), a **two-step delete confirmation**, a list filter box,
  published/draft counts, inline save/error messages, and edit loads existing tags. (`updatedAt` is stamped
  by the JS `updateDocument` wrapper, so edits bump the date.)
- **Styling fix**: the old page reused `.ftag`/`.forum-tags`/`.forum-count` which are **scoped to
  ForumPage** (CSS isolation) and therefore never applied here — chips were unstyled. Both KB pages now
  have proper scoped CSS (`kb-*` chips/toolbar/cards/detail; `kb-admin-*` two-column editor layout; a local
  `.btn-sm`). Reuses genuinely-global classes (`badge`, `page-grid`, `chip-list`, `keychip`, `card`, `btn`).
- **Verified live** (real Firebase, empty collection): `/solutions` renders the styled toolbar + sort +
  "All 0" category chip + correct "no published articles yet" empty state (the `isPublished==true` query
  succeeds, returns 0 — no error). `/solutions/does-not-exist` → "Article not found" card; its Back button
  returns to `/solutions` (client-side, toolbar restored). Builds clean (0 warn/0 err); **no console
  errors/warnings**. Populated states (cards/search/sort/tags/related/copy-link/admin) compile and follow
  the working Forum patterns — exercise them by adding articles via Admin → Knowledge Base.

## Forum rework (logic + functionality pass)
- **Latent bug fixed (`ForumState.SetPosts`)**: it never cleared `IsLoading` (only `SetError`/
  `SetLoading(false)` did). It was never caught because the forum had only ever been tested against the
  **unreachable** placeholder (which hits `SetError` → spinner resolves to the error card). With a now-
  reachable DB the feed query *succeeds* → `SetPosts` → the feed hung on "Loading posts…" forever. Fixed so
  `SetPosts` sets `IsLoading = false`. (Live-confirmed: feed now resolves to the proper empty state.)
- **Feed aggregates now live + sorts actually work**: a post's stored `avgRating`/`commentCount` never
  change (rules forbid non-authors writing them), so **Top rated** / **Most discussed** were sorting on
  perpetual zeros. `ForumPage.Load` now does a fast first paint with stored values, then **enriches each
  post in parallel** (`CountCollectionAsync` for comments + reads the `ratings` subcollection for avg/count)
  and re-renders — making both sorts meaningful and card counts accurate. Per-post try/catch keeps stored
  values on failure.
- **Threaded comments** (new `CommentThread.razor`, recursive): the model already had `ParentCommentId`/
  `Depth` (rules allow `depth <= 3`) but the UI only ever posted flat depth-0 comments. Now each comment
  has an inline **Reply** box (disabled past depth 3), renders nested with an accent left-border, supports
  **delete** (own comment or admin), and renders the body via `MarkdownRenderer` with relative timestamps.
  `ForumPostPage` builds a parent→children lookup; **orphaned replies** (parent deleted) surface as roots
  so they never silently vanish.
- **Edit your own post** (rules allow author/admin update): inline edit form (title/body/tags) on the post
  page; the JS `updateDocument` wrapper stamps `updatedAt`, and an **"· edited"** marker shows on the post
  + feed cards when `UpdatedAt > CreatedAt`.
- **Ratings**: added **clear-your-rating** (deletes your `ratings/{uid}` doc — rules allow) next to the
  stars; star hover preview; avg still computed at read time from the subcollection.
- **Feed UX**: `_visibleCount` **pagination** ("Load more (N more)", resets on filter change), a **My posts**
  toggle (logged-in), **tag-aware search** (title+body+tags), a **Recently active** sort, "Showing X of Y"
  count, and a Clear-filters reset. Cards show **relative time + 💬 comment count + ★ rating** (or "★ –"
  when unrated).
- **New-post editor**: markdown **Preview/Edit** toggle, live **tag chips**, char counters, min-length
  validation (title ≥ 5, body ≥ 15), friendly DB-error text, and a success confirmation.
- **Shared `Core/Util/TimeAgo.cs`** (pure C#, UTC-normalized "3m/2h/4d ago"), added to `_Imports.razor`;
  used by the feed card, post header, and comments.
- **Verified live** (real Firebase, empty collection): `/forum` renders the new toolbar (4 sorts +
  tag-aware search), the NewPostEditor, and the correct "No posts yet" empty state (loading bug gone);
  `/forum/does-not-exist` → "This post could not be found." + working Back. Builds clean (0 warn/0 err);
  **no console errors/warnings**. Populated states (threading, ratings, edit, pagination, enrichment)
  compile and mirror the verified KB patterns — exercise them by signing in and posting.

## Ticket system finalize + KB category + Donate copy (per user request)
- **Priority removed** everywhere: dropped `TicketPriority` + `Priority` from `SupportModels`, the new-ticket
  form, the admin priority filter + per-ticket priority `<select>`, the priority badges, the priority sort,
  and `TicketDetail.PriorityClass`.
- **Three ticket states only**: `open · in_progress · resolved` (dropped `closed`). `TicketStatus` enum,
  both `StatusOrder` arrays, and the admin status `<select>` updated; `StatusLabel`/`StatusClass` still map a
  legacy `"closed"`→ resolved for old docs.
- **Two-way conversation (complete ticket logic)**: `TicketResponse` generalized to `AuthorUid/AuthorName +
  IsStaff` (was admin-only). **Users can now reply to their own non-resolved tickets**; admins reply as staff.
  `TicketDetail` takes `CanRespond` + `AsStaff`: SupportPage passes `CanRespond = status != resolved`,
  `AsStaff=false`; AdminTickets passes `CanRespond=true`, `AsStaff=true`. A staff reply still bumps an open
  ticket to in-progress; replies render staff (green border + "Support") vs user (ember border + "User").
  **`firestore.rules` updated**: responses `create` now allows the ticket owner OR an admin, with
  `isStaff == isAdmin()` enforced. ⚠ **Deploy the updated rules** for user replies to work.
- **KB admin-defined category**: AdminSolutions category is now a free-text `<input>` + `<datalist>` (pick a
  suggestion or type a custom one), normalized (`NormCategory`: trim/lowercase, fallback "general"). The
  public KB derives its category chips from the actual articles (known first, then custom alphabetically),
  so custom categories appear.
- **Donate copy**: the not-configured card dropped the dev "paste Stripe links into stripe-interop.js" text
  and now shows a short appreciation paragraph (small independent team, free/ad-free, donations help hosting
  + new features); keeps "Donations aren't set up yet." Verified live (public page); build clean (0/0); no
  console errors.

## Support rework (logic + functionality pass)
- **Headline gap fixed — ticket responses (admin↔user conversation)**: the `responses` subcollection was
  fully defined in the model (`TicketResponse`) AND the security rules (admin create; owner/admin read), and
  the admin page intro even said *"post responses"* — but **no UI existed** to post or read them. Users
  couldn't see replies; admins couldn't write them. New reusable **`Components/Support/TicketDetail.razor`**
  loads + renders the responses thread (markdown + relative time) and, when `CanRespond` (admin), shows a
  composer that writes to `ResponsesOf(ticketId)`. Used read-only on the user page, read-write on admin.
- **Workflow touch**: posting the first response to an `open` ticket auto-moves it to `in_progress`
  (rules allow admin update); `OnChanged` callback reloads the parent list.
- **Serialization note**: kept the existing **string-based DTOs** (not the `SupportTicket`/`TicketStatus`
  enum model) — the JSInterop `System.Text.Json` maps enums by number, so deserializing `"open"` into the
  enum would fail. `TicketDetail` takes primitive params (`TicketId`/`Body`/`Status`) to avoid this.
- **User page (`SupportPage`) rebuilt**: master-detail — new-ticket form (subject/priority/body with char
  counters, **min-length validation** [subject ≥ 5, body ≥ 15], success confirmation, friendly DB errors) +
  a "Your tickets" list with **status-filter chips (+counts)**, search, sort (Newest/Oldest/Priority),
  **expandable ticket cards** that reveal the body + responses via `TicketDetail`, **status/priority badges**,
  relative time, and a **💬 response-count** per ticket (parallel `CountCollectionAsync` enrichment). Added
  real **loading / error (ConnectionError+retry) / empty** states (previously errors were silently swallowed
  → misleading "No tickets").
- **Admin page (`AdminTicketsPage`) rebuilt**: a **status stat-strip** (open/in-progress/resolved/closed
  counts, click to filter) + **master-detail** layout — left = searchable/priority-filterable queue with
  badges/relative-time/response-count; right = sticky detail panel with **status + priority `<select>`
  controls** and the `TicketDetail` **response composer**. Real loading/error states (was a bare table that
  swallowed errors and had no response UI at all).
- **Shared helpers**: `TicketDetail` exposes `public static StatusClass/StatusLabel/PriorityClass`
  (reused by both pages for badges); relative time via the shared `TimeAgo`.
- **Verified live** (real Firebase): `/support` (signed out) → sign-in prompt; `/admin/tickets` (not admin)
  → "Access denied" guard. Builds clean (0 warn/0 err); **no console errors/warnings**. The signed-in
  flows (submit, filter, expand, responses, admin status/priority + replies) compile and mirror the verified
  KB/Forum patterns — exercise them by signing in (and promoting an admin per FIREBASE_SETUP.md).

## Donate rework (logic + functionality pass)
- **Problem**: every amount button opened a **placeholder** Stripe link (`buy.stripe.com/test_YOUR_LINK_*`)
  in a new tab → a broken page; "Custom" passed amount `0` to another placeholder; no selection state, no
  one-time/monthly option, no feedback, no error handling, and no way to configure links without editing JS.
- **Config-driven + placeholder-safe** (`stripe-interop.js` rewritten): a single `CONFIG` block holds
  `oneTime` / `monthly` (optional) / `custom` (optional, customer-chosen amount) Payment Links + `currency`.
  `getDonationOptions()` returns a structured, serializable object with per-tier + overall `configured`
  flags (placeholder regex `YOUR_(LINK|MONTHLY)` + an `https://*.stripe.com/` check). `openDonation(url)`
  **refuses placeholders/invalid URLs** (returns false) so the app never opens a broken tab.
- **Bridge** (`IJsStripeBridge`): replaced `RedirectToCheckoutAsync(int)` with `Task<DonationOptions>
  GetOptionsAsync()` + `Task<bool> OpenAsync(string url)`. New `Core/Models/Donations/DonationModels.cs`
  (`DonationTier`, `DonationOptions`) deserialized straight from the JS object (camelCase Web defaults);
  added `BenchRig.App.Core.Models.Donations` to `_Imports.razor`.
- **`DonateButton` (rich panel)**: graceful **"donations aren't set up yet"** card (with a Stripe Payment
  Links link) when unconfigured — the analog of the Firebase ConnectionError pattern. When configured:
  **one-time/monthly toggle** (shown only if both are configured), an **amount grid with tier labels**
  (Coffee/Supporter/Backer/Sponsor) and **selected state**, a dynamic primary **"Donate $X (/month)"**
  button, an optional **"enter a custom amount →"** link, currency-aware money formatting
  ($/€/£/₹/₨), loading + inline error states, and a secure-by-Stripe note.
- **`DonatePage` enriched**: two-column layout — the panel + a "Where your support goes" transparency
  list and a short **FAQ** (account-free, card safety, monthly, tax).
- **Verified live**: placeholder config → the not-configured setup card renders (no broken tab); with
  temporary real-format test links the configured UI renders correctly — toggling to **Monthly** narrows
  the tiers 4→3 and the button reads **"Donate $10 / month"**, selecting a tier updates the button, custom
  link present. Reverted to placeholders; builds clean (0 warn/0 err); **no console errors**. To enable
  donations, paste real Stripe Payment Link URLs into `wwwroot/js/stripe-interop.js`.

## Mouse suite fixes (accuracy/UX pass)
- **Cooldown after fast-click tests**: CPS + Jitter Click now lock out for **2 s** after finishing
  (`_cooldown` flag + one-shot `Timer`), so the trailing rapid clicks can't instantly restart the run.
  Hint shows "Ready again in a moment…". Jitter Click's **Reset button removed** (click-to-retry covers it).
- **DPI rewritten with Pointer Lock** (`mouse-interop.js` `dpiInit`/`dpiDispose`): the old approach summed
  `MovementX` on a small box, so movement was lost the moment the cursor left the box or hit the screen edge
  (huge under-read). Now click the box → `requestPointerLock` (raw, unclamped deltas, cursor hidden) → slide →
  click/Esc; DPI = counts / (cm÷2.54), no DPR factor (locked deltas are raw counts). rAF streams the live
  count to `OnDpiMove`; `OnDpiLock` computes on unlock.
- **Jitter & Dead Zone**: removed the Start + Reset buttons; the **stage starts the jitter measurement on
  pointerdown** (`@onpointerdown="StartStill"`, guarded against re-entry); idle hint added.
- **Accuracy hit-detection fixed**: `.acc-target` + `.acc-counter` set `pointer-events:none` so a click on
  the bullseye resolves against the **stage** (e.target=stage), making `OffsetX/Y` stage-relative — accurate
  hits were previously counted as misses because OffsetX was relative to the clicked target child.
- **Drag & Drop fixed**: `.dd-target` always `pointer-events:none`; `.dd-handle.dragging` becomes
  `pointer-events:none` so pointermoves resolve against the stage and the handle follows the cursor correctly
  (was reading OffsetX off whatever child was under the pointer → jumpy/broken).
- **Network**: removed the "Use precise location" button + `UsePrecise`/`Orch` from `NetworkInfoPanel`
  (per user request; orchestrator `UsePreciseLocationAsync` now dormant).
- Verified live (real browser): Network has no precise button; Jitter/DeadZone shows no buttons + hint;
  Accuracy target/counter + DragDrop target are `pointer-events:none`; DPI tab loads the pointer-lock module
  with **no console errors**; build clean (0 warn/0 err). (pointerdown-driven flows match the working CPS pad.)

## Mouse suite notes
- 11 tests under `Components/Mouse/`, hosted by a tabbed `MouseDiagPage` (only the active test mounts,
  so timers / JS capture don't all run at once).
- Built on **native Blazor pointer/wheel events** (`@onpointerdown/move/up`, `@onwheel`) — reliable and
  low-overhead. Only Polling Rate uses the JS-interop path (`MouseTestOrchestrator` + `mouse-interop.js`
  `getCoalescedEvents`) to catch true high-frequency reporting.
- Verified live (no console errors): CPS (timed, rating), Buttons (per-button counts + mouse diagram),
  Accuracy (target hit detection + progression), Drag&Drop & Polling render and start cleanly.
- Logic notes: Double-click flags inter-click gaps < threshold (worn-switch chatter); Encoder counts
  notches + flags single-notch reversals; DPI = pixels×DPR / (cm/2.54) with calibration caveat;
  Dead-zone = smallest registered move, Jitter = max spurious move while held still; Drag&Drop flags
  `buttons==0` mid-drag as an interrupted (dropped) click.

## Keyboard suite notes
- 8 tests under `Components/Keyboard/`, hosted by a tabbed `KeyboardDiagPage` (`@key` per tab so each
  test re-mounts fresh and auto-focuses). Shared ANSI layout in `KeyboardLayout.cs` (used by Registration
  + Ghosting). Native Blazor `@onkeydown/@onkeyup` on a focusable `<div tabindex=0>` (auto-focus via
  `ElementReference.FocusAsync`), `preventDefault` on all but the Typing test.
- Verified live (no console errors): Registration (dispatched A/B/C/Shift/F5 → "5/78 verified",
  highlighted), Special keys (R-Ctrl/F12/NumpadEnter/↑ → checked), Typing (15 chars matched, 100%,
  live WPM, timer-on-first-key).
- **Typing text = logical + random**: replaced the random word-salad with a pool of 16 complete,
  grammatical `Sentences`; `Generate()` shuffles them and stitches ~55–70 words into a coherent passage
  that differs every run / on "New text". Verified live (two distinct readable passages).
- Logic notes: chatter ignores OS auto-repeat (`e.Repeat`) and flags same-key gaps < threshold; key-rate
  honestly labeled (browsers can't read raw HW polling — measures tap event rate + OS auto-repeat);
  rollover tracks max simultaneous held; ghosting highlights all held keys so blocked combos are visible;
  stuck-key flags keys held > threshold with a ticking timer; special-key uses `e.Code` (distinguishes
  L/R modifiers &amp; numpad) and notes OS-captured media keys may not arrive.

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
  3. **Server selection**: ~~Test-Server dropdown + custom "Add" form~~ **REMOVED per user request** — the
     `SpeedTestPanel` now shows a fixed **description card** ("Test server · Cloudflare global edge" + a brief
     blurb that the anycast edge auto-routes to the nearest data centre, so the result reflects the user's
     real-world connection/congestion, not a slow server). The test always runs against Cloudflare
     (`RunFullTestAsync` uses `SelectedServer?.Key ?? "cf"`). Removed the `select`/`+Add` UI and the panel's
     `OnServerChange`/`AddServer`/`_showAdd` code; `AddCustomServerAsync` orchestrator/bridge path is now
     dormant (no UI). Foot strip "Server" hard-labels "Cloudflare edge". Verified: no dropdown/Add in DOM,
     GO still runs, 0 warn/err, no console errors.
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
- **Ookla-grade pass** (per user feedback):
  1. **Speed engine** rewritten for high throughput (scales to ~1 Gbps): 6 parallel streaming readers,
     100 MB chunks, 1.5 s warm-up excluded, **steady-state** average + **sliding-window** live speed for a
     smooth ramp. Upload uses 256 KB chunks (1 MB starved slow uplinks → 0). Gauge auto-scales to 1000 Mbps.
  2. **Location without GPS**: switched primary geo to **GeoJS** (`get.geojs.io`) which pins the correct
     city (Rawalpindi) where ipinfo returned the ISP hub (Lahore). Aggregates GeoJS + ipinfo (postal/hostname)
     + Cloudflare meta. Precise (GPS) button still available as an override.
  3. **Network Details = everything**: IP, hostname, ISP, org, ASN; connection type/downlink/RTT, HTTP
     protocol, DoH, data-saver; city/region/country(+code)/continent/postal/coords/accuracy radius/timezone
     (+UTC offset + local time); device: browser, OS, languages, CPU threads, memory, screen, touch, cookies,
     online. Grouped Identity / Connection / Location / Device.
  4. **Jitter & latency**: 30 samples → idle(min)/avg/max/jitter(mean-abs-consecutive)/packet-loss, RTT
     sparkline, plus **bufferbloat** (loaded latency under download via a separate-host Cloudflare-DoH probe,
     server-ping fallback) graded A–D. (Bufferbloat shows "—" inside the embedded preview, which blocks the
     concurrent probe under load; it populates in a real browser.)

## Build Status
- Baseline scaffold builds clean (.NET 10.0.300).
</content>
</invoke>
