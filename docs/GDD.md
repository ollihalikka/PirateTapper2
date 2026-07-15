# Pirate Tapper Showdown — Game Design Document

**Version:** MVP (web remake, July 2026)
**Playable build:** `web/index.html` (self-contained HTML5 canvas game)
**Predecessor:** Unity 2021.3 project in this repository (`Assets/`)

---

## 1. Vision

A two-player tap duel of **absolute chaos and mayhem**. You're trying to tap
faster than your opponent by any means necessary, while both of you sabotage
each other. Matches are short, loud, physical, and played face to face —
one touchscreen laid flat between two pirates.

Design pillars:

1. **Memory under pressure.** The game never tells you the tap order — you
   watch buttons surface and repeat the sequence from memory. Everything that
   raises your heart rate makes remembering harder.
2. **Sabotage is the meta.** Direct speed wins games, but the real fun is
   wrecking the opponent's memory and timing at the worst possible moment.
3. **Readable chaos.** However wild the board gets, every element (button
   identity, health, timers, ability state) must be readable in a glance.

Long-term goal: **online 1v1 multiplayer** with purchasable characters.

---

## 2. Core loop

Two boards, one per player, run **independent** wave simulations. The top
board renders rotated 180° so players sit facing each other.

Per wave, per player:

1. **Forming (memorize).** `n` buttons surface one at a time, translucent and
   untappable. Spawn interval = `formTime / (n + 1)`; the first spawns after
   half an interval. *This is the memorize window — spawn order = tap order.*
2. **Active (recall).** All buttons turn opaque simultaneously; a fuse bar
   starts burning (`clearTime`, currently 5 s on every wave).
   - Tap in spawn order: each correct pop deals `dmgPerPop` to the opponent.
   - Wrong button: **−10** own health, mistap flag set.
   - Bare deck tap (in the play area): **−10** own health, mistap flag set.
3. **Resolution.**
   - All popped → wave clear: ult charge awarded (§5), next wave after 0.6 s.
   - Fuse runs out → auto-clear: **−30** own health, board wipes, 1 s stun,
     next wave after 0.5 s.

First player to **0 HP loses** (max HP **300**). Damage crossing between
players is routed through a single choke point (`Game.applyDamage`) — see §9.

### Wave schedule

Ported from the Unity `GameManager.CreateWaves()`. Columns: buttons, seconds
to form, seconds to clear, damage per correct pop.

| Waves | n | form (s) | clear (s) | dmg/pop |
|---|---|---|---|---|
| 1–2 | 2 | 1.0 / 0.8 | 5 | 5 |
| 3–5 | 3 | 1.0 / 0.8 / 1.2 | 5 | 5 |
| 6–11 | 4 | 0.8–1.2 | 5 | 7 |
| 12–20 | 5 | 0.8–1.2 | 5 | 10 |
| 21+ (overtime) | 6 | 0.7 | 4.6 − 0.15·k, floor 3.2 | 12 |

The original Unity build simply stopped after wave 20; the remake adds the
endless overtime so matches always terminate.

---

## 3. Buttons

Eight button identities — the same set as the Unity `Poppable_Buttons`
prefabs. **No numbers, no hints**: identity is the memory hook. Each button
carries **two redundant anchors** (helps memory and doubles as colorblind
support): a large painted mark and a unique face color.

| Button | Mark color | Face color |
|---|---|---|
| Skull | bone white | navy |
| Anchor | dark teal | sand |
| Coin (doubloon) | gold | royal purple |
| Rum bottle (XXX) | green glass | amber |
| Dynamite | red | sea teal |
| Flintlock pistol | black iron | dusty rose |
| Ship's rudder | dark walnut | steel blue |
| Cannonballs | black | olive |

Visual construction: every button is a disc of the deck-plank texture
("sawn from the same tree", per-button grain offset) with a carved rim, the
painted face color, and the mark filling ~85 % of the face.

Each wave deals a random subset (Fisher–Yates shuffle) of the eight kinds, so
"the rum bottle came third" is a fresh fact every wave.

Placement: random inside the play area, minimum spacing 2.35 × button radius,
up to 300 retries per button (mirrors the original's 500-retry loop).
Button radius scales with board size, clamped 22–46 px.

---

## 4. Characters (MVP: 3)

Each player picks a character on the **select screen** (three cards per
board half, tapped on the player's own side; match starts ~1 s after both
have chosen; rematch returns to select).

A character = **look + voice set + two abilities**:

- **Basic** — 8 s cooldown, minor effect. First use unlocked 4 s into the
  match (prevents pre-wave sniping).
- **Ultimate** — charged by wave clears (§5), fires at 100 charge.

| Character | Fantasy | Basic (8 s CD) | Ultimate |
|---|---|---|---|
| **Redbeard** *the Cannoneer* | reliable aggression | 💣 **Pot Shot** — 1 cannonball, 6 dmg | 💥 **Broadside** — 5 cannonballs, 8 dmg each (40 total) over ~1.9 s |
| **Frostjaw** *the Iceberg* | tempo control | ❄️ **Cold Snap** — freeze enemy board 0.8 s | 🧊 **Deep Freeze** — freeze 2.6 s; enemy wave fuse keeps burning (threatens the −30 auto-clear) |
| **Inkeye** *the Cursed* | memory attack | 🦑 **Ink Spit** — 1 ink blob hides part of the board 2.5 s | 👻 **Kraken's Curse** — 4 s: all faces wiped to identical grey + positions juggled + 2 ink blobs |

Ability rules:

- Ability buttons remain usable **while frozen** (deliberate counterplay —
  you can retaliate from inside the ice).
- Cannonballs land on random points of the enemy play area with impact
  visuals, smoke, and board shake.
- Freeze makes enemy board taps inert (frost-crack feedback, no damage
  penalty for tapping while frozen).
- Ink blobs are opaque wobbling splats over random spots; taps still work
  underneath — blind-tapping is legal and risky.
- Ghost (face wipe) removes *both* memory anchors (mark and color); position
  memory is all that's left. Juggle then attacks position memory — the two
  are stacked in Kraken's Curse on purpose.

### Effect library (engine capabilities, not all in kits)

`applyIce(secs)`, `applyGhost(secs)`, `applyInk(count, secs)`,
`applyJuggle()`, `applyPowder()` (coats up to 3 buttons; popping a powdered
button still counts but deals 12 self-damage — **currently unassigned**,
reserved for a future character), `scheduleCannons(n, dmg)`.

---

## 5. Ultimate economy

Charge is earned **only by clearing waves** — the mechanic pays out exactly
the skill the game is about:

```
gain = 10 (base)
     + round(8 × fraction of fuse remaining)   // speed bonus
     + 7 if zero mistaps this wave             // perfect bonus
→ 10–25 per wave, 100 to fire
```

Consequences (intended):

- Sloppy-slow play needs ~10 waves per ult; fast-perfect play needs ~4.
  In a typical 15–25-wave match that's **2–4 ultimates per player**.
- **Overflow is wasted** — charge gained at 100 disappears, so sitting on a
  ready ult costs income. Fire it.
- Mistaps hurt twice: −10 HP *and* the lost perfect bonus.
- A failed (timed-out) wave earns nothing.

UI: basic button with cooldown pie-sweep; larger ultimate button with a gold
charge ring, ready-glow pulse, "+N⚡" income floats and an "⚡ ULTIMATE READY"
announcement.

---

## 6. Characters on screen: avatars

Animated canvas-drawn portraits in each player's HUD (no image assets).
Data-driven: `AVATARS` = look + voice set, `CHARACTERS` = kit + avatar ref.

Reactions (synced with the original game's recorded voice grunts):

| Trigger | Face | Voice |
|---|---|---|
| Idle, every 5–15 s | random of angry/peek/shock/smile | matching grunt (port of Unity `PlayerFaceGestures`) |
| Wave timed out / juggled | angry (gritted teeth, tremble) | angry |
| Frozen / inked / powdered | shock (wide eyes, O mouth) | shock |
| Ghost-cursed | peek (one eye squinted) | peek |
| Cast ability / wave clear | grin | — / smile |
| Match end | winner grins, loser fumes (permanent) | smile / angry |

Plus idle bobbing and randomized blinks. Current looks: Redbeard (red
polka-dot bandana, gold earring, auburn beard, voice set 1), Inkeye (gold- and
purple-trimmed tricorn, eyepatch, dark beard, voice set 2), Frostjaw (ice-blue
bandana, white beard, voice set 2).

---

## 7. Presentation

- **Visual world:** night-sea arcade — lantern-lit ship deck (the Unity
  project's `PlanksDiffuse` texture), parchment-gold serif display type,
  teal (P1) vs crimson (P2) lantern glows, wooden divider with rope and
  crossed swords.
- **Audio** (all from the Unity project's `Assets/Audio`, transcoded to mp3):
  - Music: tavern and battle tracks alternating forever (port of Unity
    `MusicPlayer`), over a quiet sea-waves loop.
  - SFX: pops (pitch rises through a wave), `button-wrong` for every
    mistake/timeout, explosions for cannonballs, clicks for ability casts,
    24 pirate voice grunts (2 pirates × 4 moods × 2 takes... 16 in use).
  - Synth accents (WebAudio) for effects with no recording: freeze shimmer,
    ghost wail, juggle chirps, ult-ready ding, win fanfare.
- **Feedback rules:** every damage event floats a red number where it
  happened; every enemy effect announces itself on the victim's board
  (FROZEN / JUGGLED / CURSED / GUNPOWDER); every ability the caster fires is
  named on the caster's board.

---

## 8. Platform & UX

- Single self-contained web build; no framework, no build step. Assets load
  from `web/assets/` (repo) or inline data URIs (single-file distribution,
  built by a small Python script).
- Portrait split-screen, top half rotated 180°. Multitouch via pointer
  events — both players can tap simultaneously.
- **Fullscreen:** requested on the start/rematch gesture (plus portrait
  orientation lock where supported); ⛶ toggle chip in the divider;
  iPhone Safari (no Fullscreen API) gets Add-to-Home-Screen meta tags + hint.
- Reduced-motion preference trims shake and particle counts.

---

## 9. Architecture (multiplayer-ready)

The defining property of the design: **players never touch shared objects.**
Each `Board` is an independent simulation of one half; the opponent affects
you only through explicit events.

- All reaction-critical input (tap detection, order validation, timing) is
  resolved locally — network latency can never slow a player's own taps.
- Everything that crosses between players goes through two functions:
  `Game.applyDamage(target, amount)` and `Game.useAbility(player, which)`.
  These are the future network message boundary.
- Online plan: small WebSocket server for matchmaking + relay; shared RNG
  seed + synced start time so both clients run identical wave schedules;
  damage events ordered by **client timestamps** so a photo-finish is decided
  by when taps actually happened, not connection speed. Client-side tap
  validation is acceptable for casual play; the server can sanity-check
  clear-rates for basic anti-cheat.

---

## 10. Balance notes & levers

Current risks and where to tune:

| Concern | Lever |
|---|---|
| Redbeard (pure damage) vs disruptors | cannonball damage (6 / 8), Broadside count |
| Freeze feels unfair with 5 s fuse | freeze durations (0.8 / 2.6), or pause fuse for part of the freeze |
| Ghost erases both memory anchors | leave a faint face-color tint behind; shorten from 4 s |
| Mistap double-punishment too harsh for kids | wrong-tap damage (10) and/or drop the perfect-bonus loss |
| Ult frequency | base/speed/perfect gains (10/8/7), `ULT_MAX` (100) |
| Empty-deck tap damage causes accidental rage | scale by wave, or disable during forming |
| Overtime difficulty ramp | overtime clear-time decay (0.15 s/wave, floor 3.2 s) |

Balance philosophy: chaos is the product; tune for "outraged laughter", not
for fairness-on-paper.

---

## 11. Monetization direction (not in MVP)

Characters are the monetization unit — players buy pirates whose **visuals,
voice, and kit** appeal to them. The code is already shaped for this:

- New character = one `CHARACTERS` entry (kit) + one `AVATARS` entry
  (look + voice). No rendering or game-logic changes.
- Assets per character: portrait/animation set, ~8+ voice lines
  (4 moods × 2 takes), 2 ability definitions with icons.
- Keep kits **sidegrades, not upgrades** — paid characters must not be
  strictly stronger (chaos variety sells; pay-to-win kills a 1v1 game).

Candidate fourth kit already in the engine: Gunpowder pirate
(basic: powder 1 button / ult: powder everything + a fuse).

---

## 12. Roadmap

### Near term
- [ ] Playtest & balance pass on real tablets (wave pacing without numbers,
      freeze durations, ult frequency).
- [ ] Juggle/freeze/ghost bespoke sound recordings (currently synth).
- [ ] Character-select polish: kit tooltips or a "hold to read abilities"
      panel on the card.
- [ ] Pause/forfeit; mute toggle.

### Online multiplayer (the reason for the remake)
- [ ] WebSocket matchmaking + relay server (room codes first, then quick match).
- [ ] Seeded wave generation + synced match start.
- [ ] Timestamp-ordered damage resolution; disconnect/reconnect handling.
- [ ] Each client renders own board fullscreen + enemy status strip
      (HP, wave, ult charge, avatar reactions).
- [ ] Emotes (tap your own avatar to taunt — voice grunt on enemy screen).

### Content
- [ ] 4th+ characters (gunpowder kit ready in engine).
- [ ] Wave schedules as data (JSON) — the original code already wished for
      this (`CreateWaves` comment); enables game modes and difficulty presets.
- [ ] Game modes: FastClickMode (unfinished in the Unity original — no
      order, pure speed), best-of-3 rounds, sudden-death mode.
- [ ] Single-player practice vs. simple bot (taps with configurable
      delay/error rate) — also useful for balance sims.
- [ ] Original 3D pirate characters baked to sprite sheets in Unity and
      dropped in as premium avatar skins.

### Tech
- [ ] Extract the inline script into modules with a tiny build step once the
      multiplayer server lands (keep the single-file artifact as a build
      output).
- [ ] Deterministic simulation audit (all gameplay randomness through one
      seeded RNG) before netcode.
- [ ] Automated balance sims: scripted bots at various skill levels playing
      thousands of matches (the Playwright test harness in
      `scratchpad/test-game.mjs` is the seed of this).

---

## 13. Reference: current tuning constants

| Constant | Value |
|---|---|
| Max HP | 300 |
| Auto-clear (timeout) damage | 30 |
| Wrong-button tap | 10 |
| Bare-deck tap | 10 |
| Powdered-button self-damage | 12 (unassigned kit) |
| Wave clear window | 5 s (overtime: 4.6 → 3.2 s) |
| Basic cooldown | 8 s (first use at 4 s) |
| Ult charge | 10 + 8·speed + 7·perfect, cap 100 |
| Pot Shot / Broadside | 1×6 dmg / 5×8 dmg |
| Cold Snap / Deep Freeze | 0.8 s / 2.6 s |
| Ink Spit / Kraken's Curse | 1 blob 2.5 s / ghost 4 s + juggle + 2 blobs 4 s |
| Button min spacing | 2.35 × radius |
| Idle grunt interval | 5–15 s |
