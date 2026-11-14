# Probably-TAU — QA test plan (draft v0.1)

**Owner:** ЧАТ 05 (QA / Persistence)  
**Issues:** #6, #8 (dependencies: #2, #3, #4)  
**Status:** design-only; no Unity project, no executable test suite yet.  
**Baseline when drafted:** main @ d673322cd9d0d366a27a57b81056504dfb26c731 (2026-11-14).  
**Scope:** Core invariants, command execution, economics, events/NPC/negotiations, versioned persistence, integration, regression, CI.  
**Not in scope:** choosing gameplay balance, changing Core contracts unilaterally, runtime implementation before Gate C0, merging PRs.

## Test levels and merge gates

- **P0 / blocking:** money loss or duplication, game-state corruption, non-determinism, duplicate consequences, unrecoverable saves, invalid migration, broken build. When a feature exists, its P0 tests must pass before merge.
- **P1 / required before vertical slice:** robustness of queues and deadlines, content references, scenario flows, UX recoverability, longevity of campaign.
- **P2 / nightly or later:** load/performance, stress/randomized coverage, unusual platform faults and presentation polish.
- **Unit (EditMode):** C# Core independent of Unity scene, deterministic fixtures. Only integrate after contracts are approved.
- **Integration (EditMode):** real CommandProcessor + economy + event queue + fake/in-memory persistence; actual JSON and temporary filesystem tests.
- **PlayMode / Windows smoke:** save/load with UI, scene reload, active panels/dialogues, Alt-Tab and restart.
- **Simulation soak:** 30 days per run; 1,000 seeded runs across at least 3 strategies when a 30-day campaign exists. Detect crashes, impossible states, dominant strategies, unwanted loops. Balance thresholds require chat 00 sign-off.

## P0 — critical executable acceptance cases

| ID | Subsystem | Given / action | Expected |
| --- | --- | --- | --- |
| QA-001 | Persistence | Valid state with visitor queue, active document, ledger, flags, events, projects and RNG -> Save -> Load | Full logical state equals source state; no lost IDs or progress |
| QA-002 | Persistence | Save -> Load -> Save without gameplay commands | Canonical logical payload unchanged (ignore envelope timestamp/checksum); hashes of canonical payload match |
| QA-003 | Clock/commands | Invoke the same identified AdvanceDay command twice and retry after Load | Date and end-day effects advance at most once for that command/day |
| QA-004 | Economy | Apply income/expense sequence | Closing balance = opening balance + sum of signed unique ledger entries; long integer units only |
| QA-005 | Economy | Operation rejected (insufficient funds, wrong phase, invalid target, overflowing amount) | No partial mutation to balance, ledger, resource ownership, metrics or event state |
| QA-006 | Determinism | Equal state, PRNG state, content version and same ordered command stream | Same command outcomes, domain state, IDs, event order and final RNG state |
| QA-007 | RNG/Save | Save mid-campaign, Load, then run same commands as uninterrupted control | Same future visitors, negotiation outcomes, events and ledger |
| QA-008 | Events | Delayed effect due today; process day and restart/load/retry | Effect happens once; pending item consumed with durable execution marker |
| QA-009 | Corruption | Truncated/bad-checksum primary save; intact backup | Primary rejected; backup validated and loaded without modifying good backup |
| QA-010 | Versioning | Supported old schema -> load; unknown future schema -> load | Migration succeeds losslessly for defined fields; future version rejected clearly without overwrite |
| QA-011 | Negotiation | Save in active multi-round negotiation | Offer, round, limits, hidden flags, stable participant IDs and RNG restored |
| QA-012 | Storage | Inject failures at temp write, flush, verification, backup and final replace | Old committed save or new valid save remains readable; no acceptance of partial file |
| QA-013 | Commands | Retry already-applied decision/award with same durable command ID, including after Load | Exactly-once state effects and ledger entries; no duplicated reward |
| QA-014 | Ledger | Duplicate transaction ID or inconsistent balance in loaded snapshot | No silent acceptance; reject or explicitly repair via approved migration, never mint money |
| QA-015 | Persistence | Save at command boundary while visitor/event queue and deadlines are pending | Order, priority, cooldown, flags and deadline counters unchanged after Load |
| QA-016 | Validation | Missing required payload field, invalid ID, illegal enum, out-of-range count, 64-bit money overflow | Structured load error; never mutate live GameState or erase previous good save |
| QA-017 | Day end | Reload at an autosave checkpoint and continue to next end day | Paid obligations and project ticks are not duplicated or skipped |
| QA-018 | References | Two instances refer to the same definition, then save/load | References preserve intended IDs and instance identities; no accidental aliasing |

## P1 — vertical slice regression matrix

| ID | Coverage | Expected |
| --- | --- | --- |
| QA-019 | Deadline expiration, clarification and approval on deadline day | Eligibility follows one documented phase/order rule |
| QA-020 | Negotiation: accept, reject, timeout, walk-away | Correct terminal status; no later extra command or fee |
| QA-021 | Events: one-shot, mutually exclusive, cooldown, per-day quotas | Generated events respect all filters both before and after Load |
| QA-022 | Priority: mandatory story -> urgent -> weighted random | Stable order, no accidental displacement |
| QA-023 | Queue mutation: dequeue visitor, archive document, reopen evidence | No missing/duplicate IDs or stale UI command targets |
| QA-024 | Project: 2 parallel projects, deadline, upkeep and completion | One progress tick/obligation each eligible day |
| QA-025 | Reputation/quality boundary values and extreme effects | Approved clamping or validation; no out-of-range scores |
| QA-026 | Difficulty profile, negative-money and bankruptcy rules | Data-driven policy honored, no hidden hard-coded override |
| QA-027 | Inventory ownership/transfer and rejected move | Single owner/location; failed transfer leaves both unchanged |
| QA-028 | Manual save while dialogue and panels are open | Safe game-state checkpoint; UI restored/reconstructed without relying on scene references |
| QA-029 | Missing content definition / renamed ID | Controlled compatibility handling per agreed content-migration policy |
| QA-030 | Multiple save slots, backups and recovery after failed save | Correct isolation and no cross-slot deletion |
| QA-031 | New install / empty save directory / read-only directory | New game works; save reports actionable error and does not crash |
| QA-032 | 10-day vertical slice deterministic reference run | Logged state trajectory equals committed expected fixture |
| QA-033 | End-day pipeline and campaign terminal states | Correct report, single outcome, no extra actions after terminal state |
| QA-034 | Windows process restart, scene switch, window resize / Alt-Tab | No lost committed progress; state/UI sync after resume |

## Invariants to enforce (proposed, subject to Gate C0)

**GameState:** every mutable instance has unique stable ID; references resolve to known instances/definitions or produce a documented error; phase/date advance only via validated command; command execution is atomic; applied command IDs retained for idempotency; all persisted collections have defined ordering where order affects gameplay. A failed command leaves a deep snapshot unchanged.

**Economy:** monetary amounts are signed 64-bit integer units with checked arithmetic; balance changes only with LedgerService transaction; unique ledger entry ID; no half-posted operation; sum of signed entries reconciles to opening balance. Negative balance is permitted only if explicitly configured by the approved difficulty policy. Obligations/awards require durable once-only IDs for period and source.

**NPC/events:** visitor instance IDs stable across saves; generation uses explicit reproducible PRNG state; mutually exclusive/one-shot/cooldown constraints hold; scheduled effects have durable IDs and execution status; a consumed event cannot fire again after load; no RNG consumption during serialization, validation or purely visual operations.

**Negotiations:** no concurrent conflicting session for the same interaction unless explicitly permitted; only valid round transitions; at most one terminal resolution; offers/limits/RNG/hidden variables survive save; failed counter-offers do not charge money or consume turns unless the documented rule says so.

**Persistence:** snapshot is isolated from later mutable runtime changes; all required state incl. ledger, applied command IDs, random state, event queue and active dialogue is preserved; schema version validated before deserialization into live state; no partial live-state activation; unknown future versions never auto-convert; failed saves never overwrite the sole known-good snapshot; no references to Unity scene objects serialized.

## Test harness design (not yet implemented)

1. Deterministic baseline fixture with fixed world date, seed/PRNG state, one visitor, one negotiation, one pending event, two obligations, two projects and one recorded ledger transaction.
2. Two identical simulations: uninterrupted **A** and save/load-resumed **B**. Compare canonical domain snapshots and ordered event/ledger traces at each safe checkpoint.
3. Fault-injectable `IFileStore`/storage abstraction: fail at each write/flush/rename/checksum boundary without touching user saves.
4. Property-based / seeded cases: command retry is idempotent; ledger reconciles; queue has no duplicate IDs; serialized state has no non-finite or non-serializable data.
5. File compatibility fixtures: committed v1 snapshot, migrated v2 snapshot when supported, corrupted primary + valid backup, future version and bad checksum.
6. Trace output must record seed, command IDs, snapshot schema, expected/actual hash, failing day and minimal reproduction steps.

## CI recommendation for Issue #8

**Now (documentation-only):** Markdown/links and repository-secret checks; do not claim a successful Unity build.  
**After Unity project lands:** pinned Unity 6 LTS editor version from `unity/ProjectSettings/ProjectVersion.txt`; compile/import and EditMode tests on PR, PlayMode smoke on PR where feasible, Windows player build after integration. Validate asmdef dependencies. Save/Load integration tests run with isolated temporary dirs.
**License constraint:** automated Unity editor runs require a properly configured, authorized license method. Do not commit licenses, credentials or passwords. If runner/license is unavailable, mark checks as **NOT RUN**, not green. Prefer free, auditable workflows, seek coordinator approval before external actions/secrets.
**Nightly:** 1,000 deterministic 30-day simulations once implemented; log and persist only reproducible seeds and reports. Branch-protection required checks selected by chat 00 after infrastructure exists.

## Release / PR review checklist

- [ ] Diff only touches owned area; changes to shared contracts approved by chat 00.
- [ ] Clear intent, related Issue, test list, reproducible steps and handoff.
- [ ] Unity compiles with committed editor version; EditMode suites pass.
- [ ] P0 invariants and Save/Load/backup/migration tests pass for affected features.
- [ ] No UnityEngine dependency introduced into Core; no direct UI mutation to GameState.
- [ ] No float money or silent integer overflow; no duplicated ledger/event side effects.
- [ ] Deterministic snapshots verified, including post-load continuation.
- [ ] Failed jobs reproduced/fixed; no ignored regression marked “flaky” without issue.
- [ ] No secrets/licensing files, no fake Unity structure or unintended SDKs.
- [ ] Reviewer status: **APPROVE / REQUEST CHANGES / BLOCKED**, with links to evidence. Only chat 00 coordinates merge with owner's permission.

## Contract conflicts requiring resolution before exact assertions

1. **AdvanceDay order:** GDD §Г6 processes projects before weekly payments and checks goals after generating events; docs/ARCHITECTURE.md uses a different order. Define one durable atomic pipeline with a checkpoint boundary.
2. **Metrics/reputation model:** GDD proposes five reputation constituencies, MECHANICS minimum four, and vertical slice says three. Agree an extensible state representation and actual V-slice set.
3. **Money denomination / overdraft:** long is proposed; choose fixed minor unit, opening-balance convention, and negative-balance/difficulty policy.
4. **Idempotency strategy:** define command IDs, processed-day IDs, ledger transaction IDs and scheduled-event execution IDs (and retention period).
5. **Snapshot compatibility:** assign first schemaVersion, content definition IDs/version, migration ownership, canonical JSON/checksum scheme, and handling of missing definitions.
6. **RNG:** finalize algorithm, state serialization and whether each subsystem has its own stream.

References: README.md, AGENTS.md, docs/GDD.md, docs/MECHANICS.md, docs/ARCHITECTURE.md, docs/DECISIONS.md, docs/CHAT_WORKFLOW.md, Issues #2, #3, #6, #8.
