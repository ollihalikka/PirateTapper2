# Pirate Tapper Showdown — online relay

A tiny WebSocket relay that pairs two devices by a 4-digit room code and
forwards opaque messages between them. It holds **no game state** — each
device runs the authoritative simulation of its own board and mirrors the
enemy board from the event stream (see `docs/GDD.md` §10).

## Run

```sh
cd server
npm install
npm start          # listens on :8777  (PORT env var overrides)
```

## Point the client at it

The web client picks the relay URL from `?ws=` on the page URL, else defaults
to `ws(s)://<page-host>:8777`. For local two-device testing on a LAN:

```
http://<your-ip>:<web-port>/index.html?ws=ws://<your-ip>:8777
```

Host on one device (reads out the code), Join on the other.

## Protocol

Pairing: `{t:'host'}` → `{t:'room',code}`; `{t:'join',code}` → both get
`{t:'paired',role}`. Gameplay: `{t:'relay',d}` is forwarded to the peer as
`{t:'msg',d}`. A dropped socket sends the peer `{t:'peer_gone'}`.
