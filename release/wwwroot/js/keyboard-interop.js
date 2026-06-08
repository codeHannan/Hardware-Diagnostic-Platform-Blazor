// Keyboard event capture for registration, chatter, rollover, polling.
let down = null;
let up = null;

export function startKeyCapture(dotNetRef) {
    stopKeyCapture();
    down = (e) => {
        // Avoid OS-level repeat firing flooding chatter analysis.
        dotNetRef.invokeMethodAsync("OnKeyEvent", {
            code: e.code, key: e.key, isDown: true, timeStampMs: performance.now()
        });
        if (e.code === "Tab" || e.code.startsWith("Arrow") || e.code === "Space") e.preventDefault();
    };
    up = (e) => {
        dotNetRef.invokeMethodAsync("OnKeyEvent", {
            code: e.code, key: e.key, isDown: false, timeStampMs: performance.now()
        });
    };
    window.addEventListener("keydown", down);
    window.addEventListener("keyup", up);
}

export function stopKeyCapture() {
    if (down) window.removeEventListener("keydown", down);
    if (up) window.removeEventListener("keyup", up);
    down = null; up = null;
}
