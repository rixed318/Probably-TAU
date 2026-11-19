# First-day office UI prototype · Issue #5

**Purpose:** standalone, dependency-free browser preview of the proposed Unity 6 presentation layer. This is **not** the Unity game and is **not** an alternative GameState. The real Unity project/approved Core contracts were not present when this prototype was created.

## Run

1. Download/clone the repository.
2. Open prototypes/ui-day-one/index.html in Chrome, Edge, or Firefox.
3. Click the file on the desk (or **«Изучить заявку»**).
4. In **«Проверка»**, inspect the estimate and/or request clarification.
5. Choose **«Согласовать»**, **«Отклонить»**, or **«Отложить»**.
6. Click **«Завершить день»** for the precomputed budget report.
7. Refresh the page to try a different branch. **Day 2 is a placeholder only.**

All files are local: no third-party CDNs, fonts, trackers, SDKs or runtime dependencies.

## Test the mock gateway

With Node.js 18+:

~~~sh
node --test prototypes/ui-day-one/test-fixtures.cjs
~~~

Tests check the canned response flow, invalid commands, fixed financial read models, and report navigation. Browser visuals and Unity compilation have not been automatically verified by those tests.

## Files

- index.html: semantic layout / dialogue / document / report dialogs.
- styles.css: original CSS-painted office, palette, responsive layout and accessibility.
- fixtures.js: prewritten UI snapshots and responses, one scenario with branching results; **no actual simulation, calculation, or economy rules**.
- app.js: UI renderer, commands through a mock gateway, focusable actions, tab navigation and error presentation.
- test-fixtures.cjs: portable Node.js fixture-contract smoke tests.
- ../../docs/UI_UX_SPEC.md: wireframes, design tokens, interaction sequence, intended Unity integration.
- ../../docs/handoffs/ui.md: handoff for chat 00 / chat 01 / QA.

## Boundaries

ApproveProposal, InspectDocument, RequestClarification, RejectProposal, DeferProposal and AdvanceDay are **conceptual action identifiers**, not claims that Core currently exports these exact contracts. In Unity, UI Toolkit UXML/USS and a Presentation adapter will read approved Application read models and issue validated commands. No GameState reference or mutable domain state belongs in UI.

Mock sums, story, persons and scenes are fictional and not official financial or personnel data about Togliatti Academy of Management. The symbol on the wall is invented, not the institutional logo.


## Android offline HTML playtest (Issue #20)

**Artifact:** open [UI standalone smoke tests — workflow runs](https://github.com/rixed318/Probably-TAU/actions/workflows/ui-smoke.yml), select the latest **successful run on `feature/ui-5`**, download `day01-android-touch` under **Artifacts**, and extract the ZIP. GitHub may require signing in to download Actions artifacts; artifacts can expire. Do not use a failed or older run as evidence for the current branch. Check the run's head SHA before reporting a result. This is a **web prototype, not an APK**. The archive contains two independent entry points:

- `day01-android-touch.html` — three-visitor canonical Day01 display/mock interactions (preferred Android test).
- `day01-android-offline.html` — older one-request desktop-style mock packaged as a standalone fallback.

On an Android phone, copy/extract the HTML file into local storage, open it from a file manager with a browser that supports local `file://` HTML, and keep airplane mode enabled to verify offline operation. Some Android browsers/file managers restrict opening local HTML; in that case use a different browser/file manager and record the exact error. Do **not** infer a working hosted HTTPS URL or APK.

### Manual viewport regression checklist — not yet executed

Record device model, Android/browser version, viewport (CSS px), orientation, result (PASS/FAIL), screenshot, and any scroll/overlap defect for **each** case:

| Case | Viewport | Steps | Expected |
| --- | --- | --- | --- |
| L1 | 800×360 landscape | Open canonical touch file; inspect header, queue and first document | No clipped critical controls, no horizontal page scroll, text remains readable |
| L2 | 800×360 landscape | Select DOC-01, inspect, choose a decision, open end-day report | Actions remain reachable; no duplicate decision/ledger effect |
| P1 | 360×800 portrait | Rotate/reload; inspect three visitor entries and document | Single-column layout, no overlapping panels, buttons fully visible or reachable by vertical scroll |
| P2 | 360×800 portrait | Inspect and decide; open end-day report | No trapped scroll/focus, all action labels visible |
| S1 | Device with notch/insets | Test both orientations | Controls not hidden behind camera cutout, gesture/navigation bars |
| T1 | Both orientations | Tap each active control near its edges; test 150% browser text zoom | Target is at least 48 CSS px wide/high where applicable, no accidental adjacent action |
| O1 | Both orientations | Airplane mode, reload from extracted local file | Demo runs without remote fonts, scripts, API requests or CDN |

**Important:** 48 CSS px is a web touch-target proxy, **not proof of 48 Android dp** on every device. Measure rendered bounding boxes or test on the target device. The Node bundle tests assert static CSS and script syntax only; they do **not** perform browser layout, touch emulation, Android WebView, or Unity checks.

Run static regression separately from repository root:

```sh
node --test prototypes/ui-day-one/test-day01-android-touch.cjs
```

Report findings to QA 05 and coordinator 00 in PR #12. Keep the original desktop `index.html` unchanged; never treat a passed ZIP integrity check as a completed Android playtest.

### Device evidence record (copy once per physical Android test)

Fill this in after testing the **extracted** `day01-android-touch.html` file, not the ZIP preview. A CI PASS or desktop emulator result must not be recorded as a physical-device PASS.

```text
GitHub Actions run URL:
Run head SHA:
Artifact name: day01-android-touch
Device / Android version:
Browser / version:
Launch method (file:// or content://):
Airplane mode: ON / OFF
Landscape CSS viewport (width × height):
Portrait CSS viewport (width × height):
L1: PASS / FAIL / BLOCKED — screenshot or reason:
L2: PASS / FAIL / BLOCKED — screenshot or reason:
P1: PASS / FAIL / BLOCKED — screenshot or reason:
P2: PASS / FAIL / BLOCKED — screenshot or reason:
S1: PASS / FAIL / BLOCKED — screenshot or reason:
T1: PASS / FAIL / BLOCKED — screenshot or reason:
O1: PASS / FAIL / BLOCKED — screenshot or reason:
JS console errors (if accessible):
Reproduction steps for each failure:
Tester / date:
```

If the browser refuses local-file access, mark O1 and dependent cases BLOCKED and preserve the exact error; do not substitute an online preview for the offline acceptance criterion.
