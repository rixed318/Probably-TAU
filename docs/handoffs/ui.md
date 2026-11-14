# Handoff · Chat 03 — UI/UX · Issue #5

**Date:** 2026-11-14  
**Repository:** https://github.com/rixed318/Probably-TAU  
**Work branch:** feature/ui-5  
**Base:** main (created from d673322cd9d0d366a27a57b81056504dfb26c731)  
**State:** standalone interactive browser prototype + UI/UX proposal, waiting for coordinator 00 review.  
**PR:** will be linked after submission; check branch history for latest commit.

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
