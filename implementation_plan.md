# BenchRig Diagnostic Platform — System Architecture Blueprint

> **Version**: 1.0 · **Target Runtime**: .NET 10.0 · **SDK**: Blazor WebAssembly (Standalone PWA)
> **Data Layer**: Firebase Web SDK v10+ via JS Interop · **Author**: Architectural Design Document

---

## 1. Foundational Architectural Decisions

### 1.1 Why a Single-Project Clean Architecture?

Blazor WASM ships the **entire assembly** to the browser. Multi-project solutions (`*.Core`, `*.Infrastructure`) create additional DLLs that increase download size linearly. Instead, we enforce Clean Architecture through **namespace-bounded folders** inside a single `BenchRig.App` project, achieving:

- **Trim-friendly**: Single assembly = better IL trimming & tree-shaking
- **Faster cold-start**: Fewer DLLs to download & initialize in the browser
- **Dependency discipline**: Enforced via code review + analyzers, not assembly boundaries

> [!IMPORTANT]
> All external I/O (Firebase, Browser APIs) flows through `Services/JsInterop/` bridges. Domain logic in `Core/` **never** references `IJSRuntime` directly.

### 1.2 Firebase Integration Strategy

```
┌─────────────────────────────────────────────────────┐
│                  Blazor WASM (.NET)                  │
│                                                     │
│  C# Services ──► IJSRuntime.InvokeAsync ──────┐     │
│                                                │     │
└────────────────────────────────────────────────┼─────┘
                                                 │
                                                 ▼
┌─────────────────────────────────────────────────────┐
│            wwwroot/js/ (JavaScript Layer)            │
│                                                     │
│  firebase-interop.js ──► Firebase SDK v10 (ESM)     │
│  diagnostic-interop.js ──► Web APIs (Canvas, Audio) │
│  worker-interop.js ──► Web Workers (CPU bench)      │
└─────────────────────────────────────────────────────┘
```

Every Firebase call is **proxied** through a thin JS module that:
1. Receives serialized arguments from C# via `IJSRuntime`
2. Calls the Firebase SDK method
3. Returns serialized results or pushes updates via `DotNetObjectReference` callbacks

---

## 2. Directory & Solution Architecture Layout

```
BenchRig/
├── .github/
│   └── workflows/
│       ├── ci.yml                          # Build + lint on every PR
│       └── deploy-firebase.yml             # Deploy to Firebase Hosting
│
├── BenchRig.slnx                           # Solution file
│
└── BenchRig.App/                           # ══════ SINGLE WASM PROJECT ══════
    ├── BenchRig.App.csproj
    ├── Program.cs                          # DI bootstrap & service registration
    ├── App.razor                           # Router root
    ├── _Imports.razor                      # Global @using directives
    │
    ├── Core/                               # ══════ DOMAIN LAYER ══════
    │   │                                   # (No JS/browser dependencies)
    │   ├── Models/
    │   │   ├── Network/
    │   │   │   ├── SpeedTestResult.cs      # Download/Upload Mbps, timestamp
    │   │   │   ├── LatencyResult.cs        # Ping ms, jitter ms, packet index
    │   │   │   ├── StabilityResult.cs      # Packet loss %, dropped frames
    │   │   │   ├── StressTestResult.cs     # Concurrent streams, throughput
    │   │   │   ├── NetworkInfo.cs          # Public IP, ISP, geo-coords
    │   │   │   └── DiagnosticServer.cs     # Server URL, label, region, lat/lng
    │   │   │
    │   │   ├── Mouse/
    │   │   │   ├── ClickTestResult.cs      # CPS, total clicks, duration
    │   │   │   ├── JitterClickResult.cs    # Jitter CPS, irregularity score
    │   │   │   ├── DoubleClickResult.cs    # Interval ms, anomaly flag
    │   │   │   ├── ScrollTestResult.cs     # Steps up/down, consistency %
    │   │   │   ├── ButtonState.cs          # Button enum + pressed/released
    │   │   │   ├── PollingRateResult.cs    # Hz, sample count, deviation
    │   │   │   ├── DpiEstimate.cs          # Estimated DPI, distance px
    │   │   │   ├── DeadZoneResult.cs       # Dead zone radius px, jitter px
    │   │   │   ├── AccuracyResult.cs       # Hit %, miss %, avg deviation px
    │   │   │   └── DragDropResult.cs       # Drag integrity bool, drop offset
    │   │   │
    │   │   ├── Keyboard/
    │   │   │   ├── KeyRegistration.cs      # Key code, name, pressed timestamp
    │   │   │   ├── ChatterResult.cs        # Key, bounce count, interval ms
    │   │   │   ├── KeyPollingRate.cs       # Hz, sample window
    │   │   │   ├── TypingTestResult.cs     # WPM, accuracy %, raw WPM
    │   │   │   ├── RolloverResult.cs       # Max simultaneous keys, ghosted[]
    │   │   │   └── StuckKeyEvent.cs        # Key, stuck duration ms
    │   │   │
    │   │   ├── Audio/
    │   │   │   ├── PlaybackChannel.cs      # Channel enum (Stereo/5.1/7.1/Spatial)
    │   │   │   ├── BalanceResult.cs        # Channel, frequency Hz, volume dB
    │   │   │   ├── MicLoopbackState.cs     # IsCapturing, latency ms, level dB
    │   │   │   └── EchoLatencyResult.cs    # Round-trip ms, confidence
    │   │   │
    │   │   ├── Monitor/
    │   │   │   ├── DeadPixelConfig.cs      # Color to display, fullscreen flag
    │   │   │   ├── ColorGradient.cs        # Gradient type enum, stops[]
    │   │   │   ├── GhostingConfig.cs       # Object speed, contrast level
    │   │   │   ├── RefreshRateResult.cs    # Measured Hz, expected Hz, delta
    │   │   │   ├── GammaTestConfig.cs      # Target gamma, test pattern type
    │   │   │   └── ResponseTimeResult.cs   # Grey-to-grey ms, overshoot %
    │   │   │
    │   │   ├── Compute/
    │   │   │   ├── CpuBenchmarkResult.cs   # Score, threads used, duration ms
    │   │   │   ├── GpuBenchmarkResult.cs   # FPS, shader complexity, VRAM est.
    │   │   │   └── BenchmarkSuite.cs       # Aggregate of CPU + GPU results
    │   │   │
    │   │   ├── Report/
    │   │   │   ├── DiagnosticReport.cs     # Master report: all sub-results
    │   │   │   ├── ReportSection.cs        # Section enum + metrics list
    │   │   │   └── ReportExportFormat.cs   # JSON, PDF, Markdown enum
    │   │   │
    │   │   ├── Auth/
    │   │   │   ├── UserProfile.cs          # UID, email, displayName, role
    │   │   │   └── UserRole.cs             # Enum: Anonymous, User, Admin
    │   │   │
    │   │   ├── Forum/
    │   │   │   ├── ForumPost.cs            # Title, body, authorUid, tags[]
    │   │   │   ├── ForumComment.cs         # Body, authorUid, parentId
    │   │   │   └── PostRating.cs           # Stars 1-5, userId
    │   │   │
    │   │   ├── Support/
    │   │   │   ├── SupportTicket.cs        # Subject, body, status, priority
    │   │   │   ├── TicketStatus.cs         # Enum: Open, InProgress, Resolved, Closed
    │   │   │   └── TicketResponse.cs       # Admin response body, timestamp
    │   │   │
    │   │   └── Solutions/
    │   │       ├── SolutionArticle.cs      # Title, markdown body, category
    │   │       └── ArticleCategory.cs      # Enum: Network, Mouse, Keyboard, etc.
    │   │
    │   ├── Engines/                        # Pure C# computation — NO I/O
    │   │   ├── Network/
    │   │   │   ├── SpeedCalculator.cs      # Mbps from bytes + elapsed time
    │   │   │   ├── JitterCalculator.cs     # Statistical jitter from ping array
    │   │   │   ├── PacketLossCalculator.cs # Loss % from sent vs received
    │   │   │   └── StressAnalyzer.cs       # Throughput aggregation
    │   │   │
    │   │   ├── Mouse/
    │   │   │   ├── CpsCalculator.cs        # Clicks / time window
    │   │   │   ├── PollingRateAnalyzer.cs  # Hz from event timestamp deltas
    │   │   │   ├── DpiEstimator.cs         # DPI = pixels / physical inches
    │   │   │   └── JitterAnalyzer.cs       # Std-dev of movement deltas
    │   │   │
    │   │   ├── Keyboard/
    │   │   │   ├── ChatterDetector.cs      # Bounce detection < threshold ms
    │   │   │   ├── WpmCalculator.cs        # (correct chars / 5) / minutes
    │   │   │   ├── RolloverAnalyzer.cs     # Max concurrent key tracking
    │   │   │   └── PollingRateAnalyzer.cs  # Hz from keydown timestamp deltas
    │   │   │
    │   │   ├── Audio/
    │   │   │   └── LatencyCalculator.cs    # Round-trip time from echo offset
    │   │   │
    │   │   ├── Monitor/
    │   │   │   └── RefreshRateCalculator.cs # Hz from requestAnimationFrame deltas
    │   │   │
    │   │   ├── Compute/
    │   │   │   ├── CpuScoreCalculator.cs   # Normalize ops/sec into a score
    │   │   │   └── GpuScoreCalculator.cs   # Normalize avg FPS into a score
    │   │   │
    │   │   └── Report/
    │   │       └── ReportBuilder.cs        # Compiles all results → DiagnosticReport
    │   │
    │   ├── Interfaces/                     # Abstractions consumed by Engines
    │   │   ├── IJsNetworkBridge.cs         # Speed test, ping, fetch streams
    │   │   ├── IJsMouseBridge.cs           # Pointer events, scroll, DPI
    │   │   ├── IJsKeyboardBridge.cs        # Key events, polling capture
    │   │   ├── IJsAudioBridge.cs           # Web Audio API, MediaDevices
    │   │   ├── IJsMonitorBridge.cs         # Canvas, rAF, fullscreen
    │   │   ├── IJsComputeBridge.cs         # Web Workers, WebGL
    │   │   ├── IJsFirebaseAuthBridge.cs    # Auth: sign-in, sign-out, onAuthStateChanged
    │   │   ├── IJsFirestoreBridge.cs       # Firestore: CRUD, real-time listeners
    │   │   ├── IJsGeoLocationBridge.cs     # Geo IP lookup
    │   │   └── IJsStripeBridge.cs          # Stripe checkout session
    │   │
    │   └── Constants/
    │       ├── DiagnosticDefaults.cs       # Default thresholds, durations
    │       ├── FirestoreCollections.cs     # Collection name constants
    │       └── AppRoutes.cs               # Route string constants
    │
    ├── Services/                           # ══════ APPLICATION LAYER ══════
    │   │                                   # (Bridges, state, orchestration)
    │   │
    │   ├── JsInterop/                      # C# ↔ JavaScript bridge implementations
    │   │   ├── NetworkJsInterop.cs         # implements IJsNetworkBridge
    │   │   ├── MouseJsInterop.cs           # implements IJsMouseBridge
    │   │   ├── KeyboardJsInterop.cs        # implements IJsKeyboardBridge
    │   │   ├── AudioJsInterop.cs           # implements IJsAudioBridge
    │   │   ├── MonitorJsInterop.cs         # implements IJsMonitorBridge
    │   │   ├── ComputeJsInterop.cs         # implements IJsComputeBridge
    │   │   ├── FirebaseAuthJsInterop.cs    # implements IJsFirebaseAuthBridge
    │   │   ├── FirestoreJsInterop.cs       # implements IJsFirestoreBridge
    │   │   ├── GeoLocationJsInterop.cs     # implements IJsGeoLocationBridge
    │   │   └── StripeJsInterop.cs          # implements IJsStripeBridge
    │   │
    │   ├── State/                          # Blazor State Containers
    │   │   ├── Base/
    │   │   │   └── StateContainerBase.cs   # Abstract base: OnChange + NotifyStateChanged
    │   │   │
    │   │   ├── AuthStateContainer.cs       # Current user, role, isLoggedIn
    │   │   ├── NetworkDiagState.cs         # Live speed, latency, stability
    │   │   ├── MouseDiagState.cs           # Live CPS, polling, DPI
    │   │   ├── KeyboardDiagState.cs        # Key matrix, WPM, chatter
    │   │   ├── AudioDiagState.cs           # Playback channel, mic level
    │   │   ├── MonitorDiagState.cs         # Refresh rate, test mode
    │   │   ├── ComputeDiagState.cs         # CPU/GPU score, progress %
    │   │   ├── ReportState.cs             # Compiled report, export status
    │   │   ├── ForumState.cs              # Posts list, selected post
    │   │   ├── TicketState.cs             # User tickets, admin queue
    │   │   └── ChatbotState.cs            # Message history, isOpen
    │   │
    │   ├── Orchestrators/                  # High-level workflow coordinators
    │   │   ├── NetworkTestOrchestrator.cs  # Sequences: speed → latency → stability
    │   │   ├── MouseTestOrchestrator.cs    # Runs CPS → polling → DPI pipeline
    │   │   ├── KeyboardTestOrchestrator.cs # Runs key reg → chatter → typing
    │   │   ├── AudioTestOrchestrator.cs    # Runs playback → balance → mic
    │   │   ├── MonitorTestOrchestrator.cs  # Runs dead pixel → gradient → refresh
    │   │   ├── ComputeTestOrchestrator.cs  # Runs CPU → GPU → score
    │   │   └── ReportOrchestrator.cs       # Gathers all results → builds report
    │   │
    │   └── Auth/
    │       ├── AuthService.cs             # Login/logout/register flows
    │       └── AdminGuardService.cs       # Role-check for admin operations
    │
    ├── Components/                         # ══════ PRESENTATION LAYER ══════
    │   │
    │   ├── Layout/
    │   │   ├── MainLayout.razor           # Shell: sidebar + topbar + @Body
    │   │   ├── MainLayout.razor.css       # Scoped styles
    │   │   ├── Sidebar.razor              # Navigation rail
    │   │   ├── Topbar.razor               # User avatar, theme toggle
    │   │   └── FooterBar.razor            # Copyright, links
    │   │
    │   ├── Shared/                         # Reusable UI atoms
    │   │   ├── MetricCard.razor           # Animated stat card (value + label)
    │   │   ├── GaugeChart.razor           # SVG radial gauge
    │   │   ├── ProgressRing.razor         # Circular progress indicator
    │   │   ├── LiveGraph.razor            # Real-time line/bar chart (Canvas)
    │   │   ├── StatusBadge.razor          # Pass/Fail/Warning pill
    │   │   ├── FullscreenOverlay.razor    # For monitor tests
    │   │   ├── ConfirmDialog.razor        # Modal confirmation
    │   │   ├── ToastNotification.razor    # Non-blocking notification
    │   │   ├── MarkdownRenderer.razor     # Render markdown content
    │   │   └── LoadingSpinner.razor       # Skeleton / spinner
    │   │
    │   ├── Auth/
    │   │   ├── LoginPanel.razor           # Email/password + Google sign-in
    │   │   ├── RegisterPanel.razor        # New account form
    │   │   └── UserAvatar.razor           # Profile dropdown
    │   │
    │   ├── Network/
    │   │   ├── SpeedTestPanel.razor        # Download/upload gauges + start btn
    │   │   ├── LatencyPanel.razor          # Ping table + jitter graph
    │   │   ├── StabilityPanel.razor        # Packet loss timeline
    │   │   ├── StressTestPanel.razor       # Concurrent stream dashboard
    │   │   ├── NetworkInfoPanel.razor      # IP, ISP, geo display
    │   │   ├── ServerMapPanel.razor        # LeafletJS map + server pins
    │   │   └── ServerSelector.razor        # Multi-server dropdown
    │   │
    │   ├── Mouse/
    │   │   ├── CpsTestPanel.razor          # Click area + CPS counter
    │   │   ├── JitterClickPanel.razor      # Jitter-specific click area
    │   │   ├── DoubleClickPanel.razor      # Double-click anomaly detector
    │   │   ├── ScrollTestPanel.razor       # Scroll wheel tracker
    │   │   ├── ButtonTestPanel.razor       # 5-button state visualizer
    │   │   ├── PollingRatePanel.razor      # Hz display + event log
    │   │   ├── DpiCheckPanel.razor         # Ruler-based DPI estimator
    │   │   ├── DeadZonePanel.razor         # Crosshair tracking canvas
    │   │   ├── AccuracyTestPanel.razor     # Target-hit canvas game
    │   │   └── DragDropPanel.razor         # Drag integrity checker
    │   │
    │   ├── Keyboard/
    │   │   ├── KeyRegistrationPanel.razor  # Full keyboard layout visualizer
    │   │   ├── ChatterDetectPanel.razor    # Key bounce analysis
    │   │   ├── PollingRatePanel.razor      # Key polling Hz
    │   │   ├── TypingTestPanel.razor       # 1-min typing test UI
    │   │   ├── RolloverTestPanel.razor     # Multi-key press tester
    │   │   ├── StuckKeyPanel.razor         # Stuck key detector
    │   │   └── SpecialKeyPanel.razor       # Fn / media key mapper
    │   │
    │   ├── Audio/
    │   │   ├── PlaybackCheckPanel.razor    # Channel routing selector
    │   │   ├── BalanceCheckPanel.razor     # Frequency sweep visualizer
    │   │   ├── MicLoopbackPanel.razor      # Record + playback UI
    │   │   └── EchoLatencyPanel.razor      # Latency measurement display
    │   │
    │   ├── Monitor/
    │   │   ├── DeadPixelPanel.razor        # Fullscreen color switcher
    │   │   ├── ColorGradientPanel.razor    # Gradient + contrast render
    │   │   ├── BacklightBleedPanel.razor   # Dark edge filter overlay
    │   │   ├── GhostingTestPanel.razor     # Moving object canvas
    │   │   ├── RefreshRatePanel.razor      # Frame counter display
    │   │   ├── GammaTestPanel.razor        # Gamma calibration pattern
    │   │   └── ResponseTimePanel.razor     # G2G transition tester
    │   │
    │   ├── Compute/
    │   │   ├── CpuBenchPanel.razor         # Thread count, progress, score
    │   │   ├── GpuBenchPanel.razor          # WebGL canvas + FPS counter
    │   │   └── ReportBuilderPanel.razor    # Compile + export report
    │   │
    │   ├── Forum/
    │   │   ├── ForumFeed.razor            # Post list with infinite scroll
    │   │   ├── ForumPostCard.razor         # Individual post preview
    │   │   ├── ForumPostDetail.razor       # Full post + nested comments
    │   │   ├── CommentThread.razor         # Recursive comment tree
    │   │   ├── NewPostEditor.razor         # Create post form
    │   │   └── PostRatingStars.razor       # Star rating widget
    │   │
    │   ├── Support/
    │   │   ├── TicketSubmitForm.razor      # Submit new ticket (auth required)
    │   │   ├── TicketListPanel.razor       # User's own tickets
    │   │   └── AdminTicketDashboard.razor  # Admin: all tickets + respond/close
    │   │
    │   ├── Solutions/
    │   │   ├── SolutionBrowser.razor       # Search + category filter
    │   │   ├── SolutionArticleView.razor   # Rendered markdown article
    │   │   └── AdminArticleEditor.razor    # Admin-only CRUD editor
    │   │
    │   ├── Chatbot/
    │   │   ├── ChatBubble.razor           # Floating bottom-right toggle
    │   │   └── ChatWindow.razor           # Message thread + input box
    │   │
    │   └── Donations/
    │       └── DonateButton.razor         # Stripe checkout trigger
    │
    ├── Pages/                              # ══════ ROUTABLE PAGES ══════
    │   ├── Home.razor                      # Landing / dashboard
    │   ├── NotFound.razor                  # 404 page
    │   │
    │   ├── NetworkDiagPage.razor           # /network
    │   ├── MouseDiagPage.razor             # /mouse
    │   ├── KeyboardDiagPage.razor          # /keyboard
    │   ├── AudioDiagPage.razor             # /audio
    │   ├── MonitorDiagPage.razor           # /monitor
    │   ├── ComputeDiagPage.razor           # /compute
    │   │
    │   ├── ReportPage.razor               # /report
    │   ├── ForumPage.razor                # /forum
    │   ├── ForumPostPage.razor            # /forum/{postId}
    │   ├── SupportPage.razor              # /support  (auth gated)
    │   ├── SolutionsPage.razor            # /solutions
    │   ├── DonatePage.razor               # /donate
    │   │
    │   ├── Auth/
    │   │   ├── LoginPage.razor            # /login
    │   │   └── RegisterPage.razor         # /register
    │   │
    │   └── Admin/
    │       ├── AdminDashboard.razor        # /admin
    │       ├── AdminTicketsPage.razor      # /admin/tickets
    │       └── AdminSolutionsPage.razor    # /admin/solutions
    │
    ├── wwwroot/                            # ══════ STATIC ASSETS ══════
    │   ├── index.html                      # Host HTML (Firebase SDK script tags)
    │   ├── manifest.webmanifest
    │   ├── service-worker.js
    │   ├── service-worker.published.js
    │   ├── icon-192.png
    │   ├── icon-512.png
    │   │
    │   ├── css/
    │   │   ├── app.css                    # Global reset + design tokens
    │   │   └── themes/
    │   │       ├── dark.css               # Dark mode variables
    │   │       └── light.css              # Light mode variables
    │   │
    │   ├── js/
    │   │   ├── firebase-config.js         # Firebase app initialization (ESM)
    │   │   ├── firebase-auth-interop.js   # Auth bridge functions
    │   │   ├── firestore-interop.js       # Firestore CRUD + listeners
    │   │   ├── network-interop.js         # Fetch-based speed/latency tests
    │   │   ├── mouse-interop.js           # Pointer event capture
    │   │   ├── keyboard-interop.js        # Keyboard event capture
    │   │   ├── audio-interop.js           # Web Audio API + MediaDevices
    │   │   ├── monitor-interop.js         # Canvas rendering + rAF loop
    │   │   ├── compute-interop.js         # Web Worker + WebGL launchers
    │   │   ├── leaflet-interop.js         # LeafletJS map initialization
    │   │   ├── stripe-interop.js          # Stripe.js checkout
    │   │   └── chatbot-interop.js         # Chatbot API integration
    │   │
    │   ├── workers/
    │   │   ├── cpu-bench-worker.js        # CPU stress test (prime sieve, etc.)
    │   │   └── network-stress-worker.js   # Concurrent fetch stream spawner
    │   │
    │   ├── shaders/
    │   │   ├── gpu-stress.vert            # Vertex shader (passthrough)
    │   │   └── gpu-stress.frag            # Fragment shader (heavy math)
    │   │
    │   └── audio/
    │       ├── test-tone-left.wav         # Left channel test
    │       ├── test-tone-right.wav        # Right channel test
    │       ├── test-tone-center.wav       # Center channel
    │       ├── test-tone-sub.wav          # Subwoofer
    │       ├── test-tone-surround-l.wav   # Surround left
    │       └── test-tone-surround-r.wav   # Surround right
    │
    └── Properties/
        └── launchSettings.json
```

---

## 3. State Management Matrix

### 3.1 Core Pattern: `StateContainerBase<T>`

Every diagnostic subsystem gets a **dedicated state container** registered as a `Scoped` service. Components subscribe via `OnChange` and unsubscribe on `Dispose`. The base class handles thread-safe notification without cascading re-renders.

```csharp
// Core/Interfaces/ is NOT where this lives — it goes in Services/State/Base/
namespace BenchRig.App.Services.State.Base;

/// <summary>
/// Thread-safe reactive state container for Blazor components.
/// Subclasses hold domain-specific mutable state and call
/// NotifyStateChanged() after mutations.
/// </summary>
public abstract class StateContainerBase : IDisposable
{
    // ── Event ────────────────────────────────────────────────
    // Components subscribe:  container.OnChange += StateHasChanged;
    // Components unsubscribe: container.OnChange -= StateHasChanged;
    public event Action? OnChange;

    // ── Notify ───────────────────────────────────────────────
    // Always call on the sync context to avoid cross-thread issues.
    // SynchronizationContext is null in WASM (single-threaded), but
    // this pattern is forward-compatible with .NET 10 WASM threading.
    protected void NotifyStateChanged()
    {
        // Capture to avoid race on unsubscribe
        var handler = OnChange;
        handler?.Invoke();
    }

    // ── Batched Updates ──────────────────────────────────────
    // For high-frequency streams (mouse polling, network chunks),
    // we throttle notifications to avoid layout thrashing.
    private DateTime _lastNotify = DateTime.MinValue;
    private static readonly TimeSpan ThrottleInterval = TimeSpan.FromMilliseconds(16); // ~60 fps

    protected void NotifyThrottled()
    {
        var now = DateTime.UtcNow;
        if (now - _lastNotify >= ThrottleInterval)
        {
            _lastNotify = now;
            NotifyStateChanged();
        }
    }

    public virtual void Dispose()
    {
        // Detach all handlers to prevent memory leaks
        OnChange = null;
    }
}
```

### 3.2 Concrete State Container Example

```csharp
namespace BenchRig.App.Services.State;

public sealed class NetworkDiagState : StateContainerBase
{
    // ── Immutable snapshots exposed to UI ────────────────────
    public SpeedTestResult? LatestSpeedTest { get; private set; }
    public IReadOnlyList<LatencyResult> LatencyHistory => _latencyHistory.AsReadOnly();
    public StabilityResult? StabilityResult { get; private set; }
    public StressTestResult? StressResult { get; private set; }
    public NetworkInfo? NetworkInfo { get; private set; }
    public bool IsRunning { get; private set; }
    public double ProgressPercent { get; private set; }

    // ── Private mutable backing ──────────────────────────────
    private readonly List<LatencyResult> _latencyHistory = new(256);

    // ── Mutation methods (called by Orchestrators) ───────────
    public void SetSpeedResult(SpeedTestResult result)
    {
        LatestSpeedTest = result;
        NotifyStateChanged();
    }

    public void AppendLatency(LatencyResult result)
    {
        _latencyHistory.Add(result);
        NotifyThrottled(); // High-frequency — throttle to 60fps
    }

    public void SetProgress(double percent)
    {
        ProgressPercent = percent;
        NotifyThrottled();
    }

    public void SetRunning(bool running)
    {
        IsRunning = running;
        NotifyStateChanged();
    }

    public void Reset()
    {
        LatestSpeedTest = null;
        _latencyHistory.Clear();
        StabilityResult = null;
        StressResult = null;
        ProgressPercent = 0;
        IsRunning = false;
        NotifyStateChanged();
    }
}
```

### 3.3 Component Subscription Pattern

```csharp
// Inside any Razor component's @code block:
@implements IDisposable
@inject NetworkDiagState NetState

protected override void OnInitialized()
{
    NetState.OnChange += StateHasChanged;
}

public void Dispose()
{
    NetState.OnChange -= StateHasChanged;
}
```

### 3.4 State Registration Matrix

All state containers are registered in `Program.cs`:

```csharp
// ── State Containers (Scoped = per-circuit, one instance per tab) ──
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

// ── JS Interop Bridges ──
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

// ── Orchestrators ──
builder.Services.AddScoped<NetworkTestOrchestrator>();
builder.Services.AddScoped<MouseTestOrchestrator>();
builder.Services.AddScoped<KeyboardTestOrchestrator>();
builder.Services.AddScoped<AudioTestOrchestrator>();
builder.Services.AddScoped<MonitorTestOrchestrator>();
builder.Services.AddScoped<ComputeTestOrchestrator>();
builder.Services.AddScoped<ReportOrchestrator>();

// ── Application Services ──
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminGuardService>();
```

### 3.5 Full State Dependency Matrix

| State Container | Writes From (Orchestrator/Service) | Reads From (Components) | Throttled? |
|---|---|---|---|
| `AuthStateContainer` | `AuthService` (Firebase callback) | `Sidebar`, `Topbar`, `LoginPanel`, all auth-gated pages | No |
| `NetworkDiagState` | `NetworkTestOrchestrator` | `SpeedTestPanel`, `LatencyPanel`, `StabilityPanel`, `StressTestPanel`, `NetworkInfoPanel` | Yes (latency stream) |
| `MouseDiagState` | `MouseTestOrchestrator` | `CpsTestPanel`, `PollingRatePanel`, `DpiCheckPanel`, `DeadZonePanel`, `AccuracyTestPanel` | Yes (polling events) |
| `KeyboardDiagState` | `KeyboardTestOrchestrator` | `KeyRegistrationPanel`, `ChatterDetectPanel`, `TypingTestPanel`, `RolloverTestPanel` | Yes (key events) |
| `AudioDiagState` | `AudioTestOrchestrator` | `PlaybackCheckPanel`, `BalanceCheckPanel`, `MicLoopbackPanel`, `EchoLatencyPanel` | Yes (mic level) |
| `MonitorDiagState` | `MonitorTestOrchestrator` | `DeadPixelPanel`, `RefreshRatePanel`, `GhostingTestPanel`, `GammaTestPanel` | Yes (frame counter) |
| `ComputeDiagState` | `ComputeTestOrchestrator` | `CpuBenchPanel`, `GpuBenchPanel` | Yes (progress) |
| `ReportState` | `ReportOrchestrator` | `ReportBuilderPanel` | No |
| `ForumState` | `ForumPage` (Firestore listener) | `ForumFeed`, `ForumPostCard`, `ForumPostDetail` | No |
| `TicketState` | `SupportPage`, `AdminTicketDashboard` | `TicketSubmitForm`, `TicketListPanel`, `AdminTicketDashboard` | No |
| `ChatbotState` | `ChatWindow` | `ChatBubble`, `ChatWindow` | No |

---

## 4. Firestore Schema & Security Rules

### 4.1 Document Schema

```
firestore-root/
│
├── users/{uid}                              # ── USER PROFILES ──
│   ├── email: string
│   ├── displayName: string
│   ├── photoURL: string | null
│   ├── role: "user" | "admin"               # Default: "user"
│   ├── createdAt: timestamp
│   └── lastLoginAt: timestamp
│
├── forum_posts/{postId}                     # ── FORUM POSTS ──
│   ├── title: string                        # Max 200 chars
│   ├── body: string                         # Markdown content, max 10000 chars
│   ├── authorUid: string                    # → users/{uid}
│   ├── authorName: string                   # Denormalized for read perf
│   ├── tags: string[]                       # ["network", "latency"]
│   ├── avgRating: number                    # Computed aggregate 1.0–5.0
│   ├── ratingCount: number                  # Total ratings
│   ├── commentCount: number                 # Total comments (denormalized)
│   ├── createdAt: timestamp
│   ├── updatedAt: timestamp
│   │
│   ├── comments/{commentId}                 # ── NESTED COMMENTS (subcollection) ──
│   │   ├── body: string                     # Max 2000 chars
│   │   ├── authorUid: string
│   │   ├── authorName: string
│   │   ├── parentCommentId: string | null   # null = top-level, else reply
│   │   ├── depth: number                    # 0 = top-level, max 3
│   │   └── createdAt: timestamp
│   │
│   └── ratings/{odUserIdAsDocId}            # ── POST RATINGS (subcollection) ──
│       ├── stars: number                    # 1–5
│       └── createdAt: timestamp
│
├── support_tickets/{ticketId}               # ── SUPPORT TICKETS ──
│   ├── subject: string                      # Max 200 chars
│   ├── body: string                         # Max 5000 chars
│   ├── authorUid: string
│   ├── authorName: string
│   ├── status: "open" | "in_progress" | "resolved" | "closed"
│   ├── priority: "low" | "medium" | "high"
│   ├── createdAt: timestamp
│   ├── updatedAt: timestamp
│   │
│   └── responses/{responseId}               # ── ADMIN RESPONSES (subcollection) ──
│       ├── body: string
│       ├── adminUid: string
│       ├── adminName: string
│       └── createdAt: timestamp
│
├── solution_articles/{articleId}            # ── KNOWLEDGE BASE (Admin-only write) ──
│   ├── title: string
│   ├── body: string                         # Markdown, max 50000 chars
│   ├── category: "network" | "mouse" | "keyboard" | "audio" | "monitor" | "compute" | "general"
│   ├── tags: string[]
│   ├── authorUid: string                    # Admin UID
│   ├── authorName: string
│   ├── isPublished: boolean
│   ├── createdAt: timestamp
│   └── updatedAt: timestamp
│
├── diagnostic_reports/{reportId}            # ── SAVED REPORTS (optional) ──
│   ├── ownerUid: string
│   ├── generatedAt: timestamp
│   ├── sections: map                        # Embedded sub-results
│   │   ├── network: map { ... }
│   │   ├── mouse: map { ... }
│   │   ├── keyboard: map { ... }
│   │   ├── audio: map { ... }
│   │   ├── monitor: map { ... }
│   │   └── compute: map { ... }
│   └── summary: string                     # AI-generated or template summary
│
└── app_config/settings                      # ── GLOBAL CONFIG (Admin-only) ──
    ├── chatbotEnabled: boolean
    ├── maintenanceMode: boolean
    ├── diagnosticServers: array<map>
    │   └── [{ url, label, region, lat, lng }]
    └── stripePublishableKey: string
```

### 4.2 Firestore Security Rules

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // ══════════════════════════════════════════════════════════
    // HELPER FUNCTIONS
    // ══════════════════════════════════════════════════════════

    function isAuthenticated() {
      return request.auth != null;
    }

    function isOwner(uid) {
      return isAuthenticated() && request.auth.uid == uid;
    }

    function isAdmin() {
      return isAuthenticated()
          && exists(/databases/$(database)/documents/users/$(request.auth.uid))
          && get(/databases/$(database)/documents/users/$(request.auth.uid)).data.role == "admin";
    }

    function isValidString(field, maxLen) {
      return field is string && field.size() > 0 && field.size() <= maxLen;
    }

    function isValidTimestamp(field) {
      return field == request.time;
    }

    // ══════════════════════════════════════════════════════════
    // USER PROFILES
    // ══════════════════════════════════════════════════════════

    match /users/{uid} {
      // Anyone can read profiles (for forum authorName lookups)
      allow read: if true;

      // Users can create their own profile on first login
      allow create: if isOwner(uid)
                    && request.resource.data.role == "user"   // Cannot self-assign admin
                    && isValidString(request.resource.data.email, 320)
                    && isValidString(request.resource.data.displayName, 100);

      // Users can update their own profile (except role)
      allow update: if (isOwner(uid) && !("role" in request.resource.data)
                        || request.resource.data.role == resource.data.role)
                    || isAdmin();  // Admins can change roles

      allow delete: if false;  // Profiles are never deleted from client
    }

    // ══════════════════════════════════════════════════════════
    // FORUM POSTS
    // ══════════════════════════════════════════════════════════

    match /forum_posts/{postId} {
      // Public read
      allow read: if true;

      // Authenticated users can create posts
      allow create: if isAuthenticated()
                    && request.resource.data.authorUid == request.auth.uid
                    && isValidString(request.resource.data.title, 200)
                    && isValidString(request.resource.data.body, 10000);

      // Authors can update their own posts; admins can update any
      allow update: if isOwner(resource.data.authorUid) || isAdmin();

      // Only admins can delete posts (moderation)
      allow delete: if isAdmin();

      // ── COMMENTS (subcollection) ──
      match /comments/{commentId} {
        allow read: if true;

        allow create: if isAuthenticated()
                      && request.resource.data.authorUid == request.auth.uid
                      && isValidString(request.resource.data.body, 2000)
                      && request.resource.data.depth <= 3;  // Max nesting depth

        allow update: if isOwner(resource.data.authorUid);
        allow delete: if isOwner(resource.data.authorUid) || isAdmin();
      }

      // ── RATINGS (subcollection, doc ID = user UID) ──
      match /ratings/{userId} {
        allow read: if true;

        // Users can only rate once (doc ID = their UID)
        allow create: if isOwner(userId)
                      && request.resource.data.stars >= 1
                      && request.resource.data.stars <= 5;

        allow update: if isOwner(userId)
                      && request.resource.data.stars >= 1
                      && request.resource.data.stars <= 5;

        allow delete: if isOwner(userId);
      }
    }

    // ══════════════════════════════════════════════════════════
    // SUPPORT TICKETS
    // ══════════════════════════════════════════════════════════

    match /support_tickets/{ticketId} {
      // Users can read their own tickets; admins can read all
      allow read: if isOwner(resource.data.authorUid) || isAdmin();

      // Authenticated users create tickets
      allow create: if isAuthenticated()
                    && request.resource.data.authorUid == request.auth.uid
                    && request.resource.data.status == "open"
                    && isValidString(request.resource.data.subject, 200)
                    && isValidString(request.resource.data.body, 5000);

      // Only admins can update ticket status (close without response, etc.)
      allow update: if isAdmin();

      allow delete: if false;

      // ── ADMIN RESPONSES (subcollection) ──
      match /responses/{responseId} {
        // Ticket author + admins can read responses
        allow read: if isAuthenticated()
                    && (isAdmin()
                        || get(/databases/$(database)/documents/support_tickets/$(ticketId)).data.authorUid == request.auth.uid);

        // Only admins can respond
        allow create: if isAdmin()
                      && request.resource.data.adminUid == request.auth.uid
                      && isValidString(request.resource.data.body, 5000);

        allow update: if false;
        allow delete: if false;
      }
    }

    // ══════════════════════════════════════════════════════════
    // SOLUTION ARTICLES (Admin-only write)
    // ══════════════════════════════════════════════════════════

    match /solution_articles/{articleId} {
      // Public read (only published articles in queries)
      allow read: if true;

      // Only admins can create/update/delete
      allow create: if isAdmin()
                    && request.resource.data.authorUid == request.auth.uid
                    && isValidString(request.resource.data.title, 200)
                    && isValidString(request.resource.data.body, 50000);

      allow update: if isAdmin();
      allow delete: if isAdmin();
    }

    // ══════════════════════════════════════════════════════════
    // DIAGNOSTIC REPORTS (User-owned)
    // ══════════════════════════════════════════════════════════

    match /diagnostic_reports/{reportId} {
      allow read: if isOwner(resource.data.ownerUid) || isAdmin();

      allow create: if isAuthenticated()
                    && request.resource.data.ownerUid == request.auth.uid;

      allow update: if isOwner(resource.data.ownerUid);
      allow delete: if isOwner(resource.data.ownerUid);
    }

    // ══════════════════════════════════════════════════════════
    // APP CONFIG (Admin-only)
    // ══════════════════════════════════════════════════════════

    match /app_config/{document} {
      allow read: if true;              // Client reads config
      allow write: if isAdmin();        // Only admins can modify
    }

    // ══════════════════════════════════════════════════════════
    // DEFAULT DENY
    // ══════════════════════════════════════════════════════════
    match /{document=**} {
      allow read, write: if false;
    }
  }
}
```

---

## 5. Component Hierarchy & Data Flow

### 5.1 Master Layout Hierarchy

```mermaid
graph TD
    App["App.razor<br/>(Router)"]
    ML["MainLayout.razor<br/>@inherits LayoutComponentBase"]
    SB["Sidebar.razor"]
    TB["Topbar.razor"]
    FB["FooterBar.razor"]
    CB["ChatBubble.razor"]
    Body["@Body<br/>(Routed Page)"]

    App --> ML
    ML --> SB
    ML --> TB
    ML --> Body
    ML --> FB
    ML --> CB

    style App fill:#1a1a2e,color:#e94560,stroke:#e94560
    style ML fill:#16213e,color:#0f3460,stroke:#0f3460
    style Body fill:#0f3460,color:#e94560,stroke:#e94560
```

### 5.2 Diagnostic Page → Panel → Engine Data Flow

Each diagnostic page follows an identical structural pattern:

```mermaid
graph LR
    subgraph "Page Layer (Routed)"
        Page["NetworkDiagPage.razor<br/>@page '/network'"]
    end

    subgraph "Component Layer (Panels)"
        P1["SpeedTestPanel"]
        P2["LatencyPanel"]
        P3["StabilityPanel"]
        P4["StressTestPanel"]
        P5["NetworkInfoPanel"]
        P6["ServerMapPanel"]
    end

    subgraph "Service Layer (Injected)"
        Orch["NetworkTestOrchestrator"]
        State["NetworkDiagState"]
    end

    subgraph "Bridge Layer (JS Interop)"
        Bridge["NetworkJsInterop<br/>implements IJsNetworkBridge"]
    end

    subgraph "Engine Layer (Pure C#)"
        E1["SpeedCalculator"]
        E2["JitterCalculator"]
        E3["PacketLossCalculator"]
    end

    subgraph "JavaScript Layer (wwwroot/js)"
        JS["network-interop.js<br/>fetch() / performance.now()"]
    end

    Page --> P1 & P2 & P3 & P4 & P5 & P6
    P1 & P2 & P3 & P4 -->|"@inject"| Orch
    P1 & P2 & P3 & P4 & P5 & P6 -->|"@inject + OnChange"| State
    Orch -->|"calls"| Bridge
    Orch -->|"calls"| E1 & E2 & E3
    Orch -->|"writes"| State
    Bridge -->|"IJSRuntime.InvokeAsync"| JS
    JS -->|"DotNetObjectReference callback"| Bridge

    style Page fill:#e94560,color:#fff
    style State fill:#533483,color:#fff
    style Orch fill:#0f3460,color:#fff
    style Bridge fill:#16213e,color:#e94560
    style JS fill:#1a1a2e,color:#e94560
```

### 5.3 Event & Telemetry Pipeline (All Diagnostic Domains)

Every domain follows this exact contract:

```
┌──────────────┐     User Action      ┌──────────────┐
│   Component  │ ──────────────────►  │ Orchestrator │
│   (Panel)    │   "Start Test" btn   │              │
└──────┬───────┘                      └──────┬───────┘
       │                                     │
       │ @inject State                       │ 1. Calls JS Bridge
       │ OnChange += StateHasChanged         │ 2. Receives raw data
       │                                     │ 3. Feeds into Engine
       │                                     │ 4. Writes to State
       │                                     │
       │         NotifyStateChanged()        │
       │ ◄─────────────────────────────────  │
       │                                     │
       ▼                                     ▼
  Re-render with                       Engine computes
  latest state                         pure results
```

### 5.4 Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant LoginPanel
    participant AuthService
    participant FirebaseAuthJsInterop
    participant JS as firebase-auth-interop.js
    participant Firebase as Firebase Auth SDK
    participant AuthState as AuthStateContainer

    User->>LoginPanel: Clicks "Sign In with Google"
    LoginPanel->>AuthService: SignInWithGoogleAsync()
    AuthService->>FirebaseAuthJsInterop: SignInWithPopupAsync()
    FirebaseAuthJsInterop->>JS: invokeAsync("firebaseSignInWithPopup")
    JS->>Firebase: signInWithPopup(auth, googleProvider)
    Firebase-->>JS: UserCredential
    JS-->>FirebaseAuthJsInterop: { uid, email, displayName }
    FirebaseAuthJsInterop-->>AuthService: UserProfile
    AuthService->>AuthState: SetUser(profile)
    AuthState->>AuthState: NotifyStateChanged()
    AuthState-->>LoginPanel: OnChange fires
    LoginPanel-->>User: Redirects to Dashboard
```

### 5.5 Forum Post + Comment Hierarchy

```mermaid
graph TD
    ForumPage["ForumPage.razor<br/>@page '/forum'"]
    Feed["ForumFeed.razor<br/>Infinite scroll list"]
    Card["ForumPostCard.razor<br/>× N posts"]
    Detail["ForumPostDetail.razor<br/>Full post view"]
    Thread["CommentThread.razor<br/>Recursive"]
    Editor["NewPostEditor.razor"]
    Stars["PostRatingStars.razor"]

    ForumPage --> Feed
    ForumPage --> Editor
    Feed --> Card
    Card -->|"Click"| Detail
    Detail --> Thread
    Detail --> Stars
    Thread -->|"self-recursive<br/>depth ≤ 3"| Thread

    style ForumPage fill:#e94560,color:#fff
    style Thread fill:#533483,color:#fff
```

### 5.6 Admin Dashboard Hierarchy

```mermaid
graph TD
    AD["AdminDashboard.razor<br/>@page '/admin'<br/>🔒 Admin role guard"]

    ATP["AdminTicketsPage.razor<br/>@page '/admin/tickets'"]
    ATD["AdminTicketDashboard.razor<br/>List + respond + close"]

    ASP["AdminSolutionsPage.razor<br/>@page '/admin/solutions'"]
    AAE["AdminArticleEditor.razor<br/>CRUD markdown editor"]

    AD --> ATP
    AD --> ASP
    ATP --> ATD
    ASP --> AAE

    style AD fill:#e94560,color:#fff
    style ATD fill:#0f3460,color:#fff
    style AAE fill:#0f3460,color:#fff
```

---

## 6. JS Interop Bridge Contracts

### 6.1 Interface Definitions (C# Side)

```csharp
// ── Core/Interfaces/IJsNetworkBridge.cs ──
namespace BenchRig.App.Core.Interfaces;

public interface IJsNetworkBridge : IAsyncDisposable
{
    /// <summary>Downloads a test chunk and returns bytes received + elapsed ms.</summary>
    Task<(long bytes, double elapsedMs)> DownloadChunkAsync(string serverUrl, int chunkSizeKb);

    /// <summary>Uploads a test payload and returns elapsed ms.</summary>
    Task<double> UploadChunkAsync(string serverUrl, byte[] payload);

    /// <summary>Sends an HTTP HEAD and returns round-trip ms.</summary>
    Task<double> PingAsync(string serverUrl);

    /// <summary>Spawns N concurrent fetch streams and reports aggregate throughput.</summary>
    Task<StressTestResult> RunStressTestAsync(string serverUrl, int concurrentStreams, int durationSec);

    /// <summary>Fetches public IP + geo data from a third-party API.</summary>
    Task<NetworkInfo> GetNetworkInfoAsync();
}

// ── Core/Interfaces/IJsFirebaseAuthBridge.cs ──
public interface IJsFirebaseAuthBridge : IAsyncDisposable
{
    Task<UserProfile?> SignInWithEmailAsync(string email, string password);
    Task<UserProfile?> SignInWithGoogleAsync();
    Task<UserProfile?> RegisterWithEmailAsync(string email, string password, string displayName);
    Task SignOutAsync();

    /// <summary>
    /// Registers a .NET callback that fires on auth state changes.
    /// Returns a disposable to unregister.
    /// </summary>
    Task RegisterAuthStateListenerAsync(DotNetObjectReference<AuthService> callbackRef);
}

// ── Core/Interfaces/IJsFirestoreBridge.cs ──
public interface IJsFirestoreBridge : IAsyncDisposable
{
    Task<string> AddDocumentAsync(string collectionPath, object data);
    Task SetDocumentAsync(string documentPath, object data);
    Task UpdateDocumentAsync(string documentPath, object partialData);
    Task DeleteDocumentAsync(string documentPath);
    Task<T?> GetDocumentAsync<T>(string documentPath);
    Task<List<T>> QueryCollectionAsync<T>(string collectionPath, QueryFilter? filter = null);

    /// <summary>
    /// Subscribes to real-time updates on a document or query.
    /// Returns a subscription ID to unsubscribe later.
    /// </summary>
    Task<string> SubscribeToDocumentAsync<T>(
        string documentPath,
        DotNetObjectReference<object> callbackRef,
        string callbackMethodName);

    Task UnsubscribeAsync(string subscriptionId);
}

// ── Core/Interfaces/IJsMouseBridge.cs ──
public interface IJsMouseBridge : IAsyncDisposable
{
    /// <summary>Starts capturing pointer events and invoking the .NET callback.</summary>
    Task StartPointerCaptureAsync(
        string elementId,
        DotNetObjectReference<object> callbackRef);

    Task StopPointerCaptureAsync(string elementId);

    /// <summary>Starts high-frequency pointermove tracking for polling rate.</summary>
    Task StartPollingRateCaptureAsync(
        string elementId,
        DotNetObjectReference<object> callbackRef);

    Task StopPollingRateCaptureAsync(string elementId);
}

// ── Core/Interfaces/IJsAudioBridge.cs ──
public interface IJsAudioBridge : IAsyncDisposable
{
    Task PlayTestToneAsync(string channelAudioPath, string outputChannelId);
    Task StopPlaybackAsync();
    Task<bool> StartMicCaptureAsync();
    Task StopMicCaptureAsync();
    Task StartLoopbackAsync();   // Route mic → speakers
    Task StopLoopbackAsync();
    Task<double> MeasureEchoLatencyAsync();
    Task<double> GetMicLevelDbAsync();
}

// ── Core/Interfaces/IJsMonitorBridge.cs ──
public interface IJsMonitorBridge : IAsyncDisposable
{
    Task EnterFullscreenAsync(string elementId);
    Task ExitFullscreenAsync();
    Task<double> MeasureFrameRateAsync(int sampleFrames);
    Task StartGhostingAnimationAsync(string canvasId, double speedPxPerFrame);
    Task StopGhostingAnimationAsync(string canvasId);
}

// ── Core/Interfaces/IJsComputeBridge.cs ──
public interface IJsComputeBridge : IAsyncDisposable
{
    /// <summary>Spawns Web Workers for CPU stress. Calls back with progress.</summary>
    Task<CpuBenchmarkResult> RunCpuBenchmarkAsync(
        int threadCount,
        int durationSec,
        DotNetObjectReference<object> progressCallback);

    /// <summary>Runs a heavy WebGL shader pass. Calls back with FPS.</summary>
    Task<GpuBenchmarkResult> RunGpuBenchmarkAsync(
        string canvasId,
        int durationSec,
        DotNetObjectReference<object> progressCallback);
}
```

### 6.2 JS Interop Implementation Pattern

```csharp
// ── Services/JsInterop/NetworkJsInterop.cs ──
namespace BenchRig.App.Services.JsInterop;

public sealed class NetworkJsInterop : IJsNetworkBridge
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public NetworkJsInterop(IJSRuntime js) => _js = js;

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "./js/network-interop.js");
        return _module;
    }

    public async Task<(long bytes, double elapsedMs)> DownloadChunkAsync(
        string serverUrl, int chunkSizeKb)
    {
        var module = await GetModuleAsync();
        var result = await module.InvokeAsync<JsonElement>(
            "downloadChunk", serverUrl, chunkSizeKb);

        return (
            result.GetProperty("bytes").GetInt64(),
            result.GetProperty("elapsedMs").GetDouble()
        );
    }

    // ... remaining methods follow same pattern ...

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }
    }
}
```

### 6.3 JavaScript Module Example

```javascript
// ── wwwroot/js/network-interop.js ──

/**
 * Downloads a chunk from the given server URL and measures elapsed time.
 * @param {string} serverUrl - Base URL of the diagnostic server
 * @param {number} chunkSizeKb - Requested chunk size in KB
 * @returns {{ bytes: number, elapsedMs: number }}
 */
export async function downloadChunk(serverUrl, chunkSizeKb) {
    const url = `${serverUrl}/download?size=${chunkSizeKb}`;
    const start = performance.now();

    const response = await fetch(url, {
        method: 'GET',
        cache: 'no-store',
        mode: 'cors',
    });

    const buffer = await response.arrayBuffer();
    const elapsed = performance.now() - start;

    return { bytes: buffer.byteLength, elapsedMs: elapsed };
}

/**
 * Sends an HTTP HEAD request and returns round-trip time in ms.
 */
export async function ping(serverUrl) {
    const start = performance.now();
    await fetch(serverUrl, { method: 'HEAD', cache: 'no-store', mode: 'cors' });
    return performance.now() - start;
}

/**
 * Spawns concurrent fetch streams and measures aggregate throughput.
 */
export async function runStressTest(serverUrl, concurrentStreams, durationSec) {
    const controller = new AbortController();
    const deadline = Date.now() + durationSec * 1000;
    let totalBytes = 0;
    let completedStreams = 0;

    const streamTask = async () => {
        while (Date.now() < deadline && !controller.signal.aborted) {
            try {
                const res = await fetch(`${serverUrl}/download?size=1024`, {
                    signal: controller.signal,
                    cache: 'no-store',
                });
                const buf = await res.arrayBuffer();
                totalBytes += buf.byteLength;
            } catch { break; }
        }
        completedStreams++;
    };

    const streams = Array.from({ length: concurrentStreams }, () => streamTask());

    setTimeout(() => controller.abort(), durationSec * 1000);
    await Promise.allSettled(streams);

    return {
        totalBytes,
        durationMs: durationSec * 1000,
        concurrentStreams,
        throughputMbps: (totalBytes * 8) / (durationSec * 1_000_000),
    };
}
```

---

## 7. Orchestrator Pattern (Workflow Coordinator)

Orchestrators are the **only** classes that combine JS bridges + engines + state mutations. Components never call bridges directly.

```csharp
// ── Services/Orchestrators/NetworkTestOrchestrator.cs ──
namespace BenchRig.App.Services.Orchestrators;

public sealed class NetworkTestOrchestrator
{
    private readonly IJsNetworkBridge _bridge;
    private readonly NetworkDiagState _state;
    private readonly SpeedCalculator _speedCalc = new();
    private readonly JitterCalculator _jitterCalc = new();

    public NetworkTestOrchestrator(IJsNetworkBridge bridge, NetworkDiagState state)
    {
        _bridge = bridge;
        _state = state;
    }

    /// <summary>
    /// Runs the full speed test: multiple chunk downloads/uploads,
    /// calculates Mbps, and writes results to state.
    /// </summary>
    public async Task RunSpeedTestAsync(DiagnosticServer server, CancellationToken ct = default)
    {
        _state.SetRunning(true);
        _state.SetProgress(0);

        try
        {
            // ── Download phase (10 chunks) ──
            long totalDownBytes = 0;
            double totalDownMs = 0;
            for (int i = 0; i < 10 && !ct.IsCancellationRequested; i++)
            {
                var (bytes, ms) = await _bridge.DownloadChunkAsync(server.Url, chunkSizeKb: 2048);
                totalDownBytes += bytes;
                totalDownMs += ms;
                _state.SetProgress((i + 1) * 5.0); // 0–50%
            }

            double downloadMbps = _speedCalc.CalculateMbps(totalDownBytes, totalDownMs);

            // ── Upload phase (10 chunks) ──
            var payload = new byte[2 * 1024 * 1024]; // 2MB
            double totalUpMs = 0;
            for (int i = 0; i < 10 && !ct.IsCancellationRequested; i++)
            {
                totalUpMs += await _bridge.UploadChunkAsync(server.Url, payload);
                _state.SetProgress(50 + (i + 1) * 5.0); // 50–100%
            }

            double uploadMbps = _speedCalc.CalculateMbps(payload.Length * 10L, totalUpMs);

            _state.SetSpeedResult(new SpeedTestResult
            {
                DownloadMbps = downloadMbps,
                UploadMbps = uploadMbps,
                ServerLabel = server.Label,
                Timestamp = DateTime.UtcNow,
            });
        }
        finally
        {
            _state.SetRunning(false);
        }
    }

    /// <summary>
    /// Runs N ping iterations, calculates jitter, and streams results to state.
    /// </summary>
    public async Task RunLatencyTestAsync(DiagnosticServer server, int iterations = 50)
    {
        _state.SetRunning(true);
        var pings = new List<double>(iterations);

        for (int i = 0; i < iterations; i++)
        {
            double pingMs = await _bridge.PingAsync(server.Url);
            pings.Add(pingMs);

            _state.AppendLatency(new LatencyResult
            {
                PingMs = pingMs,
                JitterMs = _jitterCalc.CalculateRunning(pings),
                PacketIndex = i,
            });
        }

        _state.SetRunning(false);
    }
}
```

---

## 8. NuGet Dependency Map

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.0.8 | Blazor WASM runtime |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | 10.0.8 | Dev-time hot reload |
| `System.Text.Json` | (built-in) | JS Interop serialization |

> [!NOTE]
> **No additional NuGet packages are required.** Firebase, Leaflet, Stripe, and WebGL are all consumed through the JS Interop layer (loaded via `<script>` or ES module imports in `index.html`). This keeps the .NET assembly minimal and trim-friendly.

### External JS Dependencies (loaded in `index.html`)

| Library | CDN/Module | Purpose |
|---|---|---|
| Firebase JS SDK v10+ | ESM modules | Auth, Firestore |
| LeafletJS 1.9+ | CDN | Geospatial map rendering |
| Stripe.js v3 | CDN | Payment processing |
| (Chatbot provider) | Provider-specific | AI chatbot widget |

---

## 9. `index.html` Host Configuration

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>BenchRig — Hardware & Network Diagnostic Platform</title>
    <meta name="description" content="Comprehensive client-side hardware, network, and peripheral diagnostic platform." />
    <base href="/" />

    <!-- Blazor asset preload -->
    <link rel="preload" id="webassembly" />

    <!-- App Styles -->
    <link rel="stylesheet" href="css/app.css" />
    <link href="BenchRig.App.styles.css" rel="stylesheet" />

    <!-- PWA -->
    <link href="manifest.webmanifest" rel="manifest" />
    <link rel="apple-touch-icon" sizes="512x512" href="icon-512.png" />

    <!-- LeafletJS CSS -->
    <link rel="stylesheet"
          href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
          integrity="sha256-..." crossorigin="" />

    <!-- Firebase SDK (ESM — loaded as module in JS files) -->
    <script type="importmap">
    {
        "imports": {
            "firebase/app": "https://www.gstatic.com/firebasejs/10.14.0/firebase-app.js",
            "firebase/auth": "https://www.gstatic.com/firebasejs/10.14.0/firebase-auth.js",
            "firebase/firestore": "https://www.gstatic.com/firebasejs/10.14.0/firebase-firestore.js"
        }
    }
    </script>
</head>
<body>
    <div id="app">
        <svg class="loading-progress">
            <circle r="40%" cx="50%" cy="50%" />
            <circle r="40%" cx="50%" cy="50%" />
        </svg>
        <div class="loading-progress-text"></div>
    </div>

    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="." class="reload">Reload</a>
        <span class="dismiss">🗙</span>
    </div>

    <!-- LeafletJS -->
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
            integrity="sha256-..." crossorigin=""></script>

    <!-- Stripe.js -->
    <script src="https://js.stripe.com/v3/"></script>

    <!-- Blazor WASM bootstrap -->
    <script src="_framework/blazor.webassembly.js"></script>

    <!-- Service Worker (PWA) -->
    <script>navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });</script>
</body>
</html>
```

---

## 10. Key Architectural Principles Summary

### 10.1 Dependency Rule (Clean Architecture)

```
Components/Pages  →  Services/State  →  Core/Engines  →  Core/Models
       ↓                    ↓
  Services/Orchestrators    │
       ↓                    │
  Services/JsInterop  ←────┘  (implements Core/Interfaces)
       ↓
  wwwroot/js/*.js
       ↓
  Browser APIs / Firebase SDK
```

> [!IMPORTANT]
> **Arrows point inward.** `Core/` never references `Services/` or `Components/`. `Services/` never references `Components/`. JS Interop implementations live in `Services/` but implement interfaces defined in `Core/`.

### 10.2 Why Orchestrators?

Without orchestrators, components would need to:
1. Inject the JS bridge directly (couples UI to browser APIs)
2. Call pure engines and write to state (duplicates workflow logic)
3. Handle cancellation, error recovery, and progress tracking inline

Orchestrators centralize this into **testable, reusable workflow classes** that are independent of the Razor rendering lifecycle.

### 10.3 Why Throttled Notifications?

Mouse polling at 1000Hz and key events during fast typing can fire `StateHasChanged()` hundreds of times per second. The `NotifyThrottled()` method in `StateContainerBase` caps UI updates to ~60fps (16ms intervals), preventing:
- Layout thrashing (re-rendering the DOM tree on every event)
- UI thread starvation (Blazor WASM is single-threaded)
- Battery drain on laptops

### 10.4 Admin Role Enforcement (Defense in Depth)

| Layer | Mechanism |
|---|---|
| **Firestore Rules** | `isAdmin()` function checks `users/{uid}.role == "admin"` — server-enforced, untamperable |
| **C# Service** | `AdminGuardService.EnsureAdminAsync()` checks `AuthStateContainer.CurrentUser.Role` before calling any admin Firestore operation |
| **Component** | Admin pages check role on `OnInitializedAsync` and redirect to `/login` if unauthorized |
| **UI** | Sidebar conditionally renders admin links only when `AuthState.IsAdmin` is true |

---

## 11. Verification Plan

### 11.1 Build Verification

```bash
dotnet build BenchRig.slnx -c Release
dotnet publish BenchRig.App -c Release -o ./publish
```

### 11.2 Structural Verification

- Verify no `IJSRuntime` references exist in `Core/` namespace (dependency rule)
- Verify all state containers inherit from `StateContainerBase`
- Verify all JS interop classes implement a `Core/Interfaces/` interface
- Verify all orchestrators inject bridges via interface, not concrete type

### 11.3 Runtime Verification

- Run `dotnet run --project BenchRig.App` and verify the app loads at `https://localhost:5001`
- Open browser DevTools → Network tab → verify Firebase SDK modules load
- Navigate to each diagnostic page and verify component rendering
- Test auth flow: register → login → logout → role guard redirects

### 11.4 Firestore Rules Testing

```bash
firebase emulators:start --only firestore
# Run rules unit tests against the emulator
```

---

## 12. Resolved Design Decisions

All open questions have been resolved. The following sections detail the finalized architectural choices.

---

### 12.1 AI Chatbot — OpenRouter `gpt-oss-120b:free`

**Decision**: Custom-built chat UI (`ChatBubble.razor` + `ChatWindow.razor`) backed by the OpenRouter API, using the `gpt-oss-120b:free` model.

#### Architecture

```
┌──────────────────────────────┐
│  ChatWindow.razor            │
│  (Message list + input box)  │
│                              │
│  @inject ChatbotState        │
│  @inject IJsChatbotBridge    │
└──────────┬───────────────────┘
           │ SendMessageAsync(userText)
           ▼
┌──────────────────────────────┐
│  ChatbotJsInterop.cs         │
│  implements IJsChatbotBridge  │
│                              │
│  Calls chatbot-interop.js    │
└──────────┬───────────────────┘
           │ fetch("https://openrouter.ai/api/v1/chat/completions")
           ▼
┌──────────────────────────────┐
│  OpenRouter API              │
│  Model: gpt-oss-120b:free    │
│  System prompt enforced      │
└──────────────────────────────┘
```

#### C# Interface

```csharp
// Core/Interfaces/IJsChatbotBridge.cs
namespace BenchRig.App.Core.Interfaces;

public interface IJsChatbotBridge : IAsyncDisposable
{
    /// <summary>
    /// Sends a user message to the LLM and returns the assistant response.
    /// The JS layer prepends the system prompt that enforces diagnostic-only responses.
    /// </summary>
    Task<string> SendMessageAsync(string userMessage, List<ChatMessage> history);
}
```

#### Chat Models

```csharp
// Core/Models/Chatbot/ChatMessage.cs
namespace BenchRig.App.Core.Models.Chatbot;

public record ChatMessage
{
    public required string Role { get; init; }    // "user" | "assistant" | "system"
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
```

#### JavaScript Module — `chatbot-interop.js`

```javascript
// wwwroot/js/chatbot-interop.js

const OPENROUTER_API_URL = "https://openrouter.ai/api/v1/chat/completions";
const MODEL = "gpt-oss-120b:free";

// ══════════════════════════════════════════════════════════════
// SYSTEM PROMPT — ENFORCED GUARDRAIL
// The model is constrained to ONLY provide hardware/network
// diagnostic troubleshooting responses. All other topics are
// politely declined.
// ══════════════════════════════════════════════════════════════
const SYSTEM_PROMPT = `You are BenchRig Assistant, an expert technical support chatbot embedded inside a hardware and network diagnostic platform. Your ONLY purpose is to help users diagnose, troubleshoot, and resolve issues related to:

- Network performance (speed, latency, jitter, packet loss, DNS, ISP issues)
- Mouse hardware (polling rate, DPI, button registration, jitter, dead zones)
- Keyboard hardware (key registration, chattering, ghosting, NKRO, stuck keys)
- Audio devices (playback channels, microphone input, echo, latency)
- Monitor/display (dead pixels, backlight bleed, ghosting, refresh rate, gamma, response time)
- CPU and GPU performance (benchmarking, thermal throttling, driver issues)

RULES:
1. NEVER answer questions unrelated to hardware diagnostics or network troubleshooting.
2. If asked about unrelated topics, respond: "I can only assist with hardware and network diagnostic topics. Please ask me about your device or connection issues!"
3. Keep answers concise, actionable, and technically accurate.
4. When possible, reference specific BenchRig diagnostic tests the user can run.
5. Never generate code, write essays, tell stories, or engage in general conversation.`;

/**
 * Sends a chat completion request to OpenRouter.
 * @param {string} userMessage - The user's latest message
 * @param {Array<{role: string, content: string}>} history - Conversation history
 * @returns {Promise<string>} - The assistant's response text
 */
export async function sendChatMessage(userMessage, history) {
    const messages = [
        { role: "system", content: SYSTEM_PROMPT },
        ...history,
        { role: "user", content: userMessage }
    ];

    const response = await fetch(OPENROUTER_API_URL, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "HTTP-Referer": window.location.origin,
            "X-Title": "BenchRig Diagnostic Platform"
            // Note: gpt-oss-120b:free does not require an API key
        },
        body: JSON.stringify({
            model: MODEL,
            messages: messages,
            max_tokens: 1024,
            temperature: 0.4  // Lower temperature for factual diagnostic responses
        })
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`OpenRouter API error: ${response.status} - ${errorText}`);
    }

    const data = await response.json();
    return data.choices?.[0]?.message?.content ?? "I'm unable to respond right now. Please try again.";
}
```

#### State Container

```csharp
// Services/State/ChatbotState.cs
namespace BenchRig.App.Services.State;

public sealed class ChatbotState : StateContainerBase
{
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }

    private readonly List<ChatMessage> _messages = new(64);

    public void ToggleOpen()
    {
        IsOpen = !IsOpen;
        NotifyStateChanged();
    }

    public void AddUserMessage(string content)
    {
        _messages.Add(new ChatMessage { Role = "user", Content = content });
        IsLoading = true;
        NotifyStateChanged();
    }

    public void AddAssistantMessage(string content)
    {
        _messages.Add(new ChatMessage { Role = "assistant", Content = content });
        IsLoading = false;
        NotifyStateChanged();
    }

    public void SetError(string errorMessage)
    {
        _messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = $"⚠️ {errorMessage}"
        });
        IsLoading = false;
        NotifyStateChanged();
    }
}
```

> [!NOTE]
> The system prompt is embedded **in the JS layer**, not in C#. This means:
> - It cannot be inspected or modified by users via .NET reflection in the browser
> - The full message array (including system prompt) is constructed in JS just before the fetch call
> - The `gpt-oss-120b:free` model on OpenRouter requires no API key — requests are rate-limited by IP

---

### 12.2 Network Diagnostics — LibreSpeed Public Servers

**Decision**: Use [LibreSpeed](https://github.com/librespeed/speedtest) public test servers. LibreSpeed is open-source, CORS-friendly, and provides a well-documented server list API.

#### Server Discovery

```csharp
// Core/Models/Network/DiagnosticServer.cs
namespace BenchRig.App.Core.Models.Network;

public record DiagnosticServer
{
    public required int Id { get; init; }
    public required string Name { get; init; }       // "Amsterdam, NL"
    public required string Server { get; init; }     // "https://ams.host.lol/speedtest"
    public required string Sponsor { get; init; }    // "WorldStream"
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public double? DistanceKm { get; set; }          // Calculated client-side
}
```

#### JS Module — Server List + Speed Test

```javascript
// wwwroot/js/network-interop.js (additions)

// LibreSpeed public server list endpoint
const LIBRE_SERVER_LIST = "https://librespeed.org/backend/getServer.php";

/**
 * Fetches the LibreSpeed public server list.
 * Falls back to a hardcoded list if the API is unreachable.
 */
export async function getServerList() {
    try {
        const res = await fetch("https://librespeed.org/backend/getServer_dl.php");
        return await res.json();
    } catch {
        // Fallback hardcoded servers
        return [
            { id: 1, name: "Amsterdam, NL", server: "//ams.host.lol/speedtest/", sponsor: "WorldStream", lat: 52.3676, lng: 4.9041 },
            { id: 2, name: "New York, US", server: "//nyc.host.lol/speedtest/", sponsor: "HostUS", lat: 40.7128, lng: -74.0060 },
            { id: 3, name: "Singapore", server: "//sg.host.lol/speedtest/", sponsor: "DigitalOcean", lat: 1.3521, lng: 103.8198 },
            { id: 4, name: "London, UK", server: "//lon.host.lol/speedtest/", sponsor: "OVH", lat: 51.5074, lng: -0.1278 },
            { id: 5, name: "Frankfurt, DE", server: "//fra.host.lol/speedtest/", sponsor: "Hetzner", lat: 50.1109, lng: 8.6821 },
        ];
    }
}

/**
 * Downloads from a LibreSpeed server and measures speed.
 * LibreSpeed servers expose /download (garbage data) and /upload endpoints.
 */
export async function downloadChunk(serverUrl, chunkSizeKb) {
    const url = `${serverUrl}/garbage.php?ckSize=${chunkSizeKb}`;
    const start = performance.now();

    const response = await fetch(url, {
        method: 'GET',
        cache: 'no-store',
        mode: 'cors',
    });

    const buffer = await response.arrayBuffer();
    const elapsed = performance.now() - start;

    return { bytes: buffer.byteLength, elapsedMs: elapsed };
}

/**
 * Uploads a payload to a LibreSpeed server and measures speed.
 */
export async function uploadChunk(serverUrl, payloadBytes) {
    const blob = new Blob([new Uint8Array(payloadBytes)]);
    const start = performance.now();

    await fetch(`${serverUrl}/empty.php`, {
        method: 'POST',
        body: blob,
        cache: 'no-store',
        mode: 'cors',
    });

    return performance.now() - start;
}

/**
 * Measures ping via HTTP HEAD to the server.
 */
export async function ping(serverUrl) {
    const start = performance.now();
    await fetch(`${serverUrl}/empty.php`, {
        method: 'HEAD',
        cache: 'no-store',
        mode: 'cors',
    });
    return performance.now() - start;
}

/**
 * Gets public IP and geo data using ip-api.com (free, no key required).
 */
export async function getNetworkInfo() {
    const res = await fetch("http://ip-api.com/json/?fields=query,isp,org,as,reverse,lat,lon,city,regionName,country,timezone");
    return await res.json();
}
```

#### LeafletJS Map Integration

```javascript
// wwwroot/js/leaflet-interop.js

let mapInstance = null;
let markerGroup = null;

/**
 * Initializes the LeafletJS map in the given container element.
 * @param {string} elementId - The DOM element ID for the map container
 * @param {number} lat - Initial center latitude
 * @param {number} lng - Initial center longitude
 * @param {number} zoom - Initial zoom level
 */
export function initMap(elementId, lat, lng, zoom) {
    if (mapInstance) {
        mapInstance.remove();
    }

    mapInstance = L.map(elementId).setView([lat, lng], zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 18,
    }).addTo(mapInstance);

    markerGroup = L.layerGroup().addTo(mapInstance);
}

/**
 * Plots server markers on the map with ping latency labels.
 * @param {Array<{name: string, lat: number, lng: number, pingMs: number|null}>} servers
 */
export function plotServers(servers) {
    if (!mapInstance || !markerGroup) return;
    markerGroup.clearLayers();

    servers.forEach(s => {
        const color = s.pingMs === null ? 'gray'
            : s.pingMs < 50 ? '#22c55e'   // Green: excellent
            : s.pingMs < 100 ? '#eab308'  // Yellow: good
            : '#ef4444';                   // Red: poor

        const circle = L.circleMarker([s.lat, s.lng], {
            radius: 8,
            fillColor: color,
            color: '#fff',
            weight: 2,
            fillOpacity: 0.9,
        });

        const label = s.pingMs !== null
            ? `<b>${s.name}</b><br/>Ping: ${s.pingMs.toFixed(1)} ms`
            : `<b>${s.name}</b><br/>Ping: —`;

        circle.bindPopup(label);
        markerGroup.addLayer(circle);
    });

    // Fit map to show all markers
    if (servers.length > 0) {
        const bounds = L.latLngBounds(servers.map(s => [s.lat, s.lng]));
        mapInstance.fitBounds(bounds, { padding: [50, 50] });
    }
}

/**
 * Highlights the user's approximate location on the map.
 */
export function plotUserLocation(lat, lng) {
    if (!mapInstance) return;

    L.marker([lat, lng], {
        icon: L.divIcon({
            className: 'user-location-marker',
            html: '<div style="background:#3b82f6;width:16px;height:16px;border-radius:50%;border:3px solid #fff;box-shadow:0 0 8px rgba(59,130,246,0.6);"></div>',
            iconSize: [16, 16],
        })
    })
    .bindPopup('<b>Your Location</b>')
    .addTo(mapInstance);
}

export function destroyMap() {
    if (mapInstance) {
        mapInstance.remove();
        mapInstance = null;
        markerGroup = null;
    }
}
```

---

### 12.3 Stripe — One-Time Donations via Checkout Session

**Decision**: Simple one-time donation flow using Stripe Checkout in **client-only mode** (no backend needed). Stripe Checkout supports client-only integration for simple payment links.

#### Architecture

Since BenchRig is a **pure client-side** app with no backend server, we use Stripe's **Payment Links** approach. An admin pre-creates a Payment Link in the Stripe Dashboard, and the client simply redirects to it.

```
┌─────────────────┐     Click "Donate"     ┌──────────────────┐
│ DonateButton    │ ─────────────────────► │ stripe-interop.js │
│ .razor          │                        │                   │
└─────────────────┘                        └────────┬──────────┘
                                                    │
                                           window.open(paymentLink)
                                                    │
                                                    ▼
                                           ┌──────────────────┐
                                           │  Stripe Checkout  │
                                           │  (Hosted by       │
                                           │   Stripe)         │
                                           └──────────────────┘
```

#### C# Interface

```csharp
// Core/Interfaces/IJsStripeBridge.cs
namespace BenchRig.App.Core.Interfaces;

public interface IJsStripeBridge : IAsyncDisposable
{
    /// <summary>
    /// Redirects the user to a Stripe-hosted payment page for a one-time donation.
    /// </summary>
    /// <param name="amountCents">Donation amount in cents (e.g., 500 = $5.00)</param>
    Task RedirectToCheckoutAsync(int amountCents);
}
```

#### JavaScript Module

```javascript
// wwwroot/js/stripe-interop.js

// Stripe Payment Links (pre-configured in Stripe Dashboard)
// Each link maps to a fixed donation amount.
// Admin creates these in Stripe Dashboard → Payment Links.
const DONATION_LINKS = {
    500:   "https://buy.stripe.com/YOUR_LINK_5",     // $5
    1000:  "https://buy.stripe.com/YOUR_LINK_10",    // $10
    2500:  "https://buy.stripe.com/YOUR_LINK_25",    // $25
    5000:  "https://buy.stripe.com/YOUR_LINK_50",    // $50
    custom: "https://buy.stripe.com/YOUR_LINK_CUSTOM" // Custom amount
};

/**
 * Redirects to the Stripe-hosted payment page.
 * @param {number} amountCents - Donation amount in cents
 */
export function redirectToCheckout(amountCents) {
    const link = DONATION_LINKS[amountCents] ?? DONATION_LINKS.custom;
    window.open(link, '_blank', 'noopener,noreferrer');
}
```

> [!TIP]
> **No backend required.** Stripe Payment Links handle the entire checkout flow (payment form, receipt, confirmation) on Stripe's hosted pages. The admin creates links in the Stripe Dashboard. No webhooks, no server-side sessions, no Stripe SDK initialization needed on the client.

#### Donation Component

```csharp
// Components/Donations/DonateButton.razor
@inject IJsStripeBridge StripeBridge

<div class="donate-panel">
    <h3>Support BenchRig</h3>
    <p>Help us keep this diagnostic platform free and open.</p>

    <div class="donate-amounts">
        <button @onclick="() => DonateAsync(500)">$5</button>
        <button @onclick="() => DonateAsync(1000)">$10</button>
        <button @onclick="() => DonateAsync(2500)">$25</button>
        <button @onclick="() => DonateAsync(5000)">$50</button>
        <button @onclick="() => DonateAsync(0)">Custom</button>
    </div>
</div>

@code {
    private async Task DonateAsync(int amountCents)
    {
        await StripeBridge.RedirectToCheckoutAsync(amountCents);
    }
}
```

---

### 12.4 Firebase Project Configuration — Resolved

**Decision**: Firebase project `diagnostic-platform-blazor` is fully provisioned. Configuration is embedded in the JS initialization module.

#### `firebase-config.js` — Final Implementation

```javascript
// wwwroot/js/firebase-config.js
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";
import { getFirestore } from "firebase/firestore";
import { getAnalytics } from "firebase/analytics";

const firebaseConfig = {
    apiKey: "AIzaSyBtVyQ1eZLPGp1nXA45s7mtc0VH8dDYRVs",
    authDomain: "diagnostic-platform-blazor.firebaseapp.com",
    projectId: "diagnostic-platform-blazor",
    storageBucket: "diagnostic-platform-blazor.firebasestorage.app",
    messagingSenderId: "876830402780",
    appId: "1:876830402780:web:1b15fbf39ee7d980cd2c19",
    measurementId: "G-NGN2CF3SJZ"
};

// Initialize Firebase services
const app = initializeApp(firebaseConfig);

/** @type {import("firebase/auth").Auth} */
export const auth = getAuth(app);

/** @type {import("firebase/firestore").Firestore} */
export const db = getFirestore(app);

/** @type {import("firebase/analytics").Analytics} */
export const analytics = getAnalytics(app);
```

#### Updated `index.html` Import Map

The `index.html` import map must include all Firebase modules consumed by the interop layer:

```html
<script type="importmap">
{
    "imports": {
        "firebase/app": "https://www.gstatic.com/firebasejs/10.14.0/firebase-app.js",
        "firebase/auth": "https://www.gstatic.com/firebasejs/10.14.0/firebase-auth.js",
        "firebase/firestore": "https://www.gstatic.com/firebasejs/10.14.0/firebase-firestore.js",
        "firebase/analytics": "https://www.gstatic.com/firebasejs/10.14.0/firebase-analytics.js"
    }
}
</script>
```

#### Auth Interop — `firebase-auth-interop.js`

```javascript
// wwwroot/js/firebase-auth-interop.js
import { auth } from "./firebase-config.js";
import {
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    signInWithPopup,
    GoogleAuthProvider,
    signOut,
    onAuthStateChanged,
    updateProfile
} from "firebase/auth";

const googleProvider = new GoogleAuthProvider();

export async function signInWithEmail(email, password) {
    const cred = await signInWithEmailAndPassword(auth, email, password);
    return mapUser(cred.user);
}

export async function registerWithEmail(email, password, displayName) {
    const cred = await createUserWithEmailAndPassword(auth, email, password);
    await updateProfile(cred.user, { displayName });
    return mapUser(cred.user);
}

export async function signInWithGoogle() {
    const cred = await signInWithPopup(auth, googleProvider);
    return mapUser(cred.user);
}

export async function firebaseSignOut() {
    await signOut(auth);
}

/**
 * Registers a .NET callback for auth state changes.
 * @param {DotNetObjectReference} dotNetRef
 * @param {string} methodName - The [JSInvokable] method name
 */
export function registerAuthStateListener(dotNetRef, methodName) {
    return onAuthStateChanged(auth, (user) => {
        const mapped = user ? mapUser(user) : null;
        dotNetRef.invokeMethodAsync(methodName, mapped);
    });
}

function mapUser(user) {
    return {
        uid: user.uid,
        email: user.email,
        displayName: user.displayName ?? user.email?.split('@')[0] ?? 'User',
        photoURL: user.photoURL,
    };
}
```

#### Firestore Interop — `firestore-interop.js`

```javascript
// wwwroot/js/firestore-interop.js
import { db } from "./firebase-config.js";
import {
    collection, doc, addDoc, setDoc, updateDoc, deleteDoc,
    getDoc, getDocs, query, where, orderBy, limit,
    onSnapshot
} from "firebase/firestore";

const activeSubscriptions = new Map();

export async function addDocument(collectionPath, data) {
    const ref = await addDoc(collection(db, collectionPath), {
        ...data,
        createdAt: new Date().toISOString()
    });
    return ref.id;
}

export async function setDocument(documentPath, data) {
    const ref = doc(db, documentPath);
    await setDoc(ref, data);
}

export async function updateDocument(documentPath, partialData) {
    const ref = doc(db, documentPath);
    await updateDoc(ref, { ...partialData, updatedAt: new Date().toISOString() });
}

export async function deleteDocument(documentPath) {
    await deleteDoc(doc(db, documentPath));
}

export async function getDocument(documentPath) {
    const snap = await getDoc(doc(db, documentPath));
    return snap.exists() ? { id: snap.id, ...snap.data() } : null;
}

export async function queryCollection(collectionPath, filters) {
    let q = collection(db, collectionPath);
    const constraints = [];

    if (filters) {
        if (filters.whereField && filters.whereOp && filters.whereValue !== undefined) {
            constraints.push(where(filters.whereField, filters.whereOp, filters.whereValue));
        }
        if (filters.orderByField) {
            constraints.push(orderBy(filters.orderByField, filters.orderDir ?? 'asc'));
        }
        if (filters.limitCount) {
            constraints.push(limit(filters.limitCount));
        }
    }

    const snap = await getDocs(query(q, ...constraints));
    return snap.docs.map(d => ({ id: d.id, ...d.data() }));
}

/**
 * Subscribes to real-time updates on a document.
 * Returns a subscription ID for later unsubscription.
 */
export function subscribeToDocument(documentPath, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    const unsubscribe = onSnapshot(doc(db, documentPath), (snap) => {
        const data = snap.exists() ? { id: snap.id, ...snap.data() } : null;
        dotNetRef.invokeMethodAsync(methodName, data);
    });

    activeSubscriptions.set(subId, unsubscribe);
    return subId;
}

/**
 * Subscribes to real-time updates on a collection query.
 */
export function subscribeToCollection(collectionPath, filters, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    let q = collection(db, collectionPath);
    const constraints = [];

    if (filters?.whereField) {
        constraints.push(where(filters.whereField, filters.whereOp, filters.whereValue));
    }
    if (filters?.orderByField) {
        constraints.push(orderBy(filters.orderByField, filters.orderDir ?? 'asc'));
    }

    const unsubscribe = onSnapshot(query(q, ...constraints), (snap) => {
        const docs = snap.docs.map(d => ({ id: d.id, ...d.data() }));
        dotNetRef.invokeMethodAsync(methodName, docs);
    });

    activeSubscriptions.set(subId, unsubscribe);
    return subId;
}

export function unsubscribe(subscriptionId) {
    const unsub = activeSubscriptions.get(subscriptionId);
    if (unsub) {
        unsub();
        activeSubscriptions.delete(subscriptionId);
    }
}
```

---

## 13. Complete DI Registration — Final `Program.cs`

With all decisions resolved, here is the definitive service registration:

```csharp
// Program.cs
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

// ── HttpClient ──
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ══════════════════════════════════════════════════════════════
// STATE CONTAINERS (Scoped = one instance per browser tab)
// ══════════════════════════════════════════════════════════════
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

// ══════════════════════════════════════════════════════════════
// JS INTEROP BRIDGES (Interface → Implementation)
// ══════════════════════════════════════════════════════════════
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

// ══════════════════════════════════════════════════════════════
// ORCHESTRATORS (Workflow coordinators)
// ══════════════════════════════════════════════════════════════
builder.Services.AddScoped<NetworkTestOrchestrator>();
builder.Services.AddScoped<MouseTestOrchestrator>();
builder.Services.AddScoped<KeyboardTestOrchestrator>();
builder.Services.AddScoped<AudioTestOrchestrator>();
builder.Services.AddScoped<MonitorTestOrchestrator>();
builder.Services.AddScoped<ComputeTestOrchestrator>();
builder.Services.AddScoped<ReportOrchestrator>();

// ══════════════════════════════════════════════════════════════
// APPLICATION SERVICES
// ══════════════════════════════════════════════════════════════
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminGuardService>();

await builder.Build().RunAsync();
```

---

## ✅ All Open Questions Resolved

| # | Question | Decision | Impact |
|---|---|---|---|
| Q1 | Chatbot Provider | **OpenRouter `gpt-oss-120b:free`** with custom chat UI | `ChatWindow.razor` is a fully custom component; system prompt guardrails enforced in JS layer; no API key required |
| Q2 | Speed Test Servers | **LibreSpeed public servers** | `network-interop.js` fetches server list from LibreSpeed API with hardcoded fallback; servers plotted on LeafletJS/OSM map |
| Q3 | Stripe Scope | **One-time donations** via Stripe Payment Links | No backend, no webhooks — admin pre-creates Payment Links in Stripe Dashboard; client redirects with `window.open()` |
| Q4 | Firebase Config | **Project `diagnostic-platform-blazor`** fully provisioned | Config embedded in `firebase-config.js`; Auth + Firestore + Analytics initialized; import map in `index.html` |
