# Chat 02 — NPC/EventDirector handoff

**Date:** 2026-11-14. **Issue:** #4. **Branch:** `feature/npc-4`. **Status:** design proposal only; integration BLOCKED on concrete Core APIs and typed content predicates.

## Completed in this run
- Reviewed main README, AGENTS, CHAT_WORKFLOW, GDD and ARCHITECTURE; Core Gate C0-v2 in PR #9 and ADR-C0-001 in PR #14; Content PR #11 and its `CONTENT_SPEC_DRAFT.json`.
- Created `docs/proposals/NPC_EVENT_SCHEDULING_N0.md`: stable event IDs and ordering, non-recursive due handling, Core-owned atomic commit/ledger, RNG and receipt invariants, content mapping E02/E03/E09, and 8 proposed acceptance cases.
- Did **not** create a second GameState, mutable NPC state store, C# runtime classes, or executable content AST.

## Changes and validation
- Files: `docs/proposals/NPC_EVENT_SCHEDULING_N0.md`, this handoff.
- Validation performed: manual cross-reference against Core C0-v2 and Content draft IDs/DayIndex; no executable tests.
- **NOT RUN:** Unity build, C# compile, unit/EditMode tests. No compatible concrete Core command/snapshot/queue DTOs in main; feature/core-2 currently exposes standalone PCG32 but not the required complete interfaces.
- No changes to `main`, no merges, no Issues closed.

## Risks and dependencies
1. Chat 01: expose concrete scheduled queue / transaction / receipt ports and phase ordering; define policy for new child due<=closing day.
2. Chat 04: replace human-readable `condition`/`fallback` strings with typed declarative conditions; clarify E02/E03 priority and ensure E09 is not a second finance executor.
3. Chat 05: save/load stable queue, event receipts, named PCG32 streams and processed keys.

## Next small task
After coordinator review of N0 and typed content schema, implement a pure-C# immutable event definition validator with deterministic ID ordering and tests; integrate EventDirector only when Core API is concrete and accepted.

## AutoDev follow-up — 2026-11-14 (N0/C0 alignment)
- **Completed:** reviewed current main governance and Issue #4, Core PR #9 C0-v2, ADR PR #14, Content PR #11 and NPC PR #16. Updated `docs/proposals/NPC_EVENT_SCHEDULING_N0.md` to withdraw the incompatible N0 sorting recommendation and propose C0-v2-compatible priority/instanceId sorting and a due-stage snapshot after project ticks.
- **Commit:** `98f44ffbe11b18ff590a56ae8ee543cb814e548d` on `feature/npc-4`; PR #16: https://github.com/rixed318/Probably-TAU/pull/16; coordination comment: https://github.com/rixed318/Probably-TAU/pull/16#issuecomment-6880587468.
- **Checks:** manual comparison of N0 proposal with C0-v2 and ADR-C0-001; no executable source changed. **NOT RUN:** C# compilation, Unity tests, event-scheduler integration tests. No claim of successful runtime validation.
- **BLOCKED:** executable EventDirector awaits Core 01's confirmation of stableEventId (= instanceId?), priority ownership/default, snapshot boundary and child due<=n policy; typed Content predicates remain outstanding. No alternate Core/GameState/ledger created.
- **Next bounded increment:** independent read-only manifest validator for event IDs, dayIndex, feeds and phase aliases (must not sort or execute events); request Content 04 review. No merge to main.


## AutoDev 02 — ADR-C0-002 reconciliation + typed AST review (2026-11-14)

**One scoped task completed:** updated `docs/proposals/NPC_EVENT_SCHEDULING_N0.md` to match coordinator-approved-for-feature-work ADR-C0-002 in Draft PR #17. N0 now consistently uses `dueDayIndex ASC → priority DESC → instanceId ASC` (ordinal), persisted Int32 priority defaulted once to 0, due-stage snapshot after project ticks, and atomic rejection of new children due<=closing day. Removed stale advice to sort E02/E03 by creationOrdinal. Existing overdue items remain eligible; E09 is report-only.

**Review of Content PR #11:** typed AST (`always/all/any/flagEquals/factExists/dayIndexAtLeast`) is suitable for a pure, read-only evaluator, not a runtime-approved wire schema. Partner income must be gated by durable `E03_mentor_paid`, `E07_delivered`, DayIndex>=4 and a unique earned receipt. **Risk to resolve with 01/04:** both onsite/remote branches share `partner/{sourceInstanceId}/delivery` earnedId template; if each branch uses a distinct source instance, exactly-once revenue is not guaranteed. Require a canonical agreement source ID and Core ledger deduplication before wiring.

**GitHub commit:** `d761ccaa03b848a29513c907672b63e50af69d17` (N0 documentation). This handoff is committed separately on the same `feature/npc-4` branch. PR #16 stays Draft; no merge/main changes.

**Checks performed:** manual source-to-source comparison of N0, ADR-C0-002, Content event hooks, and Core GameStateSnapshotDto retrieved via GitHub. **NOT RUN:** C# compile, unit tests, Unity Editor, EventDirector integration; this task changed documentation only.

**BLOCKED for EventDirector:** concrete Core scheduled-event DTO/transaction ports and receipt APIs are missing. **Next bounded task:** independent pure-C# typed condition evaluator + deterministic negative tests (no GameState, ledger, RNG, scheduler), with actual C# execution only when a runner is available.


## AutoDev 02 — pure typed condition evaluator (2026-11-14, 23:39 UTC+4)

**One scoped task (#4):** implemented an independent pure C# condition AST and deterministic read-only evaluator matching the six operators currently proposed in Content PR #11: `always`, `all`, `any`, `flagEquals`, `factExists`, `dayIndexAtLeast`. This is **not** a runtime JSON adapter and does not claim the draft schema is approved. Code: `Runtime/Npc/EventConditionEvaluator.cs`; standalone tests: `Tests/Npc/EventConditionGoldenTests.cs`.

**Commits:** implementation `de5e5e9ddd410531499501f9b0f786cc8e0b5765`; tests `9dfd666f57f45a99aabd0db462e2a3d381889389`; handoff commit follows. All on `feature/npc-4`; PR #16 stays Draft. No main changes or merges.

**Behavior:** exact ordinal flag comparisons, missing flags/facts evaluate false, nonempty all/any groups, invalid/negative day or null context rejected, short-circuit boolean groups, no RNG, GameState, ledger, scheduler or Unity dependency. Factory methods enforce valid AST nodes; caller supplies read-only state facts/flags. The evaluator does not generate or mutate receipts, earned income, or event IDs.

**Tests:** added 16 standalone assertions including partner onsite eligible at DayIndex 4 only after both facts, remote exclusion, case sensitivity, missing fact, invalid nodes and negative day. **NOT RUN**: C# compile, golden tests and Unity EditMode (no dotnet/csc/mcs available in this execution environment). Source creation via GitHub confirmed; no claim of passing C# tests. Run `EventConditionGoldenTests.RunAll()` via a real .NET test runner once QA/Core establishes one.

**Dependencies / BLOCKED:** Full EventDirector still requires Core scheduled-event instance/transaction/receipt API and canonical earned-income source key. JSON-to-AST parsing/validation is deliberately not included; coordinate schema with Content 04. ADR-C0-002 PR #17 remains unmerged and applies only to feature development. Next bounded task: add a standalone .NET test runner or JSON schema adapter once Core/QA test infrastructure and Content schema are confirmed.
