# Probably-TAU · Cross-model prompt system and reference sheets

**Issue #13 · Chat 03 · v0.1 PROPOSED, no bulk generation authorized.**  
The same content spec, palette, pixel grid, framing, lighting, version and *master reference* must be passed to ChatGPT ImageGen and Gemini. Do NOT invent a unique art direction for each model.

## 1. Generate only after the owner chooses a direction

Record in manifest **before each generation**:
- asset_id, content_ref, style_revision (e.g. ART-A0.1), prompt_revision (P-001), exact model/provider label, creation date, prompt hash, optional seed if provider actually exposes one, reference asset ID and file hashes.
- output candidate ID, source image filename, permission/licensing status, reviewer, approval decision. If a model does not expose reproducible seeds, record **not available**; never invent seed support.
- per-character content lock: exact fictional name/age-range/wardrobe notes and ID as approved by Content 04.
- whether source uses generated original scene reference vs third-party photo. Do not reference real students/employees or unlicensed game assets.

## 2. Shared strict style block for selected direction A

Append this block **verbatim** to both models for each subject. For B/C replace the entire approved style block as one versioned release; do not let one provider change it.

### SHARED_STYLE_ART_A0_1 (English for cross-provider consistency)

Original pixel art for an original indie university management game, «Probably-TAU / President of the Academy». Fictional institution and characters. Art direction «Academic Noir»: cozy, slightly dramatic administrative office, dark evergreen architecture, walnut wood, aged cream paper, restrained brass hardware, crisp organized pixel clusters, warm intellectual mood with a hint of student humor.

Pixel discipline: fixed logical scene size 320x180 source pixels (for full-screen scene), 3/4 frontal orthographic view, slightly elevated camera, visible desk tops, non-isometric, no vanishing point. Characters 48x80 source pixels when standing; head/shoulders portraits 96x96. Exactly aligned pixel clusters, coherent 1-source-pixel outlines on character silhouettes and interactive items, no antialiasing, no blurry gradients, no photographic texture, no vector smoothness, no scattered noise. Controlled solid-color shadows: 3 intensity steps.

Color anchor palette in hexadecimal:
#101C20 #18272B #132C28 #18392F #2E5545 #47705A #72907A #567A80 #8BAAAC #C5D4C8 #503828 #76513B #A17750 #8B6B43 #C9A36C #E3C78C #CABF9E #E8DEC3 #F7EDDA #865B4A #C28B69 #E1B594 #A45E55 #63727C.
Skin tones may be adapted for distinct fictional characters while keeping 3-step skin lighting and the palette's overall relationship. Environment: cool morning window light entering from upper left; restrained warm brass desk-lamp fill from right; contact and cast shadows toward screen lower-right. Design reads at 1x and 4x.

Use **master reference image [MASTER_ID] created and approved by our own team** for color rhythm, outline thickness, texture density, camera, character proportions, desk dimensions and lighting. Keep geometry and camera exactly consistent across assets. No visual inspiration from specific copyrighted games, no logos, no real-person likeness, no readable text/signatures or numerical data inside image. Do not make letter-like scribbles: leave clean empty fields for UI text overlay. No watermark, no background outside sprite when transparency is requested.

**IMPORTANT:** If the generator is unable to render perfect grid, produce a clean draft for manual redraw and pixel cleanup; do not claim the output is automatically ready for Unity.

### For ChatGPT ImageGen — instruction wrapper

Create exactly one candidate illustration following SHARED_STYLE_ART_A0_1 and the asset-specific brief below. Use the attached approved original master (if present) as a style / spatial consistency reference, not as material to copy from a third party. Output the requested composition only, no explanatory side text or contact sheet unless specified. Honor dimensions and transparency intent as closely as possible; final resampling, pixel-grid cleanup, alpha processing and source sprite crop will be done manually. Treat all inscriptions as empty fields. This is an asset candidate, NOT final production art.

[ASSET_BRIEF]
[STYLE_BLOCK]
[MASTER_REF]
[NEGATIVE_CHECKLIST]
[OUTPUT_RULE]

### For Gemini image generation — instruction wrapper

Generate **one static image candidate** for the attached asset specification. Preserve the SAME project master reference, approved palette and pixel cluster grammar as ChatGPT's result: do not choose a new art style. This is a original fictional academic-management game, not an image of a real school or a copyrighted videogame. Follow the shared style block and content lock literally. Do not render text, labels, watermarks, logos, UI overlay numbers or real people's faces. No painterly gradients, no photorealism and no blur. Requested resolution is design-space instruction; final image will be manually aligned to the source pixel grid.

[ASSET_BRIEF]
[STYLE_BLOCK]
[MASTER_REF]
[NEGATIVE_CHECKLIST]
[OUTPUT_RULE]

## 3. Template P_SCENE — full office base

**Variables:** {asset_id}, {scene_crop_or_full}, {layer_spec}, {include_objects}, {omit_objects}, {style_rev}.

Asset brief:
Create the office environment for an original fictional university president's room at 320×180 native pixel-art design canvas, fixed framing. Place large high-level silhouettes before small detail. Back wall in dark evergreen, window on left, a few original abstract books/academic decorations to the right, walnut desk center foreground. Leave reserved space behind the desk for visitor sprite and paper hotspot. Produce ONLY {layer_spec}; all excluded objects must be absent or transparent. Camera fixed 3/4 frontal orthographic, room horizon y≈111. Reuse approved master geometric proportions. Output as individual compositing layer registered to origin (0,0). Do not put document text, actual TAU logos or a real campus building view.

Negative checklist: no isometric perspective, no 3D model render, no gradients, no non-pixel edges, no stock academic photographs, no UI panels or readable writing.

## 4. Template P_CHARACTER — standing person

**Variables:** {char_id}, {fictional_role}, {content_approved_wardrobe}, {content_approved_distinctive_features}, {pose}, {reference_master}.

Asset brief:
One unique fictional person {char_id}, {fictional_role}; standing 3/4 front view in a fixed 48×80 logical sprite cell. Foot contact at (24,79). Clothing: {content_approved_wardrobe}. Distinguishing details: {content_approved_distinctive_features}. Proportions and lighting MUST match all approved first-day people. Neutral confident posture, natural hands, no caricature. Completely separated on transparent background; no props unless specified and approved. Deliver one centered character candidate, NOT an ensemble or multiple alternative designs.

Negative checklist: no photorealism, no copied game characters, no real TAU employees, no accidental extra fingers or silhouette mutations, no text/logos.

## 5. Template P_PORTRAIT — expression-controlled head/shoulders

Asset brief:
An exact identity match to approved original character sprite {char_id} and reference {character_master_id}. 96×96 source pixel portrait, 3/4 front, head and shoulders, eye line at approximately y=34–40, 7-pixel safe margin, controlled three-stage shading, same hairstyle and clothing cut as standing art, expression {expression}. All portraits for a character share the same pose, crop and eye level. Generate separately for neutral, speaking and reaction so facial expressions can later be animated/mixed without the face jumping. Blank or transparent background.

## 6. Template P_DOCUMENT — graphical blank paper only

**Variables:** {doc_id DOC-01/DOC-02/DOC-03/ROOM/REPORT}, {document_category}, {blank_sections}, {visual_priority}.

Asset brief:
Provide one original fictional paperwork template at 192×240 source art pixels, front-facing flat page, slightly warm cream fibers simplified as intentional pixels, clean rectangular geometric form, sparse borders, 1px pixel decorative separators. Category {document_category}. Include designated EMPTY, high-contrast regions {blank_sections}; NO lettering, no numerals, no signatures, no watermark, no fake alphabet strokes. It must allow real Cyrillic text, visible room/date/time, and dynamic figures to be rendered in Unity UI on top. Different document categories may have distinctive letterhead geometry but identical scale and layout density.

Specific planned templates:
- DOC-01 procurement + empty equipment seat-map box;
- DOC-02 student project proposal with one blank mini-thumbnail area;
- DOC-03 partner offer + draft contract attachment (no commercial logos);
- dispatcher ROOM notice small urgent format;
- evening REPORT sheet with blank ledger rows and summary.

**Critical readable evidence is written by UI, not generated:** all three docs show «День 04 / Л-204 / 14:00–17:00»; DOC-02/03 room status says requested, not booked; DOC-03 conditional income clearly separated from current balance.

## 7. Template P_PROP — isolated scene item

Asset brief:
Single original pixel-art {asset_name}, native {width}×{height} source pixels, isolated 3/4 frontal orthographic view, floating on true transparent background, item base at {anchor_x},{anchor_y}, cool upper-left and warm right fill exactly matching office master. One item only, no extra scene elements, no drop shadow outside item footprint if separate shadow layer exists, no lettering on equipment. Match master object's scaling in room before acceptance.

Suggested micro-test: folder closed/open states, walnut desk top, brass lamp, 20-seat miniature class plan.

## 8. Template P_ICON — meaningful HUD icon

Asset brief:
One 16×16 native pixel icon representing {semantic_meaning}, flat 1px and 2px pixel clusters with visible silhouette at native scale, foreground ink/paper/brass colors from approved palette, transparent pixels outside, no decorative text. Icon must stay distinguishable by shape when printed grayscale, on both forest dark UI and cream paper backgrounds (prepare dark/light variants if needed).

## 9. Template P_ANIMATION — sprite sheet planning

Asset brief:
Consistent asset ID {animation_id}; {frame_count} independent same-dimension frames of {native_width}×{native_height} with an original simple motion {motion_description}. Identical character/object/perspective, unchanged background and palette, integer-pixel translation only. No black frame borders, no numbering, no captions, no margins. Pose difference {frame_rules}. Source sheet must have equal frame cells. Never invent automatic sprite-sheet equivalence without inspecting frame-to-frame consistency.

Animation A: folder slides onto desk, 5 frames, no changing wood grain.
Animation B: eye blink on a *single fixed character*, 3 expression frames; complete face landmarks fixed.
Animation C: lamp focus pulse, 4 frames, no camera movement or white glow pasted across the entire room.

## 10. Candidate comparison and reject conditions

Create an A/B evaluation board in a review document (NOT an unlicensed collage from commercial artworks). Record for each candidate:
1. identity: same character silhouette, face, props;
2. lighting: left cool + right warm;
3. palette: pixels quantized to authorized palette after cleanup;
4. structure: accurate 320×180 camera / 96×96 portraits / 192×240 blank paper;
5. legibility: 1x and 4x, documents leave enough room for Cyrillic overlays;
6. originality: no actual brands, signatures, recognizably borrowed art, or real employee likeness;
7. technical: pixel alpha, clean boundaries, correct anchors and frames.

Reject if score <4/5 on consistency, identity or readability, or if legal provenance fails. Overall selection rule: **one master across both AI providers**; no merging arbitrary candidates by model or character. Technical pixel cleanup is mandatory even for selected AI output.

## 11. Master reference-sheet freeze after sign-off

Proposed MASTER-001 components:
- upper left: 320×180 office wide scene crop and strong window/desk silhouettes;
- upper right: 48×80 standing Vera with consistent skin/clothing pixel ramps;
- bottom left: 96×96 portrait of Vera with neutral expression;
- bottom center: 192×240 DOC-01 blank template (no typography);
- bottom right: 16×16 example budget icon + 32×48 lamp.

Reference sheet itself is a **layout specification** now, not generated art. Save individual components as editable source plus reference hashes; never use third-party moodboard pictures as direct asset inputs. On approval make this MASTER-001 the shared style reference for all ChatGPT/Gemini batches.
