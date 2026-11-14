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
