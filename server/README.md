# Pirate Tapper Showdown — online relay

A tiny WebSocket relay that pairs two devices by a 4-digit room code and
forwards opaque messages between them. It holds **no game state** — each
device runs the authoritative simulation of its own board and mirrors the
enemy board from the event stream (see `docs/GDD.md` §10).

It also answers plain HTTP GET with `200 OK` on any path, so platform health
checks (e.g. Render) pass; WebSocket upgrades share the same port.

## Run locally

```sh
cd server
npm install
npm start          # listens on :8777  (PORT env var overrides)
```

## Point the client at it

The web client picks the relay URL from `?ws=` on the page URL, else defaults
to `ws(s)://<page-host>:8777`. For local two-device testing on a LAN:

```
http://<your-ip>:<web-port>/web/?ws=ws://<your-ip>:8777
```

Host on one device (reads out the code), Join on the other.

## Deploy to Render (public `wss://` for the GitHub Pages site)

The HTTPS Pages site can only reach a **secure** `wss://` relay, and GitHub
Pages can't run Node — so the relay lives on Render as a **Web Service**.

1. Render → **New → Blueprint**, pick this repo. The root `render.yaml`
   creates `pirate-tapper-relay` (free plan, `rootDir: server`,
   `npm install` / `npm start`, health check `/`). Or **New → Web Service**
   by hand: Root Directory `server`, Build `npm install`, Start `npm start`.
2. Render assigns a URL like `https://pirate-tapper-relay.onrender.com`.
   The relay is then reachable over TLS at
   `wss://pirate-tapper-relay.onrender.com`.
3. Play online by appending it to the game URL (note: **no port** — Render
   terminates TLS on 443):

   ```
   https://ollihalikka.github.io/PirateTapper2/web/?ws=wss://pirate-tapper-relay.onrender.com
   ```

   (Or bake that URL in as the client default so the bare `/web/` works.)

Free-tier services sleep after ~15 min idle; the first connection wakes it
(a ~30–60 s cold start), then it's instant.

## Protocol

Pairing: `{t:'host'}` → `{t:'room',code}`; `{t:'join',code}` → both get
`{t:'paired',role}`. Gameplay: `{t:'relay',d}` is forwarded to the peer as
`{t:'msg',d}`. A dropped socket sends the peer `{t:'peer_gone'}`.
