# Character event sounds

Drop MP3s here and they play automatically — no code changes. Missing files
fall back to the built-in sounds, so add them one at a time.

One folder per character (avatar id): `redbeard/`, `inkeye/`, `whitedeath/`.

## Slots (exact filenames)

| File | Plays when |
|---|---|
| `select.mp3` | the character is picked on the select screen |
| `start.mp3` | the match begins (right after the VS splash) |
| `hurt1.mp3` … `hurt5.mp3` | the character loses health — one of the existing takes is picked at random (throttled to once per ~1.2 s so pop-spam doesn't scream) |
| `basic_voice.mp3` | basic ability fired — the voice line |
| `basic_fx.mp3` | basic ability fired — the sound effect (plays together with the voice line; replaces the stock click) |
| `taunt.mp3` | the ult boarding gloat, played in the opponent's face |
| `victory.mp3` | match won |
| `defeat.mp3` | match lost |

You don't need all five hurt takes — the game plays whichever exist.

## Format

Mono MP3, ~64 kbps, normalized loudness, trimmed tight (no leading silence).
Keep grunts and lines under ~2 s; `victory`/`defeat` can run a little longer.

## How it works

At load the game probes each slot path (`voices/<charId>/<slot>.mp3`); files
that exist are registered and used, everything else keeps the stock sound
(the original Pirate1/Pirate2 grunts). The single-file build picks these up
automatically too (`python3 web/build-single-file.py`). Full asset guide:
`docs/ASSET_SPECS.md`.
