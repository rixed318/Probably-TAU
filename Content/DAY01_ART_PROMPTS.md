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

## Three canonical blank document art briefs (Issue #21)

These are **image-production briefs only**, not documents with preprinted gameplay values. All three IDs exist in UI 03's `docs/art/ASSET_MANIFEST_D01.csv` with native size **192×240 px**, single frame, `70_ui_modal` layer, and blank dynamic text areas. Preserve portrait orientation and identical scale, lighting and paper margins; use transparent outside edges. Text, names, dates, L-204, prices, choice buttons, signatures, inspection findings and financial values **must be rendered from the canonical fixture by UI**, never baked into the PNG. The original fictional documents are distinct visually without real university seals or third-party branding.

### `doc01_procurement_blank` — DOC-01 / D01_LAB
“Create an original pixel-art **blank procurement request paper template**, 192×240 RGBA, cream paper with muted teal corner tabs, simple ruled boxes for equipment details and a tiny abstract empty grid motif (no numbers or labels). Reserved clear areas for dynamically rendered amount, room, date and inspection result. Restrained institutional stationery, sharp nearest-neighbor pixel edges, no letters, digits, logos, emblems, handwriting or actual signatures. Full page visible with safe margins.”

### `doc02_student_proposal_blank` — DOC-02 / D01_STUDENT
“Create an original pixel-art **blank student initiative proposal template**, 192×240 RGBA, warm ivory paper with a small indigo-and-copper geometric edge pattern and two large empty proposal panels. Friendly visual rhythm, distinguishable from the procurement sheet; leave uncluttered fields for dynamic project description, budget, room and inspection. No text, numbers, school branding, seals, stamps, logos or signatures; hard pixel edges and consistent lighting.”

### `doc03_partner_letter_blank` — DOC-03 / D01_PARTNER
“Create an original pixel-art **blank letter of intent for a wholly fictional partner**, 192×240 RGBA, pale cream paper, understated plum header stripe with a small abstract non-logo geometric accent, tidy empty sections for terms and conditional revenue. Clear reserved space for live UI-rendered contract terms and deferred-payment conditions. No preprinted text, currency, digits, real corporate identity, logos, stamps or signatures; crisp pixel-art edges, matching the other two pages.”

**Acceptance / ownership:** UI 03 produces and reviews actual PNG binaries, provenance and SHA256; Content 04 supplies IDs and fictional visual intent only. QA 05 verifies IDs, 192×240 pixel dimensions, text-free blank templates, source-manifest mapping and the fixture's separate live data. No modifications to Core, EventDirector, score or cash effects.

## Three canonical neutral portrait briefs (Issue #21)

**Production instructions only; no PNG is claimed.** Each portrait must match its existing standing sprite in costume, proportions, limited palette, pixel grid and warm side lighting. Compose head and shoulders, neutral expression, full silhouette with safe padding, transparent background, no typography, logo, seal, blur or anti-aliasing. UI 03 selects dimensions from the current asset manifest and verifies binaries/provenance before committing to its own branch.

- `irina_korneeva_portrait_neutral` — `D01_LAB / npc_irina_korneeva / DOC-01`: fictional coordinator; short chestnut hair with silver streak, rectangular glasses, teal jacket collar, amber brooch; composed, angular silhouette.
- `dasha_polynskaya_portrait_neutral` — `D01_STUDENT / npc_dasha_polynskaya / DOC-02`: fictional student organizer; asymmetric copper bob, indigo round hair clip, sand hoodie, diagonal bag strap; alert neutral expression, rounded silhouette.
- `mark_sviridov_portrait_neutral` — `D01_PARTNER / npc_mark_sviridov / DOC-03`: fictional partner representative; shaved head, short beard, plum blazer, pale turtleneck; reserved expression, broad silhouette.

Compare portrait and standing sprite for identity consistency; preserve all nine asset IDs in `DAY01_ART_LINKS.json`. No gameplay effects, balance, runtime schema, DayIndex or partner cash change.
