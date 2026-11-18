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


### QA 05 — additional verified Core CI finding (2026-11-15)

Independent Core .NET 8 CI [run 43630164245](https://github.com/rixed318/Probably-TAU/actions/runs/43630164245) for PR #9 HEAD `0a64cc4a6a4f` **FAILED after successful C# compilation**. GameClock golden suite PASS; PCG32 and GameStateSnapshot suites FAIL with `System.OverflowException` in `Runtime/Core/Pcg32.cs:48`. The explicit `(uint)` narrowing of ulong in XSH-RR likely overflows under the standalone runner's checked arithmetic setting; algorithm requires an intentional unchecked 32-bit truncation, without relaxing checked financial arithmetic. Exact evidence and suggested scoped fix sent to Core PR #9 [QA comment](https://github.com/rixed318/Probably-TAU/pull/9#issuecomment-6883654163). **Core owner must fix/test in own branch; QA did not edit Core.**

**Updated coordinator 00 blocker scoreboard:** (1) full domain GameState/ledger/receipts/payables not yet available for SaveService; (2) verified Core C# PCG32 checked-overflow regression in PR #9 (2/3 suites fail), and newly added QA Core-source audit still awaiting its own run; (3) Unity Editor/EditMode NOT RUN. **Next:** Core 01 fixes checked XSH-RR narrowing and reruns real .NET golden suites; QA 05 checks new QA Actions run and records exact pass/fail. All PRs remain Draft; no merge/main changes.


## 2026-11-15 — AutoDev 05: PCG32 checked-narrowing regression guard (#8)

**One bounded QA change:** added an explicit source-level regression assertion to the independent Python PCG32 oracle. The previous pattern audit matched the XSH-RR formula but did not require the crucial `unchecked((uint)...)` narrowing that caused actual checked .NET 8 Core CI failure [run 43630164245](https://github.com/rixed318/Probably-TAU/actions/runs/43630164245). This new guard prevents a QA green check when that exact protective cast is removed in future Core revisions.

- **Commit:** `5f1622e4f74385c488c9ca6eb689e78354e3fc91` in `feature/save-6`, file `Tests/QA/test_pcg32_contract.py` only; no edits to Core or other role-owned files.
- **Evidence before this change:** Core PR #9 latest head `ea91a3cd2bff` has the actual `unchecked((uint)...)` fix. Core .NET CI [run 43638320521](https://github.com/rixed318/Probably-TAU/actions/runs/43638320521) completed **success**. QA CI [run 43637058120](https://github.com/rixed318/Probably-TAU/actions/runs/43637058120) completed **success**, with both Python/Core-source-audit and UI/Content jobs green. These runs precede the new QA test commit; do not count them as proof the new guard passed.
- **New test status at handoff write:** GitHub accepted the source commit; fresh QA workflow run for `5f1622e4` was not yet returned by the commit-run lookup. **New assertion NOT YET VERIFIED by CI**. It is a source-pattern guard, not a C# compile, Unity EditMode test or complete Save→Load replay.
- **Next:** verify the QA workflow on the updated head; if failed, inspect the actual log and fix only the QA test. SaveService remains deferred until Core publishes full ledger, command receipts, payables and typed event queue.

### Сводка для координатора 00 — 2026-11-15 01:58 UTC+4

- **PR heads checked:** #9 Core `ea91a3cd2bff`; #10 QA before write `abcc92eb38fe`, new QA test `5f1622e4f743` and this handoff commit; #11 Content `4babe0023251`; #12 UI `581e178a83bd`; #14 ADR `ebdc4be28afd`; #17 ADR-C0-002 `bc9db999b419`. PRs #15/#16 also remain open; `main` unchanged by QA.
- **Blocker 1:** complete stable Core domain snapshot with ledger, command fingerprint/outcome receipts, payables and typed scheduled instances is still missing; real SaveService/Save→Load replay cannot be integrated.
- **Blocker 2:** the new QA checked-narrowing guard needs its own completed CI evidence; older green runs do not prove it.
- **Blocker 3:** Unity Editor/EditMode and integrated gameplay tests remain **NOT RUN**.
- **Recommended next step:** confirm new QA workflow success, then independently test the next Core Ledger/transaction slice and prepare persistence adapters only after the full DTO contract.
- **Safety:** QA-only branch, no merge, no main mutation, no architecture approval or third-party IP authorization.


### Post-handoff CI verification (same QA task)
GitHub Actions [run 43645584031](https://github.com/rixed318/Probably-TAU/actions/runs/43645584031), triggered by QA test commit `5f1622e4f74385c488c9ca6eb689e78354e3fc91`, subsequently reached **completed / success**. This is now real CI evidence that the new checked-narrowing source assertion passed against the Core branch checked out by the QA workflow; it does **not** prove Unity Editor or integration tests. Earlier 'NOT YET VERIFIED' statement above records the state at the initial handoff write, not the final result.


## 2026-11-15 — AutoDev 05: independent Core Ledger .NET QA gate (#8)

**One bounded QA task:** added a read-only `core-ledger-regression` job to the existing QA offline workflow. The job checks out the actual `feature/core-2` into `core/`, prints its full Git SHA, installs .NET 8, and runs `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`. Core's current runner explicitly calls **four** suites including `LedgerGoldenTests.RunAll()`, which checks checked Int64 arithmetic, posting uniqueness, overdraft rejection and immutability. This is independent cross-branch regression coverage, **not** a Unity Editor build, SaveService or merged integration test.

- **QA-owned workflow commit:** `eb7cd6c84f6321c06607cc82e7a0b36a49b75476` in `feature/save-6`; changed `.github/workflows/qa-offline.yml` only.
- **Evidence:** actual Core source `Runtime/Core/LedgerSnapshot.cs`, `Tests/Core/LedgerGoldenTests.cs` and runner `Tests/Core/Standalone/Program.cs` inspected via GitHub. PR #9 Core HEAD `7844ab6ace3f55ec778d92c2431554fa28702109` at inspection. QA branch prior HEAD `18d31596b13e6de3649f10b412afd6458c73b7ea`.
- **Execution:** GitHub accepted the workflow commit. Immediate commit-workflow lookup returned **zero runs**, so the new job result is **NOT VERIFIED**. Local `dotnet` absent; `git ls-remote` failed with DNS `Could not resolve host: github.com`. Consequently no new C# test execution is claimed from this run. Earlier Core CI green results are not proof of this newly added QA job. Unity Editor/EditMode: **NOT RUN**.
- **Scope/safety:** no changes to Core source, no alternative GameState, no dependencies or secrets in QA code, `contents: read`, checkout `persist-credentials: false`, no `main` writes or merges. PR #10 remains Draft.

### Сводка для координатора 00 — 2026-11-15 03:35 UTC+4

- **Latest PR heads observed:** #9 Core `7844ab6ace3f`, #10 QA `18d31596b13e` before new QA workflow `eb7cd6c84f63` and this handoff, #11 Content `fe29d7617149`, #12 UI `003acfde7ab6`, #14 C0 ADR `ebdc4be28afd`, #17 ADR-C0-002 `bc9db999b419`. #15 and #16 remain open; all checked PRs Draft, no merges. `main` README/AGENTS/CHAT_WORKFLOW reviewed; QA did not modify main.
- **Blocker 1:** complete stable Core GameState snapshot with ledger integration, command fingerprint/outcome receipts, payables and typed scheduled instances is still absent; real SaveService and Save→Load replay remain deferred.
- **Blocker 2:** new QA .NET ledger regression workflow needs a completed Actions run; no green claim before log verification.
- **Blocker 3:** Unity Editor / EditMode and integrated gameplay tests remain **NOT RUN**.
- **Recommended next step:** inspect QA workflow run triggered by `eb7cd6c84f63` and record actual Core SHA and four-suite pass/fail. If green, request Core 01 complete transaction/snapshot contracts before persistence implementation.


## 2026-11-15 — AutoDev 05: verified first Core Ledger QA CI result (#8)

**One bounded QA task:** independently verify the previously queued GitHub Actions run **43654064605** against the real QA workflow in `feature/save-6`, without changing Core/UI/Content. [Run evidence](https://github.com/rixed318/Probably-TAU/actions/runs/43654064605). The run completed **SUCCESS** on 2026-11-14 23:36 UTC. All three jobs finished **success**:
- `core-ledger-regression`: real Core branch read-only checkout, .NET 8 SDK setup and `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`. **Actual job logs:** `PASS: PCG32 golden vectors and replay`; `PASS: GameClock five-day calendar`; `PASS: GameState snapshot invariants`; `PASS: Ledger checked Int64 and append invariants`; `PASS: all 4 Core golden suites.`
- `cross-branch-offline`: actual UI mock Node, art manifest and art unit tests, Content hooks and consistency validation steps all **success**.
- `pcg32-oracle`: Python reference and real Core C# source-pattern audit steps both **success** (pattern audit is not C# execution; separate Core job above is).
- **Unity Editor/EditMode: NOT RUN**. Cross-branch jobs verify branch-local code, not Unity compilation or integrated game behavior. Workflow records actual checkout revisions in logs; this report does not imply all current moving branch heads have been retested after this run.
- No new source-code changes in this bounded verification pass; handoff updated solely to persist verified evidence.

### Сводка для координатора 00 — 2026-11-15

- **PR heads at audit:** #9 Core `17bcec4c7a51`; #10 QA `3a5c322e37c3` before this handoff commit; #11 Content `3568105339d3`; #12 UI `0dd483e18ddf`; #14 ADR `ebdc4be28afd`; #17 ADR `bc9db999b419`. All these PRs open/draft. Main remains unmodified by QA.
- **Blocker 1:** No confirmed complete persisted Core DTO including command fingerprints/receipts, ledger/payables, typed scheduled event instances, so final SaveService and migration are deferred.
- **Blocker 2:** No Unity Editor/EditMode compile and integrated end-to-end verification. A successful standalone .NET 8 runner does not replace this.
- **Blocker 3:** Cross-branch QA uses moving feature refs, not a pinned multi-branch integration snapshot; exact checked-out SHAs must be retained before claiming integration readiness.
- **Next step:** independently review new Core Ledger implementation/DTO completeness and add focused persistence contract tests once stable Core fields exist; meanwhile retain the four-suite .NET CI gate. Do not merge without owner authorization.


## 2026-11-15 — AutoDev 05: fifth financial-command suite enforced in QA CI (#8)

**One bounded change:** QA `core-ledger-regression` job now requires the actual Core runner output to contain both `PASS: Financial command atomic prepare/commit and replay` and `PASS: all 5 Core golden suites.` It uses `set -euo pipefail`, `tee`, and exact-line `grep -Fx`; a future Core refactor that silently drops the fifth suite will fail this QA gate. No Core source, other role branch, shared contract or Unity project was edited.

- **QA workflow commit:** `011d20dcbd2866d1a5649680c07880569c32ca0d` on `feature/save-6`; file `.github/workflows/qa-offline.yml` only.
- **Prior real evidence:** Core PR #9 HEAD `47cd9edbc43e89c239a3529ce8e2365af3be71ed`; Core .NET 8 CI [run 43664046080](https://github.com/rixed318/Probably-TAU/actions/runs/43664046080) **completed success** and job logs explicitly show all **five** suites PASS including `Financial command atomic prepare/commit and replay`. Earlier QA [run 43662824705](https://github.com/rixed318/Probably-TAU/actions/runs/43662824705) **completed success** but checked out older Core with only **four** suites. Neither older run proves the new QA guard itself.
- **New QA workflow status:** code committed; fresh CI result for this exact change **NOT VERIFIED at handoff write**. No local .NET or Unity Editor execution claimed; Unity Editor/EditMode **NOT RUN**.
- **Implementation scope:** current Core `GameStateSnapshotDto` remains a minimal slice without ledger entries, command fingerprints/outcomes, payables or typed scheduled event instances. Ledger and financial-command unit tests are not yet an integrated persistence API; SaveService is still deferred.

### Сводка для координатора 00 — 2026-11-15 07:02 UTC+4

- **PR heads checked before QA write:** #9 Core `47cd9edbc43e`, #10 QA `8b8116ca7b72`, #11 Content `b4aa7b8096c7`, #12 UI `7ecd54714ff9`, #14 ADR `ebdc4be28afd`, #17 ADR-C0-002 `bc9db999b419`; #15 and #16 also open. QA branch gained `011d20dcbd28` plus this handoff update. No merge or main mutation.
- **Blocker 1:** complete, stable persistable GameState including command receipts, ledger/payables and typed scheduled instances still missing; SaveService/Save→Load integration blocked.
- **Blocker 2:** fresh QA CI run of the new fifth-suite guard needs verification (older QA run only tested four suites).
- **Blocker 3:** Unity Editor/EditMode and integrated gameplay/save-load tests remain **NOT RUN**.
- **Recommended next step:** check the fresh QA workflow run; if green, independently review Core transaction receipts and event instance DTO readiness before implementing persistence. Keep exact Core SHA and CI evidence. PR #10 remains Draft; no main writes, merges, contract approvals or IP/logo approvals.


## 2026-11-15 08:40 UTC+4 — AutoDev 05: verify fifth-suite QA gate (#8)

**One bounded QA task:** verified the previously unverified fifth-suite guard against the actual completed GitHub Actions run [43670550752](https://github.com/rixed318/Probably-TAU/actions/runs/43670550752) associated with QA HEAD `c200472d03fe306f094f43efc650815a2591061d`. GitHub returned **completed / success**. Inspected the workflow run's three jobs and individual step conclusions:
- `Core ledger and snapshot .NET 8 regression (not Unity)`: **success**; `Install .NET 8 SDK` and `Compile and execute Core golden suites including Ledger` both **success**. The current QA workflow uses `set -euo pipefail`, `tee`, and exact-line `grep -Fx` for `PASS: Financial command atomic prepare/commit and replay` and `PASS: all 5 Core golden suites.`; a successful step therefore verifies that the fifth-suite guard was satisfied. No claim is made about unseen per-test stdout beyond those required lines.
- `UI and Content static regression (not integration)`: **success**; UI Node, art manifest, art unit tests, content hooks and cross-file consistency steps all **success**.
- `Python PCG32 oracle (not C# or Unity)`: **success**; Python syntax, reference test and Core source-pattern audit steps all **success**.

**Scope:** independent moving-branch CI, not a pinned integration build. The run's checkout SHA values are emitted in the job logs but were not retrieved in this pass; current PR heads can differ from revisions tested by the earlier run. No new tests authored or source code changed; this is a verified CI evidence and handoff task. Unity Editor/EditMode **NOT RUN**. SaveService remains deferred until full stable persistable Core snapshot DTO.

### Сводка для координатора 00 — 2026-11-15 08:40 UTC+4

- **PR heads inspected:** #9 Core `ecc2ae5faef5`, #10 QA `c200472d03fe` before this documentation commit, #11 Content `70744e3dddc1`, #12 UI `b76e8f30c59a`, #14 ADR `ebdc4be28afd`, #17 ADR-C0-002 `bc9db999b419`; all these PRs remain open/Draft. `main` README, AGENTS.md, docs/CHAT_WORKFLOW.md and DECISIONS.md read; Issues #6/#8 reviewed. QA did not modify main.
- **Blocker 1:** Core still lacks a confirmed complete persistable domain snapshot with ledger, command fingerprints/outcome receipts, payables and typed scheduled event instances; do not start production SaveService or claim Save→Load replay.
- **Blocker 2:** Unity Editor/EditMode and integrated game/save-load tests **NOT RUN**.
- **Blocker 3:** cross-branch QA follows moving feature refs; passing this run does not prove all latest PR heads jointly integrate.
- **Recommended next step:** audit Core #9 snapshot/receipt fields against Save schema and prepare a narrow persistence contract test once fields exist; retain QA's fifth-suite gate and pin source SHAs before integration claims.
- **Safety:** documentation-only QA feature branch, no merge, no main writes, no architectural or third-party IP approvals.


## 2026-11-15 10:22 UTC+4 — QA 05: current CI + persistence-readiness audit (#6, #8)

**One bounded independent QA task:** inspect latest actual PR heads and GitHub Actions job results, then review the *real* Core `Runtime/Core/GameStateSnapshotDto.cs` against Save/Load prerequisites. No production SaveService code is attempted while the contract is incomplete.

- **QA HEAD before this audit:** `415b8a4b5b6cf8aa8458b936b74e5fa8ee53e51d`. Its GitHub Actions [run 43677859489](https://github.com/rixed318/Probably-TAU/actions/runs/43677859489) is **completed / success**; independently fetched all three jobs: Python PCG32 oracle **success**, UI/Content static regression **success**, Core .NET 8 ledger/snapshot regression **success**. Job step summaries report no failures. This is CI evidence, **not** a local test execution and **not** Unity integration.
- **Core HEAD:** `19e9bb10f536206f6742a9e94e92ded4c163c391`. Core's separate .NET 8 [run 43678924467](https://github.com/rixed318/Probably-TAU/actions/runs/43678924467) is **completed / success**; its compilation/golden-runner job is **success**. It is not a Unity Editor/EditMode run.
- **Actual DTO audit:** `GameStateSnapshotDto.cs` blob `3d64a71bfd13829a116e3fd2cdd9043482083d55` exposes `Revision`, campaign identifiers/seed, `DayIndex`, Q/S/M/E/R metrics, opening/current cash, named RNG streams, processed command/day IDs and pending/completed **event ID lists**. Its own source comment explicitly states it is a minimal slice, not final persistence wire format. It **does not expose** persisted ledger postings, payables/debt, command fingerprints/outcome receipts or typed scheduled event payloads with priority/due dates. The presence of independent Ledger and FinancialCommand golden suites does **not** fill these snapshot gaps.
- **Verdict:** CI regression gate **PASS** for the tested branch checkouts; production SaveService and Save→Load exactly-once replay **BLOCKED by incomplete Core snapshot contract**. Unity Editor/EditMode **NOT RUN**. No newly authored tests in this documentation-only audit.

### Сводка для координатора 00 — 2026-11-15 10:22 UTC+4

- **Current PR heads checked:** #9 Core `19e9bb10f536`, #10 QA `415b8a4b5b6c` before this handoff commit, #11 Content `7f8d0ac4cd5f`, #12 UI `cb3267291577`, #14 C0 ADR `ebdc4be28afd`, #16 NPC `3e76929289d4`, #17 ADR-C0-002 `bc9db999b419`. All inspected PRs open and Draft; #15 also exists. Main policy documents AGENTS.md and docs/CHAT_WORKFLOW.md re-read.
- **Blocker 1:** complete persistable Core snapshot (ledger, payables, command fingerprint/outcome receipts, typed event instances and due metadata) still absent.
- **Blocker 2:** Unity Editor/EditMode and real end-to-end Save→Load integration **NOT RUN**.
- **Blocker 3:** QA CI checks moving feature-branch refs; its success does not prove simultaneous integration of all current PR heads at pinned SHAs.
- **Recommended next step:** ask Core 01 to add a versioned **snapshot-only** extension for ledger/payables/receipts/event instances with deterministic round-trip golden tests, under the already-approved C0 and ADR-C0-002 rules; QA 05 can then implement isolated serialization/replay regression without inventing GameState or modifying Core.
- **Safety:** no merge, no main writes, no edits to other role branches, no new shared contracts, no IP/logo approvals.


## 2026-11-15 — AutoDev 05: independent regression triage, Core FinancialCommand (#6/#8)

**One completed bounded task:** independently inspect real GitHub Actions results and diagnose a newly failing Core golden test. No Core code modified; no SaveService attempted. Read current `main` AGENTS/CHAT_WORKFLOW, PR #9/#10/#11/#12/#14/#16/#17, Issues #6/#8 and current QA workflow. QA PR #10 prior HEAD `761e8c3a7aae`; Core PR #9 HEAD `dfe85e3d330a`.

**Verified CI evidence:**
- QA run [43685184318](https://github.com/rixed318/Probably-TAU/actions/runs/43685184318) for QA HEAD `761e8c3a7aae`: **success, 3/3 jobs** (Python PCG32 oracle/source audit, cross-branch UI/Content static regression, Core Ledger/Snapshot .NET job). This is branch-local evidence at the Core SHA fetched by that run, **not** proof for newer Core HEAD.
- Core run [43686861960](https://github.com/rixed318/Probably-TAU/actions/runs/43686861960) for Core HEAD `dfe85e3d330a`: **failure**. .NET compilation succeeded; PCG32, GameClock, GameStateSnapshot, Ledger suites PASS; FinancialCommand suite FAIL at `Tests/Core/FinancialCommandGoldenTests.cs:164`, assertion `encodingRejected`.
- Inspected actual source `Tests/Core/FinancialCommandGoldenTests.cs` lines 158–174: line 160 defines `string malformed = "bad\\uD800";` (two backslashes in C# source), creating literal backslash-u text rather than an unpaired UTF-16 surrogate. This is the **likely test-fixture defect**, not yet a confirmed production encoding defect. Proposed single-backslash escaped surrogate or `(char)0xD800` and rerun CI; do not relax strict encoding requirements.
- Precise QA report sent to [Core PR #9 comment](https://github.com/rixed318/Probably-TAU/pull/9#issuecomment-6891400294).
- No local C#/Node/Python tests executed in this pass; no new test added. Unity Editor/EditMode **NOT RUN**.

### Сводка для координатора 00 — 2026-11-15 12:00 UTC+4

- **Current PR HEADs:** #9 Core `dfe85e3d330a` (CI failed), #10 QA `761e8c3a7aae` before this handoff update (QA CI green), #11 Content `2f443045ccfb`, #12 UI `4a05a4a8a105`, #14 ADR `ebdc4be28afd`, #16 NPC `3f13fea10deb`, #17 ADR-C0-002 `bc9db999b419`. All inspected PRs Draft/open.
- **Blocker 1:** Core FinancialCommand golden suite fails at line 164 on newer HEAD; likely invalid malformed UTF-16 test fixture. Fix in Core branch, rerun Core .NET CI.
- **Blocker 2:** `GameStateSnapshotDto` is still a minimal slice; full ledger, payables, command fingerprints/outcomes and typed scheduled instances are not integrated into one durable snapshot. SaveService/Save→Load replay blocked.
- **Blocker 3:** Unity Editor/EditMode and integrated gameplay build NOT RUN.
- **Recommended next step:** Core 01 fixes malformed surrogate fixture, verifies all five suites on current HEAD; QA 05 checks resulting logs, then continues targeted financial receipts snapshot/replay test design once stable DTOs exist.
- **Safety:** no `main` writes, no merge, no changes to foreign feature branches or architecture contracts.


## 2026-11-15 — AutoDev 05: independent verification of Core UTF-16 regression fix (#6/#8)

**One bounded task:** independently verified that Core 01 replaced the malformed-surrogate fixture with `"bad" + (char)0xD800` in the real `feature/core-2` file `Tests/Core/FinancialCommandGoldenTests.cs`. This repairs the earlier test-fixture error without weakening production strict UTF-8 validation.

**Fresh CI evidence:** [Core Actions run #43694164162](https://github.com/rixed318/Probably-TAU/actions/runs/43694164162) was inspected through GitHub Actions jobs: `completed / success`; the job `Compile Core and run three golden suites (.NET 8; not Unity)` and its `Compile and run all Core golden suites` step both report `success`. This is evidence of that workflow's successful .NET test step, **not** a Unity compile or Unity EditMode run. Local C#/Node/Python tests were **NOT RUN** in this pass. The earlier failure [#43686861960](https://github.com/rixed318/Probably-TAU/actions/runs/43686861960) remains historical evidence only.

### Сводка для координатора 00 — 15.11.2026, новая проверка

- **PRs:** #9 Core (fixture fix `f64d32ac`, handoff `594b2719`); #10 QA (this handoff update follows); #11 Content, #12 UI and #14 ADR remain open/draft at inspection. Obtain exact live branch HEAD SHAs from GitHub before integration. Earlier QA cross-branch CI #43685184318 was green on an older source snapshot; it is not fresh integration evidence.
- **Blocker 1:** Complete versioned GameState DTO still lacks proven durable ledger/obligations, command receipt outcomes and typed delayed-event payloads; SaveService remains BLOCKED until Core contracts are stable.
- **Blocker 2:** Unity Editor / EditMode tests NOT RUN; .NET 8 golden suites do not prove Unity integration.
- **Blocker 3:** No pinned-SHA integrated all-branch build/test evidence; branch-local successes are not merge approval.
- **Recommended next step:** Core 01 complete snapshot DTO and persistence invariants, then QA implement Save→Load round-trip tests in `feature/save-6`. Coordinator 00 should separately assess integration at pinned SHAs.

**Safety:** no changes to Core, main, merges, or shared contracts.


## 2026-11-15 — AutoDev 05: independent verification of newest Core UTF-8 regression (#6/#8)

**One bounded task:** reviewed current main policies, PR #9/#10/#11/#12/#14 (also #16/#17), Issues #6/#8, QA workflow, Core handoff, the real Core `Tests/Core/FinancialCommandGoldenTests.cs` and `GameStateSnapshotDto.cs`. Verified the newest Core financial fingerprint regression coverage (malformed posting/source ID) without modifying Core.

**Evidence:** Core PR #9 HEAD `74ecb8b013beb961dc9ddd3ae7e330a049dfdfe9` has GitHub Actions [run #43701790337](https://github.com/rixed318/Probably-TAU/actions/runs/43701790337) **completed / success**. Independently fetched the job and steps: checkout, .NET 8 SDK install and `Compile and run all Core golden suites` all **success**. The actual source contains tests for malformed posting ID (strict UTF-8 fingerprint throws) and malformed source ID (`ExecuteVerified` returns `InvalidCommandEncoding` with unchanged state). This verifies the workflow step, **not** a separately inspected per-assertion log or Unity build. Local Node/Python/C# tests **NOT RUN** in this documentation-only verification. Unity Editor/EditMode **NOT RUN**.

**Snapshot readiness:** the current `Runtime/Core/GameStateSnapshotDto.cs` blob `3d64a71bfd13829a116e3fd2cdd9043482083d55` remains a minimal slice, explicitly excluding persisted ledger entries, debts, receipt fingerprints/outcomes and typed delayed-event payloads. Production SaveService **BLOCKED**; no substitute GameState model created.

### Сводка для координатора 00 — 2026-11-15 15:21 UTC+4

- **Latest checked PR HEADs before this QA commit:** #9 Core `74ecb8b013be`; #10 QA `4a5fdd3bf953`; #11 Content `aa19c995d988`; #12 UI `4357cdd3b2e1`; #14 ADR `ebdc4be28afd`; #16 NPC `ddaf4a0c142a`; #17 ADR-C0-002 `bc9db999b419`. All seven checked PRs open/draft. Main policy docs unchanged in this task.
- **Blocker 1:** complete durable GameState snapshot DTO still missing ledger, obligations, verified command receipts and typed pending events; SaveService cannot be implemented safely.
- **Blocker 2:** Unity Editor/EditMode/PlayMode and integrated Save→Load **NOT RUN**.
- **Blocker 3:** no pinned-SHA integrated multi-branch Unity build; independent feature-branch CI success is not merge approval.
- **Recommended next step:** Core 01 should extend the Core-owned versioned snapshot with ledger/obligations/receipt outcomes and typed scheduled event instances plus deterministic round-trip tests. QA 05 can then build Save→Load tests without redefining Core. For new QA CI evidence, record actual checked-out Core/UI/Content SHAs.
- **Safety:** only QA handoff updated; no main write, merge, third-party IP approval or shared architecture decision.


## 2026-11-15 — AutoDev 05: extend read-only UI Day 01 regression (#8)

**One bounded change:** QA workflow now executes two existing UI branch Node suites in addition to the earlier mock fixture suite: `node --test prototypes/ui-day-one/test-day01-read-model.cjs` and `node --test prototypes/ui-day-one/test-day01-preview.cjs`. Verified both files exist in `feature/ui-5` before editing. Read-only checkout with `persist-credentials: false` retained. No UI, Core, Content or main files modified. Workflow commit: `f16ffc4cad9484b8f71fdfc9642aede50a4db5d7`.

**Execution evidence:** GitHub connector returned no pull-request workflow runs yet for workflow commit `f16ffc4`. Therefore the **new two suites are NOT YET VERIFIED PASS**. No local Node/Python/C# execution in this pass. Unity Editor/EditMode/PlayMode **NOT RUN**. Existing historical CI successes are not evidence for this new change. Check Actions for the new QA branch commit and record actual UI/Content/Core checkout SHAs and job logs before calling it green.

### Сводка для координатора 00 — 2026-11-15 16:58 UTC+4

- **Latest checked PR heads:** #9 Core `3a726cac4819`; #10 QA `f16ffc4cad94` (before this handoff update); #11 Content `ad4ca530bc82`; #12 UI `c572e152752d`; #14 ADR `ebdc4be28afd`; #16 NPC `ff121fe4b582`; #17 ADR-C0-002 `bc9db999b419`. All checked PRs open/Draft. C0-v2 and ADRs are for feature implementation only, not merge permission.
- **Blocker 1:** Newly extended QA workflow has no confirmed Actions run/log yet; UI read-model and preview suites remain **NOT VERIFIED** in QA CI.
- **Blocker 2:** Complete durable GameState DTO still lacks confirmed ledger/payables, command receipts and typed pending-event snapshots; real SaveService **BLOCKED**.
- **Blocker 3:** Unity Editor/EditMode/PlayMode and pinned-SHA integrated Save→Load **NOT RUN**.
- **Recommended next step:** inspect the next QA Actions run, confirm all jobs and Node test output for both newly added UI suites, capture the checkout SHAs; then continue snapshot completeness audit when Core 01 updates DTO.
- **Safety:** no merge, no main writes, no foreign branch edits, no new common contracts or third-party IP approvals.


## 2026-11-15 — AutoDev 05: verify new UI Day 01 QA suites (#8)

**One bounded task:** verify the two newly added UI canonical Node regression suites in the actual QA cross-branch workflow, including raw GitHub Actions job logs. No source/test workflow changes in this pass.

**New evidence:** [QA Actions #43716587348](https://github.com/rixed318/Probably-TAU/actions/runs/43716587348), completed **success**, all three jobs **success**. Inspected raw logs of UI/Content job `132819357210`: mock Node **8/8 PASS**, canonical Day 01 read-model Node **7/7 PASS**, canonical Day 01 preview Node **11/11 PASS**; art validator Python unittest **8/8 PASS**; manifest validation and both Content Node validation steps completed successfully. The workflow read-only checkout logs pin UI `c572e152752d3a9fdd20b4daab0b7138175b706e` and Content `ad4ca530bc826bb9c69d16f996e91ba1769c8f45`. These are **historical checked-out revisions**, not current branch HEADs. Core .NET and PCG32 jobs also succeeded in this run; no Unity compile is implied. Local Node/Python/C# **NOT RUN**. Unity Editor/EditMode/PlayMode **NOT RUN**.

### Сводка для координатора 00 — 2026-11-15, 18:40 UTC+4

- **Current PR HEADs inspected:** #9 Core `3f55e0428f45`; #10 QA `146a203a33e9` before this handoff commit; #11 Content `853af9d288a3`; #12 UI `bfd8513966e2`; #14 ADR `ebdc4be28afd`; #16 NPC `9ffbf1ff7bbf`; #17 ADR `bc9db999b419`. All open/Draft. QA Actions #43716587348 passed against earlier pinned UI/Content revisions; it does **not** establish success of their newer HEADs.
- **Blocker 1:** Complete Core-owned durable GameState DTO (ledger/payables, verified command receipts, typed pending event payloads) is not confirmed; real SaveService **BLOCKED**.
- **Blocker 2:** Unity Editor/EditMode/PlayMode and end-to-end Save→Load **NOT RUN**.
- **Blocker 3:** No pinned-SHA integrated test of the *current* Core/UI/Content/NPC HEADs; cross-branch green run is not merge approval.
- **Recommended next step:** obtain fresh CI logs on current QA commit and current dependency HEADs; request stable full Core snapshot DTO before Save→Load implementation. Coordinator 00 retains all architecture and merge approvals.
- **Safety:** no main writes, merges, foreign-branch edits, new shared contracts or IP approvals.


## 2026-11-15 20:15 UTC+4 — QA 05: independent verification of newer cross-branch CI (#8)

**One bounded task:** audited completed [QA Actions run #43725665784](https://github.com/rixed318/Probably-TAU/actions/runs/43725665784) for QA PR #10 head `ccd01c528b65c6073d1483b04513bdfce3b3014a`. Fetched all three job statuses and raw job logs. **All 3 jobs SUCCESS**. No code or workflow change in this pass; this is evidence verification only.

- **Actual checked-out SHAs from raw logs:** Core `3f55e0428f455925039d1d7707db15b6c065c366`; UI `bfd8513966e297ac233861972ff67bcd1b887627`; Content `853af9d288a3f8d5ca274be75bab80a3bb08bef3`.
- **Core .NET 8 job:** compile/run step SUCCESS; raw stdout contains PASS for PCG32, GameClock, GameStateSnapshot, Ledger, FinancialCommand and `PASS: all 5 Core golden suites.` This is pure C#, **not Unity**.
- **UI/Content job:** mock Node, canonical Day 01 read-model and preview, art manifest and art unit tests, Content event hooks and consistency steps all SUCCESS. Raw log contains Content event-hook `PASS: 9 deterministic fixture/negative checks (including 4 ADR metadata drift checks)` and cross-file `PASS: 29 cross-file fixture/negative checks`. Python oracle/source audit job SUCCESS.
- **Important limitation:** tested Core/UI/Content SHAs are **older than current PR heads**. The run does not establish that the current heads are green or that the branches integrate. No local execution; Unity Editor/EditMode **NOT RUN**. SaveService still deferred pending complete durable Core DTO.

### Сводка для координатора 00 — 2026-11-15 20:15 UTC+4

- **Latest PR heads observed:** #9 Core `9f2f3b3a77e9`, #10 QA `ccd01c528b65` before this handoff commit, #11 Content `61213269154f`, #12 UI `6b6f0c76e63b`, #14 ADR `ebdc4be28afd`, #16 NPC `2f93378766ae`, #17 ADR `bc9db999b419`. All checked PRs open/draft. No main mutation or merge.
- **Blocker 1:** durable GameState snapshot still not established complete (ledger, obligations, command receipts/fingerprints, typed scheduled instances), so SaveService/Save→Load remains BLOCKED.
- **Blocker 2:** cross-branch CI verified older moving-ref SHAs, not latest Core/UI/Content PR heads; pinned integrated verification missing.
- **Blocker 3:** Unity Editor/EditMode and full gameplay integration tests NOT RUN.
- **Recommended next step:** obtain CI evidence on latest Core/UI/Content commits with recorded checkout SHAs, then re-audit Core persistence DTO completeness before implementing SaveService. Keep PR #10 Draft; no contract approvals, main writes or merges.


## 2026-11-15 22:04 UTC+4 — QA 05: audit current Core snapshot and latest available QA run (#6/#8)

**One bounded task:** independently re-read the Core-owned `Runtime/Core/GameStateSnapshotDto.cs` at PR #9 head `e6f6f6f1c2b0af079faf5c0dfce1ef9e1e422419` (blob `3d64a71bfd13829a116e3fd2cdd9043482083d55`) and verify the newest QA workflow run returned for the previous QA commit. No Core code or shared contracts changed.

**Specific persistence finding:** `GameStateSnapshotDto` exposes campaign/revision/day/metrics/balances, RNG streams, processed command/day **IDs**, and pending/completed event **IDs**. Its own source comment explicitly excludes receipt fingerprints/outcomes, ledger entries, debts and typed NPC state. Event IDs alone do not preserve due-time/payload/processing status; processed command IDs alone cannot reproduce verified command receipt outcomes. Thus a production Save→Load round-trip and exactly-once restoration are **BLOCKED** pending Core-owned durable DTO expansion. This is a source review, not a newly executed test.

**Additional CI evidence:** [QA Actions #43734567100](https://github.com/rixed318/Probably-TAU/actions/runs/43734567100) for QA commit `8605ea1e873c3623f92206dac889ba9a134d0429` completed SUCCESS (3/3 jobs); independently inspected all three raw job logs. Actual checkout SHAs: Core `9f2f3b3a77e9fcbffbcc4f45a6327ba2892ca530`, UI `6b6f0c76e63b1d61d868f7afecd996d73373ff9b`, Content `61213269154ffae8e1abf1c628ba785676da9f1e`. Core log prints five PASS suites (PCG32, GameClock, GameStateSnapshot, Ledger, FinancialCommand); Content prints 9 event-hook and 31 cross-file checks PASS; UI/Art steps and Python oracle job succeeded. These are **older SHAs than current PR heads**, not latest-head/integrated Unity evidence. Local Node/Python/C# **NOT RUN** in this pass; Unity Editor/EditMode/PlayMode **NOT RUN**.

### Сводка для координатора 00 — 2026-11-15 22:04 UTC+4

- **Latest inspected PR heads:** #9 Core `e6f6f6f1c2b0`, #10 QA `8605ea1e873c` (before this handoff commit), #11 Content `a3986bd043b0`, #12 UI `9e4600289e12`, #14 ADR `ebdc4be28afd`, #16 NPC `654526511c92`, #17 ADR `bc9db999b419`. All open/Draft. C0-v2/ADR feature-development guidance remains non-merge authority.
- **Blocker 1:** Core-owned durable snapshot missing ledger, obligations, receipt fingerprints/outcomes and typed delayed-event/NPC state; SaveService remains BLOCKED.
- **Blocker 2:** verified CI run #43734567100 checked out older moving-ref Core/UI/Content revisions, not current PR heads.
- **Blocker 3:** Unity Editor/EditMode/PlayMode and integrated Save→Load NOT RUN.
- **Recommended next step:** ask Core 01 for stable full durable DTO with deterministic round-trip tests; rerun QA on current dependency SHAs and capture exact checkout hashes before any integration claim. Keep PR #10 Draft.
- **Safety:** only QA handoff updated, no merge/main writes/foreign branch edits or new shared contracts.


## 2026-11-15 23:41 UTC+4 — QA 05: Content money-wire gate (#8)

**One bounded code change:** QA workflow `.github/workflows/qa-offline.yml` now invokes `node Content/validate-money-wire.cjs` in the existing read-only `feature/content-7` checkout, following event-hook and cross-file validation. QA workflow commit: `7e4583d6740d7f91975a03d1b732387003578888` in `feature/save-6`. Source validator was verified present in Content branch (blob `bba7df10212ebad99c26810d21c42cbd9f08f256`). No Content source edited, no new shared contracts.

**Execution evidence:** GitHub accepted workflow commit. New money-wire step is **NOT YET VERIFIED** by Actions log; local Node/.NET/Python **NOT RUN**. Existing QA Actions #43744752193 3/3 success predates this new step and must **not** be cited as its result. The workflow still checks out moving Core/UI/Content refs; actual SHAs are logged but not frozen. Read-only NPC .NET runner and pin/freeze mode are separate next tasks; do not imply either is implemented. Unity Editor/EditMode/PlayMode **NOT RUN**.

### Сводка для координатора 00 — 2026-11-15 23:41 UTC+4

- **Latest PR references:** #9 Core, #10 QA (workflow commit `7e4583d6740d7f91975a03d1b732387003578888` plus this handoff), #11 Content, #12 UI, #14 C0 ADR, #16 NPC; current PR head SHAs must be independently refreshed from GitHub before any release-check. Prior checked Core head `e6f6f6f1c2b0` is historical, not a current-head assertion. `main` untouched.
- **Blocker 1:** complete durable Core GameState snapshot (ledger, obligations, command receipts, typed events/NPC) still not confirmed; SaveService deferred.
- **Blocker 2:** new Content money-wire QA step has no completed CI evidence; moving checkout refs still lack pinned/fail-closed release mode and NPC runner.
- **Blocker 3:** Unity Editor/EditMode/PlayMode and integrated Save→Load **NOT RUN**.
- **Recommended next step:** inspect next QA Actions job log and record exact Core/UI/Content SHA + money-wire PASS/FAIL. Then add pinned-SHA release-check or read-only NPC runner as one separately verified QA task.
- **Safety:** no merges, main writes, foreign-branch edits, or IP/architecture approvals.


## 2026-11-16 01:20 UTC+4 — QA 05: read-only NPC .NET runner (Issues #8/#18)

**One bounded code change:** added `npc-conditions-regression` job to `.github/workflows/qa-offline.yml` on QA branch `feature/save-6`, workflow commit `f0b552134f1b91a859fc9832a3901822d7fb00b8`. It checks out `feature/npc-4` read-only (`persist-credentials: false`), prints actual `NPC_SHA`, installs .NET 8 and runs `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. Confirmed the csproj and NPC's own workflow exist at NPC head `89f2f04bce64a5d2817397826e3d2f206fd075d4` before editing. No NPC source changed.

**Execution:** new QA workflow job **NOT VERIFIED** until a completed GitHub Actions run and logs are inspected. No local .NET/Node/Python or Unity Editor/EditMode/PlayMode tests executed in this pass. Existing Content money-wire step remains in workflow, but no new PASS claim is made. Moving Core/UI/Content/NPC refs are still unpinned; this is NOT a release-check or Unity compile. UI standalone `prototypes/ui-day-one/day01-playable.html` exists in UI branch (blob `c33773809f2ed8fc0d4367364681d3ca77a48629`), but offline launch, interaction, and downloadable artifact are **NOT VERIFIED**. Issue #18 status: **BLOCKED / NOT READY_FOR_OWNER_TEST** until exact-SHA playable demo, end-day/financial assertions, offline launch and artifact are evidenced.

### Сводка для координатора 00 — 2026-11-16 01:20 UTC+4

- **PR HEADs checked before QA update:** #9 Core `cf4c9b4be0dc2be727e4e151e325b04c7134d10b`, #10 QA `73107fc4c6aaf48e03c142d06623dd654a733f76`, #11 Content `41558e123b70c5a29bbcbd321e34d05904b52ab0`, #12 UI `cf4de6ed525b25abdce09737d9101b0fc5ff210b`, #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`, #16 NPC `89f2f04bce64a5d2817397826e3d2f206fd075d4`. QA workflow commit `f0b552134f1b91a859fc9832a3901822d7fb00b8`; this handoff commit follows.
- **Blocker 1:** QA new NPC job needs real CI logs; moving refs have no fail-closed pinned-SHA integration mode.
- **Blocker 2:** Issue #18 owner demo offline launch, three visitor choices, EndDay financial invariants and downloadable artifact not independently evidenced.
- **Blocker 3:** Core durable snapshot/Save→Load not confirmed complete; Unity Editor/EditMode/PlayMode NOT RUN.
- **Next step:** inspect CI for QA workflow commit and exact checkout SHAs, then independently verify UI #12 playable artifact/launch and Node gameplay assertions for Issue #18. Do not announce READY_FOR_OWNER_TEST without evidence.
- **Safety:** no changes to main/other branches, no merges, no new architecture contracts or third-party IP approvals.


## 2026-11-16 02:55 UTC+4 — QA 05: pinned cross-branch QA checkout (#18)

**One bounded code change:** commit `5a07a727d46c47999bb723d3896a79996d4548cf` in `feature/save-6` changes only `.github/workflows/qa-offline.yml`: read-only Core/UI/Content/NPC checkouts now use full immutable PR HEAD SHAs instead of moving feature refs, print actual checkout SHA and fail closed on mismatch. Exact reviewed source refs: Core PR #9 `0f5a85ef1136fec0cbc0285568b11985f0e7200c`; UI PR #12 `cc44bf037f2aad91c41718ec2f3f336b7a0cd464`; Content PR #11 `f3d9950e9e607176e28e0af70abc6bb9566555be`; NPC PR #16 `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`. Money-wire and NPC .NET steps remain present. These are frozen integration candidates, **not** automatically current HEAD after future PR pushes.

**Actual evidence this pass:** GitHub accepted workflow source update. CI execution of this new pinned workflow **NOT VERIFIED**; no fresh PASS claims. Local Node/.NET/Python **NOT RUN**. UI HTML `prototypes/ui-day-one/day01-playable.html` verified as GitHub source at pinned UI SHA, but independent offline browser launch, 3-visitor EndDay behavior and downloadable artifact **NOT VERIFIED**. Unity Editor/EditMode/PlayMode and Save→Load **NOT RUN**. Status Issue #18: **BLOCKED; NOT READY_FOR_OWNER_TEST**.

### Сводка для координатора 00 — 2026-11-16 02:55 UTC+4

- **PRs/commits:** #9 Core `0f5a85ef`, #10 QA workflow `5a07a727` (handoff commit follows), #11 Content `f3d9950e`, #12 UI `cc44bf03`, #14 ADR `ebdc4be2`, #16 NPC `d82bc6ee`; all inspected as open/Draft. Main untouched. Issue #18 remains open.
- **Blocker 1:** pinned QA workflow requires a completed CI run and per-job logs before asserting Core/UI/Content/NPC checks pass on frozen refs.
- **Blocker 2:** offline launch and gameplay/finance/EndDay smoke, plus downloadable web-demo artifact, remain unverified; no READY_FOR_OWNER_TEST.
- **Blocker 3:** durable Core GameState snapshot/Save→Load and Unity Editor tests remain unverified.
- **Next step:** inspect Actions for pinned QA SHA and UI demo artifact workflow; independently execute UI Day01 smoke against the exact pinned SHA, verify artifact extraction/offline launch and 3 visitors, choices, EndDay and integer/once-only cash before owner gate. Do not merge or edit foreign branches.


## 2026-11-16 — QA 05 / Issue #18: pinned CI money-wire failure (independent review)

**One bounded task:** inspected completed pinned GitHub Actions run [43772020964](https://github.com/rixed318/Probably-TAU/actions/runs/43772020964) and failing [cross-branch job 132989808026](https://github.com/rixed318/Probably-TAU/actions/runs/43772020964/job/132989808026), plus the exact pinned Content validator source. **Result: 3/4 jobs SUCCESS, 1/4 FAILURE.** Core .NET 8, PCG32 Python, and NPC .NET 8 jobs succeeded; UI Node mock (8), read-model (7), preview (12), art tests (8), Content event hooks (9) and cross-file checks (31) succeeded within the failing cross-branch job. The final `node Content/validate-money-wire.cjs` step FAILED with `AssertionError [ERR_ASSERTION]: expected field-specific diagnostic` at validator line 62, invoked at line 66; Node v24.22.2. No claims of all-green CI.

**Root cause identified by source inspection (not a local reproduction):** `inspect()` constructs indexed diagnostic paths with brackets, e.g. `$.documentTypes[0].visible.amountKopecks`, but `failsAt()` asserts `errors[0].includes(path.join("."))`, expecting `documentTypes.0.visible.amountKopecks`. The first nested negative fixture on line 66 fails because of this formatting mismatch, even though `errors.length === 1` passed. Content owner should make expected paths match actual pointer syntax or assert against a shared formatter, preserving strict field-specific rejection. QA does not edit Content-owned validator.

**Exact pinned checkout refs in workflow:** Core `0f5a85ef1136fec0cbc0285568b11985f0e7200c`; UI `cc44bf037f2aad91c41718ec2f3f336b7a0cd464`; Content `f3d9950e9e607176e28e0af70abc6bb9566555be`; NPC `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`. GitHub job steps confirmed the checkout SHA checks succeeded. Local Node/.NET/Python NOT RUN this pass; Unity Editor/EditMode and Save→Load NOT RUN. No downloadable artifact in this workflow run (artifacts API returned empty); offline browser gameplay, three interactive visitor decisions, EndDay exactly-once and conditional revenue remain NOT VERIFIED. Verdict **BLOCKED / NOT READY_FOR_OWNER_TEST**.

### Сводка для координатора 00 — 2026-11-16

- **PRs:** #9 Core (pinned `0f5a85ef`), #10 QA (this handoff commit follows), #11 Content (pinned `f3d9950e`), #12 UI (pinned `cc44bf03`), #16 NPC (pinned `d82bc6ee`); #14 ADR is not merged. These are *verified CI source refs*, not assertions about current branch HEADs.
- **Blocker 1:** Content money-wire validator's nested-path assertion fails in pinned CI; owner 04 must fix and QA must rerun exact-SHA validation.
- **Blocker 2:** Issue #18 lacks evidenced offline playable smoke and downloadable demo artifact; READY_FOR_OWNER_TEST prohibited.
- **Blocker 3:** Complete durable GameState snapshot/Unity Editor validation unavailable; SaveService and Unity tests NOT RUN.
- **Recommended next step:** Content owner fix the pointer-format assertion; QA repin updated Content SHA, rerun CI and separately verify offline demo artifact + 3 visitor/EndDay finance invariants before any readiness verdict. No merge/main/foreign branch writes.


## 2026-11-16 06:22 UTC+4 — Issue #18: playable gameplay gateway CI gate

**One scoped QA code change:** commit `5c73cc4e6be9280a73e720e90e4bf65adcab17a9` adds `node --test prototypes/ui-day-one/test-day01-gameplay.cjs` to the read-only, SHA-pinned UI checkout in `.github/workflows/qa-offline.yml`. This suite is confirmed to exist at pinned UI `cc44bf037f2aad91c41718ec2f3f336b7a0cd464` (blob `06db14140987d6bccd411856228bd664e99225d9`). Source inspection shows four test cases covering DayIndex 0, three distinct NPCs, 200000000 kopecks, all three decisions, conditional partner cash, divergent inspected choices, EndDay gating and repeated EndDay no-op. **Test file presence and assertions are verified, but this is not an execution result.**

**Execution:** new QA CI run/log NOT VERIFIED at handoff time; local Node/.NET/Python NOT RUN; Unity Editor, offline Windows browser launch and Save→Load NOT RUN. Previous pinned CI [43772020964](https://github.com/rixed318/Probably-TAU/actions/runs/43772020964) had 3/4 successful jobs and Content money-wire diagnostic mismatch; this new step was not part of that run. Artifact download/launch not verified. READY_FOR_OWNER_TEST remains **BLOCKED**.

### Сводка для координатора 00 — 2026-11-16 06:22 UTC+4

- **PR/commits:** #10 QA workflow `5c73cc4e6be9280a73e720e90e4bf65adcab17a9` (this handoff commit follows); #9 Core pinned `0f5a85ef`; #11 Content pinned `f3d9950e`; #12 UI pinned `cc44bf03`; #16 NPC pinned `d82bc6ee`. These are **CI frozen refs**, not asserted latest branch heads. #14 ADR and #17 due-event ADR remain feature-development guidance, not merge approval.
- **Blocker 1:** Content money-wire validator fails at pinned Content SHA (array diagnostic pointer mismatch), so cross-branch job not green.
- **Blocker 2:** New playable gateway test needs a successful exact-SHA CI log; Windows offline launch and downloadable demo artifact have not been independently demonstrated.
- **Blocker 3:** Durable GameState DTO incomplete for SaveService; Unity Editor/EditMode/PlayMode NOT RUN.
- **Recommended next step:** inspect new QA CI run and its gameplay step; ask Content owner to fix validator on their branch, then update pinned Content SHA and re-run. Separately verify artifact and offline browser launch before READY_FOR_OWNER_TEST.
- **Safety:** QA branch only; no main writes, merges, other-branch edits or shared-contract approvals.


## 2026-11-16 — P0 #18: re-pin corrected Content validator (QA 05)

**One bounded change:** QA workflow Content checkout and exact-SHA assertion moved from failing `f3d9950e9e607176e28e0af70abc6bb9566555be` to Content PR #11 `c2d9ba267549c4e1a404fd6fa68a7d1037566597`. Workflow commit: `328640d6dc363fe879a7d970b25c117bff177c3b`. No Core/UI/NPC refs changed; no other subsystem branch modified.

**Observed regression:** QA Actions [#43791893275](https://github.com/rixed318/Probably-TAU/actions/runs/43791893275) at prior QA HEAD `f215ebf57156ffd50e91e213db96e9d7d3b80e58` finished failure: Core, NPC and PCG32 jobs success, UI/Content job failed specifically at `Content canonical money wire validation`. Source review confirms old Content validator compares dotted paths against bracketed array diagnostics; new Content blob `d3f3215f8cf9a9a956ed583626c3e48c640711ed` builds the exact bracketed pointer and uses `startsWith(pointer + ":")`. **New pinned workflow CI NOT YET VERIFIED**; source comparison is not a Node execution.

**UI artifact evidence:** UI PR #12 exact HEAD `8951e40eece975bec8cf731aae8c2746e97364f3` has own Actions [#43789277275](https://github.com/rixed318/Probably-TAU/actions/runs/43789277275) success and `day01-web-demo` artifact ID `14191677502` (8,400 bytes, not expired at inspection). This is a newer UI revision than QA's pinned `cc44bf037f2aad91c41718ec2f3f336b7a0cd464`; it does **not** establish independent gameplay/browser PASS at the newer SHA. Browser offline launch, 3 visitors/choices/EndDay and downloadable ZIP execution **NOT RUN** in this QA pass. Unity Editor/EditMode and Save→Load **NOT RUN**.

### Сводка для координатора 00 — 2026-11-16 08:00 UTC+4

- **Latest inspected PR heads:** #9 Core `e08952448b19`, #10 QA workflow `328640d6dc36` (this handoff commit follows), #11 Content `c2d9ba267549`, #12 UI `8951e40eece9`, #14 ADR `ebdc4be28afd`, #16 NPC `a737f5c1d2ac`. No merge/main changes.
- **Blocker 1:** New QA CI for corrected pinned Content SHA not yet confirmed; previous run #43791893275 failed money-wire at old SHA.
- **Blocker 2:** QA-pinned UI SHA predates current UI #12; exact-current-SHA gameplay/interactive offline launch and independent artifact smoke are missing. `READY_FOR_OWNER_TEST` / `PLAYABLE_WEB_DEMO` **BLOCKED**.
- **Blocker 3:** Durable Core snapshot incomplete for receipt/ledger/typed event replay; Unity Editor and Save→Load not tested.
- **Recommended next step:** fetch the workflow run for QA commit `328640d6dc36`, inspect all four jobs and money-wire logs; then update UI pin to latest verified artifact SHA and execute independent offline browser gameplay smoke. Do not claim ready before all Issue #18 gates are evidenced.


## 2026-11-16 09:40 UTC+4 — P0 #18: verified pinned QA CI results

**One bounded task:** independently inspected completed GitHub Actions [run 43801271386](https://github.com/rixed318/Probably-TAU/actions/runs/43801271386) and [run 43801287579](https://github.com/rixed318/Probably-TAU/actions/runs/43801287579). Both runs have **4/4 jobs SUCCESS**: Python PCG32 oracle, Core .NET 8 standalone, NPC .NET 8 standalone, and cross-branch UI/Content static regression. In run 43801271386, job [133081861718](https://github.com/rixed318/Probably-TAU/actions/runs/43801271386/job/133081861718) confirms all steps success, including the **UI playable Day 01 gameplay gateway Node test** and **Content canonical money wire validation**. This supersedes the older money-wire failure; it is not Unity compilation or an offline browser launch.

**Exact pinned refs in executed workflow:** Core `0f5a85ef1136fec0cbc0285568b11985f0e7200c`, UI `cc44bf037f2aad91c41718ec2f3f336b7a0cd464`, Content `c2d9ba267549c4e1a404fd6fa68a7d1037566597`, NPC `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`. The `Record exact source revisions` step succeeded, including fail-closed SHA assertions. These are pinned **tested refs**, not assertions that feature branches remain at those heads.

**Artifact/offline limitation:** artifacts query for QA run 43801271386 returned **zero artifacts**. A separate UI workflow previously produced `day01-web-demo` artifact for UI `8951e40e`, but that SHA differs from QA-pinned `cc44bf03`. The pinned HTML entrypoint exists; actual downloaded ZIP extraction, Windows offline browser launch, all three visitor interactions and EndDay manually exercised in browser remain **NOT RUN**. No independent READY_FOR_OWNER_TEST claim. Local Node/.NET/Python not run this pass; CI executions only. Unity Editor/EditMode/PlayMode and Save→Load **NOT RUN**.

### Сводка для координатора 00 — 2026-11-16 09:40 UTC+4

- **PRs/commits:** #9 Core (CI pin `0f5a85ef`), #10 QA (handoff commit from this pass), #11 Content (CI pin `c2d9ba26`), #12 UI (CI pin `cc44bf03`; separate artifact SHA `8951e40e`), #14 ADR, #16 NPC (CI pin `d82bc6ee`). All CI refs are exact and not necessarily latest branch HEADs. Issue #18 remains open.
- **Blocker 1:** UI demo artifact is evidenced only for newer UI SHA, while QA regression passed older pinned SHA; no same-SHA artifact + gameplay proof yet.
- **Blocker 2:** Independent offline Windows/browser interactive smoke (3 visitors, decisions, EndDay and once-only finance) remains NOT RUN.
- **Blocker 3:** Durable Core snapshot still not confirmed complete; Unity Editor and Save→Load NOT RUN.
- **Recommended next step:** repin QA workflow to the latest UI artifact-producing SHA after source review, run the same gameplay suite and verify artifact ZIP/extracted offline browser launch on that exact SHA. Keep Issue #18 BLOCKED until the owner-test gate has matching evidence. No merge, no main or foreign-branch writes.


## 2026-11-16 11:27 UTC+4 — P0 #18: align QA UI pin with artifact revision

**Bounded change:** QA cross-branch workflow UI checkout and fail-closed SHA assertion updated from `cc44bf037f2aad91c41718ec2f3f336b7a0cd464` to `8951e40eece975bec8cf731aae8c2746e97364f3`. The UI #12 HEAD at inspection is `8951e40eece975bec8cf731aae8c2746e97364f3`, and UI Actions [#43789277275](https://github.com/rixed318/Probably-TAU/actions/runs/43789277275) has successful artifact `day01-web-demo` ID `14191677502`, size 8400 bytes, SHA-256 `cc90f202a966506357c5b0672ffb05bd82434675ba9093fedcc07d7166d7b138` (artifact metadata only; archive not extracted or executed). The HTML references sibling `day01-read-model.cjs`, `day01-gameplay-gateway.cjs`, and `day01-playable.js`, so a standalone HTML file by itself is insufficient.

**Tests:** UI source and four Node gameplay assertions read at exact SHA; no local Node/browser execution in this pass. Prior QA Actions [#43811815380](https://github.com/rixed318/Probably-TAU/actions/runs/43811815380) succeeded on **older** UI pin; it is NOT evidence for this update. New workflow run must be observed before claiming same-SHA CI pass. Windows offline browser smoke and ZIP extraction NOT RUN; Unity Editor/EditMode and Save→Load NOT RUN. No new shared architecture contracts.

### Сводка для координатора 00 — 2026-11-16 11:27 UTC+4

- **Latest inspected PR HEADs:** #9 Core `2333c93e9606`; #10 QA `ac2bb365caaf` before this change; #11 Content `aef96e038e06`; #12 UI `8951e40eece9`; #16 NPC `c30668d972d1`; #14 ADR open Draft. `main` `d673322cd9d0`. QA workflow retains pinned Core `0f5a85ef`, Content `c2d9ba26`, NPC `d82bc6ee`; UI now `8951e40e`.
- **Blocker 1:** Newly pinned UI gameplay Node suite must PASS in QA Actions on exact `8951e40e` (no results yet at commit preparation).
- **Blocker 2:** Artifact ZIP extraction and real Windows offline browser interactions (3 visitors/choices/EndDay, once-only postings, conditional partner cash) remain NOT RUN; `READY_FOR_OWNER_TEST` BLOCKED.
- **Blocker 3:** Durable Core GameState DTO remains incomplete for persistence; Unity/Save→Load NOT RUN.
- **Next:** inspect new QA Actions jobs and actual checkout SHAs, then independently extract/launch artifact offline and exercise all three decisions and EndDay. No merge/main writes.


## 2026-11-16 14:43 UTC+4 — P0 #18: exact-UI-SHA QA CI evidence

**One bounded verification:** inspected GitHub Actions [run #43824110415](https://github.com/rixed318/Probably-TAU/actions/runs/43824110415) for QA PR #10 at prior HEAD `a0562946dcc5f65f74a7efcc7ebfd0143d9b20da`. All **four jobs completed SUCCESS**: Python PCG32 oracle, NPC standalone .NET 8, UI/Content static regression, and Core ledger/snapshot standalone .NET 8. The workflow file at that QA HEAD pins and asserts exact read-only checkout refs: Core `0f5a85ef1136fec0cbc0285568b11985f0e7200c`, UI `8951e40eece975bec8cf731aae8c2746e97364f3`, Content `c2d9ba267549c4e1a404fd6fa68a7d1037566597`, NPC `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`. Thus the CI job success now covers the **same UI source SHA** as the previously documented UI web-demo artifact. No local tests executed this pass; CI jobs are not browser launch or Unity compilation. Artifacts query for this QA run returned **zero artifacts**, which does not invalidate the separately reported UI artifact but prevents independent ZIP execution proof.

**Issue #18 verdict:** **BLOCKED / NOT READY_FOR_OWNER_TEST**. Actual downloaded artifact extraction and offline Windows browser interaction (three visitors/choices, EndDay, no double postings, conditional partner cash, integer finance) remain **NOT RUN**. Unity Editor/EditMode and Save→Load **NOT RUN**. No Core/NPC/UI/Content branch writes, no merge, no architectural approval.

### Сводка для координатора 00 — 2026-11-16 14:43 UTC+4

- **Latest PR HEADs at inspection:** #9 Core `50066c979b12`; #10 QA `a0562946dcc5` before this handoff commit; #11 Content `a241569d3c7d`; #12 UI `8951e40eece9`; #14 ADR `ebdc4be28afd`; #16 NPC `b2680f76b21f`. All listed PRs remain Draft; tested pinned refs differ from current Core/Content/NPC heads.
- **Blocker 1:** No independent extraction/launch of the UI artifact in offline Windows/browser and interactive end-to-end Day 01 proof. Same-UI-SHA CI static/Node checks are now SUCCESS, not equivalent to browser proof.
- **Blocker 2:** Pinned Core/Content/NPC refs are older than current PR heads; release candidate integration readiness needs re-pin/re-run after review.
- **Blocker 3:** Durable Core GameState snapshot not yet confirmed complete for Save→Load; Unity tests NOT RUN.
- **Recommended next step:** obtain the UI run artifact ZIP and perform a genuinely offline browser smoke test of all 3 visitors/choices/EndDay and finance invariants on UI SHA `8951e40e`; attach concrete evidence to Issue #18 before declaring READY_FOR_OWNER_TEST.


## 2026-11-16 16:33 UTC+4 — P0 #18: offline entrypoint static dependency gate

**One bounded change:** commit `e84deb7499450127fb1922508b225b698feb6fd8` adds a read-only Node step to `.github/workflows/qa-offline.yml` for the pinned UI SHA `8951e40eece975bec8cf731aae8c2746e97364f3`. It checks that `day01-playable.html` references exactly three relative local JavaScript files, that all three exist in the UI checkout, and that the HTML has no obvious remote asset/network references. This is a **static offline dependency closure check**, NOT a browser run, ZIP extraction, full JavaScript network audit, or Windows verification. No Core/NPC/UI/Content source changes.

**Evidence:** GitHub UI artifact ID `14191677502` (run `43789277275`, 8400 bytes, not expired) has the exact pinned UI SHA; prior QA Actions run `43824110415` passed 4/4 jobs at QA SHA `a0562946`, before this new step. The new workflow step has **NOT RUN / CI NOT VERIFIED** at handoff time. Local Node/.NET/Python **NOT RUN** this pass; Unity Editor/EditMode/PlayMode and Save→Load **NOT RUN**. READY_FOR_OWNER_TEST remains **BLOCKED**.

### Сводка для координатора 00 — 2026-11-16 16:33 UTC+4

- **Latest inspected PR HEADs:** #9 Core `dc05ba68b239`, #10 QA `e84deb749945` (handoff commit follows), #11 Content `287387ab08a4`, #12 UI `8951e40eece9`, #14 ADR `ebdc4be28afd`, #16 NPC `b2680f76b21f`. CI pins remain Core `0f5a85ef`, UI `8951e40e`, Content `c2d9ba26`, NPC `d82bc6ee`; do not conflate tested refs with moving PR heads. Main remains `d673322cd9d0`.
- **Blocker 1:** New static offline-closure CI step has no verified run yet; artifact ZIP extraction and actual offline browser interactions (3 visitors, choices, EndDay, once-only postings, conditional partner cash, integer finance) remain NOT RUN.
- **Blocker 2:** Core/Content/NPC pinned refs are older than latest PR heads; integration release-check requires deliberate review and re-pin.
- **Blocker 3:** Durable GameState snapshot contract is not confirmed sufficient for real SaveService; Unity Editor and Save→Load NOT RUN.
- **Recommended next step:** inspect Actions run for QA commit `e84deb74`, then independently download/extract artifact `14191677502` and run offline browser smoke with evidence on exact UI SHA. Keep Issue #18 BLOCKED until that proof exists; no merge/main writes.


## 2026-11-16 19:02 UTC+4 — P0 #18: independent pinned-entrypoint source audit

**One bounded verification:** fetched `prototypes/ui-day-one/day01-playable.html` directly from immutable UI SHA `8951e40eece975bec8cf731aae8c2746e97364f3` (blob `c33773809f2ed8fc0d4367364681d3ca77a48629`). Independently parsed its script references: exactly `./day01-read-model.cjs`, `./day01-gameplay-gateway.cjs`, `./day01-playable.js`; no literal http(s) URLs, fetch(), XMLHttpRequest or WebSocket tokens in HTML. This is a **source-only static inspection**, not proof of ZIP contents, JS dependency closure or offline browser launch. No runtime tests executed this pass (Node/.NET/Python NOT RUN; Unity and Save→Load NOT RUN). PR #10 remains open Draft.

**CI evidence:** previously confirmed run [#43824110415](https://github.com/rixed318/Probably-TAU/actions/runs/43824110415) has four successful jobs, but it predates the offline-entrypoint static step introduced at `e84deb749945`. Commit-associated PR-run lookup for QA HEAD `d07f32e605c0` returned zero records; **the new step remains NOT VERIFIED**, not failed. No new CI success is claimed.

### Сводка для координатора 00 — 2026-11-16 19:02 UTC+4

- **PR heads verified:** #9 Core `f25401e6d055`, #10 QA `d07f32e605c0` before this handoff commit, #11 Content `f92f4da7222a`, #12 UI `8951e40eece9`, #14 ADR `ebdc4be28afd`, #16 NPC `b2680f76b21f`. All are open Draft. Main base `d673322cd9d0`. Tested QA pins remain Core `0f5a85ef`, UI `8951e40e`, Content `c2d9ba26`, NPC `d82bc6ee` — never substitute moving heads for tested SHAs.
- **Blocker 1:** Artifact ZIP extraction and independent offline browser Day01 smoke (3 visitors, decisions, EndDay, once-only posting, conditional partner cash, integer finance) NOT RUN; Issue #18 **BLOCKED**, not READY_FOR_OWNER_TEST.
- **Blocker 2:** CI result for the newly introduced static offline dependency step not verified; latest Core/Content/NPC heads differ from tested pins.
- **Blocker 3:** Durable GameState DTO for real SaveService not established; Unity Editor/Save→Load NOT RUN.
- **Recommended next step:** retrieve UI artifact ID `14191677502`, extract and launch it with networking disabled, execute all Day01 scenarios, capture exact SHA and evidence, then inspect new QA CI before owner-ready claim. No main/foreign-branch write, no merge, no new shared contracts.


## 2026-11-16 20:03 UTC+4 — P0 #18: extracted artifact browser gameplay smoke

**One bounded independent verification (no source change):** downloaded actual GitHub Actions artifact ID `14191677502` (`day01-web-demo`, run `43789277275`, UI source SHA `8951e40eece975bec8cf731aae8c2746e97364f3`), extracted ZIP and verified five files: `HOW_TO_PLAY.txt`, `day01-playable.html`, `day01-read-model.cjs`, `day01-gameplay-gateway.cjs`, `day01-playable.js`. Artifact metadata reports 8400 bytes, SHA256 digest `cc90f202a966506357c5b0672ffb05bd82434675ba9093fedcc07d7166d7b138`, not expired.

**Actual execution:** local Python Playwright + installed Chromium (headless) executed the extracted HTML DOM and injected all three extracted JavaScript files with HTTP(S) routes blocked. Browser verified three distinct queue entries; EndDay initially disabled; selected and accepted DOC-01 full 220,000 RUB, DOC-02 full 65,000 RUB, DOC-03 onsite 40,000 RUB; budget changed 2,000,000 → 1,780,000 → 1,715,000 → **1,675,000 RUB**; EndDay became enabled and produced report with **exactly three distinct ledger keys** (`day0:DOC-01/02/03`), no partner signing revenue, and explicit no-auto-booking note. Browser page errors: none. **PASS for this one path in an isolated Chromium DOM with local scripts and HTTP blocked.** The check did not exercise alternate choices, duplicate decision clicks, integer overflow edges, or a real Windows double-click file launch. Direct `file://` navigation failed with `net::ERR_BLOCKED_BY_ADMINISTRATOR` in this execution environment; therefore **offline file launch on Windows NOT VERIFIED**. This is not an assertion of full artifact launch readiness.

**Additional CI:** GitHub reports QA workflow run [43892508977](https://github.com/rixed318/Probably-TAU/actions/runs/43892508977) `completed/success` associated with QA commit `c892ca13d48563e56a77f823aa74eb7e4df14540`; job-level steps for this run not inspected in this pass. Prior [43824110415](https://github.com/rixed318/Probably-TAU/actions/runs/43824110415) independently verified 4/4 jobs successful. Unity and Save→Load **NOT RUN**. No new shared contracts, no merge.

### Сводка для координатора 00 — 2026-11-16 20:03 UTC+4

- **PR heads checked:** #9 Core `f25401e6d055`, #10 QA `c892ca13d485` before this handoff commit, #11 Content `f92f4da7222a`, #12 UI **`fdfc468264ce`** (newer than tested artifact `8951e40e`), #14 ADR `ebdc4be28afd`, #16 NPC `b2680f76b21f`. CI pins still Core `0f5a85ef`, UI `8951e40e`, Content `c2d9ba26`, NPC `d82bc6ee`.
- **Blocker 1:** Direct `file://` browser launch blocked by sandbox administrator; real extracted-folder Windows/Edge/Chrome offline launch remains unverified. Playwright local-script DOM smoke passed only one three-decision path.
- **Blocker 2:** Current UI PR head `fdfc4682` differs from artifact-tested `8951e40e`; alternate decisions, duplicate posting and new-head artifact regression require evidence before owner-ready claim.
- **Blocker 3:** Durable GameState snapshot contract/Unity Editor/Save→Load still not independently validated.
- **Recommended next step:** owner/QA on Windows extract exact artifact and double-click `day01-playable.html` with networking off; cover alternate decisions and repeated clicks, then pin/retest latest UI SHA and capture screenshots/logs. **Issue #18 BLOCKED; READY_FOR_OWNER_TEST not declared.**


## 2026-11-16 21:17 UTC+4 — P0 #18: independent QA CI step-level verification

**One bounded verification, no code change:** inspected GitHub Actions run [43892508977](https://github.com/rixed318/Probably-TAU/actions/runs/43892508977) via job and step APIs. All four jobs report `success`. In job `133387195524`, each relevant step independently reports `completed/success`: pinned source revision recording, playable Day01 gameplay gateway regression, static offline entrypoint dependency closure, Content canonical money-wire validation, and other UI/Content regressions. This closes the earlier **CI NOT VERIFIED** gap for the static offline check on that run. These results apply to the **pinned refs configured in the workflow at that commit**, not to moving PR heads. This is not an independent Windows file:// launch or full Unity test.

**Tests executed locally this pass:** none (Node/.NET/Python NOT RUN). **Tests observed in GitHub CI:** four successful jobs; named steps above completed successfully. Unity Editor, Save→Load, Windows offline file launch, alternate choice combinations on the extracted artifact: **NOT RUN / NOT VERIFIED**. No runtime implementation or architectural contract changed.

### Сводка для координатора 00 — 2026-11-16 21:17 UTC+4

- **Latest independently verified QA evidence:** PR #10 branch previous recorded commit `bbde3fdc7320`; this handoff-only commit follows. PR #12 artifact-tested UI `8951e40eece975bec8cf731aae8c2746e97364f3`, whereas the last observed newer UI head was `fdfc468264ce`. Last observed PR #9 Core `f25401e6d055`, PR #11 Content `f92f4da7222a`, PR #14 ADR `ebdc4be28afd`, PR #16 NPC `b2680f76b21f`. These non-QA heads are **previously observed, not revalidated as current heads**. CI pinned revisions remain Core `0f5a85ef`, UI `8951e40e`, Content `c2d9ba26`, NPC `d82bc6ee` pending deliberate review. Main was read-only; no merge.
- **Blocker 1:** Independent direct offline launch from extracted folder on Windows remains NOT VERIFIED; Playwright injected-script smoke is not equivalent to double-clicking HTML offline.
- **Blocker 2:** Tested UI artifact SHA differs from last observed newer PR #12 head; alternate paths and duplicate-click regression on the newer artifact not evidenced.
- **Blocker 3:** Unity Editor and real Save→Load NOT RUN; durable GameState DTO readiness not independently confirmed.
- **Recommended next step:** obtain an artifact from the newest UI exact SHA, extract and launch with networking disabled on Windows, verify three visitors, alternative choices, repeated EndDay, unique ledger keys and conditional partner revenue; then record browser evidence. Issue #18 remains **BLOCKED**, no READY_FOR_OWNER_TEST.


## 2026-11-16 23:00 UTC+4 — Issue #19: immutable integration acceptance matrix (documentation only)

**One bounded deliverable:** independently refreshed GitHub PR metadata and documented an exact-SHA integration matrix for coordinator 00. This is a *test plan*, not a claim that cross-branch integration has been executed. No changes to Core/NPC/Content/UI, no shared contracts approved. Read main README, AGENTS.md, CHAT_WORKFLOW.md, Issues #6/#8/#18/#19 and PRs #9/#10/#11/#12/#14/#16. The ADR remains unmerged and C0-v2 governs proposals.

| Source | Latest PR HEAD observed 16 Nov 23:00 UTC+4 | Cross-branch integrated test on this exact SHA |
| --- | --- | --- |
| Core #9 | `eb3a74e8e58b82ba6a91545590f6296e0ebfd82c` | NOT RUN |
| NPC #16 | `b2680f76b21f9d85e11911f94dd27460a9e9acf4` | NOT RUN |
| Content #11 | `f92f4da7222a9dc60c07e6d870182e23d62ebaa2` | NOT RUN |
| UI #12 | `df0b4bc2ce39e1fadd9e04db961c5c7fc23acdc6` | NOT RUN |
| QA #10 | `8b1d2f35a92f332a1c36f27e97b804c81aed0ce7` (before this commit) | Earlier pinned CI only |
| ADR #14 | `ebdc4be28afd56e82bc14ca397af88376ae59d20` | Documentation only |

**Minimal first Unity Editor acceptance for #19 (future, not performed):** import the exact pinned integration commit in Unity 6.3 LTS; confirm no compile/asmdef errors; in PlayMode execute three unique document commands through the Core command processor with NPC visitor identities and Content values; check each command's single durable receipt, exactly-once ledger entries, expectedRevision and integer-kopeck balances; EndDay only after all three resolved; inspect serialized durable snapshot and replay after load without duplicate transactions. These checks are *proposed acceptance*, not a new architecture contract. Before the test, the coordinator must choose exact integration commit and record the four input SHAs; do not use mutable branch refs as evidence.

**#18 artifact provenance:** GitHub Actions run [43913954397](https://github.com/rixed318/Probably-TAU/actions/runs/43913954397) currently exposes unexpired artifact `day01-web-demo`, ID `14247763859`, 8484 bytes. This pass checked artifact metadata **only**, not its ZIP bytes, source SHA linkage, browser interactions or Windows double-click. Coordinator reports two previous Chromium paths (1,675,000 and 1,780,000 RUB) but this pass did not independently rerun them. Latest UI PR HEAD `df0b4bc2` is not assumed identical to artifact source. Node/.NET/Python local tests NOT RUN; Unity Editor and Save→Load NOT RUN.

### Сводка для координатора 00 — 2026-11-16 23:00 UTC+4

- **Latest PRs/commits:** #9 `eb3a74e8`, #10 `8b1d2f35` before this documentation commit, #11 `f92f4da7`, #12 `df0b4bc2`, #14 `ebdc4be2`, #16 `b2680f76`. Main PR base `d673322c`; no main write.
- **Blocker 1:** No cross-branch integration tests on the refreshed immutable SHA matrix; earlier green QA CI uses older pinned inputs.
- **Blocker 2:** Latest UI exact SHA has not been matched to downloaded artifact and independently exercised via Windows offline double-click; #18 READY_FOR_OWNER_TEST remains BLOCKED.
- **Blocker 3:** Unity Editor compile/PlayMode and real Save→Load still NOT RUN; no release claim.
- **Recommended next step:** coordinator records one integration candidate commit with the four input SHAs, runs pure .NET/Content tests against it, then Unity 6.3 LTS import/PlayMode smoke. Separately, obtain exact-SHA UI artifact and test offline launch and replay. No merge until owner authorization.


## 2026-11-17 00:38 UTC+4 — Issue #20 mobile acceptance preflight (one bounded static audit)

**Scope:** exact UI PR #12 HEAD `a7d12d7634a84fb448d97ed12e96f285fb9a7eaa`, file `prototypes/ui-day-one/day01-playable.html` (blob `c33773809f2ed8fc0d4367364681d3ca77a48629`). This is a static GitHub source audit, **not** Chromium touch emulation or Android installation. Read main AGENTS.md and CHAT_WORKFLOW.md, Issues #6/#8/#18/#19/#20/#21, PRs #9/#10/#11/#12/#14 and QA handoff before editing.

**Observed checks (source text):**
- PASS: HTML declares a viewport meta tag and has `@media(max-width:650px)` to stack the two-column layout on narrow screens.
- PASS: HTML uses three **relative external scripts** (`day01-read-model.cjs`, `day01-gameplay-gateway.cjs`, `day01-playable.js`); no HTTP(S) script URLs in this file.
- NOT SATISFIED by this file: single-file self-contained Android distribution. It depends on three other local files. This is **not** proof that a separate single-file artifact does not exist elsewhere.
- NOT VERIFIED: 48dp hit targets (button padding is defined, but there is no explicit minimum touch target), safe-area insets, landscape overflow, touch interaction, replay/duplicate ledger, EndDay behavior. No DOM/browser measurements performed.
- Source manifest `docs/art/ASSET_MANIFEST_D01.csv` contains 85 asset rows. No actual PNG bytes/signatures/dimensions/manifest SHA256 were checked; do **not** infer that assets have been committed from rows alone.

**Android acceptance checklist for QA once UI provides an exact-SHA self-contained HTML artifact:**
1. Record UI commit SHA, Actions run, artifact ID and ZIP SHA256; inspect extracted contents and confirm a **single** HTML with embedded JS/CSS, no remote resource requests.
2. Run isolated Chromium with mobile/touch emulation at **360×800 portrait** and **800×360 landscape**. Measure `documentElement.scrollWidth <= innerWidth`, critical content visibility, touch-target bounding boxes (minimum 48 CSS px as conservative emulation proxy; actual Android dp must be device-verified), focus and no hover-only actions.
3. On the exact artifact, inspect three distinct documents, exercise two divergent choice paths, EndDay once, repeated click/replay, unique ledger keys and integer-kopeck budget reconciliation; signing partner request must not immediately credit 120,000 RUB.
4. Report **WEB_MOBILE_EMULATION_PASS** only after these measurements and interactions; report Android physical device **NOT RUN** until real device evidence. Unity Editor compile/APK, Save→Load **NOT RUN**.
5. Future real Unity APK: pin Unity 6.3 LTS and Android Build Support/SDK/NDK/OpenJDK versions, verify build target/package ID/arm64, use debug/development signing for sideload; **never commit keystore/passwords**. Verify produced APK file SHA256 and actual Android installation/rotation/resume separately. No APK exists or is claimed by this audit.

**Tests actually run this pass:** deterministic static text inspection of the exact GitHub HTML (viewport, responsive breakpoint, script references, absence of remote script src, presence/absence of explicit touch/safe-area/min-size patterns). No Node/.NET/Python/Chromium/Unity tests executed. The static check cannot certify mobile UX.

### Сводка для координатора 00 — 2026-11-17 00:38 UTC+4

- **Latest PR HEADs fetched:** #9 Core `eb3a74e8e58b82ba6a91545590f6296e0ebfd82c`; #10 QA `603d7d7540d437d14d6a454cad754a4b0bb6c58e` before this commit; #11 Content `94ee96b691bb58f56aef7b998c883e070efecb88`; #12 UI `a7d12d7634a84fb448d97ed12e96f285fb9a7eaa`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `b2680f76b21f9d85e11911f94dd27460a9e9acf4`. Main base reported `d673322cd9d0d366a27a57b81056504dfb26c731`. C0-v2/ADR are not permission to merge.
- **Blocker 1:** exact current UI HTML is multi-file, while Issue #20 requires a verified downloadable single-file offline artifact; no current exact-SHA artifact/browser touch smoke performed.
- **Blocker 2:** portrait/landscape 48dp targets, overflow, alternative decisions and replay/EndDay still NOT VERIFIED on current UI SHA; Windows file double-click and real Android device NOT RUN.
- **Blocker 3:** Unity 6.3 Editor, Android APK build, real Save→Load and committed PNG binary verification NOT RUN.
- **Recommended next step:** UI #12 publishes self-contained HTML artifact pinned to `a7d12d76` or a new exact SHA; QA then runs both viewport touch scenarios and ledger replay against the actual extracted artifact. Keep #18/#20 BLOCKED for owner mobile readiness until evidence. No main/foreign branch write or merge.


## 2026-11-17 02:18 UTC+4 — #20 exact-head mobile artifact gate (read-only source verification)

**One bounded task:** refresh current GitHub PR heads and re-audit the actual UI Day01 entrypoint at immutable PR #12 SHA `fdc601a8bdec5d4e079ca9f2e93dfe5a58919890`. Read main README/AGENTS.md/CHAT_WORKFLOW.md, QA handoff, PRs #9/#10/#11/#12/#14 and Issues #6/#8/#18/#19/#20/#21. No cross-team code or architectural contracts changed.

**Verified source:** `prototypes/ui-day-one/day01-playable.html` at UI SHA `fdc601a8bdec5d4e079ca9f2e93dfe5a58919890`, blob `c33773809f2ed8fc0d4367364681d3ca77a48629`. It contains three external local script tags (`day01-read-model.cjs`, `day01-gameplay-gateway.cjs`, `day01-playable.js`), viewport meta and a max-width 650px breakpoint. This **specific entrypoint is not single-file**. Fetching the candidate `prototypes/ui-day-one/day01-mobile-single.html` returned 404; this does **not** establish that no differently named bundled artifact exists. Source blob matches the prior audited entrypoint, so no new mobile-ready evidence can be inferred from the newer moving UI head.

**Executed this pass:** GitHub API read-only file/PR/issue checks and static inspection of actual HTML text; no local Node/.NET/Python test, no artifact ZIP extraction, no Chromium 360x800 or 800x360 touch run, no Android device or Unity Editor/APK test, no PNG binary signature/dimension validation, no Save→Load. All runtime checks **NOT RUN**. The previous CI/browser results on older SHAs are not transitive to `fdc601a8bdec5d4e079ca9f2e93dfe5a58919890`.

### Сводка для координатора 00 — 2026-11-17 02:18 UTC+4

- **Latest PR heads (GitHub read):** #9 Core `eb3a74e8e58b82ba6a91545590f6296e0ebfd82c`; #10 QA `935329e6b4e8dabcc4122a3628207ea13946386f` before this handoff commit; #11 Content `fd2d3d08ff08d37a8dcff9499a19e6d7a50b5d08`; #12 UI `fdc601a8bdec5d4e079ca9f2e93dfe5a58919890`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `a9aa94ab0adeb1048c0d11d8c1fffbe11265c0e8`. PRs remain open/draft. Main only read, never written.
- **Blocker 1:** Current exact-SHA Day01 entrypoint is multi-file; no downloaded/extracted *single-file* artifact tied to UI `fdc601a8bdec5d4e079ca9f2e93dfe5a58919890` independently evidenced.
- **Blocker 2:** Chromium touch portrait/landscape, 48px targets, overflow, alternate choices, EndDay replay and partner cash behavior on current exact SHA NOT RUN.
- **Blocker 3:** Actual PNG binaries, Unity Editor/APK/physical Android and Save→Load NOT VERIFIED.
- **Recommended next step:** UI owner publish the self-contained Day01 HTML as a named Actions artifact with run ID and exact UI commit SHA. QA then download ZIP, hash and inspect bytes, run 360x800/800x360 Chromium touch paths and ledger replay; only then consider WEB_MOBILE_EMULATION_PASS. Issue #18/#20 remain BLOCKED; no READY_FOR_OWNER_TEST, no merge.


## 2026-11-17 04:04 UTC+4 — #20 exact UI HEAD static acceptance gate

**One bounded QA task:** recheck GitHub main policy, AGENTS.md, docs/CHAT_WORKFLOW.md, QA handoff, PR #9/#10/#11/#12/#14 and Issues #6/#8/#18/#19/#20/#21. Audit the immutable current UI PR #12 head `91572d56796372883fd43d31504c09c2e8ad869b` rather than a moving branch for the Android single-file entrypoint gate. No architecture change.

**Actual static verification:** GitHub fetched `prototypes/ui-day-one/day01-playable.html` at exact UI SHA above; blob `c33773809f2ed8fc0d4367364681d3ca77a48629`, 2579 text characters. Counted **three external local script tags**: `day01-read-model.cjs`, `day01-gameplay-gateway.cjs`, `day01-playable.js`. Viewport meta exists; explicit CSS `min-height:48px`/48px touch target declaration absent. **FAIL for single-file criterion of this specific entrypoint**; no assertion about differently named bundled artifacts. Responsive touch quality cannot be inferred from text.

**Actually executed:** GitHub source/metadata reads and deterministic HTML string inspection only. Browser touch 360x800/800x360, alternative choices, ledger replay, EndDay, offline Windows double-click, PNG signatures/dimensions, local Node/.NET/Python, Unity Editor, APK/Android device, Save→Load: **NOT RUN**. Previous CI/browser evidence on older UI SHAs is not evidence for this head. Issue #18/#20 remain **BLOCKED**, not READY_FOR_OWNER_TEST.

### Сводка для координатора 00 — 2026-11-17 04:04 UTC+4

- **Latest GitHub PR HEADs:** #9 Core `1675b14306079d746a052115d2e7099ff76500a5`; #10 QA `dd3141eac7123ccbf70735366a96911be214b6e3` before this handoff commit; #11 Content `1c9fb02347a1e6593d89b9a844d2189bc23c465a`; #12 UI `91572d56796372883fd43d31504c09c2e8ad869b`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `5fe9364ee86c81795e25c1ae72567f39f16a79c9`. Main base shown in PR metadata: `d673322cd9d0d366a27a57b81056504dfb26c731`.
- **Blocker 1:** Checked Day01 HTML entrypoint still has three external scripts; no verified exact-SHA single-file offline artifact.
- **Blocker 2:** Mobile touch viewports, 48dp hitboxes/overflow, three documents, EndDay, replay and finance invariants not executed on current UI SHA.
- **Blocker 3:** Original PNG binary validation and real Unity Editor/APK/Android/Save→Load acceptance not executed.
- **Recommended next step:** UI 03 publish named self-contained Day01 HTML artifact with exact commit provenance. QA 05 should download and hash the artifact, then execute Chromium touch 360x800 and 800x360 tests with multiple choice paths and replay; do not claim mobile-ready or Unity acceptance prematurely.

**Safety:** only feature/save-6 handoff updated; no main write, merge, third-party IP approval, or new shared contract.


## 2026-11-17 05:41 UTC+4 — Issue #20: correction of Android single-file artifact discovery

**One bounded QA task:** independently locate and statically inspect the *actual* Android-specific standalone entrypoint, not the older multi-file `day01-playable.html`. Exact immutable UI PR #12 head: `723dcc69534557d54b574991a425eaf5e13161c1`; file `prototypes/ui-day-one/day01-android-touch.html`, blob SHA `28450454bd89792d1d1ac16413ea6507f8b16dd8`. The previously inspected `day01-playable.html` remains multi-file, but that fact **must not** be generalized to the separate Android bundle. Previous QA handoffs' implication that no Android single-file candidate existed was incomplete.

**Actually executed (GitHub source bytes + deterministic text assertions):** 3 inline script elements, **0 external script src**, **0 external stylesheet links**, **0 literal HTTP(S) URLs**; viewport-fit=cover, four safe-area CSS insets, explicit min-width/min-height 48px, portrait and landscape media rules, all DOC-01/02/03 identifiers and EndDay control present. These checks **PASS as static source assertions only**. They do not prove artifact ZIP provenance, script runtime behavior, actual 48dp computed hitboxes, overflow, or playable mobile acceptance. `day01-mobile-single.html` is not the published entrypoint; `day01-android-touch.html` is.

**NOT RUN:** artifact ZIP download/extraction/hash; Chromium 360x800 or 800x360 touch interactions; ledger replay, alternate decisions, duplicate posting and partner cash assertions; actual Windows file:// and Android device; PNG binary signatures/dimensions; local Node/.NET/Python; Unity Editor/APK; Save→Load. Issue #18/#20 remain **BLOCKED**, READY_FOR_OWNER_TEST not granted. No contract changes, no merge/main writes.

### Сводка для координатора 00 — 2026-11-17 05:41 UTC+4

- **Latest PR HEADs checked:** #9 Core `540945353ebb748fd7eff4625cbe4df5a2dd6cfd`; #10 QA `56f9c4f37ce6cac90afdc2be1703fdc3e368991d` before this handoff update; #11 Content `97e8c32455a817ff51d65abc9697b2026008ef24`; #12 UI `723dcc69534557d54b574991a425eaf5e13161c1`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `e9eb3fe0c0d4f7eecfcda95b08a39a637b0a416a`. PR base main `d673322cd9d0d366a27a57b81056504dfb26c731`.
- **Blocker 1:** no independently extracted and hashed Actions ZIP at exact current UI SHA; standalone source exists but release artifact provenance is unverified.
- **Blocker 2:** actual Chromium touch viewport gameplay, target geometry/overflow, alternative choices, ledger replay and partner cash still NOT RUN on exact SHA.
- **Blocker 3:** real Android/Unity APK, Save→Load and original PNG binary verification NOT RUN.
- **Recommended next step:** download exact-SHA `day01-android-touch` artifact and validate extracted bytes; then Chromium 360x800/800x360 with 3 decisions/EndDay/replay. Distinguish WEB prototype from Unity/APK. Keep main protected.


## 2026-11-17 07:25 UTC+4 — Issue #20 immutable-head Android source regression

**One bounded QA check:** independently re-fetch Android single-file source at PR #12 exact HEAD `ca6c7ab43d3e2199259167ca097a4b9ffa080fbd`, file `prototypes/ui-day-one/day01-android-touch.html`, GitHub blob `28450454bd89792d1d1ac16413ea6507f8b16dd8`. Compared with previous audit at UI `723dcc69534557d54b574991a425eaf5e13161c1`: same content blob, despite branch HEAD change. This supports *source-byte equivalence*, not equivalence of release artifacts or tests on the new HEAD.

**Executed static assertions (GitHub fetched text, 10/10 PASS):** exactly 3 inline scripts; no script src; no linked external stylesheet; no literal HTTP(S) URL; viewport-fit=cover; CSS min-width/min-height 48px; DOC-01/02/03 identifiers; EndDay button; report ledger rendering; explicit zero immediate partner cash wording. This is a text-level source check, **not computed CSS geometry or functional execution**. All actual mobile Chromium 360×800 and 800×360 gameplay, overflow, double-tap, replay, artifact ZIP hash/extraction, Android device, Unity Editor/APK, PNG binary and Save→Load checks **NOT RUN** this pass. No new READY_FOR_OWNER_TEST.

### Сводка для координатора 00 — 2026-11-17 07:25 UTC+4

- **Latest PR HEADs:** #9 Core `540945353ebb748fd7eff4625cbe4df5a2dd6cfd`; #10 QA `3c038028fb17152352fae2f303a3910a0a1df6a5` before this update; #11 Content `97e8c32455a817ff51d65abc9697b2026008ef24`; #12 UI `ca6c7ab43d3e2199259167ca097a4b9ffa080fbd`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `e7adc9152ad81542acaa9d1faa77b3542e40844f`. Main policy AGENTS.md and CHAT_WORKFLOW.md re-read. No merge/main write.
- **Blocker 1:** No exact-HEAD UI Actions ZIP downloaded/extracted and byte-hashed against the standalone HTML; artifact provenance not established.
- **Blocker 2:** Chromium touch 360×800 and 800×360 actual choice/EndDay/replay and computed 48dp/overflow still NOT RUN.
- **Blocker 3:** Unity Editor/APK, real Android, PNG binary verification and Save→Load still NOT RUN.
- **Recommended next step:** retrieve downloadable Actions artifact tied to UI `ca6c7ab43d3e2199259167ca097a4b9ffa080fbd`, hash/extract it, then execute portrait/landscape touch and financial replay assertions on extracted bytes. Treat web mock separately from Unity build.


## 2026-11-17 09:00 UTC+4 — Issue #20 Android gateway static invariant audit

**Bounded task:** re-read actual UI branch `prototypes/ui-day-one/day01-android-touch.html` (GitHub blob `28450454bd89792d1d1ac16413ea6507f8b16dd8`) and inspect six *source-level* finance/replay guards. Exact branch commit HEAD was not independently resolved in this check; do not equate the blob with a branch commit or artifact ZIP.

**Executed via connector-fetched source and literal assertions: 6/6 static PASS:** (1) duplicate decision rejection `decisions.has(id)`; (2) ledger entry key `day0:` + document ID; (3) partner model `recognizedRevenueKopecks: "0"` and no detected `balance+=BigInt(` expression; (4) repeated EndDay returns existing report and initial EndDay requires three decisions; (5) decision cost uses BigInt and subtracts from balance; (6) report notes explicitly disclaim automatic L-204 booking. These assertions **do not establish** runtime idempotency, uniqueness across Save→Load, absence of all possible revenue paths, or real Android usability. No runtime tests were executed in this pass.

**NOT RUN / BLOCKED:** actual exact-commit Actions ZIP provenance and extracted-file hash; Chromium touch 360x800 / 800x360, 48dp computed geometry, overflow, three-document interactions, replay; Android hardware, Unity Editor/APK, Save→Load; PNG binary validation. Do not grant READY_FOR_OWNER_TEST.

### Сводка для координатора 00 — 2026-11-17 09:00 UTC+4

- **PRs:** #9 Core, #10 QA, #11 Content, #12 UI, #14 ADR, #16 NPC remain the integration review set; current exact commit heads **not independently verified in this pass**. QA branch was updated by this handoff commit (SHA in GitHub PR #10). UI Android source blob `28450454bd89792d1d1ac16413ea6507f8b16dd8` checked; do not mistake for commit SHA. C0-v2 and ADR remain upstream decisions, not approved here.
- **Blocker 1:** exact-commit Android HTML artifact ZIP not downloaded/hashed.
- **Blocker 2:** interactive Chromium portrait/landscape, double-tap, EndDay, finance replay and computed touch geometry NOT RUN.
- **Blocker 3:** original PNG binary provenance and real Unity/APK/Android/Save→Load NOT RUN.
- **Recommended next step:** pin UI PR #12 commit SHA, download its Actions single-file artifact, verify extracted HTML SHA256 and execute 360x800 + 800x360 Chromium touch with three documents, replay and partner-cash assertions. Only after that evaluate WEB_READY; APK remains a separate gate.

**Safety:** no shared architecture contracts approved, no main push/merge, no third-party IP authorization.


## 2026-11-17 10:35 UTC+4 — exact UI HEAD standalone HTML provenance check (Issue #20)

**One bounded task:** independently pin PR #12 HEAD `1b15c25c83d252b52213440d9d9edbdb790303e3` and read `prototypes/ui-day-one/day01-android-touch.html` at that exact commit. GitHub returned blob SHA `28450454bd89792d1d1ac16413ea6507f8b16dd8`, matching the previously audited blob. Static source checks (connector-fetched UTF-8): no external `<script src>`; no external stylesheet `<link>`; exactly 3 inline `<script>` tags; viewport meta present. A generic `http(s)` string scan is informational only and cannot establish offline runtime. The identical blob indicates **no HTML byte changes** across the audited commits, not that a downloadable Actions ZIP is identical. No browser execution or binary artifact retrieval in this pass.

**Actual checks:** 4 static structural assertions PASS (no external script tags, no external CSS links, 3 inline scripts, viewport). Node, .NET, Chromium touch 360x800/800x360, finance replay, 48dp computed hit targets, overflow, APK/Unity/Save→Load, and PNG binary signatures **NOT RUN**. No claim of WEB_READY/READY_FOR_OWNER_TEST.

### Сводка для координатора 00 — 2026-11-17 10:35 UTC+4

- **Latest verified PR HEADs:** #9 Core `540945353ebb748fd7eff4625cbe4df5a2dd6cfd`; #10 QA `48c8b8ecc0baf0aa45bf639afc4403e2285f3da8` before this handoff; #11 Content `91e5413841a883f9055634d8682427e2e32f9a5f`; #12 UI `1b15c25c83d252b52213440d9d9edbdb790303e3`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `661ca75c2689bc29911ab00bcbd8b2f4a5da5d4b`. Main AGENTS.md and CHAT_WORKFLOW.md re-read; C0-v2/ADR are not merged into main.
- **Blocker 1:** no exact-commit Actions artifact ZIP downloaded and hashed against the verified HTML blob.
- **Blocker 2:** Chromium touch gameplay (portrait/landscape, 3 decisions, EndDay, replay, no partner immediate cash) and computed 48dp/overflow NOT RUN.
- **Blocker 3:** PNG binaries, Unity Editor/APK/real Android and Save→Load NOT RUN.
- **Recommended next step:** download the Actions ZIP associated with UI `1b15c25c...`, verify extracted HTML byte hash and execute the two touch viewport flows; do not treat the web prototype as a Unity build. No main write, merge or external IP authorization.


## 2026-11-17 12:23 UTC+4 — Issue #20 latest-head Android source verification

**One bounded independent QA task:** checked current PR #12 HEAD `f509b64ad516f1ba83d84afdfe6af9b9773dc186` and fetched `prototypes/ui-day-one/day01-android-touch.html` by that immutable commit. GitHub returned unchanged blob `28450454bd89792d1d1ac16413ea6507f8b16dd8` (15,520 UTF-8 characters). This is source provenance only, **not** a released ZIP artifact or browser run.

**Actually executed via GitHub connector and JS literal checks:** 3 inline script blocks; 0 external script-src blocks; CSS explicit `min-height:48px` and `min-width:48px`; safe-area references; all three document IDs; `decisions.has(id)` duplicate guard; `decisions.size!==3` EndDay guard; literal recognized partner revenue `0`. All eight named source-presence checks PASS. CSS `overflow-x` declaration absent; this alone does not establish overflow or its absence. **Important:** these are static text assertions, not runtime, hit-target geometry or financial execution.

**Execution environment check:** Chromium and Node are installed in the QA container, but direct GitHub raw-content retrieval failed with DNS/proxy resolution; the connector text cannot automatically be transferred to that browser filesystem. Therefore touch emulation 360×800 / 800×360, three interactive decisions, double-tap, EndDay, ledger replay, horizontal overflow and computed target sizes **NOT RUN**. Exact-SHA Actions ZIP not downloaded or byte-verified. Unity Editor, APK, physical Android, PNG binary signatures and Save→Load **NOT RUN**. No READY_FOR_OWNER_TEST.

### Сводка для координатора 00 — 2026-11-17 12:23 UTC+4

- **Latest PR HEADs verified:** #9 Core `80dd030e630fb61375dec5e0c084ed18b8d2ff85`; #10 QA `1a72df7c7fa33e19f62a25ba86b2ac7e62818d3f` before this update; #11 Content `b91c81e296111f2a092eb281c782b7682752234a`; #12 UI `f509b64ad516f1ba83d84afdfe6af9b9773dc186`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `fd6837dde29ebcfd4bc8d47faa6a54d22ddac83d`. Main read and AGENTS/CHAT_WORKFLOW checked. C0-v2 and ADR are feature-development guidance, not merged approval.
- **Blocker 1:** exact-UI-SHA release ZIP not extracted and hashed; GitHub raw DNS retrieval failed in browser environment.
- **Blocker 2:** no actual mobile Chromium 360×800 / 800×360 gameplay, computed touch target, overflow, replay and partner-cash checks.
- **Blocker 3:** real Unity/APK/Android, PNG binary verification and Save→Load tests absent.
- **Recommended next step:** retrieve an immutable Actions artifact associated with UI `f509b64a...`, transfer extracted standalone HTML into Chromium test environment, then run both mobile viewports and finance/replay acceptance. Keep web mock distinct from Unity; no main push/merge or IP approvals.


## 2026-11-17 14:00 UTC+4 — Issue #20 Android Unity APK acceptance/signing checklist (QA-only)

**One bounded deliverable:** define a reproducible owner-test checklist for a *future real Unity Android APK*, without claiming a build exists or modifying Core/UI. Read current `main` AGENTS.md and CHAT_WORKFLOW.md, PRs #9/#10/#11/#12/#14/#16, Issues #6/#8/#18–#21. C0-v2 in #9 and ADR #14 remain feature-development guidance; QA approves no shared contract.

**Preflight / provenance:** pin immutable Core/NPC/Content/UI commit SHAs and Unity editor version (Issue #19 targets Unity 6.3 LTS); record Android Build Support, SDK/NDK/OpenJDK versions; log clean checkout, build command, CI run URL, output APK filename, byte size and SHA-256. Do not treat branch names as immutable inputs. Record package ID, versionCode/versionName, min/target SDK and architecture actually produced. Fail the gate if any of these are unknown.

**Signing / secrets:** use a dedicated test signing identity (debug signing acceptable for an explicitly labeled test APK); do not commit keystores, passwords, `*.keystore`, `*.jks`, `*.p12` or secret-bearing Unity settings. Prefer secure CI secret storage or a local signing setup outside the repository. Record only certificate fingerprint and signing mode, never credentials. Verify the resulting APK signature with Android SDK `apksigner verify --verbose --print-certs <apk>` and archive redacted logs.

**Install / launch:** on a physical Android phone capture Android version, model, orientation, device resolution and `adb devices`; install with `adb install -r <apk>`, cold-launch via launcher/adb, disconnect network and confirm offline start. Collect `adb logcat` (redact personal data) for crashes/ANRs; capture screenshots for 360x800 portrait and 800x360 landscape where the device/emulator can reproduce those sizes. Emulator touch tests are distinct from physical-device results.

**Gameplay gate:** from a fresh state process DOC-01/02/03 with recorded choices, verify exactly three unique ledger entries and integer-denominated balance; EndDay once, attempt double-tap/replay and ensure no duplicate financial postings; partner immediate recognized cash remains zero unless an approved contract changes it. Capture expected/actual balance, ledger IDs, report and screenshots. A second fresh run should exercise different choices. If stable Core DTO/SaveService is absent, mark Save→Load NOT RUN, never simulate it with browser localStorage.

**Result policy:** PASS only with exact SHA, artifact hash, successful signature verification, install + offline launch and observed gameplay logs. Any missing evidence = BLOCKED/NOT RUN, not READY_FOR_OWNER_TEST. The browser single-file HTML and its touch emulation are a separate web-demo gate, not proof of an APK.

**Actual tests this pass:** GitHub metadata and document reads only. No APK artifact or physical Android/Unity Editor available to this check; `apksigner`, `adb`, Unity tests, Node/.NET, Chromium and Save→Load **NOT RUN**. No PNG binary validation was performed. This is a checklist, not a test result.

### Сводка для координатора 00 — 2026-11-17 14:00 UTC+4

- **Verified PR HEADs before this QA commit:** #9 Core `98ea35dae6b43817eb1f2d140390e6fa1e064e21`; #10 QA `1682ee9ff6cb5c7859886f0a56f1b946b01e4202`; #11 Content `165215f13ce85ae0262131d1f92f0c67786f6f6f`; #12 UI `8ccb613dbff8d6b893eccf3de41ffc726036fb77`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `9e18b8e3568be94986372916e188ca25074cc61b`. QA HEAD will advance with this documentation-only commit.
- **Blocker 1:** no immutable UI Actions ZIP retrieved/hashed and no Chromium touch 360x800 / 800x360 gameplay/replay run for latest UI SHA.
- **Blocker 2:** no verified Unity APK, signing/install/offline launch or physical Android acceptance; checklist is prepared only.
- **Blocker 3:** original PNG signatures/dimensions/manifest hashes and real Save→Load still unverified.
- **Recommended next step:** retrieve exact-UI-SHA single-file Actions ZIP and execute mobile Chromium tests; separately ask Unity integrator to produce an SHA-pinned test APK with signature/hash and install evidence using the checklist above. No READY_FOR_OWNER_TEST, no merge/main write, no third-party IP authorization.


## 2026-11-17 15:36 UTC+4 — Issue #21 PNG delivery gate: independent PR tree audit

**One bounded task:** inspect PR #12's paginated changed-filename inventory at immutable UI HEAD `9a80e9360216867c3e6dd7f400b911783bb2d4ae`, and fetch `docs/art/ASSET_MANIFEST_D01.csv` at that SHA. **Result: FAIL for the first-batch binary-delivery gate:** PR #12 changed-file inventory contains **zero paths ending in `.png`**. The manifest's sampled first-batch rows (office_wall, office_floor, office_window, desk_body, NPC portrait) are `planned` with blank `source_file`, `UNASSIGNED` generator and `MASTER_TBD` reference. These are planned entries, not uploaded assets. This is a repository/manifest audit, not image decoding; PNG signature, dimensions, alpha, SHA-256 and visual originality **NOT RUN** because no PNG files were present in the PR changed-file inventory. Do not claim the entire repository has no PNG; scope is PR #12 diff.

**Actual verification:** GitHub connector returned PR #12 HEAD, complete paginated changed filenames and manifest blob `57d69589a7b6671b85810dad77506c311d5ffa43`; exact filename suffix filter found 0 `.png` paths. No local Python/Node, Chromium, Unity or APK tests executed. No rights or IP approval inferred.

**QA request to UI 03:** commit 3–5 actual original PNG binaries to `feature/ui-5` with populated `source_file`, provenance and SHA-256 in the manifest; after commit, QA will retrieve each immutable blob and check PNG signature, IHDR dimensions and manifest SHA-256 before any art-ready assertion.

### Сводка для координатора 00 — 2026-11-17 15:36 UTC+4

- **Verified PR HEADs before QA commit:** Core #9 `98ea35dae6b43817eb1f2d140390e6fa1e064e21`; QA #10 `160923265062cf7c5cbf92b310d62bc979588c74`; Content #11 `165215f13ce85ae0262131d1f92f0c67786f6f6f`; UI #12 `9a80e9360216867c3e6dd7f400b911783bb2d4ae`; ADR #14 `ebdc4be28afd56e82bc14ca397af88376ae59d20`; NPC #16 `7c78ed88f3324f9018f7233f4877e690a32cd365`. Main AGENTS.md and CHAT_WORKFLOW.md re-read; C0-v2/ADR are not merged authority.
- **Blocker 1:** Issue #21 PR #12 currently has no changed PNG binaries to verify; manifest entries remain planned.
- **Blocker 2:** exact-SHA Actions single-file ZIP and Chromium touch 360x800/800x360 gameplay/replay/48dp geometry remain NOT RUN.
- **Blocker 3:** real Unity Editor/APK/Android and Save→Load acceptance NOT RUN.
- **Next:** UI 03 commits 3–5 PNGs with provenance/hash; QA independently verifies bytes and dimensions. In parallel, retrieve exact-SHA HTML artifact for mobile Chromium acceptance. No READY_FOR_OWNER_TEST, no main push/merge or third-party IP approval.


## 2026-11-17 — QA 05: independent alpha-audit logic probe (#21)

**One bounded task:** inspect exact GitHub UI source `tools/art/audit_candidate.py` (blob `9f57e3890aa498cf32b9eea7830b40e8123ed0fb`) and `tools/art/test_audit_candidate.py` (blob `2636e0b4a6b7b48204265fa0fe679fb82905cf5f`) on UI HEAD `0d06323f18f7cac395572482ee22184b62f7a298`. Verify the recently added transparent/partial-alpha counting rules independently. This is NOT binary art acceptance, and no architecture/API change is proposed.

**Actually executed locally:** Python 3 + Pillow 12.3.0, four independent 320x180 image probes using the same Counter/alpha expressions as the GitHub source: RGB opaque -> 0/false; RGBA alpha 0 -> 57600/false; RGBA alpha 128 -> 57600/true; RGBA alpha 255 -> 0/false. **4/4 PASS** for this isolated logic probe. The test uses a locally transcribed/reimplemented expression, not a checkout of the repository module; **the repository's 8-test unittest suite was NOT RUN**. Git clone failed with `Could not resolve host: github.com`; authenticated GitHub connector read succeeded. No actual candidate PNG bytes were read, and PNG signature/dimensions/SHA256 of committed production assets remain **NOT RUN**.

**Source review limitation:** `production_ready: False` is hardcoded, as appropriate for candidate audit. Counts are over raw RGBA tuples; transparent pixels with differing hidden RGB can inflate `unique_colors` (not a blocking defect for this candidate-only diagnostic, but should not be treated as palette certification). No Unity Editor, Android APK/device or Chromium touch runtime test executed in this pass.

### Сводка для координатора 00 — 2026-11-17 17:23 UTC+4

- **Verified branch heads before this QA update:** main `d673322cd9d0d366a27a57b81056504dfb26c731`; PR #9 Core `98ea35dae6b43817eb1f2d140390e6fa1e064e21`; PR #10 QA `448e790f04cf92fdf4af6adf4df47dde20fda842` (handoff commit follows); PR #11 Content `e451699f14cfdcb2c12513d1d2f6619aa32ef5e7`; PR #12 UI `0d06323f18f7cac395572482ee22184b62f7a298`; PR #14 ADR remains unmerged. PRs #9/#10/#11/#12 and Issues #6/#8/#20/#21 were read via GitHub; no merge performed.
- **Blocker 1:** Issue #21 still lacks verified committed original PNG binaries; synthetic alpha probes do not verify the claimed C001 candidate or manifest SHA.
- **Blocker 2:** exact-SHA Android single-file artifact and Chromium 360x800 / 800x360 interactive paths (3 docs, EndDay, ledger replay, 48dp/overflow) remain NOT RUN in this pass; container cannot resolve github.com for checkout.
- **Blocker 3:** Unity Editor APK/Android-device and full Save→Load regression remain NOT RUN; final persistence requires stable integrated DTO and runtime test environment.
- **Recommended next step:** UI 03 commit real PNG bytes with provenance and SHA256; QA verify 3–5 binaries and run its actual Pillow suite. Independently obtain pinned Android HTML artifact and execute Chromium touch acceptance. Do not grant READY_FOR_OWNER_TEST from source-only checks.
- **Safety:** only QA handoff changed in `feature/save-6`; no main push/merge, no shared contract approval.


## 2026-11-17 19:00 UTC+4 — QA 05: art-manifest approval gate source review (#21)

**One bounded independent review:** inspected UI PR #12 HEAD `357d8e811bc7472b08242684396178c2a41e84b0`, `tools/art/validate_manifest.py` blob `278f3c0e6d65a8513932144b1b42fb7b09932435`, test blob `995a0ac284ca05db499a1012289d359e9b0379a5`, and the manifest blob `57d69589a7b6671b85810dad77506c311d5ffa43`. PR changed-file inventory contains **zero .png paths**. The manifest validator checks provenance strings for approved/production rows but **does not open source_file, check PNG signature/IHDR, verify dimensions against native_px, or compare SHA-256**. In particular, any nonempty source_file and any rights_review value other than 'pending' or empty passes these two checks even if the path is nonexistent. The current 8 unittest cases test CSV rules, not binary asset acceptance. This is a **source-level coverage gap**, not a claim that an executed test failed.

**Actual checks:** authenticated GitHub PR metadata, complete changed-filename inventory, source and manifest reads. **NOT RUN:** Python unittest suite, PNG binary verification (no committed PNG in PR diff), Chromium touch 360x800/800x360, Node/.NET, Unity Editor, APK/Android and Save→Load. No approved third-party IP.

**Precise recommendation to UI 03:** retain CSV structural validator but add a separate binary gate for each asset promoted to qa_ready/approved/production: resolve source_file within repository, reject missing/out-of-tree files, verify PNG 8-byte signature and IHDR dimensions against native_px, record/compare SHA-256 in manifest, and cover missing/tampered/wrong-dimension fixtures. Do not infer rights approval from arbitrary CSV text. QA will independently run the gate against 3–5 actual committed binaries when available.

### Сводка для координатора 00 — 2026-11-17 19:00 UTC+4

- **PR HEADs checked:** #9 Core `98ea35dae6b43817eb1f2d140390e6fa1e064e21`; #10 QA before this commit `e59371686227dc7b65163239f9e79eea2c73552f`; #11 Content `6bfe29ecedf7bd05be120132ecd9a291a951f336`; #12 UI `357d8e811bc7472b08242684396178c2a41e84b0`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `0fe6f31e1f9ac6acff8f5bba63e5dade627e0421`. Main base `d673322cd9d0d366a27a57b81056504dfb26c731`. PR #9 C0-v2 and PR #14 ADR remain unmerged; no new contract approved.
- **Blocker 1:** Issue #21 has no PNG in UI PR diff; current manifest validator cannot certify PNG bytes/dimensions/hash.
- **Blocker 2:** exact-SHA HTML ZIP and Chromium 360x800/800x360 interactive Day01 acceptance remain NOT RUN.
- **Blocker 3:** Unity Editor APK/physical Android and complete Save→Load regression remain NOT RUN.
- **Recommended next step:** UI 03 deliver 3–5 original PNG binaries with SHA-256 and binary validation, then QA audit actual bytes. Separately run exact-SHA HTML mobile interaction tests. **READY_FOR_OWNER_TEST not granted.** QA-only handoff change, no main write/merge.


## 2026-11-17 20:41 UTC+4 — QA 05: standalone PNG header/hash regression (#21)

**One bounded task:** added `Tests/QA/test_png_integrity.py` in QA-owned `feature/save-6`, commit `2746222d55b889b73f28754551d3d6dbce139c3c`. Standard-library-only independent gate checks PNG signature, IHDR length/type, IHDR CRC, exact width/height and SHA-256. CLI accepts `--file PATH --width 320 --height 180 --sha256 HEX`. This does **not** decode complete PNG chunks, certify image appearance, or grant IP/rights approval; use Pillow or a full decoder for final art acceptance.

**Actually executed locally:** `python3 /mnt/data/qa/test_png_integrity.py -v` -> **5 tests passed** (synthetic header fixture; invalid signature, wrong dimensions, wrong SHA, corrupt IHDR CRC). GitHub committed source was not separately downloaded and rerun after upload. PR #12 changed-file inventory checked at `371f8ae0da653fd5e436d936aac8fe00215d83d0`: 31 changed paths, **0 PNG**. Real PNG binary acceptance NOT RUN; no actual files to certify. Chromium touch 360x800/800x360, Node/.NET, Unity Editor, APK/Android and Save→Load NOT RUN this pass. No C0/ADR change.

### Сводка для координатора 00 — 2026-11-17 20:41 UTC+4

- **Heads before QA writes:** main `d673322cd9d0d366a27a57b81056504dfb26c731`; #9 Core `2961ab2ece1cdbbc0a8448268ab517b2d87149e2`; #10 QA `36964ac3c01f2777591f06ffc0fc1d6e6855950a` -> new test commit `2746222d55b889b73f28754551d3d6dbce139c3c` -> this handoff commit; #11 Content `baaee3d95714b203d19637a558488c589e6e8efc`; #12 UI `371f8ae0da653fd5e436d936aac8fe00215d83d0`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `e8672c866801076c62ce9e71c98642afbf26f0e4`. PR #9 C0-v2 and #14 ADR remain open/unmerged.
- **Blocker 1:** #21 actual original PNG binaries and manifest SHA still not independently verified (zero PNG in PR #12 diff).
- **Blocker 2:** #20 exact-SHA ZIP and Chromium touch interactive Day01 acceptance (3 docs, EndDay, ledger replay, 48dp, overflow) NOT RUN.
- **Blocker 3:** Unity Editor APK/physical Android and complete Save→Load regression NOT RUN; stable integrated DTO/runtime needed.
- **Recommended next step:** UI 03 commit 3–5 real original PNG binaries with expected SHA-256; QA run the new binary header/hash gate **and** full PNG decoding against those bytes, then pinned HTML mobile acceptance. No READY_FOR_OWNER_TEST, no main push/merge, no shared contract or IP approval.


## 2026-11-17 22:17 UTC+4 — QA 05: PNG IHDR encoding rejection (#21)

**One bounded QA change:** strengthened `Tests/QA/test_png_integrity.py` to reject illegal PNG IHDR combinations (color type/bit depth, compression, filter and interlace) even when IHDR CRC and file SHA-256 match. Added five negative tests. Commit `8e7505fde12b18de426abab6b98b2884c8fbb8a9` in `feature/save-6`. No shared architecture or SaveService contract changed.

**Actually executed:** local Python 3 standalone equivalent of the changed verification logic and ten test cases: **10/10 PASS** (five previous scenarios + five new invalid IHDR encoding cases). This local probe was manually reconstructed from the same logic; **the exact post-push GitHub file was not downloaded and rerun**, and CI status is unverified. GitHub connector confirmed the file write and returned the commit SHA. The gate still checks only signature/IHDR/CRC/dimensions/SHA; it does not validate IDAT, IEND, full PNG decoding, visual quality, or IP rights. Do not use synthetic header fixtures as evidence of a real image.

**Read-only checks:** main README/AGENTS/CHAT_WORKFLOW, Issues #6/#8/#20/#21 and PR descriptions #9/#10/#11/#12/#14; PR #10 changed-file list includes QA test, PR #12 changed-file list has **0 .png paths**. C0-v2 in #9 and ADR in #14 remain feature-development guidance only, not merge approval.

### Сводка для координатора 00 — 2026-11-17 22:17 UTC+4

- **Latest QA commit:** `8e7505fde12b18de426abab6b98b2884c8fbb8a9` in PR #10 (this handoff update produces the next commit). Other PRs reviewed by number: #9 Core, #11 Content, #12 UI, #14 ADR; exact latest HEAD SHAs for those branches **not independently retrieved this pass**. Last recorded heads from preceding handoff: main `d673322cd9d0`, #9 `2961ab2ece1c`, #11 `baaee3d95714`, #12 `371f8ae0da65`, #14 `ebdc4be28afd`; do not treat them as current.
- **Blocker 1:** #21 no verified 3–5 real PNG binaries/manifest SHA; current gate is header-level only.
- **Blocker 2:** #20 exact-SHA offline HTML ZIP and Chromium touch 360×800 / 800×360 (three documents, EndDay, ledger replay, 48dp and overflow) **NOT RUN**.
- **Blocker 3:** Unity Editor, real APK/Android device and full Save→Load regression **NOT RUN**; stable integrated DTO/runtime still required.
- **Recommended next step:** UI 03 commits original PNGs and manifest hashes; QA independently decodes and verifies 3–5 actual binaries, then tests pinned Day01 HTML in mobile Chromium. Keep READY_FOR_OWNER_TEST withheld. No push to main, no merge, no IP approval.


## 2026-11-17 23:59 UTC+4 — QA 05: current UI PNG delivery gate (#21)

**One bounded independent verification:** queried current PR #12 HEAD `2327469711c18123b8de62a1801166482d775b36` and the complete paginated changed-filename inventory via GitHub. It contains **31 paths and 0 .png files**. The PR does include `tools/art/audit_candidate.py`, tests, a CSV manifest, and `prototypes/ui-day-one/day01-android-touch.html`, but source/test/manifest paths are not binary image delivery. This is a PR-diff inventory result, **not** a repository-wide absence claim. Issue #21 acceptance of 3–5 committed PNGs cannot be certified from this PR. No source files changed in other branches.

**Actually executed:** GitHub authenticated metadata and PR changed-file inventory checks. **NOT RUN:** local Python/Node/.NET tests, real PNG signature/dimensions/SHA-256/decode (no PNG in PR diff), Chromium touch 360×800/800×360 and three-doc/EndDay/ledger/48dp/overflow interactions, Unity Editor, APK/physical Android, Save→Load. Earlier 10/10 reconstructed synthetic tests are not a new execution. No new C0/ADR/IP approval.

### Сводка для координатора 00 — 2026-11-17 23:59 UTC+4

- **Verified PR HEADs:** #9 Core `efec6461288211eef3de4d459a8333aa758d2932`; #10 QA before this handoff `69dde1f4505f5bbd1b8065e61cb9e2e771f351ea`; #11 Content `c1ccc28311091ba978f51e3a0bc0d891faa4736b`; #12 UI `2327469711c18123b8de62a1801166482d775b36`; #14 ADR `ebdc4be28afd56e82bc14ca397af88376ae59d20`; #16 NPC `79c6a5b27ce1a21df9407bb8e00ce043b1df8d12`. Main README read on main; no independent main HEAD commit lookup. PR #9 C0-v2 and PR #14 ADR remain open Draft, not merged.
- **Blocker 1:** Issue #21: 0 PNG in current UI PR diff; actual 3–5 original PNG binaries, their dimensions, provenance and manifest SHA-256 are not certified.
- **Blocker 2:** Issue #20: exact-current-SHA HTML artifact and Chromium touch interactive acceptance remain NOT RUN.
- **Blocker 3:** Unity Editor/APK/Android and durable integrated Save→Load regression remain NOT RUN; do not implement a parallel GameState.
- **Recommended next step:** UI 03 deliver 3–5 real original PNG binaries with SHA-256 in PR #12; QA then run header/hash and full decode against actual committed bytes. Separately pin current HTML SHA and perform both Chromium touch viewport scenarios. **READY_FOR_OWNER_TEST withheld.** No main writes, merges, foreign-branch edits or IP approvals.


## 2026-11-18 01:38 UTC+4 — QA 05: independent embedded Day01 gateway regression (#20)

**One bounded change:** added `Tests/QA/test_day01_offline_html.cjs` to `feature/save-6`. It reads the *actual checked-out* `prototypes/ui-day-one/day01-android-touch.html`, extracts embedded read-model and gateway scripts into a Node VM context and asserts: exactly three documents and EndDay gate; inspection prerequisite and duplicate-debit rejection; ledger opening+sum=closing, unique ledger keys and zero immediate partner revenue. This is a Node logic regression, **not** a Chromium touch/geometry test or Unity test. Initial commit `39a44b31c7e1ab882b436f30c3be878f744ad8e6`; corrected closing balance fixture in `7dc09e0f44965d4bc6e4243a948e3c8d50ae3b60` (200000000 - 22000000 - 6500000 - 4000000 = 167500000).

**Execution:** GitHub file writes and source inspection confirmed. `node --test Tests/QA/test_day01_offline_html.cjs` **NOT RUN** in a checked-out exact GitHub tree; no Node runtime evidence is claimed. Chromium 360×800 / 800×360, physical Android, Unity Editor/APK, Save→Load and real PNG decoding also **NOT RUN**. No shared contracts or other branches changed.

### Сводка для координатора 00 — 2026-11-18 01:38 UTC+4

- **QA PR #10:** new Node regression test committed as `7dc09e0f44965d4bc6e4243a948e3c8d50ae3b60` before this handoff commit. **UI PR #12:** exact checked-out branch HTML blob `28450454bd89792d1d1ac16413ea6507f8b16dd8`; PR changed-file inventory 31 paths, 0 PNG. PRs #9 (C0-v2), #11 (content), #12 (UI), #14 (ADR) and Issues #6/#8/#20/#21 reviewed; PR branch HEAD SHAs not independently retrieved in this pass. Main README, AGENTS.md and CHAT_WORKFLOW.md read, no main writes.
- **Blocker 1:** #20 new Node test needs actual execution on exact checked-out UI/QA integration; Chromium touch 360×800 and 800×360, 48dp/overflow and interactive flows NOT RUN.
- **Blocker 2:** #21 no actual 3–5 PNG binaries in UI PR changed-file list to validate against manifest SHA and decode.
- **Blocker 3:** real Unity Editor/APK/Android and integrated Save→Load NOT RUN; await stable approved Core DTO.
- **Next step:** run `node --test Tests/QA/test_day01_offline_html.cjs` in a checkout containing PR #12 UI HTML and PR #10 QA test, recording exact combined SHA/tree and actual result. Then run pinned Chromium touch scenarios and verify PNG when delivered. **READY_FOR_OWNER_TEST withheld**. No merge/main push or IP approval.


## 2026-11-18 — AutoDev 05: pinned Android touch Node gate (#20)

**One bounded change:** QA workflow now checks out UI PR #12 at exact SHA `e09653994625adb6de312b2070b5e7745fc7cd83` (rather than the older UI SHA) and runs `node --test prototypes/ui-day-one/test-day01-android-touch.cjs` in the read-only UI checkout. Commit `d04914c72cd06974b5d8d055f6af2169acc2c8b9` on `feature/save-6`. This is Node regression coverage, **not** Chromium touch emulation or Android APK. Existing Core/Content/NPC refs are deliberately unchanged; workflow is pinned and may lag future upstream changes.

**Evidence:** GitHub source and PR metadata were read; UI PR #12 changed-files list includes the referenced test file. Workflow update was accepted by GitHub. **New CI execution and test results NOT VERIFIED / NOT RUN** at this handoff. No claim of green CI. Unity Editor, APK, device, Chromium 360x800 / 800x360, full Save→Load and actual PNG binary checks **NOT RUN**. No PNG files in the current PR #12 changed-filenames list (this does not establish repository-wide absence).

### Сводка для координатора 00 — 2026-11-18

- **PR heads inspected before QA commit:** #9 Core `2b070e74c3ef`; #10 QA `14dabde68095` → workflow commit `d04914c72cd0` → this handoff commit; #11 Content `d55b16385ce1`; #12 UI `e09653994625`; #14 ADR `ebdc4be28afd`; #16 NPC `a02d2240311d`. Read main AGENTS.md, CHAT_WORKFLOW.md, README and Issues #6/#8/#20/#21. C0-v2/ADR remains feature-only, not authorization to merge.
- **Blocker 1:** New exact-SHA Android Node gate has no verified Actions result; Chromium mobile touch, ledger replay and EndDay interactive acceptance remain NOT RUN.
- **Blocker 2:** No actual PNG binaries in UI PR #12 changed-file list to validate signature/dimensions/manifest SHA.
- **Blocker 3:** No verified Unity Editor APK/device execution or stable complete snapshot DTO for production SaveService; Unity and Save→Load NOT RUN.
- **Recommended next step:** inspect the GitHub Actions run for QA workflow commit `d04914c72cd0`; fix any actual Node failures, then perform Chromium portrait/landscape checks on the exact self-contained HTML artifact. Request UI 03 to commit 3–5 original PNGs with hashes. No merge/main write.


## 2026-11-18 — QA 05: verified exact-SHA CI evidence (Issue #8 / #20)

**One bounded task:** independently inspect GitHub Actions for QA PR #10 commit `42ec2ed154fcac3145ecabe6c43556bfcad990bd` and audit its pinned cross-branch revisions. No gameplay, Core contract or SaveService change.

**Verified GitHub Actions:** workflow `QA offline regression`, run [44198843105](https://github.com/rixed318/Probably-TAU/actions/runs/44198843105), conclusion **success**. All four jobs completed successfully: Python PCG32 oracle, UI/Content static+Node regressions (including `test-day01-android-touch.cjs`), Core .NET 8 golden suites, NPC .NET 8 golden assertions. This is **CI evidence on pinned source snapshots**, not a test of current feature branch heads or a Unity build. The workflow records exact SHAs and checks them; pinned UI `e09653994625adb6de312b2070b5e7745fc7cd83`, Content `c2d9ba267549c4e1a404fd6fa68a7d1037566597`, Core `0f5a85ef1136fec0cbc0285568b11985f0e7200c`, NPC `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`. Current heads have advanced, so these successes must not be represented as latest-HEAD passes. Node touch tests are **not Chromium emulation**.

**NOT RUN:** Chromium touch 360x800 and 800x360, real-device Android/APK, Unity Editor tests, binary PNG checks on actual submitted assets, integrated Save→Load. No independent local execution in this pass. No shared architecture approved.

### Сводка для координатора 00 — 2026-11-18

- **Current PR heads (read from GitHub):** #9 Core `912be59eff09`; #10 QA before this handoff `42ec2ed154fc`; #11 Content `34edb4fe9203`; #12 UI `133a85bcc673`; #14 ADR `ebdc4be28afd`; #16 NPC `f64514bce12d`. All open/draft; none merged. This handoff commit becomes the new QA head.
- **Blocker 1:** Exact-SHA CI passes refer to older pinned UI/Core/Content/NPC commits, not current PR heads; re-pin and re-run after controlled review.
- **Blocker 2:** No verified Chromium touch viewport/overflow/48dp inspection on latest single-file Day01, and no actual Android Unity APK.
- **Blocker 3:** Actual 3–5 original PNG binaries and complete integrated Save→Load verification remain unconfirmed; stable DTO boundary still requires Core/00 review.
- **Recommended next step:** Coordinator 00 review SHA drift and authorize QA-only CI pin refresh for the latest vetted revisions, then run real Chromium touch tests on the same exact UI SHA; do not claim Android APK readiness.

**Safety:** feature/save-6 only; no main push, merge, other-branch changes, IP approval or invented test results.


## 2026-11-18 — QA 05: refresh exact-SHA offline CI matrix (Issue #8)

**One bounded change:** updated only QA-owned `.github/workflows/qa-offline.yml` to replace stale pinned cross-branch revisions with independently fetched PR heads, without merging branches or changing game contracts. Workflow commit: `9f25c3b6873cb5f4eced014aaf9469a6a6db2716` on `feature/save-6`.

- Core PR #9: `5a57f3ae963002901595894b3aef9c4e5d993a16` (previous `0f5a85ef1136fec0cbc0285568b11985f0e7200c`).
- UI PR #12: `8364b9445d6ef7bfe04b1f758ebbbc27b150958a` (previous `e09653994625adb6de312b2070b5e7745fc7cd83`).
- Content PR #11: `a1646a221695e143b6f7b94006874ca7a704da8f` (previous `c2d9ba267549c4e1a404fd6fa68a7d1037566597`).
- NPC PR #16: `628f0e6fe4140abfedce2ad7b4636b5f25109287` (previous `d82bc6ee4bfc82d6bcfcccfd6c84760a5d58761c`).

**Evidence:** GitHub source fetch and PR-head metadata verified; PR #12 changed-file list contains 31 paths and **zero PNG paths**, so 3–5 binary art assets are still not independently verified. Prior QA SHA `cf515bd4895c28437d259e66cfc8909768fa4b91` has a completed successful workflow run `44211811079`, but this does **not** verify the new pinned matrix. **New CI execution: NOT YET VERIFIED**. Local Python/Node/.NET: NOT RUN in this pass. Chromium touch 360×800 and 800×360, Unity Editor, APK/Android, actual PNG binary validation and integrated Save→Load: NOT RUN. No new shared contracts or SaveService.

### Сводка для координатора 00 — 2026-11-18 06:40 +04

- **Latest reviewed heads:** main `d673322cd9d0`; #9 Core `5a57f3ae9630`; #10 QA workflow `9f25c3b6873c` (this handoff commit follows); #11 Content `a1646a221695`; #12 UI `8364b9445d6e`; #14 ADR `ebdc4be28afd`; #16 NPC `628f0e6fe414`; #17 ADR-C0-002 `bc9db999b419`. Gate C0-v2 and ADR remain feature-development guidance, not permission to merge.
- **Blocker 1:** newly refreshed exact-SHA CI run not yet verified; green historical CI is not evidence for current heads.
- **Blocker 2:** Android Chromium touch interactive acceptance and real Unity APK/device tests NOT RUN; latest UI single-file HTML has not been interactively verified.
- **Blocker 3:** PR #12 diff has zero PNG binaries; full snapshot DTO/ledger persistence and Save→Load not verified.
- **Recommended next step:** inspect the Actions run and individual logs for QA workflow SHA `9f25c3b6873c`; fix only reproducible QA-owned failures, then test the exact latest HTML in Chromium portrait/landscape and audit real PNGs when delivered. Keep main protected; no merge, no owner-test approval.


## 2026-11-18 — AutoDev 05: exact-SHA CI refresh (Issue #8)

**One bounded task:** update only pinned read-only source revisions in `.github/workflows/qa-offline.yml`, preserving checkout isolation, exact-SHA assertions, no credentials and no merge. Workflow commit: `d127602b7246e337708185589c459641f6a4334b` in `feature/save-6`.

Pinned source heads read from GitHub PRs this pass: Core #9 `75362a245ed4ec5a041308e820994ed7589d0a24`; UI #12 `f857f2871fbb097182ebc54dad6ca3e7caea6456`; Content #11 `488c33e7bdf562f9c99e521eb7e7dc6a6dd72df3`; NPC #16 `d2e022483c462000914806287c2e4c21647e8aeb`. QA PR #10 was `1f67e8e7820d9a5f35287bd369d174360b33b510` before this update; ADR #14 `ebdc4be28afd56e82bc14ca397af88376ae59d20` remains unmerged and only approved for feature development. Read `AGENTS.md`, `docs/CHAT_WORKFLOW.md`, QA handoff, PR #9/#10/#11/#12/#14/#16, Issues #6/#8/#20/#21.

**Execution evidence:** GitHub contents API accepted the workflow update. No local Node/Python/.NET commands executed in this pass. GitHub Actions for the new workflow commit: **NOT VERIFIED** (do not infer green status from previous runs). Chromium 360×800 / 800×360 touch, three documents, EndDay, ledger replay, 48dp/overflow: **NOT RUN**. Unity Editor, APK, physical Android: **NOT RUN**. Real PNG binary verification: **NOT RUN**. SaveService remains deferred pending stable complete DTO; no alternative GameState created.

### Сводка для координатора 00 — 2026-11-18

- **Latest PR/commits:** Core #9 `75362a24`; QA #10 workflow `d127602b` (handoff commit follows); Content #11 `488c33e7`; UI #12 `f857f287`; ADR #14 `ebdc4be2`; NPC #16 `d2e02248`.
- **Blocker 1:** new pinned CI run and actual test logs not yet verified.
- **Blocker 2:** Chromium touch portrait/landscape acceptance and real Unity APK/Android are NOT RUN.
- **Blocker 3:** 3–5 original PNG binaries and complete stable Core snapshot DTO not yet independently validated; no full Save→Load.
- **Recommended next step:** inspect Actions run for QA workflow SHA `d127602b`, report each job result and exact tested SHAs, then execute Chromium touch tests on exact UI HTML artifact. Keep main protected; no merge or readiness approval.


## 2026-11-18 — AutoDev 05: investigate failing exact-SHA CI (#8)

**One bounded task:** independently inspect the completed GitHub Actions run [44237151006](https://github.com/rixed318/Probably-TAU/actions/runs/44237151006) for QA branch SHA `5d797098a0ff86bf775ec00524c78912124b66ee`. This is a CI failure triage, not a code fix or a claim that all tests passed.

**Observed jobs (GitHub Actions actual logs):**
- Core ledger/snapshot .NET 8 — **SUCCESS**.
- Python PCG32 oracle — **SUCCESS**.
- UI/Content static regression — **FAILURE**. Earlier UI Node suites reported zero failures; the Android touch static assertion failed: `AssertionError: The input did not match the regular expression /env\\(safe-area-inset-top\\)/`. This is evidence of an unmet assertion on the pinned HTML/CSS artifact, not evidence of a Chromium touch test. UI owner should verify safe-area CSS and test expectations at the pinned UI SHA.
- NPC standalone .NET 8 — **FAILURE at checkout**: `/usr/bin/git` exit code 1 during fetch, after retry; NPC C# tests **NOT RUN**, so this is not an NPC regression verdict. Re-run checkout before diagnosing NPC logic.

**Execution:** only remote GitHub Actions jobs above were executed; this pass performed read-only GitHub metadata/log inspection. No local Node/Python/.NET, Chromium, Unity Editor, APK, Android device, or real-PNG binary tests were run. No shared C0/ADR contracts approved, no SaveService implemented.

### Сводка для координатора 00 — 2026-11-18 09:59 UTC+4

- **Latest PR heads observed:** #9 Core `2fd4f9e810f2`; #10 QA `5d797098a0ff` (this handoff commit follows); #11 Content `1ce07eda2f62`; #12 UI `d3f695073b2c`; #14 ADR `ebdc4be28afd`; #16 NPC `7c720f11617b`. All observed open. Main was read only.
- **Blocker 1:** CI run 44237151006 has UI touch assertion failure; cannot mark Android-ready or overall CI green.
- **Blocker 2:** NPC checkout failed before tests; latest NPC regression status unknown.
- **Blocker 3:** Chromium touch 360x800/800x360, actual 3–5 PNG binary/manifest checks, Unity APK/Android and integrated Save→Load remain unverified; stable persistence DTO gate still required.
- **Recommended next step:** UI 03 fix or justify the missing `env(safe-area-inset-top)` in the exact offline HTML artifact; retry NPC checkout separately; then QA re-run exact-SHA CI and actual Chromium touch checks. No main push/merge; READY_FOR_OWNER_TEST not granted.


## 2026-11-18 11:43 UTC+4 — QA 05: pin Android bundle regression to current reviewed UI SHA (#8, #20)

**One bounded change:** updated only the UI revision (both checkout ref and exact-SHA assertion) in `.github/workflows/qa-offline.yml` from `f857f2871fbb097182ebc54dad6ca3e7caea6456` to `7bfffdb59dccb2256e7d58196d95d5aa9bd3e6b3`. Workflow commit: `d56faf5af06dfa4932e25dcee1a8efee98295c44`. Core, Content and NPC pins intentionally unchanged; no cross-branch merge.

**Independent evidence:** inspected GitHub Actions run [44237151006](https://github.com/rixed318/Probably-TAU/actions/runs/44237151006): Core and PCG32 SUCCESS; UI/Content FAIL at Android bundle assertion `/env\\(safe-area-inset-top\\)/` on old UI SHA `f857f287`; NPC FAIL during checkout of `d2e02248`, before C# execution. Fetched current UI `test-day01-android-touch.cjs` (blob `224b544f`) and `day01-android-touch.html` (blob `28450454`) at `7bfffdb5`: source text contains exact `env(safe-area-inset-top)` and the test expects it. **This is a static source observation, NOT a successful Node or Chromium run.** The old failure must not be assigned to the new UI revision. A separate fresh CI run is required.

**Tests actually executed this pass:** GitHub Actions logs reviewed, current UI source fetched and inspected; no local Node/Python/.NET execution. Fresh CI at `d56faf5a` NOT VERIFIED. Chromium touch 360x800/800x360, 3 docs, EndDay, ledger replay, 48dp/overflow, actual PNG signatures/manifest SHA, Unity Editor/APK/Android and Save→Load all **NOT RUN**.

### Сводка для координатора 00 — 2026-11-18 11:43 UTC+4

- **PR heads read before change:** #9 Core `f22f2afbee30`; #10 QA `ad52349d6fb3` -> QA workflow `d56faf5af06d` (handoff commit follows); #11 Content `3311b3a14714`; #12 UI `7bfffdb59dcc`; #14 ADR `ebdc4be28afd`; #16 NPC `7723a66afe65`. C0-v2/ADR remain unmerged; no architecture approval by QA.
- **Blocker 1:** fresh exact-SHA UI Node/Android CI not yet verified; prior UI failure belongs to old pinned SHA.
- **Blocker 2:** NPC checkout of pinned `d2e02248` failed before tests; distinguish checkout failure from NPC logic, rerun with a valid exact SHA after review.
- **Blocker 3:** Chromium touch, real PNG, Unity APK/device and complete Save→Load still lack execution evidence; snapshot DTO stability remains a prerequisite.
- **Recommended next step:** inspect fresh QA Actions run for `d56faf5a`; if UI Android Node passes, run actual Chromium 360x800 and 800x360 against the exact bundled HTML, then independently verify real PNG binaries when delivered.
- **Safety:** only `feature/save-6` changed, no main push, no merge, no claims of READY_FOR_OWNER_TEST.


## 2026-11-18 — QA 05: exact-SHA UI regression refresh (#8, #20)

**One bounded change:** QA-owned `.github/workflows/qa-offline.yml` now pins UI PR #12 to `436f823d30d903341323a5e09fcaef9949e9ced3` in both the checkout and exact-SHA assertion, replacing `7bfffdb59dccb2256e7d58196d95d5aa9bd3e6b3`. Commit `a152f735945508d75f2a8d3c4f103ad420647ee3`, branch `feature/save-6`. No changes to UI or Core code. All other pins remain unchanged; this is **not** an integrated latest-branches test.

**Actual verification:** GitHub returned a successful file-update response with commit SHA. GitHub metadata for PR #12 confirms the new SHA is its head at the time of inspection. No new CI job result was obtained; Python, Node, .NET, Chromium touch (360x800 / 800x360), PNG binary checks, Unity Editor and Android APK all **NOT RUN in this pass**. The workflow change alone does not prove safe-area or Day01 behavior. SaveService remains deferred until stable Core DTO.

### Сводка для координатора 00 — 2026-11-18 13:15 UTC+4

- **PR heads observed before QA update:** #9 Core `6750501c88e4`; #10 QA `c3e9b17c5c73` → new workflow `a152f7359455` → this handoff commit; #11 Content `ab3d4d4fa708`; #12 UI `436f823d30d9`; #14 ADR `ebdc4be28afd`; #16 NPC `79d4fd54590b`. Main was not modified.
- **Blocker 1:** Fresh pinned UI Node CI execution not yet verified; no Chromium touch/48dp/overflow test evidence for latest single-file Day01.
- **Blocker 2:** Actual 3–5 PNG binaries, signatures, dimensions and manifest SHA have not been verified in this pass.
- **Blocker 3:** Unity Editor APK/device testing and complete Save→Load snapshot DTO integration remain unverified; Unity/Android tests NOT RUN.
- **Recommended next step:** inspect GitHub Actions run for the new QA head and collect exact test logs; run real Chromium touch acceptance on the same UI SHA; then binary-validate committed PNGs once supplied. Do not grant READY_FOR_OWNER_TEST or merge without evidence and owner approval.


## 2026-11-18 14:55 UTC+4 — QA 05: CI run #44274302838 failure triage (#8, #20)

**One bounded task:** independently inspect the actual GitHub Actions job/step results and failure logs for QA PR #10 head `7f34fe9cf1c06d0cfcd78f0e5b0a7b1cb1335cb0`. This is evidence collection only, not a code change or rerun.

**Run:** https://github.com/rixed318/Probably-TAU/actions/runs/44274302838 — completed **FAILURE** (3 jobs SUCCESS, 1 FAILURE):
- Python PCG32 oracle — **SUCCESS**; Core source audit step passed.
- UI/Content static regression — **SUCCESS**; Android touch Node static regression, offline entrypoint, gameplay, art manifest and content validation steps passed. **Not Chromium touch, APK, or real PNG binary proof.**
- Core ledger/snapshot .NET 8 — **SUCCESS**; golden-suite command passed on the workflow's pinned Core revision, not current Core PR HEAD.
- NPC standalone .NET 8 — **FAILURE during checkout** (job `134698386309`); `/usr/bin/git` exit code 1 at 09:16:05, 09:16:23 and 09:16:40 UTC in job logs. .NET install and NPC golden assertions were **SKIPPED / NOT RUN**. Do not label this an NPC logic failure. Verify pinned ref availability / checkout diagnostics before rerun.

**Execution scope:** GitHub metadata, jobs, steps and failure logs inspected. No local Python/Node/.NET, browser, Unity Editor, Android device/APK, or actual PNG binary tests run by QA in this pass. SaveService deferred pending stable approved Core snapshot DTO; C0-v2 PR #9 and ADR PR #14 are not merged.

### Сводка для координатора 00 — 2026-11-18 14:55 UTC+4

- **PR heads checked:** #9 Core `db287d877c65`, #10 QA `7f34fe9cf1c0` (handoff update commit follows), #11 Content `71eceb4f1d03`, #12 UI `abaf9d1a788d`, #14 ADR `ebdc4be28afd`, #16 NPC `4fca66045950`. These are live heads, **not** the exact SHAs exercised by the pinned CI checkout steps. All PRs open at inspection. No main push or merge.
- **Blocker 1:** NPC pinned checkout fails before .NET execution; run #44274302838 overall FAILURE despite 3 passing jobs.
- **Blocker 2:** Latest UI PR head `abaf9d1a788d` and latest Core/Content/NPC heads are not established as tested by the pinned CI; Chromium 360×800 and 800×360 touch/48dp/overflow acceptance remains NOT RUN.
- **Blocker 3:** 3–5 actual PNG binary signature/dimensions/manifest SHA validation, Unity Editor APK/device testing, and complete Save→Load remain unverified.
- **Recommended next step:** inspect the exact NPC checkout ref in QA workflow and its existence; repair only QA-owned CI pin if stale, rerun exact-SHA CI, then perform Chromium touch acceptance on the latest single-file HTML. Do not grant READY_FOR_OWNER_TEST without evidence.
