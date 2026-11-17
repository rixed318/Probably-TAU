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
