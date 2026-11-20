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


---

## AutoDev 03 pass — 2026-11-15 · Issue #5 (Day 01 preview failure-state UX)

**One bounded task:** make the standalone read-only preview fail visibly and accessibly if its canonical read-model script is missing, incompatible, or throws during rendering. Previously the initial `Загрузка черновых данных…` message remained indefinitely when `TAU_DAY01_READ_MODEL` was absent. The preview now replaces it with an explicit Russian `role="alert"` message and clears any partial conflict/document DOM. Normal valid-model rendering is unchanged. No command, budget, room reservation, Content, GameState or Core changes.

**Branch:** `feature/ui-5`; **Draft PR:** #12. **Commits:** `361fd13a539b9eba7ef05c2c0591563469fb28e4` (preview error fallback), `8f83198910c732ecf93618de14b474f36d2bfa14` (two new regression tests), plus this handoff commit.

**Files:** `prototypes/ui-day-one/day01-preview.js`, `prototypes/ui-day-one/test-day01-preview.cjs`, `docs/handoffs/ui.md`. Tests cover absent fixture and a throwing fixture function; both require a visible alert and empty document/conflict containers.

**Actual verification:** GitHub source fetch and successful commits confirmed. Attempted a real `git clone --depth 1 --branch feature/ui-5 https://github.com/rixed318/Probably-TAU.git` in the container, but DNS failed with `Could not resolve host: github.com`; **Node tests NOT RUN**. Browser/screen-reader/320px/200% visual checks and Unity/C# compile **NOT RUN**. QA 05 should run `node --test prototypes/ui-day-one/test-day01-preview.cjs` against the actual branch and manually verify a missing `day01-read-model.cjs` gives a visible error.

**Next:** obtain real Node/CI evidence and browser accessibility smoke. Art Direction A remains 320×180 planning basis; no new art generated. No merge to main.


---

## AutoDev 03 pass — 2026-11-15 · Issue #5 (offline no-JavaScript fallback)

**One scoped task:** added an explicit `<noscript>` message to standalone `day01-preview.html` so a browser with JavaScript disabled no longer displays only an indefinite loading message. The message explains the local preview requires JavaScript and sends no data to a server. This does not alter gameplay, C0 read-model, fixture, budget, NPC or legacy mock gateway.

**Branch/PR:** `feature/ui-5` / Draft PR #12. **Commits:** `5ecc8be4414d17dea90d0770834434ae6e25472c` (HTML), `779b2cc2d5d75f139a3abec3290eead40873d93c` (regression source). This handoff is a separate commit.

**Files:** `prototypes/ui-day-one/day01-preview.html`, `prototypes/ui-day-one/test-day01-preview.cjs`, `docs/handoffs/ui.md`.

**Checks actually performed:** fetched updated GitHub HTML and test source, verified five static assertions (noscript block, explanatory text, no-server text, test presence, unchanged script references) in an isolated JS source inspection. **NOT RUN:** Node `--test` execution, browser JavaScript-disabled/manual test, 320px/200% zoom rendering, Unity EditMode/PlayMode. Static assertions are NOT equivalent to running Node tests.

**QA 05 follow-up:** `node --test prototypes/ui-day-one/test-day01-preview.cjs`; manually disable JavaScript and open `day01-preview.html`, then check at 320px width/200% zoom. Expected: readable fallback, no endless loading-only UX. No external scripts or services added.

**Constraints:** Art Direction A (320×180) remains planning baseline; no generated/approved sprites or third-party marks. PR #11 remains draft. **No merge to main.**


---

## AutoDev 03 · 2026-11-15 · CI for standalone UI and art checks (#5, #13)

**One scoped change:** added `.github/workflows/ui-smoke.yml` in `feature/ui-5`, commit `01209e0c762246181921dd6a92b63a1396f2806e`. This does **not** modify mock gateway, canonical Day01 fixture, Core or Unity files.

The workflow runs on pushes to `feature/ui-5`, PR changes to the listed UI/art paths and manual dispatch. It checks out read-only (no persisted credentials), uses Node 22 and Python stdlib, and runs three actual Node test files (`test-fixtures.cjs`, `test-day01-read-model.cjs`, `test-day01-preview.cjs`) plus the art manifest validator and its unittest suite. No external npm dependencies, tokens, write permissions or Unity Editor. Runtime timeout: 8 minutes.

**Verification status:** GitHub accepted the workflow file and returned commit SHA above. Immediate lookup of GitHub Actions runs for that commit returned **zero runs**; no tests have been observed executing, so CI status is **NOT RUN / NOT VERIFIED**. GitHub Actions may require branch-specific workflow activation or delayed indexing; check Actions and inspect logs before claiming success. Static source inspection is not runtime validation. No browser 320px/200% zoom or Unity PlayMode test performed.

**Follow-up:** QA 05 / coordinator 00 should inspect the workflow run for commit `01209e0c`, verify all steps, then perform a real browser accessibility/layout check at 320px and 200% zoom. Any failure should be fixed in this UI branch, not hidden or labelled green. PR #12 stays Draft; `main` unchanged.


---

## AutoDev 03 · 2026-11-15 · Verified first UI CI execution (#5, #13)

**One scoped task:** close the evidence gap left by the previous CI workflow commit. Inspected the actual GitHub Actions run associated with the current UI branch HEAD `b76e8f30c59af70b7a0864b2d3cf2aa9898c21c1`, without changing production UI or game state.

**Verified evidence:** GitHub Actions [run #43675977238](https://github.com/rixed318/Probably-TAU/actions/runs/43675977238), workflow `UI standalone smoke tests`, status **completed**, conclusion **success**. Its single job `Node UI and art manifest regression (not Unity)` completed successfully. The reported step conclusions are **success** for `Checkout UI branch`, `Setup Node.js 22`, `Node UI mock and canonical Day 01 tests`, `Validate art manifest`, and `Art manifest regression tests`. Commands are defined in `.github/workflows/ui-smoke.yml`: three Node test suites and Python art validator/unittest suite. This confirms those CI steps exited successfully for that HEAD; individual assertion counts and full logs were **not** independently inspected.

**Still NOT VERIFIED:** browser visual/keyboard/screen-reader behavior at 320 px and 200% zoom, JavaScript-disabled behavior in a real browser, Unity Editor/PlayMode and real C0-to-UI adapter. A green standalone workflow is not a Unity build or cross-PR integration proof.

**Next task:** run actual browser accessibility/layout smoke for `prototypes/ui-day-one/day01-preview.html` at 320 px and 200% zoom; attach screenshots/notes or mark BLOCKED if no browser. No need to recreate the already-completed Day01 read-model or rework `dayAdvanced`. Art Direction A remains 320×180; no new art or third-party IP approved. PR #12 stays Draft, no merge to `main`.


---

## AutoDev 03 · 2026-11-15 · Accessible loading state for Day 01 preview (#5)

**One bounded task:** give the standalone document list a semantic loading state. `day01-preview.html` now initializes `#day01-documents` with `aria-busy="true"`; `day01-preview.js` clears it to `false` after all three cards render, and also when a missing/throwing fixture enters the existing error fallback. No Core, GameState, mock gateway, content, finance, room reservation, art or Unity changes.

**Branch:** `feature/ui-5`; **PR:** #12 Draft. **Commits:** `bb918ac8471cdaf9780332dd3928084f765b819d` (HTML), `895086b05cb5278b54d171c5b2608d4e3e6415b8` (renderer), `d95eca28700a567bd07c604ea396b616a67daa4a` (two Node tests), followed by this handoff update.

**Verification:** GitHub API accepted and returned SHAs for all three source changes. Immediately queried workflow runs for `d95eca28700a567bd07c604ea396b616a67daa4a`; **zero runs returned**. Therefore **Node tests NOT VERIFIED**, browser/320px/200%/screen reader NOT RUN, Unity NOT RUN. Prior green CI run #43675977238 applies to the *previous* HEAD, not this change. Run `node --test prototypes/ui-day-one/test-day01-preview.cjs` and inspect the new `UI standalone smoke tests` workflow when available. New regression assertions cover initial HTML busy state, success clearing and missing-fixture clearing; the pre-existing broken-fixture test still checks alert and empty cards.

**Next:** confirm CI for the new HEAD and perform actual browser accessibility/zoom check. This is a small accessibility enhancement, not evidence of screen-reader conformance. No merge to `main`.


## CI verification — 2026-11-15 (role 03)

- Checked PR #12 head `4a05a4a8a10557846d36a1a8c0fdfa7a7b6d8ef0` via GitHub connector.
- GitHub Actions workflow run `43691340453` for that exact head reports `status=completed`, `conclusion=success`. This verifies CI's reported outcome, not a local test execution or visual accessibility audit.
- Follow-up still required: inspect individual job logs and manually test 320px viewport, 200% zoom, keyboard navigation and screen-reader announcements. No Unity Editor/PlayMode checks claimed.
- No changes to GameState/Core or the existing mock. PR remains Draft; do not merge without owner approval.


## CI evidence refresh — 2026-11-15 (role 03, Issue #5)

**One scoped task:** verify the GitHub Actions outcome for the previous handoff commit `bb1eb04a8b251a9b8bd6b130ba88260558558ba2` rather than add unverified UI changes. GitHub returned workflow run [#43699123522](https://github.com/rixed318/Probably-TAU/actions/runs/43699123522), status `completed`, conclusion `success`. Job `132766108709` (`Node UI and art manifest regression (not Unity)`) reports `success` for checkout, Node 22 setup, Node UI mock and canonical Day 01 tests, manifest validation and art manifest regression tests. These are GitHub-reported CI step results, **not** locally rerun assertions or a visual accessibility audit.

**Not verified:** actual 320 px viewport at 200% zoom, keyboard focus/screen-reader behavior, Unity Editor/PlayMode, or integration with Core. No production code, GameState, art assets, financial fixture or `main` changes in this task. PR #12 remains Draft. **Next:** browser smoke with screenshots/notes when an actual browser is available; avoid claiming visual conformance from green CI.


## AutoDev 03 · 2026-11-15 · Corrupt fixture busy-state regression (#5)

**One scoped task:** extended `prototypes/ui-day-one/test-day01-preview.cjs` to assert `aria-busy="false"` when `getRoomConflictNotice()` throws, complementing existing missing-fixture and successful-render coverage. No renderer behavior or GameState/Core changed; the original mock and canonical Day01 fixture remain separate. Test commit: `9002ff89fda07eff4231ed4f9ab631d95f74872b` on `feature/ui-5`.

**Verification:** GitHub accepted the test commit. Local `git clone` failed with `Could not resolve host: github.com`; consequently Node tests and browser 320px/200% zoom/screen-reader/Unity tests are **NOT RUN** for this commit. Check GitHub Actions against the exact new branch HEAD; earlier green runs do not cover this assertion. PR #12 remains Draft, no merge or main changes.


## AutoDev 03 · 2026-11-15 · CI verification of corrupt-fixture regression (#5)

**One scoped task:** checked GitHub Actions for the exact prior branch HEAD `c572e152752d3a9fdd20b4daab0b7138175b706e`. GitHub reports [UI standalone smoke tests run #43714885981](https://github.com/rixed318/Probably-TAU/actions/runs/43714885981), `status=completed`, `conclusion=success`. This is verified workflow outcome for the new corrupt-fixture busy-state assertion; individual assertion counts and browser behavior were not independently inspected. No new UI code changes.

**NOT RUN here:** local Node tests, actual 320px/200% browser visual/keyboard/screen-reader checks, Unity Editor/PlayMode. This handoff-only commit itself is not yet CI-verified. Next task: real browser smoke with screenshot/notes, if a browser becomes available. Existing canonical Day01 fixture and old mock remain separate. No Core/GameState changes; PR #12 Draft; no merge to main.


---

## AutoDev 03 · 2026-11-15 · accessible Day01 document regions (#5)

**One scoped change:** Give each read-only Day01 document `<article>` an `aria-labelledby` reference to its own uniquely identified `<h3>` heading. The three labels are derived from array positions, not from untrusted document IDs. This improves assistive-technology navigation without changing the canonical read-model, legacy mock, commands, finances, or GameState.

**Branch:** `feature/ui-5`; **PR:** #12 (Draft). **Code commit:** `9db6fea5aa579973b3a910311d1275a1d26e4377`; **test commit:** `edadb2241aadaff5699014f2d841087e18e654c4`.

**Verification:** Source updates accepted by GitHub; Node test added to `prototypes/ui-day-one/test-day01-preview.cjs`, **not locally executed**. Run `node --test prototypes/ui-day-one/test-day01-preview.cjs` or inspect CI for the exact new HEAD. Actual browser 320px/200% zoom, keyboard, screen reader, and Unity tests remain **NOT RUN**. No merge or main changes.


## CI verification · 2026-11-15 · role 03

- Verified exact PR #12 HEAD `6b6f0c76e63b1d61d868f7afecd996d73373ff9b` using GitHub Actions run [#43733024210](https://github.com/rixed318/Probably-TAU/actions/runs/43733024210): `completed/success`.
- Job **Node UI and art manifest regression (not Unity)**: `completed/success`; steps **Node UI mock and canonical Day 01 tests**, **Validate art manifest**, **Art manifest regression tests** each `success`.
- This confirms CI for the preceding unique `aria-labelledby` document-heading regression. It does **not** confirm browser rendering at 320px/200% zoom, keyboard/screen reader behavior, Unity compile or PlayMode tests; those remain NOT RUN.
- No changes to mock gateway, Core/GameState, money logic, content, art assets or main. PR stays Draft. This documentation-only update itself is not covered by the cited prior-HEAD CI.


## AutoDev 03 · 2026-11-15 · Canonical Day01 queue selection (#5)

- **One bounded feature:** standalone `day01-preview.html` now exposes three keyboard-operable queue buttons; selection displays the matching canonical document card and marks the active button with `aria-pressed`. The separate `day01-selection-gateway.cjs` holds only a selected document ID, reads the immutable canonical fixture, and never writes to GameState, money, room bookings, or the legacy single-case mock.
- **Files:** `prototypes/ui-day-one/day01-selection-gateway.cjs`, `day01-preview.html`, `day01-preview.js`, `test-day01-selection.cjs`.
- **Tests added:** two Node tests for three-document selection, invalid IDs, and immutable starting money/conditional revenue. **NOT RUN locally**: clone failed with `Could not resolve host: github.com`. GitHub Actions for new HEAD not yet confirmed. Browser/Unity tests NOT RUN.
- **Remaining functional priority:** end-of-day report preview for three decisions using display-only mock projections, without invented Core finance. Preserve old `fixtures.js` and `index.html` until coordinator review.
- **Integration:** PR #12 Draft, no main merge. C0-v2 PR #9 and ADR PR #14 remain contract references; no runtime adapter implemented.


## AutoDev 03 · 2026-11-16 · P0 Issue #18 standalone offline playable Day01
- Implemented `prototypes/ui-day-one/day01-playable.html` + `day01-playable.js` + isolated `day01-gameplay-gateway.cjs` using the existing canonical read model. Legacy `index.html`/`fixtures.js` and read-only preview are unchanged.
- Three distinct visitor documents, optional inspections, three choices per document, competing L-204 requests (never automatic bookings), budget in integer BigInt kopecks, five Q/S/M/E/R metrics, EndDay gated until all three decisions, immutable closed report with mock ledger, idempotent repeated EndDay. No partner income credited at signing. All mock consequences are **demonstration choices**, not authoritative Core policy.
- Run: download and extract `day01-web-demo` from GitHub Actions, double-click `day01-playable.html` on Windows. Offline dependencies are relative files in the same folder; no server required. See `HOW_TO_PLAY.txt`.
- Node tests: `node --test prototypes/ui-day-one/test-day01-gameplay.cjs`. Workflow `.github/workflows/ui-smoke.yml` includes tests and artifact upload.
- **NOT VERIFIED at authoring time:** new HEAD CI, downloadable artifact existence, Windows browser manual run, Unity Editor. Do not call READY_FOR_OWNER_TEST until exact SHA workflow and artifact are confirmed by QA. SaveService/deferred outcomes not implemented. PR #12 Draft; no main merge.


## AutoDev 03 · 2026-11-16 · P0 #18 artifact verification

**Scoped task:** independently verify the exact first playable demo CI result and its downloadable ZIP, rather than add test-only UI churn.

- Source commit: `cf4de6ed525b25abdce09737d9101b0fc5ff210b` on `feature/ui-5`.
- GitHub Actions [run #43761757451](https://github.com/rixed318/Probably-TAU/actions/runs/43761757451): `completed/success`; job `Node UI and art manifest regression (not Unity)` and the Node UI, manifest validation, regression, staging and artifact upload steps each report `success`.
- Verified artifact `day01-web-demo` ID `14181090655`, 8,400-byte downloadable ZIP, non-expired, associated with exact source SHA. Downloaded and unpacked archive; confirmed five files: `day01-playable.html`, `day01-playable.js`, `day01-gameplay-gateway.cjs`, `day01-read-model.cjs`, `HOW_TO_PLAY.txt`. All HTML script references resolve within the extracted folder, with no remote script URLs.
- Additional local Node 22 check against extracted gateway: three reject decisions then `endDay()` yielded exact closing balance `200000000` kopecks. This is **one manually executed smoke scenario**, not the full Node test suite.
- **NOT VERIFIED:** real interactive Windows double-click session, keyboard/screen-reader/zoom audit, Unity Editor or compiled Windows Unity build. Headless Chromium smoke attempt timed out; do not claim browser PASS or QA 05 sign-off. SaveService/deferred consequences remain mock-only.
- Windows launch: extract ZIP into one directory, double-click `day01-playable.html`, process all three docs, finish day; see `HOW_TO_PLAY.txt`. [Artifact download via GitHub Actions run](https://github.com/rixed318/Probably-TAU/actions/runs/43761757451).
- PR #12 remains Draft; no merge, no changes to Core/GameState/main. **P0 CI/artifact gate passed; full READY_FOR_OWNER_TEST still awaits interactive launch/independent QA.**


## AutoDev 03 · 2026-11-16 · P0 #18 independent QA gate checkpoint

- Exact UI HEAD before this documentation update: `cc44bf037f2aad91c41718ec2f3f336b7a0cd464`; own UI Actions [#43770562434](https://github.com/rixed318/Probably-TAU/actions/runs/43770562434) is `completed/success`. This is **not** an independent Windows browser launch.
- QA 05's pinned cross-branch [run #43772020964](https://github.com/rixed318/Probably-TAU/actions/runs/43772020964) reports **3/4 jobs success, UI/Content integration job failed** in `Content/validate-money-wire.cjs:62`: `expected field-specific diagnostic`. QA traced mismatch between bracketed `inspect()` paths and dotted `failsAt()` expectation in Content SHA `f3d9950e9e607176e28e0af70abc6bb9566555be`. This is **Content/QA-owned**, not an excuse for UI to edit another branch. QA reports no artifact for that cross-branch run.
- P0 **BLOCKED / NOT READY_FOR_OWNER_TEST**: independent exact-SHA QA PASS and real Windows offline double-click interactive smoke are missing. UI's own `day01-web-demo` ZIP remains available from successful UI run; do not conflate with failed QA run. No code, Core, GameState or legacy preview change in this checkpoint. No local Node, browser, Unity or Windows tests executed in this pass. Next: Content owner correct diagnostic expectation, QA rerun pinned workflow and perform actual offline browser smoke.


## AutoDev 03 · 2026-11-16 · Windows owner smoke acceptance checklist (Issue #18)

**Single scoped deliverable:** explicit, reproducible Windows acceptance checklist for the already-built `day01-web-demo` ZIP. This is documentation only; **not** evidence that Windows testing happened.

1. Open the **successful UI workflow run matching the `feature/ui-5` commit SHA**; download its `day01-web-demo` artifact and record run URL, source SHA and artifact ID before testing. Do not substitute a QA cross-branch artifact.
2. On Windows, disconnect internet, extract the complete ZIP into a new folder, and double-click `day01-playable.html`. The browser must render the Day 01 HUD without a local web server or network.
3. Confirm DayIndex 0, starting budget **200000000 integer kopecks** (2,000,000 fictional RUB), five Q/S/M/E/R metrics, three distinct visitors and three distinct documents. Read/inspect each document.
4. Confirm all three applications compete for room **L-204, Day 04, 14:00–17:00**; no choice silently reserves it. Confirm the partner's **120000 RUB is not credited on signature**.
5. Try `EndDay` before all three decisions (must remain unavailable); inspect a document, choose a decision, then try the same decision again (no duplicate ledger/budget effect). Complete all three documents and open EndDay report; check closing budget and each ledger entry against the chosen options.
6. Reload and test the alternative route (three rejects): EndDay should show **200000000 kopecks**, no decision-expense ledger entries. Verify all controls remain usable by keyboard and at browser zoom 150%.
7. Record browser name/version, Windows version, exact ZIP artifact/run/SHA, each step PASS/FAIL, screenshots or error text, and QA 05 review link in PR #12. Only after real interactive PASS and exact artifact CI verification may the coordinator decide whether to mark `READY_FOR_OWNER_TEST`.

**Current evidence boundary:** successful CI/Node results reported in previous handoff entries do **not** prove Windows double-click, offline browser rendering, or actual player interaction. This checklist has not been executed in Windows in this pass. No Unity Editor/PlayMode tests. `main`, GameState/Core and old preview remain untouched; PR #12 remains Draft.


## AutoDev 03 · 2026-11-16 · keyboard focus retention (#18)

- **Single UI change:** `day01-playable.js` remembers the focused queue-button index before rebuilding the three visitor buttons and restores keyboard focus to the matching button after re-render. Prevents keyboard users losing their place when switching documents. No mock gateway, money, content or Core changes.
- **Verification status:** GitHub source update only; Node gameplay, browser keyboard interaction, Windows offline double-click and Unity Editor **NOT RUN** in this pass. Existing Node gameplay tests do not assert DOM focus. CI for the new commit must be inspected separately. PR #12 remains Draft; no merge.


## P0 #18 — exact CI artifact verification (2026-11-16)

- Verified PR #12 head: `871d6897ccf90e775cbc8b37a745dc358b618e82` (`feature/ui-5`).
- GitHub Actions run `43913954397` (`UI standalone smoke tests`) reports **completed / success** for that exact SHA: https://github.com/rixed318/Probably-TAU/actions/runs/43913954397
- Run artifact `day01-web-demo`, ID `14247763859`, size **8,484 bytes**, `expired=false`, digest `sha256:a8ce1838497564d590ece80a9c082968627126196580639337a15e9a1cdb2805`; verified via GitHub artifact metadata. This is a verified downloadable CI artifact, **not** proof of Windows execution.
- **Actually executed in this pass:** GitHub API metadata reads only. No local Node, browser, Windows, Unity Editor, or ZIP extraction tests. CI status is a remote reported result, not an independently observed browser run.
- **Remaining P0 acceptance gate:** extract this exact ZIP on Windows; double-click `day01-playable.html` with network disabled; complete three documents, L-204 conflict and EndDay; check budget/ledger, duplicate actions and no partner cash at signing. Record actual outcomes and screenshots/logs before `READY_FOR_OWNER_TEST`.
- No Core/GameState modifications, no approval of real-world trademarks or third-party IP. PR remains Draft; no merge.


## AutoDev 03 · 2026-11-17 · Android P0 #20 single-file touch bundle

- **One scoped deliverable:** `prototypes/ui-day-one/day01-android-touch.html` embeds the existing Day01 read-model, mock gateway and UI scripts in their original order. No new gameplay rules, no external JS/CSS, no network requirement. Desktop `day01-playable.html` is unchanged.
- Mobile shell: `viewport-fit=cover`, safe-area padding, landscape-first compact two-column layout, 48px minimum touch buttons, portrait stacking, no hover-only commands. This is a WEB prototype, **not an APK**.
- Workflow uploads separate `day01-android-touch` artifact while retaining `day01-web-demo`. `test-day01-android-touch.cjs` statically checks the single-file bundle, three parseable scripts and mobile CSS; it is **not** a DOM/viewport/device test.
- Android manual procedure: download the `day01-android-touch` Actions artifact, unzip, open `day01-android-touch.html` from the Android file manager in a browser supporting local HTML, rotate landscape, inspect and decide all three documents, EndDay. Some Android browsers block `file://` opening; record actual browser/device behavior before claiming success.
- **Verification at authoring:** in-memory bundle assertions only; Node CI, 360x800/800x360 browser screenshots, actual Android file opening and Unity Editor **NOT RUN**. Verify exact new commit's workflow/artifact. No Core/GameState, IP/logo or main changes. PR #12 stays Draft.


## AutoDev 03 · 2026-11-17 · Android #20 exact CI artifact verification

- Verified branch `feature/ui-5` at `a7d12d7634a84fb448d97ed12e96f285fb9a7eaa` before this documentation-only update. `main` remains at `d673322cd9d0d366a27a57b81056504dfb26c731`.
- GitHub Actions `UI standalone smoke tests` run **43949663503** for the exact Android bundle commit reports `completed / success`: https://github.com/rixed318/Probably-TAU/actions/runs/43949663503 . This is remote CI evidence, not a device run.
- Verified artifact metadata from the same run: **day01-android-touch**, artifact ID **14266916387**, **7012 bytes**, `expired=false`, created 2026-11-16T20:25:51Z. The existing **day01-web-demo** artifact is also present: ID **14266301602**, **8484 bytes**, `expired=false`. Download both from the run page under Artifacts (GitHub authentication may be required).
- Actually performed: live GitHub branch, workflow status and artifact metadata reads. **NOT RUN:** local Node tests, ZIP extraction/content verification, 360x800/800x360 DOM viewport rendering, Android device/offline file launch, Windows browser, Unity Editor/PlayMode. CI success must not be presented as those tests passing.
- Next P0 gate: QA/device smoke test the downloaded Android ZIP in landscape and portrait, including all three documents, L-204 conflict, EndDay ledger, duplicate-effect prevention, and no partner cash on signature. Record device/browser, screenshots, and PASS/FAIL before declaring Android playtest ready. No Unity APK exists by virtue of this artifact.
- PR #12 remains Draft. No Core/GameState changes, no main merge, no use of real academy marks or third-party game art.


## AutoDev 03 · 2026-11-17 · Android touch CSS regression assertions

- Scoped change: extend `test-day01-android-touch.cjs` with explicit checks for 48px minimum width and height, `touch-action:manipulation`, landscape compact layout, portrait stacking, and all four safe-area insets. The existing single-file inline-script and no-external-resource checks remain.
- This is a **source-level regression guard**, not a real Android viewport rendering or physical-device smoke test. Do not claim it proves usable 48dp targets under browser scaling.
- Device acceptance still BLOCKED pending offline file open, landscape/portrait interaction, all three documents, room L-204, EndDay and ledger checks on Android. No Unity APK or image PNG is claimed by this change.


## AutoDev 03 · 2026-11-17 · Android #20 CI verification for touch-regression commit

- Source commit checked: `91572d56796372883fd43d31504c09c2e8ad869b` on `feature/ui-5`; PR #12 remains Draft.
- GitHub Actions run [43981459487](https://github.com/rixed318/Probably-TAU/actions/runs/43981459487) (`UI standalone smoke tests`) reports `completed / success` for that exact SHA. This is GitHub-reported CI, not a locally executed test in this pass.
- The same run publishes two non-expired downloadable artifacts: `day01-android-touch` (ID `14283416899`, 7012 bytes, sha256 `7d532965001b5f345cdbcb73a355c7b17585e9e99873817d010b043d1a98261c`) and `day01-web-demo` (ID `14283193440`, 8484 bytes, sha256 `7a53a4f7a407d00903c8366386f5e62038cd45729e6c1b52c87b511ffe4c5f75`). These are artifact metadata digests, not locally recomputed hashes.
- **Actually checked:** GitHub PR/Issue/instructions, remote workflow status, artifact IDs, names, sizes, expiration flags, source SHA and digests. **Not executed:** local Node tests, ZIP download/extraction, Android/Windows browser, 360x800 or 800x360 viewport rendering, Unity Editor. The CI's own result is not proof of real-device behavior.
- **BLOCKED for Android device acceptance:** need actual offline Android file launch and touch playthrough of three visitors/documents, L-204, EndDay, ledger and duplicate-action protection. Do not mark READY_FOR_OWNER_TEST or claim APK. No GameState/Core edits, merge, or third-party artwork.


## AutoDev 03 · 2026-11-17 · first original art candidate (#21)

- **Single scoped task:** generated one original Style-A candidate office illustration for the fictional president's office using ChatGPT ImageGen. This is an exploratory review candidate, **not** an approved/production asset, and does not imply the 3–5 PNG acceptance criterion is complete.
- Generated candidate ID: `office_bg__imagegen__20261117__c001`; generation ID `699b24b7-6fe6-4e02-842e-a7dbd3fdf90c`; native generated image **1672×941 RGB PNG**; local SHA256 `85648d763853a2ab530bba55f3fbab58dcb6183b57e82090971859ce4090579f`. Prompt subject: original deep-green/teal administrative office with walnut desk, brass lamp, cool left window light, warm right desk lamp, pixel clusters, no institutional branding, no lettering or people.
- Visual review: suitable composition reference, but generated image is **not** the 320×180 native sprite, is not guaranteed to conform to the exact 24-color palette or 1px clusters, and has not received Content 04 / owner approval. Do not register it as `production` in the asset manifest.
- **BLOCKED binary upload:** PNG exists in this run's isolated working directory but the authenticated GitHub connector cannot access local image bytes; local git network/credentials are unavailable. No PNG was committed to GitHub and no claim of an uploaded preview is made. The candidate must be transferred via a permitted binary bridge (authenticated git or create_blob(base64)), then verified in `feature/ui-5` before changing manifest status.
- Actually checked: generated image exists locally; PIL confirmed dimensions and RGB mode; SHA256 calculated locally; GitHub files/PR/Issues/Art Bible/manifest/pipeline read. **Not run:** Unity import, palette cleanup, Android/Windows browser, Node gameplay tests, and real-device smoke. No Core/GameState or main changes.


## AutoDev 03 · 2026-11-17 · candidate C001 native-grid handoff (#21)

- **One bounded task:** record the existing office background candidate after native-grid conversion, without claiming it is production-ready. Source asset: ChatGPT ImageGen generation `699b24b7-6fe6-4e02-842e-a7dbd3fdf90c` (original art, fictional setting, no real academy marks). Derived review file: `probably_tau_office_c001_320x180_candidate.png` in the conversation working directory; **not committed to GitHub**.
- Local verification in this pass: file exists; PNG signature parser reports **320×180, RGB8, non-interlaced**, **32781 bytes**; SHA256 **e0a1236ee20665f5b063fab7e9b0385cd5842fb03f3a0bcbd20a22d5c7d85232**. This is a downscaled/quantized candidate, **not evidence of hand-cleaned pixel clusters**. No alpha channel; suitable only as a flattened composition reference, not a layered office scene.
- Intended review ID `office_bg__imagegen__20261117__c001`, style `ART-A0.1`, scene grid `320x180`, no approved production manifest status. Compare against `docs/art/ART_BIBLE_V0_1.md` and `docs/art/ASSET_MANIFEST_D01.csv` before accepting. Need Content 04 and owner visual review; actual manifest entries remain planned.
- **BLOCKED:** connector can read/write GitHub text but cannot access local PNG bytes; container git cannot resolve github.com. No binary upload, Git blob, verified committed PNG, Android device smoke, or Unity Editor test performed. Next: use a working binary bridge and atomic branch-head-checked commit of the actual PNG plus manifest; verify `fetch_file` binary metadata and SHA256 after retrieval. Do not set READY_FOR_OWNER_TEST or merge PR #12.


## AutoDev 03 · 2026-11-17 · C001 palette audit (#21)

- **One bounded task:** perform reproducible local pixel/palette QA on the existing candidate `probably_tau_office_c001_320x180_candidate.png` before any attempt to register it as a production sprite. No new image generation or gameplay edits.
- **Executed locally:** Python Pillow opened the actual PNG and counted RGB pixel values. Result: **320×180**, **RGB (no alpha)**, **exactly 24 unique RGB colors**. File SHA256: `e0a1236ee20665f5b063fab7e9b0385cd5842fb03f3a0bcbd20a22d5c7d85232`. The three most common colors are `#1C3936` (3816 px), `#304C43` (3793 px), and `#2F2827` (3500 px). These counts verify quantization, **not** exact match to Art Bible anchor colors or hand-cleaned clusters.
- **Assessment:** suitable for *composition review only*. A single flattened RGB office image does not meet the layered transparent world-prop contract in the Art Bible; no production approval or rights sign-off is implied. Keep manifest entries `planned` until binary is committed and reviewed.
- **BLOCKED for GitHub binary:** local PNG bytes are not accessible to the authenticated GitHub connector, and local git failed DNS resolution of `github.com`. Thus no PNG blob/upload is claimed. Owner/Content 04 review, layer cleanup, and actual binary verification remain required.
- **Not run:** Node gameplay, GitHub CI for this documentation-only commit, Android viewport/device, Windows browser, Unity Editor/PlayMode. No Core/GameState or main changes.


## AutoDev 03 · 2026-11-17 · reproducible C001 PNG audit (#21)

- **Bounded task:** add `tools/art/audit_candidate.py`, a read-only Pillow audit of candidate PNG dimensions, mode, SHA256, byte count, unique colors and most common RGB colors. Output JSON explicitly marks `production_ready: false`; it cannot approve artwork or replace manual pixel/rights QA.
- **Executed locally:** `python tools/art/audit_candidate.py /mnt/data/probably_tau_office_c001_320x180_candidate.png` (equivalent local copy) and `python -m py_compile tools/art/audit_candidate.py`. Both passed. Observed 320×180, RGB, 24 colors, 32781 bytes, SHA256 `e0a1236ee20665f5b063fab7e9b0385cd5842fb03f3a0bcbd20a22d5c7d85232`.
- **Still BLOCKED:** actual PNG bytes have not been committed; authenticated GitHub connector cannot directly read container files. No GitHub CI, Android hardware, Unity Editor, pixel-cluster cleanup or production approval claimed. No Core/main changes.


## AutoDev 03 · 2026-11-17 · candidate PNG audit regression (#21)

- **One bounded task:** add `tools/art/test_audit_candidate.py` with six synthetic Pillow-based tests for existing read-only `audit_candidate.py`: native 320×180 RGB and SHA256, wrong grid, RGBA, >24 colors, non-PNG content, and palette-mode rejection. No art assets, production manifest entries, gameplay or Core changed.
- **Test commit:** `f3709edbedba85cbeff4d7dcd5d5b3ed23eb99b2` on `feature/ui-5`. This handoff is a separate documentation commit (GitHub Contents API only supports one file per commit here).
- **Actually executed locally:** Python 3 + Pillow 12.3.0, `python -m unittest discover -s tools/art -p 'test_audit_candidate.py' -v`: **6/6 passed** in 0.059s against a local copy of the repository's `audit_candidate.py` fetched and reviewed from GitHub. These synthetic tests do **not** validate the original C001 binary, GitHub CI, Android hardware or Unity.
- **BLOCKED:** C001 actual PNG binary still absent from GitHub because this authenticated connector cannot ingest the local PNG file bytes. Candidate remains unapproved and production_ready=false. No merge or main changes.


## AutoDev 03 · 2026-11-17 · transparent PNG audit guard (#21)

- **One bounded task:** extend the candidate-only Pillow audit to report `alpha_pixels` (number of pixels with alpha < 255) and `has_partial_alpha` (any alpha 1..254). These fields reveal whether candidate art violates the Art Bible's binary-edge preference; they do **not** mark art production-ready. RGB images report 0/false. Existing output fields preserved.
- **Commits:** `7024b649176086391f571f94c9be237669752bac` (audit) and `95c949469a8db6bf9b80dd12eb345774650fc634` (two synthetic alpha regression tests), plus this handoff commit. Branch: `feature/ui-5`, PR #12 remains Draft.
- **Actual verification:** fetched current source and tests through authenticated GitHub before editing; GitHub Contents API acknowledged both commits. **Tests NOT RUN** in this pass: 8-test Pillow suite, CI, Android viewport/device, Unity. QA 05 should execute `python -m unittest discover -s tools/art -p 'test_audit_candidate.py' -v` on checkout.
- **BLOCKED:** actual office C001 PNG binary still not committed to GitHub; this pass does not change manifest status or imply art approval. No GameState/Core/main changes.


## AutoDev 03 · 2026-11-17 · execute existing PNG audit regression (#21)

- **One bounded task:** execute the existing eight candidate-audit regression tests and audit the real local C001 PNG. No source code, manifest, gameplay or production approval changed.
- **Actually executed locally:** Python unittest with Pillow, eight tests passed (8/8, 0.095 s), covering 320x180, RGB/RGBA, SHA, palette limit, invalid PNG/mode, binary alpha and partial alpha. Local copies of `tools/art/audit_candidate.py` and `tools/art/test_audit_candidate.py` were reconstructed from the authenticated GitHub contents read for this pass; tests were not run in GitHub Actions.
- **Actual local C001 audit:** `probably_tau_office_c001_320x180_candidate.png`: 320x180 RGB, 32781 bytes, 24 colors, 0 transparent pixels, no partial alpha, SHA256 `e0a1236ee20665f5b063fab7e9b0385cd5842fb03f3a0bcbd20a22d5c7d85232`; `production_ready=false`. This verifies file metadata and palette, not manual pixel cleanup or style approval.
- **BLOCKED for art delivery:** real PNG binary still not committed to `feature/ui-5`. GitHub connector supports base64 blobs, but no verified bridge from local file bytes to the connector was available in this pass. No Android device, Unity Editor, or CI test performed. Do not merge PR #12 or touch main/Core.


---

## AutoDev 03 · 2026-11-17 · Issue #21 — visible palette audit

**Scope:** One bounded PNG-audit correction. Fully transparent RGBA pixels can retain arbitrary invisible RGB values after image editing; those hidden colors should not count against the 24-color *visible* palette budget. The audit now counts only colors with nonzero alpha for `unique_colors`, `top_colors` and `within_24_colors`, while retaining raw transparency counts and partial-alpha detection. Added a regression fixture with 30 distinct invisible RGB values and one visible color.

**Commits:** `5d0d6905befa2130aed2d791c32bd3f53eaa9afe` (audit), `bebee7e2f226330dc6821cb716c374ad7d35ec1c` (test). Files: `tools/art/audit_candidate.py`, `tools/art/test_audit_candidate.py`, this handoff. No manifest semantics, game state, economics or Unity integration changed.

**Verification:** GitHub accepted both commits; source-level inspection only. The new regression test has **not been executed** in this pass, and the full test suite is **not claimed passing**. Reproduce with `cd tools/art && python -m unittest -v test_audit_candidate.py` after installing Pillow. The existing local C001 remains a candidate; no new binary was uploaded or verified in the GitHub tree. Real Android/Unity tests not performed.

**BLOCKED for production art:** C001 PNG is still absent from `feature/ui-5` and requires a verified binary upload plus owner art review. PR #12 remains Draft; no merge or main push.


## AutoDev 03 · 2026-11-17 · Issue #21 — RGB palette across alpha variants

- **Single bounded task:** Fix candidate PNG palette counting when one RGB color occurs at several nonzero alpha values. The 24-color RGB budget now merges those alpha variants, while `has_partial_alpha` continues to report them independently. Fully transparent colors remain excluded. No asset, Core, manifest, Unity, Android or gameplay changes.
- **Commits:** `160eebbd2039137b9b1bec2fff29760fe021deb6` (audit fix), `aab37cabee664809f078e8a8340b5d41f2472ff4` (new regression test); this handoff commit follows.
- **Actually executed:** local targeted Python/Pillow reproduction of the updated Counter algorithm on a 320x180 RGBA image with alpha 255/128/1: PASS (1 visible RGB color, 57600 pixels). **Full repository unittest suite, GitHub CI, real C001 binary, Android and Unity NOT run** in this pass. The new test was committed but its execution has not been verified.
- **BLOCKED for Issue #21 delivery:** C001 PNG binary still absent from GitHub; image generator output cannot be read directly by the GitHub connector. Needs binary upload, blob verification and owner visual review. PR #12 remains Draft. No main push/merge.


## AutoDev 03 · 2026-11-17 · Issue #20 — coarse touch / Android safe-area CSS

- **One bounded task:** added coarse-pointer-only minimum 48px interactive targets, 96×64px document hotspot, safe-area offsets for landscape and toast, and viewport-height-limited dialogs to existing desktop prototype CSS. Desktop fine-pointer styling remains unchanged. This is incremental preparation, **not** the required single-file offline mobile artifact.
- **Code commit:** `a24939cf7d35be8b785a7619ae6e5a1ed4f84577` in `feature/ui-5`; file `prototypes/ui-day-one/styles.css`. No JS fixture, Content, Core/GameState or main edits.
- **Actually verified:** authenticated GitHub Contents API accepted the code commit. CSS syntax was reviewed at source level; **no** browser rendering, Android viewport, touch hardware, Node, CI or Unity tests executed in this pass. Do not claim 48dp physical size on device until browser/device test.
- **Remaining #20:** landscape/portrait screenshot regression, safe-area visual inspection, single-file offline bundle and actual Android playtest. **Remaining #21:** C001 real PNG binary upload and manual art review. PR #12 stays Draft; no merge.


---

## AutoDev 03 pass — 2026-11-18 · Issue #20 (offline Android HTML artifact)

**One bounded task:** packaged the existing Day01 browser prototype as a real single-file offline HTML artifact, preserving the separate desktop sources unchanged.

**Artifact:** `prototypes/ui-day-one/day01-android-offline.html` (56,675 UTF-16 JS string units at packaging time). Includes original HTML + inline CSS + inline mock fixtures + inline UI controller. Added `viewport-fit=cover` for Android safe-area CSS. No external CSS/JS/CDN/service worker or GameState/Core changes. This remains the legacy one-request mock, **not** the canonical three-visitor Day01 gameplay slice and **not** an APK.

**Artifact commit:** `eca8d53575b05f0eff82e1bf58fdb58afd6aafdb` on `feature/ui-5`. Direct browser/open link: https://github.com/rixed318/Probably-TAU/blob/feature/ui-5/prototypes/ui-day-one/day01-android-offline.html ; download the raw file then open locally in an Android browser. GitHub's blob page is a code viewer, not a playable HTTPS host.

**Actually verified:** connector read all four original source files; packager assertions confirmed both inline scripts, one inline stylesheet, `viewport-fit=cover`, mock gateway and no relative `./` script/style links. GitHub file creation returned a commit SHA. **NOT RUN:** Android browser DOM/touch viewport tests (800×360, 360×800), Node tests, Unity, real device, Actions artifact upload or HTTPS deployment. Existing CSS touch sizing still needs visual QA. No claim of playable Android validation.

**Next:** QA 05 should open the downloaded HTML in Chrome on Android or Android-shaped emulation and check inspect/decision/end-day, clipping, 48 CSS px targets and safe areas. Issue #20 still requires a GitHub Actions downloadable build artifact; this committed HTML is directly downloadable from GitHub but no Actions artifact has been published in this pass.


---

## AutoDev 03 pass — 2026-11-18 · Issue #20 (offline dependency regression)

**One bounded task:** extended `prototypes/ui-day-one/test-day01-android-touch.cjs` with an offline packaging regression that rejects CSS `@import`, remote CSS `url(http...)`, remote media/frames, runtime `fetch`/XHR/WebSocket/EventSource and external script/link references. The existing canonical three-visitor `day01-android-touch.html` is unchanged; this is a source-level guard, not browser/device proof.

**Code commit:** `195c93ae190b32775b6b1e11d7b4336b4a6fe171` on `feature/ui-5`.

**Actually verified:** GitHub accepted the test-file commit and returned its blob SHA. **Not run:** Node test suite, GitHub Actions, Android viewport/touch emulation, physical Android, Unity. The new assertions need CI execution; no test pass claimed.

**Remaining:** confirm GitHub Actions run/artifact, real Android landscape and portrait viewport checks, Day01 interaction regression. Art #21 still needs real PNG binary commit and artistic review. No Core/GameState/main edits; PR #12 stays Draft.


---

## AutoDev 03 pass — 2026-11-18 · Issue #20 (Android artifact completeness)

**One bounded task:** updated `.github/workflows/ui-smoke.yml` so the existing `day01-android-touch` Actions artifact stages BOTH `day01-android-touch.html` (canonical three-visitor mock) and `day01-android-offline.html` (self-contained earlier prototype). Desktop artifact and original desktop files remain unchanged. This is packaging only, not Android runtime validation.

**Workflow commit:** `7bac2bece0cc80199b0bb5f0cf73090ee4f577f4` on `feature/ui-5`.

**Actually verified:** GitHub file read and successful Contents API update. **NOT RUN:** GitHub Actions job/artifact download, Node regression suite, Android viewport/touch emulator or device, Unity. The workflow must run successfully before claiming the ZIP contains both HTML files.

**Next:** verify workflow run and downloaded ZIP contents; test landscape 800×360 and portrait 360×800 interactions and safe areas. Issue #21 PNG binary still pending. No main/Core/GameState changes; PR #12 remains Draft.


## 2026-11-18 · Android artifact regression (#20)

- Added one Node smoke case to `prototypes/ui-day-one/test-day01-android-touch.cjs` for the **second** packaged file, `day01-android-offline.html`. It checks viewport safe-area, 48px controls, no external resources/network calls and JavaScript syntax of embedded scripts. The existing `ui-smoke.yml` already invokes this test file and packages both HTML files.
- Scope: test only; no change to gameplay, Unity, Core, or source HTML.
- Commit: `4c39761499caa8ae1732fab4013819c3ac49c1ab` (follow-up content update `e2b70bbe66b598c3d1303a102b49cb2faa3528be`).
- Verified: source and workflow inspected through GitHub; commit writes acknowledged. **Not verified:** Node runner, GitHub Actions execution/artifact ZIP, physical Android touch/viewport, Unity.
- Next: run Node smoke in CI and inspect artifact ZIP; preserve PR #12 as draft until coordinator review.


## 2026-11-18 · Issue #21 — CI coverage for candidate PNG audit

- **One bounded task:** add the existing `tools/art/test_audit_candidate.py` synthetic Pillow regression suite to `.github/workflows/ui-smoke.yml`. Installs Pillow 10–12 on the Ubuntu runner and executes `python3 -m unittest discover -s tools/art -p 'test_audit_candidate.py' -v` before packaging Android artifacts. This checks PNG audit logic only; it does not imply an actual art binary was generated or approved.
- **Workflow commit:** `35fe65fe58141f1a8c6e0ca0fd0bf3ef028c7510` in `feature/ui-5`; PR #12 remains Draft.
- **Actually verified:** GitHub Contents API accepted the workflow edit. **Not executed in this pass:** GitHub Actions workflow, Pillow suite, Node tests, Android viewport/device, Unity, artifact ZIP download. CI success is unconfirmed.
- **Still blocked for #21:** original C001 PNG binary is not verified in GitHub; requires authenticated binary upload, tree/blob verification, and owner art review. No GameState/Core/main edits or merge.


### 2026-11-18 · CI selection regression wiring (#20)

- One bounded change: `.github/workflows/ui-smoke.yml` now runs existing `node --test prototypes/ui-day-one/test-day01-selection.cjs` alongside the other Day01 smoke tests; test implementation and GameState/Core unchanged.
- CI change commit: `284f1f457191e9fa091387dec7a6d59368a1e28d`.
- Verified: workflow YAML was read back from GitHub and the new command is present. **Not verified:** Node runner execution, GitHub Actions outcome, Android browser/viewport, artifact ZIP, Unity.
- PR #12 remains draft. C0-v2 PR #9 and ADR PR #14 remain pending integration. Real art PNG and device playtest are still outstanding.


## AutoDev 03 — 2026-11-18 · CI regression diagnosis (Issue #20)

- **One bounded fix:** `test-day01-android-touch.cjs` now accepts valid CSS `env(safe-area-inset-top, 0px)` fallback syntax in the legacy single-file Android demo. Previous regex required `env(safe-area-inset-top)` without fallback and falsely failed.
- **Code commit:** `f7706f5d2d66b76147bf1ff7a4025f600a4ea4fc` on `feature/ui-5`.
- **Evidence:** GitHub Actions run [44246139066](https://github.com/rixed318/Probably-TAU/actions/runs/44246139066), job 134605440319: 2/3 Android bundle tests passed; third failed at the strict `env(...)` regex; later jobs and artifact upload were skipped. This is an actual failed CI run, **not** a successful test claim.
- **Verification:** Source and workflow read through GitHub; code update confirmed. New CI run, Android device/viewport, Unity, and ZIP artifact **not yet verified**. Recheck Actions for the new commit; do not claim artifact readiness until a successful run and ZIP inspection.
- **Scope:** no GameState/Core changes, no merge, no main push. PR #12 remains draft. Art Issue #21 still needs verified binary PNG and review.


---

## AutoDev 03 pass — 2026-11-18 · Issue #20 (CI artifact verification)

**One bounded task:** verify the latest successful UI workflow run and record actual artifact availability; no gameplay/UI/Core code changed.

**Evidence:** GitHub Actions [run 44258000756](https://github.com/rixed318/Probably-TAU/actions/runs/44258000756) for `feature/ui-5` commit `7bfffdb59dccb2256e7d58196d95d5aa9bd3e6b3` completed with **success**. GitHub jobs API reports successful Node UI tests, manifest validation/regressions, Pillow install, synthetic PNG audit tests, staging and both artifact upload steps. Artifact API lists `day01-android-touch` (ID `14406184954`, 24,307 bytes, not expired) and `day01-web-demo` (ID `14406389122`, 8,484 bytes, not expired). Download at the run's Artifacts section.

**Limitations:** Artifact ZIP bytes and included file list were NOT independently downloaded/inspected; Android viewport/touch and physical-device checks NOT RUN. This is a web prototype, not APK/Unity. Issue #21 still needs verified original binary PNG assets and art review. No C0/API or Unity adapter claims.

**Next:** QA 05 to download ZIP and validate offline behavior at 800×360 landscape and 360×800 portrait, including tap targets, clipping, decision and EndDay flow. Keep PR #12 draft; no merge.


## AutoDev 03 — 2026-11-18 · Issue #20 — downloaded Android artifact ZIP integrity

**One bounded verification task:** downloaded GitHub Actions artifact `day01-android-touch` (ID `14406184954`) from successful run [44258000756](https://github.com/rixed318/Probably-TAU/actions/runs/44258000756), branch `feature/ui-5`, workflow head `7bfffdb59dccb2256e7d58196d95d5aa9bd3e6b3`. Locally opened the actual 24,307-byte ZIP with Python `zipfile`; `testzip()` returned `None` (no corrupt member).

**Verified ZIP members:** `day01-android-offline.html` (63,742 bytes; SHA256 `70d0b663da2bd37cdc1db5960d70ae77f6461eade084d2ac7aa81658d5541383`) and `day01-android-touch.html` (16,954 bytes; SHA256 `4637e2ad709495be2c9380abefe00f3866ec46ab1de8316a0ce0b6bc8b08ff3f`). Both begin with `<!doctype html>`; no additional ZIP members. These checks establish downloadable package integrity and file presence, **not** functional browser correctness.

**Not tested:** Chrome/Android rendering, 800×360 / 360×800 clipping, tap targets, decision/EndDay behavior on device, Unity or APK. Art Issue #21 still requires real approved PNG binaries. No changes to Core/GameState, main, or other branches. PR #12 remains Draft.


---

## AutoDev 03 — 2026-11-18 · Issue #20 — Android offline playtest protocol

**One bounded task:** add actionable Android offline opening instructions and a seven-case manual viewport/touch regression matrix to `prototypes/ui-day-one/README.md`. The checklist covers 800×360 landscape, 360×800 portrait, cutouts/safe areas, edge taps, 150% text zoom, and airplane-mode offline reload; it distinguishes the three-visitor touch file from the older one-request offline file. **No HTML/CSS/JS, Core, Unity, GameState, or desktop demo changes.**

**Documentation commit:** `b112064f3a68a3f6456d8fb3e0aee033e7377a30` on `feature/ui-5`. This handoff is a follow-up documentation-only commit. PR #12 remains Draft; no merge/main push.

**Actually verified:** GitHub Contents API accepted the README change. The protocol is documentation only, so **zero Android viewport/device tests were executed** in this pass. Existing ZIP integrity evidence is from the preceding pass; it does not establish rendering correctness. Node/Unity/PlayMode **NOT RUN**.

**Next:** QA 05 or owner runs the matrix on real Android/browser, records device/version/screenshots and failures; Issue #21 still needs original PNG binaries, verified GitHub blobs and owner art review.


---

## AutoDev 03 — 2026-11-18 · Issue #20 — staged Android artifact regression

**One bounded task:** add a pre-upload verification step to `.github/workflows/ui-smoke.yml` for the Android HTML artifact. It asserts the staging directory contains exactly `day01-android-touch.html` and `day01-android-offline.html`, each staged file is byte-for-byte identical to the corresponding source file, and both begin with an HTML doctype. The workflow prints staged sizes and SHA256 values. Existing desktop demo and all runtime/Core files remain unchanged.

**Implementation commit:** `c79e17bc570bf87d1d26f60d6888a2848758c765` on `feature/ui-5`. **Verification:** GitHub accepted the workflow commit; a previously downloaded artifact ZIP was locally inspected (two expected members), but the new GitHub Actions step has **not yet executed**. No successful CI, Android device/viewport, touch, or Unity result is claimed for this change. PR #12 stays Draft; no main push or merge.

**Next:** inspect the new workflow run, then QA 05/owner perform real Android viewport/touch tests. Issue #21 still needs original PNG assets with verified binary commits and art review.


## AutoDev 03 — 2026-11-18 · Issue #20 — new staging verifier CI result

**One bounded verification task:** inspect GitHub Actions run [44318829098](https://github.com/rixed318/Probably-TAU/actions/runs/44318829098) for feature/ui-5 SHA `9bdc0bd6376fe7c066dac98d8aefb68860f5d1bb`. The job `Node UI and art manifest regression (not Unity)` completed **success**. Crucially the newly added `Verify staged Android artifact files` step (#11) completed **success**, as did Node UI tests, art manifest validation, synthetic PNG audit, and both upload steps. This confirms staged byte-for-byte source comparison executed in CI, not merely YAML presence.

**Artifacts confirmed by GitHub API:** `day01-android-touch` ID `14434206187`, 24,307 bytes, SHA256 artifact digest `11eed03b1249ca9c9b376be57da6af8d8214d55890b548f03484aece8cc6fd5d`; `day01-web-demo` ID `14433078253`, 8,484 bytes; both `expired=false` at verification. Download via run Artifacts section. These are web HTML artifacts, **not APK**.

**Not executed this pass:** fresh ZIP download or browser/device render; Android viewport 800×360 / 360×800, touch controls, physical Android playtest, Unity Editor/PlayMode. Original art PNG binaries in Issue #21 still need actual generation/upload and owner review. No changes to GameState/Core, main, other branches, or public contracts. PR #12 remains Draft.


---

## AutoDev 03 · 2026-11-18 · Issue #20 — latest CI verification

**Single scoped task:** verify the latest available UI workflow run and artifact metadata on the existing `feature/ui-5` HEAD; no gameplay, Core, art or workflow source modifications.

- Checked commit: `bf46bc65f7ed060a3bc0925692c872abc65b62dc` (PR #12 head before this handoff update).
- GitHub Actions run [44338441956](https://github.com/rixed318/Probably-TAU/actions/runs/44338441956), job `Node UI and art manifest regression (not Unity)`: `completed/success`.
- GitHub reports successful steps for Node UI tests, manifest validation, manifest regressions, synthetic PNG candidate audit, staging and verifying Android HTML files, and both uploads. This confirms CI step status only; no local rerun or Android browser execution was performed.
- Published artifacts: `day01-android-touch` (24,307 bytes; artifact id `14443001283`; GitHub SHA256 digest `1838e05d660836a90d896a495262f404f89cebaebcac0ccbe37926160e1d7202`) and `day01-web-demo` (8,484 bytes; id `14441794000`). Both reported `expired=false`.
- **Not tested:** extracted content of this new ZIP, Android 800×360 / 360×800 visual and touch playtest, real original PNG candidate review, Unity Editor, APK. Issue #21 is not resolved by synthetic PNG tests.
- **Next:** Android device/browser playtest against the documented seven checks; then art candidate generation and owner review. C0-v2 PR #9 and ADR PR #14 remain unmerged proposals; do not wire a real Core adapter yet.


---

## AutoDev 03 · 2026-11-18 · Issue #20 — durable Android artifact instructions

**One bounded task:** replace the README's pinned historical Actions run link with the live `ui-smoke.yml` workflow listing. Android testers now select the latest **successful** `feature/ui-5` run, verify its head SHA, and download `day01-android-touch`; instructions also warn that Actions artifacts may require sign-in and expire. Existing desktop demo, HTML, CSS, JS, C0/Core and GameState were not changed.

**Implementation commit:** `a4e5eb72dda5a7dfcaf5f787ceb9198c4672f220` (`prototypes/ui-day-one/README.md`). **Verification actually performed:** fetched original README and workflow from GitHub; GitHub Contents API accepted the README update. No Node, Android viewport, physical-device, Unity, or PNG image generation tests were run. This is a documentation-only change and does not establish Android rendering correctness.

**Next:** QA 05/owner should execute the seven Android checks on a device. Issue #21 still requires real original PNG files, GitHub blob verification, and art review. PR #12 stays Draft; no merge or main push.


## 2026-11-19 — Issue #20: Android Day01 DOM initialization bug

- **Fix:** `prototypes/ui-day-one/day01-android-touch.html` now attaches its UI event handlers and calls `render()` from `DOMContentLoaded`, after the body elements exist. Previously its inline head script accessed `#inspect` before body parsing, causing `Cannot read properties of null (reading 'addEventListener')` and leaving the demo at “Загрузка…”.
- **Code commit:** `637dc9ac099b9da0418adf64c0d79bd9ba4ba48d` in `feature/ui-5`. No Core/GameState changes.
- **Actually executed locally:** Chromium headless via Python Playwright `page.set_content` on the patched standalone HTML at 800×360 and 360×800, touch/mobile emulation. Both had zero page errors, document width equal to viewport width, and visible button bounding boxes at least 48×48 CSS px. Screenshots saved locally. Initial run on the old artifact reproduced the null addEventListener error at both sizes.
- **Limitations:** `file://` navigation was blocked by local browser administrator policy; `set_content` validates DOM execution/layout, **not** Android local-file launch. No physical Android, complete tap-through, Unity, APK, or new GitHub Actions run verified. Existing downloaded ZIP remains the old broken build until CI publishes a new artifact. A regression test guarding DOM-ready initialization should follow.


## 2026-11-19 — Android DOM-ready regression guard (#20)

- Branch: `feature/ui-5`; PR #12 remains Draft. Commit `4856318532676d43c69ea9a25ccb2aacb7d07dc3` adds one static Node regression to `prototypes/ui-day-one/test-day01-android-touch.cjs`.
- Guard verifies the head-script/HTML-body ordering and that `inspect`/`end` event wiring and first `render()` remain inside `DOMContentLoaded`. It protects the previously fixed permanent «Загрузка…» failure.
- Validation performed: GitHub accepted the test-file commit; source HTML inspected and contains the expected deferred handler. Node/CI and real Android tests **not executed in this pass**; success is not claimed.
- No GameState/Core, Unity runtime, desktop HTML or art assets changed. Android real-device test and Issue #21 real PNGs remain pending.


---

## 2026-11-19 — Issue #20: DOM-ready regression confirmed in CI

- Scoped task: verify the newly added Android DOM-ready regression on the actual `feature/ui-5` HEAD, not merely inspect its source.
- Verified HEAD before this documentation commit: `f9703db638e3275995a73ba52ddca8637c914576`; PR #12 remains Draft.
- GitHub Actions [run 44411464460](https://github.com/rixed318/Probably-TAU/actions/runs/44411464460): `completed/success`; job `Node UI and art manifest regression (not Unity)` succeeded. The `Node UI mock and canonical Day 01 tests` step succeeded, which includes `test-day01-android-touch.cjs` DOM-ready guard in the current test suite. Art manifest validation/regression, synthetic PNG audit, staged HTML verification, and both artifact upload steps also succeeded.
- Artifacts verified via GitHub metadata: `day01-android-touch` id `14476189172`, 24,325 bytes, SHA256 archive digest `c4e52db46d8b09356f33d2b26e667f5f219241baff192f5c376488bb06d6915c`; `day01-web-demo` id `14476873919`, 8,484 bytes. Both reported unexpired at this check. The Android archive differs in size from the older broken ZIP; do not reuse old copies.
- Actual verification performed: live GitHub PR/issue/docs review, CI run/job/step conclusions and artifact metadata. No local Node rerun, downloaded ZIP extraction, physical Android `file://` launch, touch-flow/device test, Unity Editor or APK test in this pass. Synthetic PNG tests are NOT actual original art assets; Issue #21 remains open.
- Next: owner/QA device test on the new ZIP at 800×360 and 360×800; record screenshots and interactions. Keep C0-v2 / ADR proposals in PR #9/#14 separate from UI mock; no Core/GameState changes or main merge.


## 2026-11-19 — Issue #20: clear stale error messages on successful touch actions

- Scoped UI-only fix: Android Day01 `inspect` and `end` click handlers now clear `#error` after a successful gateway operation, matching the already-correct dynamically generated decision buttons. Previously a validation error could remain visible even after a successful inspection or day completion.
- Implementation commit: `b9adbe4d5a40e8f6e04e99a0acf42192ac01f81d` on `feature/ui-5`; no desktop prototype, GameState, Core or Unity API changes.
- Actually verified: fetched original Android HTML from GitHub, matched exact old handler expressions, changed only two success paths, GitHub Contents API accepted the commit. No Node, browser, physical Android, Unity or CI run executed for this commit; no runtime PASS claimed.
- Remaining: execute browser regression at 800×360 and 360×800, test a failed then successful action with error clearance, Android local-file playtest, and Issue #21 original pixel-art PNG review. PR #12 stays Draft; C0-v2 PR #9 and ADR PR #14 are not merged.


## 2026-11-19 — Issue #20: stale-error regression coverage

- One scoped task: add a Node source-level regression for Android touch inspect and end handlers, requiring a successful gateway call to clear #error before render(). Production HTML unchanged; this guards UI fix b9adbe4d5a40e8f6e04e99a0acf42192ac01f81d.
- Branch feature/ui-5; test commits e074692d9a4c1a33f5310408ab310fc898ed592e (initial guard) and 32e43b210f222719fb96abffd0da786fdb1b8623 (correct inline-script capture regex). File: prototypes/ui-day-one/test-day01-android-touch.cjs.
- Verified via GitHub Contents API: updated test file accepted and re-read; existing HTML handler ordering inspected. Node tests, CI, Chromium and physical Android NOT executed for these commits; no runtime PASS claimed. This is static regression coverage, not proof of touch behavior.
- Next: run node --test prototypes/ui-day-one/test-day01-android-touch.cjs and check fresh CI; then actual Android device playtest. Issue #21 still needs original PNG generation, binary upload and owner art review. C0-v2 PR #9 / ADR PR #14 not merged. No changes to GameState/Core or main.


## 2026-11-19 — Issue #20: stale-error regression confirmed in CI

- Scoped task: verify the corrected static regression test for Android inspect/end-day stale-error clearing against the current UI branch. No production code changes.
- Branch HEAD before this handoff update: `9563ff623c4905cddd7926077f76b8a9cbcec53b`; corrected test commit: `32e43b210f222719fb96abffd0da786fdb1b8623`.
- GitHub Actions [run #44450388944](https://github.com/rixed318/Probably-TAU/actions/runs/44450388944) on HEAD: `completed/success`. Job `Node UI and art manifest regression (not Unity)` and its `Node UI mock and canonical Day 01 tests` step passed; art manifest checks, synthetic PNG audit, staged Android HTML verification and artifact uploads also passed. Earlier runs #44450349312 and #44450342754 for initial test commit `e074692d` failed; they do not establish current status.
- **Actually verified:** live GitHub branch SHA, workflow run conclusion, individual job/step conclusions and corrected test source. **Not executed here:** local Node/Chromium, real Android `file://`, physical touch, Unity Editor or APK. This is static source regression coverage, not proof of real-device interaction.
- Next: QA/owner tests Android on device; Issue #21 requires actual original PNG generation, binary commit and art review. C0-v2 PR #9 and ADR PR #14 remain integration gates; no Core/GameState or main modifications.


## 2026-11-19 — Issue #20: Android physical-device evidence template

- Scoped change: added a copyable L1/L2/P1/P2/S1/T1/O1 test record to `prototypes/ui-day-one/README.md`, requiring run head SHA, artifact identity, Android/browser versions, `file://` or `content://` launch method, orientation viewport dimensions, screenshots, offline mode and failure reproduction.
- Implementation commit: `c1cec6f96e7b83cf1a39c9013d54d698d7935958` on `feature/ui-5`.
- Actually verified: GitHub Contents API accepted the README update; no automated test, Android hardware, Chromium or Unity Editor executed for this documentation-only change. All device cases remain UNTESTED, not PASS.
- Next: QA/owner should fill the evidence record against the latest successful Android artifact. Issue #21 still requires real original PNGs; no Core/GameState/main changes, PR #12 remains Draft.


## 2026-11-19 — Issue #20: accessible Day01 decision progress

- **One scoped change:** `prototypes/ui-day-one/day01-android-touch.html` progress counter (`Решений: N из 3`) now has `role="status" aria-live="polite" aria-atomic="true"`. The existing `render()` text updates can be announced to assistive technologies without stealing focus; no visual design, mock gameplay, Core/GameState, or desktop demo changes.
- **Implementation commit:** `7792aea3a08d9afd0350b7b8abfa5caa9ea1f814` on `feature/ui-5` (PR #12). This is a web accessibility improvement, **not** proof of Android TalkBack behavior.
- **Actual verification:** live GitHub source read before edit; exactly one `id="progress"` element replaced; GitHub accepted the commit. No Node/browser/physical Android/TalkBack/Unity tests executed in this pass; all runtime accessibility checks remain UNTESTED.
- **Follow-up:** QA/owner should check TalkBack announcement after each decision and ensure no duplicate/spam announcements in Android Chrome; execute L1/L2/P1/P2/S1/T1/O1 on device. Issue #21 original PNGs remain outstanding. C0-v2 PR #9 and ADR PR #14 are not merged; no main push or merge.


---

## AutoDev 03 · 2026-11-19 · Issue #20 — Android progress accessibility regression

**Scope:** one Node static regression test for the existing Android Day01 decision-progress live region. No gameplay, Core, HTML styling, assets, or Unity changes.

**Implementation commit:** `f8c7b933252f5fd30e77ce1da9e285a1eba5a7f8` in `feature/ui-5`.

**Changed file:** `prototypes/ui-day-one/test-day01-android-touch.cjs`. The new test asserts `#progress` has `role=status`, `aria-live=polite`, `aria-atomic=true`, that the progress count updates via `textContent`, and that `#error` remains an alert. These attributes were already in the HTML; this pass only prevents accidental regression.

**Verification:** GitHub accepted the test commit and the updated test file was fetched again, confirming the new assertions are present. **Node tests, CI on this commit, Android Chrome, physical TalkBack, and Unity were NOT executed/verified in this pass.** Static assertions are not evidence that TalkBack announces the status correctly.

**Run:** `node --test prototypes/ui-day-one/test-day01-android-touch.cjs`; then inspect the Android offline artifact and verify progress announcements with TalkBack on a real device. Keep PR #12 Draft. C0-v2 (PR #9), ADR (PR #14) and content PR #11 remain pending for runtime integration. Original pixel-art PNGs for #21 remain unverified/not delivered.


---

## AutoDev 03 pass — 2026-11-19 · Issue #20 (TalkBack manual acceptance)

**One bounded task:** documented a reproducible physical-Android TalkBack acceptance procedure in `prototypes/ui-day-one/README.md`: offline extracted canonical HTML, swipe/double-tap decisions, progress announcements 1/3–3/3, focus retention, alert announcement, browser/TalkBack version and recording evidence. This does not modify UI behavior, Core or GameState.

**Documentation commit:** `5619d794fb5efd52a78c451c4ab3d9bfe2e6ebd5` on `feature/ui-5`; this handoff is a subsequent commit. **Tests actually run:** none (documentation-only). No physical Android, TalkBack, Chromium, Node, Unity or APK test was performed; all device results remain **NOT TESTED**. `aria-live` source checks do not prove speech output. Real PNGs for #21 remain outstanding.

**Next:** QA 05 / owner executes the physical-device checklist, records evidence in PR #12; then review original art samples. PR #12 remains Draft; no merge to main.


---

## 2026-11-19 · Issue #21 · one original office art candidate (NOT COMMITTED)

- Generated one original office background candidate using ChatGPT ImageGen (generation ID `fbd3be12-4b93-4ddc-b8ec-0876a6d78624`), Style A / Academic Noir. Scene: fictional dark-fir office, walnut desk, cool left window, warm right lamp, no institutional logo or readable text.
- Source image: 1672×941 RGB. Local nearest-neighbor 320×180 RGB preview created as `probably_tau_office_art_A_candidate_320x180.png`; SHA256 `90b9c982fbf507bce26efb16cd3927ba44bd437f11ebd3057024a7e8c6b822d3`.
- **Candidate only**: 320×180 downscale is a review preview, NOT manually pixel-cleaned production art; scene composition, palette conformity, world-layer registration and quality at 1× remain unapproved. No manifest row marked approved/production.
- **BLOCKED binary upload**: image generation succeeded, but the generated PNG is available only in the execution container; its GitHub CLI cannot reach github.com, and the GitHub connector cannot read container bytes. Therefore **no PNG was committed to `feature/ui-5`**. Do not claim Issue #21 binary acceptance is met.
- Next: transfer the actual PNG bytes to GitHub via authenticated `create_blob(base64)` → `create_tree` → `create_commit` → `update_ref(expected_sha)`, then re-fetch binary to verify SHA and update the asset manifest. Do not merge PR #12 or modify Core.


---

## 2026-11-19 · Issue #21 · office candidate review gate

One bounded documentation task: added `docs/art/reviews/office_background_candidate_2026-11-19.md` with exact prior ImageGen provenance, local 320×180 RGB preview SHA-256, and explicit binary/pixel-cleaning/owner-review acceptance gates. Documentation commit: `246e172a659502ec94d2b2ccae0674f75e476c87` in `feature/ui-5`.

**Actual checks:** live GitHub AGENTS/workflow/PR #9/#11/#12/#14, Issues #5/#13/#20/#21, art bible, manifest and pipeline inspected; local preview `file`/SHA256 checked (320×180 RGB PNG; `90b9c982fbf507bce26efb16cd3927ba44bd437f11ebd3057024a7e8c6b822d3`). Review record created through authenticated GitHub connector. **Not performed:** binary GitHub upload, art approval, Node/CI/Android/Unity tests. Do not mark the RGB preview as production RGBA8 or a GitHub asset.

**BLOCKED for binary acceptance:** GitHub connector has no access to local image bytes; PNG remains only in the execution container. Need authenticated binary transfer and GitHub blob re-verification. PR #12 stays Draft; no Core/GameState/main edits or merge.


---

## 2026-11-19 · Issue #20/#21 — PR #12 status reconciliation

**One scoped task:** reconcile the outdated PR #12 description with current Android WEB prototype and original-art delivery gates; no production UI, assets, Core, GameState, or contracts changed.

- Android Day01 standalone touch HTML, static regression tests, CI artifact workflow and physical-device checklist are present in feature/ui-5. These are **WEB prototype** deliverables, not Unity/APK. Earlier CI success is not a physical Android/TalkBack pass.
- Issue #21 office art candidate: ImageGen preview exists in the previous local run and is documented in `docs/art/reviews/office_background_candidate_2026-11-19.md`; **binary PNG is NOT in GitHub**. Review/cleanup and upload remain blocked until authenticated transfer of actual bytes. The 85 manifest entries are plans, not delivered sprites.
- PR #9 C0-v2 and PR #14 ADR remain unmerged integration references. PR #11 content remains draft; do not promote mock data into canonical runtime behavior.
- Verification in this pass: live GitHub source reads of main README/AGENTS/DECISIONS, workflow, PR #9/#11/#12/#14, Issues #5/#13/#20/#21, art bible, manifest, pipeline and this handoff. No Node, CI, Android, TalkBack or Unity tests run in this pass.
- Next: owner/QA real-device Android playtest; authenticated binary transfer and review of first original office candidate; Unity UI Toolkit only after real Editor and stable API. PR #12 stays Draft, no merge.


---

## AutoDev 03 · 2026-11-19 · Issue #20 — offline navigation guard

**One scoped task:** add a source-level Node regression to `prototypes/ui-day-one/test-day01-android-touch.cjs` rejecting HTML forms, meta-refresh, direct location navigation/window.open, and absolute/protocol-relative anchor destinations in the self-contained Android touch bundle. It guards against accidental network/navigation regressions; it is **not** a runtime sandbox or a security audit.

**UI test commit:** `8999e77cf712d9d73d8a130abe14cb30d1961204` in `feature/ui-5`. No Core, GameState, desktop demo, art manifest or production asset changes.

**Actual verification:** GitHub accepted the test commit; source was inspected. **Node tests NOT RUN** in this pass, nor CI, Chromium, physical Android/TalkBack or Unity. Reproduce with `node --test prototypes/ui-day-one/test-day01-android-touch.cjs`; check Android browser separately. Keep WEB_READY distinct from ANDROID_DEVICE_TESTED and APK_BUILT.

**Art #21:** office preview remains candidate only, with no verified PNG binary in the repository. PR #12 stays Draft; C0-v2 PR #9 and ADR PR #14 are references, not a merge authorization.


---

## AutoDev 03 · 2026-11-19 · Issue #20 — repair offline navigation regression

**One bounded fix:** the four regular-expression literals added by the previous offline-navigation guard used doubled backslashes (for example `/<form\\b/`), so they matched literal backslash sequences rather than HTML word boundaries / JavaScript navigation. Corrected these expressions and added positive-control test snippets for form, meta refresh, location assignment and protocol-relative external anchor.

**Implementation commit:** `ba26abef0f58a972104e9a35526209e3be55f560` on `feature/ui-5`; handoff updated separately. Only `prototypes/ui-day-one/test-day01-android-touch.cjs` and this handoff changed. No GameState/Core, desktop prototype, art or Unity changes.

**Verification:** source read from live GitHub and corrected test file committed. Test suite execution, CI, browser, Android/TalkBack and Unity **not run in this connector pass**; positive controls are source assertions, not an observed test result. Run `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` and inspect CI. Existing Issue #21 PNG delivery and real-device Android playtest remain pending. PR #12 remains Draft, no merge.


## 2026-11-19 · Role 03 · Issue #20 · offline escape-route regression

- Commit `d70b9d4746cfc038c0002dc43cf8283286de8077` adds static guards to `prototypes/ui-day-one/test-day01-android-touch.cjs` against `navigator.sendBeacon`, `importScripts`, `new Worker`, and protocol-relative CSS `url(//...)` in the main standalone Android HTML.
- Scope: regression-test source only; no HTML behavior, Core/GameState, art assets, or Unity changes.
- Verification: GitHub contents API accepted the source commit. Full Node test suite, browser, real Android, CI for this SHA, and Unity were **not run** in this pass. The guards are static heuristics, not a security sandbox; legacy HTML is not covered by these two added assertions.
- Follow-up: run `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` and verify Android viewport/device; PNG delivery for #21 remains pending.


## 2026-11-20 · Role 03 · Issue #20 · legacy offline guard parity

One bounded regression-test change: `prototypes/ui-day-one/test-day01-android-touch.cjs` now applies existing `sendBeacon` / `importScripts` / `new Worker` and CSS protocol-relative `url(//...)` source guards to **both** distributed Android offline HTML variants, not only `day01-android-touch.html`. Implementation commit `b7f87fcf427c9710ef0f83c0898b669227245ef1` in `feature/ui-5`. No production HTML, desktop demo, Core/GameState, Unity or art changed.

Verification: live GitHub source read and authenticated test-file commit confirmed. **Node suite NOT executed** in this run; no browser, CI, real Android/TalkBack, or Unity execution. Run `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` and perform physical Android playtest before marking device-ready. Static patterns do not prove network isolation. Issue #21 office PNG still not verified as committed. PR #12 remains Draft; no main merge.


## 2026-11-20 · Role 03 · Issue #20 · offline navigation parity

One scoped test-only change: apply the existing `forbiddenNavigation` source checks to the second distributed `prototypes/ui-day-one/day01-android-offline.html` file, so both Android HTML variants reject form elements, meta-refresh, direct location/window navigation, and external anchors. The checks are static heuristics, not runtime isolation. Test source commit: `450bd1fc3aba03cbc2727b13564a244f26c53037` in `feature/ui-5`.

Verification: GitHub accepted the test-file commit. Node test suite, CI, browser, physical Android/TalkBack and Unity were **not executed** in this run. Reproduce with `node --test prototypes/ui-day-one/test-day01-android-touch.cjs`; validate on device separately. Issue #21 original PNG remains a candidate, not verified in GitHub. C0-v2 PR #9 and ADR PR #14 remain integration references. No GameState/Core edits, no main push or merge; PR #12 remains Draft.


## 2026-11-20 · Role 03 · Issue #20 · location-navigation guard coverage

One bounded test-only change: expanded `forbiddenNavigation` in `prototypes/ui-day-one/test-day01-android-touch.cjs` to reject direct `location = ...`, `window.location = ...`, `document.location = ...`, `top.location.replace(...)`, and `self.location.assign(...)`, in addition to existing `location.href` and `window.open` patterns. Both Android offline HTML variants use this shared guard. Added representative positive-control snippets. Implementation commit: `6ffdc44b7b2acd2551407fbbbc62bfdbca272d0a` on `feature/ui-5`.

Verification: live GitHub source read, connector accepted commit. Full Node suite, CI, Chromium, real Android/TalkBack and Unity **not run**; these regex checks are heuristics and not a browser security guarantee. GitHub git transport from local container failed DNS resolution, so no local checkout-based test was possible. Run `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` in CI/QA. No Core/GameState, production HTML, art, desktop demo or main changes. Issue #21 PNG binary delivery and physical Android playtest remain outstanding; PR #12 Draft, no merge.


## 2026-11-20 · Role 03 · Issue #20 · legacy touch/cutout regression

One bounded test-only change to `prototypes/ui-day-one/test-day01-android-touch.cjs`: the second offline Android HTML now has source assertions for the coarse-pointer media query, 48px minimum width (in addition to existing 48px height), and all four safe-area inset references. This guards the legacy desktop CSS's 35px small-button default from accidentally losing its mobile override. No production HTML, GameState/Core, Unity or art was changed.

Implementation commits on `feature/ui-5`: `29462614aeec28b50a3532a9c9e77e813e7f9524`, then `994fda5a1a8789a111bdc4d5a65e6cfeb33674ec` (simplified safe-area checks). **Verification:** authenticated GitHub writes succeeded and source was re-read; Node test execution, CI for these commits, browser viewport emulation, physical Android/TalkBack and Unity were **not performed**. Static checks do not prove actual tap target geometry. Run `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` and inspect 800×360 / 360×800 viewports; real device playtest remains open. Issue #21 binary PNG not verified in GitHub. PR #12 remains Draft; no main merge.
