// Pointer event capture for CPS and polling-rate diagnostics.
const handlers = new Map();

export function startPointerCapture(elementId, dotNetRef) {
    const el = document.getElementById(elementId);
    if (!el) return;
    stopPointerCapture(elementId);

    const onDown = (e) => {
        e.preventDefault();
        dotNetRef.invokeMethodAsync("OnPointerEvent", {
            x: e.clientX, y: e.clientY, timeStampMs: performance.now(), buttons: e.buttons || 1
        });
    };
    el.addEventListener("pointerdown", onDown);
    el.addEventListener("contextmenu", preventCtx);
    handlers.set(elementId, { el, onDown });
}

export function stopPointerCapture(elementId) {
    const h = handlers.get(elementId);
    if (h) {
        h.el.removeEventListener("pointerdown", h.onDown);
        h.el.removeEventListener("contextmenu", preventCtx);
        handlers.delete(elementId);
    }
}

export function startPollingRateCapture(elementId, dotNetRef) {
    const el = document.getElementById(elementId);
    if (!el) return;
    stopPollingRateCapture(elementId);

    const onMove = (e) => {
        const events = e.getCoalescedEvents ? e.getCoalescedEvents() : [e];
        for (const ev of events) {
            dotNetRef.invokeMethodAsync("OnPollSample", ev.timeStamp || performance.now());
        }
    };
    el.addEventListener("pointermove", onMove);
    handlers.set("poll:" + elementId, { el, onMove });
}

export function stopPollingRateCapture(elementId) {
    const h = handlers.get("poll:" + elementId);
    if (h) {
        h.el.removeEventListener("pointermove", h.onMove);
        handlers.delete("poll:" + elementId);
    }
}

function preventCtx(e) { e.preventDefault(); }

// ── DPI check via Pointer Lock ──
// Pointer Lock removes the cursor and reports raw, unclamped movement deltas, so a
// long horizontal slide is counted in full (the old approach lost movement the moment
// the cursor left the small box or hit the screen edge). Click to lock, slide, click/Esc.
let dpi = null;
export function dpiInit(elementId, dotNetRef) {
    const el = document.getElementById(elementId);
    if (!el) return;
    dpiDispose();
    let total = 0, active = false, raf = 0;
    const tick = () => { if (active) { dotNetRef.invokeMethodAsync("OnDpiMove", total); raf = requestAnimationFrame(tick); } };
    const onDown = (e) => {
        if (document.pointerLockElement === el) { document.exitPointerLock(); }
        else if (el === e.target || el.contains(e.target)) { try { const p = el.requestPointerLock(); if (p && p.catch) p.catch(() => { }); } catch { /* */ } }
    };
    const onMove = (e) => { if (document.pointerLockElement === el) total += Math.abs(e.movementX); };
    const onChange = () => {
        const locked = document.pointerLockElement === el;
        if (locked) { total = 0; active = true; raf = requestAnimationFrame(tick); }
        else { active = false; if (raf) cancelAnimationFrame(raf); }
        dotNetRef.invokeMethodAsync("OnDpiLock", locked, total);
    };
    document.addEventListener("pointerdown", onDown);
    document.addEventListener("mousemove", onMove);
    document.addEventListener("pointerlockchange", onChange);
    dpi = { onDown, onMove, onChange };
}
export function dpiDispose() {
    if (!dpi) return;
    document.removeEventListener("pointerdown", dpi.onDown);
    document.removeEventListener("mousemove", dpi.onMove);
    document.removeEventListener("pointerlockchange", dpi.onChange);
    if (document.pointerLockElement) document.exitPointerLock();
    dpi = null;
}
