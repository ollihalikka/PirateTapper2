# Asset Specs — how to add art & audio

The game draws everything procedurally by default, but every visual and voice
is **overridable**: drop a file into `web/assets/` with the right name and it
replaces the drawn version. Anything missing falls back to the procedural
art, so you can add assets **one at a time** and never break the build.

There is no build step for the repo version — add the file, reload the page.
(The shareable single-file artifact is produced by a script that inlines every
asset; see §7.)

---

## 0. Quick start

| I want to replace… | Put a file here | Format |
|---|---|---|
| A button's mark | `web/assets/icons/<kindId>.png` | transparent PNG, 256² |
| A pirate's face (static) | `web/assets/characters/<charId>.png` | PNG, 256² |
| A pirate's face (animated) | `<charId>.png` + `<charId>.json` | sheet + manifest |
| A pirate's voice | `web/assets/<voiceId>-<mood><take>.mp3` | mono mp3 |
| The deck background | `web/assets/planks.jpg` | JPG, ~1024² tileable |

Reload over a local server (`cd web && python3 -m http.server`, open
`http://localhost:8000`). Opening the file directly (`file://`) works for
everything **except** animated sprite sheets (browsers block the JSON read
from disk) — use a server or the artifact for those.

---

## 1. IDs you can target

**Button kinds** (16 in the pool, one icon each):
`skull` · `anchor` · `coin` · `bottle` · `barrel` · `pistol` · `rudder` ·
`balls` · `cannon` · `chest` · `hook` · `map` · `ship` · `skull_flag` ·
`spyglass` · `swords`

**Characters / avatars** (3): `redbeard` · `inkeye` · `whitedeath`
(the avatar `id`s in `AVATARS`).

**Voices** (2 sets today): `p1` · `p2` — declared in `VOICES` in `index.html`.

To add a *new* character or voice, add one entry to `CHARACTERS` / `AVATARS` /
`VOICES` in `web/index.html`, then drop its files. Everything else
(reactions, ability wiring, select screen) already keys off those entries.

---

## 2. Button icons

`web/assets/icons/<kindId>.png`

If the file exists it becomes the **whole button face**; if not, the game
draws a procedural wooden disc with a painted mark as fallback.

- **Format:** PNG, square, **256×256 px**.
- **Full-face, own background:** the image fills the entire button (drawn at
  ~108% of the button diameter so its circular art meets the edge). **Bake a
  bold background color into the PNG** — a distinct hue per button. Color is a
  memory cue *alongside* the mark, so make each one clearly different in both.
- **Safe area:** keep the important art inside a centered circle; the extreme
  corners fall just outside the button.
- **Style:** the shipped set is a bold mark on a solid colored disc with a
  soft rim/shadow — match it for a coherent look. Flat, high-contrast art
  reads best at ~40–50 px on screen; fine detail disappears.

Icons also appear in the start-screen preview grid automatically.

> The 16 shipped icons live in `web/assets/icons/`. To change which are in
> play, edit the `BUTTON_KINDS` list in `web/index.html`
> (each is `{ id, bg }`; `bg` is only the pop-particle color).

---

## 3. Character portraits & animation

Portraits show in the player's HUD (small) and on the character-select cards
(large), clipped into a circle. Two ways to provide them:

### Option A — single portrait (static, simplest)

`web/assets/characters/<charId>.png`

- PNG, square, **256×256 px** (transparent or opaque both fine).
- Used for **every** expression — the pirate won't emote, but it's the fastest
  way to get custom art in.
- Center the face; corners are clipped off by the circle.

### Option B — sprite sheet (animated, reacts to the game)

`web/assets/characters/<charId>.png` + `web/assets/characters/<charId>.json`

The PNG is a grid of equal-size frames. The JSON maps **moods** to rows (and
optional start columns) and says how many frames each mood animates through.

```json
{
  "frameW": 256,
  "frameH": 256,
  "fps": 6,
  "moods": {
    "idle":  { "row": 0, "frames": 2 },
    "blink": { "row": 0, "col": 2, "frames": 1 },
    "smile": { "row": 1, "frames": 2 },
    "angry": { "row": 2, "frames": 2 },
    "shock": { "row": 3, "frames": 2 },
    "peek":  { "row": 4, "frames": 1 }
  }
}
```

- `frameW`/`frameH` — pixel size of one cell (the sheet is a grid of these).
- `fps` — animation speed for multi-frame moods.
- Each mood: `row` (0-based), optional `col` (start column, default 0),
  `frames` (how many columns to cycle from `col`).
- **Only `idle` is required.** Any mood you omit falls back to `idle`.

**Frame math** (what the engine does): for mood `m`,
`frame = floor(now·fps) mod m.frames`, source cell =
`( (m.col + frame)·frameW , m.row·frameH )`.

### Mood triggers (what the game will show)

| Mood | When it fires |
|---|---|
| `idle` | default; also random idle chatter every 5–15 s |
| `blink` | brief, random, only while idle |
| `smile` | cleared a wave, cast an ability, won the match |
| `angry` | wave timed out, got juggled, lost the match |
| `shock` | frozen, inked, or gunpowdered |
| `peek` | ghost-cursed |

Design tip: the differences that matter are **eyes + mouth + brow**. Even 2
frames per mood (a subtle idle bob, an open-mouthed shock) sell it.

---

## 4. Character voices

The original pirate voice grunts live in `web/assets/` as
`<voiceId>-<mood><take>.mp3`. A character's `voice` field (`'p1'` / `'p2'`)
picks a set; the engine plays a random take for the matching mood.

- **Naming:** `<voiceId>-<mood><take>.mp3`, e.g. `p1-angry1.mp3`,
  `p1-angry2.mp3`. Moods: `angry`, `peek`, `shock`, `smile`. Takes: `1`, `2`
  (add more by extending the list in `SFX_FILES`).
- **Format:** mono MP3, ~64 kbps is plenty, normalized to a consistent
  loudness, trimmed tight (no leading silence). Keep each grunt < 1.5 s.
- **Add a new voice:** add its id to `VOICES` in `index.html`, drop
  `<id>-angry1.mp3` … `<id>-smile2.mp3` (4 moods × 2 takes = 8 files), and set
  a character's `voice` to that id.
- **Taunts** (planned, §Roadmap): same idea, a `<voiceId>-taunt<take>.mp3`
  set fired by the emote button.

Non-voice SFX (`pop`, `wrong`, `boom`, `click`, `cast`) and music
(`music-tavern.mp3`, `music-battle.mp3`, `sea-waves.mp3`) live flat in
`web/assets/` and can be swapped by replacing the file of the same name.

---

## 5. Deck / ship backgrounds

`web/assets/planks.jpg` is the deck texture, tinted at runtime with each
player's lantern color. Replace it to reskin the whole board.

- **Format:** JPG, ~1024×1024, ideally horizontally tileable (it's tiled to
  fill wide screens).
- Keep it fairly **even and mid-toned** — busy or high-contrast textures fight
  the buttons and hurt readability. The game darkens and vignettes it at
  runtime.
- **Ships** (planned monetization, see GDD §12) will generalize this into a
  theme object: deck texture + divider style + ambient audio, selected per
  player. Until that lands, `planks.jpg` is the single shared deck.

---

## 6. Sizes & performance budget

- Icons/portraits at **256²** are ample; the largest on-screen use is a
  select-card portrait (~150 px). Don't ship 2048² art — it bloats the
  artifact and the download for no visible gain.
- Sprite sheets: keep total sheet dimension ≤ **2048²** (safe on all mobile
  GPUs). Six moods × 2 frames at 256² fits in 1024×768.
- PNGs: run them through a compressor (`pngquant`, `oxipng`, TinyPNG) — art is
  the bulk of the download. The whole current asset set (all audio + textures)
  is ~2 MB; keep it there.
- Everything is drawn on a 2D canvas each frame; a handful of `drawImage`
  calls per frame is free. There is no per-asset runtime cost to worry about.

---

## 7. How the shareable artifact picks up your assets

`python3 web/build-single-file.py` walks `web/assets/` **recursively** and
inlines every `.png` / `.jpg` / `.mp3` / `.json` as a base64 data URI, keyed
by its path relative to `assets/` (e.g. `icons/skull.png`), producing one
portable HTML file. So any asset you add under `web/assets/` is automatically
included the next time you run it — no code changes. READMEs and other
non-asset files are skipped. (A published build only loads assets that were
actually bundled, so it never 404s for art you haven't made yet.)

---

## 8. Testing an asset you added

1. `cd web && python3 -m http.server 8000`
2. Open `http://localhost:8000` and start a match.
3. In DevTools console, confirm it loaded:
   - button icon: `IMG['icon:skull']` → an `<img>`, not `null`
   - portrait: `IMG['char:redbeard']`
   - sheet: `SHEETS['frostjaw']` → `{ img, meta }`
4. If it's `null`, check the filename/path and that the file is a valid image.

Loaded art appears immediately — no cache-busting needed beyond a hard reload.
