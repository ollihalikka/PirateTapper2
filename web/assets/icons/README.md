# Button icon overrides

Drop a PNG here named `<kindId>.png` to give a button its face. If the file
exists it is drawn as the **entire button** (full circle); if it's missing,
the game falls back to a procedural wooden disc with a painted mark, so you
can add icons one at a time.

The shipped set (16 icons) are complete circular buttons — a bold mark on its
own colored disc. Match that style for consistency.

Kind IDs currently in the button pool:
`skull`, `anchor`, `bottle`, `pistol`, `rudder`, `chest`, `hook`,
`skull_flag`, `spyglass`, `swords`.

Not in the pool but still used: `balls.png` (bomb) and `cannon.png` are
Redbeard's ability icons. `coin`, `ship`, `map`, `barrel` are retired
(files kept for later cleanup).

- **Format:** PNG, square, ~256×256 px.
- **Full-face:** the image fills the whole button (drawn at ~108% of the
  button diameter so its circular art meets the edge). Put the button's
  **background color inside the PNG** — a bold, distinct hue per button, since
  color is a memory cue alongside the mark.
- **Safe area:** keep the important art within a centered circle; the very
  corners sit just outside the button and won't show.
- **Distinctness:** each button should be instantly tellable from the others
  by mark *and* color — that's what players memorize.

To change which marks are in play, edit `BUTTON_KINDS` in `web/index.html`
(add/remove `{ id, bg }` entries; `bg` is just the pop-particle color).
