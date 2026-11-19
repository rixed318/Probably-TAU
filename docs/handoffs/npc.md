# Chat 02 — NPC/EventDirector handoff

## AutoDev 02 — verification of expanded Day01 screen golden tests (2026-11-19)

**One bounded task:** verified the GitHub Actions result for the preceding regression additions on branch `feature/npc-4`, commit `bc39db86b2aa2b5c31c0733269d2d9d88bf85818`. [NPC golden workflow run #44583909347](https://github.com/rixed318/Probably-TAU/actions/runs/44583909347) returned `completed/success` for that SHA. This is evidence of the actual GitHub .NET golden workflow, not a Unity or Android player test. No production logic changed.

**Scope/dependencies:** Issue #4, Android parity #20, original planned art IDs #21. Existing Day01 document/art/inspection/choice routing tests remain read-only. C0-v2 PR #9, ADR PR #14 and Content PR #11 remain integration inputs. Full EventDirector still **BLOCKED** on a confirmed Core atomic event/receipt/earned-key port and Content E03/E07 origin agreement; do not create a second GameState or ledger. No image files created or uploaded.

**Tests:** prior SHA GitHub Actions `completed/success`; local .NET, Unity Editor/PlayMode, Windows player and Android device/APK **NOT RUN**. This documentation-only commit has no separately verified CI at writing. No main push/merge.


**Updated:** 2026-11-19. **Issues:** #4, #18. **Branch:** `feature/npc-4`. **PR:** #16 (Draft). **Status:** immutable Day01 visitor models, typed condition evaluator, read-only choice/document adapters and golden tests implemented; full EventDirector integration BLOCKED on Core atomic settlement/receipt/earned-key ports. Content typed predicates are implemented as a proposed adapter, not an approved runtime wire schema. Latest verified NPC .NET golden CI: `b96ba7ec15378d1d2e5c17800b427072835ac322` — [Actions #44545966216](https://github.com/rixed318/Probably-TAU/actions/runs/44545966216) `completed/success`; Unity/PlayMode and Android NOT RUN.

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

## AutoDev 02 — P0 document-to-visitor lookup (2026-11-16)

- **Scope / Issue #18:** added read-only `Day01Visitors.TryGetByDocumentId` so a document-focused UI can resolve DOC-01/02/03 to the canonical immutable visitor without maintaining a second registry. Ordinal matching; null, unknown and wrong-case document IDs fail closed. No GameState, money, ledger, scheduling or receipt logic.
- **Code commit:** `9eede9c9b97c158f0f2a83a190a24ecbe3a3c38f`; **test commit:** `c1727543fc1e9be265a6d41e6f217a50e07f9f5b`. This handoff is a separate documentation commit, all on `feature/npc-4`, PR #16 Draft.
- **Golden assertions added:** exact three document-to-visit/archetype mappings, unknown/wrong-case/null rejection. Run: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. **NOT RUN locally**; no .NET SDK or Unity Editor executed in this pass. Prior HEAD `5b4fe6d2` has successful GitHub Actions [43777579853](https://github.com/rixed318/Probably-TAU/actions/runs/43777579853), but this does **not** validate the new commits; current-head CI pending verification.
- **Integration dependency:** canonical content fixture in Content PR #11; verified Core receipts for `E03_mentor_paid` and `E07_delivered`, original agreement occurrence `sourceInstanceId`, atomic transaction and earned-income dedup remain required before EventDirector. No main push/merge.

## AutoDev 02 — P0 choice-to-visitor identity adapter (2026-11-16)

- **Scope / Issue #18:** pure C# `Day01Visitors.TryGetByChoiceId` resolves the nine canonical LAB/STUDENT/PARTNER choice IDs in Content PR #11's `DAY01_UI_FIXTURE.json` to immutable visit/document identities. Explicit ordinal switch rejects unknown, wrong-case, room-resolution and null IDs; never interprets prefixes or grants cash, eligibility, room reservation or deferred income. This is an identity lookup only, **not** a gameplay decision processor.
- **Files:** `Runtime/Npc/VisitorModels.cs`, `Tests/Npc/VisitorIdentityGoldenTests.cs`, this handoff. Golden assertions cover all nine choices and four rejection cases. Runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **Verification at commit creation:** source-level fixture cross-check; .NET/Unity tests **NOT RUN locally**, new-head GitHub Actions result not yet verified. Do not claim PASS until workflow confirms this exact commit. Branch `feature/npc-4`, PR #16 Draft, no merge/main changes.
- **BLOCKED for EventDirector:** Core-owned atomic transaction, scheduled occurrence and durable payment/delivery/earned receipts; Content facts `E03_mentor_paid`, `E07_delivered` and original agreement `sourceInstanceId` must be sourced from those verified Core ports. No alternate GameState or ledger.


## AutoDev 02 — P0 immutable choice inspection metadata (2026-11-16)

- **One bounded production increment (#18):** `Day01Visitors.TryDescribeChoice` returns an immutable `ChoiceDescriptor` with canonical visit and required inspection document. Exactly `LAB-2 → DOC-01` and `PARTNER-2 → DOC-03` require inspection in Content PR #11; other seven choices have no prerequisite. The existing `TryGetByChoiceId` delegates to the same mapping to prevent divergence. Unknown/room/wrong-case/null IDs fail closed. Metadata is **not** proof of inspection or Core authorization; no money, GameState, ledger or scheduler introduced.
- **Tests added:** exact two prerequisites, seven no-prerequisite cases, and negative IDs. Runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. **NOT RUN locally**; C# and Unity verification for this new HEAD require GitHub Actions/Editor confirmation. Manual source-to-Content fixture review performed.
- **Dependencies:** Content fixture PR #11 remains draft; changes to choice requirements require synchronized adapter update. EventDirector still BLOCKED on Core atomic transaction, event instance, verified E03/E07 receipts and earned dedup ports. No merge/main changes.


## AutoDev 02 — P0 CI verification and Core dependency gate (2026-11-16)

- **Issue #18 / PR #16:** verified the previous production increment `Day01Visitors.TryDescribeChoice` at exact commit `d6278f4b412b483adc7b433537b31e6027af5782`. GitHub Actions [NPC condition golden tests #43807241719](https://github.com/rixed318/Probably-TAU/actions/runs/43807241719) returned `completed/success` for that SHA. This validates the existing .NET golden runner at workflow level, including `VisitorIdentityGoldenTests`; no local .NET or Unity tests were run this pass.
- **Core contract inspection:** Core `feature/core-2` handoff now documents `ScheduledEventQueueSnapshot`, `DurableGameStateSnapshot` and `DurablePayablesSnapshot`. These are immutable snapshot/preparation slices, **not** an atomic `AdvanceDay` transaction/receipt/earned-key port. Do not bind NPC EventDirector to them as though settlement were implemented.
- **Current production status:** three canonical immutable Day01 visitors, document/choice lookups, and inspection metadata are implemented. No additional production code or tests were changed in this pass: adding another lookup would not advance the playable P0 criterion. Keep full EventDirector **BLOCKED** until Core exposes atomic event/receipt/earned-key APIs, and Content agreement occurrence provenance remains Core-confirmed. UI owns the temporary playable web gateway; NPC must not create a second gameplay state.
- **Validation:** read GitHub source and handoffs, checked exact-SHA Actions result. **NOT RUN:** local dotnet, Unity Editor/PlayMode, Core/NPC integration, end-to-end web demo. No main writes/merges.

## AutoDev 02 — shared Windows/Android Day01 read-only cards (2026-11-17)

- **Scope:** Issues #20/#4/#18; added `Day01VisitorCard` and `Day01Visitors.CreateReadOnlyCards()` in `Runtime/Npc/VisitorModels.cs`. These are immutable pure-C# presentation identity snapshots (visit ID, fictional NPC definition ID, document ID, role, DayIndex) from the same canonical Day01 factory used on Windows. No UnityEngine, platform branch, mutable GameState, ledger, scheduler or extra cash behavior.
- **One golden regression:** `VisitorIdentityGoldenTests` checks the three cards against canonical visit/archetype/document IDs, replay stability, independent immutable objects, and read-only collection semantics. Fixture cross-check: Content PR #11 `DAY01_UI_FIXTURE.json` (`D01_LAB/DOC-01`, `D01_STUDENT/DOC-02`, `D01_PARTNER/DOC-03`).
- **Verification:** manually reviewed C# mapping and Content JSON; local .NET/Unity/Android builds **NOT RUN** (SDK/Editor unavailable). GitHub Actions for this new commit must be checked before reporting pass. Commit SHA: see current PR #16 head (same atomic commit as code/test/handoff).
- **Art coordination:** Issue #21 belongs to UI 03. Portrait asset IDs/descriptions are not yet approved as a cross-role mapping; do not invent IDs or imply PNGs exist. Content 04 owns fictional character identities; request alignment before wiring portrait assets.
- **BLOCKED:** EventDirector still requires Core-owned atomic transaction/receipt/earned-key ports and confirmed `E03_mentor_paid` / `E07_delivered` facts tied to original agreement occurrence. No `main` merge.


## AutoDev 02 — Android read-only choice card adapter (2026-11-17)

- **Issue #20, one bounded increment:** `Day01Visitors.TryGetCardByChoiceId` maps all nine canonical Content Day01 choices to a fresh immutable `Day01VisitorCard` for Windows/Android UI. It does not authorize a decision, inspect documents, mutate Core, or issue money/receipts.
- **Files:** `Runtime/Npc/VisitorModels.cs`, `Tests/Npc/VisitorIdentityGoldenTests.cs`, this handoff. Golden assertions cover all nine choices and invalid/case-mismatched/null choices.
- **Validation:** source and fixture manually cross-checked; .NET and Unity/Android execution NOT RUN locally (SDK unavailable). GitHub Actions must be checked for this exact commit before claiming pass.
- **Dependencies:** UI 03/Content 04 must approve original portrait asset IDs under Issue #21; no image asset invented. EventDirector remains blocked on Core transaction/receipt ports and canonical partner earnedId; E03_mentor_paid/E07_delivered must originate from Core-confirmed facts.
- **Safety:** only feature/npc-4; Draft PR #16; no main merge, no alternate GameState/ledger.


## AutoDev 02 — planned Day01 portrait identity on Windows/Android (2026-11-17)

- **One bounded Issue #20/#21 increment:** `Day01VisitorCard.PlannedPortraitAssetId` exposes three read-only planned IDs exactly as Content 04's `Content/DAY01_ART_LINKS.json` (feature/content-7). The property explicitly does **not** attest that PNGs exist, that a UI manifest has been approved, or that assets are loaded. Same pure-C# DTO on Windows and Android; no engine fork, Core/GameState/ledger/receipts changes.
- **Regression:** `VisitorIdentityGoldenTests` checks each planned portrait ID and replay stability. Manually compared source IDs with Content JSON; **NOT RUN locally:** dotnet/Unity/Android (SDK unavailable). GitHub Actions status must be verified against this new SHA.
- **Coordination:** UI 03 owns actual PNGs and art manifest; Content 04 owns character/asset mapping. If draft IDs change, update this adapter in a reviewed PR. EventDirector remains **BLOCKED** on Core atomic event/receipt/earned-key ports and E03/E07 provenance.
- **Safety:** feature/npc-4 only; Draft PR #16; no merge/main writes.


## AutoDev 02 — planned blank document art IDs (2026-11-17)

- **Issue #20/#21, one bounded increment:** added `Day01VisitorCard.PlannedDocumentAssetId` in the existing Unity-free read-only DTO. Canonical mapping from Content 04 `Content/DAY01_ART_LINKS.json` (feature/content-7): `D01_LAB` → `doc01_procurement_blank`, `D01_STUDENT` → `doc02_student_proposal_blank`, `D01_PARTNER` → `doc03_partner_letter_blank`. These are planned art references, **not** proof of PNG existence; document text and money remain dynamic UI data.
- **Regression:** extended `VisitorIdentityGoldenTests` to assert exact planned document art IDs and stable snapshots on repeated creation, alongside portrait IDs. Source manually compared with Content art links; **NOT RUN locally:** .NET, Unity, Android (SDK/Editor unavailable). Check GitHub Actions for this commit before claiming green.
- **Ownership/dependencies:** UI 03 owns PNGs/manifest; Content 04 owns fictional identities and draft art links. EventDirector remains blocked until Core 01 exposes atomic event/receipt/earned-key ports and confirms E03/E07 provenance. No second GameState, ledger or engine fork. Only `feature/npc-4`; PR #16 stays Draft; no main merge.


## AutoDev 02 — planned Day01 standing sprite IDs (2026-11-17)

- **One bounded Issue #20/#21 increment:** `Day01VisitorCard.PlannedStandingAssetId` exposes the three draft `standingAssetId` strings from Content 04 `Content/DAY01_ART_LINKS.json`, on the same immutable Unity-free Windows/Android DTO. No asset binaries are claimed to exist and no gameplay state/ledger is modified.
- **Regression added:** `VisitorIdentityGoldenTests` checks exact IDs for three visitors and stable repeat snapshots. IDs manually compared to Content branch JSON. **NOT RUN:** local .NET (SDK absent), Unity, Android; check Actions against this commit.
- **Dependencies:** UI 03 owns original sprite PNGs and final art manifest; Content 04 owns mapping. EventDirector still blocked by Core transaction/receipt/earnedId ports and E03/E07 provenance. Only feature/npc-4; PR #16 Draft, no main merge.


## AutoDev 02 — document-tap card lookup (2026-11-17)

- **One bounded production increment (#20):** added pure C# `Day01Visitors.TryGetCardByDocumentId` for document-tap presentation on Windows and Android. Uses canonical `DOC-01/02/03` IDs, returns a fresh immutable `Day01VisitorCard`, fails closed for unknown/case-mismatched/null IDs; no Core state, ledger, Unity API or engine fork.
- **Golden assertions added:** all three document-to-card mappings plus three invalid inputs in `VisitorIdentityGoldenTests`. **NOT RUN locally:** .NET/Unity/Android (SDK unavailable); CI status must be checked for resulting commit.
- **Dependencies:** UI 03 owns document tap UI and actual PNGs; Content 04 owns canonical mappings. EventDirector still BLOCKED pending Core atomic transaction/receipt/earned-key ports and E03/E07 provenance. PR #16 Draft; no main merge.


## AutoDev 02 — canonical visit restore card (2026-11-17)

- **One scoped increment (#20):** added `Day01Visitors.TryGetCardByVisitId` to resolve persisted canonical Day01 visit IDs into fresh immutable, Unity-free `Day01VisitorCard` snapshots for Windows/Android navigation. It performs exact ordinal lookup, returns false/null for unknown IDs, and never authorizes decisions or changes Core state.
- **Golden test:** added three successful visit restore assertions and invalid/case-mismatched/null failure assertions to `VisitorIdentityGoldenTests.RunAll()`. Canonical Content fixture IDs `D01_LAB`, `D01_STUDENT`, `D01_PARTNER` checked against PR #11; art IDs remain planned only.
- **Validation:** source-to-fixture manual review; local .NET, Unity, Android tests **NOT RUN** in connector runtime. GitHub Actions result must be checked separately for the new SHA. One atomic commit includes runtime, test and handoff; see PR #16 commit history for SHA.
- **Dependencies:** EventDirector still BLOCKED pending Core atomic transaction/receipt/earnedId ports; Content 04 owns E03/E07 provenance; UI 03 owns actual PNG and art manifest. No second GameState/ledger, no main merge.


## AutoDev 02 — unified read-only choice preview (2026-11-17)

- **Issue #20, one bounded production increment:** added `Day01Visitors.TryGetChoicePreview` and immutable `ChoicePreview`, combining the canonical visitor card and inspection hint for a single Windows/Android presentation lookup. This is display metadata only: no eligibility decision, inspection completion, Core mutation, ledger or partner receipts.
- **Golden regression:** nine canonical choices must map to their visitor and exact Content inspection prerequisite (only LAB-2 → DOC-01 and PARTNER-2 → DOC-03); invalid/room/case-mismatched/null choices fail closed. Added to `VisitorIdentityGoldenTests.RunAll()`.
- **Verification:** manually compared `Content/DAY01_UI_FIXTURE.json` on `feature/content-7`; executable C#/.NET/Unity/Android tests NOT RUN locally (no SDK/Editor). Check CI for the exact commit before claiming pass.
- **Dependencies:** UI 03 owns real PNGs; Content 04 owns draft identities/art links and E03/E07 provenance. EventDirector remains BLOCKED on Core-owned atomic settlement/receipt/earned-key ports. Only `feature/npc-4`, Draft PR #16, no main merge.


## AutoDev 02 — inspection hint for cross-platform choice preview (2026-11-17)

- **Issue #20, one bounded increment:** added derived `ChoicePreview.RequiresInspection` to the existing immutable Day01 Windows/Android DTO. It is display-only, computed from `RequiredInspectionDocumentId`, and does not attest inspection completion or authorize a Core command.
- **Regression added:** all nine canonical Day01 choices must expose the expected boolean (true only for LAB-2 and PARTNER-2) and the unchanged inspection document IDs. No second GameState, ledger, Android fork, or asset claim.
- **Validation:** source reviewed against Content PR #11. Local .NET/Unity/Android tests NOT RUN in connector runtime; check GitHub Actions for the exact commit. EventDirector BLOCKED on Core atomic receipt/earnedId transaction ports and Content E03/E07 provenance. PR #16 Draft; no main merge.


## AutoDev 02 — visitor definition card lookup (2026-11-17)

**One bounded increment (#20, #4):** added `Day01Visitors.TryGetCardByVisitorId(string, out Day01VisitorCard)` to resolve the three canonical Content `visitorId` values into fresh immutable, Unity-free Windows/Android cards. It uses ordinal exact matching and fails closed for unknown, empty, or null IDs. This is read-only navigation, not command eligibility, event settlement or proof that PNG art exists.

**Production commit:** `0df59a9d3aa10543fb20a3dbc94f22e8a63d7dac`. **Golden assertion commit:** `d1865956bc0c238e920831b6ac8356389f6c82a3`. Tests added for three valid IDs, canonical card identity and invalid IDs. **NOT RUN:** local dotnet/Unity/Android execution; CI for the new SHA not yet verified. Source mapping manually compared with `Content/DAY01_UI_FIXTURE.json` from Content PR #11.

**BLOCKED:** Full EventDirector still requires concrete Core atomic event transaction/receipt and earned-key deduplication ports; E03/E07 provenance and partner agreement source ID require Core/Content alignment. Art IDs are planned only; real PNGs belong to UI #21. No changes to main, no merge. PR #16 remains review-gated.


## AutoDev 02 — planned art ID reverse lookup (2026-11-17)

- **One bounded production increment (#20/#21):** added `Day01Visitors.TryGetCardByPlannedAssetId` for the nine canonical portrait, standing and document-blank IDs in Content 04's `Content/DAY01_ART_LINKS.json`. It returns a fresh immutable `Day01VisitorCard` on Windows/Android without UnityEngine or mutable gameplay state. Exact ordinal match; unknown/null/case-mismatched IDs fail closed. Planned IDs do **not** prove PNGs exist.
- **Production commit:** `2f96c437aaea6d7c2af21a73aa9be73e01464b31`; **golden assertions commit:** `16146738f026a47632ca37de0d48834a556126f7`. Added 9 positive mappings, four invalid cases. Source IDs reviewed against Content's draft art links. **NOT RUN locally:** .NET/Unity/Android executable tests; GitHub Actions must be checked against test SHA before claiming pass.
- **Ownership:** Content 04 owns final canonical art mapping; UI 03 owns original PNGs/manifest and must verify assets exist before displaying. EventDirector remains BLOCKED on Core atomic transaction/receipt/earned-key API and E03/E07 provenance. PR #16 Draft; no main merge.


## AutoDev 02 — ordered Day01 choice menu for Windows/Android (2026-11-17)

- **Scope (#4/#20):** Added pure C# `Day01Visitors.TryGetChoiceIdsByVisitId` to expose three canonical choice IDs per Day01 visit, preserving Content 04 fixture order. Returns an `IReadOnlyList<string>` backed by `Array.AsReadOnly`; rejects unknown/case-mismatched/null visit IDs. Presentation metadata only; no eligibility checks, command authorization, UnityEngine, mutable GameState, ledger, or Android fork.
- **Commits:** production `ae8065a91c18601e7e53eda71dd7102fdc417317`; golden assertions `1c2b292cecff12984a57e43bbadaca4b81591400`; test import fix `5b2334dfd75b2d0231067688f42821ad23062a05`. Handoff commit follows separately.
- **Verification:** manual source review against `Content/DAY01_UI_FIXTURE.json` in `feature/content-7`; assertions cover all nine IDs/order, read-only collection type, and three invalid inputs. Executable local .NET, Unity and Android tests **NOT RUN**; CI must be verified for exact final head. Test entry: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **Dependencies / BLOCKED:** full EventDirector still needs Core-owned atomic transaction/receipt and earned-key deduplication ports; Content/Core must confirm E03/E07 provenance. UI 03 owns real original PNGs, planned asset IDs do not prove files exist. PR #16 remains Draft; no main push/merge.


## AutoDev 02 — immutable Day01 choice preview menus (2026-11-17)

- **One scoped production increment (#4/#20):** added `Day01Visitors.TryGetChoicePreviewsByVisitId(string, out IReadOnlyList<ChoicePreview>)`. Windows/Android UI receives ordered, immutable previews for each of three Day01 visitors; `LAB-2` and `PARTNER-2` retain inspection hints. This is read-only UI data, not Core authorization or inspection completion.
- **Canonical source:** Content PR #11 Day01 IDs, with art-link coordination from `Content/DAY01_ART_LINKS.json` on `feature/content-7`. No PNGs created, no branding/IP approval claimed.
- **Production SHA:** `1d8c7fda51c44627bd3313a44c7b9917da943460`. **Test-source SHA:** `5b18d3ea1711f4d4f6d11e2d22d1790aed571e03`. PR #16 remains Draft; handoff SHA is the commit updating this file.
- **Verification:** manually compared nine choice IDs and inspection mapping against existing fixture-based NPC definitions. Added golden assertions for three ordered menus, read-only wrappers and invalid IDs. **NOT RUN:** .NET execution, Unity Editor/PlayMode, Android build/device test. No successful CI asserted for these SHAs.
- **Dependencies/BLOCKED:** full EventDirector still needs Core-owned atomic transaction/receipt/earned-key APIs, agreement occurrence identity, and Content/01 confirmation of E03/E07 provenance. C0-v2 PR #9 and ADR PR #14/#17 remain Draft; no GameState/ledger fork, no main push/merge.


## AutoDev 02 — visit-scoped choice preview guard (2026-11-17)

- **One bounded production increment (#4/#20):** `Day01Visitors.TryGetChoicePreviewForVisit(visitId, choiceId, out preview)` prevents a UI menu for one visitor from resolving another visitor's choice. Exact ordinal Content IDs; immutable presentation snapshot only, not Core command eligibility or proof of inspection. Same pure C# behavior for Windows and Android.
- **Golden assertions:** nine valid visit-choice pairs, nine cross-visitor rejections, and null/case-mismatch/unknown combinations. Added to `VisitorIdentityGoldenTests.RunAll()`. **NOT RUN:** local .NET, Unity and Android tests; GitHub Actions must be inspected for the exact HEAD SHA.
- **Dependencies/BLOCKED:** Core atomic settlement/receipt/earnedId APIs and E03/E07 fact provenance remain unresolved; no EventDirector integration. Content 04 owns canonical IDs, UI 03 owns original art assets. No second GameState/ledger, no main push/merge.

## AutoDev 02 — document-scoped choice routing (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetChoicePreviewForDocument(documentId, choiceId, out preview)` so Windows/Android document views cannot resolve another document's Day01 choice. Exact ordinal Content fixture IDs; immutable UI preview only, never a Core command, inspection receipt or economic mutation.
- **Production SHA:** `aa15f108e4f388b13c807599b966c3c66a05ac78`. **Golden assertion SHA:** `41517facf0c1ca82fa2fb969c33634cf627c9bc9`. Cases added: 9 valid document/choice pairs, 9 cross-document rejections, null/case-mismatch/unknown choice/document failures. Final handoff commit follows.
- **Verification:** source and fixture manually compared. Executable .NET/Unity/Android tests **NOT RUN locally**; check exact SHA CI before claiming success. Command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **Dependencies / BLOCKED:** EventDirector still needs Core-owned atomic transaction/receipt and earned-key deduplication API; Content/Core must confirm E03/E07 fact provenance and source agreement occurrence. C0-v2 PR #9 and ADR PR #14/#17 remain unmerged. UI 03 owns real PNGs; no image generated. PR #16 stays Draft, no main push/merge.

## AutoDev 02 — document-scoped immutable choice menus (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetChoicePreviewsByDocumentId(documentId, out IReadOnlyList<ChoicePreview>)`. It resolves the canonical Day01 document to the existing ordered visitor menu, preserving Content choice IDs and inspection hints for Windows/Android. Returns fresh read-only snapshots; unknown/null/case-mismatched document IDs fail closed. This is presentation routing only, not eligibility, receipt, inspection completion or Core state mutation.
- **Production SHA:** `c7fdee1f30703acf50508ec4eb7c992ab52211cc`. **Golden test-source SHA:** `d920c6eb6e5f0745d2da787f2677a3dfbcf6ddd7`. Tests cover three document menus, all nine choice mappings, inspection hints, read-only collection wrappers and three invalid document IDs.
- **Verification:** manually checked against existing canonical visit menus and Content fixture mappings. **NOT RUN locally:** .NET golden runner, Unity EditMode/PlayMode or Android device/build. No CI success claimed for this increment. Test command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **Dependencies/BLOCKED:** EventDirector still requires concrete Core-owned atomic transaction/receipt and earned-key deduplication ports; Content/Core must agree on E03/E07 provenance. C0-v2 PR #9 and ADR PR #14/#17 remain review-gated. Content 04 owns canonical IDs; UI 03 owns original portrait PNGs (#21), and planned art IDs do not prove asset existence. PR #16 remains Draft. No main push, merge, new GameState/ledger or third-party IP approval.


## AutoDev 02 — Day01 triple identity guard (2026-11-18)

**One bounded increment, Issues #4/#20:** added `Day01Visitors.TryGetChoicePreviewForVisitDocument(visitId, documentId, choiceId, out preview)` to reject stale/mismatched Windows/Android UI selections when both the visit and document must match the canonical choice. Read-only DTO only; never an eligibility, inspection or Core authorization check. No new GameState, ledger, RNG, EventDirector or art.

**Commits:** production `9d11c60caa92b45ebe6d367def2c8cc61fe62a51`; golden assertions `ad18c6d05b31badb3e26f8ebf1b53a1752f199e8`. Assertions cover nine valid triples, mismatched visit/document and null/case-invalid IDs.

**Verification:** source edits accepted by GitHub; executable .NET, Unity and Android tests **NOT RUN** during this update. CI outcome must be checked against the final HEAD; assertions added are not evidence of passing tests.

**Dependencies:** PR #9 C0-v2 and PR #14 ADR apply; PR #11 canonical Content fixture IDs remain the source of truth. Full EventDirector remains BLOCKED pending Core atomic settlement/receipt/earned-key APIs and Content E03/E07 provenance. Issue #21 art IDs are only planned links; no asset existence asserted. PR #16 remains Draft; no main push or merge.

## AutoDev 02 — canonical Day01 inspection previews (2026-11-18)

- **One bounded increment (#4/#20):** `Day01Visitors.TryGetInspectionPreviewByDocumentId` returns immutable, Unity-free document/inspection IDs and first/repeat token costs from Content `DAY01_UI_FIXTURE.json`: DOC-01/check_seat_map, DOC-02/check_room_calendar, DOC-03/check_contract_appendix; all first costs 1 token, repeat costs 0. Invalid IDs fail closed. Read-only metadata only: does not authorize an inspection, spend tokens or assert a completed receipt. Same C# contract for Windows and Android.
- **Production commit:** `cb46947f4da11454dcdaa9dcce43c1afb0ba3e10`; **golden assertion commit:** `52a0dfeac6031ad65dc7dec90ff016000e9ab08e`. Changed `Runtime/Npc/VisitorModels.cs`, `Tests/Npc/VisitorIdentityGoldenTests.cs`, and this handoff.
- **Validation:** manual comparison with Content Day01 fixture (three IDs, costs 1/0); assertions added for three positive and five negative inputs. **NOT RUN:** local dotnet, Unity EditMode/PlayMode, Android build/device; CI not assumed green. Runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **BLOCKED for EventDirector:** Core atomic settlement, receipts and earnedId deduplication ports; Content/Core provenance of E03/E07 and original agreement occurrence. C0-v2 PR #9 and ADR PR #14/#17 still govern feature work. Content 04 owns canonical IDs; UI 03 owns real art (#21), no PNG generated. PR #16 Draft; no main push or merge.

## AutoDev 02 — visit-scoped Day01 inspection preview (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetInspectionPreviewByVisitId(visitId, out InspectionPreview)` to map canonical visit IDs to the existing immutable inspection metadata through their canonical document IDs. Same pure C# lookup on Windows/Android; no UnityEngine, mutable GameState, token spending, receipt generation, ledger or new scheduler. Content PR #11's `DAY01_UI_FIXTURE.json` remains canonical.
- **Production SHA:** `3e9c37e6be72147f0220814a203e5f8be2b9e312`; **golden test-source SHA:** `14bda84829d675715dd160ea885429e495185eee`. Added three positive visit/document/inspection mappings and five negative/null/case/unknown visit cases to `VisitorIdentityGoldenTests.RunAll()`.
- **Verification:** GitHub accepted both file writes; source-to-source comparison with existing document inspection mapping. **NOT RUN locally:** .NET golden runner, Unity EditMode/PlayMode, Android build/device. No new CI success claimed; run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` and verify the exact SHA.
- **Dependencies/BLOCKED:** EventDirector still requires Core-owned atomic settlement/receipt and earnedId deduplication ports, plus Content/Core E03/E07 fact provenance and original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 govern feature work while unmerged. Content owns IDs; UI owns original portrait assets (#21); planned IDs are not evidence of PNG existence. PR #16 Draft; no main push, merge, second GameState/ledger or third-party IP approval.


## AutoDev 02 — choice-scoped Day01 inspection preview (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetInspectionPreviewByChoiceId(choiceId, out InspectionPreview)`. All nine canonical Day01 choices resolve their document's read-only inspection metadata, including choices that do **not** require inspection. This is deliberately *not* an eligibility check, token debit, receipt, or command authorization. Same Unity-free DTO for Windows/Android; invalid IDs fail closed.
- **Production SHA:** `5042fbd578c4cb1a87b6f6c52e9c0f85019db33b`. **Golden assertion SHA:** `51c322d8755dec2fe752474458d0e47625e0a0a3`. Source files: `Runtime/Npc/VisitorModels.cs`, `Tests/Npc/VisitorIdentityGoldenTests.cs`. Tests added for nine canonical choice/document/inspection mappings and six invalid choice inputs.
- **Verification:** GitHub accepted both writes. Local `dotnet`, `csc` and `mcs` were checked and are unavailable; **no executable C#, Unity or Android tests run in this pass**. Test command when available: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. CI outcome for final HEAD not yet confirmed.
- **Dependencies / BLOCKED for full EventDirector:** Core-owned atomic settlement/receipt/earnedId deduplication ports and Content/Core agreement on E03/E07 provenance. C0-v2 PR #9 and ADR PR #14/#17 are still feature-stage, not merged; PR #11 Content fixture remains canonical. UI 03 owns original art (#21), and planned asset IDs are not proof of PNG files. PR #16 stays Draft; no main push/merge or second GameState/ledger.

## AutoDev 02 — visit/document-scoped inspection metadata (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetInspectionPreviewForVisitDocument(visitId, documentId, out preview)`. It resolves canonical Day01 inspection metadata only when the displayed visit and document match; exact ordinal Content IDs. Pure C#, immutable read-only DTO for Windows/Android. It never spends tokens, creates receipts, authorizes decisions, modifies Core/GameState or checks actual asset availability.
- **Production SHA:** `c29082e634d35b62e9cbdc7aed6a563b161131ea`. **Golden test-source SHA:** `e74a75ad7ab81a4ff1cf26d7951b3c1f02d426f4`. Golden assertions added for three valid pairs, three stale cross-document pairs and five invalid/null/case-mismatched pairs.
- **Verification:** GitHub accepted the source and test file writes; checked `InspectionPreview.DocumentId` exists in actual branch source. **NOT RUN:** local .NET runner (dotnet not installed), Unity Editor/PlayMode, Android build/device. Do not claim CI success without checking the exact final SHA. Intended runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **BLOCKED for EventDirector:** Core-owned atomic settlement/receipt and earnedId deduplication ports plus Content/Core agreement on E03/E07 provenance and canonical agreement occurrence. PR #9 C0-v2 and PR #14/#17 ADR remain review-gated. Content #11 owns canonical IDs; UI #21 owns original portrait assets. No second GameState/ledger, no main push/merge.


## AutoDev 02 — triple-scoped inspection preview (2026-11-18)

- **One bounded increment (#4/#20):** `Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(visitId, documentId, choiceId, out preview)` validates the canonical Day01 visit/document/choice triple before returning existing immutable inspection metadata. It is a Unity-free, Android/Windows-safe presentation guard, **not** inspection eligibility, spending, Core authorization or receipt creation.
- **Production SHA:** `ce75dfc5893941b0c5f736426fdec98103e7c387`. **Test-source SHA:** `66925152ee01a4afda7e654d97239ce6c51030b0`. Golden assertions added for three valid triples, nine mismatched triples and three invalid inputs.
- **Verification:** GitHub accepted source and test commits; executable .NET, Unity and Android tests **NOT RUN in this pass**. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` and verify CI for final HEAD before claiming success.
- **BLOCKED:** EventDirector integration still awaits Core atomic settlement/receipt/earned-key ports and Content/Core E03/E07 provenance; C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain governing dependencies. Content owns canonical IDs, UI owns original portrait PNGs (#21). PR #16 stays Draft; no main push/merge, no new GameState/ledger or IP approval.

## AutoDev 02 — NPC-ID inspection preview (2026-11-18)

**One bounded increment (#4, #20):** added `Day01Visitors.TryGetInspectionPreviewByVisitorId(string, out InspectionPreview)` to `Runtime/Npc/VisitorModels.cs`. This returns a read-only inspection hint for each canonical Content `npcId` (Irina / Dasha / Mark) using the existing document mapping. Unknown/case-mismatched IDs fail closed with null output. The helper does not spend inspection tokens, authorize choices, mutate GameState, or claim that art assets exist. Windows/Android share the same pure C# DTO.

**Commits:** production `4a9b66d398a4724d36b853aa0a82cda0dffa6c3f`; golden assertions `34af24c9ed89f8a60a7a30234e4497848294c0ea`; this handoff update is a separate commit. PR #16 stays Draft; no main changes or merges.

**Verification:** read actual feature branch source and Content draft JSON; GitHub accepted the two C# writes. Added 3 positive and 4 negative golden assertions. **NOT RUN:** .NET build/golden execution, Unity Editor, Android runtime. Await CI result for the new head; do not treat assertions added as assertions passed.

**Dependencies:** Core atomic scheduled-event/receipt/earned-key API remains unconfirmed for full EventDirector. Content PR #11 draft remains proposed; portrait IDs and descriptions are only planned references, requiring Content/UI #21 coordination. C0-v2 PR #9 and ADR PR #14 remain unmerged; no second GameState or ledger.

## AutoDev 02 — composite Day01 visit screen DTO (2026-11-18)

- **One bounded production increment (#4/#20):** added `Day01Visitors.VisitScreenPreview` and `TryGetVisitScreenPreview(visitId, out screen)`. One immutable, Unity-free screen snapshot combines canonical visitor card, document inspection hint, and three ordered read-only choice previews. This avoids independently routing mismatched visitor/document/choice IDs on Windows and Android. This is UI identity only: no Core command authorization, spending, receipts, state mutation, RNG, ledger or EventDirector.
- **Production SHA:** `fd04fd3ebfaa1617ad8bf0f4898858d66340f48e`. **Test-source SHA:** `259d9f0df0aa86b07b0c76714c8118cb95a5c22d`. Golden assertions cover all three visit/document/inspection mappings, nine ordered choices, read-only choice collection and four invalid visit IDs.
- **Verification:** GitHub accepted both source updates. **NOT RUN:** .NET runner, Unity EditMode/PlayMode, Android build/device; added tests are not claimed passing. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` in a .NET 8 environment and verify CI at the final HEAD.
- **Dependencies / BLOCKED for full EventDirector:** Core-owned atomic settlement, receipts and earned-key deduplication, plus Content/Core agreement on E03/E07 provenance and original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 govern integration. UI 03 owns real portrait PNGs (#21); planned asset IDs are not proof of files. PR #16 stays Draft; no main push/merge or external IP approval.

## AutoDev 02 — document-routed Day01 screen parity (2026-11-18)

- **One bounded increment (#4/#20):** `Day01Visitors.TryGetVisitScreenPreviewByDocumentId(documentId, out screen)` resolves the existing immutable `VisitScreenPreview` through canonical Content document→visit identity. Windows/Android can open a document and display the same visitor, inspection and ordered choices as the visit-based route. Unknown/case-mismatched IDs fail closed; no Core mutation, ledger, inspection receipts, UnityEngine or new GameState.
- **Production commit:** `b6c8292da4b942fdae4c4748d541e28c0dfcc947`; **golden test-source commit:** `d7d1b8f9df525ffd42df097a48ea3f247aa80e7f`. Assertions compare all three document/visit screen mappings, inspection IDs, ordered choices and four invalid inputs.
- **Validation:** source edits committed by GitHub; assertions added but **NOT EXECUTED** in this pass. No local .NET/Unity/Android test result claimed. Runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. CI must be checked for the final SHA before claiming success.
- **Dependencies/BLOCKED for EventDirector:** Core 01 atomic event settlement, receipts and earnedId deduplication; Content/Core provenance for E03/E07 and original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain governing feature-stage contracts. UI/Content own original portrait assets (#21); planned art IDs are not evidence of existing PNGs. PR #16 Draft; no main push/merge or third-party IP approval.

## AutoDev 02 — canonical NPC-routed Day01 screen (2026-11-18)

- **One bounded production increment (#4/#20):** added `Day01Visitors.TryGetVisitScreenPreviewByVisitorId(visitorId, out screen)`. The read-only Windows/Android DTO now resolves by canonical Content `npcId` and reuses the existing visit-screen assembly, preserving the same visit/document/inspection/ordered choices. Unknown, empty and case-mismatched IDs fail closed with null output. No GameState, ledger, spending, event receipt, UnityEngine or new art files.
- **Production commit:** `6a639818741e95fd2c507927fa70d12a126b3592`; **test-source commit:** `87723201380cde11249fd9c4508fe456444223d4`. Golden assertions cover three canonical NPC routes, choice-order parity and four invalid IDs.
- **Actual verification:** GitHub accepted both source changes. Assertions were **added but NOT EXECUTED**; no .NET, Unity or Android compile/device test in this pass. Runner when available: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. CI at final HEAD must be checked separately.
- **Dependencies / BLOCKED for full EventDirector:** Core-owned atomic settlement/receipt and earnedId deduplication ports; Content/Core E03/E07 provenance and original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain feature-stage contracts. UI/Content own original portrait PNGs (#21); planned IDs are not uploaded assets. PR #16 stays Draft; no main push/merge or third-party IP approval.

## AutoDev 02 — choice-routed Day01 screen (2026-11-18)

- **One bounded production increment (#4/#20):** added `Day01Visitors.TryGetVisitScreenPreviewByChoiceId(choiceId, out screen)`, using existing canonical choice-to-visit mapping and the same immutable Windows/Android screen DTO. No GameState/ledger/receipts/inspection spending, Unity dependency, new art or Core authorization.
- **Production SHA:** `5c313e048453374d570735473e37bdaa350db10c`; **test-source SHA:** `43e60117d838c9470a857bff813b9f8f9fd2b38e`. Added nine choice-to-visit/document/inspection/menu-order parity checks and five negative/null/case/room/unknown ID cases.
- **Actual verification:** GitHub accepted source/test writes; golden assertions added, **NOT EXECUTED** locally. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`; Unity/Android tests NOT RUN. Check Actions for final HEAD before claiming CI success.
- **Dependencies / BLOCKED for EventDirector:** Core-owned atomic settlement, receipts and earnedId deduplication; Content/Core E03/E07 provenance and canonical agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain feature-stage. UI/Content own original art (#21); planned IDs are not proof of PNGs. PR #16 Draft, no main push/merge or third-party IP approval.

## AutoDev 02 — triple-scoped Day01 screen routing (2026-11-18)

- **One bounded increment (#4/#20):** added `Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(visitId, documentId, choiceId, out screen)` to resolve the existing immutable Windows/Android `VisitScreenPreview` only when all three canonical Content IDs belong to the same Day01 visitor. This is presentation routing, **not** Core authorization, token spending, inspection proof, event settlement or state mutation.
- **Production commit:** `77afacfcc090b7a3b691f9d933b80cac9c5bac82`. **Golden-test source commit:** `16f004bb9fa213150df2daa40d9eb01a08ec3b1b`. Added nine valid triple cases, 27 cross-identity rejections and four invalid/null/case-mismatched inputs.
- **Actual validation:** GitHub accepted the two source writes. **NOT RUN:** local .NET golden runner (dotnet unavailable), Unity Editor/PlayMode and Android device/build. Assertions added are not claimed passing. Runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`; check CI on final HEAD.
- **Dependencies / BLOCKED for full EventDirector:** Core-owned atomic event settlement/receipts/earnedId deduplication and Content/Core agreement on E03/E07 provenance and agreement occurrence. Follow C0-v2 PR #9, ADR PR #14 and Content PR #11 until merged. UI/Content own original portrait art (#21); planned IDs are not proof of actual PNGs. PR #16 remains Draft; no main push/merge, new GameState/ledger or third-party IP approval.

## AutoDev 02 — planned asset to read-only Day01 screen (2026-11-19)

- **One bounded production increment (#4/#20/#21):** added `Day01Visitors.TryGetVisitScreenPreviewByPlannedAssetId(assetId, out screen)`. Reuses existing exact-case planned portrait/standing/document asset ID mapping and the existing immutable Day01 visit-screen DTO. This is a platform-neutral navigation hint for Windows/Android, **not** proof that any portrait/PNG exists, inspection receipt, command authorization or mutable Core state.
- **Production commit:** `bb9f230577f49a3cc3608c442ae86985c9ae1e9e`. **Golden assertion source commit:** `748e7ac1ff1ce7487fd9c2579ce452055350fd89`. Added nine positive asset→visit/document/inspection/choice-order parity cases and five invalid asset-ID cases.
- **Actually verified:** GitHub accepted the production and golden-test source updates. **NOT RUN:** .NET runner, Unity EditMode/PlayMode and Android build/device. Execute `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` and inspect CI for the final HEAD before claiming passing tests.
- **Dependencies/BLOCKED for full EventDirector:** Core 01 atomic scheduled event settlement/receipts/earnedId deduplication and Content/Core E03/E07 provenance and original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 are not overridden. Content/UI Issue #21 owns original art production and asset verification; no images generated or uploaded. PR #16 Draft; no main push/merge, GameState fork or third-party IP approval.

## AutoDev 02 — visit-scoped planned art routing (2026-11-19)

- **One bounded increment (#4/#20/#21):** `Day01Visitors.TryGetVisitScreenPreviewForVisitAsset(visitId, assetId, out screen)` returns the existing immutable Day01 screen only when the canonical visit and planned portrait/standing/document asset ID belong to the same fictional visitor. Exact-case mismatch fails closed with null output. This is Windows/Android presentation identity only, not PNG existence, inspection proof, command authorization, GameState, ledger or EventDirector.
- **Production SHA:** `81acac71f3a6b1608470ba4e85879b55e49a8331`; **golden test-source SHA:** `b2fb643df0a8013e1d8b913c88e7d6671838f299`. Added 9 matching asset/visit checks, 18 cross-visitor rejections, and 3 invalid pair checks.
- **Actual checks:** GitHub accepted two source commits; `dotnet`, `mono` and `Unity` executables absent in available container. **NOT RUN:** executable golden tests, Unity, Android; no successful CI claimed. Intended runner: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`.
- **BLOCKED for full EventDirector:** Core atomic event settlement/receipts/earned-key deduplication and Content/Core E03/E07 provenance/original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain unmerged dependencies. UI/Content Issue #21 owns actual original PNGs; planned IDs do not prove assets exist. PR #16 remains Draft; no main push/merge or second GameState/ledger.


## AutoDev 02 — Day01 identity uniqueness regression (2026-11-19)

**One bounded increment (#4/#20/#21):** added a deterministic golden assertion that all three canonical Day01 visit IDs, visitor definition IDs, document IDs and all nine planned portrait/standing/document asset IDs are unique (ordinal comparison). Prevents ambiguous cross-platform screen routing if Content fixtures change. No runtime API, Core state, ledger, art file or gameplay behavior changed.

**Test source commit:** `9fdffe72098c4d560fc09030cfbe6c4925850abe` on `feature/npc-4`, `Tests/Npc/VisitorIdentityGoldenTests.cs`. **Verification:** GitHub accepted the source update. **NOT RUN:** .NET golden runner, Unity Editor, Windows or Android builds; no passing test claim. Command: `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release`. Handoff commit is the subsequent branch HEAD.

**Dependencies / BLOCKED:** EventDirector still requires Core atomic settlement, persisted event receipts and earned-key deduplication; E03/E07 source identity must be reconciled with Content. Planned art IDs do not establish real PNG availability or asset ownership. PR #16 stays Draft; main unchanged.


## AutoDev 02 — document-scoped Day01 art routing (2026-11-19)

- **One bounded production increment (#4/#20/#21):** `Day01Visitors.TryGetVisitScreenPreviewForDocumentAsset(documentId, assetId, out screen)` returns the existing read-only visit screen only when a canonical document and planned portrait/standing/document art ID belong to the same Day01 visitor. It fails closed on unknown, null or cross-document IDs. Pure C#, no UnityEngine, GameState, ledger, Core command or PNG assumption.
- **Commits on feature/npc-4:** production `e3455a34e2fdf54900232d137572dbd8d0845a1c`; test additions `9a086555fc17b5b3a4a04ac9b4600ed0ee223d8e`; canonical partner fixture correction `1c4a08363ed4a357043d819ddb9f0f3710b712d1`. 27 positive/cross-document combinations plus five negative inputs are encoded as assertions.
- **Actual verification:** GitHub accepted source updates; manually compared partner art ID against existing golden fixture. **NOT RUN:** dotnet runner, Unity Editor, Android build/device, CI at final HEAD. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` in .NET 8 environment.
- **Dependencies/BLOCKED for EventDirector:** Core-owned atomic settlement/receipts/earnedId deduplication, E03/E07 provenance and original agreement occurrence with Content. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 govern feature development pending integration. Content/UI #21 own original art; planned IDs do not establish actual PNG existence. PR #16 Draft; no main push/merge.


## AutoDev 02 — repair NPC golden C# compilation (2026-11-19)

- **One bounded task (#4/#20):** repaired three name collisions in `Tests/Npc/VisitorIdentityGoldenTests.cs` without changing assertions, expected outcomes, runtime NPC API, Content data, Core state, ledger or planned art. Renamed the nested `missing` and `invalidPreview` variables and reused the earlier `scopedDocuments` array instead of redeclaring it.
- **Evidence:** GitHub Actions NPC .NET 8 run [#44436740987](https://github.com/rixed318/Probably-TAU/actions/runs/44436740987) on previous PR head `70dc27cd0e23dd65b43f4e2ae94ad80b3ded2757` failed to compile with CS0136 at lines 33/66 and CS0128 at line 659. **Fix commit:** `6146f43249647e47e136dd888a2a996bb388cdd4` on `feature/npc-4`.
- **Actual verification:** prior failing workflow logs inspected and GitHub accepted the source update. **NOT RUN/NOT YET CONFIRMED:** new-head .NET golden execution, Unity Editor/PlayMode, Windows or Android builds. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` and verify Actions for exact final HEAD.
- **Dependencies/BLOCKED for EventDirector:** Core 01 must expose an approved atomic due-event/receipt/earned-key integration port; Content 04 must settle E03/E07 agreement occurrence provenance. C0-v2 PR #9 and ADR PR #14/#17 govern until integration; PR #11 remains Content source. Planned original portrait IDs under #21 do not establish existing PNGs. PR #16 remains Draft; no main push/merge.


## 2026-11-19 — CI verification of prior compile fix (Issue #4 / #20)
- Verified current `feature/npc-4` head before this documentation update: `2be35a4dd8255785b88930b84c60031d74eaedea`.
- GitHub Actions run [#44448434879](https://github.com/rixed318/Probably-TAU/actions/runs/44448434879) reports `completed/success` for that SHA (`NPC condition golden tests`, run #223). This verifies the branch's .NET NPC workflow after the prior `CS0136`/`CS0128` name-collision fix; it is not a Unity or Android build.
- This run changes only the handoff/PR verification record; no new runtime code or tests. No local .NET, Unity Editor or Android tests executed. Do not infer Core/EventDirector integration from this success.
- **BLOCKED for EventDirector:** await concrete Core atomic scheduled-event settlement, receipt and earned-key deduplication ports; E03/E07 provenance requires Content/Core agreement. Planned art IDs are not evidence of shipped image assets.
- Next: Core/Content review of receipt and agreement-occurrence identifiers, then one isolated integration step. No main merge.


## 2026-11-19 — document/art fail-closed regression (#4 / #20 / #21)
- **One bounded change:** extended existing `VisitorIdentityGoldenTests.RunAll` document-scoped asset routing negatives to reject null, empty and wrong-case planned asset IDs, retaining unknown-ID coverage. Expected output is null on failure; canonical Day01 identifiers and production code are unchanged.
- **Test source SHA:** `50848181edf758cef833b6ad7cb7d9edc1dbe918` on `feature/npc-4`. This adds three negative inputs beyond the previously tested unknown asset (four total in that group).
- **Actually checked:** GitHub accepted the source commit; reviewed the literal test change. **NOT RUN:** .NET golden runner for this new SHA, Unity Editor, Android build/device. Prior successful .NET workflow #44448434879 belongs to older SHA `2be35a4dd8255785b88930b84c60031d74eaedea`, not this change. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` or verify a new exact-SHA CI run.
- **Dependencies / BLOCKED for full EventDirector:** Core-owned atomic due-event settlement/receipts/earned-key deduplication and Content/Core E03/E07 original agreement occurrence. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain governing unmerged contracts. Planned portrait IDs do not prove PNG availability (#21). No GameState/ledger fork, art upload, main push or merge. PR #16 remains Draft.


## 2026-11-19 — repair CS0136 from latest NPC CI (#4 / #20)
- **One bounded task:** GitHub Actions [run #44471055253](https://github.com/rixed318/Probably-TAU/actions/runs/44471055253), testing PR #16 merge ref containing branch SHA `7d20679063db701f5281e41f11f2f1dd293654dc`, failed in .NET compilation at `Tests/Npc/VisitorIdentityGoldenTests.cs:39`: CS0136 local `invalidAsset` conflicts with another local in enclosing scope.
- **Fix:** renamed only the newly introduced foreach variable to `invalidDocumentAssetId` and its use at line 43; no assertions, expected results, runtime logic, Core, Content, ledger or GameState changed. Source commit `50bd763a558b15f6100ecb7a7202d47d519e6f3a` on `feature/npc-4`.
- **Actual verification:** fetched failed workflow job logs and confirmed GitHub accepted source commit. **NOT RUN/NOT VERIFIED:** new-head .NET golden test run, Unity Editor, Windows or Android build. Previous workflow is a failure, not evidence of a passing fix. Recheck exact-SHA GitHub Actions and run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` when available.
- **Dependencies / BLOCKED for full EventDirector:** Core atomic due-event settlement, receipts and earned-key deduplication, and Content/Core agreement provenance for E03/E07. Follow C0-v2 PR #9, ADR PR #14/#17, Content PR #11; portrait IDs under #21 remain planned, not confirmed assets. PR #16 stays Draft. No main push or merge.


## 2026-11-19 — verify CS0136 repair via exact-head CI (#4 / #20)
- **One bounded task:** checked PR #16 `feature/npc-4` HEAD `4b86e7cb03cf444c2b4ecd7cb88642fbcba01802` after previous variable-shadowing repair. GitHub Actions [run #44484658438](https://github.com/rixed318/Probably-TAU/actions/runs/44484658438), job `npc-conditions` (job ID `135425006898`), reports `completed/success`; the `Build and execute actual NPC C# golden assertions` step also reports `success`. This is evidence of the .NET NPC golden runner passing at the pre-documentation HEAD, not a Unity/Windows/Android build.
- **Scope:** handoff verification record only. No production C#, assertions, Content IDs, GameState, ledger or images changed. No local .NET/Unity/Android tests run. A new documentation-only HEAD is not covered by the prior exact-SHA check.
- **BLOCKED for EventDirector:** approved Core atomic due-event settlement, receipt and earned-key deduplication port; E03/E07 original agreement occurrence provenance to be coordinated with Content. PR #9 C0-v2, PR #14 ADR and PR #11 Content remain unmerged; PR #16 remains Draft. Do not merge or push to main.


## 2026-11-19 — verify current NPC head CI (#4 / #20)
- **One bounded verification task:** inspected PR #16 feature/npc-4 HEAD `a5e45120e6a1787a17906ef86dc0f5148619e9e6` and its exact-commit GitHub Actions association. NPC condition golden tests workflow run [#44499082029](https://github.com/rixed318/Probably-TAU/actions/runs/44499082029) reports `completed/success` (run 231). This confirms the reported .NET workflow outcome for that commit; no local .NET invocation or Unity/Windows/Android test was performed in this pass.
- **Scope:** verification and handoff only; no production source, test assertions, Content IDs, GameState, ledger or art changed. A subsequent documentation-only commit is not independently validated by that prior run.
- **Dependencies/BLOCKED for full EventDirector:** Core-owned atomic scheduled-event settlement, durable receipts and earned-key deduplication; Content/Core E03/E07 original agreement occurrence provenance. C0-v2 PR #9, ADR PR #14/#17 and Content PR #11 remain governing unmerged contracts. Planned art IDs under Issue #21 do not establish real PNG availability. PR #16 stays Draft; no main push or merge.


## 2026-11-19 — exact-head NPC CI verification and PR status sync (#4 / #20)
- **One bounded task:** verified branch HEAD `6f9076b78366f1a4355b46f90f4e0eac72c4edef` against GitHub Actions run [#44513994487](https://github.com/rixed318/Probably-TAU/actions/runs/44513994487): `completed/success`. This is the GitHub NPC .NET workflow result associated with the pre-handoff SHA, not Unity Editor, Windows player, Android APK or device testing.
- **Scope:** synchronize the stale PR #16 verification text and this handoff with the current verified workflow. No C# production source, assertions, Core, ledger, Content IDs or art changed. The documentation commit produced by this update is not itself covered by the verified earlier SHA.
- **BLOCKED for EventDirector:** Core 01 atomic scheduled-event settlement/receipt/earned-key deduplication ports and Content/Core E03/E07 original agreement occurrence provenance. C0-v2 PR #9 and ADR PR #14/#17 govern feature work only; no main merge. Planned portrait asset IDs (#21) are not actual PNG evidence.

## 2026-11-19 — exact-head CI status, no runtime integration (#4 / #20)
- **One bounded verification task:** checked `feature/npc-4` at `b96ba7ec15378d1d2e5c17800b427072835ac322`. GitHub Actions [NPC golden run #44545966216](https://github.com/rixed318/Probably-TAU/actions/runs/44545966216) reports `completed/success` (run 234) for that SHA. This is a GitHub .NET workflow result, not Unity/Windows/Android validation.
- **Scope:** verified CI and recorded provenance; no NPC runtime/Content/Core, test assertions, asset files or mutable state changed. This documentation update has a later SHA not covered by the reported run. No local tests executed.
- **Integration remains BLOCKED:** require Core-owned atomic due-event settlement, durable receipt and earned-key deduplication API; E03/E07 original agreement occurrence provenance from Core/Content. C0-v2 PR #9 and ADR PR #14/#17 remain unmerged. No main push/merge; PR #16 remains Draft.


## AutoDev 02 — document-scoped art screen parity regression (2026-11-19)

- **One bounded task (#4/#20):** added golden assertions comparing each of the nine valid Day01 document/portrait/standing/blank-art routes against the canonical document screen. Checks visit identity, inspection ID/token cost and ordered choice IDs. This catches cross-platform UI routing drift without adding another gameplay state, ledger, or Android-specific implementation.
- **Test-source commit:** `7912067a0bae3be554dc1de8412009400c99ad0c` on `feature/npc-4`; only `Tests/Npc/VisitorIdentityGoldenTests.cs` changed. This handoff is a separate documentation commit. Draft PR #16; no main push or merge.
- **Verification:** manually inspected existing `TryGetVisitScreenPreviewByDocumentId` and `TryGetVisitScreenPreviewForDocumentAsset` methods. Previous head `c9ee2c649fef320f750b2c4e0a9b5fb7a2d05d1c` has successful NPC GitHub Actions run #44565196266. **New test SHA: no associated workflow run returned at inspection time; NOT verified as passing.** Local .NET, Unity and Android tests NOT RUN. Run `dotnet run --project Tests/Npc/Standalone/NpcConditionGolden.csproj --configuration Release` when runner is available.
- **BLOCKED for EventDirector:** Core atomic AdvanceDay settlement/receipts/earned-key ports and Content/Core E03/E07 provenance remain unresolved. C0-v2 PR #9 and ADR PR #14 remain unmerged; planned Issue #21 portrait IDs are not evidence of actual PNGs.
