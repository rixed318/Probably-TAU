# Probably-TAU — versioned save/load specification (draft v0.1)

**Owner:** ЧАТ 05 · **Issue:** #6 · **Dependency:** Gate C0 by chat 00 / Core #2 + Economy #3.  
**Implementation state:** NOT IMPLEMENTED. This file proposes the contract; field names, migration boundaries and file-store API are not approved.

## Goal and guarantees

Save and restore one complete, deterministic single-player session at an **atomic gameplay checkpoint**. Restore player-visible progression and all hidden state required to produce the same future gameplay: clock/phase, economy and immutable ledger history, projects, documents and inspections, visitor queue, dialogue/negotiation session, inventory, delayed events, flags/cooldowns, applied command IDs and PRNG state. No duplicate payment, award, NPC generation or event after a reload.

**Safety priorities:** (1) never mutate live GameState on invalid input; (2) never destroy the sole valid save; (3) never resume a half-applied command; (4) fail visibly with actionable error.

## Proposed envelope

Shape follows GDD §Г8. JSON example is illustrative, not binding Core DTO:

```json
{
  "schemaVersion": 1,
  "buildVersion": "0.1.0-dev",
  "timestampUtc": "2026-11-14T12:00:00Z",
  "checksum": "<hex-encoded SHA-256 of canonical envelope excluding checksum>",
  "payload": {
    "campaignId": "<stable campaign GUID>",
    "contentVersion": "content-v1",
    "worldTime": { "dayIndex": 1, "phase": "visitors" },
    "difficultyId": "normal",
    "economy": {
      "openingBalanceMinor": 200000000,
      "balanceMinor": 200000000,
      "ledger": [],
      "obligations": []
    },
    "randomState": { "algorithm": "<approved algorithm>", "state": "<opaque bytes or deterministic scalars>" },
    "appliedCommandIds": [],
    "processedDayIds": [],
    "documents": [],
    "visitorQueue": [],
    "negotiations": [],
    "projects": [],
    "inventory": [],
    "eventQueue": [],
    "eventHistory": [],
    "reputation": {},
    "metrics": {},
    "flags": {},
    "campaignState": {}
  }
}
```

All field names inside payload are **placeholders pending C0**; save only canonical validated DTOs, not runtime references. Explicitly include evolving content identifiers so deterministic replay can detect content/version mismatch. `timestampUtc` is informational metadata and must not influence simulation or replay. Money is an integer 64-bit minor unit; currency unit must be agreed globally. Use stable IDs, not list indices, for identity. No `UnityEngine.Object`, `GameObject`, `ScriptableObject` references or executable content in file.

**Integrity proposal:** canonical UTF-8 JSON with defined property ordering and invariant culture; SHA-256 of canonical envelope with the `checksum` property omitted. SHA-256 guards against accidental corruption here; it is **not** an authentication or anti-cheat mechanism. Hash must be computed/validated over precisely identical bytes; agree normalization and serializer/settings before shipping. Unknown JSON fields policy and integer-overflow behavior require explicit tests.

## Core boundary proposal (needs C0 approval)

- `CaptureSnapshot(GameState) -> SaveSnapshotDto`: read-only deep copy of a logically committed state; never advances RNG or time.
- `ValidateSnapshot(dto, ContentRegistry) -> ValidationResult`: validates invariants and references before activation.
- `RestoreSnapshot(dto) -> GameState`: constructs a fresh fully verified state (no in-place partial modification).
- `ISaveStorage`: reading/writing primary, temporary and backup files; filesystem is infrastructure, not Core.
- `SaveService.Save(slotId, gameState)`: capture at validated safe checkpoint, serialize, checksum, write temp/verify/commit; returns structured success/error.
- `LoadService.Load(slotId)`: read primary/backup, verify envelope, migrate through registered versions, validate, restore; returns isolated state and source of recovery.
- Game orchestration, not file I/O, decides save triggers (manual safe point, end-of-day checkpoint, graceful exit if possible).

These are **conceptual contracts**, not declared classes or enforced signatures. Do not implement until chat 01 confirms the state and command APIs.

## Save transaction protocol

1. Confirm command boundary: no `CommandProcessor` transaction or `AdvanceDay` intermediate stage in flight. If busy, queue request until a safe checkpoint or return a non-destructive Busy error.
2. Capture immutable/deep-copy DTO including applied-effect IDs and RNG, then validate. Never mutate world state while capturing.
3. Serialize into deterministic canonical JSON; compute checksum; emit an envelope with non-simulation metadata.
4. Write to a **unique temporary file in the same directory/volume**, close/flush as platform permits; read back and verify schema, checksum and basic structural validity.
5. Preserve the previous valid primary as `.bak` and commit the temporary file to primary using supported atomic replacement. Prefer `File.Replace` where supported; choose a documented, tested fallback for unsupported filesystems. Do not claim power-loss durability across all filesystems.
6. Verify committed primary; clean abandoned temporary files safely. A bad/incomplete temp can never be treated as primary. Keep at least one last-known-good recoverable copy before any destructive cleanup.

**Important:** backup rotation must not replace a good backup with an already-corrupt primary. Perform validation before promoting a primary into backup. Startup recovery must tolerate a temp left after crash and must not guess which artifact is newest solely from timestamps.

## Load transaction protocol

1. Enumerate primary and backup for selected slot. Never delete or rewrite any candidate during discovery.
2. Read bytes with file size limits, handle permissions/I/O errors, parse constrained JSON (no executable types).
3. Validate checksum, supported `schemaVersion`, required fields, IDs and integer ranges. If primary fails, try backup and report recovered source.
4. If old version is supported, run deterministic, explicit, ordered migrations `vN -> vN+1`, each pure/validated, without overwriting the source file. Never auto-downgrade unknown future versions.
5. Resolve content definition IDs according to approved compatibility policy; reject or map missing/renamed definitions explicitly. Never silently substitute unrelated definitions.
6. Validate all cross-entity links, ledger/finance reconciliation, unique execution IDs, event queue, RNG state and negotiation phase/round.
7. Construct complete new GameState; only after success swap session reference and rebuild UI read-models. Do not emit domain side effects as if saved events had just occurred.
8. Optionally offer to commit a newly migrated snapshot **after** a successful load and user-appropriate recovery policy; preserve original/backup until a valid migrated snapshot is durably committed.

## Save slots and compatibility

- Suggested filenames: `slot-<validated-id>.json`, `slot-<validated-id>.bak`, `slot-<validated-id>.<unique>.tmp`. Reject path traversal and unsafe slot IDs. Store under Unity's `Application.persistentDataPath` through the Infrastructure adapter.
- A schema version change is separate from a content data change and a build version change. Increment **schemaVersion** whenever DTO meaning/shape changes incompatibly; use explicit migration steps.
- Keep committed compatibility fixtures as future tests (at least oldest supported schema + one per breaking migration). Migrations should be idempotent at the **version boundary** and never duplicate economic entries, generate visitors or consume RNG.
- If content changes alter event definitions, version/pin the content needed to resume deterministically or explicitly invalidate cross-version replay; do not promise deterministic output across different game/content builds.
- Save schema cap/retention policy and exact number of slots need chat 00 approval.

## Stable invariants in persisted payload

1. Every visitor, document, project, event instance, ledger entry and command has a unique stable ID in its relevant namespace. Cross-reference IDs resolve.
2. Money reconciliation: `balance = opening balance + signed sum of committed unique ledger entries`, with overflow checked. Additional balance accounts require explicitly documented reconciliation per account.
3. No completed day/command/award/effect appears as unprocessed and eligible to execute again.
4. Clock/phase and deadlines form a legal state; `AdvanceDay` cannot have partial stage effects stored as a committed checkpoint.
5. Future event queue ordering and cooldowns/seen flags preserved. Same state + same commands + same definitions produce same future outcomes.
6. Negotiation session and pending visitor/document state can be resumed exactly; no invisible extra round/offer generated on load.
7. RNG algorithm and full internal state preserved, or an independently reproducible per-system stream contract is documented.
8. Optional presentation state is separate from authoritative domain state. Scene objects/UI windows are reconstructed without changing command results.

## Required tests (link to docs/QA_TEST_PLAN.md)

```text
SaveLoad_ComplexSession_RoundTrips            QA-001 / QA-015
SaveLoad_CanonicalPayload_IsStable           QA-002
Load_RetryAppliedAdvanceDay_DoesNotRepeat     QA-003 / QA-017
SaveLoad_LedgerReconciles                     QA-004 / QA-014
Load_ReplayMatchesUninterruptedRun            QA-006 / QA-007
Load_PendingEffect_ExecutesExactlyOnce        QA-008 / QA-013
Load_CorruptPrimary_UsesValidBackup          QA-009
Load_OldSchema_ExecutesMigrationChain         QA-010
Load_FutureSchema_RejectsWithoutMutation     QA-010
Load_ActiveNegotiation_RestoresRoundState     QA-011
Save_AtEveryIOFailpoint_KeepsValidCopy        QA-012
Load_InvalidSnapshot_NeverMutatesSession      QA-016
SaveLoad_CrossReferences_PreserveIdentity     QA-018
```

**Oracle:** semantic/canonical domain-state equality excluding envelope timestamp/diagnostic fields; compare PRNG state, queue order, applied execution IDs, ledger and future outcomes. A “no exception” assertion alone does not qualify.

## Decisions requested from 00 / 01 before runtime

- Gate C0 GameState fields/IDs, DTO rules and immutable snapshot boundary.
- Final `AdvanceDay` order and stable idempotency keys for processed days/commands/effects/ledger.
- Money unit, negative balance policy, overflow strategy and ledger reconciliation convention.
- PRNG algorithm/state shape and stable content-version policy.
- Serializer/package policy and canonical hashing format.
- Platform save/backup replacement and user-facing recovery UX.
- Version 1 schema freeze and migrations/fixture ownership.

**Handoff:** docs/handoffs/qa.md. No implementation code, files or automated test results exist yet.
