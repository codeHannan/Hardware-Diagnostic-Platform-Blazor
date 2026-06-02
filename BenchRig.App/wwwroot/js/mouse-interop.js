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
