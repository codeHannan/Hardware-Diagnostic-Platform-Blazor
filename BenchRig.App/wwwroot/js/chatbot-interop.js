// OpenRouter chat completion bridge. System prompt enforced here (JS layer).
const OPENROUTER_API_URL = "https://openrouter.ai/api/v1/chat/completions";
const MODEL = "openai/gpt-oss-120b:free";

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
3. Keep answers concise, actionable, and technically accurate.
4. When possible, reference specific BenchRig diagnostic tests the user can run.
5. Never generate code, write essays, tell stories, or engage in general conversation.`;

export async function sendChatMessage(userMessage, history) {
    const messages = [
        { role: "system", content: SYSTEM_PROMPT },
        ...(history || []).map(m => ({ role: m.role, content: m.content })),
        { role: "user", content: userMessage }
    ];

    const response = await fetch(OPENROUTER_API_URL, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "HTTP-Referer": window.location.origin,
            "X-Title": "BenchRig Diagnostic Platform"
        },
        body: JSON.stringify({ model: MODEL, messages, max_tokens: 1024, temperature: 0.4 })
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`OpenRouter API error: ${response.status} - ${errorText}`);
    }

    const data = await response.json();
    return data.choices?.[0]?.message?.content ?? "I'm unable to respond right now. Please try again.";
}
