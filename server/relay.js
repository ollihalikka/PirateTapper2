#!/usr/bin/env node
/* Pirate Tapper Showdown — WebSocket relay for device-vs-device play.
 *
 * Deliberately dumb: it pairs two sockets by a 4-digit room code and
 * forwards opaque messages between them. All game logic lives in the
 * clients (each device simulates its own board; see docs/GDD.md §10).
 *
 *   npm install && npm start          # listens on :8777 (PORT overrides)
 *
 * Protocol (JSON):
 *   client → { t:'host' }                → { t:'room', code }
 *   client → { t:'join', code }          → both get { t:'paired', role }
 *                                          (role: 'host' | 'guest')
 *            bad code                    → { t:'err', why:'no_such_room' }
 *   client → { t:'relay', d:<any> }      → peer gets { t:'msg', d }
 *   peer socket closes                   → { t:'peer_gone' }
 */
'use strict';
const http = require('http');
const { WebSocketServer } = require('ws');

const PORT = Number(process.env.PORT) || 8777;
const rooms = new Map();   // code -> { host, guest }

const send = (ws, obj) => { if (ws && ws.readyState === 1) ws.send(JSON.stringify(obj)); };

function newCode(){
  for (let tries = 0; tries < 100; tries++){
    const code = String(1000 + Math.floor(Math.random() * 9000));
    if (!rooms.has(code)) return code;
  }
  return null;
}

// A tiny HTTP server shares the port so hosts (e.g. Render) can health-check
// over plain HTTP, while the WebSocket server handles the upgrade requests.
const httpServer = http.createServer((req, res) => {
  res.writeHead(200, { 'content-type': 'text/plain' });
  res.end('pirate-tapper relay ok — ' + rooms.size + ' room(s) open\n');
});
const wss = new WebSocketServer({ server: httpServer });
httpServer.listen(PORT, () => console.log(`pirate-tapper relay listening on :${PORT}`));

wss.on('connection', ws => {
  ws.isAlive = true;
  ws.on('pong', () => { ws.isAlive = true; });

  ws.on('message', raw => {
    let m;
    try { m = JSON.parse(raw); } catch (e) { return; }

    if (m.t === 'host'){
      const code = newCode();
      if (!code){ send(ws, { t:'err', why:'server_full' }); return; }
      ws.room = code; ws.role = 'host';
      rooms.set(code, { host: ws, guest: null });
      send(ws, { t:'room', code });

    } else if (m.t === 'join'){
      const room = rooms.get(String(m.code));
      if (!room || room.guest || room.host.readyState !== 1){
        send(ws, { t:'err', why:'no_such_room' });
        return;
      }
      ws.room = String(m.code); ws.role = 'guest';
      room.guest = ws;
      send(room.host, { t:'paired', role:'host' });
      send(ws,        { t:'paired', role:'guest' });

    } else if (m.t === 'relay'){
      const room = rooms.get(ws.room);
      if (!room) return;
      send(ws === room.host ? room.guest : room.host, { t:'msg', d: m.d });
    }
  });

  ws.on('close', () => {
    const room = rooms.get(ws.room);
    if (!room) return;
    send(ws === room.host ? room.guest : room.host, { t:'peer_gone' });
    rooms.delete(ws.room);
  });
});

// reap dead sockets so abandoned rooms free their codes
setInterval(() => {
  for (const ws of wss.clients){
    if (!ws.isAlive){ ws.terminate(); continue; }
    ws.isAlive = false;
    ws.ping();
  }
}, 30000);
