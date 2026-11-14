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

## 2026-11-14 — AutoDev 05 pass: independent PCG32 oracle (Issue #6, #8)

**One completed QA increment:** `Tests/QA/test_pcg32_contract.py` committed in `feature/save-6` at `0f265fae866a7052c3c09b737339f37258410342`. Pure-Python oracle for PCG-XSH-RR 64/32 (PCG32-v1), published 42/54 golden vector, 256-value Save→Load replay, bounded draws and invalid arguments. Optional `--core-source` checks expected C# algorithm patterns, not C# execution.

**Actual local execution:** `python /mnt/data/qa/test_pcg32_contract.py` → 4 tests, **3 passed, 1 skipped** (source path omitted). Separately, the optional C# source-pattern test was run against a **locally reconstructed representative excerpt**, giving 4/4 passed; **this is NOT proof that GitHub's actual C# source passes**. Before merging, run `python Tests/QA/test_pcg32_contract.py --core-source Runtime/Core/Pcg32.cs` against an actual checked-out PR #9 tree, then run C# golden tests and Unity tests when toolchains exist. Source-pattern tests do not prove C# semantic correctness.

**Current cross-PR state inspected:** main docs still pre-production; PR #9 Core `19669332657972246b6ac815dc24697e2d5bc4f4`, #11 Content, #12 UI (GitHub mergeable=false, reason unverified), #14 ADR-C0-001, #15 scheduler, #16 NPC. All remain draft/open at review. The ADR is approved for feature development only, not merged to main. No SaveService, GameState DTO, or Unity Editor verified.

### Сводка для координатора 00

- **PR/heads:** #9 Core `196693326579`; #10 QA (new QA commit `0f265fae866a`, followed by this handoff); #11 Content (see PR for current head); #12 UI (mergeable=false); #14 ADR `ebdc4be28afd`; #15 Ops `313d6294b499`; #16 NPC `6725e1851876`. For accurate future heads, query GitHub immediately before integration.
- **Blocker 1:** No confirmed complete GameState DTO/CommandProcessor/transaction snapshot boundary; cannot implement real SaveService safely.
- **Blocker 2:** No confirmed Unity Editor / C# compiler / automated Unity CI; C# and Unity tests remain **NOT RUN**.
- **Blocker 3:** PR #12 reports `mergeable=false`; coordinator should inspect actual conflicts/mergeability before any integration, without force merges.
- **Recommended next step:** Core 01 deliver stable snapshot DTO + executable C# golden tests; then QA 05 can implement serializer/checksum/fault-storage tests in its own branch. Meanwhile check PR #12 merge conflicts and run offline QA oracle against a real checkout of PR #9.

**Safety:** No changes to main, no merges, no closing Issues. No new shared Core contract approved by QA.


## 2026-11-14 — AutoDev 05: offline CI gate (#8)

**One completed increment:** created `.github/workflows/qa-offline.yml` in `feature/save-6`, commit `7769480da2158e566acb116120c1a475905f638b`. Triggered on pushes to this branch, relevant PR file changes and manual dispatch; read-only contents permission, checkout credentials not persisted, 5-minute timeout. Python syntax + independent PCG32 oracle run; C# source-pattern audit is conditional on the *actual file existing in the checked-out tree*. It does not borrow code from PR #9 automatically and cannot establish C# correctness. No secrets, Unity or paid runner configured.

**Execution evidence:** GitHub connector successfully fetched real `Tests/QA/test_pcg32_contract.py` from feature/save-6 (blob `01872ad83f3accd9df1fb697010d5c1b01181026`), real `Runtime/Core/Pcg32.cs` from feature/core-2 (blob `9831bc6dcd53896fc8020856c01c862c75bdd508`), and real UI test file from feature/ui-5 (blob `7c91a3067fc0dac461f151c42fc5eea806f7856c`). A real local git checkout failed due to DNS `Could not resolve host: github.com`. **CI result NOT YET VERIFIED; local tests NOT RUN in this pass; Unity/C# tests NOT RUN.** Existing earlier oracle result remains 3 passed / 1 skipped on standalone Python; it is not a new run. UI test is outside this QA branch and not run by this workflow.

### Сводка для координатора 00 — обновление

- **Known PRs:** #9 Core, #10 QA, #11 Content, #12 UI, #14 ADR, #16 NPC remain open at this pass; exact current PR head SHAs must be fetched before integration. New QA CI commit `7769480da2158e566acb116120c1a475905f638b` (handoff update follows). Core PCG32 and UI mock tests were verified as real GitHub source files, not reconstructed snippets.
- **Blocker 1:** Stable Core GameState/snapshot DTO and transaction API not confirmed; SaveService remains deferred.
- **Blocker 2:** Local GitHub checkout blocked by DNS; CI run status and C#/Unity execution not verified. New CI only validates Python oracle, not game compilation.
- **Blocker 3:** QA and Core/UI sources are in separate feature branches; this CI cannot audit PR #9 C# or PR #12 Node tests until code is present in the same tested tree / a safe integration checkout.
- **Recommended next step:** verify GitHub Actions job result on PR #10 and run Node smoke tests on PR #12; after Core snapshot DTO, add Save→Load tests. Do not merge without owner approval.


## 2026-11-14 — AutoDev 05: read-only cross-branch offline CI (#8)

**One bounded task:** extend the existing QA GitHub Actions workflow to run reproducible Node/Python regressions against the *actual* UI and Content feature branches, without merging or modifying those branches. This is a test harness change only, not a SaveService or Unity integration.

- **Workflow commit:** `6651357b6fbb5b5d605343e21190e1a12590d7b5` in `feature/save-6`.
- **File:** `.github/workflows/qa-offline.yml`.
- **New job:** `cross-branch-offline`, read-only checkout of `feature/ui-5` and `feature/content-7` to distinct directories; `persist-credentials: false`, `permissions: contents: read`, timeout 10 minutes. Logs the actual UI/Content checkout commit SHAs.
- **Commands:** `node --test prototypes/ui-day-one/test-fixtures.cjs`, `python3 tools/art/validate_manifest.py`, `python3 -m unittest discover -s tools/art -p 'test_validate_manifest.py' -v`, `node Content/validate-event-hooks.cjs`, `node Content/validate-content-consistency.cjs`. They execute inside the respective checked-out branch directory. This job tests **branch-local** data and validators, not the combined game or C#.
- **Execution evidence at handoff time:** GitHub accepted the workflow update; querying workflow runs for commit `6651357b6fbb5b5d605343e21190e1a12590d7b5` returned no runs yet. **New job NOT YET VERIFIED; all five new commands NOT RUN/NO LOGS CONFIRMED**. Previous QA Python-only run `43619170218` was `success`, but that is *not* evidence for this job. C#/Unity Editor tests NOT RUN.
- **Core snapshot review:** PR #9 `GameStateSnapshotDto` currently contains a minimal read-only slice, not ledger entries, payable records, command fingerprints/outcomes or typed event instances. Do not implement final SaveService/SaveEnvelope serialization against it.
- **Risks:** workflow references moving feature branch heads; commit SHA logging provides traceability but not pinning. If the job fails, inspect actual Actions logs and correct only QA-owned workflow, or file exact issue/comment to subsystem owner. No extra permissions, dependencies or writes to other branches.

### Сводка для координатора 00 — 2026-11-14 22:43 UTC+4

- **Latest PR heads checked:** #9 Core `b39b5084add9`; #10 QA before this update `c4a85eed94cf` (new workflow commit `6651357b6fbb`, this handoff follows); #11 Content `e7c6ad6dc268`; #12 UI `9bd08f297a57`; #14 ADR `ebdc4be28afd`; #17 ADR-C0-002 `bc9db999b419`. Other open PRs: #15 operations, #16 NPC. `main` remains unchanged by QA.
- **Blocker 1:** no complete stable GameState with ledger/receipts/payables/typed scheduled instances; final SaveService and replay tests deferred.
- **Blocker 2:** new cross-branch workflow result not yet verified; no claim of green Node/art/content checks.
- **Blocker 3:** Unity Editor and real C# compilation still unverified; Python/Node static tests cannot establish integration correctness.
- **Recommended next step:** fetch new Actions run and job logs, record exact UI/Content revisions and test outcomes; if green, add independent review of C# runner once Core publishes it, then complete snapshot contract before SaveService.
- **Safety:** no merge, no main writes, no changes to Core/UI/Content branches; PR #10 remains Draft.


## 2026-11-15 — AutoDev 05: real Core source audit in QA CI (#8)

**One bounded task:** remove a misleading always-skipped Core PCG32 source-pattern check from the QA workflow. Previously, the QA branch did not contain `Runtime/Core/Pcg32.cs`, so a green job silently skipped the only Core-specific audit. This is independent static QA, not a C# compile or a SaveService.

- **QA workflow commit:** `be4048e4856c438ff4ecb82cb82a8dba24da0a74` on `feature/save-6`.
- **Change:** `pcg32-oracle` now checks out actual `feature/core-2` read-only into `core-source/`, logs the exact Core HEAD SHA, and invokes `python3 Tests/QA/test_pcg32_contract.py --core-source core-source/Runtime/Core/Pcg32.cs` unconditionally. A missing Core file or failed pattern audit now fails the job, not a false-green SKIP. `contents: read`, no persisted credentials, no new dependencies, no writes to Core.
- **Existing cross-branch CI evidence (verified this run):** GitHub Actions run [43628217751](https://github.com/rixed318/Probably-TAU/actions/runs/43628217751) completed **success** at 2026-11-14 18:43 UTC. Both jobs green. Actual checked-out revisions: UI `9bd08f297a57e820867f3eab40092016978d96af`, Content `e7c6ad6dc2686e27b86773a5c86eb5628aaa2140`. UI Node 8/8 pass; art manifest 85 planned assets validated; art Python unittest 8/8 pass; typed event hooks 5/5 checks; content consistency 5/5 checks. Independent Python PCG32 3 passed / 1 skipped (optional Core audit skipped in **previous** workflow). These results are for earlier checked-out revisions, not today's newer UI/Content heads.
- **New workflow:** commit accepted by GitHub; its new Core audit result must be queried separately and is **NOT VERIFIED** at this handoff. No C# compilation/Unity Editor run is claimed from QA.
- **Review:** Core PR #9 now has standalone .NET 8 runner project and `.github/workflows/core-dotnet.yml`; independently inspect its Actions result, do not infer success from file presence. `GameStateSnapshotDto` remains a minimal DTO missing ledger, payables, fingerprints/outcomes, typed event instances. SaveService remains deferred.

### Сводка для координатора 00 — 2026-11-15 00:20 UTC+4

- **Latest PR heads observed before QA write:** #9 Core `0a64cc4a6a4f`, #10 QA `8ec3059b4ef0` (plus QA workflow `be4048e4856c` and this handoff commit), #11 Content `d934c51506a7`, #12 UI `7ce30ca965e9`, #14 C0 ADR `ebdc4be28afd`, #16 NPC `8611e493d7c7`, #17 ADR-C0-002 `bc9db999b419`. PRs #15 operations and all listed PRs remain open. `main` is pre-production; QA did not change it.
- **Blocker 1:** full stable Core GameState, ledger, receipts, payables and typed event queue are not available for real SaveService and Save→Load replay.
- **Blocker 2:** the new QA Core source audit and independent Core .NET workflow need fresh CI result verification; previous successful QA run did **not** compile C#.
- **Blocker 3:** Unity Editor / EditMode integration tests remain **NOT RUN**.
- **Recommended next step:** inspect new QA run (real Core source audit) and Core's own .NET CI logs; triage actual failures, then add targeted Save/Load tests only when Core publishes a complete snapshot contract.
- **Safety:** PR #10 remains Draft. No merge, no `main` writes, no other branch mutation or shared architecture approval.
