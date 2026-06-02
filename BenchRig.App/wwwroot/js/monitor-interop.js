// Canvas rendering, requestAnimationFrame timing, fullscreen for monitor tests.
const ghostLoops = new Map();

export async function enterFullscreen(elementId) {
    const el = document.getElementById(elementId) || document.documentElement;
    if (el.requestFullscreen) await el.requestFullscreen();
}

export async function exitFullscreen() {
    if (document.fullscreenElement && document.exitFullscreen) await document.exitFullscreen();
}

export function measureFrameRate(sampleFrames) {
    return new Promise((resolve) => {
        let count = 0;
        let start = 0;
        const tick = (ts) => {
            if (start === 0) start = ts;
            count++;
            if (count >= sampleFrames) {
                const elapsed = ts - start;
                resolve(elapsed > 0 ? (count - 1) * 1000 / elapsed : 0);
            } else {
                requestAnimationFrame(tick);
            }
        };
        requestAnimationFrame(tick);
    });
}

export function startGhosting(canvasId, speedPxPerFrame) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    stopGhosting(canvasId);
    const g = canvas.getContext("2d");
    let x = 0;
    let raf = 0;
    const draw = () => {
        const w = canvas.width, h = canvas.height;
        g.fillStyle = "#222831";
        g.fillRect(0, 0, w, h);
        g.fillStyle = "#DFD0B8";
        g.fillRect(x, h / 2 - 30, 60, 60);
        x = (x + speedPxPerFrame) % (w + 60);
        raf = requestAnimationFrame(draw);
    };
    raf = requestAnimationFrame(draw);
    ghostLoops.set(canvasId, () => cancelAnimationFrame(raf));
}

export function stopGhosting(canvasId) {
    const stop = ghostLoops.get(canvasId);
    if (stop) { stop(); ghostLoops.delete(canvasId); }
}
