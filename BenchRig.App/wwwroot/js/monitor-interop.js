// Fullscreen control + frame-timing measurement for monitor diagnostics.
// Visual test patterns are rendered in Blazor/CSS; this module only handles
// the Fullscreen API and requestAnimationFrame timing.

export async function enterFullscreen(elementId) {
    const el = document.getElementById(elementId) || document.documentElement;
    try { if (el.requestFullscreen) await el.requestFullscreen(); } catch { /* user gesture / unsupported */ }
}

export async function exitFullscreen() {
    try { if (document.fullscreenElement && document.exitFullscreen) await document.exitFullscreen(); } catch { /* noop */ }
}

// Notifies .NET when the user leaves fullscreen (e.g. presses Esc), so the
// overlay can close itself in sync.
export function watchFullscreenExit(dotNetRef, method) {
    const handler = () => {
        if (!document.fullscreenElement) {
            document.removeEventListener("fullscreenchange", handler);
            try { dotNetRef.invokeMethodAsync(method); } catch { /* circuit gone */ }
        }
    };
    document.addEventListener("fullscreenchange", handler);
}

// Collects per-frame deltas (ms) for the duration; the caller derives
// refresh rate, jitter and dropped frames.
export function measureFrameDeltas(durationMs) {
    return new Promise((resolve) => {
        const deltas = [];
        let last = 0;
        const start = performance.now();
        function tick(ts) {
            if (last > 0) deltas.push(ts - last);
            last = ts;
            if (ts - start < durationMs) requestAnimationFrame(tick);
            else resolve(deltas);
        }
        requestAnimationFrame(tick);
    });
}
