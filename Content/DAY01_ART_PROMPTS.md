# Day 01 — original pixel-art production briefs (Content 04)

**Scope:** Issue #21, PR #11; handoff to UI 03 PR #12. Three **review candidates**, not delivered PNGs. Canonical ID source: `Content/DAY01_ART_LINKS.json`; visitor/document linkage: `Content/DAY01_UI_FIXTURE.json`. DayIndex=0; Windows and Android share identical source art.

## Common art constraints
Original fictional setting; direction A, 320×180 logical world; crisp hard pixel edges, limited deliberate palette, readable silhouettes at game scale, transparent background, no gradients, blur, anti-aliasing, logos, letters, text, UI, photorealism or references to real individuals. Keep proportions and lighting consistent across all three. Output separate RGBA PNGs, one character per image, with safe padding and no cropping. **UI 03 must choose dimensions/frame counts from its current manifest before generation**, review image quality, document provenance and SHA256, and only then commit binaries to `feature/ui-5`. Do not treat these prompts as proof of assets.

## Candidate 1 — `irina_korneeva_standing`
**Canonical:** `D01_LAB` / `npc_irina_korneeva` / `DOC-01`.
**Prompt:** “Create one original full-body standing 2D pixel-art sprite of a fictional academy digital-disciplines coordinator. Short chestnut hair with one silver streak, rectangular glasses, deep teal tailored jacket, small amber brooch, practical dark trousers and low shoes. Composed upright posture, neutral expression, distinct sharp collar silhouette. Pixel-perfect edges, coherent warm side lighting, no backdrop or floor shadow, no text or insignia, transparent background, centered entire body with padding. Match a 320×180 pixel-art game's compact readable style.”

## Candidate 2 — `dasha_polynskaya_standing`
**Canonical:** `D01_STUDENT` / `npc_dasha_polynskaya` / `DOC-02`.
**Prompt:** “Create one original full-body standing 2D pixel-art sprite of a fictional student project organizer. Asymmetric copper-orange bob haircut, round indigo hair clip, light sand hoodie, diagonal messenger-bag strap and comfortable sneakers. Energetic yet neutral resting pose; clearly different silhouette and palette from the coordinator. Pixel-perfect edges, same warm side lighting and sprite scale, no backdrop or floor shadow, no text or insignia, transparent background, centered entire body with padding.”

## Candidate 3 — `mark_sviridov_standing`
**Canonical:** `D01_PARTNER` / `npc_mark_sviridov` / `DOC-03`.
**Prompt:** “Create one original full-body standing 2D pixel-art sprite of a fictional external project representative. Shaved head, short beard, plum blazer, pale turtleneck, dark trousers; holds a slim plain document case without writing. Restrained neutral expression, broad simple blazer silhouette, visually distinct from the other two characters. Pixel-perfect edges, same warm side lighting and sprite scale, no backdrop or floor shadow, no text or insignia, transparent background, centered entire body with padding.”

## Review checklist
Compare with the nine stable asset IDs in `DAY01_ART_LINKS.json`; verify original character traits, silhouette readability at native UI size, consistent pixel grid/palette/lighting, no third-party IP or real academy branding, transparent PNG dimensions and manifest provenance. **No changes** to choices, cash deltas, Q/S/M/E/R, room L-204, deferred earned income, Core, or EventDirector.
