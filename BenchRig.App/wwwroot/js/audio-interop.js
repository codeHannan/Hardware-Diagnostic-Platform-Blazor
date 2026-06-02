// Web Audio API + MediaDevices bridge for audio diagnostics.
let audioCtx = null;
let currentSource = null;
let micStream = null;
let analyser = null;
let loopbackNode = null;

function ctx() {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    return audioCtx;
}

// Generates a panned test tone instead of requiring .wav assets.
export function playTestTone(channelAudioPath, outputChannelId) {
    stopPlayback();
    const c = ctx();
    const osc = c.createOscillator();
    const gain = c.createGain();
    const panner = c.createStereoPanner();

    osc.type = "sine";
    osc.frequency.value = 440;
    gain.gain.value = 0.15;

    switch (outputChannelId) {
        case "left": panner.pan.value = -1; break;
        case "right": panner.pan.value = 1; break;
        default: panner.pan.value = 0;
    }

    osc.connect(gain).connect(panner).connect(c.destination);
    osc.start();
    currentSource = { osc, gain };
}

export function stopPlayback() {
    if (currentSource) {
        try { currentSource.osc.stop(); } catch { /* already stopped */ }
        currentSource = null;
    }
}

export async function startMicCapture() {
    try {
        micStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const c = ctx();
        const src = c.createMediaStreamSource(micStream);
        analyser = c.createAnalyser();
        analyser.fftSize = 1024;
        src.connect(analyser);
        analyser._src = src;
        return true;
    } catch {
        return false;
    }
}

export function stopMicCapture() {
    if (micStream) {
        micStream.getTracks().forEach(t => t.stop());
        micStream = null;
    }
    analyser = null;
}

export function startLoopback() {
    if (!micStream) return;
    const c = ctx();
    const src = c.createMediaStreamSource(micStream);
    loopbackNode = src;
    src.connect(c.destination);
}

export function stopLoopback() {
    if (loopbackNode) {
        try { loopbackNode.disconnect(); } catch { /* noop */ }
        loopbackNode = null;
    }
}

export async function measureEchoLatency() {
    // Best-effort: returns the audio context base latency in ms.
    const c = ctx();
    const base = (c.baseLatency || 0) + (c.outputLatency || 0);
    return base > 0 ? base * 1000 : 20;
}

export function getMicLevelDb() {
    if (!analyser) return -100;
    const data = new Uint8Array(analyser.fftSize);
    analyser.getByteTimeDomainData(data);
    let sum = 0;
    for (let i = 0; i < data.length; i++) {
        const v = (data[i] - 128) / 128;
        sum += v * v;
    }
    const rms = Math.sqrt(sum / data.length);
    return rms > 0 ? Math.max(-100, 20 * Math.log10(rms)) : -100;
}
