// AI assistant bridge — Firebase AI Logic (firebase/ai) with the Gemini Developer API backend.
// Uses the app's own Firebase project config: NO API key in the client, and the Gemini
// Developer API backend has a free tier. Enable it once in the Firebase console
// (Build → AI Logic → Gemini Developer API). See FIREBASE_SETUP.md.

import { app } from "./firebase-config.js";
import { getAI, getGenerativeModel, GoogleAIBackend } from "firebase/ai";

const MODEL = "gemini-3.5-flash";

const SYSTEM_PROMPT = `You are BenchRig Assistant, an expert technical support chatbot embedded inside a hardware and network diagnostic platform. Your ONLY purpose is to help users diagnose, troubleshoot, and resolve issues related to:

- Network performance (speed, latency, jitter, packet loss, DNS, ISP issues)
- Mouse hardware (polling rate, DPI, button registration, jitter, dead zones)
- Keyboard hardware (key registration, chattering, ghosting, NKRO, stuck keys)
- Audio devices (playback channels, microphone input, echo, latency)
- Monitor/display (dead pixels, backlight bleed, ghosting, refresh rate, gamma, response time)
- CPU and GPU performance (benchmarking, thermal throttling, driver issues)

RULES:
1. NEVER answer questions unrelated to hardware diagnostics or network troubleshooting.
2. If asked about unrelated topics, respond: "I can only assist with hardware and network diagnostic topics. Please ask me about your device or connection issues!"
3. Keep answers concise, actionable, and technically accurate. Use short markdown bullet points where helpful.
4. When possible, reference specific BenchRig diagnostic tests the user can run.
5. Never write essays, tell stories, or engage in general conversation.`;

// Lazily build and cache the Firebase AI Logic model.
let fbModel = null;
function firebaseModel() {
    if (!fbModel) {
        const ai = getAI(app, { backend: new GoogleAIBackend() });
        fbModel = getGenerativeModel(ai, { model: MODEL, systemInstruction: SYSTEM_PROMPT });
    }
    return fbModel;
}

export async function sendChatMessage(userMessage, history) {
    try {
        const model = firebaseModel();
        const chat = model.startChat({
            history: (history || []).map(m => ({
                role: m.role === "assistant" ? "model" : "user",
                parts: [{ text: m.content }],
            })),
        });
        const result = await chat.sendMessage(userMessage);
        return result.response.text();
    } catch (e) {
        const msg = (e && e.message) ? e.message : String(e);
        throw new Error(`Firebase AI Logic error: ${msg}`);
    }
}
