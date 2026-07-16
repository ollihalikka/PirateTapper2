# Pirate Tapper Showdown — web remake

A browser remake of the original Unity game, rebuilt for the road to online multiplayer.

## Run it

Open `index.html` in any browser — no build step needed; it loads the art and
audio from `assets/` (originals from the Unity project's `Assets/` folder,
transcoded for the web). Best experienced on a touchscreen laid flat between
two players.

To make one portable file with everything inlined (for sharing/hosting):
`python3 build-single-file.py`.

## Adding your own art & audio

The game draws everything procedurally, but every visual and voice is
overridable: drop a file into `assets/` with the right name and it replaces the
drawn version (missing files fall back, so add them one at a time). See
[`../docs/ASSET_SPECS.md`](../docs/ASSET_SPECS.md) for the full guide, and the
READMEs in `assets/icons/` and `assets/characters/`.

## Gameplay

Two players share the screen, one half each (the top half is rotated 180° so the
players sit facing each other). Buttons surface in waves, one by one — watch the
order they appear, then pop them in that same order. There are no numbers or
hints: the eight distinct button faces (skull, anchor, coin, bottle, dynamite,
pistol, rudder, cannonballs — the same set as the Unity `Poppable_Buttons`
prefabs) are what your memory latches on to. Every correct pop damages the
opponent. Ported from the Unity original:
300 HP, the 20-wave schedule from `GameManager.CreateWaves()` (endless overtime
after wave 20), −30 for letting a wave time out, −10 for wrong or stray taps.

New in this version: playable characters. Before each match both players pick
a pirate on their own half of the screen. Each character has two abilities:

- a **basic** on an 8-second cooldown (minor effect), and
- an **ultimate** charged by clearing waves — charge = 10 base + up to 8 for
  speed (time left on the fuse) + 7 for a perfect clear (no mis-taps), so an
  ultimate comes online roughly 2–4 times per match. Charge gained while full
  is wasted, so hoarding costs you.

| Character | Basic (8s CD) | Ultimate |
|---|---|---|
| **Redbeard** the Cannoneer | Pot Shot — one cannonball, 6 dmg | Broadside — five cannonballs, 8 dmg each |
| **Frostjaw** the Iceberg | Cold Snap — 0.8s freeze | Deep Freeze — 2.6s freeze while their wave timer burns |
| **Inkeye** the Cursed | Ink Spit — one ink blob, 2.5s | Kraken's Curse — 4s of wiped faces + juggled buttons + ink |

## Avatars

Each player has an animated pirate avatar in their HUD (canvas-drawn, no image
assets) that blinks, bobs, and reacts to the board: angry on failed waves and
juggles, shocked when frozen or inked, grinning when casting a power-up or
winning. Characters are data-driven — `CHARACTERS` bundles kit definitions and
`AVATARS` the looks and voice sets, so adding purchasable characters later
(custom visuals, sounds, and ability loadouts) means adding entries, not
rewriting rendering or game logic.

## Multiplayer readiness

The code is structured so a network layer can be added without rewriting the game:

- `Board` is an independent simulation of one player's half — all
  reaction-critical input is resolved locally on the tapping player's device.
- Everything that crosses between players (damage, power-up effects) flows
  through `Game.applyDamage` / `Game.usePowerup`, which are the future
  message boundaries.
- Next step: a small WebSocket server for matchmaking, a shared RNG seed +
  synced start time, and timestamp-ordered damage events so connection speed
  can never decide a photo-finish.
