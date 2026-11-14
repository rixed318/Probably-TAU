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
