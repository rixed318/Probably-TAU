# Handoff · Chat 03 — UI/UX · Issue #5

**Date:** 2026-11-14  
**Repository:** https://github.com/rixed318/Probably-TAU  
**Work branch:** feature/ui-5  
**Base:** main (created from d673322cd9d0d366a27a57b81056504dfb26c731)  
**State:** standalone interactive browser prototype + UI/UX proposal, waiting for coordinator 00 review.  
**PR:** https://github.com/rixed318/Probably-TAU/pull/12 (draft; do not merge without owner approval).

## Goal

Propose an original 2D visual language and ship a click-through first day before Core, without duplicating GameState, financial rules, or NPC/event generation.

## Delivered

- docs/UI_UX_SPEC.md — visual thesis, colors, 3 text wireframes, complete first-day UX flow, responsive behavior, accessibility, port proposal awaiting Core C0.
- prototypes/ui-day-one/index.html — browser office view, queue, four reputation groups, read-only metrics, document tabs, mock dialogue, end-day report.
- prototypes/ui-day-one/styles.css — self-contained original CSS illustrations, neutral papers, office, character, responsive treatment.
- prototypes/ui-day-one/fixtures.js — fixed snapshots for inspect, clarify, approve, reject, defer, end-day and day-two placeholder. All money/rep results are canned mock data, not calculations.
- prototypes/ui-day-one/app.js — view rendering and command dispatch to mock gateway; no access to GameState.
- prototypes/ui-day-one/test-fixtures.cjs — Node.js command/fixture smoke tests.
- prototypes/ui-day-one/README.md — manual walkthrough and test command.

## Explicitly NOT delivered

- Unity scene, UXML/USS/MonoBehaviour integration, real sprites, compiled Unity game, PlayMode test.
- Actual GameState or application adapter: contracts not yet approved by chat 00 / Core 01.
- Real economy, deterministic event engine, generation of visitors, content balancing, Save/Load.
- Official TAU brand identity or claims about real staff or finances.
- Advanced animation, sound or gameplay beyond day one.

## Proposed interface/ownership

Presentation consumes read-only Dashboard/Document/Report projections and sends validated commands through an Application port. See docs/UI_UX_SPEC.md §4. Names are proposals only until Gate C0.

Chat 01: owns authoritative commands, read models, phase/end-day policy, money units.  
Chat 02: owns NPC conversations and events.  
Chat 04: owns approved proposal content, consequences and balance.  
Chat 05: independently verifies code, tests and save/load integration.  
Chat 00: approves visual direction and integration.

No changes to Core/Application, GameState, ledger or Economics in this branch.

## Verify manually

1. Open prototypes/ui-day-one/index.html locally, no web server required.
2. Observe date/budget/reputation, one visitor in office and queue.
3. Click the document on the desk; change the Content, Inspection and History tabs.
4. Use Inspection then Clarification. Confirm the displayed amount changes to 210 000 ₽, but the budget does not change.
5. Approve and verify the budget displayed is 1 290 000 ₽; the case is marked decided; duplicate decision actions disabled.
6. End the day and inspect a two-row mock ledger and explanatory consequences. Confirm after-approval final reserve 1 255 000 ₽.
7. Refresh, choose Reject; confirm no decision expense before end day, then operating costs 35 000 ₽ in report.
8. Refresh, choose Defer; confirm visible remaining deadline and report; navigate to day-two placeholder.
9. Test Esc, tab/focus, small viewport, browser zoom 150%, disabled End Day before decision.

## Automated tests

Run: node --test prototypes/ui-day-one/test-fixtures.cjs

These are scripted tests **provided but not executed in this remote connector environment**. Unity EditMode/PlayMode and browser automation have **not** been run; don't mark their checks green.

## Risks and blockers

- C0 Core commands and read model shape are unapproved; adapter port can change. Mock command names are examples.
- Illustrative finance and reputation values are not balanced game systems.
- Browser CSS mock is not a portable Unity prefab; requires intentional UI Toolkit translation and artwork.
- Readability and layout have not been rendered/validated in an actual Unity 1366x768 build.
- Visual tone, license, actual institutional branding need owner review before final art.

## Next steps

1. Chat 00 reviews UX direction and first-day flow in PR; leaves feedback without merging.
2. Chat 01 publishes C0. UI adapter mapping is implemented only after approved stable contracts.
3. UI team implements the same flow in real Unity Presentation assets once Unity project exists, then QA 05 checks against Issue #5 acceptance criteria.


---

## Art extension (Issue #13 · 2026-11-14)

**Status:** Proposal awaiting owner / coordinator 00 sign-off; **do not bulk generate**.

Files produced in this same UI branch:
- docs/art/ART_BIBLE_V0_1.md — three directions, recommended Academic Noir, candidate 320×180 logical grid, 24-color scene palette, layered office, 3/4 orthographic, sprite resolutions, alpha/lighting/animations and first-day narrative content locks.
- docs/art/ASSET_MANIFEST_D01.csv — **85 planned assets** (44 P0, 34 P1, 7 P2), four fictional characters, documents DOC-01/02/03, ROOM notification, report, furniture, props, HUD and 6 animation entries. All marked planned; no fake model/tool claims.
- docs/art/PROMPTS_AND_REFERENCES.md — versioned shared ImageGen/Gemini prompts and master reference-sheet specification.
- docs/art/PIPELINE.md — production selection, provenance, cleanup, quality, Unity import proposal, licensing/rights checks.
- tools/art/validate_manifest.py — offline CSV schema, ID, size, frames, anchors and provenance validator (not executed in GitHub tool environment).

**Content 04 binding:** Based on the *draft* PR #11 story, this supersedes the HTML prototype's old mock concept for art PLANNING only. Secretary Vera and three visitors (Irina/Dasha/Mark), three types DOC-01/02/03, triple room-slot clue Л-204 / Day 04 14:00–17:00, conditional partner revenue and room-conflict note. Content 04/00 still own approval of names/wardrobe/scene and terms; asset pipeline cannot declare story canonical.

**UI status:** original browser prototype remains a separate mock of a single request; no attempt to rewrite economics or make it behave like 3-scripted-content day until future approved C0 and Content schema. This addition is only planning, not gameplay implementation.

**Dependency handoff:** Chat 04 to review character roles/visual cues and blank document shells; coordinator 00 to approve one direction A/B/C and 320×180 world grid; Chat 01 to provide Unity scaffold later; QA 05 to run importer and visual tests. No official TAU logo/real person photograph approved.

**Verification:** Static GitHub files verified, offline tool included; actual artist sample sprites, Unity import, browser PlayMode, legal approval and native raster checks not yet performed.


---

## AutoDev 03 pass — 2026-11-14 · Issue #13 (manifest validator regression)

**Single scoped task:** Add offline negative/positive regression coverage to the existing manifest validator, without changing manifest semantics, Core, Unity or the HTML prototype.

**Commit:** `d47cbbc7c424c57f7d00eee9e313f00df22668ff` on `feature/ui-5`.  
**New file:** `tools/art/test_validate_manifest.py` (8 Python stdlib `unittest` cases).

Cases: current CSV valid; duplicate asset ID; missing P0 critical ID; wrong 320x180 world size; out-of-bounds anchor; approved asset without provenance; invalid Unity target; invalid frame count. The tests copy the real CSV into a temporary file, mutate one field per negative case and invoke the existing `validate(path)`; no production manifest or art data is modified.

**Execution status:** NOT RUN. Attempted to download the three GitHub source files to a local Python runtime for executing the tests, but this execution container cannot resolve `raw.githubusercontent.com` (curl exit 6). The GitHub connector allowed writing the new test file but does not execute Python. Therefore **0/8 tests are claimed passed**. No Unity Editor, browser visual test or PNG QA performed. No assets generated.

**Reproduction (Python 3.9+):**
```sh
python3 tools/art/validate_manifest.py
python3 -m unittest discover -s tools/art -p 'test_validate_manifest.py' -v
```

**Handoff to QA 05:** Run both commands in a checked-out `feature/ui-5` workspace and report exact failures; if all pass, consider including this offline art validator in the repository's CI after coordinator review.

**Design note:** Owner selected Art Direction A and 320×180 in the separate planning discussion; this pass does not change G0 status or manifest asset approval because the corresponding sign-off has not yet been reconciled into GitHub. PR #11 remains draft narrative content. Do not bulk-generate or claim approved sprites. PR #12 remains Draft; no merge into main.
