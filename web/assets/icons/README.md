# Button icon overrides

Drop a transparent PNG here named `<kindId>.png` to replace a button's drawn
mark with your own art. Missing files fall back to the built-in drawing, so
you can add these one at a time.

Kind IDs (one button each): `skull`, `anchor`, `coin`, `bottle`, `dynamite`,
`pistol`, `rudder`, `balls`.

- **Format:** PNG with transparency, square, ~256×256 px.
- **Safe area:** artwork is drawn at ~75% of the button diameter, centered.
  Keep the mark inside the middle ~75% of the canvas; leave the edges clear.
- **Background:** don't paint one — each button keeps its signature face color
  behind your transparent mark (the color is a memory cue). Design the mark to
  read on that color (see the table in `docs/ASSET_SPECS.md`).

Example: `web/assets/icons/skull.png` replaces the skull mark everywhere it
appears (buttons and the start-screen preview row).
