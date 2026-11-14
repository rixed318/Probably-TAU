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


---

## AutoDev 03 pass — 2026-11-14 · Issue #5 (day transition guard)

**Scope:** mock-only day-transition regression. `ShowDayTwo` now requires successful `AdvanceDay`; `AdvanceDay` can succeed only once per gateway, and post-advance decisions/inspections are rejected. A fresh gateway still starts on day one. No GameState/Core or narrative content changes.

**Commits:** `bbca200fe260a19b14a77aa4cfe53c00cd750dd0` (gateway), `36a0efca6cdba6927b080d91c227c40d7317f231` (tests), branch `feature/ui-5`, Draft PR #12.

**Files:** `prototypes/ui-day-one/fixtures.js`, `prototypes/ui-day-one/test-fixtures.cjs`, this handoff.

**Regression cases:** day-two before decision, after decision but before advance, after successful advance, duplicate AdvanceDay, no mutation after advance, approve/reject/defer path checks. Existing clarified ledger assertions retained. `dayAdvanced` is a private mock gateway flag, not an authoritative GameState property.

**Test status:** NOT RUN — GitHub connector can edit source but cannot execute Node. Run `node --test prototypes/ui-day-one/test-fixtures.cjs` on actual checkout; QA 05 should confirm. Browser/Unity tests NOT RUN. No claim of passed tests.

**Next:** QA 05 to run Node tests, 00 to review PR #12. Later work: approved direction A / 320×180 controlled sample assets, without bulk generation or real-person marks.


---

## AutoDev 03 pass — 2026-11-14 · Issue #5 (canonical Day 01 read-model fixture)

**One bounded task:** created a separate **immutable, display-only** Day 01 fixture from Content PR #11 `content-draft-0.2`, without modifying the legacy one-case mock gateway, GameState, ledger, Core, or any gameplay commands.

**New files:**
- `prototypes/ui-day-one/day01-read-model.cjs`: DayIndex 0, opening 200000000 kopecks (2 000 000 fictional ₽), C0 five metrics (Q=52, S=48, M=50, E=45, R=47), DOC-01/02/03 mapped to distinct NPCs/visits, shared requested room Л-204 DayIndex 3 14:00–17:00, and inspection alternatives. Conditional partner income 12000000 kopecks is **not credited on signing** (`recognizedRevenueKopecks="0"`; earliest index 4). Source amounts are **canonical decimal strings** at this UI boundary, converted to BigInt for display; no float-based financial logic.
- `prototypes/ui-day-one/test-day01-read-model.cjs`: six Node built-in regression tests for calendar/metrics, distinct documents and slot, exact amounts, conditional revenue, formatting/Int64 boundaries, and immutable isolation from old mock.

**Commits (feature/ui-5):** `df1414738c18d82614b5314841c60fe79133dc98` (fixture), `fa75843d92a379ce80ffa1902c7547d57a47acda` (tests), `e0f090a275258b5246031d84efb0e74caef7455b` (thousands-separator correction), `d324b4267a0609051d9ebcbfbd4dfb3846204123` (avoid running browser-only legacy fixture inside Node). This handoff follows as a separate commit.

**Validation:** fetched GitHub sources and manually reviewed mapping against real Content PR #11 JSON. **Node tests NOT RUN** in this tool session; no runtime execution or green claim. QA 05 must run:
```sh
node --test prototypes/ui-day-one/test-day01-read-model.cjs
node --test prototypes/ui-day-one/test-fixtures.cjs
```
No Unity compile, PlayMode, image generation or browser visual checks performed.

**Boundary / next:** fixture is not wired into `fixtures.js` or a live C0 API. Content PR #11 remains draft; its monetary authoring JSON currently uses numeric values, while UI fixture deliberately uses decimal-string money wire values. Once Content 04 finalizes a schema and Core publishes read models, replace this static fixture with an adapter and add integration checks. PR #12 remains Draft; **no merge to main**.


---

## AutoDev 03 pass — 2026-11-14 · Issue #5 (read-only room conflict evidence)

**One bounded task:** the existing canonical Day 01 display-only fixture now marks all DOC-01/02/03 room slots as `request-not-booking` and exposes `getRoomConflictNotice()` to show the shared Л-204 DayIndex 3, 14:00–17:00 collision. The method groups immutable requests by room/day/time, returns a frozen, human-readable UI notice with all three document IDs and does not reserve the room, mutate state, credit money, or infer a decision. The legacy `fixtures.js` gateway, Core and Content remain unchanged.

**Branch:** `feature/ui-5`; **Draft PR:** #12. **Commits:** `98241013fc5fa02841c052c86443bcc054459254` (fixture), `9528229fb6516b6cf710d5713a65743e18f525f2` (test), plus handoff commit. **Files:** `prototypes/ui-day-one/day01-read-model.cjs`, `prototypes/ui-day-one/test-day01-read-model.cjs`, `docs/handoffs/ui.md`.

**Regression added:** assert one shared unresolved request collision, three document IDs, DayIndex 3 and exact time, explicit not-booked status, frozen output and unchanged opening balance. Existing six tests remain. **NOT RUN:** Node.js tests, browser and Unity. GitHub connector can inspect/edit source but does not execute Node. QA 05 should run `node --test prototypes/ui-day-one/test-day01-read-model.cjs` and `node --test prototypes/ui-day-one/test-fixtures.cjs` on actual checkout and attach CI logs. No production integration or Unity API claim.

**Next:** after QA test evidence and Core read-model contract, render this conflict notice in an accessible UI card; no independent room booking rules in UI. Art Direction A / 320×180 stays approved as planning basis, no generated PNG or production sprite approval in this pass. No merge to main.


---

## AutoDev 03 pass — 2026-11-15 · Issue #5 (standalone canonical Day 01 preview)

**One bounded task:** added a **separate read-only browser preview** for the existing canonical Day 01 fixture, including an accessible room-conflict notice and all three document requests. The legacy one-document `index.html` / `fixtures.js` / `app.js` prototype remains untouched; the preview neither reserves rooms nor issues commands/ledger postings.

**Branch:** `feature/ui-5`; **Draft PR:** #12.
**Commits:** `88f2acfbd227205d21e5dfceec53531a8e8e9e22` (HTML), `1aaea562249f42a0b88d76e7c1d2233dd6bd2332` (browser renderer), `5d1aee784592a9466637c77ee0ce2f37fd961c0d` (Node regression tests); this handoff update follows.
**Files:** `prototypes/ui-day-one/day01-preview.html`, `day01-preview.js`, `test-day01-preview.cjs`, `docs/handoffs/ui.md`.

**UX:** separate page shows DayIndex 0, fictional starting balance, three fictional visitors/documents and unresolved shared room Л-204 DayIndex 3 14:00–17:00; `role=status` for conflict; no confirmed booking inferred; conditional partner revenue explicitly labelled future/unposted. Content remains editable DOM text (no baked-in image); uses `textContent` and `createElement`, not `innerHTML`. The page loads `day01-read-model.cjs` directly in browser; it does not load the legacy mock gateway. HTML uses neutral original styling and no third-party brand assets.

**Validation:** reviewed actual GitHub fixture and Content PR #11 v0.3 against the preview. New tests cover standalone script isolation, three requests/conflict, no premature partner revenue, and source-level unsafe HTML/command checks. **NOT RUN:** Node tests, browser visual checks, Unity/C# compile; GitHub connector edits source but does not execute Node. QA 05 should run:
```sh
node --test prototypes/ui-day-one/test-day01-preview.cjs
node --test prototypes/ui-day-one/test-day01-read-model.cjs
node --test prototypes/ui-day-one/test-fixtures.cjs
```
Open `prototypes/ui-day-one/day01-preview.html` through a static HTTP server to visually verify text layout, keyboard zoom, and conflict announcement. The preview is **not** Unity UI Toolkit or an approved runtime adapter.

**Next:** QA evidence + manual accessibility/browser smoke; only after concrete Core read models, connect the presentation adapter. No GameState/Core/Content changes, no generated art, no merge to main.


---

## AutoDev 03 pass — 2026-11-15 · Issue #5 (keyboard/mobile Day 01 preview)

**One bounded task:** improve keyboard and narrow-screen accessibility of the **standalone read-only** canonical Day 01 preview, without changing its data, scenario, gateway, game commands, finances, art or Core. The canonical Day01 fixture and legacy interactive \`index.html\` remain unchanged.

**Branch:** \`feature/ui-5\`; **Draft PR:** #12.
**Commits:** \`ba63888c5fcba077660e7b9ec914c3f11a1df84f\` (HTML/CSS), \`e02cc994073ab81a75b6d9a3c0a1d40b11e81e5d\` (two regression tests), plus this handoff update.
**Files:** \`prototypes/ui-day-one/day01-preview.html\`, \`test-day01-preview.cjs\`, \`docs/handoffs/ui.md\`.

**UX:** keyboard skip link to the documents heading; target is programmatically focusable (\`tabindex="-1"\`), visible \`:focus-visible\` outline and a focus-only skip link; narrow viewport padding and \`overflow-wrap\` for long text; reduced-motion CSS guard. The document title, labels, room conflict and finance values remain actual editable DOM text. No copyrighted images, logos, or new asset generation.

**Tests:** two new Node \`node:test\` assertions inspect the actual HTML for the skip target/focus styling and narrow-screen/reduced-motion rules. GitHub files were fetched and source changes reviewed. **Node CLI NOT RUN in this tool session; browser keyboard/zoom/screen-reader checks NOT RUN; Unity/C# NOT RUN.** Run \`node --test prototypes/ui-day-one/test-day01-preview.cjs\`, then manually tab from top of \`day01-preview.html\` and activate the skip link; check 320px width and 200% zoom. No green test claim before execution.

**Next:** QA 05 to verify Node and manual browser behavior; later UI Toolkit adapter after real Core read models and Unity scene. No merge to main.
