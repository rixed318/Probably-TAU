# Office background candidate — 2026-11-23 (Issue #21)

Status: **GENERATED LOCALLY / NOT COMMITTED AS PNG / REVIEW PENDING**. This is a new original candidate for ART-A0.1, not an approved sprite, not a Unity asset and not a finished 3–5-asset batch.

- Asset ID (candidate only): office_day01_candidate_20261123
- Intended asset: empty fictional president office backdrop, 320×180, no text, characters or institutional logo.
- Generator: ChatGPT ImageGen, generation ID `9d524018-a68b-48c9-839a-ee0920b7794d`; created 2026-11-23.
- Prompt summary: original symmetrical fictional executive office; forest-green tufted chair and curtains, wooden desk, warm banker lamp, bookshelves, window, cream/gold trim, crisp pixel-art clusters, no recognizable school branding.
- Original generated raster: 1672×941 RGB. A center crop to 16:9 and **nearest-neighbor resize** produced a **320×180 RGBA PNG candidate**. This mechanical conversion has NOT been manually pixel-cleaned or palette-approved.
- Local candidate filename: `office_day01_candidate_20261123.png` (not in GitHub).
- Candidate PNG SHA256: `95322760313ff6831a96385d0c38dcbc59e51c99d6d51c479d1fffb5e30bb374`; bytes: 89090.
- Intended repository destination after review: `Assets/Art/Source/office_day01_candidate_20261123.png`.
- Content rights: original fictional depiction; provider terms and final redistribution/asset review still pending. Do not claim the candidate approved or copy real TAU symbols.

**Blocker:** generated PNG bytes exist in this run's local working container, but authenticated GitHub Git Data API cannot directly read that container file; container git network access fails DNS resolution for github.com. No binary upload, blob/tree verification, or production manifest promotion was performed. The GitHub text record alone does not constitute asset delivery.

**Next:** use an authenticated binary-capable upload path with actual PNG bytes, verify PNG signature/dimensions/SHA256 after fetching from `feature/ui-5`, request art-direction review and only then promote manifest status. Preserve the previously documented 2026-11-19 candidate as a separate revision.

## Pixel QA preflight — 2026-11-23

Measured against the **actual local** `office_day01_candidate_20261123.png` using Pillow (not a GitHub blob):

- Raster: **320×180**, RGBA; all pixels have alpha **255** (fully opaque; acceptable for a single full-screen background, not a separated prop).
- **22,129 distinct RGBA colors** across 57,600 pixels. This is far above ART-A0.1's target of **24 working scene colors + alpha**. Consequently the candidate **fails G3 pixel-cleanup/palette review**; nearest-neighbor resizing alone did not turn the AI illustration into palette-constrained source pixel art.
- These are source-image measurements, **not** a manual edge-cluster/1× visual acceptance, GitHub binary verification, Unity import or license approval.
- **Decision:** keep status `candidate / review pending`, do **not** promote to approved/production or replace the existing manifest rows. Next art iteration should manually simplify clusters, quantize to a reviewed palette, and compare at 1× and integer 4× before review; do not silently overwrite the generated source.


## Palette-constrained local preview — 2026-11-23 (not approved)

One bounded iteration of the same original ImageGen candidate: nearest RGB squared-distance mapping to the **24 exact ART-A0.1 master colors**, without dithering, from the preserved source PNG. No repaint, no new generator output, no change to the approved art direction. Preview file (local only): `office_day01_palette24_preview_20261123.png`.

- Actual local Pillow/NumPy inspection: **320×180 RGBA**, alpha 255 throughout; **24 distinct RGBA colors**, every RGB value belongs to the art-bible palette; valid PNG signature `89504e470d0a1a0a`.
- Output **21,089 bytes**, SHA-256 `d6b9b9e1bfb7bc352a50ed457b1145fe99a2ca78cf80f4d98450c361476d36b0`.
- Original remains unchanged: SHA-256 `95322760313ff6831a96385d0c38dcbc59e51c99d6d51c479d1fffb5e30bb374`.
- **Palette-count preflight PASS only.** Pixel cluster cleanup, 1× and 4× readability, compositional/UI occlusion, art-direction approval, rights review, Unity import, and binary GitHub verification **NOT DONE**. The 24-color preview may show posterization; do not call it production-ready.
- **BLOCKED GitHub binary upload:** local git still fails DNS resolution for github.com; connector cannot read local binary. Neither preview nor original is committed. Keep manifest status unchanged; do not mark G3 overall PASS.
