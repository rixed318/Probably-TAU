# ADR-C0-002 — Scheduled event instance ordering and same-day children

**Date:** 2026-11-14. **Decision by coordinator 00:** APPROVED FOR FEATURE-BRANCH IMPLEMENTATION ONLY. **Not merged into `main` without user authorization.** This narrows ambiguous implementation details of approved C0-v2; it does not replace the canonical EndDay stages.

**Context:** Gate C0-v2 in PR #9, ADR-C0-001 in PR #14, reconciliation proposal N0 in PR #16, event hooks draft PR #11, Save spec PR #10. Earlier N0 text sorted by `phase` and `creationOrdinal`, conflicting with C0's `DueDayIndex`, descending `Priority`, ascending `StableEventId`.

## Decision

1. **One scheduled occurrence = one immutable stable `instanceId`.** In the C0 due-event comparator, `StableEventId` means this **instanceId** (ordinal, case-sensitive). `definitionId` identifies the reusable content definition and is *not* sufficient to distinguish repeated instances. Both IDs remain persisted and immutable for the occurrence.
2. **Priority:** every scheduled occurrence persists a signed Int32 `priority`. If a content definition omits it, Core assigns 0 **once at scheduling time**; it is not recalculated on load or content-version change. Ordering: `dueDayIndex ASC → priority DESC → instanceId ASC`, using relational integer comparison and `StringComparer.Ordinal`, never arithmetic subtraction that could overflow.
3. **Phase boundary:** C0 owns the high-level EndDay stages. At the **start of the scheduled due-outcomes stage, after the project-tick stage**, Core fixes a deterministic snapshot of all eligible persisted occurrences with `dueDayIndex <= closing DayIndex n`; only those occurrences are considered for this pass. Neither `triggerPhase` nor `creationOrdinal` participates in the due queue comparator. `triggerPhase` is a content routing/presentation label and must be mapped explicitly to C0 stages.
4. **New child due on/before today:** if an effect *attempts to create a new scheduled child* during closing day n with `dueDayIndex <= n`, Core rejects the **entire prepared command/AdvanceDay transaction without partial state, ledger, receipt, or RNG changes**. Content must schedule the child for at least n+1. Already-persisted overdue events with due day <=n remain valid eligible candidates. This is a fail-closed v1 policy, not recursive execution and not implicit date rewriting.
5. **Idempotency:** durable receipt keys include `instanceId` plus resolution kind; a processed instance is removed/marked completed exactly once. Restoring a snapshot must retain instance IDs, priority, due day, receipt IDs and RNG; cannot reapply effects or financial postings. NPC proposes effects, but Core alone commits metrics/ledger and RNG. E09 from content PR #11 is report-only and **never posts a second weekly settlement**.
6. **Ownership:** Core 01 defines concrete types and validates command/EndDay transaction; NPC 02 defines the immutable content-to-instance adapter and proposes eligible intents; Content 04 defines conditions/effects/definitions; QA 05 verifies ordering/replay. No agent introduces a second mutable GameState or a second finance engine.

## Acceptance tests for the implementation

- Same due day, different priorities: higher priority first; same priority: instanceId ordinal ascending regardless of collection insertion order.
- Two occurrences of the same definitionId have distinct persisted instanceIds and run independently.
- A successful project tick can make an eligible due effect visible at the phase snapshot; a child spawned during this phase is not processed recursively.
- A child with `dueDayIndex<=n` rejects atomically; there is no partial ledger write, RNG movement, or receipt.
- Save→Load preserves instanceId, priority, due day, receipts, and deterministic outcomes.
- E09's report is not a second income/obligation posting.
- Unknown or invalid priority/ID fails validation rather than silently reordering events.

## Compatibility / migration

This is an **unmerged pre-runtime contract clarification**. The current minimal `GameStateSnapshotDto` in PR #9 stores only pending/completed string IDs, not the eventual event-instance DTO, so no existing persisted save format is being migrated. When full event queues are added, new fields must be versioned with the snapshot schema and documented for QA. The draft Content JSON does not become executable merely by referencing this ADR.

## Integration / approval boundary

Agent work may proceed in their feature branches against these details. This PR does not authorize merger into `main`; the repo owner must explicitly approve that. If implementation reveals a conflict, open a new ADR and do not silently change the comparator, event key, or financial phase.
