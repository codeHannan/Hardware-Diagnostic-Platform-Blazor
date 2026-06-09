<div align="center">

<img src="BenchRig.App/wwwroot/favicon.svg" alt="BenchRig logo" width="120" />

# ⚙️ BenchRig

### Hardware &amp; Network Diagnostic Platform — entirely in your browser

*Bench your mouse, keyboard, audio, display, CPU and connection with zero installs.*
*No drivers. No downloads. Open a tab, run the rig, save the report.*

<br/>

[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Firebase](https://img.shields.io/badge/Firebase-Auth%20%26%20Firestore-FFCA28?logo=firebase&logoColor=black)](https://firebase.google.com/)
[![Cloudflare Workers](https://img.shields.io/badge/Cloudflare-Workers-F38020?logo=cloudflare&logoColor=white)](https://workers.cloudflare.com/)
[![PWA](https://img.shields.io/badge/PWA-installable-5A0FC8?logo=pwa&logoColor=white)](#-progressive-web-app)
[![Status](https://img.shields.io/badge/status-active-success.svg)](#)
[![License](https://img.shields.io/badge/license-proprietary-red.svg)](#-license--usage)

<br/>

**[Diagnostics](#-diagnostic-suite) · [Features](#-platform-features) · [Architecture](#-architecture) · [Quick Start](#-quick-start) · [Deploy](#-build--deploy)**

</div>

---

## 📑 Table of Contents

- [Overview](#-overview)
- [Why BenchRig?](#-why-benchrig)
- [Diagnostic Suite](#-diagnostic-suite)
- [Platform Features](#-platform-features)
- [Architecture](#-architecture)
- [How a Test Runs](#-how-a-test-runs)
- [Tech Stack](#-tech-stack)
- [Application Routes](#-application-routes)
- [Project Structure](#-project-structure)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [Build &amp; Deploy](#-build--deploy)
- [Progressive Web App](#-progressive-web-app)
- [Security Model](#-security-model)
- [Roadmap](#-roadmap)
- [FAQ](#-faq)
- [Author](#-author)
- [License &amp; Usage](#-license--usage)

---

## ✨ Overview

**BenchRig** is a full-featured diagnostic platform built entirely on **Blazor WebAssembly (.NET 10)** that runs
100% client-side in any modern browser. It turns a browser tab into a hardware lab: probe your peripherals,
stress-test your CPU, benchmark your connection, then save a consolidated diagnostic report to the cloud — without
installing a single driver or executable.

But BenchRig is more than a test bench. It ships with a **community forum**, a curated **solutions knowledge base**,
a **support-ticket helpdesk**, an **AI troubleshooting assistant**, **donations**, and a full **admin console** —
making it a complete, production-shaped web product rather than a classroom demo.

<div align="center">

> 🎓 Engineered as a Fourth-Semester **Visual Programming** project at **Air University (BSCS)** — built like real SaaS.

</div>

---

## 💡 Why BenchRig?

| | Traditional diagnostic tools | **BenchRig** |
| :-- | :-- | :-- |
| **Install** | Downloads, drivers, admin rights | ❌ None — runs in the browser |
| **Platform** | Usually Windows-only `.exe` | ✅ Any OS with a modern browser |
| **Trust** | Opaque native binaries | ✅ Open, sandboxed web APIs |
| **Reports** | Local files you lose | ✅ Saved to your account, anywhere |
| **Help** | You're on your own | ✅ AI assistant + forum + helpdesk |
| **Updates** | Re-download every release | ✅ Always the latest, instantly |

---

## 🧪 Diagnostic Suite

Six independent test modules, each backed by a clean **Engine → Orchestrator → State → UI** pipeline so the
diagnostic logic is pure C#, fully decoupled from the browser, and unit-testable.

<table>
<tr>
<td width="50%" valign="top">

### 🖱️ Mouse
- Click & double-click accuracy
- Button mapping (L / R / middle / side)
- Scroll-wheel behavior
- **Polling rate**, jitter & latency

### ⌨️ Keyboard
- Full-layout key coverage
- **N-key roll-over** & ghosting detection
- Stuck-key & repeat checks
- Per-key press latency

### 🔊 Audio
- Left / right speaker channel test
- Microphone capture & level meter
- Frequency sweep + waveform analysis

</td>
<td width="50%" valign="top">

### 🖥️ Monitor
- Dead / stuck pixel hunt
- Color banding & gradient uniformity
- Response-time & ghosting patterns

### 🧠 Compute
- CPU stress workloads
- Throughput benchmarking
- Sustained-performance scoring

### 🌍 Network
- Download / upload throughput
- Ping & jitter
- **Geo-located** server selection (Leaflet map)
- Self-hostable Cloudflare Worker backend

</td>
</tr>
</table>

> 🧾 Every module feeds the **Report Builder**, which compiles a single, shareable diagnostic report saved to Firestore.

---

## 🚀 Platform Features

<table>
<tr><td>📋</td><td><b>Diagnostic Reports</b></td><td>Consolidated, account-bound reports compiled from every test you run.</td></tr>
<tr><td>🤖</td><td><b>AI Assistant</b></td><td>In-app chatbot (powered by <b>Gemini</b>) that interprets results and walks you through fixes.</td></tr>
<tr><td>💬</td><td><b>Community Forum</b></td><td>Threaded posts, nested comments (up to 3 deep) and 1–5 star ratings.</td></tr>
<tr><td>📚</td><td><b>Solutions Knowledge Base</b></td><td>Curated, admin-authored articles for common hardware problems.</td></tr>
<tr><td>🎫</td><td><b>Support Helpdesk</b></td><td>Users open tickets; staff reply in a secured, role-gated thread.</td></tr>
<tr><td>🛡️</td><td><b>Admin Console</b></td><td>Dashboard, ticket management and solution-article authoring.</td></tr>
<tr><td>🔐</td><td><b>Authentication</b></td><td>Firebase email/password auth with role-based access (user / admin).</td></tr>
<tr><td>🗺️</td><td><b>Geo-aware Tests</b></td><td>Leaflet map + location picker to benchmark the nearest server.</td></tr>
<tr><td>💳</td><td><b>Donations</b></td><td>Stripe-powered "buy me a coffee" support flow.</td></tr>
<tr><td>📱</td><td><b>Installable PWA</b></td><td>Offline-first service worker; installs to desktop or phone.</td></tr>
</table>

---

## 🏗️ Architecture

BenchRig follows a **layered, dependency-injected architecture**. All browser-specific work hides behind interfaces
(`IJs*Bridge`), so the core diagnostic logic stays platform-agnostic, mockable, and testable. State is held in
**scoped containers** (one instance per browser tab), and every test is driven by a dedicated **orchestrator**.

```
╔══════════════════════════════════════════════════════════════╗
║  Pages / Components        Razor UI (routable + reusable)    ║
╠══════════════════════════════════════════════════════════════╣
║  State Containers          scoped per browser tab            ║
╠══════════════════════════════════════════════════════════════╣
║  Orchestrators             coordinate a full test run        ║
╠══════════════════════════════════════════════════════════════╣
║  Engines                   pure C# diagnostic logic          ║
╠══════════════════════════════════════════════════════════════╣
║  JS Interop Bridges        IJs*Bridge → native browser APIs  ║
╚══════════════════════════════════════════════════════════════╝
              │                                  │
              ▼                                  ▼
     Firebase Auth / Firestore        Cloudflare Worker
     (identity, reports, forum,       (CORS-enabled, self-hosted
      tickets, solutions)              speed-test backend)
```

**Design principles**

- 🧩 **Separation of concerns** — UI never touches `IJSRuntime` directly; it goes through a bridge.
- 🔌 **Inversion of control** — everything is registered in [`Program.cs`](BenchRig.App/Program.cs) and injected.
- 🧪 **Testable core** — engines are plain C# with no Blazor dependency.
- 🗂️ **Feature-first folders** — models, components and engines are grouped by domain.

---

## 🔄 How a Test Runs

```
User clicks "Start"
        │
        ▼
  ┌───────────────┐   reads/writes    ┌────────────────────┐
  │  Razor Page   │ ◄───────────────► │  State Container   │  (scoped, reactive)
  └──────┬────────┘                   └────────────────────┘
         │ invokes
         ▼
  ┌───────────────┐   calls            ┌────────────────────┐
  │ Orchestrator  │ ─────────────────► │   IJs*Bridge        │ ──► Browser API (Web Audio,
  └──────┬────────┘                    └────────────────────┘     Pointer, Canvas, etc.)
         │ feeds raw samples
         ▼
  ┌───────────────┐   produces         ┌────────────────────┐
  │    Engine      │ ─────────────────► │   Result / Score    │ ──► Report Builder ──► Firestore
  └───────────────┘                    └────────────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology |
| :-- | :-- |
| **Frontend** | Blazor WebAssembly · .NET 10 · C# · Razor Components · scoped CSS |
| **Browser APIs** | JS Interop bridges — Web Audio, Pointer, Keyboard, Canvas, Geolocation |
| **Auth & Data** | Firebase Authentication · Cloud Firestore |
| **Speed-test backend** | Cloudflare Worker (LibreSpeed-compatible + CORS proxy) |
| **AI** | Gemini-powered chatbot assistant |
| **Maps & Payments** | Leaflet · Stripe |
| **Delivery** | Progressive Web App · offline-first service worker · Firebase Hosting |

---

## 🧭 Application Routes

| Route | Page | Access |
| :-- | :-- | :-- |
| `/` | Home / dashboard | Public |
| `/mouse` `/keyboard` `/audio` | Peripheral diagnostics | Public |
| `/monitor` `/compute` `/network` | Display / CPU / network diagnostics | Public |
| `/report` | Consolidated diagnostic report | Auth |
| `/forum` · `/forum/{PostId}` | Community forum & threads | Public read |
| `/solutions` · `/solutions/{Id}` | Knowledge-base articles | Public read |
| `/support` | Support ticket helpdesk | Auth |
| `/donate` | Stripe donation flow | Public |
| `/login` · `/register` · `/account` | Authentication & profile | Public / Auth |
| `/admin` · `/admin/tickets` · `/admin/solutions` | Admin console | **Admin only** |

---

## 📂 Project Structure

```
BenchRig/
├── BenchRig.App/                 # Blazor WebAssembly client
│   ├── Core/
│   │   ├── Engines/              # Pure diagnostic logic (Mouse, Keyboard, Network, Report…)
│   │   ├── Models/              # Domain models, grouped by feature
│   │   ├── Interfaces/          # IJs*Bridge contracts
│   │   ├── Constants/ · Util/   # Shared constants & helpers
│   ├── Services/
│   │   ├── Orchestrators/       # Drive each test end-to-end
│   │   ├── State/              # Scoped, reactive state containers
│   │   ├── JsInterop/          # Bridge implementations (browser APIs)
│   │   └── Auth/               # AuthService, AdminGuardService
│   ├── Pages/                   # Routable pages (diagnostics, forum, admin, auth…)
│   ├── Components/              # Reusable UI, grouped by feature
│   ├── Program.cs               # DI registration & app bootstrap
│   └── wwwroot/                 # JS interop, CSS, icons, manifest, service worker
├── cloudflare-worker/           # Free, CORS-enabled speed-test backend
├── firestore.rules              # Hardened, role-based Firestore security rules
└── firebase.json                # Hosting config (SPA rewrites + cache headers)
```

---

## 🚦 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A modern browser (Chrome, Edge, Firefox)
- *(Optional)* A [Firebase](https://firebase.google.com/) project — auth, forum, reports, tickets
- *(Optional)* A [Cloudflare](https://workers.cloudflare.com/) account — your own speed-test server

### Run locally

```bash
# 1. Clone
git clone https://github.com/codeHannan/diagnostic-platform-blazor.git
cd diagnostic-platform-blazor

# 2. Restore & run the Blazor app
cd BenchRig.App
dotnet restore
dotnet run
```

Open the printed `https://localhost:<port>` URL and start benching. 🎉

> 💡 The diagnostics work standalone. Firebase and the Cloudflare Worker are only needed for accounts,
> cloud reports, forum/helpdesk, and self-hosted speed tests.

---

## ⚙️ Configuration

<details>
<summary><b>🔥 Firebase (auth, forum, reports, tickets)</b></summary>

<br/>

1. Create a project at the [Firebase Console](https://console.firebase.google.com/).
2. Enable **Email/Password** authentication and **Cloud Firestore**.
3. Drop your web-app keys into `BenchRig.App/wwwroot/firebase-config.js`.
4. Deploy the bundled, hardened security rules:

```bash
firebase deploy --only firestore:rules
```

</details>

<details>
<summary><b>☁️ Cloudflare Worker (self-hosted speed test)</b></summary>

<br/>

Browsers can only run speed tests against servers that send `Access-Control-Allow-Origin: *`. The included
Worker provides exactly that — a free, CORS-enabled backend you control (and it can proxy other LibreSpeed
servers for location-specific tests).

```bash
npm i -g wrangler
wrangler login
cd cloudflare-worker
wrangler deploy
```

Add the printed URL to `BenchRig.App/wwwroot/network-servers.json` (or via **Network → + Add** at runtime).
Full guide: [`cloudflare-worker/README.md`](cloudflare-worker/README.md).

</details>

---

## 📦 Build & Deploy

Produce an optimized static build:

```bash
cd BenchRig.App
dotnet publish -c Release -o release
```

The output in `release/wwwroot` is a plain static site — deployable to **Firebase Hosting**,
**Cloudflare Pages**, **GitHub Pages**, **Netlify**, or any static host:

```bash
# Firebase Hosting (config already points to release/wwwroot)
firebase deploy --only hosting
```

[`firebase.json`](firebase.json) is preconfigured with SPA rewrites and cache headers tuned for the
Blazor `_framework` and service worker.

---

## 📱 Progressive Web App

BenchRig is a fully installable **PWA**:

- 🛜 An offline-first **service worker** caches the .NET runtime and all assets.
- 📲 A **web manifest** lets users add it to their home screen or desktop.
- ⚡ After first load, it launches instantly and works without a connection.

Look for the **install** icon in your browser's address bar.

---

## 🔐 Security Model

Firestore access is governed by [`firestore.rules`](firestore.rules) — defense in depth, enforced server-side:

- **🧑‍⚖️ Role-based access control** — `user` vs `admin`; roles can't be self-escalated.
- **🔑 Ownership checks** — users may only mutate their own posts, comments, tickets and reports.
- **✅ Field validation** — required fields and length limits on every write (titles, bodies, ratings, comment depth).
- **🎫 Gated helpdesk** — ticket threads readable only by their owner or an admin; staff replies must match real role.
- **🛑 Default-deny** — anything not explicitly permitted is rejected.

---

## 🗺️ Roadmap

- [ ] 🎮 GPU diagnostics (WebGL / WebGPU stress patterns)
- [ ] 🧾 Exportable PDF diagnostic reports
- [ ] 📈 Historical report trends & device comparisons
- [ ] 🌐 Multi-language UI
- [ ] 🏷️ Shareable public report links

---

## ❓ FAQ

<details>
<summary><b>Do I need to install anything to use BenchRig?</b></summary>
<br/>
No. It runs entirely in the browser on Blazor WebAssembly — no drivers, no executables. You can optionally
install it as a PWA for offline use.
</details>

<details>
<summary><b>Does it work without a Firebase / Cloudflare account?</b></summary>
<br/>
Yes. The diagnostic tools run standalone. Firebase enables accounts, cloud reports, the forum and the helpdesk;
the Cloudflare Worker is only needed if you want your own speed-test server.
</details>

<details>
<summary><b>Is my hardware data sent anywhere?</b></summary>
<br/>
Tests run locally in your browser. Results are only persisted to Firestore — under your own account — when you
choose to save a report.
</details>

<details>
<summary><b>Which browsers are supported?</b></summary>
<br/>
Any modern browser with WebAssembly and the relevant web APIs (Chrome, Edge, Firefox). Some tests rely on
Web Audio / Pointer / Geolocation permissions.
</details>

---

## 👤 Author

<div align="center">

**Abdul Hannan Qureshi**
BSCS — Air University · *Visual Programming*

[![GitHub](https://img.shields.io/badge/GitHub-codeHannan-181717?logo=github&logoColor=white)](https://github.com/codeHannan)

</div>

---

## 📄 License & Usage

> ⚠️ **This project is NOT free to use.**

**© 2026 Abdul Hannan Qureshi. All Rights Reserved.**

This software and its source code are proprietary. No part of this repository may be copied, reproduced,
modified, distributed, published, sublicensed, or used — in whole or in part, for personal, academic, or
commercial purposes — without the **prior, explicit, written permission** of the author.

Viewing the source for reference is permitted; using or redistributing it is not. For licensing or usage
requests, contact the author.

<div align="center">

<br/>

**⭐ If BenchRig impressed you, drop a star — it means a lot.**

*Built with 💜, C#, and Blazor WebAssembly.*

</div>
