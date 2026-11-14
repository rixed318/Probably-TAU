# Handoff — ЧАТ 05 / QA + Persistence

**Date:** 2026-11-14  
**Owner:** ЧАТ 05 (QA engineer, persistence and independent PR reviewer)  
**Issues:** #6 (Save/Load/migration/regression), #8 (CI/PR review); dependencies #2 Core, #3 Economy, #4 NPC.  
**Branch:** `feature/save-6` (created from `main` at `d673322cd9d0d366a27a57b81056504dfb26c731`)  
**PR:** see GitHub PR associated with this branch (documentation-only draft until Gate C0).

## Investigated source of truth

Read README.md, AGENTS.md, docs/GDD.md, docs/ARCHITECTURE.md, docs/MECHANICS.md, docs/CHAT_WORKFLOW.md, docs/DECISIONS.md, Issues #6, #8; additionally checked Issues #2 and #3, main, branches, open PRs, and recent commits. At audit: no Unity project, Core runtime or open PR, and only `main` branch existed. GitHub access includes push permission.

## Created in branch

- `docs/QA_TEST_PLAN.md`: priorities, 18 P0 cases + 16 P1 cases, GameState/economy/NPC/events/negotiation/persistence invariants, deterministic test harness, CI strategy and merge gates.
- `docs/SAVE_SPEC.md`: proposed envelope and DTO boundaries, atomic temp/backup/commit flow, safe load/migration/recovery, RNG/ledger/queue preservation and test oracles.
- `docs/handoffs/qa.md`: this durable context for chat 00 and other chats.

**What is NOT done:** no SaveService/LoadService code, no executable tests, no Unity build, no CI pipeline, no migrations, no finished save schema. No tests have run. No branches merged. Do not treat these proposals as approved Core contracts.

## Key assertions / critical scenarios

- Repeated AdvanceDay, economic commands and delayed effects must not double-apply even across a restart.
- Balance must reconcile with unique ledger entries in one approved integer denomination, with checked arithmetic.
- Equal logical state + PRNG state + content build + commands must give equal future simulation.
- Save -> Load -> Save must preserve a canonical logical payload; wall-clock metadata is excluded.
- Corrupt primary with valid backup must recover. Injected failure at each write boundary must preserve a valid snapshot.
- Future schema must be rejected without changing active runtime state. Known old schema migrates only through explicit tests.
- Active negotiation, pending visitors/events, document observations and project progress survive a load.

## Gate C0 dependencies / coordination requests

1. **Core (#2):** GameState model and stable instance IDs; commands/results/events; transaction boundary; durable command/process-day/effect IDs; GameClock and phase; PRNG state/algorithm.
2. **Economy (#3):** ledger conventions/opening balance, signed integer units, overdraft rules and exactly-once obligations.
3. **NPC (#4):** visitor instance state, deterministic queue/order, negotiation round state, delayed consequence IDs, content references.
4. **Coordinator (00):** approve one authoritative AdvanceDay ordering (GDD §Г6 differs from docs/ARCHITECTURE.md), schema/version policy, migration/backward compatibility, CI/license strategy and test merge gates.
5. **Content (#7):** stable definition identifiers, content version or migration policy for retired/renamed definitions.

## Review protocol / reproducible checks

- Each new PR: read diff, linked Issue, handoff, changed contract signatures, test results, possible behavior changes and migration impact; reproduce P0 cases before APPROVE.
- For defects: leave a precise PR review comment or open GitHub Issue with observed/expected, reproduction, affected commit, severity and suggested owner. No speculative “tests green” reports.
- For missing dependencies: verdict BLOCKED, link the C0 decision needed. No unilateral merge; chat 00 coordinates and requires user approval.

## Implementation order (once C0 is approved)

1. Confirm schemaVersion 1, snapshot capture/restore contract, serializer and storage port; get coordinator review before coding.
2. Implement versioned DTO, validation and SaveService/LoadService only in owned Infrastructure path; no Core redefinition.
3. Add in-memory fault-store and EditMode tests for QA-001…QA-018; commit compatibility fixture snapshots.
4. Add filesystem temp/backup/recovery tests on Windows; test active dialogues and long campaigns.
5. CI integration after real Unity project and approved licensing; publish test results in PR.
6. Handoff refreshed with actual branch SHA, files, test commands, evidence and unresolved bugs; request independent review.
