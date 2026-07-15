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
players is routed through a single choke point (`Game.applyDamage`) — see §10.

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

## 9. Play modes

### Shipped — Local PVP
The MVP mode: two players, one touchscreen, boards facing each other (§2).

### Planned

- **Single Player — Endless Waves.** No opponent: survive an ever-escalating
  wave schedule and chase a high score (score from waves cleared with speed
  and perfect-clear multipliers). Character kits get **PvE variants** — with
  no opponent to harm, each ability is re-specced to benefit the player while
  keeping the character's identity:
  - Redbeard: cannons blast buttons off *your* board (auto-pop the next in
    order).
  - Frostjaw: freezes the **fuse**, not a board — buys time.
  - Inkeye: the curse becomes second sight — briefly reveals order hints.
  Doubles as onboarding and solo skill training; the high-score chase is the
  retention hook.
- **Single Player — PVE ladder.** Climb a ladder of CPU pirates of rising
  skill (bot = configurable tap delay + error rate — the same bot as the
  balance sims). Optional light story wrapper: climb the fleet, dethrone the
  Tapper King.
- **Multiplayer — online PVP.** Device vs device; roadmap Phase 3.
- **Multiplayer — co-op PVE.** Two pirates vs shared escalating waves
  (shared health pool, no cross-damage). Open question whether demand
  exists — cheap to prototype locally after Endless Waves lands; validate
  before investing.

## 10. Architecture (multiplayer-ready)

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

## 11. Balance notes & levers

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

### Open question: should waves speed up as a PvP match progresses?

Ramping by *wave number* beyond the current schedule is the wrong axis — the
faster player would face harder waves than the slower one, punishing the
winner. The promising variant is a **shared, time-based "storm" ramp**:
after ~2.5 minutes of match time, both players' clear windows shrink a few
percent every 30 s, symmetrically. It caps match length, raises late-game
drama, and makes the −30 auto-clear more common exactly when health is low.
Needs playtesting (Phase 5): implement behind a config flag; watch the
match-length distribution and whether comebacks still happen under the storm.

---

## 12. Retention, progression & economy

### What keeps players coming back

Layered, from strongest to lightest:

1. **Rivalry.** A 2-minute chaos duel's natural retention is "run it back, I
   want revenge." Everything that feeds rematch culture is retention work and
   cheap: head-to-head records vs. a specific opponent, post-match stat cards
   ("perfect clears 4–1", "fastest wave 1.9 s"), instant rematch.
2. **Mastery.** Per-character stats and the ult economy's skill expression
   give a visible "I'm getting better" curve. Later: ranked ladder.
3. **Collection & status.** Characters, skins, voice lines, and **ships** —
   cosmetics the *opponent must look at* are status goods, the strongest buy
   motivation in a 1v1 game.
4. **Cadence.** Daily first-win bonus; one weekly challenge ("win with Inkeye
   without using a basic"). Light, never chore-like.

### Progression system (design)

- **Character XP:** playing a character earns XP for that character
  (win > loss; perfect-clear and speed bonuses). Levels unlock **alternate
  abilities as sidegrades, not upgrades** — a leveled character has more
  *options* (pick basic/ult loadout pre-match), never more power. Power
  behind grind breaks PvP and churns new players. Cosmetic milestones
  (titles, portrait frames, victory taunts) mark levels too.
  - Example: Redbeard L5 unlocks *Chain Shot* (2 weaker cannonballs that
    also nudge button positions) as an alternative to Pot Shot.
- **Currency:** earned by both players every match (winner ~2×, bonuses for
  perfect clears and challenges). Spent on: new characters, skins, voice /
  taunt packs, and **ships** — board themes (deck texture, divider style,
  ambient audio) rendered like any other theme object.
- **Persistence:** local-first (localStorage profile) so progression ships
  before accounts exist; migrate to server profiles when online lands.
- Premium currency, if ever added, buys cosmetics only.

The code is already shaped for the content side: new character = one
`CHARACTERS` entry (kit) + one `AVATARS` entry (look + voice); a ship = a
theme entry (textures + palette + ambience). Candidate fourth kit already in
the engine: Gunpowder pirate (basic: powder 1 button / ult: powder
everything).

---

## 13. Roadmap

Phased by dependency: specs before art, online before deep progression.

### Phase 1 — Visibility polish *(days; no dependencies)*
- [ ] Brighten the whole scene: lift the night overlay on the deck, raise
      lantern glow, brighten button wood. Same palette, higher exposure.
- [ ] Health bar rework: taller, higher-contrast fill, **HP number inside
      the bar** (outlined for legibility), flash on damage.

### Phase 2 — Asset production pipeline *(parallel track; art drops in over time)*
Specs first, then art: the code prefers sprites and falls back to the current
canvas drawing, so the game never blocks on art.
- [ ] Asset specs doc: file formats, sizes, naming (`docs/ASSET_SPECS.md`).
- [ ] **Icon assets** — button marks + ability icons, transparent PNG 256 px,
      one file per mark; slots in behind `drawButtonFace`.
- [ ] **Character assets** — portrait sprite sheet per character: 6 mood
      states (idle/blink/angry/shock/peek/smile) × 2–3 frames, fixed grid.
      The reaction system already keys on these mood names.
- [ ] **Character animations** — frame cycling per mood row; cast + victory
      poses as extra rows later.
- [ ] **Character sounds** — per-character recording spec: 4 moods × 2–3
      takes + ~4 taunt lines, mono, normalized; routed via `AVATARS.voice`.
- [ ] Best authentic source: bake the original 3D pirate models to sprite
      sheets in Unity (camera + animation capture per character).

### Phase 3 — Device vs Device *(the milestone the remake was architected for)*
- [ ] Determinism audit: all gameplay randomness through one seeded RNG.
- [ ] WebSocket server: create/join by **room code** (quick match later),
      event relay, shared seed + synced match start.
- [ ] Protocol = the existing choke points (`applyDamage`, `useAbility`) +
      wave/state events, **ordered by client timestamps** so ping never
      decides a photo-finish.
- [ ] Remote layout: own board fullscreen; enemy as a status strip (HP,
      wave, ult ring, their avatar reacting to what you do to them).
- [ ] Disconnect/reconnect handling; latency feel testing on real phones.
- [ ] Emotes (tap your own avatar to taunt — voice grunt on enemy screen).

### Phase 4 — Progression & economy v1 *(after Phase 3; local-first parts earlier)*
- [ ] Local profile: per-character XP, currency, unlock state (localStorage).
- [ ] Match rewards + post-match stat card (doubles as rivalry fuel).
- [ ] Loadout picker on character select (unlocked ability variants).
- [ ] First purchasables: 4th character, one skin per character, one ship.
- [ ] Server profiles once online accounts exist.

### Phase 5 — Live tuning & retention systems *(ongoing once 3+4 exist)*
- [ ] **Storm mode playtest** (see §11 open questions): time-based symmetric
      ramp behind a config flag; measure match-length distribution, fail
      rates, comeback frequency.
- [ ] Head-to-head records, daily first win, weekly challenge.
- [ ] Ranked ladder / seasonal reset if the population supports it.

### Content backlog (any phase)
- [ ] Wave schedules as data (JSON) — the original `CreateWaves` comment
      wished for this; enables modes and difficulty presets.
- [ ] **Endless Waves** single-player mode with PvE ability variants (§9) —
      good early: no netcode needed, doubles as onboarding + high-score hook.
- [ ] **PVE ladder** vs CPU pirates (§9) — the bot doubles as the
      balance-sim engine.
- [ ] Co-op PVE prototype once Endless Waves exists (§9 — validate interest).
- [ ] Game modes: FastClickMode (unfinished in the Unity original — no
      order, pure speed), best-of-3 rounds, sudden death.
- [ ] Juggle/freeze/ghost bespoke sound recordings (currently synth).
- [ ] Character-select kit tooltips; pause/forfeit; mute toggle.

### Tech backlog
- [ ] Extract the inline script into modules with a tiny build step when the
      multiplayer server lands (keep the single-file artifact as an output).
- [ ] Automated balance sims: scripted bots at various skill levels playing
      thousands of matches (the Playwright harness is the seed of this).

---

## 14. Reference: current tuning constants

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
