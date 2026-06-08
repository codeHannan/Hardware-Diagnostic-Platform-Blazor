// CPU / GPU / Memory benchmarking.
//  - CPU: real multithreading via Web Workers running a mixed integer + float + array
//    kernel; measures single-core and multi-core throughput (work-units/sec).
//  - GPU: WebGL2 heavy fractal fragment shader at a chosen resolution/complexity;
//    measures sustained FPS (warm-up excluded).
//  - Memory: progressive (capped) allocation + sequential read/write bandwidth.

// ── Hardware detection ──
export function detectHardware() {
    const out = {
        threads: navigator.hardwareConcurrency || 0,
        deviceMemoryGb: navigator.deviceMemory || 0,
        userAgent: navigator.userAgent || "",
        gpuVendor: "", gpuRenderer: "",
        webgl2: false, webgpu: !!navigator.gpu,
        jsHeapLimitMb: 0,
    };
    try {
        const c = document.createElement("canvas");
        out.webgl2 = !!c.getContext("webgl2");
        const gl = c.getContext("webgl2") || c.getContext("webgl");
        if (gl) {
            const dbg = gl.getExtension("WEBGL_debug_renderer_info");
            if (dbg) {
                out.gpuVendor = gl.getParameter(dbg.UNMASKED_VENDOR_WEBGL) || "";
                out.gpuRenderer = gl.getParameter(dbg.UNMASKED_RENDERER_WEBGL) || "";
            }
            if (!out.gpuRenderer) out.gpuRenderer = gl.getParameter(gl.RENDERER) || "";
        }
    } catch { /* ignore */ }
    try { if (performance.memory) out.jsHeapLimitMb = Math.round(performance.memory.jsHeapSizeLimit / 1048576); } catch { /* Chrome only */ }
    return out;
}

// ── CPU ──
function cpuWorkerSource() {
    return `self.onmessage = (e) => {
        const end = performance.now() + e.data.durationMs;
        let units = 0, acc = 0;
        const N = 4096, arr = new Float64Array(N);
        let x = 123456789 | 0;
        while (performance.now() < end) {
            for (let i = 0; i < 8000; i++) { x ^= x << 13; x ^= x >> 17; x ^= x << 5; acc += (x >>> 0) % 1009; }
            for (let i = 0; i < 4000; i++) { acc += Math.sqrt(i * 1.0001) + Math.sin(i * 0.001) * Math.cos(i * 0.002); }
            for (let i = 0; i < N; i++) { arr[i] = arr[i] * 1.0000001 + (acc % 7); }
            units++;
        }
        self.postMessage({ units, acc });
    };`;
}

export async function runCpuBenchmark(threads, durationMs, progressCb) {
    const blobUrl = URL.createObjectURL(new Blob([cpuWorkerSource()], { type: "application/javascript" }));
    const totalMs = durationMs * 2;
    const globalStart = performance.now();
    const report = () => { try { progressCb && progressCb.invokeMethodAsync("OnProgress", Math.min(100, (performance.now() - globalStart) / totalMs * 100)); } catch { } };
    const timer = setInterval(report, 150);

    async function runPass(n) {
        const workers = [], promises = [];
        const start = performance.now();
        for (let i = 0; i < n; i++) {
            const w = new Worker(blobUrl);
            workers.push(w);
            promises.push(new Promise(res => { w.onmessage = ev => res(ev.data.units); }));
            w.postMessage({ durationMs });
        }
        const units = await Promise.all(promises);
        workers.forEach(w => w.terminate());
        const elapsed = (performance.now() - start) / 1000;
        return units.reduce((a, b) => a + b, 0) / Math.max(0.001, elapsed); // units/sec
    }

    const singleUps = await runPass(1);
    const multiUps = await runPass(Math.max(1, threads));
    clearInterval(timer);
    try { progressCb && progressCb.invokeMethodAsync("OnProgress", 100); } catch { }
    URL.revokeObjectURL(blobUrl);
    return { singleUps, multiUps, threads: Math.max(1, threads), durationMs: totalMs };
}

// ── GPU (WebGL2 volumetric ray-marcher, "Volume Shader BM" style) ──
// Per pixel we ray-march uComplexity steps through animated 3D fbm noise (5 octaves of
// value noise per step) with lighting/accumulation — a genuinely heavy, GPU-bound load
// (far more so than the old fractal), which is what makes it a real GPU stress benchmark.
const VERT = `#version 300 es
precision highp float;
const vec2 verts[3] = vec2[3](vec2(-1.0,-1.0), vec2(3.0,-1.0), vec2(-1.0,3.0));
void main(){ gl_Position = vec4(verts[gl_VertexID], 0.0, 1.0); }`;

const FRAG = `#version 300 es
precision highp float;
out vec4 outColor;
uniform vec2 uRes; uniform float uTime; uniform int uComplexity;

float hash(vec3 p){
    p = fract(p * 0.3183099 + vec3(0.71, 0.113, 0.419));
    p *= 17.0;
    return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
}
float noise(vec3 x){
    vec3 i = floor(x); vec3 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash(i + vec3(0,0,0)), hash(i + vec3(1,0,0)), f.x),
                   mix(hash(i + vec3(0,1,0)), hash(i + vec3(1,1,0)), f.x), f.y),
               mix(mix(hash(i + vec3(0,0,1)), hash(i + vec3(1,0,1)), f.x),
                   mix(hash(i + vec3(0,1,1)), hash(i + vec3(1,1,1)), f.x), f.y), f.z);
}
float fbm(vec3 p){
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 5; i++){ v += a * noise(p); p = p * 2.03 + vec3(1.7, 9.2, -3.1); a *= 0.5; }
    return v;
}
void main(){
    vec2 uv = (gl_FragCoord.xy - 0.5 * uRes) / uRes.y;
    vec3 ro = vec3(0.0, 0.0, -3.2);
    vec3 rd = normalize(vec3(uv, 1.4));
    float ang = uTime * 0.2;
    float ca = cos(ang), sa = sin(ang);
    mat2 rot = mat2(ca, -sa, sa, ca);
    ro.xz *= rot; rd.xz *= rot;

    int steps = uComplexity;
    float stepLen = 6.0 / float(steps);   // fixed total march distance
    vec4 acc = vec4(0.0);
    float t = 0.4;
    for (int i = 0; i < 1024; i++){
        if (i >= steps) break;            // uComplexity ray-march steps (full, deterministic load)
        vec3 pos = ro + rd * t;
        float d = fbm(pos * 1.3 + vec3(0.0, uTime * 0.15, 0.0));
        d = smoothstep(0.45, 0.95, d);
        if (d > 0.001){
            float lt = clamp(d * 1.4, 0.0, 1.0);
            vec3 col = mix(vec3(0.15, 0.2, 0.45), vec3(1.0, 0.85, 0.6), lt);
            float alpha = d * 0.35;
            acc.rgb += (1.0 - acc.a) * col * alpha;
            acc.a += (1.0 - acc.a) * alpha;
        }
        t += stepLen;
    }
    vec3 bg = mix(vec3(0.015, 0.02, 0.05), vec3(0.2, 0.12, 0.25), clamp(uv.y + 0.5, 0.0, 1.0));
    outColor = vec4(acc.rgb + (1.0 - acc.a) * bg, 1.0);
}`;

function compile(gl, type, src) {
    const s = gl.createShader(type); gl.shaderSource(s, src); gl.compileShader(s);
    if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(s) || "shader");
    return s;
}

export async function runGpuBenchmark(canvasId, width, height, complexity, durationMs, progressCb) {
    const canvas = document.getElementById(canvasId);
    const fail = { avgFps: 0, frames: 0, width, height, complexity, supported: false };
    if (!canvas) return fail;
    const gl = canvas.getContext("webgl2");
    if (!gl) return fail;
    canvas.width = width; canvas.height = height;

    let prog;
    try {
        prog = gl.createProgram();
        gl.attachShader(prog, compile(gl, gl.VERTEX_SHADER, VERT));
        gl.attachShader(prog, compile(gl, gl.FRAGMENT_SHADER, FRAG));
        gl.linkProgram(prog);
        if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) return fail;
    } catch { return fail; }

    gl.useProgram(prog);
    const uRes = gl.getUniformLocation(prog, "uRes");
    const uTime = gl.getUniformLocation(prog, "uTime");
    const uComplexity = gl.getUniformLocation(prog, "uComplexity");
    gl.viewport(0, 0, width, height);
    gl.uniform2f(uRes, width, height);
    gl.uniform1i(uComplexity, complexity);

    const start = performance.now();
    const warmupMs = 700;
    const deadline = start + durationMs;
    let frames = 0, measuredFrames = 0, measureStart = 0, t = 0;
    const pixel = new Uint8Array(4);
    // One frame per GPU sync: the volumetric shader is heavy, so per-frame render time
    // dwarfs the readback overhead, and syncing each frame keeps the tab responsive
    // (we yield after every chunk) and avoids long single-stall GPU-watchdog risk.
    const BATCH = 1;

    // Throughput loop, vsync-INDEPENDENT and GPU-SYNCED. Critically we sync with
    // gl.readPixels(), NOT gl.finish(): WebGL's gl.finish() returns immediately across the
    // browser's GPU-process boundary, so it measured JS submit rate (≈thousands of fps on any
    // GPU) — that's why a weak GPU could "beat" a fast one. readPixels stalls the CPU until the
    // GPU has actually finished the queued draws, giving a real frame time. Yields every ~16 ms.
    while (performance.now() < deadline) {
        const chunkEnd = Math.min(deadline, performance.now() + 16);
        do {
            for (let b = 0; b < BATCH; b++) {
                t += 0.016;
                gl.uniform1f(uTime, t);
                gl.drawArrays(gl.TRIANGLES, 0, 3);
            }
            gl.readPixels(0, 0, 1, 1, gl.RGBA, gl.UNSIGNED_BYTE, pixel); // forces real GPU completion
            const now = performance.now();
            frames += BATCH;
            if (measureStart === 0 && now - start >= warmupMs) { measureStart = now; measuredFrames = 0; }
            else if (measureStart > 0) measuredFrames += BATCH;
        } while (performance.now() < chunkEnd);
        try { progressCb && progressCb.invokeMethodAsync("OnProgress", Math.min(100, (performance.now() - start) / durationMs * 100)); } catch { }
        await new Promise(r => setTimeout(r, 0));
    }

    const measSec = measureStart > 0 ? (performance.now() - measureStart) / 1000 : (performance.now() - start) / 1000;
    const avgFps = measSec > 0 ? measuredFrames / measSec : 0;
    return { avgFps, frames, width, height, complexity, supported: true };
}

// ── Memory: progressive allocation (capped) + bandwidth ──
function measureBandwidth() {
    const n = 8 * 1024 * 1024;            // 8M doubles = 64 MB
    const a = new Float64Array(n);
    let t = performance.now();
    for (let i = 0; i < n; i++) a[i] = i * 1.0001;
    const wt = performance.now() - t;
    t = performance.now();
    let s = 0;
    for (let i = 0; i < n; i++) s += a[i];
    const rt = performance.now() - t;
    const bytes = n * 8;
    return { write: bytes / (wt / 1000) / 1e9, read: bytes / (rt / 1000) / 1e9, checksum: s };
}

export async function runMemoryTest(targetMb, progressCb) {
    const chunkMb = 32;
    const chunkBytes = chunkMb * 1048576;
    const chunks = [];
    let allocated = 0;
    let oom = false;
    try {
        while (allocated < targetMb) {
            const buf = new Uint8Array(chunkBytes);
            for (let i = 0; i < buf.length; i += 4096) buf[i] = (i & 0xff); // touch pages
            chunks.push(buf);
            allocated += chunkMb;
            try { progressCb && progressCb.invokeMethodAsync("OnProgress", Math.min(100, allocated / targetMb * 100)); } catch { }
            if (allocated % 128 === 0) await new Promise(r => setTimeout(r, 0)); // yield
        }
    } catch { oom = true; }

    const bw = measureBandwidth();
    chunks.length = 0; // release
    let heapLimitMb = 0;
    try { if (performance.memory) heapLimitMb = Math.round(performance.memory.jsHeapSizeLimit / 1048576); } catch { }

    return { peakMb: allocated, oom, writeGBs: bw.write, readGBs: bw.read, heapLimitMb };
}
