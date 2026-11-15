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
