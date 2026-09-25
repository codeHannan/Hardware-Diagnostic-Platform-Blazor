# BenchRig speed-test Worker

A free Cloudflare Worker that gives BenchRig a CORS-enabled speed-test backend you
control. Browsers can only run speed tests against servers that send
`Access-Control-Allow-Origin: *`; most public speed servers don't, so this Worker
fills the gap (and can proxy ones that don't, enabling location-specific tests).

## Deploy (free)

```bash
npm i -g wrangler
wrangler login
cd cloudflare-worker
wrangler deploy
```

Wrangler prints a URL like `https://benchrig-speedtest.<your-subdomain>.workers.dev`.

## Use it in BenchRig

Edit `BenchRig.App/wwwroot/network-servers.json` and add your Worker (and any
proxied LibreSpeed servers). These appear in the **Test server** dropdown.

```json
[
  {
    "key": "myedge",
    "name": "My Cloudflare Worker",
    "location": "Cloudflare edge",
    "type": "worker",
    "url": "https://benchrig-speedtest.your-subdomain.workers.dev"
  },
  {
    "key": "london",
    "name": "London (proxied)",
    "location": "via Worker proxy",
    "type": "ls",
    "url": "https://benchrig-speedtest.your-subdomain.workers.dev/proxy?url=https://lon.example-librespeed.net",
    "dlURL": "backend/garbage.php",
    "ulURL": "backend/empty.php",
    "pingURL": "backend/empty.php"
  }
]
```

- `type: "worker"` uses the Worker's own `/down`, `/up`, `/ping` endpoints.
- `type: "ls"` targets a LibreSpeed-compatible server (optionally through the
  Worker's `/proxy?url=` to add CORS for a specific upstream location).

You can also add a server at runtime from the **Network** page via **+ Add**.
