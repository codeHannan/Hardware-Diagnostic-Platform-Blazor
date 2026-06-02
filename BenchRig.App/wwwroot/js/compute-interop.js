// CPU stress (Web Workers via inline blob) and GPU stress (WebGL fill) benchmarks.

function makeCpuWorker() {
    const src = `
        self.onmessage = (e) => {
            const deadline = Date.now() + e.data.durationMs;
            let ops = 0;
            // Prime-sieve style busy work.
            while (Date.now() < deadline) {
                for (let n = 2; n < 5000; n++) {
                    let prime = true;
                    for (let d = 2; d * d <= n; d++) { if (n % d === 0) { prime = false; break; } }
                    if (prime) ops++;
                }
            }
            self.postMessage({ ops });
        };`;
    const blob = new Blob([src], { type: "application/javascript" });
    return new Worker(URL.createObjectURL(blob));
}

export async function runCpuBenchmark(threadCount, durationSec, progressCallback) {
    const durationMs = durationSec * 1000;
    const start = performance.now();
    const workers = [];
    const promises = [];

    // Progress ticker
    let progTimer = setInterval(() => {
        const pct = Math.min(100, (performance.now() - start) / durationMs * 100);
        progressCallback.invokeMethodAsync("OnProgress", pct);
    }, 200);

    for (let i = 0; i < threadCount; i++) {
        const w = makeCpuWorker();
        workers.push(w);
        promises.push(new Promise((resolve) => {
            w.onmessage = (e) => resolve(e.data.ops);
            w.postMessage({ durationMs });
        }));
    }

    const results = await Promise.all(promises);
    clearInterval(progTimer);
    workers.forEach(w => w.terminate());
    progressCallback.invokeMethodAsync("OnProgress", 100);

    const totalOps = results.reduce((a, b) => a + b, 0);
    const elapsedMs = performance.now() - start;
    const opsPerSec = totalOps / (elapsedMs / 1000);

    return { score: 0, threadsUsed: threadCount, durationMs: elapsedMs, opsPerSec };
}

export async function runGpuBenchmark(canvasId, durationSec, progressCallback) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return { score: 0, avgFps: 0, shaderComplexity: 0, vramEstimateMb: 0 };

    const gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
    if (!gl) return { score: 0, avgFps: 0, shaderComplexity: 0, vramEstimateMb: 0 };

    const durationMs = durationSec * 1000;
    const start = performance.now();
    let frames = 0;

    return new Promise((resolve) => {
        const draw = () => {
            const t = (performance.now() - start) / 1000;
            // Heavy-ish per-frame fill with shifting colors.
            for (let i = 0; i < 50; i++) {
                gl.clearColor(Math.sin(t + i) * 0.5 + 0.5, Math.cos(t) * 0.5 + 0.5, 0.4, 1);
                gl.clear(gl.COLOR_BUFFER_BIT);
            }
            frames++;
            const elapsed = performance.now() - start;
            progressCallback.invokeMethodAsync("OnProgress", Math.min(100, elapsed / durationMs * 100));
            if (elapsed < durationMs) {
                requestAnimationFrame(draw);
            } else {
                const avgFps = frames / (elapsed / 1000);
                resolve({ score: 0, avgFps, shaderComplexity: 50, vramEstimateMb: 0 });
            }
        };
        requestAnimationFrame(draw);
    });
}
