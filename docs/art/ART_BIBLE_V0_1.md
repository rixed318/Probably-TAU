# Probably-TAU · Art Bible v0.1

**Status:** DIRECTION A (ART-A0.1) OWNER-SELECTED 2026-11-16; individual sprites, palette refinements, MASTER-001 and production usage remain subject to review. A previously generated office PNG is an unapproved candidate outside the repository, not a committed production asset. No Android/Unity art import has been verified.  
**Maintainer:** Chat 03 · **Issue:** #13 · **Inputs:** PR #11 (draft content), PR #12 (draft UI) · **Date:** 2026-11-14.  
**Scope:** one Windows-first 2D first day; no changes to Core, economy or NPC logic.

## 0. Narrative ground truth (DRAFT, owned by Chat 04)

First-day scene: «Три подписи, одна аудитория»:
- 08:45 fictional secretary Вера Лисина introduces three folders.
- 09:10 Ирина Корнеева, coordinator, procurement DOC-01: 35 computers, 220 000 ₽, room Л-204, Day 04 14:00–17:00; inspection shows room holds 20, alternative 160 000 ₽.
- 11:20 Даша Полынская, student organizer, proposal DOC-02: 65 000 ₽, same slot, room is requested but NOT booked; alternative mobile 35 000 ₽.
- 14:05 Марк Свиридов, fictional «Пульс Инжиниринг» rep, offer and draft contract DOC-03: +120 000 ₽ **conditionally after delivery**, 8 placements, same room/slot; inspection reveals mentor cost and online alternative.
- 16:30 Vera brings separate dispatcher ROOM notice; 17:30 report. All people and locations are **fictional**; no actual university finances or incidents are shown.

PR #11's initial reserve 2 000 000 fictional ₽ and five Q/S/M/E/R scores are PROPOSALS. PR #12 uses outdated one-case 1 500 000 mock; do not treat mock as canonical content.

## 1. Three mutually exclusive art directions

### A — «Академический нуар» · recommended

Dark fir wall, walnut desk, warm aged paper, muted brass; a little cool skylight from upper left and a desk lamp to the right. Focus contrast on visitor, folder and document. Mood: managerial drama, evidence in paperwork, a serious but cozy office. It continues the forest/paper concept from PR #12 without copying another game's UI.

Anchors: forest #18392F, walnut #76513B, brass #C9A36C, cream #E8DEC3, ink #18272B.

**Risk:** losing readability in shadows. Keep document value brighter than architectural background.

### B — «Ретро-администрация»

Beige-grey interior, old printer, archive drawers, faded sage, institutional linoleum and flat daytime fluorescent light. Mood: restrained bureaucratic satire and retro management.

Anchors: sage #829181, office-grey #717E81, linen #DDD7C4, rust #AF7856, ink #263139.

**Risk:** generic Soviet-office nostalgia; must not copy a real TAU room or real staff portraits.

### C — «Неоакадемия»

Modern academic office with teal architecture, clean light, copper hardware and contemporary terminals, stylized as original 2D pixel-art. Mood: ambitious campus modernization.

Anchors: deep teal #143D43, turquoise #367982, navy #172D43, copper #D4A15C, pearl #E7E9D8.

**Risk:** looking like a generic sci-fi corporate dashboard. Do not combine all three into one atlas. Style revision is fixed per production pass, e.g. ART-A0.1.

## 2. Proposed technical foundation: ART-A0.1

### Source pixel grid and device scaling

- One **logical world canvas 320×180 px**, 16:9. Every office scene layer has exactly 320×180 source pixels and the SAME origin, with separate transparent sprites for interactive props/foreground.
- Pixel-world scale in integer steps, nearest neighbor: **1280×720 = 4×**, **1920×1080 = 6×**, **1366×768 = 4×** (1280×720 + ~43 horizontal and ~24 vertical pixel margins filled by framing/UI). Do not interpolate pixel art with noninteger scaling. World and sharp UI font layers may scale independently.
- Composition reserve source x=62..258, y=14..161 for the central office focus under left/right UI rails. Scene layout will need a real Unity test before final camera lock.
- **PPU candidate: 16 px / Unity unit**. Final Unity Pixel Perfect Camera reference resolution and world size are to be confirmed after Chat 01 supplies the Unity project; PPU is not a GameState contract.
- **Character standing:** native 48×80 with foot anchor (24,79), fixed 3/4 frontal viewpoint. **Character portrait:** 96×96 head-and-shoulders, at least 7px edge safe area, eye baseline approximately y=34..40. **Visually occluded seated NPC:** use standing sprite behind foreground desk, do not distort human proportions.
- **Props:** prefer 16×16, 24×24, 32×32, 48×32 or explicit larger dimensions in manifest. Place origins on an 8px scene registration grid while allowing 1px contours. **HUD icons:** 16×16 native; dynamic lettering remains vector UI/bitmap font.
- Strict pixel clusters: 1px outlines for interactive silhouettes and character perimeter; selective 0–1px contours for room architecture; no mixed-width oversized 3px black contour. No AA, Gaussian blur, photographic gradients, vector-smooth strokes or random isolated noisy dots.
- **Orthographic frontal 3/4** office at mildly raised eye line: visible desk tops, no converging point-perspective and NO isometric tiles. Fixed horizon around y=111. Character scale, prop angles and side-face visibility consistent across models.

### Light, shadow, color

- Key light **upper-left cool window**, secondary weaker **right-hand amber desk lamp**. Screen-right/down cast shadows, 1–2 source px contact shadows; never soft blurry sprite shadows.
- Use **three discrete luminance steps** on solid surfaces: highlight, local midtone and shadow. RGB color ramps are hand-controlled, not photo auto-gradients.
- Palette: **24 working scene colors + alpha**, fixed master below; aim 6–12 colors per small prop, 12–18 for main characters. Facial skin-tone ramps may be substituted/extended *as necessary for diversity*, logged in palette revision; the cap is a production target, not a reason to force one skin tone.
- Dithering: only intentional 2×2 repeating clusters in broad surfaces, never on faces or document text.

### Candidate ART-A0.1 master palette

| ID | Hex | Design use |
|---|---|---|
| ink-deep | #101C20 | outlines |
| ink-shadow | #18272B | dark strokes |
| fir-deep | #132C28 | room shadow |
| evergreen | #18392F | walls |
| leaf-shade | #2E5545 | furniture shade |
| leaf-mid | #47705A | wall lit |
| sage | #72907A | reflected light |
| muted-blue | #567A80 | cool accents |
| sky | #8BAAAC | sky midtone |
| cool-white | #C5D4C8 | window highlights |
| walnut-shade | #503828 | desk shade |
| walnut | #76513B | desk panels |
| walnut-lit | #A17750 | desk lip |
| bronze | #8B6B43 | brass shadow |
| brass | #C9A36C | selectable accents |
| brass-lit | #E3C78C | brass highlights |
| paper-shade | #CABF9E | paper back |
| paper | #E8DEC3 | page |
| paper-lit | #F7EDDA | near-white |
| skin-shade | #865B4A | default skin ramp shade |
| skin-mid | #C28B69 | default skin ramp midtone |
| skin-lit | #E1B594 | default skin ramp highlight |
| clay | #A45E55 | caution |
| slate | #63727C | clothing/metal |

These values are provisional. UI text palettes are not limited to scene-sprite 24 colors; contrast and legibility take priority.

## 3. Exact first-day art consistency

### Characters

Produce coherent original reference sheets for **four** fictional people (three visitors + secretary), no recognizable real persons:
- **Вера**: professional secretary, attentive, calm expression, holds stacked folders, function: exposition and scheduling twist. Clothing/color to approve with 04.
- **Ирина**: tech-discipline coordinator, practical, attentive to equipment; carries DOC-01 paperwork; error is old seat-count template.
- **Даша**: energetic student organizer, holds a student project folder; clear personality and age-appropriate outfit, no caricature.
- **Марк**: representative of fictional partner; tidy business-casual and a contract portfolio, communicates conditional finance rather than confirmed payment.

Deliver a portrait 96×96, a consistent 48×80 full sprite, optional expressions. Face, hair, clothing, ID/name and lore must NOT silently change between ChatGPT and Gemini variants. All four should share one fixed physical proportion sheet and view direction.

### Documents and evidence

Do not bake text into pixels. **Always overlay actual UI text** (Unity UI Toolkit in production): amount, dates, room number, slot, booking status, inspection result, conditions and ledger explanations.

Distinct **blank frame** styles:
- DOC-01 procurement: institutional lines / equipment list / empty seat-map attachment panel.
- DOC-02 student proposal: handmade but legible illustrated proposal with project thumbnail block.
- DOC-03 partner letter + draft contract: business letterhead *invented*, attached draft terms and conditional payment tag.
- Dispatcher ROOM notice: distinct small urgent notice, not a fourth content document type.
- End-day report: calm formal ledger sheet, actual strings overlaid.
- Visual **critical evidence** on all documents (in text): **«День 04 / Л-204 / 14:00–17:00»**. On DOC-02 & DOC-03 display **«Запрос, не бронь»**; on DOC-03 **«120 000 ₽ — после проведения»**. Do not hide this evidence behind a check.

## 4. Layers, file rules and minimal animations

Sort/order from back to front:
1. 00_room_wall — base wall/architecture/floor, opaque.
2. 10_room_window_light — separate optional window light decal.
3. 20_room_background_props — shelf, cabinet, framed decorative emblems (fictional only).
4. 30_character — visitor behind desk.
5. 40_desk_foreground — desk occludes NPC lower torso.
6. 50_interaction_props — document, lamp, pen, folder, clickable objects.
7. 60_ui_overlay — Unity UI Toolkit / clean text and interactive controls.
8. 70_ui_modal — document view, evidence, report above room.

Final export: PNG RGBA8 with straight alpha; binary alpha edges unless deliberate separate fade/glow. Crop only if manifest records trim offset. All source anchors integer pixel coordinates (top-left origin in art files), conversion to normalized Unity pivots by importer. Keep master editable source separately; candidates never silently overwrite master.

Priority MVP animations:
- **document_slide**: 5 frames of folder/paper placed on desk;
- **visitor_blink**: 3-frame facial overlay / optional first-day characters;
- **lamp_focus**: 4 frames localized ambient pulse / optional effect, plus static accessible focus state.
Frame durations and sprite-sheet cell sizes explicitly recorded; 8–12 fps for deliberate pixel movement, longer idle holds for blink. No mandatory animations for essential information. Reduced motion disables decorative movement, not affordances.

## 5. Production acceptance gates

**G0** Owner approves direction, master grid, sample mood/room and legal boundaries.  
**G1** Build 1 cross-model reference sheet: 1 room crop, 1 visitor, 1 blank document, 1 prop and icon. Get approval again before a batch.  
**G2** Generate **3–6 controlled candidates per asset**, not 20 unrelated variants; ChatGPT/Gemini use the SAME master and common prompt constraints.  
**G3** Pixel clean-up manually: fix clusters, remove AA/noise, enforce palette/anchors, regenerate hands/eyes where needed; AI output is a CONCEPT source until pixel QA passes.  
**G4** Verify uniqueness, no copyrighted asset copying, no university official marks or real-person depiction without permission; preserve generation metadata and rights assessment.  
**G5** Test 1× and 4×; import candidate sprites into actual Unity Presentation after scaffold; proposed Point filtering, no lossy compression for pixel-critical sprites, fixed PPU, Sprite Atlas where useful.  
**G6** Coordinator approves production set, manifest status changes to approved; only then use in actual Unity game. Large approved binaries may need Git LFS or durable artifact storage.

## 6. Open sign-offs and cross-chat coordination

Chat 00: Art A/B/C, camera source grid 320×180, pixel art vs mixed illustrated, scene mood, official logo policy, Unity UI style.  
Chat 04: Confirm visual age range/wardrobe/props and correct blanks of DOC-01/02/03, room evidence and office mise-en-scène; Content 04 owns final scenario and fictional cast.  
Chat 01: Real approved adapter contracts when C0 is decided; no new Core class/command created here.  
Chat 05: visual tests, import smoke tests, resolution tests, licensing/compliance verification.

**No actual sprite, Unity folder, new licensed asset or shared architecture decision is approved by this document.** This branch contains planning documentation and inventory only.

See docs/art/ASSET_MANIFEST_D01.csv, docs/art/PROMPTS_AND_REFERENCES.md and docs/art/PIPELINE.md.
