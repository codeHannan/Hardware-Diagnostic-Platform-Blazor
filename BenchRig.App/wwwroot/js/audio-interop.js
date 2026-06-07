// Web Audio API + MediaDevices bridge for audio diagnostics.
//  - Multichannel playback (stereo / 5.1 / 7.1) via a ChannelMerger + discrete routing.
//  - Immersive spatial sweep via an HRTF PannerNode.
//  - Stereo-balance panned tones.
//  - Microphone capture: level meter, time-domain waveform, loopback (hear yourself).
//  - Echo round-trip latency (speaker -> mic click detection) + system latency.

let ctx = null;
let active = [];          // nodes to tear down on stop
let activeTimer = 0;      // animation interval for sweeps
let micStream = null;
let analyser = null;
let loopbackSrc = null;
let balancePanner = null; // live-adjustable stereo panner for the balance test

function getCtx() {
    if (!ctx) ctx = new (window.AudioContext || window.webkitAudioContext)();
    if (ctx.state === "suspended") ctx.resume();
    return ctx;
}

export function getMaxChannels() {
    try { return getCtx().destination.maxChannelCount || 2; } catch { return 2; }
}

export function stopPlayback() {
    if (activeTimer) { clearInterval(activeTimer); activeTimer = 0; }
    for (const n of active) {
        try { if (n.stop) n.stop(); } catch { /* already stopped */ }
        try { n.disconnect(); } catch { /* noop */ }
    }
    active = [];
    balancePanner = null;
}

// ── Balance: continuous tone whose stereo pan can be adjusted live ──
export function startBalanceTone(freq) {
    stopPlayback();
    const c = getCtx();
    const osc = c.createOscillator();
    const gain = c.createGain();
    const panner = c.createStereoPanner();
    osc.frequency.value = freq || 440;
    gain.gain.value = 0.16;
    osc.connect(gain).connect(panner).connect(c.destination);
    osc.start();
    balancePanner = panner;
    active = [osc, gain, panner];
}

export function setPan(value) {
    if (balancePanner) balancePanner.pan.value = Math.max(-1, Math.min(1, value));
}

// ── Multichannel: route a tone to one discrete output channel ──
export function playChannelTone(channelIndex, totalChannels, freq) {
    stopPlayback();
    const c = getCtx();
    const n = Math.max(2, Math.min(totalChannels, c.destination.maxChannelCount || 2));
    try {
        c.destination.channelCount = n;
        c.destination.channelCountMode = "explicit";
        c.destination.channelInterpretation = "discrete";
    } catch { /* device may not allow */ }

    const merger = c.createChannelMerger(n);
    const osc = c.createOscillator();
    const gain = c.createGain();
    osc.type = "sine";
    osc.frequency.value = freq || 440;
    gain.gain.value = 0.0001;
    osc.connect(gain);
    gain.connect(merger, 0, Math.min(channelIndex, n - 1));
    merger.connect(c.destination);
    osc.start();
    // soft attack to avoid clicks
    gain.gain.exponentialRampToValueAtTime(0.2, c.currentTime + 0.04);
    active = [osc, gain, merger];
}

// ── Stereo balance: panned tone ──
export function playPannedTone(pan, freq) {
    stopPlayback();
    const c = getCtx();
    const osc = c.createOscillator();
    const gain = c.createGain();
    const panner = c.createStereoPanner();
    panner.pan.value = Math.max(-1, Math.min(1, pan));
    osc.frequency.value = freq || 440;
    gain.gain.value = 0.18;
    osc.connect(gain).connect(panner).connect(c.destination);
    osc.start();
    active = [osc, gain, panner];
}

// ── Immersive: HRTF panner orbiting the listener ──
export function startSpatialSweep() {
    stopPlayback();
    const c = getCtx();
    const osc = c.createOscillator();
    const gain = c.createGain();
    const panner = c.createPanner();
    panner.panningModel = "HRTF";
    panner.distanceModel = "inverse";
    osc.type = "triangle";
    osc.frequency.value = 320;
    gain.gain.value = 0.22;
    osc.connect(gain).connect(panner).connect(c.destination);
    osc.start();
    const start = performance.now();
    activeTimer = setInterval(() => {
        const a = (performance.now() - start) / 1000 * 1.6;
        if (panner.positionX) {
            panner.positionX.value = Math.cos(a) * 4;
            panner.positionZ.value = Math.sin(a) * 4;
            panner.positionY.value = 0;
        } else {
            panner.setPosition(Math.cos(a) * 4, 0, Math.sin(a) * 4);
        }
    }, 40);
    active = [osc, gain, panner];
}

// ── Microphone ──
export async function startMicCapture() {
    try {
        micStream = await navigator.mediaDevices.getUserMedia({ audio: { echoCancellation: false, noiseSuppression: false, autoGainControl: false } });
        const c = getCtx();
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
    stopLoopback();
    if (micStream) { micStream.getTracks().forEach(t => t.stop()); micStream = null; }
    analyser = null;
}

export function startLoopback() {
    if (!analyser || !analyser._src) return;
    stopLoopback();
    loopbackSrc = analyser._src;
    try { loopbackSrc.connect(getCtx().destination); } catch { /* noop */ }
}

export function stopLoopback() {
    if (loopbackSrc) { try { loopbackSrc.disconnect(getCtx().destination); } catch { /* noop */ } loopbackSrc = null; }
}

export function getMicLevelDb() {
    if (!analyser) return -100;
    const data = new Uint8Array(analyser.fftSize);
    analyser.getByteTimeDomainData(data);
    let sum = 0;
    for (let i = 0; i < data.length; i++) { const v = (data[i] - 128) / 128; sum += v * v; }
    const rms = Math.sqrt(sum / data.length);
    return rms > 0 ? Math.max(-100, 20 * Math.log10(rms)) : -100;
}

// Down-sampled time-domain waveform (0..255) for a visualizer.
export function getMicWaveform() {
    if (!analyser) return [];
    const data = new Uint8Array(analyser.fftSize);
    analyser.getByteTimeDomainData(data);
    const points = 72, step = Math.max(1, Math.floor(data.length / points));
    const out = [];
    for (let i = 0; i < points; i++) out.push(data[i * step]);
    return out;
}

// ── Echo round-trip: play a click, detect it via the mic ──
export async function measureEchoLatency() {
    const c = getCtx();
    if (analyser) {
        return await new Promise((resolve) => {
            const data = new Uint8Array(analyser.fftSize);
            const osc = c.createOscillator();
            const g = c.createGain();
            osc.frequency.value = 1000; g.gain.value = 0.6;
            osc.connect(g).connect(c.destination);
            const t0 = performance.now();
            osc.start();
            osc.stop(c.currentTime + 0.04);
            const iv = setInterval(() => {
                analyser.getByteTimeDomainData(data);
                let peak = 0;
                for (let i = 0; i < data.length; i++) peak = Math.max(peak, Math.abs(data[i] - 128));
                const dt = performance.now() - t0;
                if (peak > 45 && dt > 8) { clearInterval(iv); resolve(dt); }
                else if (dt > 700) { clearInterval(iv); resolve(0); }
            }, 4);
        });
    }
    const base = (c.baseLatency || 0) + (c.outputLatency || 0);
    return base > 0 ? base * 1000 : 20;
}

export function getSystemLatency() {
    const c = getCtx();
    return ((c.baseLatency || 0) + (c.outputLatency || 0)) * 1000;
}
