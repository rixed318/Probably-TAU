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

## AutoDev follow-up — 2026-11-14 (N0/C0 alignment)
- **Completed:** reviewed current main governance and Issue #4, Core PR #9 C0-v2, ADR PR #14, Content PR #11 and NPC PR #16. Updated `docs/proposals/NPC_EVENT_SCHEDULING_N0.md` to withdraw the incompatible N0 sorting recommendation and propose C0-v2-compatible priority/instanceId sorting and a due-stage snapshot after project ticks.
- **Commit:** `98f44ffbe11b18ff590a56ae8ee543cb814e548d` on `feature/npc-4`; PR #16: https://github.com/rixed318/Probably-TAU/pull/16; coordination comment: https://github.com/rixed318/Probably-TAU/pull/16#issuecomment-6880587468.
- **Checks:** manual comparison of N0 proposal with C0-v2 and ADR-C0-001; no executable source changed. **NOT RUN:** C# compilation, Unity tests, event-scheduler integration tests. No claim of successful runtime validation.
- **BLOCKED:** executable EventDirector awaits Core 01's confirmation of stableEventId (= instanceId?), priority ownership/default, snapshot boundary and child due<=n policy; typed Content predicates remain outstanding. No alternate Core/GameState/ledger created.
- **Next bounded increment:** independent read-only manifest validator for event IDs, dayIndex, feeds and phase aliases (must not sort or execute events); request Content 04 review. No merge to main.


## AutoDev 02 — ADR-C0-002 reconciliation + typed AST review (2026-11-14)

**One scoped task completed:** updated `docs/proposals/NPC_EVENT_SCHEDULING_N0.md` to match coordinator-approved-for-feature-work ADR-C0-002 in Draft PR #17. N0 now consistently uses `dueDayIndex ASC → priority DESC → instanceId ASC` (ordinal), persisted Int32 priority defaulted once to 0, due-stage snapshot after project ticks, and atomic rejection of new children due<=closing day. Removed stale advice to sort E02/E03 by creationOrdinal. Existing overdue items remain eligible; E09 is report-only.

**Review of Content PR #11:** typed AST (`always/all/any/flagEquals/factExists/dayIndexAtLeast`) is suitable for a pure, read-only evaluator, not a runtime-approved wire schema. Partner income must be gated by durable `E03_mentor_paid`, `E07_delivered`, DayIndex>=4 and a unique earned receipt. **Risk to resolve with 01/04:** both onsite/remote branches share `partner/{sourceInstanceId}/delivery` earnedId template; if each branch uses a distinct source instance, exactly-once revenue is not guaranteed. Require a canonical agreement source ID and Core ledger deduplication before wiring.

**GitHub commit:** `d761ccaa03b848a29513c907672b63e50af69d17` (N0 documentation). This handoff is committed separately on the same `feature/npc-4` branch. PR #16 stays Draft; no merge/main changes.

**Checks performed:** manual source-to-source comparison of N0, ADR-C0-002, Content event hooks, and Core GameStateSnapshotDto retrieved via GitHub. **NOT RUN:** C# compile, unit tests, Unity Editor, EventDirector integration; this task changed documentation only.

**BLOCKED for EventDirector:** concrete Core scheduled-event DTO/transaction ports and receipt APIs are missing. **Next bounded task:** independent pure-C# typed condition evaluator + deterministic negative tests (no GameState, ledger, RNG, scheduler), with actual C# execution only when a runner is available.


## AutoDev 02 — pure typed condition evaluator (2026-11-14, 23:39 UTC+4)

**One scoped task (#4):** implemented an independent pure C# condition AST and deterministic read-only evaluator matching the six operators currently proposed in Content PR #11: `always`, `all`, `any`, `flagEquals`, `factExists`, `dayIndexAtLeast`. This is **not** a runtime JSON adapter and does not claim the draft schema is approved. Code: `Runtime/Npc/EventConditionEvaluator.cs`; standalone tests: `Tests/Npc/EventConditionGoldenTests.cs`.

**Commits:** implementation `de5e5e9ddd410531499501f9b0f786cc8e0b5765`; tests `9dfd666f57f45a99aabd0db462e2a3d381889389`; handoff commit follows. All on `feature/npc-4`; PR #16 stays Draft. No main changes or merges.

**Behavior:** exact ordinal flag comparisons, missing flags/facts evaluate false, nonempty all/any groups, invalid/negative day or null context rejected, short-circuit boolean groups, no RNG, GameState, ledger, scheduler or Unity dependency. Factory methods enforce valid AST nodes; caller supplies read-only state facts/flags. The evaluator does not generate or mutate receipts, earned income, or event IDs.

**Tests:** added 16 standalone assertions including partner onsite eligible at DayIndex 4 only after both facts, remote exclusion, case sensitivity, missing fact, invalid nodes and negative day. **NOT RUN**: C# compile, golden tests and Unity EditMode (no dotnet/csc/mcs available in this execution environment). Source creation via GitHub confirmed; no claim of passing C# tests. Run `EventConditionGoldenTests.RunAll()` via a real .NET test runner once QA/Core establishes one.

**Dependencies / BLOCKED:** Full EventDirector still requires Core scheduled-event instance/transaction/receipt API and canonical earned-income source key. JSON-to-AST parsing/validation is deliberately not included; coordinate schema with Content 04. ADR-C0-002 PR #17 remains unmerged and applies only to feature development. Next bounded task: add a standalone .NET test runner or JSON schema adapter once Core/QA test infrastructure and Content schema are confirmed.


## AutoDev 02 — standalone .NET 8 NPC golden test runner (2026-11-15)

**One bounded task (#4):** add a self-contained .NET 8 executable project that compiles the **real** existing NPC condition evaluator and its 16 standalone golden assertions, plus a minimal GitHub Actions job to run it on the NPC branch. No new gameplay logic, Core API, GameState, ledger, event queue or content data were modified.

**Files added:**
- `Tests/Npc/Standalone/NpcConditionGolden.csproj` — explicit compile links to `Runtime/Npc/EventConditionEvaluator.cs` and `Tests/Npc/EventConditionGoldenTests.cs`; net8.0 executable, warnings-as-errors, no external NuGet test framework.
- `Tests/Npc/Standalone/Program.cs` — invokes `EventConditionGoldenTests.RunAll()`, exits nonzero on exception and prints failure details.
- `.github/workflows/npc-conditions.yml` — read-only checkout, .NET 8 setup, `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` on feature branch / PR paths.

**Commits:** project `bd1a5aefca8064a4906273ac4368361b20ea7158`; runner `38410ee3986c8e0f036b9c25d8ac20edfa994785`; workflow `060ec23fe63f19461d9bf2fccf114e8af2e5f559`. Handoff update committed separately.

**Verification:** GitHub accepted all three file writes; source paths and referenced classes were read directly from feature/npc-4. **NOT RUN:** local dotnet/C# compile and tests (dotnet/csc/mcs unavailable); Unity tests. GitHub workflow result **NOT VERIFIED** at time of handoff: no associated PR-triggered run was returned for workflow commit. Do not claim passing assertions until GitHub Actions shows a successful run. Manual command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**BLOCKED for full EventDirector:** still no concrete Core scheduled-event instance/transaction/receipt API; canonical earned-income source ID needs Core/Content agreement. Independent condition evaluator and its test runner can proceed without those dependencies. PR #16 remains Draft; no merge/main changes.

**Next:** inspect GitHub Actions logs and fix any actual C# compile/test failures in NPC-owned files. Once green, request QA 05 to include the runner in integration CI; avoid duplicating Core's own test runner.


## AutoDev 02 — condition evaluator regression expansion (2026-11-15)

**One scoped task (#4):** expanded the existing `Tests/Npc/EventConditionGoldenTests.cs` without changing runtime logic or adding dependencies. Tests now cover nested all/any with the exact day-index boundary and int.MaxValue, defensive copying of AST child arrays and read-only exposed children, no mutation of caller-provided facts/flags, null context rejection, null group rejection, and canonical flag identifier/value validation. Source commit: `45e04bfb4b5aa369bbd3aad6e8d64ae05262e079`, on `feature/npc-4`.

**Baseline verified:** the preceding GitHub Actions NPC condition golden tests run `43642594836` completed **success** on previous branch head `308e6053`; it ran actual .NET 8 C# code. This does **not** prove the new regression assertions pass. A new run on the updated head must be checked and reported separately.

**Dependencies:** Core PR #9 still exposes only a partial GameState snapshot (pending/completed string IDs), not a concrete event-instance DTO, atomic command/ledger ports or EventDirector adapter. Content PR #11 has draft typed predicates and canonical partner agreement source ID, but runtime schema integration is still pending. ADR-C0-002 in PR #17 governs ordering for feature implementation only. No Core/GameState/ledger/scheduler edits, no main merge.

**Next bounded task:** if the new GitHub CI passes, consider a separate deterministic read-only Content AST adapter/validator with explicit failure on unknown op/shape; do not create an alternative scheduler.


## AutoDev 02 — strict draft JSON condition adapter (2026-11-15)

**One bounded task (#4):** added `Runtime/Npc/EventConditionJsonAdapter.cs`, a read-only strict adapter from Content PR #11's *condition* JSON AST into the existing independent `EventCondition` model. Only `always/all/any/flagEquals/factExists/dayIndexAtLeast` are accepted. Unknown/duplicate/missing fields, invalid numeric day, malformed groups, null values and depth >=32 are rejected; ordinal comparisons remain in the existing evaluator. Does not read effects, schedule events, post ledger entries or mutate GameState. This adapter uses .NET 8 `System.Text.Json` for standalone testing and is **not approved as a Unity runtime serializer**.

**Commits:** implementation `fdaba518b4b18b4092ce40290bb99078d95cba5c`; new golden assertions `4d622892c13813c3c6d952586f3dd0d7661a0032`; standalone project update `ff210b7d195e666448020fcfc0c62c3f9c367f64`; runner update `3c48999b7500d12733a75462997b338ad4a0caaa`. Handoff update follows.

**Verification:** prior NPC workflow run `43651213848` completed success for prior head `5396808a`; this does not validate the new adapter. The updated `.csproj` links the new source and tests and the runner invokes both suites. **Local dotnet/csc/mcs NOT AVAILABLE, new CI outcome pending verification** at time of this handoff; do not claim a green test until new HEAD workflow is checked. Command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**Dependencies / BLOCKED:** concrete Core scheduled-instance DTO, command transaction and receipt ports still absent in Core PR #9, so no EventDirector runtime integration. Content PR #11 typed AST remains draft; earned-income source identity must be canonical and Core-owned. ADR-C0-002 PR #17 is feature-development-only, unmerged. Next: verify workflow and fix any failing assertions; then seek Content/QA review before wiring any JSON in Unity. No main changes, no merge.


## AutoDev 02 — fix failed JSON adapter CI (2026-11-15)

**One scoped task (#4):** investigated real failing GitHub Actions run [43659607524](https://github.com/rixed318/Probably-TAU/actions/runs/43659607524) on PR #16. The .NET 8 project compiled, but the deeply nested JSON negative test expected `FormatException` and received `System.Text.Json.JsonReaderException` from the JSON parser's own default 64-depth guard *before* the adapter's 32-depth guard. This is a real red test, not an infrastructure outage.

**Fix:** `Runtime/Npc/EventConditionJsonAdapter.cs` now catches `JsonException` from `JsonDocument.Parse` / read and wraps it in `FormatException` (with original exception preserved as InnerException), keeping invalid JSON and over-deep input on the documented adapter error contract. No gameplay semantics, content, Core, money, RNG or scheduler changed. **Source commit:** `a06cad058df28e3a8b2d5864c00071ade1f033e8`.

**Verification:** prior run 43659607524: **FAIL** in `EventConditionJsonGoldenTests.RunAll` (real log). New .NET 8 workflow for this fix: check run status before declaring pass. Local C# execution NOT RUN; Unity EditMode NOT RUN. Test command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**BLOCKED for full EventDirector:** concrete Core event-instance, atomic transaction, receipt API still absent in Core PR #9. ADR-C0-002 PR #17 applies to feature implementation, not merged main. PR #16 remains Draft; no merge.


## AutoDev 02 — malformed JSON exception regression (#4, 2026-11-15)

**Scoped task:** add regression coverage for the parser-level `JsonException` → public `FormatException` contract fixed in commit `a06cad05`. Added three negative fixtures to `Tests/Npc/EventConditionJsonGoldenTests.cs`: truncated JSON, trailing garbage, and incomplete property value. Each asserts a `FormatException` with `System.Text.Json.JsonException` as `InnerException`. No production behavior, Core state, scheduling, ledger, content, or Unity runtime was changed.

**Commits:** `8a7d9a71c3415a8bd7b88986f729984c605f3956` (test additions), `50868075dd1d597134ba0ae1868ab12d94dd2d2b` (correct C# fixture escaping); this handoff follows as a separate commit. PR #16 remains Draft.

**Evidence:** the preceding post-fix .NET workflow for head `49ce22ee`, run [43667185679](https://github.com/rixed318/Probably-TAU/actions/runs/43667185679), completed **success**. It does **not** validate these newly added tests. Current new-head .NET CI outcome: **NOT VERIFIED** until checked on the final handoff commit. No local dotnet/Unity execution in this run. Manual command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**Dependencies / BLOCKED:** EventDirector requires Core-owned event instance DTO, command transaction and receipt APIs; the current `GameStateSnapshotDto` still only has pending/completed ID lists. ADR-C0-002 PR #17 remains feature-only/unmerged. No merge or main writes.


## AutoDev 02 — partner condition regression (2026-11-15)

**One bounded task (#4):** extended `Tests/Npc/EventConditionJsonGoldenTests.cs` with regression assertions for Content PR #11's mutually exclusive onsite/remote partner conditions, ordinal/case-sensitive flag matching, DayIndex=3 vs 4, Int32.MaxValue day boundary, and nested duplicate JSON field rejection. This does not post money or create event instances; it only checks the existing pure JSON condition adapter. No changes to runtime implementation or Core.

**Commits:** `0c0d283a5b86f4301ed4f3a2263afcfde6a6fc58` tests, `38f020f62387ee894e1cc35342faec8be6bc3368` C# JSON literal escaping correction; this handoff committed separately. All writes in `feature/npc-4`, PR #16 Draft; no main/merge operations.

**Prior verified CI:** NPC workflow run `43675160835` for prior head `a559ca65` concluded `success`. **Current tests NOT YET VERIFIED:** source inspected after the escaping fix, but no .NET execution result for the new head at handoff. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` and inspect associated GitHub Actions. Unity EditMode NOT RUN.

**Dependencies:** Full EventDirector remains blocked by concrete Core scheduled-instance DTO, atomic transaction/receipt APIs, and integration of canonical partner earnedId source. ADR-C0-002 PR #17 applies to feature work but is not merged. Next step: verify new CI, then consider independent content fixture adapter validation without inventing an EventDirector.

## AutoDev 02 — Unicode JSON field alias regressions (2026-11-15)

**Scope:** one independent regression increment for the strict Content JSON condition adapter. Added four cases to `Tests/Npc/EventConditionJsonGoldenTests.cs`: escaped Unicode alias for `op` must be rejected as duplicate, case-variant `Op` must be rejected as unknown, escaped `factId` alias must be rejected as duplicate, and a single escaped `op` key must parse correctly. No runtime, Core, GameState, ledger, scheduler, or Content changes.

**Commits:** `c18d71872a2523af8fefea4cef1c727cd1e6d82a` initial test addition; `c68b293e3f0ba3de53d78b45d053d7398afc0134` corrected C# JSON escaping; this handoff commit follows. Branch `feature/npc-4`, PR #16 Draft. Previous HEAD `3e769292` had GitHub Actions run `43682379205` success, **but that does not validate the new tests**. Current new-HEAD CI pending confirmation; Unity Editor not run.

**Dependencies:** EventDirector still BLOCKED on Core event instance/atomic command/receipt APIs. ADR-C0-002 PR #17 governs due-order for feature development; no merge to main.


## AutoDev 02 — missing-field JSON contract regression (2026-11-15)

**One scoped task (#4):** normalized missing required `op`, `key`, `value`, `factId` fields in the independent Content condition adapter to `FormatException` instead of leaking `KeyNotFoundException`. Added seven negative golden cases, including absent `dayIndexAtLeast.value` and `all.items` (already covered by the existing field guard). No new scheduler, GameState, ledger, event mutation, or content schema approval.

**Commits:** implementation `d28e4afb31ea4c0c3f596c2a24f18548d9d31e74`; tests `d59e61fc3f18870ca626576cf7a0ed02c2bcc65b`; corrected test escaping `0f5cc9464737f554ac963777102991ddc468840b`. All on `feature/npc-4`, PR #16 remains Draft. Prior head `3f13fea1` had GitHub Actions success #43689843236; this **does not validate the new commits**. New CI must be checked separately; no local Unity or .NET execution claimed.

**Dependencies / BLOCKED for EventDirector:** Core 01 must supply compatible event instance/queue DTOs, atomic transaction and receipt APIs; Content 04 must finalize typed AST and canonical earnedId/source identity. ADR-C0-002 PR #17 is feature-only; never merge without owner permission.


## AutoDev 02 — reject escaped JSON control characters (2026-11-15)

**One scoped task (#4):** harden the independent draft condition JSON adapter against control characters inside persisted condition identifiers/values (e.g. JSON-escaped newline and tab). `ReadString` now rejects Unicode control characters as `FormatException`; two golden negative fixtures cover `factId` and `flagEquals.value`. Also normalize unexpected AST factory `ArgumentException` to adapter `FormatException` with preserved cause. No Core/GameState/ledger/queue/scheduler modifications.

**Commits on `feature/npc-4`:** `d5e931b0` exception boundary, `22b45461` initial regression fixtures, `9cf38436` explicit control-character validation, `81e8777f` corrected final fixtures. This handoff is a separate commit. PR #16 remains Draft.

**Validation:** manually inspected adapter, immutable AST factory and final JSON fixture escaping through GitHub. **NOT RUN:** local .NET/C# tests, Unity EditMode; new-head GitHub Actions result must be checked before claiming pass. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**BLOCKED for EventDirector:** Core's current `GameStateSnapshotDto` still exposes pending/completed event ID lists but not concrete scheduled instance/atomic transaction/receipt APIs; Content's typed AST and earnedId/source identity remain draft. ADR-C0-002 in PR #17 is feature-development-only. No main writes or merge.


## AutoDev 02 — ordinal IDs independent of caller collection comparer (2026-11-15)

**One scoped fix (#4):** the read-only condition evaluator previously delegated flag-key and fact-ID matching to the caller's dictionary/set comparer. An OrdinalIgnoreCase collection could silently make `partner` match `PARTNER` or `e03_mentor_paid` match `E03_mentor_paid`, contradicting the Content typed AST's exact identifier semantics. Evaluator now enumerates the read-only collections and compares keys/IDs with `StringComparison.Ordinal`; flag values remain ordinal. Four golden assertions cover exact and mismatched casing with intentionally case-insensitive input containers.

**Commits:** runtime `15042f07b1acebc67379bbd3dd7d34bf9de01abc`; tests `0f7a5ed514d8d56fcdb1d82deab1fb0e910ebf95`; handoff update is a separate commit. Branch `feature/npc-4`, PR #16 Draft. No main writes/merge.

**Validation:** manually inspected the exact source change and test assertions. Previous head `ddaf4a0c` passed GitHub Actions run [43704983083](https://github.com/rixed318/Probably-TAU/actions/runs/43704983083), but it does NOT validate this fix. New-head .NET workflow and Unity Editor results are **NOT VERIFIED** at handoff. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. Note linear scan of input collections is intentional to guarantee ordinal semantics independently of their comparer.

**BLOCKED:** EventDirector still needs Core scheduled instance DTO, atomic command/ledger transaction and receipt APIs, plus Content canonical earnedId source mapping. ADR-C0-002 PR #17 remains feature-only/unmerged.


## AutoDev 02 — direct AST control-character parity (2026-11-15)

**One scoped task (#4):** the direct `EventCondition` factory previously accepted embedded control characters even though the Content JSON adapter rejected them. `Required` now rejects `char.IsControl` characters in keys, fact IDs and flag values. Added three negative factory assertions and one positive ordinal fact assertion. This prevents bypassing the JSON adapter's canonical identifier rules. No Core, GameState, ledger, scheduler or Unity changes.

**Commits:** runtime `b549209c7e9afaf283bccac9afb6e6e25032a7cc`; tests `31f2e0529745d8f850dd14d02f2620452e63eadc`; handoff update committed separately on `feature/npc-4` (PR #16 Draft). **Validation:** GitHub source write verified; .NET/Unity tests NOT RUN locally; GitHub Actions for this head must be verified before claiming success. Command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**BLOCKED for EventDirector:** Core scheduled instance/atomic transaction/receipt APIs and Content canonical earnedId/source identity still require integration; ADR-C0-002 PR #17 is feature-only. No main write or merge.


## AutoDev 02 — nested boolean false-path regression (2026-11-15)

**One bounded task (#4):** added four independent C# golden assertions for `all`/`any` combinations with absent facts, mutually exclusive partner flags, and nested boolean groups. Tests-only change in `Tests/Npc/EventConditionGoldenTests.cs`; no runtime logic, Core, GameState, ledger or scheduler modified.

**Source commit:** `644be8a9bdb5f6c66762b106888e4e26ce502532` on `feature/npc-4`. This handoff is a separate commit; PR #16 remains Draft. No main write/merge.

**Verification:** preceding HEAD `9ffbf1ff7bbf024fd6636ecda17a08b6afb8b0d7` has GitHub Actions success run [43721725550](https://github.com/rixed318/Probably-TAU/actions/runs/43721725550), which **does not** validate the newly added assertions. Local .NET SDK and Unity Editor unavailable, so new tests **NOT RUN locally**; CI for new HEAD not yet verified. Runner command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.

**BLOCKED for full EventDirector:** concrete Core scheduled-instance, atomic transaction and receipt APIs; Content canonical earnedId/source identity. ADR-C0-002 PR #17 is approved only for feature implementation. Next: check new-head CI, then prioritize integration dependency resolution rather than expanding redundant condition tests.


## AutoDev 02 — 2026-11-15: escaped control JSON regression (#4)

- Scope: three standalone negative assertions in `Tests/Npc/EventConditionJsonGoldenTests.cs` for escaped NUL in `factId`, LF in `op`, and DEL in `value`; validates rejection after JSON decoding. No production API or Core changes.
- Commits: `366d26a621598f9089de4b0d2f02d167856af443` (initial assertions), `162d6f2a470f838fad7d5ed1f0069ccda85fa729` (corrected C# JSON literals); this handoff commit follows. Branch `feature/npc-4`, PR #16 remains unmerged.
- Validation: fetched changed test source from GitHub and manually inspected the corrected literals. NOT RUN: dotnet compile/golden assertions, Unity tests. CI for this HEAD not yet confirmed; no passing claim.
- Dependencies: EventDirector remains BLOCKED on Core scheduled occurrence/atomic transaction/receipt APIs and canonical Content earnedId. No GameState, ledger, scheduler or merge introduced.


## AutoDev 02 — 2026-11-15: UTF-16 identifier validation (#4)

- One bounded task: AST factories now reject unpaired high/low UTF-16 surrogates in condition IDs/values, while allowing correctly paired surrogate characters. This prevents invalid Unicode identifiers from bypassing JSON validation via direct C# construction. No Core, ledger, GameState or scheduler edits.
- Commits: `2de1d0c2ee3b0c6ce1165465d7ac7c57a231ee48` (implementation), `75980863c02c0ac16bf70773b183311c76606e34` (three regression assertions); this handoff commit follows. Branch `feature/npc-4`, PR #16 Draft; no main changes/merge.
- Validation: read back changed source from GitHub; **NOT RUN** .NET golden tests, Unity tests, or a new CI run. Passing status of this HEAD is unverified.
- Dependencies: EventDirector still BLOCKED on Core event-instance/atomic transaction/receipt ports and canonical Content `earnedId`. ADR-C0-002 PR #17 remains feature-only until owner-approved merge.


## P0 Issue #18 — immutable Day01 visitor mapping (2026-11-16)

**Scope:** immutable `VisitorArchetype` and `VisitorInstance`, plus deterministic `Day01Visitors.Create()` mapping three fictional visitors to Content PR #11 `DAY01_UI_FIXTURE.json`: `D01_LAB/npc_irina_korneeva/DOC-01`, `D01_STUDENT/npc_dasha_polynskaya/DOC-02`, `D01_PARTNER/npc_mark_sviridov/DOC-03`. Zero-based DayIndex=0. `VisitorInstance.InstanceId` is a *visit* ID, not Core's scheduled-event occurrence ID; do not reuse it as a receipt key. Pure .NET, no GameState/ledger/RNG/scheduler.

**Files:** `Runtime/Npc/VisitorModels.cs`, `Tests/Npc/VisitorIdentityGoldenTests.cs`, standalone csproj and Program runner. Tests cover exact fixture mapping, replay-stable IDs, fresh immutable objects, null archetype and negative day rejection. CI executes via `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. **NOT RUN locally:** dotnet unavailable; CI result must be checked for new HEAD before claiming PASS. Unity Editor NOT RUN.

**Cross-role contract:** Content fixture proposes `E03_mentor_paid` and `E07_delivered` as verified durable facts; producer must be Core-committed E03 mentor payment receipt and E07 delivery completion respectively, not UI flags. `partner/{sourceInstanceId}/delivery` uses original agreement occurrence's Core-stable instance ID; Content 04/Core 01 must confirm actual producer mapping and ownership before integration. No partner income on signature.

**BLOCKED:** full EventDirector needs concrete Core event-instance DTO, atomic transaction and receipt ports. This increment does not change Core state, balance or event scheduler. PR #16 remains Draft; no merge to main.


## AutoDev 02 — P0 provenance reconciliation and verified NPC CI (2026-11-16)

**Scoped task / Issue #18:** reconciled the immutable Day01 visitor mapping with Content 04's newly documented typed fact and earned-key provenance (Content handoff commit `f3d9950e9e607176e28e0af70abc6bb9566555be`, PR #11). This is a contract/handoff update only; no speculative Core adapter, ledger, scheduler or new GameState was added.

- `E03_mentor_paid` is permitted only after a **Core-confirmed durable mentor payment receipt** correlated to the original partner agreement occurrence; a signature or queued event is insufficient.
- `E07_delivered` is permitted only after **Core-confirmed contracted service delivery** for that **same** agreement occurrence; a narrative/UI choice is insufficient.
- `partner/{sourceInstanceId}/delivery` uses the **Core-assigned original agreement occurrence ID**. Never use visit `D01_PARTNER`, choice `PARTNER-1/2`, event definition `E07` or child-event instance ID as the earned source. Core alone owns durable exactly-once receipt/ledger posting. Signing is zero cash; E09 is report-only.
- **Producer/consumer integration remains BLOCKED:** Core 01 must expose concrete typed event-instance identity, verified payment/delivery receipts, atomic command/transaction ports and durable earned deduplication. NPC must not manufacture facts or revenue before those ports exist.

**Actual verification:** GitHub Actions NPC .NET 8 golden workflow [run 43759188365](https://github.com/rixed318/Probably-TAU/actions/runs/43759188365) reports **completed / success** for preceding NPC HEAD `89f2f04bce64a5d2817397826e3d2f206fd075d4` (including VisitorModels and golden tests). This is a workflow-level result, not a locally executed test log. Content handoff and PR #11 reply were read from GitHub. **NOT RUN this pass:** local dotnet, Unity, new integration/receipt tests, browser. No runtime code changed; CI for this documentation-only commit is not claimed.

**Next P0 criterion:** consume typed, Core-confirmed agreement occurrence and receipt APIs when actually available, then add a minimal adapter with one deterministic provenance test; until then keep EventDirector blocked. PR #16 remains Draft; no merge/main writes.


## AutoDev 02 — P0 canonical visit lookup (2026-11-16)

- **Scope / Issue #18:** added pure read-only `Day01Visitors.TryGetByVisitId` for UI/Content adapters to resolve the three canonical fictional visit IDs without a mutable visitor registry. Uses `StringComparison.Ordinal`; unknown, null and wrong-case IDs fail closed with null result. Existing factory, Core ownership, money and scheduler unchanged.
- **Tests added:** golden checks for all three exact ID→archetype→document mappings and unknown/case-mismatch/null rejection in `VisitorIdentityGoldenTests`. Execute `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. **NOT RUN locally** (SDK unavailable); CI result for this new commit must be verified separately. Unity NOT RUN.
- **Git commit:** this handoff, production method and golden assertions are in the same commit on `feature/npc-4` (see PR #16 head). No merge/main write.
- **BLOCKED:** EventDirector still requires Core transaction, scheduled occurrence, verified receipts and earned dedup ports. Provenance of E03/E07 and agreement source ID is documented in prior handoff; no duplicate financial logic introduced.
