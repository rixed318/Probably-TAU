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
