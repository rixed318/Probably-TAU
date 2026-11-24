# Office background — candidate review record (Issue #21)

Status: **CANDIDATE / NOT IN REPOSITORY / NOT APPROVED**. Art direction: ART-A0.1, Academic Noir. Review required before import or use in game.

## Provenance and exact artifact
- Generator: ChatGPT ImageGen; generation ID `fbd3be12-4b93-4ddc-b8ec-0876a6d78624`; generated 2026-11-19.
- Subject: original fictional office with fir-green walls, walnut desk, cool window light left, amber lamp right; no official marks or readable text.
- Raw generation: 1672×941 RGB; locally downscaled review preview: **320×180 RGB PNG**, filename `probably_tau_office_art_A_candidate_320x180.png`.
- Preview SHA-256: `90b9c982fbf507bce26efb16cd3927ba44bd437f11ebd3057024a7e8c6b822d3`. SHA describes **local bytes only**, not a GitHub blob.
- The exact image was produced in a prior run. This markdown file contains **no image bytes** and must not be interpreted as a committed asset.

## Acceptance gaps
- [ ] Transfer actual PNG binary to `feature/ui-5` via authenticated GitHub blob/tree/commit/ref flow, verify committed byte content and SHA-256.
- [ ] Pixel-clean the source at native 320×180; the downscaled RGB preview is **not** compliant RGBA8 production art.
- [ ] Check 24-color palette / 3-tone ramps, crisp pixel clusters, lighting and safe central area at 1× and 4×.
- [ ] Decide whether this is a single flat reference or separate registered world layers; do not silently map it to `office_wall` or any existing layered manifest row.
- [ ] Record an explicit owner/coordinator visual approval and rights review; only then update the asset manifest status and production path.
- [ ] Test nearest-neighbor Unity import after an actual Unity Editor/project is available.

No real university marks, Probably Stolen assets, Core/GameState or UI copy are introduced. Android playtest Issue #20 remains independent and unverified on physical hardware.
