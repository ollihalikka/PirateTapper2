# Character portrait overrides

Drop art here to replace a pirate's drawn portrait (shown in the HUD and on the
character-select cards). Two options — pick per character:

## Option A — single portrait (simplest)

`<charId>.png` — one square PNG, used for every expression (static, no
animation). Great for getting your character in fast.

- **Format:** PNG, square, ~256×256 px (opaque or transparent both fine).
- The image is clipped into a circle, so keep the face centered.

## Option B — animated sprite sheet (reacts to the game)

`<charId>.png` + `<charId>.json`. The PNG is a grid of equal frames; the JSON
says which rows/columns are which mood. The game already switches moods
(idle, blink, smile, angry, shock, peek) in response to play — see
`docs/ASSET_SPECS.md` for what triggers each.

`<charId>.json`:

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

Only `idle` is required; any missing mood falls back to `idle`.

> Sprite sheets load over HTTP or in the built artifact. Opening
> `index.html` directly from disk (`file://`) can't read the `.json` (browser
> security) — a single portrait (Option A) works from disk too.

## Character IDs

`redbeard` (P1 default), `blackhat` (Inkeye), `frostjaw`. Add a new pirate by
adding an entry to `AVATARS`/`CHARACTERS` in `web/index.html`, then dropping
`<newId>.png` here.
