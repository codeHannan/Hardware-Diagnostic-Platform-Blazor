# BenchRig — Software Design Document

**Project:** BenchRig — Hardware & Network Diagnostic Platform
**Type:** Single-project Blazor WebAssembly (.NET 10), Clean Architecture, Firebase backend
**Document:** Design specification — requirements (user-story format) + UML/ER/sequence diagrams

> The diagrams are written in **Mermaid**. They render directly on GitHub, in the VS Code "Markdown Preview
> Mermaid" extension, and at <https://mermaid.live> (where you can also export PNG/SVG/PDF for submission).

---

## 1. Introduction

### 1.1 Purpose
BenchRig is a client-side web application that lets anyone benchmark and diagnose every part of their PC —
network, mouse, keyboard, audio, monitor, and compute (CPU/GPU/memory) — directly in the browser with **no
installs and no drivers**. Results compile into a shareable report. A community layer (forum, knowledge base,
support tickets) and an AI assistant help users interpret and resolve issues.

### 1.2 Scope
- **Diagnostics (client-side):** Network, Mouse (11 tests), Keyboard (8 tests), Audio (4 tests),
  Monitor (11 tests), Compute (CPU + GPU + Memory).
- **Workspace:** Diagnostic Report, Community Forum, Knowledge Base, Support, Donations.
- **Platform:** Authentication (email/password + Google), role-based access (User / Admin), AI chatbot.
- **Out of scope:** native OS-level sensors (browser cannot read raw hardware), payment processing
  (delegated to Stripe), and certified/absolute benchmark certification (scores are *approximate indices*).

### 1.3 Definitions
| Term | Meaning |
|---|---|
| **Diagnostic suite** | A group of related tests for one component (e.g. the Mouse suite). |
| **Orchestrator** | A service that drives a multi-step diagnostic flow and streams results into state. |
| **Bridge** | A `Core` interface implemented by a JS-interop class to reach a browser/Web API. |
| **State container** | An observable in-memory store UI components subscribe to (`OnChange`). |
| **Report** | A compiled, exportable summary of all diagnostics run in the current session. |

---

## 2. System Architecture

BenchRig follows **Clean Architecture**: the `Core` layer is pure C# (no browser dependencies); browser/Web
APIs are reached only through `Core` interfaces ("bridges") implemented in the `Services/JsInterop` layer.

```mermaid
flowchart TB
    subgraph UI["Presentation — Pages & Components (Blazor .razor)"]
        P[Diagnostic Pages / Workspace Pages / Admin]
        C[Shared & Feature Components]
        CB[Chat Assistant]
    end
    subgraph SVC["Services (Application)"]
        ST[State Containers]
        OR[Orchestrators]
        AU[AuthService / AdminGuardService]
        JS[JsInterop Bridge Implementations]
    end
    subgraph CORE["Core (Domain) — pure C#"]
        M[Models / Records]
        I[Interfaces / Bridges]
        E[Engines — score & metric calculators]
        K[Constants]
    end
    subgraph JSW["wwwroot/js — ES modules"]
        JM[network · mouse · keyboard · audio · monitor · compute · firestore · auth · chatbot · stripe · leaflet]
    end
    subgraph EXT["External services"]
        FB[(Firebase Auth + Firestore)]
        AI[Firebase AI Logic / Gemini]
        CF[Cloudflare Speed Edge]
        SP[Stripe Payment Links]
    end

    UI --> ST
    UI --> OR
    UI --> AU
    OR --> I
    OR --> ST
    AU --> I
    JS -. implements .-> I
    JS --> JM
    OR --> E
    ST --> M
    JM --> FB
    JM --> AI
    JM --> CF
    JM --> SP
```

**Tech stack:** Blazor WASM (.NET 10) · C# · Firebase JS SDK 11.10 (Auth, Firestore, AI) · WebGL2 ·
Web Audio API · Web Workers · Pointer/Keyboard events · Cloudflare `speed.cloudflare.com` · Leaflet · Stripe
Payment Links · Mermaid (docs).

---

## 3. Actors

| Actor | Description |
|---|---|
| **Guest** | Unauthenticated visitor. Can run **all** client-side diagnostics, compile/export reports, browse the forum & knowledge base, chat with the AI assistant, and donate. |
| **Registered User** | A signed-in account (extends Guest). Can create/edit/delete own forum posts, comment, rate, submit support tickets, and reply to their own tickets. |
| **Admin** | Extends Registered User. Can moderate any post/comment, respond to and triage any ticket, and author/publish/delete knowledge-base articles. |
| **Firebase** (secondary) | Authentication + Firestore database. |
| **Gemini AI** (secondary) | LLM backing the assistant via Firebase AI Logic. |
| **Cloudflare** (secondary) | CORS-enabled edge endpoints for speed/latency measurement. |
| **Stripe** (secondary) | Hosted donation checkout via Payment Links. |

---

## 4. Functional Requirements (User Stories)

> Format: **As a** ⟨role⟩, **I want** ⟨capability⟩, **so that** ⟨benefit⟩. AC = acceptance criteria.

### Epic A — Authentication & Accounts
- **FR-A1** — As a **Guest**, I want to register with email/password (or Google), so that I can access
  community features.
  *AC:* a `users/{uid}` profile is created with `role = "user"`; duplicate/invalid emails are rejected with a
  friendly message.
- **FR-A2** — As a **Registered User**, I want to sign in and out, so that my identity is attached to my posts
  and tickets.
- **FR-A3** — As a **Registered User**, I want my session to persist across reloads, so that I'm not signed
  out unexpectedly.
- **FR-A4** — As an **Admin**, I want elevated access gated by my `role`, so that only I can reach admin tools.
  *AC:* non-admins visiting `/admin/*` see "Access denied".

### Epic B — Network Diagnostics
- **FR-B1** — As a **Guest**, I want to measure download, upload, ping, and jitter against Cloudflare's edge,
  so that I know my real connection speed.
  *AC:* live speed streams onto a gauge during the test; final values reflect a warm-up-excluded steady state.
- **FR-B2** — As a **Guest**, I want a clear description of the test server (Cloudflare anycast edge), so that
  I understand what my result represents.
- **FR-B3** — As a **Guest**, I want detailed connection info (IP, ISP, ASN, location, device, DoH), so that I
  can inspect my network environment.
- **FR-B4** — As a **Guest**, I want idle/loaded latency, jitter, packet loss, and a bufferbloat grade, so
  that I can judge connection stability.

### Epic C — Peripheral Diagnostics (Mouse · Keyboard · Audio)
- **FR-C1** — As a **Guest**, I want a mouse suite (CPS, Kohi, jitter/double-click, button registration,
  polling rate, DPI, jitter/dead-zone, accuracy, drag-and-drop), so that I can verify my mouse hardware.
  *AC:* DPI uses Pointer Lock for accurate counts; accuracy/drag use stage-relative coordinates; fast-click
  tests lock out for 2 s after finishing.
- **FR-C2** — As a **Guest**, I want a keyboard suite (key registration, chatter, key-rate, 60-second typing
  test with logical random text, NKRO rollover, ghosting, stuck-key, special-key), so that I can verify my
  keyboard.
- **FR-C3** — As a **Guest**, I want an audio suite (multi-channel playback, balance, mic loopback, echo &
  latency), so that I can verify my speakers and microphone.

### Epic D — Display Diagnostics (Monitor)
- **FR-D1** — As a **Guest**, I want fullscreen monitor test patterns (dead pixel, uniformity, gradients,
  contrast, sharpness, viewing angles, backlight bleed, ghosting, gamma, response time), so that I can assess
  panel quality.
- **FR-D2** — As a **Guest**, I want a live refresh-rate measurement (Hz, range, jitter, dropped frames,
  compliance), so that I can confirm my monitor runs at its rated rate.

### Epic E — Compute Benchmarks
- **FR-E1** — As a **Guest**, I want CPU (single + multi-threaded via Web Workers), GPU (Volume-Shader
  volumetric ray-marcher), and memory benchmarks with selectable presets, so that I can score my hardware.
- **FR-E2** — As a **Guest**, I want my scores compared against reference GPUs/CPUs, so that I have context.
  *AC:* GPU FPS is measured with a real GPU sync (`readPixels`); scores are normalized per preset so all modes
  report the same tier; references mirror UserBenchmark ordering.

### Epic F — Diagnostic Report
- **FR-F1** — As a **Guest**, I want to compile every diagnostic I ran this session into one report, so that I
  can review everything together.
- **FR-F2** — As a **Guest**, I want to export the report as Markdown, so that I can share or archive it.

### Epic G — Community Forum
- **FR-G1** — As a **Guest**, I want to browse, search, sort, and filter posts by tag, so that I can find
  relevant discussions.
- **FR-G2** — As a **Registered User**, I want to create posts (with markdown + tags), so that I can ask
  questions or share fixes.
- **FR-G3** — As a **Registered User**, I want to comment with threaded replies (up to depth 3) and rate posts
  (1–5 stars), so that I can participate.
- **FR-G4** — As a **Registered User**, I want to edit/delete my own posts and comments, so that I control my
  content. *AC:* Admins may delete any post/comment (moderation).
- **FR-G5** — As a **Guest**, I want feed cards to show accurate comment counts and average ratings, so that
  sorting by "Top rated"/"Most discussed" is meaningful.

### Epic H — Knowledge Base
- **FR-H1** — As a **Guest**, I want to search/sort/filter published articles by category and tag, with
  reading time and related articles, so that I can self-serve solutions.
- **FR-H2** — As a **Guest**, I want shareable article URLs (`/solutions/{id}`), so that I can bookmark/link.
- **FR-H3** — As an **Admin**, I want to create, edit, tag, categorize (including custom categories), preview,
  publish/unpublish, and delete articles, so that I can curate the knowledge base.

### Epic I — Support Tickets
- **FR-I1** — As a **Registered User**, I want to submit a ticket (subject + body), so that I can get help.
- **FR-I2** — As a **Registered User**, I want to view my tickets with status (Open / In Progress / Resolved),
  search, filter, and a response count, so that I can track them.
- **FR-I3** — As a **Registered User**, I want to read staff responses and **reply** to my active
  (non-resolved) tickets, so that we can hold a two-way conversation.
- **FR-I4** — As an **Admin**, I want a triage queue (status stats, search) and a detail panel to set status
  and post responses, so that I can resolve tickets. *AC:* a staff reply moves an Open ticket to In Progress.

### Epic J — Donations
- **FR-J1** — As a **Guest**, I want to donate via Stripe (one-time or monthly, preset or custom amount), so
  that I can support the project. *AC:* placeholder/unconfigured links never open a broken tab — a friendly
  "not set up yet" message with an appreciation note is shown instead.

### Epic K — AI Assistant
- **FR-K1** — As a **Guest**, I want to chat with a diagnostics assistant (Firebase AI / Gemini), so that I
  can get help interpreting results. *AC:* the assistant only answers hardware/network topics; replies render
  as markdown; errors degrade gracefully with no key required in the client.

### Epic L — Administration
- **FR-L1** — As an **Admin**, I want a console linking the tickets queue and KB editor, so that I have one
  moderation entry point.

---

## 5. Non-Functional Requirements (User Stories)

### Performance
- **NFR-P1** — As a **user**, I want the app to remain responsive while a benchmark runs, so that the tab
  never freezes. *AC:* heavy loops yield to the event loop (~16 ms); no single GPU frame risks a watchdog reset.
- **NFR-P2** — As a **user**, I want measurements to be accurate, so that results reflect reality. *AC:* speed
  excludes warm-up; GPU uses a real GPU sync; latency uses steady-state windows.

### Usability & Responsiveness
- **NFR-U1** — As a **mobile user**, I want every page to adapt to my screen, so that I can use the app on a
  phone. *AC:* the sidebar collapses into a slide-in drawer with a backdrop below 980 px.
- **NFR-U2** — As a **user**, I want clear, professional copy and consistent loading/empty/error states, so
  that the app feels polished and trustworthy.

### Reliability & Resilience
- **NFR-R1** — As a **user**, I want the app to fail fast with a clear message when the backend is
  unreachable, so that it never hangs. *AC:* Firestore ops time out at 12 s and show a retry card.
- **NFR-R2** — As a **user**, I want diagnostics to work even if the backend is down, so that the core value
  (local testing) is always available.

### Security & Privacy
- **NFR-S1** — As an **Admin**, I want server-side security rules (not just UI checks) to enforce access, so
  that data is protected. *AC:* Firestore rules enforce owner/admin permissions for every collection.
- **NFR-S2** — As a **user**, I want my card details handled only by Stripe and the AI to require no client
  key, so that no secrets live in the browser.
- **NFR-S3** — As a **user**, I want my location/IP gathered only for the network panel and never persisted,
  so that my privacy is respected.

### Portability & Maintainability
- **NFR-M1** — As a **developer**, I want browser APIs isolated behind `Core` interfaces, so that the domain
  stays testable and the backend is swappable (e.g. Firestore → SQL via repository adapters).
- **NFR-M2** — As a **developer**, I want the design to cascade from CSS design tokens + global classes, so
  that re-theming is centralized.

### Accessibility & Compatibility
- **NFR-A1** — As a **keyboard/screen user**, I want focusable controls, sensible contrast, and Esc-to-exit on
  fullscreen tests, so that the app is usable for everyone.
- **NFR-A2** — As a **user**, I want the app to run on any modern evergreen browser with WebGL2/Web Audio, so
  that I don't need special software.

---

## 6. UML Use Case Diagram

```mermaid
flowchart LR
    Guest(["👤 Guest"])
    User(["👤 Registered User"])
    Admin(["👤 Admin"])
    FB[["Firebase"]]
    AI[["Gemini AI"]]
    CF[["Cloudflare"]]
    SP[["Stripe"]]

    User -. extends .-> Guest
    Admin -. extends .-> User

    subgraph SYS["BenchRig System"]
        UC1(["Run Network Test"])
        UC2(["Run Peripheral Tests"])
        UC3(["Run Monitor Tests"])
        UC4(["Run Compute Benchmarks"])
        UC5(["Compile & Export Report"])
        UC6(["Browse Forum / KB"])
        UC7(["Use AI Assistant"])
        UC8(["Donate"])
        UC9(["Register / Login"])
        UC10(["Create Post / Comment / Rate"])
        UC11(["Submit & Reply to Tickets"])
        UC12(["Respond to & Triage Tickets"])
        UC13(["Manage KB Articles"])
        UC14(["Moderate Content"])
    end

    Guest --- UC1
    Guest --- UC2
    Guest --- UC3
    Guest --- UC4
    Guest --- UC5
    Guest --- UC6
    Guest --- UC7
    Guest --- UC8
    Guest --- UC9

    User --- UC10
    User --- UC11

    Admin --- UC12
    Admin --- UC13
    Admin --- UC14

    UC1 -. include .-> CF
    UC7 -. include .-> AI
    UC8 -. include .-> SP
    UC9 -. include .-> FB
    UC10 -. include .-> FB
    UC11 -. include .-> FB
    UC12 -. include .-> FB
    UC13 -. include .-> FB
```

---

## 7. UML Domain Model

Conceptual entities and relationships (attributes only, no behaviour).

```mermaid
classDiagram
    class User {
        +string uid
        +string email
        +string displayName
        +Role role
    }
    class DiagnosticReport {
        +string id
        +string ownerUid
        +string summary
        +DateTime createdAt
    }
    class ReportSection {
        +string title
        +Map metrics
    }
    class ForumPost {
        +string id
        +string title
        +string body
        +string[] tags
        +double avgRating
        +int commentCount
        +DateTime createdAt
    }
    class Comment {
        +string id
        +string body
        +string parentCommentId
        +int depth
        +DateTime createdAt
    }
    class Rating {
        +string userId
        +int stars
    }
    class SolutionArticle {
        +string id
        +string title
        +string body
        +string category
        +string[] tags
        +bool isPublished
    }
    class SupportTicket {
        +string id
        +string subject
        +string body
        +TicketStatus status
        +DateTime createdAt
    }
    class TicketResponse {
        +string id
        +string body
        +bool isStaff
        +DateTime createdAt
    }
    class ChatMessage {
        +string role
        +string content
        +DateTime timestamp
    }
    class DiagnosticServer {
        +string key
        +string name
        +string location
    }

    User "1" --> "*" DiagnosticReport : owns
    User "1" --> "*" ForumPost : authors
    User "1" --> "*" Comment : writes
    User "1" --> "*" SupportTicket : opens
    User "1" --> "*" TicketResponse : posts
    DiagnosticReport "1" *-- "*" ReportSection : contains
    ForumPost "1" *-- "*" Comment : has
    ForumPost "1" *-- "*" Rating : has
    Comment "0..1" --> "*" Comment : replies-to
    SupportTicket "1" *-- "*" TicketResponse : has
    User "1" --> "*" SolutionArticle : authors
    User "1" --> "*" ChatMessage : sends
```

---

## 8. UML Class Diagram (Design Level)

Representative slice of the Clean-Architecture types (Network suite shown as the exemplar; the same pattern
repeats for Mouse/Keyboard/Audio/Monitor/Compute).

```mermaid
classDiagram
    direction LR

    class StateContainerBase {
        <<abstract>>
        +event OnChange
        #NotifyStateChanged()
        #NotifyThrottled()
    }
    class NetworkDiagState {
        +SpeedPhase Phase
        +double DownloadMbps
        +double UploadMbps
        +double PingMs
        +SetDownload()
        +SetUpload()
        +CompleteTest()
    }
    class AuthStateContainer {
        +UserProfile CurrentUser
        +bool IsLoggedIn
        +bool IsAdmin
        +bool IsInitialized
    }
    class ForumState
    class ChatbotState

    class NetworkTestOrchestrator {
        +RunFullTestAsync()
        +LoadNetworkInfoAsync()
        +OnDownloadProgress(mbps)
        +OnUploadProgress(mbps)
    }
    class ComputeTestOrchestrator {
        +RunCpuAsync()
        +RunGpuAsync()
        +RunMemoryAsync()
    }
    class ReportOrchestrator {
        +Compile()
        +ToMarkdown()
    }
    class AuthService {
        +InitializeAsync()
        +SignInWithEmailAsync()
        +RegisterWithEmailAsync()
        +SignOutAsync()
    }

    class IJsNetworkBridge {
        <<interface>>
        +MeasureLatencyAsync()
        +MeasureDownloadAsync()
        +MeasureUploadAsync()
        +GetNetworkInfoAsync()
    }
    class IJsFirestoreBridge {
        <<interface>>
        +AddDocumentAsync()
        +QueryCollectionAsync()
        +UpdateDocumentAsync()
        +CountCollectionAsync()
    }
    class IJsComputeBridge {
        <<interface>>
        +RunGpuBenchmarkAsync()
        +RunCpuBenchmarkAsync()
    }
    class ModuleInteropBase {
        <<abstract>>
        #ModuleAsync()
    }
    class NetworkJsInterop
    class FirestoreJsInterop

    class GpuScoreCalculator {
        +Score(fps,w,h,steps) double
    }
    class CpuScoreCalculator
    class JitterCalculator

    class NetworkDiagPage
    class SpeedTestPanel
    class ForumPage

    StateContainerBase <|-- NetworkDiagState
    StateContainerBase <|-- AuthStateContainer
    StateContainerBase <|-- ForumState
    StateContainerBase <|-- ChatbotState

    ModuleInteropBase <|-- NetworkJsInterop
    ModuleInteropBase <|-- FirestoreJsInterop
    IJsNetworkBridge <|.. NetworkJsInterop
    IJsFirestoreBridge <|.. FirestoreJsInterop

    NetworkTestOrchestrator --> IJsNetworkBridge
    NetworkTestOrchestrator --> NetworkDiagState
    ComputeTestOrchestrator --> IJsComputeBridge
    ComputeTestOrchestrator --> GpuScoreCalculator
    ComputeTestOrchestrator --> CpuScoreCalculator
    AuthService --> AuthStateContainer

    NetworkDiagPage --> NetworkTestOrchestrator
    SpeedTestPanel --> NetworkDiagState
    ForumPage --> IJsFirestoreBridge
    ForumPage --> ForumState
```

---

## 9. Entity-Relationship Diagram (Firestore data model)

Firestore is a document store; collections/subcollections are modeled here as entities. `PK` = document id,
`FK` = reference field. Subcollections are owned (identifying) relationships.

```mermaid
erDiagram
    USERS ||--o{ FORUM_POSTS : authors
    USERS ||--o{ SOLUTION_ARTICLES : authors
    USERS ||--o{ SUPPORT_TICKETS : opens
    USERS ||--o{ DIAGNOSTIC_REPORTS : owns
    FORUM_POSTS ||--o{ COMMENTS : contains
    FORUM_POSTS ||--o{ RATINGS : contains
    SUPPORT_TICKETS ||--o{ RESPONSES : contains

    USERS {
        string uid PK
        string email
        string displayName
        string role "user | admin"
        datetime createdAt
    }
    FORUM_POSTS {
        string id PK
        string authorUid FK
        string authorName
        string title
        string body
        array tags
        double avgRating
        int ratingCount
        int commentCount
        datetime createdAt
        datetime updatedAt
    }
    COMMENTS {
        string id PK
        string postId FK
        string authorUid FK
        string body
        string parentCommentId FK
        int depth
        datetime createdAt
    }
    RATINGS {
        string userId PK
        string postId FK
        int stars
        datetime createdAt
    }
    SOLUTION_ARTICLES {
        string id PK
        string authorUid FK
        string title
        string body
        string category
        array tags
        bool isPublished
        datetime createdAt
        datetime updatedAt
    }
    SUPPORT_TICKETS {
        string id PK
        string authorUid FK
        string authorName
        string subject
        string body
        string status "open | in_progress | resolved"
        datetime createdAt
        datetime updatedAt
    }
    RESPONSES {
        string id PK
        string ticketId FK
        string authorUid FK
        string authorName
        bool isStaff
        string body
        datetime createdAt
    }
    DIAGNOSTIC_REPORTS {
        string id PK
        string ownerUid FK
        string summary
        json sections
        datetime createdAt
    }
    APP_CONFIG {
        string document PK
        json settings
    }
```

---

## 10. Sequence Diagrams

### 10.1 Run Network Speed Test
```mermaid
sequenceDiagram
    actor U as User
    participant Panel as SpeedTestPanel
    participant Orch as NetworkTestOrchestrator
    participant State as NetworkDiagState
    participant Bridge as IJsNetworkBridge / NetworkJsInterop
    participant JS as network-interop.js
    participant CF as Cloudflare Edge

    U->>Panel: Click GO
    Panel->>Orch: RunFullTestAsync()
    Orch->>State: BeginTest()
    Orch->>Bridge: MeasureLatencyAsync(30)
    Bridge->>JS: measureLatency()
    JS->>CF: ping requests
    CF-->>JS: RTTs
    JS-->>Orch: latency samples
    Orch->>State: SetLatency(min,avg,max,jitter)
    Orch->>Bridge: MeasureDownloadAsync(12s, callback)
    Bridge->>JS: measureDownload()
    loop every 100 ms
        JS->>CF: stream chunks (6 parallel)
        JS-->>Orch: OnDownloadProgress(mbps)
        Orch->>State: SetLiveMbps()
        State-->>Panel: OnChange (gauge updates)
    end
    JS-->>Orch: {mbps, bloatMs}
    Orch->>State: SetDownload()
    Orch->>Bridge: MeasureUploadAsync(8s, callback)
    Bridge->>JS: measureUpload() (XHR/fetch + sync)
    JS-->>Orch: upload mbps
    Orch->>State: CompleteTest(result, stability)
    State-->>Panel: OnChange (final result shown)
```

### 10.2 Run GPU Benchmark (Volume Shader)
```mermaid
sequenceDiagram
    actor U as User
    participant Panel as GpuBenchPanel
    participant Orch as ComputeTestOrchestrator
    participant Calc as GpuScoreCalculator
    participant State as ComputeDiagState
    participant JS as compute-interop.js
    participant GPU as WebGL2 GPU

    U->>Panel: Select preset, click Run
    Panel->>Orch: RunGpuAsync(canvas,w,h,steps,ms)
    Orch->>State: BeginRun()
    Orch->>JS: runGpuBenchmark(...)
    loop until duration (yield each ~16ms)
        JS->>GPU: drawArrays (volumetric ray-march)
        JS->>GPU: readPixels(1x1) forces real GPU sync
        GPU-->>JS: pixel (frame complete)
        JS-->>Orch: OnProgress(%)
        Orch->>State: SetProgress()
        State-->>Panel: OnChange (ring updates)
    end
    JS-->>Orch: {avgFps, frames}
    Orch->>Calc: Score(avgFps,w,h,steps)
    Calc-->>Orch: normalized score
    Orch->>State: CompleteGpu(result)
    State-->>Panel: OnChange (score + reference bars)
```

### 10.3 Post a Threaded Forum Reply
```mermaid
sequenceDiagram
    actor U as Registered User
    participant Page as ForumPostPage
    participant Thread as CommentThread
    participant FS as IJsFirestoreBridge
    participant JS as firestore-interop.js
    participant DB as Firestore

    U->>Thread: Click "Reply", type, submit
    Thread->>Page: OnReply(parentId, depth, body)
    Page->>FS: AddDocumentAsync(comments, {body, authorUid, parentCommentId, depth})
    FS->>JS: addDocument()
    JS->>DB: create comment (rules: depth<=3, author==uid)
    DB-->>JS: ok / denied
    JS-->>Page: result
    Page->>FS: QueryCollectionAsync(comments, orderBy createdAt)
    FS->>JS: queryCollection()
    JS->>DB: read comments
    DB-->>JS: comments
    JS-->>Page: comments
    Page->>Page: BuildTree() (parent→children, orphans→roots)
    Page-->>U: re-render nested thread
```

### 10.4 Support Ticket Lifecycle (User ⇄ Admin)
```mermaid
sequenceDiagram
    actor U as User
    actor A as Admin
    participant SP as SupportPage
    participant TD as TicketDetail
    participant AP as AdminTicketsPage
    participant FS as IJsFirestoreBridge
    participant DB as Firestore

    U->>SP: Submit ticket (subject, body)
    SP->>FS: AddDocumentAsync(support_tickets, status="open")
    FS->>DB: create ticket
    A->>AP: Open ticket in queue
    A->>TD: Post staff reply
    TD->>FS: AddDocumentAsync(responses, isStaff=true)
    FS->>DB: create response
    TD->>FS: UpdateDocumentAsync(ticket, status="in_progress")
    FS->>DB: update status
    U->>SP: Open ticket, read staff reply
    U->>TD: Post reply (active ticket)
    TD->>FS: AddDocumentAsync(responses, isStaff=false)
    FS->>DB: create response (rules: owner allowed)
    A->>TD: Set status = "resolved"
    TD->>FS: UpdateDocumentAsync(ticket, status="resolved")
    FS->>DB: update status
```

### 10.5 AI Assistant Message
```mermaid
sequenceDiagram
    actor U as User
    participant CW as ChatWindow
    participant State as ChatbotState
    participant Bridge as IJsChatbotBridge
    participant JS as chatbot-interop.js
    participant AI as Firebase AI (Gemini)

    U->>CW: Type message, Send
    CW->>State: AddUserMessage(text)
    CW->>Bridge: SendMessageAsync(text, history)
    Bridge->>JS: sendChatMessage(text, history)
    JS->>AI: startChat(history).sendMessage(text)
    alt success
        AI-->>JS: response text
        JS-->>CW: reply
        CW->>State: AddAssistantMessage(reply)
    else error / not enabled
        AI-->>JS: error
        JS-->>CW: throw
        CW->>State: SetError("Couldn't reach the assistant…")
    end
    State-->>CW: OnChange (render markdown reply)
```

### 10.6 Register / Login
```mermaid
sequenceDiagram
    actor G as Guest
    participant Panel as Login/Register Panel
    participant Svc as AuthService
    participant Bridge as IJsFirebaseAuthBridge
    participant JS as auth.js
    participant FB as Firebase Auth
    participant DB as Firestore
    participant State as AuthStateContainer

    G->>Panel: Enter credentials, submit
    Panel->>Svc: RegisterWithEmailAsync() / SignInWithEmailAsync()
    Svc->>Bridge: register/signIn
    Bridge->>JS: createUser / signIn
    JS->>FB: auth request
    FB-->>JS: credential / error
    opt new account
        JS->>DB: create users/{uid} {role:"user"}
    end
    JS-->>Svc: UserProfile
    Svc->>State: set CurrentUser, IsInitialized
    State-->>Panel: OnChange (redirect home, UI reflects login)
```

---

## 11. Traceability (requirements → diagrams)

| Requirement | Use Case | Sequence | Data |
|---|---|---|---|
| FR-A* | UC9 | §10.6 | USERS |
| FR-B* | UC1 | §10.1 | — (transient) |
| FR-C/D/E* | UC2/UC3/UC4 | §10.2 | — / DIAGNOSTIC_REPORTS |
| FR-F* | UC5 | §10.2 | DIAGNOSTIC_REPORTS |
| FR-G* | UC6/UC10/UC14 | §10.3 | FORUM_POSTS, COMMENTS, RATINGS |
| FR-H* | UC6/UC13 | — | SOLUTION_ARTICLES |
| FR-I* | UC11/UC12 | §10.4 | SUPPORT_TICKETS, RESPONSES |
| FR-J* | UC8 | — | — (Stripe) |
| FR-K* | UC7 | §10.5 | — (ephemeral) |

---

*End of design document.*
