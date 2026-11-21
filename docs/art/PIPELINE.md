# Probably-TAU · AI Pixel Art production pipeline · v0.1

**Status: DIRECTION A OWNER-SELECTED / CANDIDATE REVIEW PENDING / NO BULK GENERATION** · Chat 03 · Issues #13/#21 · PR #12.  
Prerequisites: art direction approved by owner + scene/content design lock from Chat 04. Unity import is deferred until Chat 01's actual Unity project/Presentation folders exist.

## Step 0 — lock the brief (before images)

From the approved ART_BIBLE_V0_1.md select one style ID, e.g. ART-A0.1. Freeze a human-reviewable MASTER-001 reference sheet with native crops and asset IDs. The same file hash and prompts are supplied to ImageGen and Gemini. Register source permissions, canonical story entity IDs and final room/document values with Content 04. Do not portray real employees, official building photos or logo without rights confirmation.

The first day has **three** distinct visitor requests and secretary Vera; the new art cannot assume PR #12's old one-case mock is the final loop. No modification of NPC logic, state, economy, event graph, UI simulator or core APIs.

## Step 1 — budgeted controlled generation

- For each P0 asset, generate 2–3 ChatGPT and 2–3 Gemini candidates **only after G0**. Keep candidates under a stable ID such as ASSET_ID__provider__p001__c001.
- Store raw generations outside the game repo in a private draft workspace; include tool/provider, exact model string, timestamp, full input prompt, approved master hash, content ref, rights notes, source hash and candidate status.
- Generate one physical asset per prompt, unless building an explicitly authorized uniform sprite sheet. Generated raw scale/output pixel size may differ from target; do not treat raw AI output as perfect-pixel native art.
- Reject stylistic drift immediately: no ultra-detailed faces in one asset and flat 8-bit faces in another, no inverse lighting, no random camera or diagonal desk perspective.
- If no candidate is suitable, fix prompt or make manual draft; do not aggregate mismatched styles.

## Step 2 — identity and content review (Chat 04)

Provide 04 a compact art review sheet (per 4 fictional people, 3 docs):
- Vera: age range, role, clothes, expression, paperwork, relation to 16:30 ROOM reveal.
- Irina: digital coordinator, DOC-01, 35 seats displayed / 20-seat inspection; no insinuation about a real staff member.
- Dasha: student organizer, DOC-02, claimed but not booked room.
- Mark: fictional partner representative, DOC-03, conditional revenue AFTER delivered practical event.
- All DOC-01/02/03 have Day 04, room Л-204, 14:00–17:00 readable in UI. Inspection only reveals its authorized extra facts.

04 may veto content inaccuracies; 03 controls pose, palette, sprites and visual production. Submit exact note/PR thread and record response. Do not edit Content 04 files or turn the draft schema into a runtime contract.

## Step 3 — pixel clean-up

Recommended editor: Aseprite or LibreSprite or Krita (choose one consistent master pipeline; check editor/file licensing).
1. Isolate individual art layers; crop only if preserving trim offset.
2. Downscale/redraw into exactly target pixel grid. **Do not claim a bicubic-resized AI image is hand-authored pixel art.**
3. Quantize carefully to approved palette, then redraw broken pixel clusters and contours, skin and faces; avoid arbitrary dither.
4. Use hard alpha (0/255) at sprite edges and check against black, white and project green backdrops; where intentional translucent effect exists, export it as a distinct overlay with explicit alpha contract.
5. Align to manifest native dimensions; set x/y anchor, pivot and sorting layer.
6. Document any palette extension, illumination override or character identity change as a candidate spec change for review.
7. Avoid AI-generated text; generate blank paper and layer Cyrillic labels/amounts as real UI Toolkit text.

## Step 4 — asset validation and review

Minimum independent review:
- consistency: same art style revision, left cool/right warm light, 3-step value ramp, 1px silhouette approach;
- composition: desk at same world position, 3/4 orthographic, 320×180 common stage origin;
- identity: character face/clothing same between full sprite, portrait and expression, no real likeness;
- semantics: correct visible DOC type, 20 seats ONLY after DOC-01 inspection, no premature confirmed booking, conditional partner payout not misrepresented;
- engineering: PNG RGBA8, no unintended semi-alpha/noise, fixed dimensions/anchors, expected sheets frames, no extra transparent padding where not documented;
- accessibility: icons readable at 1× and 4×, focus not only color, essential writing still legible at 1366×768 and 150% UI scale;
- legal/provenance: original prompts/reference sheets, rights source, permission to redistribute, model/tool terms checked when created, no copied commercial UI/characters, no institutional logos without permission.

A reviewer explicitly approves candidate IDs; pass/fail score and notes stay in a small review log. The entire first-day batch should be presented as **one consistent contact sheet** on a dark and cream neutral panel. Only final sources with recorded approval are committed into the Unity project; no hundred intermediate PNGs in GitHub.

## Step 5 — Unity import *proposal* after scaffold

When the actual Unity 6 project exists, Chat 03 creates and owns Presentation art only under the UI / Art layout approved by Chat 00/01. Do not create a fictitious Unity project.

**Importer candidate settings for pixel sprites:**
- Texture Type: Sprite (2D and UI), Sprite Mode Single or Multiple for sheets.
- Sprite Pixels Per Unit: **16** for scene sprites and consistent sheet variants; portrait/UI pixels may be rendered with independent UI Toolkit scaling and per-component sprites, but source pixel grid stays intact.
- Filter Mode: Point (no filtering), Aniso Level 0/1 if offered; no bilinear/trilinear for discrete pixel world.
- Compression: None / uncompressed for pixel-critical sprites; inspect platform overrides, disable mipmaps unless a measured use case requires them; enable alpha and sRGB as appropriate.
- Mesh Type: Full Rect for predictable pixel sprite bounds/animation; control pivot in Sprite Editor, record trim offsets.
- Use a Pixel Perfect Camera **if compatible with the chosen render pipeline**; candidate reference resolution 320×180 and integer zoom. Verify at 1280×720, 1366×768 and 1920×1080; fallback letterbox/framers rather than arbitrary fractional scaling.
- Use Sprite Atlas only for approved compatible sprites; point filtering; disable uncontrolled rotation/mesh packing as needed for pixel orientation, use ≥2px extrude/padding where useful to avoid atlas color bleed; test transparency and sampling.
- UI Toolkit: backgrounds, panel frames and icons as Sprite/UI backgrounds; Cyrillic text, numeric values, ledger and forms drawn separately in actual text widgets. No inaccessible baked copy.
- Screen/UI coordinate anchors must be independent of camera game world sprites. Verify mouse/touch targets and keyboard focus, all dialogue/menus readable at 100–150% scale, min 44px pointer areas for primary actions.

**Render test matrix:**

| Display | World scale candidate | Expected behavior |
|---|---:|---|
| 1280×720 | 4× | fills scene 16:9 |
| 1366×768 | 4× | stable 1280×720 pixels, surround as frame/UI |
| 1920×1080 | 6× | fills scene 16:9 |
| 2560×1440 | 8× | fills scene 16:9 |

These are world pixel-layer design scales, NOT a guarantee that gameplay sidebars fit unchanged. True render area and UI Toolbox scaling must be verified.

## Step 6 — repository conventions

Proposed approved paths once Unity project exists:

- source art, editable files: project-design storage linked from manifest, not all binary drafts in Git;
- individual production PNG: unity/Assets/_Project/Art/World/, Characters/, Props/, Documents/, UI/, Animation/;
- imported .meta files: commit with every approved Unity asset;
- material manifest: docs/art/ASSET_MANIFEST_D01.csv (maintained in Chat 03 branch; use immutable asset_id);
- preview and approvals: docs/art/reviews/ or a linked ticket/PR once final, never overfill docs with raw candidate PNGs.

Use semantic snake_case paths, explicit revision only in metadata or final immutable source filename; maintain stable Unity asset GUIDs when updating existing sprites. Where large binary assets require Git LFS, check repository policy and owner approval first. Do not force-push, merge or change main.

**Manifest fields:** asset_id, group, purpose, content_ref, priority, native_px, anchor_px, layer, frames, style_rev, authoring_tool_model, prompt_ref, reference_id, status, rights_review, source_file, unity_target, notes. Possible statuses: planned → candidate → selected_for_cleanup → qa_ready → approved → production, or rejected. **At present all assets are planned and rights pending.**

## Binary handoff recovery for #21 (candidate, not production)

The office candidate `office_day01_candidate_320x180.png` is **outside GitHub**. Its SHA-256 is `75b0bd61c1ceacae390edb88fdc6d3d2704a5e91acd6a30279be36f67de8f11f` (79,749 bytes, 320×180 RGB). Preserve these original bytes when transferring; a palette-quantized or recompressed PNG is a **different revision** and must get a new hash and review. Never reconstruct a PNG by pasting its filename or a data URL into a UTF-8 contents-file update.

When an authenticated binary-capable environment is available, perform one non-force fast-forward on `feature/ui-5` only:

1. Read the branch HEAD SHA and tree SHA immediately before writing; check that the proposed path `Assets/Art/Source/office_day01_candidate_320x180.png` does not already exist.
2. Read actual local PNG bytes and base64-encode the **bytes**, not their textual representation. GitHub Git Data API: `POST /git/blobs` with `encoding: base64`; verify the returned blob exists and represents the PNG bytes.
3. Create a tree based on the branch HEAD tree, with one entry `mode: 100644, type: blob, path: Assets/Art/Source/office_day01_candidate_320x180.png, sha: <blob_sha>`. Create a commit with the current branch HEAD as its sole parent.
4. Update `refs/heads/feature/ui-5` with the **expected old HEAD SHA** and `force: false`. If the lease fails, fetch the new HEAD and re-evaluate; never overwrite concurrent work.
5. Fetch the committed PNG back from the new branch, decode it, verify PNG signature, 320×180 dimensions, 79,749 bytes and the SHA-256 above. Only after this may the handoff say **binary committed**. Keep status `candidate` and rights/owner review `pending` until explicit approval; do not mark the manifest row `production`.

If the connector cannot access local binary bytes or DNS prevents Git transport, report **BLOCKED**, keep the candidate out of the production manifest, and do not substitute a text-only commit for binary delivery.

## Step 7 — acceptance before release

A release candidate batch must contain at least **3–5 actually approved and technically reviewed master assets** (one office scene layer, one furniture/prop, one character/portrait, one document shell, one icon), all coherent at native resolution, with source provenance recorded. QA 05 reviews Unity import/runtime/UI focus; Chat 00 and owner sign off. No use of copyrighted third-party game art and no reference photo with unresolved rights.

**Current gates (2026-11-21):** Owner directive selects direction A (ART-A0.1); this does **not** approve any generated candidate for production. A single original office concept was generated outside GitHub and reduced to a 320×180 PNG candidate, but its binary is **not verified in feature/ui-5**. The candidate is not MASTER-001 or a production asset. Remaining blockers: owner visual review/cleanup and rights review of candidate, Content 04 approval of character physical cues, verified binary GitHub upload, and real Unity Editor/import tests. PR #9 C0-v2 and PR #14 ADR authorize feature work but are unmerged; do not treat them as merged main contracts.
