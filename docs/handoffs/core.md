# ЧАТ 01 — handoff: Simulation Core / Economy

**Дата:** 14.11.2026  
**Issues:** #2 (Unity 6 + GameState/Clock/Commands), #3 (Economy + Ledger).  
**Ветка:** feature/core-2 (создана от main d673322cd9d0d366a27a57b81056504dfb26c731).  
**Draft PR:** https://github.com/rixed318/Probably-TAU/pull/9  
**Контрактный commit:** 4ccf57c08faca0518e2c8ceb8f5d0748eaea6dab.  
**Статус:** AUDITED → C0 CONTRACTS PROPOSED → **REVISIONS REQUESTED BY 00 → C0-v2 REVISION SUBMITTED FOR RE-REVIEW**. **Код/Unity-проект ещё не созданы.**

## 1. Что фактически сделано

- Через GitHub прочитаны README.md, AGENTS.md, docs/GDD.md, docs/ARCHITECTURE.md, docs/DECISIONS.md, docs/CHAT_WORKFLOW.md и docs/MECHANICS.md, открытые Issues #2/#3, root tree и .github/PULL_REQUEST_TEMPLATE.md.
- Проверен main: на момент аудита d673322, документация + задачи, нет Unity Assets/Packages/ProjectSettings и существующего runtime-кода.
- Подтверждены права GitHub (push=true); создана исключительно feature/core-2.
- Добавлено предложение docs/proposals/CORE_CONTRACTS_C0.md и открыт Draft PR #9 без merge.
- Проработаны инварианты, deterministic API, деньги в long копейках, ledger, повторное применение команд и AdvanceDay, границы внешних чат-систем.
- Принятых решений в docs/DECISIONS.md не меняли (владелец решений — чат 00).

## 2. Контракты, ожидающие решения

См. docs/proposals/CORE_CONTRACTS_C0.md, блок C0-01..C0-10:
- Versioned GameState с 5 нефинансовыми метриками, бюджетом, типизированными секциями, схемой IDs, revision, command receipts.
- IGameCommand / ICommandHandler / ICommandProcessor, prepare/validate/atomic commit, DuplicateAlreadyApplied, typed errors.
- DomainEventEnvelope с sequence и immutable read-only queries для UI.
- GameClock: DayIndex начинается с 0; ровно 5 рабочих дней в неделе, недельный расчёт в конце дня 4/9/14.
- EconomyService/LedgerService: money Int64 в копейках, append-only ledger, уникальный PostingId, due-key, задолженность.
- PRNG: versioned PCG32-v1, именованные потоки, сохраняемый state, golden vectors.
- CheckpointRequested после коммита AdvanceDay; file IO, миграции и SaveService в ответственности чата 05.
- Координатор 00 в PR #9 определил порядок: finalize → projects → due scheduled outcomes → due income/obligations → metrics/terminal → conditional next-day generation → report/commit/checkpoint. Эта последовательность внесена в C0-v2 и ожидает повторного Gate C0 review.
- Для v1 по указанию координатора: добровольные расходы без cash отклоняются, обязательства без cash становятся payable без отрицательного cash; овердрафт/частичные выплаты вне v1. Внесены примеры для QA.

**Пока это предложенные интерфейсы, не публичные типы в репозитории.**

## 3. Конкретный план реализации после принятия Gate C0

### Этап A — настоящий Unity 6.3 LTS scaffold (#2)
- Открыть Unity Editor 6.3 LTS и создать 2D проект в unity/.
- Сохранить ProjectSettings/ProjectVersion.txt, Packages/manifest.json, packages-lock.json (при создании), .meta и конфигурацию Unity Test Framework.
- Один asmdef на Core, Application, Economy, один на EditMode tests; избежать циклических зависимостей.
- Проверить Unity import / отсутствие compile errors и обновить README по фактическому состоянию **после** подтверждённого открытия проекта.

### Этап B — Core state, ID, GameClock и RNG (#2)
- Добавить GameState/typed state contracts, GameClock, AcademyMetrics, GameState read-only projections, allocator ID.
- Реализовать PCG32-v1 с явным seed/state и named-stream isolation.
- Добавить unit/EditMode tests: day indexing, metrics bounds, RNG golden/state replay, snapshot consistency.

### Этап C — CommandProcessor / Domain Events (#2)
- Command registry, immutable command envelopes, fingerprint/deduplication, expected revision/day, typed result.
- Staged transaction commit/rollback, deterministic event IDs и read models.
- Тесты: double-submission, collision same-ID/different-payload, stale revision/day, rollback across compound ops, no RNG mutation on reject.

### Этап D — Economy + Ledger (#3)
- Money/Amount checked Int64, Balance, LedgerEntry, PostingId, zero/positive delta policy.
- EconomyService/Obligation scheduler, due-key, accrued payable + payment settlement по утверждённой политике.
- Тесты: недостаток средств, overflows, duplicate ledger posting, unique due-period, balance audit invariant.

### Этап E — AdvanceDay (#2/#3)
- Единственный pipeline со строго согласованным порядком и стабильным DayReport.
- Отделить игровые доменные события от уведомления CheckpointRequested.
- Тесты: double AdvanceDay with same and different IDs, weekly 5-day charges, 30-day replay, atomic rollback during end-day, same seeded generation when external event provider attached.

### Этап F — интеграция и review
- Зафиксировать принятые классы/события и compatibility notes, обновить данный handoff.
- Проверить Unity EditMode suite и фактическую компиляцию; указать Unity editor version и команды проверки.
- Попросить чат 05 независимо провести QA; отдать PR на code review чату 00.
- Только после явного разрешения владельца возможен merge; самостоятельно merge в main не делать.

## 4. Зависимости и что передать другим чатам

- **00:** принять C0-01..C0-10, особенно порядок EndDay и insufficient-funds policy, обновить DECISIONS.md и при необходимости ARCHITECTURE.md. Не считать GDD предложениями уровня ADR.
- **02 NPC:** не изменять mutable GameState напрямую; использовать обработчики команд, версионированные domain events и named RNG; подготовить requirements к секциям Visitors/EventState.
- **03 UI:** работать на mock FinanceView/CalendarView/CommandResult; после C0 только команды, никакого budget -=.
- **04 Content:** стоимость в копейках, definition ID как stable string; содержание баланса и числа — в data definitions, не hardcode Core.
- **05 Save/QA:** ожидать контракт GameState/receipts/ledger/RNG, отдельно спроектировать migration+IO; не производить повторное начисление на загрузке.

## 5. Что НЕ сделано и верификация

- Не создан Unity-проект, Runtime C# исходники, asmdef, тесты, скрипты content или SaveService.
- Ничего не затрагивает UI, NPC, сюжет, балансные цифры; main не изменён.
- Проверка GitHub: ветка и Draft PR созданы; read-операции в main выполнены.
- В текущей среде выполнения **не найден Unity Editor, dotnet или mono**. Поэтому запуск Unity, создание настоящего редакторного проекта и проверка компиляции не заявляются. Для этапа A необходима среда с Unity 6.3 LTS. Код, если будет написан без неё, считается непроверенным в Unity до отдельного editor-run.

## 6. Риски

1. Параллельные чаты могут преждевременно закодировать непринятый API — сначала review Gate C0.
2. DDD-секции GameState ещё не имеют owner-defined DTO; без этого SaveService не должен закреплять финальный wire-format.
3. День/неделя расходятся по порядку между GDD и ARCHITECTURE; есть существенные финансовые последствия.
4. Недостаточно определено поведение кассового разрыва/задолженности; не кодировать догадки в экономику.
5. Без реального Unity Editor нельзя правдиво считать #2 завершённым.

**Следующий шаг:** координатор 00 проводит **повторное** review обновлённого Gate C0-v2 в https://github.com/rixed318/Probably-TAU/pull/9; после утверждения ЧАТ 01 переходит к реализации только в feature/core-2 и обновляет handoff с результатами реальных тестов.

## 7. Обновление по результатам review координатора 00 (14.11.2026)

**Источник:** https://github.com/rixed318/Probably-TAU/pull/9#issuecomment-6877397075. Вердикт: REQUEST REVISIONS (не APPROVED). Смежные Draft PR: #10 (QA/Save), #11 (Content Day 01), #12 (UI); Issue #13 (AI pixel-art) принадлежит чату 03.

**Новый C0-v2:** `docs/proposals/CORE_CONTRACTS_C0.md`, commit `1c8d8eaa0e8573a84a54fd8bdd7f5959bbe17460`. Внесено:

1. **Один source of truth Q/S/M/E/R** (Q=EducationQuality; S=StudentSatisfaction; M=StaffMorale; E=EmployerTrust; R=PublicReputation). Отдельный mutable ReputationState не вводится; четыре группы отношений — read model из S/M/E/R, абитуриенты позднее.
2. **EndDay-очерёдность 00**, single processed-day ID и durable effect/earned/due keys; project tick и deadlines до ledger, next-day generation только при nonterminal.
3. **v1 cash policy:** voluntary reject, mandatory payable + warning, nonnegative cash, checked overflow.
4. **DayNumber=DayIndex+1**; недельные проводки при closing n=4,9,14, то есть UI День 05/10/15.
5. **Разные версии:** domain GameState.SchemaVersion, SaveEnvelope.schemaVersion, contentVersion, buildVersion; serialized receipts/ledger/RNG/due/effects без floating-point 64-bit conversions.
6. **Proposed read models** для Dashboard/Finance/Calendar/Document/DayReport; чистые snapshot/restore ports для QA и инфраструктуры.
7. **Девять golden acceptance scenarios**: calendar/settlement, payables, insufficient funds, conditional revenue, rollback, terminal state, reference PCG32 outputs, Save→Load, deterministic events.

**Осталось:** повторное ревью C0 от чата 00, ADR в DECISIONS (не редактируем сами), честная проверка в Unity Editor после создания проекта. Ветка `feature/core-2` и PR #9 остаются Draft, merge не разрешён. Соседние PR не менялись.

## 8. AutoDev 01 — PCG32-v1 реализация (14.11.2026)

**Gate:** координатор 00 разрешил C0-v2 к реализации в комментарии PR #9 от 14.11.2026; ADR-C0-001 оформлен в Draft PR #14, но не находится в main. В этом проходе реализован только один независимый подкомпонент RNG для Issue #2.

**Изменения:**
- `Runtime/Core/Pcg32.cs` — pure C# PCG-XSH-RR 64/32, `AlgorithmId=pcg32-v1`, эталонная инициализация `FromSeed(seed, sequence)`, `NextUInt32()`, unbiased rejection sampling `NextUInt32(maxExclusive)` / `NextInt(maxExclusive)`, `Copy()` и точное `Restore(state, increment)`. Все overflow-операции RNG выполнены через `unchecked`, increment обязан быть нечётным.
- `Tests/Core/Pcg32GoldenTests.cs` — самодостаточные тесты reference vector `seed=42, sequence=54`: `a15c02b7 7b47f409 ba1d3330 83d2f293 bfa4784b cbed606e`, replay Save→Load на 128 значениях, независимость копии, bounded range и аргументы. Метод `RunAll()` позже подключить к NUnit/EditMode wrapper без изменения тестовой логики.

**Коммиты:** `b7284b0` (реализация), `b3bba47` (тестовый исходник), затем коммит handoff. PR #9 остаётся Draft. Ни одна другая подсистема не менялась.

**Верификация:** reference vector независимо пересчитан Python 3 с маской 64-bit и совпал с опубликованной последовательностью PCG. Это **не** исполнение C# тестов: в текущей среде `dotnet`, `csc`, `mcs`, Unity Editor не найдены; компиляция C#/Unity и `RunAll()` = **NOT RUN**. Не считать Issue #2 выполненным.

**Ограничения/следующее:** named-stream derivation, сериализуемый RandomState DTO, commit/rollback RNG и публичный IRandomSource ещё не реализованы. В следующем Core проходе подготовить GameClock либо typed RandomState/streams согласно C0-v2, не менять публичные API без ADR. Чату 05 — подключить `Pcg32GoldenTests.RunAll()` к реальному тестовому раннеру после появления среды и проверить golden vector на C#.


## 9. AutoDev 01 — GameClock calendar helpers (14.11.2026, second pass)

**Scope:** one independent C0-v2 Issue #2 task: pure calendar calculations, without state mutation or `AdvanceDay` orchestration. `main` and other subsystem files unchanged.

**Files:** `Runtime/Core/GameClock.cs` and `Tests/Core/GameClockGoldenTests.cs`.

**Commits:** `c7abc5804ea95fdd9870d0cec54a3e2897deccb2` (GameClock) and `5d138aefbeac6d64d4a143543f26e8784ff1662f` (standalone tests). Handoff commit follows separately.

**Contract:** `DayIndex` is zero-based; `DayNumber=DayIndex+1`; `WeekIndex=DayIndex/5`; `WeekDayIndex=DayIndex%5`; settlement days are 4,9,14,...; `NextWeekSettlementDayIndex` includes the current day when it is a closing day. Invalid negative days raise `ArgumentOutOfRangeException`; calculations that exceed `Int32` raise `OverflowException`. `GameClock` is a stateless utility, not a second mutable clock owner. Campaign phase, command deduplication, and end-day processing remain unimplemented.

**Verification:** Reviewed formulas against C0-v2 section 5 and added a standalone `RunAll()` suite covering DayIndex 0..999, first two weekly closings, negative inputs and overflow boundaries. **C# tests NOT RUN**, Unity Editor tests **NOT RUN**; no claim of compilation. No new packages. QA 05 should invoke `GameClockGoldenTests.RunAll()` when a C# test environment becomes available.

**Next:** typed `GameClockState` and integration into `GameState`/CommandProcessor only after coordinating public DTO layout with QA 05. Do not treat these helpers as `AdvanceDay` implementation or close Issue #2.


## 10. AutoDev 01 — minimal GameState snapshot v1 slice (14.11.2026)

**Scope:** one bounded implementation task for Issue #2, C0-v2; no Unity scaffold, no mutable GameState, no CommandProcessor, no ledger posting.

**Files:** `Runtime/Core/GameStateSnapshotDto.cs`, `Tests/Core/GameStateSnapshotGoldenTests.cs`.
**Commits:** `2044039ef8cf618c94a623c46b43888884a81c28` (DTO), `f92da3ae10cca5bb1ac710f55266c2f5348d7479` (tests), plus this handoff commit.
**Contract:** immutable-at-public-boundary snapshot slice with SchemaVersion=1, revision, campaign/seed/difficulty/content, 0-based day and Finished, five 0..100 Q/S/M/E/R metrics, nonnegative Int64 opening/current cash, named PCG32 streams with exact state/increment, unique processed command IDs/day keys and pending/completed event IDs. Duplicate IDs and pending∩completed are rejected. Input lists are copied, and exposed as read-only collections. The snapshot has no IO and is not an active mutable session state.

**Known incomplete:** this intentionally is NOT a full C0 GameState/Save wire format. Receipts currently contain IDs only (no canonical fingerprint/outcome); Economy ledger/obligations/payables, domain events, day reports, flags, and owner-specific typed section payloads are not implemented. Do NOT wire this directly as final SaveEnvelope. A future additive DTO revision or dedicated complete GameState snapshot is required; QA/00 must review layout before persistence integration. Constructor rejects invalid metric values (does not silently clamp); transactional metric delta clamping belongs to CommandProcessor.

**Tests:** standalone `GameStateSnapshotGoldenTests.RunAll()` checks metrics, cash, duplicate IDs, pending/completed overlap, input alias isolation, read-only collection, exact PCG replay and invalid increment. **C# execution NOT RUN; Unity Editor NOT RUN** (dotnet/csc/mcs/Unity unavailable). Source reviewed statically only.

**Next:** request QA #10 and NPC #16 feedback on IDs and pending-event snapshot semantics; then implement a complete receipt and ledger-aware domain snapshot in a separate bounded pass, before SaveService integration. PR #9 stays Draft; no merge/main changes.


## 11. AutoDev 01 — standalone .NET 8 golden runner and GitHub Actions (14.11.2026, 23:04+04)

**Issue:** #2. **Scope:** one bounded test-infrastructure increment; no new public Core APIs, no changes to C0/ADR-C0-002, no Unity project, no economy logic, no other subsystem changes.

**Committed files in feature/core-2:**
- `Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj`: dependency-free net8.0 executable, explicit compilation of actual `Runtime/Core/*.cs` and `Tests/Core/*GoldenTests.cs`, overflow checks and warnings-as-errors.
- `Tests/Core/Standalone/Program.cs`: invokes `Pcg32GoldenTests.RunAll()`, `GameClockGoldenTests.RunAll()`, `GameStateSnapshotGoldenTests.RunAll()` independently, logs pass/fail per suite, exits nonzero if any fails.
- `.github/workflows/core-dotnet.yml`: minimal-permission GitHub Actions for feature/core-2 pushes, relevant PR changes, manual dispatch; installs .NET 8 and runs actual Core C# source via `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`.

**GitHub commits:** `61caa27c76cf0841d19c194afb93e171d130708a` (project), `9cb15729b2beb9863b0d882ca005380b5d843489` (runner), `7532f2deee08af84fe31d632101e27b735ecdbcf` (workflow). This handoff update follows separately.

**Verification:** GitHub accepted the source and workflow commits. Initial PR-associated workflow query for `7532f2de` returned **zero runs**; **C# compile and golden tests NOT YET VERIFIED**, do not claim success. The independent QA Python workflow success is not a C# build. Unity Editor/Unity EditMode **NOT RUN**. Follow up by querying GitHub Actions for the latest branch commit and inspecting the job logs; if CI fails, fix only the failing Core test infrastructure in this branch.

**Next:** once real .NET 8 compile and all three suites pass, begin a bounded append-only Ledger invariant implementation under #3, followed by atomic CommandProcessor/AdvanceDay. Keep PR #9 Draft; no merge to main.


## 12. AutoDev 01 — checked .NET 8 PCG32 overflow regression (15.11.2026)

**Issue:** #2. **Scope:** one minimal corrective Core change, no contract change, no Unity scaffold, no changes to other roles. QA 05 independently confirmed real GitHub Actions [run 43630164245](https://github.com/rixed318/Probably-TAU/actions/runs/43630164245) failed: GameClock PASS, PCG32 and GameStateSnapshot FAIL with `System.OverflowException` at `Runtime/Core/Pcg32.cs:48`. The runner uses `CheckForOverflowUnderflow=true`.

**Root cause and fix:** PCG-XSH-RR 64/32 intentionally discards upper bits of its 64-bit xorshifted intermediate when converting to `uint`. Under checked compilation, the implicit project-wide checked context made the explicit `(uint)` cast throw for large values. In `Pcg32.NextUInt32()`, changed **only** this narrowing conversion to `unchecked((uint)(...))`. The existing `unchecked` state transition remains unchanged; checked overflow protection remains enabled project-wide, including future money calculations. The existing published PCG32 golden vector, 128-step Save→Load replay, and GameStateSnapshot PCG replay serve as regression cases in the three-suite runner.

**Commit:** `1c51de1bfbd0b135a8d6f2a7f60a2e2b924c1703` in `feature/core-2`. **Validation at commit time:** GitHub source write succeeded; actual .NET 8 workflow result must be queried on the latest branch commit. Unity Editor/EditMode NOT RUN. If CI still fails, inspect real job logs and make a separate bounded correction; do not declare green without a completed success run.

**Next:** verify Core .NET 8 workflow on this branch; then implement a bounded append-only Ledger invariant under Issue #3 (checked Int64, unique posting IDs, balance = opening + sum ledger), before CommandProcessor. PR #9 remains Draft, no merge to main or Issue closure.


## 13. AutoDev 01 — immutable Ledger snapshot slice (15.11.2026)

**Scope:** one bounded Core/Economy prerequisite for Issue #3, approved Gate C0-v2 / ADR-C0-001. No CommandProcessor, GameState mutation, payable/obligation policy, settlement or Unity project. This is a pure C# *preparation* API; it must not be mistaken for a fully atomic domain transaction.

**Files/commits:**
- `Runtime/Core/LedgerSnapshot.cs` commit `0e330922`: immutable append-only ledger snapshot with stable posting/source IDs, checked signed Int64 kopecks, no zero-value entries, unique posting IDs, nonnegative cash. Constructor copies entries, validates every intermediate balance and detects overflow. `WithPosting` returns a fresh snapshot or throws without mutating the original.
- `Tests/Core/LedgerGoldenTests.cs` commits `badfe830`, `746e67ce`: golden checks for balance arithmetic, input alias isolation, read-only entries, duplicate posting IDs, insufficient cash, checked overflow, invalid IDs/zero postings, and unchanged original after rejected append.
- `Tests/Core/Standalone/Program.cs` commit `72abba2a`: registers fourth golden suite. Existing csproj glob includes all `Runtime/Core/*.cs` and `Tests/Core/*GoldenTests.cs` automatically.

**Important boundaries:** The immutable LedgerSnapshot is NOT the owner of command-level commit, durable receipts, mandatory-payable handling, or idempotent replay of the same CommandId. A duplicate PostingId is rejected rather than silently re-applied; CommandProcessor will map duplicate commands to prior receipts. OpeningBalance is not a fabricated ledger posting. SaveEnvelope must not persist this alone as the full GameState.

**Verification:** previous Core .NET 8 workflow run [43638320521](https://github.com/rixed318/Probably-TAU/actions/runs/43638320521) succeeded for the earlier three-suite code. The newly added Ledger suite must be evaluated on a **new** workflow run; do not inherit old PASS. Unity Editor/EditMode NOT RUN.

**Next:** verify new Core .NET run; address real failures if any. Then implement atomic command transaction/receipt layer, integrating ledger and GameState only after QA #10 review. No main write, merge or Issue closure.

**CI follow-up (verified):** [Core .NET run 43647817751](https://github.com/rixed318/Probably-TAU/actions/runs/43647817751), triggered by code commit `72abba2a`, is **completed / success**. The job's `Compile and run all Core golden suites` step completed successfully with the fourth Ledger suite registered. This verifies the standalone .NET 8 runner, not Unity compilation/EditMode nor cross-branch integration. The workflow job's displayed name still says 'three golden suites' (cosmetic stale label), while Program.cs actually invokes four.


## 14. AutoDev 01 — atomic Ledger batch preparation (15.11.2026)

**Scope:** one bounded additive Issue #3 implementation under C0-v2 / ADR-C0-001. Does not implement CommandProcessor, mutable GameState, obligations, payables, settlement, or Unity.

- `Runtime/Core/LedgerSnapshot.cs` commit `99ac644a`: added `WithPostings(IEnumerable<LedgerEntry>)`. It builds a detached ordered candidate, checks duplicate PostingId both against the existing ledger and within the batch (ordinal IDs), validates nulls, checked Int64 balance after **each** entry and nonnegative intermediate cash. Returns a new immutable LedgerSnapshot only when every posting succeeds. Rejected batches cannot mutate the original snapshot or expose a partially committed result.
- `Tests/Core/LedgerGoldenTests.cs` commit `3f168976`: tests two-entry success and order, empty batch, null collection/entry, duplicate existing/intra-batch IDs, insufficient funds at first/later entry, overflow after a valid first entry, unchanged source and subsequent valid append after rejection.
- Existing `Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj` compiles all Core sources and `LedgerGoldenTests.RunAll()` without runner edits.

**Baseline verification:** GitHub Actions Core .NET 8 run [43647859964](https://github.com/rixed318/Probably-TAU/actions/runs/43647859964) completed/success for preceding Core HEAD `7844ab6a` (four suites). This is **not** evidence for the new batch changes. New workflow result must be checked separately and only declared successful on completed/success. Local dotnet/Unity Editor not verified; Unity EditMode NOT RUN.

**Contract boundary:** `WithPostings` is a *preparation* helper, not a transaction commit or a replacement for durable CommandId fingerprints/receipts. Mandatory-payment shortage must still create payable via future Economy/CommandProcessor policy, not by forcing negative cash. The C0 public command contract is unchanged; this is an additive ledger API.

**Next:** verify new GitHub Actions run; then implement the smallest CommandProcessor atomic prepare/commit + idempotent receipt slice after QA #10 review. PR #9 remains Draft, no main write, no merge or Issue closure.

**Verified CI follow-up:** [Core .NET 8 run 43656370911](https://github.com/rixed318/Probably-TAU/actions/runs/43656370911) for test/code HEAD `3f168976` is **completed / success**. This verifies compilation and four Core golden suites including the new batch cases. Later documentation-only commits do not modify compiled code. Unity Editor/EditMode remains NOT RUN.


## 15. AutoDev 01 — financial command atomic slice (15.11.2026)

**One bounded Issue #2/#3 implementation:** a pure C# immutable *financial-only* command preparation/commit prototype; **not** the complete mutable GameState or public CommandProcessor/Save contract.

- `Runtime/Core/FinancialCommandProcessor.cs` (commit `804c446c`): `FinancialCommandState` holds an immutable LedgerSnapshot, revision/day and copied receipt map; `FinancialPostingCommand` holds command ID, caller-provided fingerprint, expected revision/day and defensive-copy postings; `Execute` handles prior ID first (same fingerprint replay / differing fingerprint collision), stale state, and atomic `WithPostings` plus revision increment and receipt commit. Invalid batches or overflow reject without mutating the source state.
- `Tests/Core/FinancialCommandGoldenTests.cs` (commit `68755261`): successful two-posting commit, defensive copy, replay despite stale revision, ID collision, stale revision/day, duplicate posting, partial-batch overdraft rejection, valid command after failure, balance/revision overflow and empty batch.
- `Tests/Core/Standalone/Program.cs` (commit `56cd2caa`): fifth golden suite registered in existing .NET 8 runner; csproj already includes all `Runtime/Core/*.cs` and `*GoldenTests.cs`.

**Verification at authoring:** GitHub files were committed successfully. Local `dotnet`, `csc`, `mcs` unavailable; **new CI result NOT VERIFIED yet** (commit workflow lookup returned no runs immediately after commit). Do not infer success from prior green four-suite run 43656370911. Unity Editor/EditMode NOT RUN.

**Important limitations:** The fingerprint is caller-supplied and is **not yet verified against canonical command payload**; this is therefore a transaction *prototype*, not a security or persistence boundary. Receipts are only in the in-memory immutable FinancialCommandState and are not integrated into GameStateSnapshotDto/SaveEnvelope. No external command event publication, domain metrics, scheduled outcomes, obligations/payables, or complete EndDay. Do not integrate NPC/Save against this as a final C0 public API. The accepted C0-v2/ADR-C0-001/ADR-C0-002 remain normative.

**Next:** verify real five-suite GitHub Actions run and fix failures if present. Then design a canonical fingerprint/receipt representation and connect the transaction to the single GameState owner with QA #10 review. No main write/merge/Issue closure.

**Verified CI follow-up:** [Core .NET 8 run 43664000246](https://github.com/rixed318/Probably-TAU/actions/runs/43664000246), triggered for code/test commit `56cd2caa`, completed with **success**. This confirms standalone .NET 8 compilation and five golden suites, including financial command cases; not Unity Editor/EditMode or Save→Load integration.


## 15.11.2026 — финансовые receipts: export/restore (AutoDev 01, #2)

**Одна ограниченная задача:** обеспечить сохранение и восстановление *только* in-memory финансовых receipts для `FinancialCommandState`, не вводя полноценный SaveService или новый публичный GameState контракт.

- `Runtime/Core/FinancialCommandProcessor.cs` (`e90f6825`): `ExportReceipts()` возвращает неизменяемую отсортированную по revision/CommandId коллекцию; `Restore(ledger, revision, dayIndex, receipts)` восстанавливает immutable state без повторного применения проводок. Проверяются duplicate CommandId/revision, допустимые revision и совпадение последнего receipt balance с ledger при равной ревизии. Конструктор `FinancialCommandReceipt` проверяет canonical ID, fingerprint, revision>=1, balance>=0.
- `Tests/Core/FinancialCommandGoldenTests.cs` (`8d7106bd`): проверка replay/collision после восстановления, порядка экспорта и отказа от дубликатов/подмены balance/revision.
- **Граница:** это provisional financial-only API, не версия GameStateSnapshot/SaveEnvelope. Не сериализует ledger, payables, RNG, event instances, не обеспечивает canonical fingerprint. QA #10 должен проверить contract прежде чем подключать persistence.
- **Проверка:** .NET 8 GitHub Actions workflow для commit `8d7106bd` **NOT VERIFIED** на момент записи (связанных PR runs пока не обнаружено). Предыдущий run `43664000246` прошёл, но не покрывает новые изменения. Локально dotnet/Unity Editor не запускались. Команда: `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`.
- **Следующий блокер:** полное persisted GameState с ledger/receipts и проверяемый canonical command fingerprint; не заявлять Save→Load интеграцию.
- **Безопасность:** изменена только `feature/core-2`, PR #9 остаётся Draft; main/Issues/чужие подсистемы не затронуты.


## 18. AutoDev 01 — canonical fingerprint + opt-in verified financial boundary (15.11.2026)

**Scope:** one bounded Core #2 security/determinism increment, no GameState/SaveService/Unity scaffold or change to approved C0. Prior receipt export/restore run [43672224600](https://github.com/rixed318/Probably-TAU/actions/runs/43672224600) was completed/success for previous HEAD; it is not evidence for this new code.

**Files/commits:** `Runtime/Core/FinancialCommandFingerprint.cs` (commit `c9411274`) computes deterministic versioned SHA-256 over commandId, expected revision/day and the entire ordered ledger posting sequence (IDs, source IDs, Int64 deltas) using length-prefixed UTF-8 and invariant decimal integer strings. `ExecuteVerified` rejects an incorrect caller-provided fingerprint before any replay/collision/ledger handling. `Tests/Core/FinancialCommandGoldenTests.cs` (commit `ecc2098b`) covers deterministic digest, order/amount/command/revision sensitivity, verified commit and replay, forged payload rejection without mutation and invalid null posting. Existing fifth suite stays registered to preserve the QA workflow output gate.

**Contract boundary:** this is an **opt-in** verified entry point; legacy `FinancialCommandProcessor.Execute` still accepts caller-provided fingerprints for compatibility with existing prototype tests and is **not** a secure integration boundary. New integrations must use `FinancialCommandFingerprint.ExecuteVerified`. The hash format is versioned `tau-financial-command-v1`; changes require explicit migration/ADR. No SaveEnvelope, durable full GameState receipts, payables, event queue, NPC adapter, domain events or Unity integration are claimed.

**Verification:** source committed to `feature/core-2`; latest .NET 8 workflow must be inspected for completed/success before marking this new change green. Unity Editor/EditMode NOT RUN. No main writes, merges or Issue closures.

**Next:** verify CI; then implement one bounded Core domain GameState transaction/receipt integration step after QA review.

## AutoDev 01 — 15.11.2026: strict UTF-8 financial fingerprint boundary

**Scope:** one bounded security/integrity regression in `feature/core-2`, Issue #2 / PR #9. The .NET 8 runner, Ledger and financial processor already existed; this pass did not rebuild them or touch another subsystem.

- `Runtime/Core/FinancialCommandFingerprint.cs`: strict UTF-8 encoding (`new UTF8Encoding(false, true)`) rejects malformed UTF-16 rather than replacing invalid surrogate code units with U+FFFD and potentially aliasing distinct command/posting/source IDs. `ExecuteVerified` maps `EncoderFallbackException` to `Rejected / InvalidCommandEncoding` without committing state.
- `Tests/Core/FinancialCommandGoldenTests.cs`: added malformed command ID and source ID regression checks and immutable-state rejection check; fifth golden suite remains registered in the existing runner.
- Code commits: `6a7e26b9`, `955ea70e`, `99471718`. No public C0 contract change; existing valid ASCII/UTF-8 fingerprints remain unchanged. Legacy `FinancialCommandProcessor.Execute` remains a provisional unverified boundary; integrations must use `ExecuteVerified`.

**Verification:** GitHub Actions run for latest code must be checked separately; no Unity Editor/EditMode available. Do not claim success based on earlier green runs. **Remaining:** full GameState/EndDay, durable ledger/receipt/event DTO, payables and Unity integration. No merge/main/Issue closure.


## AutoDev 01 — 15.11.2026: QA P0 strict UTF-8 fixture repair

**One bounded task, Issue #2 / PR #9:** independently reviewed QA 05 finding in PR #9 comment 6891400294 and confirmed failing Core .NET run [43686861960](https://github.com/rixed318/Probably-TAU/actions/runs/43686861960) at `FinancialCommandGoldenTests.cs:164`. The fixture used `"bad\\uD800"` (literal backslash-u characters) rather than a malformed UTF-16 string, so the expected `EncoderFallbackException` could not be raised. Fixed the fixture to `"bad" + (char)0xD800` in commit `f64d32ac`, guaranteeing a genuine unpaired high surrogate independent of C# escape rendering. Both command ID and source ID tests use the same malformed value; `ExecuteVerified` rejection and immutability assertions are preserved. **No production code or contract changed.**

**Verification:** .NET 8 workflow on the new HEAD must complete successfully before green claim. Local Unity Editor/EditMode NOT RUN. Existing financial processor remains provisional; full GameState snapshot, payables, events and SaveService still blocked. Branch only `feature/core-2`; PR Draft; no main merge/Issue closure.


## AutoDev 01 — 15.11.2026: malformed posting/source ID regression coverage

**One bounded test-only task** after confirming current branch Core .NET 8 CI `43694171144` completed/success for HEAD `594b2719`: extended `Tests/Core/FinancialCommandGoldenTests.cs` to verify that malformed UTF-16 in a posting ID is rejected by strict fingerprint computation and malformed source ID is rejected by `ExecuteVerified` as `InvalidCommandEncoding` with original financial state untouched. Commit `afce0a1f`. No production API or C0/ADR contract change. The prior green run does **not** verify this new test commit; check the new GitHub Actions workflow before claiming success. Unity Editor/EditMode NOT RUN. PR #9 stays Draft; no main write/merge/Issue closure.


## AutoDev 01 — malformed posting ID verified-boundary regression (15.11.2026)

**Scope:** one bounded Issue #2 test-only increment in `feature/core-2`. Existing strict UTF-8 code rejects malformed posting IDs during fingerprint computation, but the verified execution boundary lacked a direct assertion for this specific field.

**Commit:** `dbc7f361d3e5dd418220b431d72085078da47c52` updates `Tests/Core/FinancialCommandGoldenTests.cs` to call `FinancialCommandFingerprint.ExecuteVerified` with an unpaired UTF-16 surrogate in `LedgerEntry.PostingId`, asserting `Rejected / InvalidCommandEncoding`, identical original state, and no ledger or receipt mutation. No production API, contract, Unity, or other role changes.

**Verification:** preceding Core .NET 8 run [43701790337](https://github.com/rixed318/Probably-TAU/actions/runs/43701790337) completed/success for prior HEAD `74ecb8b0`; this does **not** validate the new test. Query workflow for this new commit before claiming PASS. Unity Editor/EditMode not run. PR #9 remains Draft; do not merge or close Issues.


## AutoDev 01 — fingerprint boundary/Unicode regression (15.11.2026)

**Scope:** one bounded Issue #2 test-only change, no public API change. Added assertions to `Tests/Core/FinancialCommandGoldenTests.cs` that length-prefixed canonical fingerprint distinguishes different field boundaries (command ID vs posting ID) and different UTF-8 Unicode identifiers. Code commit: `32a5763c394119d3df1549a87bbca88be608189f`.

**Verification:** prior GitHub Actions run [43701790337](https://github.com/rixed318/Probably-TAU/actions/runs/43701790337) completed successfully, but predates this change. New-head CI must be checked separately; Unity Editor/EditMode not run. No changes to `main`, no merge, no Issue closure. Next: verify current HEAD CI and continue integrated GameState/ledger work only after QA/00 contract review.


## AutoDev 01 — 15.11.2026: locale-invariant financial fingerprint regression (#2)

One bounded test-only change: `Tests/Core/FinancialCommandGoldenTests.cs` now compares canonical SHA-256 command fingerprints calculated under `fr-FR`, `tr-TR` and `ar-SA` against a baseline computed before culture switching, including negative Int64 delta, revision/day and Unicode identifiers. The test restores the original `CurrentCulture` in `finally`, preventing leakage into other golden tests. Commits `72c98f58`, `8edf5c0b`. No production API, approved Gate C0-v2, ADR or other subsystem changed.

Previous HEAD `3f55e042` GitHub Actions run 43718143815 completed/success, but it **does not verify** these new test commits. Check the new commit workflow until completed before claiming success. Unity Editor/EditMode not run. Full durable GameState/Ledger/receipts/events and SaveService integration remain blocked on agreed snapshot shape and QA/00 review. PR #9 remains Draft; no main merge or Issue closure.


## AutoDev 01 — 15.11.2026: verified receipt replay after restore (#2)

**One bounded test-only task:** commit `202e043947b423566d558fce915a204d71e989c5` extends `FinancialCommandGoldenTests.RunAll()` with a verified-command save/load boundary regression. Export one committed canonical SHA-256 receipt, reconstruct `FinancialCommandState.Restore`, replay the identical command via `ExecuteVerified` and assert no additional ledger entry or receipt. Reuse the same CommandId with modified postings and the original fingerprint, assert `Rejected / InvalidCommandFingerprint`, unchanged state and cash. No new public contract, production code, other subsystem or Unity scaffold changes.

**Verification:** check GitHub Actions on the exact test commit and handoff HEAD; do not infer PASS from an older workflow. Unity Editor/EditMode NOT RUN. The financial-only state is not the full durable GameState/SaveEnvelope. Next substantive blocker remains QA/00-approved complete GameState snapshot with Ledger, obligations, verified receipts and typed due events. PR #9 stays Draft; no main merge or Issue closure.


## AutoDev 01 — 15.11.2026: verified atomic overdraft rollback (#2)

**One bounded test-only task:** commit `4873559309335cfa6b1d4e65f4eee4f715e37e8c` extends `Tests/Core/FinancialCommandGoldenTests.cs`. A canonical SHA-256 verified two-posting command (first affordable, second overdraws) must reject the entire transaction without ledger, receipt, revision or cash mutation. A corrected affordable command reusing the rejected CommandId must then commit exactly once. No public API, financial policy, Unity scaffold, or other subsystem changed.

**CI:** previous handoff HEAD `e6f6f6f1` had a completed/success .NET 8 PR workflow #43736980365. This is **not** evidence for the new test commit; verify the exact new code SHA in GitHub Actions before claiming PASS. Unity Editor/EditMode NOT RUN. Full GameState snapshot and SaveService integration remain outstanding. PR #9 stays Draft, no main write/merge/Issue closure.


## AutoDev 01 — P0 durable finance slice (15.11.2026, Issue #18)

**Scope:** one production increment in `feature/core-2`, PR #9. New `Runtime/Core/DurableFinanceSnapshot.cs` composes existing GameState header and ordered Ledger with committed command receipts (command ID, persisted fingerprint, committed outcome/revision and ordered posting keys). Constructor validates cash reconciliation, unique command/revision IDs, complete ledger posting-key coverage and absence of reused/missing posting keys. `Restore()` reconstructs/validates without replaying postings, moving RNG or changing balances. Deterministic receipt ordering by revision then ordinal command ID. This is an additive intermediate Core domain slice, **not** the final SaveEnvelope or complete playable GameState.

**Tests:** extended `GameStateSnapshotGoldenTests.RunAll()` with round-trip and invalid posting coverage. Source commits `8de2e9fe42e1787189428615d0c6f48bb828af8c` and `05230a9ce6a5fe9fe9b80cecf1b1d0a8c0c532f2`. GitHub CI must be checked on the final HEAD before declaring success; Unity Editor/EditMode not executed.

**Limitations:** persisted fingerprint is retained but cannot be independently recomputed without original command payload; this slice validates key associations, **not** cryptographic proof of original payload. Existing financial processor receipts do not yet expose posting keys; a Core-owned adapter/transaction boundary is still required. Does not yet store payables, typed scheduled events, complete domain state or AdvanceDay. Coordinate with QA PR #10 and NPC PR #16 before promoting this slice to stable save wire format. No merge/main changes.

## AutoDev 01 — P0 receipt-to-ledger provenance validation (16.11.2026, Issue #18)

Production hardening of `DurableFinanceSnapshot`: each persisted posting key must resolve to a ledger entry whose `SourceId` exactly equals the claiming committed receipt `CommandId` (ordinal comparison). Previously a receipt could claim a real posting belonging to a different command and pass coverage checks. Added a targeted negative case to `GameStateSnapshotGoldenTests.RunAll()`; normal round-trip remains covered. No new wire format or foreign subsystem changes. GitHub Actions on the new SHA must finish before claiming PASS; Unity Editor/EditMode NOT RUN. Next P0: payable and typed scheduled event state, then unified AdvanceDay. No main write or merge.

## AutoDev 01 — P0 typed scheduled occurrence snapshot (16.11.2026, Issue #18)

**Scope:** one additive Core-owned production slice implementing ADR-C0-002 queue metadata without creating an alternative NPC/EventDirector or mutating GameState. `Runtime/Core/ScheduledEventQueueSnapshot.cs` persists immutable `ScheduledEventOccurrence` (InstanceId/DefinitionId/DueDayIndex/Priority), validates pending/completed ID disjointness, sorts by due ASC / priority DESC / InstanceId ordinal ASC, and provides a fixed `CaptureDueStage(closingDayIndex)` plus child due-day guard. `Restore()` revalidates and preserves deterministic order. No RNG draws, money postings, or IO.

**Test:** `Tests/Core/GameStateSnapshotGoldenTests.cs` now checks due ordering, priority ties, snapshot input isolation, restore, duplicate completed ID rejection, read-only due view, and child due<=closing-day rejection. Runner project includes `Runtime/Core/*.cs` and `*GoldenTests.cs` automatically. Production commit `7ea67d21`, test commit `55d37aac`. GitHub Actions status for final HEAD must be inspected; do not claim green before completed/success. Unity Editor/EditMode NOT RUN.

**Boundary:** This is a standalone typed event-state slice, not yet wired into `GameStateSnapshotDto` or the atomic `AdvanceDay` pipeline. Event payload, child emission, E09 report-only behavior, earned keys, finance payables and SaveEnvelope remain outside scope. QA PR #10 and NPC PR #16 should review before serialization/integration. Next P0: unified domain GameState snapshot with durable finance and event queue, then atomic AdvanceDay. No changes to main or other subsystem; PR #9 remains Draft.
