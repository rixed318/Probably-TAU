# NPC/EventDirector — deterministic scheduling contract (proposal N0)

**Status:** PROPOSED, non-executable design. Owner: Chat 02. Issue #4. Reviewed against Gate C0-v2 (PR #9) and ADR-C0-001 (PR #14), and Content PR #11 on 2026-11-14. This document does not add or override a Core API. All names/people/events in PR #11 are fictional game content, not statements about the real academy.

## Scope of this small increment
Define deterministic queue ordering, idempotency and boundaries for *scheduled consequences*, so Chat 01 can implement AdvanceDay and Chat 04 can author content without conflicting ownership. No C# runtime is added while concrete Core GameState/CommandProcessor DTOs are unavailable.

## Ownership and lifecycle
- **Core GameState** is the sole mutable owner of persisted scheduled-event instances and event completion receipts. NPC/EventDirector receives a read-only state snapshot, stable content definitions, and a deterministic named RNG stream from Core, and **proposes** command effects/schedule intents; only the Core command transaction validates and commits.
- Definitions are immutable content records keyed by `definitionId`; instances have distinct `instanceId`. One definition may spawn multiple instances with distinct IDs.
- A scheduled item has `instanceId`, `definitionId`, `dueDayIndex` (0-based), `phase`, `sourceCommandId`, `sourceInstanceId` (nullable), `creationOrdinal` (monotonic Int64), `payload` (versioned content-defined immutable data) and `status`. These are **proposed logical fields**, not finalized Core C# property names.
- A receipt is identified by stable `instanceId + resolutionKind` (or a Core-approved equivalent). Replaying a committed command or restoring a save must not emit a second receipt, metric effect, scheduled child, or ledger posting.
- Scheduled financial effects are *intents*, not direct mutations: only Core Economy/Ledger posts checked Int64 kopecks with unique posting keys. NPC code never modifies cash, payables or metrics directly.

## Canonical deterministic ordering
For eligible events at the current closing `DayIndex = n`, Core takes a **fixed queue snapshot at transition start**. Order by:
1. `dueDayIndex` ascending;
2. `phase` in a **Core-owned explicit order** (never alphabetic; for v1 scheduled due outcomes only; `endday_finance` is a separate Core settlement phase);
3. `creationOrdinal` ascending;
4. `instanceId` ordinal, case-sensitive as final tie-break.

Items with `dueDayIndex > n` remain queued. Any new item created during this transition with `dueDayIndex <= n` is **not executed recursively** in the same AdvanceDay. Its handling (reject invalid due date or defer to next transition) must be defined by Core before executable integration. No reliance on dictionary iteration, locale-sensitive sorting, `DateTime.Now`, `Guid.NewGuid`, or unseeded randomness.

## Resolve / schedule transaction
1. Read the immutable snapshot, select eligible candidates in stable order, and evaluate content predicates against the same documented phase snapshot.
2. Select branches using the `npc.visitors` or `economy.events` named Core PCG32 stream as appropriate, with candidates sorted by stable ID before weighted selection. Queries/preview must not advance RNG.
3. Prepare one atomic Core command containing expected revision/day, proposed metric deltas, ledger intents, child schedule intents, receipt keys, and consumed RNG state.
4. Core validates unique IDs, available resources, due-day constraints and idempotency. On reject, **nothing changes** (including RNG).
5. On commit, Core writes the state/ledger/queue/receipts and only then publishes domain events. Repeated `CommandId` returns the prior receipt without reapplying effects.

## Content PR #11 compatibility notes
- Day 01 = `DayIndex 0`; `E02/E03` have `dayIndex 1`; `E09` is closing `DayIndex 4` settlement; `E14` has `dayIndex 9`.
- `eventManifest[].triggerPhase = "scheduled-content"` is a **content alias**, not a runtime enum. `E09` has `triggerPhase = "EndDay-settlement"` and must not become a normal NPC visit or an independent second weekly billing pass.
- `E02` and `E03` both occur on DayIndex 1 but differ in kind; the content author must explicitly define scene/desk-task priority if one is required. Until then use deterministic `creationOrdinal`, not JSON object order.
- `requirements` and `fallback` are currently human-readable strings. They are **not safe executable predicates**; typed condition AST/IDs require separate content-schema agreement with Chat 04 and coordinator 00.
- `PARTNER-1/2` conditional revenue is **not** posted on contract signing. A future earned-income intent must require actual delivery, due index >= 4 and a unique Core ledger key. `E09` is not an authority to re-post Core's weekly income/obligation.
- Current `budgetKopecks` values in Content draft are JSON numbers; before interoperable 64-bit wire format they must be serialized as decimal strings per C0-v2. This is a pending schema revision owned by Chat 04/05, not changed here.

## Proposed acceptance tests for future implementation
| Test | Expected result |
|---|---|
| N0-01: same seed, content version, ordered commands | identical visitor IDs, queue order, branch outcomes, receipts |
| N0-02: two events same due index, different creation ordinal | stable ordinal order independent of insertion/dictionary order |
| N0-03: same `CommandId` applied twice | one event resolution, one child schedule, one posting |
| N0-04: rejected event due to invalid resources | no RNG draw committed, no state mutation |
| N0-05: event schedules child due today | no recursive execution in same transition |
| N0-06: save/load between schedule and due day | same event IDs, order, outcomes and RNG state |
| N0-07: E09 on DayIndex 4 | exactly one Core weekly settlement, no NPC duplicate |
| N0-08: unavailable content branch | deterministic fallback, no crash or invented choice |

**Not run:** these are acceptance specifications, not implemented tests.

## Integration blockers / decisions needed
1. Core 01 must publish concrete GameState snapshot, CommandProcessor, scheduled-event and RNG stream interfaces, and define phase order / invalid due<=current behavior.
2. Content 04 must replace string predicates with a typed schema and clarify E02/E03 priority and E09's presentation-only role.
3. QA 05 must cover receipt, queue and RNG serialization, including resume without duplicate effects.

**Next executable increment:** implement pure-C# immutable event definition validator/adapter only after N0 fields and content condition schema are accepted; then EventDirector using Core-owned transaction ports. No Unity Editor or C# compiler execution claimed here.
