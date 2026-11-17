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
