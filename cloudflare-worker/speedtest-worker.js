/**
 * BenchRig speed-test Worker (Cloudflare Workers, free tier).
 *
 * Gives you a CORS-enabled speed-test endpoint you fully control, so BenchRig
 * can test against *your* Worker instead of (or in addition to) Cloudflare's
 * shared edge. It can also proxy any upstream server that lacks CORS, which
 * lets you test against geographically specific origins.
 *
 * Endpoints (all send Access-Control-Allow-Origin: *):
 *   GET  /down?bytes=N        -> N bytes of garbage (default 25 MiB, max 100 MiB)
 *   POST /up                  -> sinks the body, returns { received }
 *   GET  /ping                -> tiny 204 for latency probes
 *   GET  /meta                -> { colo, ip, country, city } from request.cf
 *   GET  /proxy?url=ENCODED   -> streams an upstream URL back with CORS added
 *   POST /proxy?url=ENCODED   -> forwards the upload to an upstream URL
 *
 * Deploy:
 *   1. npm i -g wrangler && wrangler login
 *   2. wrangler deploy   (from this folder; see wrangler.toml below)
 *   3. Add the resulting URL to wwwroot/network-servers.json, e.g.:
 *        [{ "key":"myedge", "name":"My Worker", "location":"Cloudflare edge",
 *           "type":"worker", "url":"https://benchrig-speedtest.<you>.workers.dev" }]
 *
 * wrangler.toml:
 *   name = "benchrig-speedtest"
 *   main = "speedtest-worker.js"
 *   compatibility_date = "2024-11-01"
 */

const CORS = {
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
    "Access-Control-Allow-Headers": "*",
    "Cache-Control": "no-store",
};

export default {
    async fetch(request) {
        const url = new URL(request.url);
        if (request.method === "OPTIONS") return new Response(null, { headers: CORS });

        switch (url.pathname.replace(/\/+$/, "")) {
            case "/down": {
                const bytes = Math.min(parseInt(url.searchParams.get("bytes") || "26214400", 10) || 0, 104_857_600);
                const chunk = new Uint8Array(65536); // 64 KiB of zeros (compresses poorly enough for a rough test)
                let remaining = bytes;
                const stream = new ReadableStream({
                    pull(controller) {
                        if (remaining <= 0) { controller.close(); return; }
                        const n = Math.min(chunk.length, remaining);
                        controller.enqueue(n === chunk.length ? chunk : chunk.subarray(0, n));
                        remaining -= n;
                    },
                });
                return new Response(stream, { headers: { ...CORS, "Content-Type": "application/octet-stream", "Content-Length": String(bytes) } });
            }
            case "/up": {
                const buf = await request.arrayBuffer();
                return new Response(JSON.stringify({ received: buf.byteLength }), { headers: { ...CORS, "Content-Type": "application/json" } });
            }
            case "/ping":
                return new Response(null, { status: 204, headers: CORS });
            case "/meta": {
                const cf = request.cf || {};
                return new Response(JSON.stringify({ colo: cf.colo, country: cf.country, city: cf.city, ip: request.headers.get("cf-connecting-ip") }), { headers: { ...CORS, "Content-Type": "application/json" } });
            }
            case "/proxy": {
                const target = url.searchParams.get("url");
                if (!target) return new Response("missing url", { status: 400, headers: CORS });
                const upstream = await fetch(target, { method: request.method, body: request.method === "POST" ? request.body : undefined });
                return new Response(upstream.body, { headers: { ...CORS, "Content-Type": upstream.headers.get("Content-Type") || "application/octet-stream" } });
            }
            default:
                return new Response("BenchRig speed Worker. See /down, /up, /ping, /meta, /proxy.", { headers: { ...CORS, "Content-Type": "text/plain" } });
        }
    },
};
