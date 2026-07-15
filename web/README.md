# Pirate Tapper Showdown — web remake

A browser remake of the original Unity game, rebuilt for the road to online multiplayer.

## Run it

Open `index.html` in any browser — no build step needed; it loads the art and
audio from `assets/` (originals from the Unity project's `Assets/` folder,
transcoded for the web). Best experienced on a touchscreen laid flat between
two players.

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

New in this version: six single-use offensive power-ups per player —
Ice Block, Monkey Juggler, Cannon Blast, Gunpowder Barrel, Kraken Ink, Ghost Curse.

## Avatars

Each player has an animated pirate avatar in their HUD (canvas-drawn, no image
assets) that blinks, bobs, and reacts to the board: angry on failed waves and
juggles, shocked when frozen or inked, grinning when casting a power-up or
winning. Avatars are data-driven — the `AVATARS` array bundles each character's
look and voice set, so adding purchasable characters later (custom visuals,
sounds, and eventually per-avatar power-up loadouts) means adding entries, not
rewriting rendering.

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
