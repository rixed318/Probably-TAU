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

## AutoDev 01 — P0 durable GameState composition boundary (16.11.2026, Issue #18)

**Scope:** Core-only additive production type `Runtime/Core/DurableGameStateSnapshot.cs` composes existing `DurableFinanceSnapshot` (ordered Ledger and verified-command receipt metadata) and `ScheduledEventQueueSnapshot` (typed scheduled event occurrences). It requires exact ordinal set equality between the header's pending/completed event IDs and the typed queue's corresponding IDs. Constructor defensively reconstructs both slices; `Restore()` revalidates without executing commands, posting money, consuming RNG or firing due events. Existing public snapshot constructors remain unchanged. No competing NPC event engine or SaveService IO.

**Test:** `GameStateSnapshotGoldenTests.RunAll()` includes `DurableCompositionRoundTrip()`, asserting round-trip cash, pending/completed IDs, and rejection of mismatched pending or completed IDs. Production commit `284c55fd`; test commit `cf4b7f78`. CI for final HEAD must be verified; Unity Editor/EditMode **NOT RUN**. This is a composition boundary, **not yet a complete serializable SaveEnvelope**: payables, payloads, earned/effect keys and atomic `AdvanceDay` remain P0. QA PR #10 and NPC PR #16 should review the shape before wire-format stabilization. No main write/merge/Issue closure.

## AutoDev 01 — P0 durable unpaid mandatory liabilities (16.11.2026, Issue #18)

**Scope:** additive Core-only `DurablePayablesSnapshot` with positive Int64-kopek amounts, canonical unique due keys and deterministic due-day/ordinal-key ordering. `DurableGameStateSnapshot` now has a backward-compatible three-argument overload composing finance, events and outstanding payables; existing two-argument constructor defaults to an empty payable snapshot. `Restore()` revalidates without paying, posting Ledger entries or consuming RNG. No SaveEnvelope or settlement policy is invented.

**Tests:** `GameStateSnapshotGoldenTests.RunAll()` checks stable ordering, input isolation, duplicate due-key rejection, zero amount rejection, read-only view and composed restore preserving cash. Run with `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`. GitHub Actions status for new HEAD must be checked; Unity Editor/EditMode not run. QA #10 and NPC #16 should review the wire boundary before adoption.

**Remaining P0:** payable settlement/due-key history, typed event effects and atomic `AdvanceDay`. No main writes or merge.


## 16.11.2026 — P0 due-payables EndDay preparation (#18)

- Branch: `feature/core-2`; PR #9 Draft. Production commit `67db6760`, regression commit `432d3315`.
- Added `DurablePayablesSnapshot.CaptureDueStage(int closingDayIndex)`: returns a fixed, read-only, deterministically ordered list of obligations due on or before the closing day (due day ASC, due key ordinal ASC). Rejects negative day indices. No ledger postings, RNG draws, mutation, or debt settlement.
- Added targeted checks in `GameStateSnapshotGoldenTests.RunAll()`: due selection across day 0/1/3, stable ordering, immutable view, invalid index, original outstanding list preserved.
- This is a **preparation-only slice**, not atomic `AdvanceDay` or a payable settlement transaction. Next P0: use this fixed due-stage view in an atomic Core-owned EndDay transaction with receipts, processed-day key and insufficient-funds policy, then QA #10 / NPC #16 review.
- Verification: .NET 8 workflow for the final HEAD must be inspected before claiming PASS; Unity Editor/EditMode/PlayMode NOT RUN. No main writes, merge, or Issue closure.

## 16.11.2026 — P0 fail-closed quiet-day EndDay boundary (#18)

- `DurableDayAdvance.PrepareQuietDay` prepares an immutable replacement GameState, increments day/revision with checked arithmetic, records a canonical processed-day key, and preserves Ledger/receipts/RNG. Caller owns atomic publication.
- Fails closed for stale revision/day, finished campaign, due event/payable and weekly settlement day; **not a full AdvanceDay**. Projects, content intents, earned income, weekly charges, terminal metrics and next-day generation are not silently skipped in a claimed complete pipeline.
- Golden test covers transition, original immutability, restore, stale request, payable guard and weekly guard. Run `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release`. Unity Editor not run. Verify exact SHA CI before PASS.
- Next: integrate actual EndDay event/economy intents and settlement with QA #10 / NPC #16; no main merge.

## 16.11.2026 — P0 #18 affordable due-stage planning

Core-only production addition: `DurablePayablesSnapshot.CaptureAffordableDueStage(day, cash)` selects full mandatory obligations in deterministic due-day/ordinal order within available nonnegative cash, skips unaffordable obligations without overdraft or partial payments, returns a read-only view, and never mutates Ledger, receipts or outstanding liabilities. Targeted golden regression covers insufficient cash, later-due exclusion, immutability and invalid inputs. This is a **settlement preparation step**, not full `AdvanceDay`; atomic posting, unpaid warning, receipts, weekly accrual, event effects and checkpoint remain outstanding. QA #10 to review cash selection policy before integrating into EndDay. .NET workflow for exact commit must be checked; Unity Editor not run.

## P0 Issue #18 — atomic affordable payables EndDay (16.11.2026)

Added `DurableDayAdvance.PreparePayablesDay`: validates expected day/revision and blocks pending due events/weekly settlement, stages ordered affordable payments as Ledger postings with a single command receipt and canonical fingerprint, advances day/revision, preserves unpaid liabilities, and returns a new snapshot. Repeated/stale calls fail; original snapshot is unchanged. One golden test covers paid/unpaid split, ledger/receipt integrity, restore and stale rejection. No NPC event processing, weekly settlement, SaveEnvelope, Unity build or Editor verification. CI result must be checked for the exact commit. QA #10: review receipt and payable semantics; NPC #16: due events remain fail-closed.


## AutoDev 01 — zero-payment EndDay receipt (16.11.2026)

**Issue #18 / #3, branch feature/core-2, PR #9.** `PreparePayablesDay` now records a committed receipt and command ID even when all due obligations are unaffordable (or none are due). Previously it advanced the day without a receipt, leaving no durable command provenance for the zero-posting transition. Existing ledger and cash stay unchanged; unpaid liabilities remain outstanding. Fingerprint retains the canonical `end-day-payables-v1:` prefix with an empty selection, and the receipt contains an empty posting-key list. Added a targeted no-cash EndDay golden case verifying durable restore and stale replay rejection. This does **not** implement NPC effects, weekly settlement, or Unity integration. .NET CI exact SHA must be checked after push; Unity Editor NOT RUN. QA #10: review zero-posting receipt semantics before Save wire adoption.


## AutoDev 01 — Quiet EndDay durable receipt (16.11.2026)

Issue #18, branch feature/core-2, PR #9. A successful PrepareQuietDay now appends the same end-day command ID to ProcessedCommandIds and a zero-posting DurableCommandReceipt (fingerprint end-day-quiet-v1, committed revision). Previously only ProcessedDayKeys advanced, losing command provenance across restore. No Ledger, RNG, event, payable, or other subsystem mutation. Targeted golden assertions cover durable receipt and restore; standalone .NET runner includes this suite. Unity Editor NOT RUN. This is not the full AdvanceDay: scheduled events and weekly settlement remain fail-closed. QA #10 should review zero-posting receipt semantics.


## AutoDev 01 — unified fail-closed EndDay entrypoint (16.11.2026)

**Issue #18 / #2/#3, PR #9, branch feature/core-2.** Added `DurableDayAdvance.PrepareDay(current, expectedRevision, expectedDayIndex)` to route through the existing quiet or mandatory-payables transaction path, based on the deterministic due-stage. Rejects stale/finished state and due typed events or weekly settlement before routing; does not consume RNG or modify the original snapshot. This is an additive Core-owned entrypoint; the specialized methods remain available. No event execution, weekly income, terminal checks, project tick, SaveService, or Unity build is claimed.

**Files:** `Runtime/Core/DurableDayAdvance.cs`, `Tests/Core/GameStateSnapshotGoldenTests.cs`, this handoff. **Commits:** `d6e2e5e5` (production), `301e2854` (targeted tests), handoff commit. Existing golden tests now exercise unified entrypoint for quiet day, affordable due payments, stale revision and weekly fail-closed behavior. No local .NET/Unity toolchain; exact-head GitHub CI must be checked independently. **Next P0:** integrate NPC-owned typed due-event outcomes with Core-controlled receipts/earned-key provenance and atomic ledger/queue transition, then weekly settlement. QA #10 and NPC #16 review required. No main changes or merge.


## AutoDev 01 — settled payable restore invariant (17.11.2026)

**Issue #19 / #18; feature/core-2; PR #9.** `DurableGameStateSnapshot` now rejects an outstanding `DueKey` when its canonical `payable:<DueKey>` posting already exists in the ordered Ledger. This fail-closed validation prevents restoring an inconsistent paid-and-unpaid liability and subsequent duplicate settlement. A targeted golden test covers rejection and a distinct outstanding due key surviving restore. No NPC, UI, weekly settlement, or Unity project touched. The standalone .NET runner invokes the test through `GameStateSnapshotGoldenTests.RunAll()`. Unity/Android toolchain and local dotnet unavailable here; do not claim Unity build or local tests. Await exact-HEAD CI and QA #10 review. Android APK remains BLOCKED without verified Unity 6.3 LTS Editor/Android modules.


## 17.11.2026 — Core EndDay report projection (#19)

Added `Runtime/Core/DurableDayReport.cs`: read-only projection of a single prepared EndDay transition, with verified day/revision, ledger-prefix integrity, matching durable receipt and cash delta; exposes ordered postings and remaining payables without replaying settlement. Golden runner adds paid and quiet cases plus invalid transition rejection. No policy numbers, NPC/UI changes or new state writer. Exact .NET CI status must be checked for this commit; local .NET/Unity/Android SDK/NDK unavailable. Android APK BLOCKED pending real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK. Next: agree typed document effect IDs and command transaction with Content #11, NPC #16 and QA #10. No main merge.


## 17.11.2026 — Document decision Core transaction (#19)

Added `Runtime/Core/DurableDocumentDecision.cs` with `DocumentDecisionEffects` (caller-provided typed immediate cash and Q/S/M/E/R deltas) and `DurableDocumentDecision.Prepare`. This is a detached atomic prepare boundary: validates expected revision/day, rejects duplicate command IDs, reserves EndDay command namespace, applies checked metric bounds and Int64 kopeck ledger postings, and stores one durable receipt with a canonical effect fingerprint. No new game balance values are invented by Core; Content #11 must supply approved values, NPC #16 must bind stable document/visitor IDs and event completion through a later integration contract. The document command does NOT mark an NPC request completed or enforce that the chosen document is currently available; it must not be exposed directly to UI without a validated application handler. Targeted golden checks cover paid decision → EndDay → report, replay, stale revision, insufficient cash, bounds and restore. No Unity Editor/Android builder or local .NET SDK verified; CI for the new SHA must be checked. QA #10 review required. No main merge or Issue closure.

## 17.11.2026 — Day01 detached three-choice orchestration (#19)

Added `Runtime/Core/DurableDay01Flow.cs`: exactly three caller-approved document choices are staged through existing `DurableDocumentDecision.Prepare`, followed by canonical `DurableDayAdvance.PrepareDay` and `DurableDayReport.Capture`. Only the final detached snapshot is returned for publication; any rejected choice or EndDay leaves the original snapshot untouched. Duplicate command/decision IDs, stale revision, wrong day and invalid choice count fail closed. Golden checks exercise ordered three-choice ledger, Q/S/M/E/R, receipts, restore, EndDay, replay and rollback on insufficient funds. **This is not NPC availability/completion integration, content balance approval, or full typed event/weekly pipeline.** Do not expose the helper directly to UI until NPC/Content validation is connected. Unity 6.3 LTS Editor/Android SDK/NDK and local dotnet unavailable in this runtime; exact-SHA CI is the only available compile/test verification. Android APK remains BLOCKED. QA #10, NPC #16, Content #11 review requested. No main merge.


## AutoDev 01 — Day01 duplicate decision regression (17.11.2026)

**Scope:** one bounded test-only increment for Issue #19; no new Core API, NPC/Content contract, economy rule or Unity scaffold. Added a golden regression to `Tests/Core/GameStateSnapshotGoldenTests.cs` for distinct CommandId values referencing the same DecisionId, plus checks that rejected Day01 preparations do not publish partial Ledger, receipt, revision or metric changes. Also checks replay against an already advanced DayIndex. Existing `DurableDay01Flow.Prepare` remains the single Core-owned staged path.

**Verification:** test source updated; local C# execution and Unity Editor compile unavailable in this environment. GitHub Actions result for this exact commit must be checked separately. Android APK remains blocked until real Unity 6.3 LTS and Android Build Support/SDK/NDK are available. PR #9 remains Draft; no main merge or Issue closure.


## AutoDev 01 — Day01 late-stage validation regressions (17.11.2026)

**Issue #19 / PR #9, bounded tests only.** Added golden regressions for a third document choice that exceeds the Q/S/M/E/R metric range and for a document choice attempting to use the reserved `end-day:` command namespace. Both must fail before publishing any staged Ledger, receipt, revision or metric changes. No production API, NPC/Content contract, economic policy or Unity scaffold changed.

**Verification:** C# golden runner test cases authored; local dotnet/Unity/Android toolchains unavailable, so local compilation and APK are BLOCKED. Check exact-commit GitHub Actions before claiming tests passed. Remaining P0: NPC document availability/completion contract and typed Content effect binding; no main merge or Issue closure.


## AutoDev 01 — Day01 due-event fail-closed regression (17.11.2026)

**Issue #19 / PR #9; test-only increment.** Added a golden regression for three staged Day01 document decisions followed by an EndDay blocked by a due scheduled event. The expected `InvalidOperationException` must leave the input snapshot's revision, day, balance, Ledger, receipts and pending event unchanged. No new production API, NPC/Content contract or financial policy; event processing still requires a separately agreed handler.

**Commit:** `8991642235a91c25fa8741c4cbf2819d4f9b6427` (test file); this handoff update follows as a documentation commit on the same feature branch.

**Verification:** local dotnet/Unity/Android toolchains unavailable; C# runner and Unity Editor not executed locally. Exact-SHA GitHub Actions result must be checked before claiming test success. Android APK remains BLOCKED until real Unity 6.3 LTS and Android Build Support/SDK/NDK are available. PR remains Draft; no merge or Issue closure.


## AutoDev 01 — Zero-cash document receipt regression (18.11.2026)

**Issue #19 / PR #9; bounded Core golden-test increment.** Added a zero-cash document-decision case to `DocumentDecisionTransactionGolden`: revision and Q/S/M/E/R update, one durable receipt with no posting keys, no zero-amount Ledger entry, Save→Load receipt persistence and duplicate command rejection. The input snapshot remains unchanged. No production API, balance policy or NPC/Content contract changes.

**Verification:** C# test authored but not locally executed (no dotnet); Unity Editor/Android Build Support/SDK/NDK/adb absent, so Unity compile and APK are BLOCKED. Exact-SHA CI must be checked independently. Android #20 and real Day01 NPC document binding #19 remain outstanding; no main merge or Issue closure.


## 18.11.2026 — Core 01 / Day01 payable EndDay golden regression

- Issue #19; branch `feature/core-2`; test commit `d5df8ef96296d85f75424cc2502b5a8406a07b6c`.
- Added a bounded regression to `Day01FlowGolden`: three approved document choices followed by one affordable due payable, canonical EndDay ledger posting and receipt, report projection, snapshot Restore, and replay rejection. Checks original snapshot stays unchanged.
- No new production contract, content financial rule, NPC document ownership logic, Unity scaffold or Android build.
- Local `dotnet`, Unity Editor, Android SDK/NDK and adb not found (Java/Javac present). Test has NOT been executed locally; exact-SHA GitHub CI must be checked before claiming PASS.
- Remaining integration dependency: NPC/Content-approved typed document availability/completion and due-event/weekly EndDay support. PR #9 stays Draft; no main merge.

## AutoDev 01 — Day01 insufficient payable golden regression (18.11.2026)

**Issue:** #19 / #3. Added one bounded golden regression in `Day01FlowGolden`: after three document decisions leave 750 kopecks, a due payable of 800 kopecks remains outstanding; EndDay advances exactly once, emits a receipt with no posting keys, creates no extra ledger posting, and reports the unpaid payable. Restore preserves the debt and replay rejects; original snapshot stays unchanged. No new policy or public API; no NPC/UI/Content changes. Local dotnet/Unity/Android toolchain unavailable; source-only review, exact-SHA CI to be checked. No Unity build or APK claimed.

## AutoDev 01 — mixed affordable/unaffordable Day01 payables (18.11.2026)

**Issue #19 / #3; PR #9; feature/core-2.** Added one bounded golden regression to `Day01FlowGolden`: after three document choices leave 750 kopecks, EndDay pays a 200-kopeck due liability and preserves an 800-kopeck liability. Checks deterministic Ledger posting and receipt, day/revision/cash, report paid/unpaid projections, Restore, replay rejection and immutable input. No production API, balance rules, NPC/UI/Content files or Unity scaffold changed. Local dotnet/Unity Editor/Android SDK/NDK/adb absent (Java/Javac present), so local C# execution, Unity compile and Android APK remain BLOCKED. Check exact-SHA GitHub CI before claiming pass. No merge or issue closure.


## AutoDev 01 — payable input-order determinism (18.11.2026)

**Issue #19 / #3; PR #9; feature/core-2.** Added one golden regression to `Day01FlowGolden`: the same two Day01 due liabilities supplied in reverse order must produce identical post-decision cash, settlement posting ID, EndDay receipt fingerprint and unpaid payable. Input snapshots remain immutable. This guards canonical due-key ordering during Day01 EndDay without modifying production rules or public contracts. Local .NET SDK and Unity 6.3 Android build toolchain are unavailable; C# tests and APK not executed locally. Verify exact-SHA CI; no main merge or issue closure.


## AutoDev 01 — future payable boundary at Day01 EndDay (18.11.2026)

**Issue #19 / #3; PR #9; branch `feature/core-2`.** Added a bounded golden regression in `Day01FlowGolden`: a 200-kopeck liability due on DayIndex 1 must NOT settle when DayIndex 0 closes after three decisions. The EndDay receipt has no payable posting, cash remains at the post-decision 750 kopecks, the future obligation survives Restore, and the original snapshot stays immutable. This guards the due-day boundary without changing production API, payment policy or NPC/Content contracts.

**Test commit:** `91fb44393ed54689c13e8b873ea989fdd10550eb`. Local .NET SDK and Unity Editor/Android Build Support/SDK/NDK/adb are unavailable, so local C#/Unity tests and Android APK are BLOCKED; check exact-SHA CI before claiming PASS. Still outstanding: typed NPC/Content document binding, due-event handling, weekly EndDay and real Android build. No main merge or Issue closure.


## AutoDev 01 — mixed due-now/future payable Day01 regression (18.11.2026)

**Issue #19 / #3; PR #9; feature/core-2.** Added a single golden regression to `Day01FlowGolden` for a mixed payable queue: a 200-kopeck obligation due on DayIndex 0 and a 100-kopeck obligation due on DayIndex 1. After three existing approved test choices, EndDay must settle only the due-now obligation, leave 550 kopecks, emit one payable ledger posting and one EndDay posting key, retain the future obligation, and preserve it through Restore. The input snapshot must remain unchanged. No production contract, financial policy, NPC or Content subsystem changed.

**Test commit:** `b4bde5e9022a5290afee273119f021edfa5b21b0`. Local .NET SDK and Unity Editor/Android Build Support/SDK/NDK/adb absent: C# tests and Unity/Android build **NOT RUN** locally. Check exact-SHA CI; no APK, no main merge or Issue closure. Remaining integration blockers: typed NPC/Content documents, due-event handling, weekly EndDay, actual Unity/Android toolchain.


## AutoDev 01 — three non-monetary Day01 choices (18.11.2026)

**Issue #19 / PR #9 / branch feature/core-2.** Added one bounded golden regression to `Day01FlowGolden`: three distinct document decisions with zero cash effects must advance Day01 once, increment revision four times, apply Q/S/M metric deltas, preserve 1000 kopecks, create four durable receipts with empty posting keys and zero Ledger entries, survive Restore, reject replay and leave the input snapshot immutable. Test commit: `e442cb6c3eb0be59b60ba7460906929034f12595`.

**Scope:** tests only; no production API, economic policy, NPC/Content mapping or Unity scaffold. **Verification:** local dotnet SDK and Unity 6.3 LTS Editor/Android Build Support/SDK/NDK/adb absent; C# runner and Unity/Android builds NOT RUN locally. Exact-SHA GitHub CI must be checked. Android APK BLOCKED. Pending real Day01 NPC document integration and due-event/weekly EndDay handling. No main push/merge or Issue closure.


## Core 01 incremental handoff — 18.11.2026, deterministic Day01 Prepare replay (#19)

- Test commit: `dd1e8431b38453aa986c82d92f237b3a34a9fda4` on `feature/core-2`; PR #9 remains unmerged.
- Added a bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`: two `DurableDay01Flow.Prepare` invocations against the same immutable payable-bearing Day01 snapshot and identical ordered document choices must yield matching revision, balance, ledger posting IDs and running balances, and all durable receipt fingerprints. Original snapshot stays unchanged.
- No production API, economy policy, content/NPC, Unity project or main changes.
- Local checks on this run: `dotnet`, `mono`, Unity Editor, Android SDK/NDK/adb not found in executable PATH; Java/Javac found. Thus .NET tests, Unity compile, and Android APK **not run / BLOCKED**. Exact-SHA CI outcome requires independent GitHub Actions verification; no passing claim.
- Remaining: NPC typed documents/events and EndDay due-event handling, real Unity 6.3 Editor + Android toolchain, independent QA 05 review. Do not merge without owner approval.


### Core 01 — Day01 future scheduled event regression (18.11.2026)
- Issue #19; branch `feature/core-2`; test commit `ef3e93157a2ccff907b8d83f401fbbd444a6d1e6`.
- Added one bounded golden regression to `Tests/Core/GameStateSnapshotGoldenTests.cs`: an event due on DayIndex 1 must not block closing DayIndex 0 after three document decisions. The pending event remains queued and unconsumed across `Restore()`; the original durable snapshot is not mutated. Complements the existing due-now event fail-closed test.
- No production API, economy policy, NPC/Content logic, or Unity project changes.
- Verify: `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release` in a .NET 8 environment; inspect exact-head CI. Local runtime lacks dotnet/mono, Unity Editor, Android SDK/NDK/adb; no local C# run, Unity build, or APK claimed.
- Still blocked: typed NPC event execution and native Unity Android build require cross-role contracts and real Unity 6.3/Android toolchain. No main merge or Issue closure.


## Core 01 — future event + due payable coexistence (18.11.2026)

**Issue #19 / PR #9 / feature/core-2.** One bounded C# golden regression added to `Day01FlowGolden` in `Tests/Core/GameStateSnapshotGoldenTests.cs`, test commit `2d7e85aca945a70bce8c9613ebe263405fda3e61`. After three existing document decisions, EndDay settles a due-now 200-kopeck payable while a scheduled event due on DayIndex 1 remains pending. Assertions cover 550-kopeck balance, single payable posting and EndDay receipt, zero outstanding payable, event queue preservation, Restore and immutable input. No new Core API, financial policy, event handler, NPC/Content/UI code or Unity scaffold.

**Verification:** local dotnet and Unity 6.3 Editor/Android Build Support/SDK/NDK/adb unavailable (Java/Javac present); C# test not run locally and Android APK BLOCKED. Run `dotnet run --project Tests/Core/Standalone/TAU.Core.GoldenRunner.csproj --configuration Release` with .NET 8 and check CI for the exact handoff HEAD. Due-now event execution still fails closed pending approved typed event contract. QA #10/00 review required; no main push/merge or issue closure.


## AutoDev 01 — due-event + payable atomic rollback (18.11.2026)

- Scope: Issue #19, test-only regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`; commit `a7b71e84b3eafc32881b36c7932725ee95c6127a`.
- Scenario: three valid Day01 document choices, an event due at DayIndex 0 and an affordable payable of 200 kopecks due the same day. The canonical EndDay rejects the unresolved due event. The original durable snapshot must retain revision/day/balance, empty ledger and receipts, pending event and unpaid payable; Restore must preserve both pending items.
- Contract/policy: no production API, NPC, content, financial rule, Unity scaffold or main changes. This tests atomic failure before due-payable settlement; it does **not** implement scheduled-event resolution.
- Verification: GitHub source update confirmed. Local C# test **NOT RUN** (dotnet/csc/mcs/mono unavailable); Unity Editor and Android SDK/NDK/adb unavailable, so Unity compilation and APK **BLOCKED**. Java is present. Exact-SHA CI must be checked independently.
- Next: QA 05/00 review and real NPC event resolution contract before any production integration. PR #9 remains Draft; no merge or issue closure.

## Core 01 — deferred event + unaffordable Day01 payable (18.11.2026)

- Issue #19 / #3; PR #9; branch `feature/core-2`. Test commit `4452285ed8054d9527806f11fa9d3686673e1027`.
- Added one bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`: after three choices leave 750 kopecks, a due-now 800-kopeck liability remains unpaid while a DayIndex=1 event stays queued. Assert no EndDay payable posting, no duplicate receipts, unpaid DayReport projection, durable Restore of both debt and event, replay rejection and immutable input.
- No production contracts, economy policy, NPC/Content/UI files or Unity scaffold changed.
- Local toolchain check: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` not found; Java/Javac available. C# test NOT RUN locally; Unity compile and Android APK BLOCKED. Check exact-head GitHub Actions before claiming test success.
- Remaining integration: typed NPC event execution and canonical content mapping, independent QA/00 review and real Unity 6.3 Android toolchain. No main push/merge or Issue closure.

## Проход 18.11.2026 — Day02 restored due-event guard (#19)

- Commit теста: `fee584cab8987b3585f1a58faa289a4a13e168b7` (только `feature/core-2`).
- В `Tests/Core/GameStateSnapshotGoldenTests.cs` дополнен сценарий: Day01 переносит событие на DayIndex=1, Restore сохраняет его, затем попытка EndDay на DayIndex=1 отклоняется до обработки NPC-события. Проверяется отсутствие частичных изменений day/revision/cash/Ledger/receipts/queue.
- Это **регрессионная проверка защитного поведения**, а не реализация NPC event consumption. Контракты, production API и экономика не изменены.
- Локально: `.NET SDK`, Unity Editor 6.3 LTS, Android SDK/NDK и `adb` отсутствуют; `java` доступна. C# тесты/Unity build/APK **не запускались**. Нужна CI-проверка exact SHA и QA review.
- Не выполнялись merge в main, закрытие Issues или изменения чужих подсистем.


## Core 01 — fix CI CS1061 in deterministic Day01 replay test (18.11.2026)

- Issue #19 / PR #9 / branch `feature/core-2`. GitHub Actions Core .NET 8 golden runner #44364185017 on prior HEAD `3ecd82c8cfb69fee0ffdf51cb6ee5367048b91bc` **FAILED** compilation with CS1061 at `Tests/Core/GameStateSnapshotGoldenTests.cs:746-747`: `LedgerEntry` has no `BalanceAfterKopecks` property.
- Bounded repair commit `94b64faf28c5e3775ca69edf768b6aeeecef9651`: remove the invalid per-entry running-balance assertion from the deterministic Prepare replay golden test. Keep per-entry posting-ID equality, total balance/revision, entry counts and all receipt fingerprints. No production Core API or economy policy changes.
- Verification: prior CI failure confirmed from actual job logs; new SHA CI **not yet verified**. Local .NET SDK, Unity Editor, Android SDK/NDK/adb unavailable; no local C# execution, Unity compile or APK. Recheck exact-head CI and QA review before any readiness claim.
- NPC integration remains separately blocked by NPC PR #16 CS0136 according to QA #10; no NPC file changes. No main push/merge or Issue closure.


## 19.11.2026 — Day01 staggered-payable CI failure isolation (#19)

**One bounded task:** prior exact-head Core .NET 8 workflow [44383622311](https://github.com/rixed318/Probably-TAU/actions/runs/44383622311) failed inside the multi-predicate staggered payable assertion in `Day01FlowGolden`. Changed only `Tests/Core/GameStateSnapshotGoldenTests.cs` in commit `aea5a31c0a427b1e2c03d5853581696116f115a9`: replaced that compound assertion with individually named checks and added `AssertNamed` to print the first failed invariant. This is **diagnostics only**: no production code, economic policy, test expectation, public C0 contract, or other subsystem changed. Do not treat it as a fix to the underlying failing assertion.

**Verification:** local `dotnet`, Unity Editor and Android SDK/NDK/adb not found (Java available). .NET tests and Unity/Android build **NOT RUN locally**. Exact new commit CI must run and identify the failing predicate before implementing a targeted fix. APK **BLOCKED**. No main push, merge, or Issue closure.


## Core 01 — Day01 future payable report CI repair (19.11.2026)

- Issue #19 / PR #9, branch `feature/core-2`. Exact-head .NET 8 workflow run #44417008746 failed at `staggered report unpaid count` in `Day01FlowGolden` (job 135199824350); compilation and other suites succeeded.
- Root cause: test expected `Report.UnpaidPayables.Count == 0` while `DurableDayReport.Capture` deliberately projects **all** remaining outstanding payables from the closed snapshot, including future DayIndex=1 liabilities. A future `day02-future` payable remains after Day01 and therefore report count must be 1.
- Changed only the test expectation to 1 and added an explicit `DueKey == "day02-future"` assertion. No production API, settlement policy, NPC/Content, or Unity project changes.
- Test fix commit: `5ddc7a9dded496483d3fde791d0e0d0bbd1dec68`. Local .NET SDK, Unity Editor and Android SDK/NDK/adb unavailable (Java/Javac present); .NET tests and Unity/Android build **NOT RUN locally**. Exact-head CI result pending. Android APK BLOCKED. No main push/merge or Issue closure.


## Core 01 — exact-head CI verification (19.11.2026)

**Issue #19 / PR #9 / `feature/core-2`.** Verified GitHub Actions `Core .NET 8 golden tests` run [#44431140510](https://github.com/rixed318/Probably-TAU/actions/runs/44431140510): `status=completed`, `conclusion=success` for commit `1c5cf0064133a13c59a468750343321819e794ac`. This verifies the previously failing staggered Day01 payable-report assertion after test correction (`Count == 1` with future DueKey `day02-future`) in the standalone CI suite; it does **not** verify Unity Editor compilation, Unity EditMode/PlayMode, Android packaging, or NPC integration. No new C# changes in this verification pass; CI validation is the single bounded task. Local environment check: dotnet/mono/csc/Unity Editor/adb/sdkmanager absent; Java present. Android APK remains BLOCKED by missing real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK toolchain verification. Follow-up P0: approved NPC/Core typed due-event integration and Day01 document binding, with QA/00 review; no main merge or Issue closure.


## Core 01 — simultaneous future event and payable Day01 regression (19.11.2026)

- Issue #19 / PR #9 / branch `feature/core-2`. Test commit `37877bc41ee9872470d8d728eacf69d5867b9106`.
- Added one bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`: a scheduled NPC event and a mandatory payable both due on DayIndex=1 survive three approved Day01 decisions and EndDay on DayIndex=0. Day01 cash stays at 750 kopecks; no payable posting is generated; the report still projects the future outstanding payable. Both survive Restore, the event is not marked completed, and original input is immutable.
- No production API, financial policy, event handler, NPC/Content/UI changes, or fabricated Unity project. This does **not** implement due-event consumption.
- Local toolchain check: `dotnet`, Unity Editor, Android SDK/NDK/adb unavailable; Java/Javac available. Local .NET golden tests, Unity build and Android APK **NOT RUN**. Exact-head CI verification and QA/00 review required; do not claim PASS before workflow success. No main push, merge, or Issue closure.


## Core 01 — Day02 deferred payable settlement regression (19.11.2026)

- Issue #19 / PR #9 / `feature/core-2`; test commit `4af4bd394c0a0b3fae0723aa506e4cddc9803491`.
- One bounded C# golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`: an affordable 200-kopeck payable due on DayIndex=1 is not charged during Day01 (after three choices, cash 750); on Day02 canonical `DurableDayAdvance.PrepareDay` settles it once (cash 550), records exactly one posting/receipt, removes liability, survives Restore, rejects stale replay and leaves source snapshots immutable.
- No due NPC events in this fixture. This does **not** implement NPC due-event handling or change public C0/ADR contracts, production rules, UI, Content or other subsystems.
- Verified preceding HEAD `4323078d6c15de8a1a32386ca108e3cc7ec65f02` via [Core .NET 8 golden run #44466951168](https://github.com/rixed318/Probably-TAU/actions/runs/44466951168): completed/success. **New test SHA CI not yet verified**; do not claim its PASS until exact-head run completes.
- Local toolchain check: `dotnet`, `mono`, `csc`, Unity Editor, `adb`, `sdkmanager` unavailable; Java/Javac available. Local C# tests and Unity/Android build NOT RUN; real APK BLOCKED by missing Unity 6.3 LTS Editor and Android SDK/NDK/Build Support.
- Next: exact-head CI + QA/00 review, approved typed NPC event handler contract for Day01/EndDay. No main push, merge, or Issue closure.


## Core 01 — exact-head Day02 deferred payable CI verification (19.11.2026)

- Issue #19 / PR #9 / `feature/core-2`. One bounded verification task: checked GitHub Actions `Core .NET 8 golden tests` run [#44479241744](https://github.com/rixed318/Probably-TAU/actions/runs/44479241744), `status=completed`, `conclusion=success`, associated with code/handoff HEAD `d3b7bca3195bc9c9baabde31d2232d292b0594a4` (test commit `4af4bd394c0a0b3fae0723aa506e4cddc9803491`). Thus the new Day02 deferred-payable golden regression passed in CI; no C# code changes needed for this pass.
- Local toolchain rechecked: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java` and `javac` present. No local C# tests, Unity Editor compilation, Windows player build or Android APK. Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK verification and native APK remain BLOCKED.
- Remaining P0: approved typed NPC due-event transaction contract / Core EndDay integration, canonical Day01 content binding, independent QA/00 review. No new API or other subsystem changes, no push/merge to main, no Issue closure.

## Core 01 — Day02 insufficient mandatory payable regression (19.11.2026)

- Issue #19 / PR #9, branch `feature/core-2`. One bounded C# golden-test increment, commit `228fc781246babe30e78ab98427482f62ede416a`, in `Tests/Core/GameStateSnapshotGoldenTests.cs`.
- Fixture: three Day01 choices leave cash 750 kopecks; future mandatory liability `day02-shortfall` is due on DayIndex=1 for 800 kopecks. After Day01 and Restore, Day02 EndDay must keep cash 750, retain the outstanding payable, add no payable Ledger posting, preserve receipts count and survive Restore without mutating source snapshots. No NPC due-event is present; this does not implement NPC consumption.
- Prior branch HEAD `a297b394677a9a34ab8272be3d673f56570eea2a` has associated Core .NET 8 workflow run #44492731732 with conclusion success. **New test commit CI is not yet verified**; do not claim PASS until exact-code-SHA workflow succeeds.
- Local toolchain rechecked: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` available. Local .NET tests, Unity build and real Android APK NOT RUN / BLOCKED. No production contracts, economic policy, NPC/UI/Content or main changes. No PR merge or Issue closure.

## Core 01 — exact-head Day02 insufficient-payable CI verification (19.11.2026)

- Issue #19 / PR #9 / `feature/core-2`. One bounded verification task: GitHub Actions [Core .NET 8 golden run #44508143544](https://github.com/rixed318/Probably-TAU/actions/runs/44508143544), associated with HEAD `9687b7f80eb8b5d61db62a0507b54eae28b05b44` (test commit `228fc781246babe30e78ab98427482f62ede416a`), reports `status=completed`, `conclusion=success`. This verifies the insufficient-funds Day02 golden regression in standalone .NET 8 CI, not in Unity.
- No C# code or public contract changes in this pass. Regression keeps 800-kopeck payable outstanding against 750-kopeck cash without negative cash or extra Ledger posting; Restore behavior is covered by the test.
- Local environment rechecked: `dotnet`, `mono`, `csc`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` present. Local C# tests and Unity/Windows/Android builds NOT RUN. Real Android APK remains BLOCKED on Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK validation.
- P0 remains approved typed NPC due-event handler / atomic EndDay integration and QA/00 review; no main push, merge or Issue closure.

## Core 01 — Day02 insufficient-payable replay guard (19.11.2026)

- Issue #19 / PR #9, `feature/core-2`: bounded golden-test improvement commit `d5f58635589b7bf5adbb59626a3b7012ae61c8c0`. After an unaffordable 800-kopeck Day02 liability against 750-kopeck cash, restoring the committed snapshot and replaying EndDay with stale DayIndex=1 must throw `InvalidOperationException` and leave cash, day index, payable, Ledger and receipt count unchanged. No production API, contract, NPC logic or economic policy changes.
- Local toolchain: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. Local .NET/Unity/Android tests NOT RUN; APK BLOCKED. Exact-new-SHA CI and QA/00 review required before claiming PASS. No main push, merge or Issue closure.

## Core 01 — Day02 insufficient-payable stale replay exact-SHA CI verification (19.11.2026)

- Issue #19 / PR #9 / `feature/core-2`. Verified exact source HEAD `ec181538cee535a87daba109c08566a4186ffcc6` using [Core .NET 8 golden workflow #44539707466](https://github.com/rixed318/Probably-TAU/actions/runs/44539707466): `status=completed`, `conclusion=success`. This validates the Day02 unaffordable payable stale EndDay replay regression from test commit `d5f58635589b7bf5adbb59626a3b7012ae61c8c0` under standalone .NET 8 CI only; not a Unity Editor test.
- One bounded task this pass: exact-SHA CI verification and handoff. No new C# code, economy policy or contract changes. QA/00 independent review remains pending.
- Toolchain checked locally: `dotnet`, `mono`, `csc`, `Unity`, `unity-editor`, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. No local C# tests, Unity import/compile, Windows build or Android APK; Android remains BLOCKED until real Unity 6.3 LTS Editor, Android Build Support/SDK/NDK/OpenJDK are available and verified.
- Next P0: approved typed NPC due-event atomic EndDay transaction and Day01 integration, without cross-subsystem edits. No main push/merge or Issue closure.


## Core 01 — latest handoff HEAD standalone CI verification (19.11.2026)

- Issue #19 / PR #9 / `feature/core-2`. Verified workflow run [#44557150316](https://github.com/rixed318/Probably-TAU/actions/runs/44557150316) associated with exact source commit `3a81186f1c442e5bf3c711e991caf154e0b5f352`: `completed/success` for Core .NET 8 golden runner. This is standalone Core CI, **not** Unity Editor compilation or an Android APK.
- Bounded scope this pass: check latest CI and record reproducible evidence. No C# production or test changes, no new public contracts, no changes to NPC/UI/QA subsystems.
- Environment rechecked: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` present. Local Core tests and Unity/Android builds **NOT RUN**; Android APK **BLOCKED** on real Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK validation.
- P0: typed NPC due-event processing with atomic EndDay/ledger/receipts per Gate C0-v2 and ADR-C0-001; coordinate handler contract before implementation. Independent QA/00 review required. No main push, merge, or Issue closure.


## Core 01 — exact-HEAD CI verification (19.11.2026, 22:59 Samara)

- Issue #19 / PR #9 / `feature/core-2`. GitHub Actions [Core .NET 8 golden tests run #44576155664](https://github.com/rixed318/Probably-TAU/actions/runs/44576155664) for exact previous HEAD `acaf0b59efbbe95c830b0850cdbabbdb40472336`: `completed/success` (run 239). This verifies standalone .NET Core golden tests only; no Unity Editor or Android build evidence.
- Bounded task this pass: verify exact-HEAD CI and record evidence; no C# source, contract, NPC/UI/QA or economy changes. Independent QA/00 review and approved typed due-NPC event transaction remain pending.
- Toolchain check in current runtime: `dotnet`, `Unity`, `unity-editor`, `adb`, `sdkmanager` absent; `javac` available. Local C# tests, Unity Editor/Windows build and Android APK NOT RUN. Android remains BLOCKED on real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK verification.
- No push/merge to `main`, PR merge or Issue closure.


## Core 01 — CI verification for 20.11.2026

- Issue #19 / PR #9 / branch `feature/core-2`. Verified exact previous HEAD `bd8db10c0562ebcaedad27bf1a0ec6fb25d9a272` with [Core .NET 8 golden run #44593617110](https://github.com/rixed318/Probably-TAU/actions/runs/44593617110): `completed/success`, run 241. This validates standalone .NET tests only, not Unity compilation or an APK.
- One bounded verification task; no new C# code, API or economic-policy changes. NPC due-event transaction remains dependent on coordinated typed handler contract and QA/00 review. Avoid creating speculative public interfaces.
- Current runtime: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. Local .NET tests, Windows/Unity build and Android APK NOT RUN; Android BLOCKED on Unity 6.3 LTS Editor plus Android Build Support/SDK/NDK/OpenJDK validation.
- No main push/merge, PR merge or Issue closure. Subsequent CI for this documentation-only HEAD is not claimed successful.


## Core 01 — exact-head CI verification (20.11.2026, 02:19 Samara)

- Issue #19 / PR #9 / `feature/core-2`: GitHub Actions [Core .NET 8 golden tests #44610651034](https://github.com/rixed318/Probably-TAU/actions/runs/44610651034) for prior exact HEAD `b2d8aa9bc3e4762046a1ba8099606f5f67456429` returned `completed/success`. Standalone .NET CI only, not Unity Editor verification.
- One bounded verification task; no C# code, public contract, NPC/UI/Content or economy changes. Typed NPC due-event EndDay transaction remains pending agreed interface and QA/00 review; do not invent event API.
- Current environment checked: `dotnet`, `csc`, `mcs`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` available. Local C# tests, Unity Editor/Windows and Android APK builds NOT RUN. Native Android BLOCKED on verified Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK.
- No main push, merge or Issue closure. CI for this handoff-only commit must be checked separately.

## Core 01 — 20.11.2026: Day02 stale-revision guard regression (#19)

- Scope: one additional pure-C# golden assertion in `Tests/Core/GameStateSnapshotGoldenTests.cs`; code commit `6c6ccad859a48f3b927fa3fa80c8ffb67d578e35` on `feature/core-2` only.
- Scenario: after Day01 three decisions, Day02 pending 200-kopeck payable, `DurableDayAdvance.PrepareDay` receives correct DayIndex=1 but stale `expectedRevision=Header.Revision-1`. Expect `InvalidOperationException` and unchanged day, cash (750), payable, ledger (3 entries) and receipts (4).
- Prior HEAD `2779ea1a0003bc8d30e6504b092b16f2593136ff`: GitHub Actions run #44627577277 `completed/success` (standalone .NET 8 golden tests). New code SHA CI not yet verified; do not mark new regression PASS before exact-SHA CI.
- Environment checked this run: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. Local C# tests, Unity Editor build and Android APK NOT RUN / BLOCKED. No Unity project fabricated.
- No public API, NPC, UI, Content, `main`, merge or Issue closure changes. Next P0 remains approved typed NPC due-event integration in atomic EndDay after contract review and QA/00 signoff.

## Core 01 — 20.11.2026: exact-HEAD Day02 stale-revision CI verification

- Issue #19 / PR #9 / `feature/core-2`. Verified exact code-and-handoff HEAD `44b200f3bf9a1bb7ab902bee78d90b326c4aa375` with [Core .NET 8 golden run #44641696137](https://github.com/rixed318/Probably-TAU/actions/runs/44641696137): `completed/success` (run 249). This covers the Day02 stale-expectedRevision regression introduced by `6c6ccad859a48f3b927fa3fa80c8ffb67d578e35` in standalone CI, **not** Unity compilation.
- Single bounded task this pass: verify prior exact-head CI and record evidence. No new C# code or public API; no edits to NPC/UI/Content/QA.
- Current runtime checked: `dotnet`, `csc`, `mcs`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java` and `javac` available. Local C# tests, Unity/Windows build, Android APK NOT RUN. Native APK remains BLOCKED pending real Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK.
- Next P0: coordinate typed NPC due-event EndDay integration contract with 02/00, then implement atomically with Ledger/receipts and independent QA review. No main push/merge or Issue closure. CI for this new documentation-only HEAD must be checked separately.

## Core 01 — 20.11.2026: exact-HEAD CI verification (07:21 Samara)

- Issue #19 / PR #9 / `feature/core-2`: confirmed GitHub Actions [Core .NET 8 golden run #44654812624](https://github.com/rixed318/Probably-TAU/actions/runs/44654812624) for prior exact HEAD `f4a3fc1678e37824835326c8b6a8f233da5f211a`: `completed/success`. Standalone .NET tests only, **not** Unity Editor/Android validation.
- Bounded scope: exact-SHA CI verification and handoff only. No new C# implementation, public API, NPC/UI/Content or economy policy changes. Typed NPC due-event atomic EndDay integration remains pending approved interface and QA/00 review.
- Runtime toolchain: `dotnet`, Unity Editor, Android `sdkmanager`/`ndk-build` unavailable; `java`/`javac` present. No local C# tests, Unity import/compile, Windows build or Android APK. APK BLOCKED until verified Unity 6.3 LTS Editor, Android Build Support, SDK/NDK/OpenJDK.
- No `main` push, merge, PR merge or Issue closure. New documentation-only HEAD CI not yet verified.

## Core 01 — 20.11.2026: exact-HEAD CI verification (08:59 Samara)

- Issue #19 / PR #9 / `feature/core-2`: GitHub Actions [Core .NET 8 golden run #44667211614](https://github.com/rixed318/Probably-TAU/actions/runs/44667211614) for exact previous HEAD `9a185255e7c33e9621e6f8ebb3582476191b5de2` reports `completed/success`. This is standalone .NET CI only; no Unity Editor or Android validation.
- Bounded task: verify existing golden CI and record reproducible evidence. No new C# code or public contract changes; typed NPC due-event EndDay integration still needs coordinated contract and QA/00 review.
- Runtime checked: `dotnet`, `csc`, `mcs`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` present. Local C# tests, Unity/Windows builds and Android APK NOT RUN; APK BLOCKED pending actual Unity 6.3 LTS Editor, Android Build Support, SDK/NDK/OpenJDK.
- No changes to `main`, no merge or Issue closure. CI for this handoff-only commit is not yet verified.

## Core 01 — 20.11.2026: exact-HEAD CI verification (10:39 Samara)

- Issue #19 / PR #9: confirmed standalone Core .NET 8 golden CI [run #44678009917](https://github.com/rixed318/Probably-TAU/actions/runs/44678009917) for exact previous `feature/core-2` HEAD `e12de1416ab97359801740167b1a2de85d5e27af`: `completed/success`. No Unity compilation or Android validation implied.
- One bounded verification task; no new C# code or contract changes. Typed NPC due-event atomic EndDay still requires coordinated handler contract with 02/00 and independent QA.
- Current environment: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `javac` present. Local tests, Windows/Unity build and Android APK NOT RUN; APK BLOCKED pending actual Unity 6.3 LTS Editor, Android Build Support, SDK/NDK/OpenJDK.
- No main push/merge or Issue closure. New handoff-only commit CI not yet verified.

## Core 01 — 20.11.2026: verified exact-HEAD CI (12:16 Samara)

- Issue #19 / PR #9: verified `feature/core-2` HEAD `95a788fe5cb631bc6d99c7d1b7ce26b72bb89a02` with [Core .NET 8 golden run #44689417895](https://github.com/rixed318/Probably-TAU/actions/runs/44689417895): `completed/success` (run 257). This confirms standalone .NET CI only, not Unity.
- Bounded task: exact-SHA CI verification and handoff; no C# or public contract changes. Typed NPC due-event atomic EndDay remains blocked on coordinated interface approval by 02/00 and QA review.
- Toolchain checked in current runtime: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `javac` available. No local C# tests, Windows/Unity build, or Android APK. Android remains BLOCKED until actual Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK verified.
- No main push, merge, or Issue closure. CI for this handoff-only commit remains unverified.

## Core 01 — 20.11.2026: exact-HEAD CI verification (14:01 Samara)

- Issue #19 / PR #9: verified prior `feature/core-2` HEAD `7fe76ab6e226904cc16c4e131e3a4c8ea7c4a0f9` via [Core .NET 8 golden workflow #44701246412](https://github.com/rixed318/Probably-TAU/actions/runs/44701246412), `completed/success`. This is standalone .NET CI, not Unity Editor or Android evidence.
- Single bounded task: exact-SHA CI verification and handoff. No new C# code or public contracts; typed NPC due-event EndDay integration remains pending 02/00 contract coordination and independent QA review.
- Current runtime checked: `dotnet`, Unity Editor, Android `adb`/`sdkmanager`/`ndk-build` absent; `javac` available. Local C# tests, Unity/Windows build and Android APK NOT RUN; APK BLOCKED until real Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK verified.
- No main push, PR merge or Issue closure. CI for this new documentation-only commit not yet verified.

## Core 01 — Day02 future-day precondition regression (20.11.2026, 15:43 Samara)

- Issue #19 / PR #9 / `feature/core-2`: one bounded C# golden-test increment in `Tests/Core/GameStateSnapshotGoldenTests.cs`, commit `587c08d62ca313d479931e011974b6c48db752f1`. The Day02 snapshot (DayIndex=1, 750 kopecks, one due 200-kopeck payable) must reject an EndDay command whose expected day is **future DayIndex=2** despite a correct revision, before settlement. Assert original day, cash, payable, Ledger (3 entries), receipts (4) unchanged. No public API, economy policy, NPC/UI/Content changes.
- Previous branch HEAD `db3c27115db3d2ea9685e12d081fa6ebc5c7dc92` has [standalone Core .NET 8 workflow #44715697289](https://github.com/rixed318/Probably-TAU/actions/runs/44715697289) `completed/success`; **this does not verify the new test**. Exact-new-SHA CI and QA/00 review required before PASS.
- Runtime probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `javac` available. Local C# tests, Unity/Windows build, Android APK NOT RUN; APK BLOCKED pending verified Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK. No Unity project fabricated.
- P0 remains coordinated typed NPC due-event atomic EndDay integration. No main push/merge or Issue closure.

## Core 01 — Day02 past-day EndDay guard (20.11.2026, 17:24 Samara)

- Issue #19 / PR #9: one bounded C# golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, commit `2ca4f090a9b5719363d51ddf570861ae25124ecc`. A Day02 snapshot (DayIndex 1) rejects `PrepareDay` with **current revision but past expectedDayIndex 0**. Assertions require no cash (750 kopecks), payable (1), Ledger (3 entries) or receipts (4) mutation. Complements stale-revision and future-day rejection tests. No production contract or foreign subsystem changes.
- Previous exact HEAD `9f958960b5315562dbfb685a648738977411d16b` has [Core .NET 8 golden run #44729933319](https://github.com/rixed318/Probably-TAU/actions/runs/44729933319) `completed/success`. **New regression commit CI not verified yet**; QA/00 must review exact SHA.
- Runtime checked: no `dotnet`, Unity Editor, `adb`, `sdkmanager` or `ndk-build`; `java`/`javac` available. No local .NET test, Unity import/Windows build or Android APK. Native APK **BLOCKED** until verified Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK. No Unity project or APK fabricated.
- Next P0: typed NPC due-event EndDay atomic integration after 02/00 contract approval and QA review. No main push/merge, PR merge or Issue closure.

## Core 01 — Day02 negative EndDay token regression (20.11.2026, 19:00 Samara)

- Issue #19 / PR #9: one bounded C# golden regression added to `Tests/Core/GameStateSnapshotGoldenTests.cs`, code commit `59a56ae534825a90f97b1c6ee9fb78eeae86d92f` on `feature/core-2`. A Day02 snapshot (`DayIndex=1`) with current revision must reject `expectedDayIndex=-1` without changing day, 750-kopeck cash, one outstanding payable, 3 ledger entries, or 4 receipts. No production API, NPC/UI/Content or economic policy changed.
- Previous exact HEAD `60a5285e2c097877a18728d3bd25bd94d6775381` has GitHub Actions [Core .NET 8 run #44744111477](https://github.com/rixed318/Probably-TAU/actions/runs/44744111477) `completed/success`; **new regression SHA CI is not yet verified**. QA/00 must independently review the new test and exact-head CI.
- Current environment probe: `dotnet`, Unity Editor, Android `adb`/`sdkmanager`/`ndk-build` absent; `javac` available. Local C# tests, Unity/Windows builds and Android APK NOT RUN. Android APK BLOCKED until verified Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- Next P0: typed NPC due-event atomic EndDay integration after coordinated 02/00 contract. No main push, merge or Issue closure.

## Core 01 — 20.11.2026 20:44 Samara: exact-head CI and tooling audit

- Scope: verified the previous exact `feature/core-2` HEAD `203a671e33e9ca963315198caa1958176b7dbcce` via [Core .NET 8 golden run #44759220354](https://github.com/rixed318/Probably-TAU/actions/runs/44759220354): `completed/success`. This validates standalone C# tests including the negative expected-day regression, **not** Unity/Android.
- Attempted one bounded new exact-funds Day02 payable regression (750 kopecks payable, zero ending balance, no duplicate ledger/receipt), but the GitHub update request failed with `RemoteProtocolError: Server disconnected without sending a response`. Subsequent fetch confirms the test file blob SHA remains `117771f22da36f50b1811f02427f1f3a3145e088`; **no new C# code committed**. Do not claim the regression is implemented or tested.
- Current local probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` available. No local .NET test, Unity import/Windows build or Android APK. APK BLOCKED pending real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- Next action: retry the exact-funds regression only after confirming remote write capability; independently verify exact-new-SHA CI, then request QA/00 review. NPC due-event atomic EndDay remains dependent on approved 02/00 contract. No main push/merge or Issue closure.

## Core 01 — 20.11.2026 22:22 Samara: Day02 exact-funds settlement regression

- Issue #19 / PR #9, branch `feature/core-2`: one bounded C# regression added to `Tests/Core/GameStateSnapshotGoldenTests.cs`, commit `1d9eed9a96a413649a0a3824cbb9802be4a66dcc`. Day02 starting cash=750 kopecks, due payable=750: `PrepareDay` must leave cash=0, remove payable, append exactly one ledger posting `payable:day02-exact-funds` and one EndDay receipt. Restore preserves result, stale replay rejects, immutable input stays unchanged. No production contracts, NPC/UI/Content changes.
- Prior exact HEAD `a7fff173f2ac4728cd81931dc7bc0bb19e389555` has [standalone Core .NET 8 CI #44776438368](https://github.com/rixed318/Probably-TAU/actions/runs/44776438368) `completed/success`; **new test commit CI NOT YET VERIFIED**. Independent QA/00 review required.
- Toolchain probe in this runtime: `dotnet`, `csc`, `mcs`, Unity Editor, Android `adb`/`sdkmanager`/`ndk-build` unavailable; `java`/`javac` present. Local C# tests, Unity/Windows build, Android APK NOT RUN. Native Android build BLOCKED pending real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- Next P0: agreed typed NPC due-event EndDay atomic transaction with 02/00; retain one deterministic Core. No main push/merge or Issue closure.

## Core 01 — 20.11.2026 23:58 Samara: Day02 one-kopeck shortfall boundary

- Issue #19 / PR #9: added one bounded regression to `Tests/Core/GameStateSnapshotGoldenTests.cs`, code commit `3176d2c7648d36bdf1e5f9b5add4431113bfd290` on `feature/core-2`. Day02 cash=750 kopecks, payable=751: no debit, payable remains outstanding, ledger stays at 3, receipts advance once to 5, original input remains unchanged. Complements existing exact-funds 750/750 test; no production API or foreign subsystem changed.
- Prior exact HEAD `f8c0b2fc889676a5e6e960a3c5c1e1e91fcd27a8` verified via [Core .NET 8 golden CI #44792288337](https://github.com/rixed318/Probably-TAU/actions/runs/44792288337): `completed/success`. New regression commit CI **NOT YET VERIFIED**; independent QA/00 review required.
- Environment probe: no `dotnet`, Unity Editor, `adb`; `java`/`javac` present. No local C# tests, Unity/Windows build, or Android APK. APK BLOCKED pending real Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- P0 remains typed NPC due-event atomic EndDay integration after approved 02/00 contract. No main push/merge or Issue closure.


## AutoDev 01 — Day02 due-event fail-closed regression (21.11.2026)

**Issue:** #19; branch `feature/core-2`; PR #9. One bounded Core golden-test increment, no public contract or production-code changes.

- `Tests/Core/GameStateSnapshotGoldenTests.cs`: after a successful Day01 with a Day02 payable and a Day02 NPC event, calling `DurableDayAdvance.PrepareDay` on Day02 must throw `InvalidOperationException` (event handler not yet integrated). Assert the source snapshot retains DayIndex=1, 750 kopecks cash, one outstanding payable, one pending event, zero completed events, three ledger entries and four receipts. This prevents partial payable settlement while a due NPC event remains unhandled.
- C# commit: `10ceab60d3aa96f1de7636cacde3fdaf82a2046d`.
- Environment probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` present. **Local C# tests NOT RUN; Unity/Windows build and Android APK BLOCKED.** The new SHA requires GitHub Actions verification; do not claim a passing CI until observed.
- P0: integrate typed due-event handler with atomic EndDay only after 02/00 contract approval; QA/00 independent review required. No merge/main changes or Issue closure.

## Core 01 — 21.11.2026: Day02 event-only fail-closed regression

- Issue #19 / PR #9, branch `feature/core-2`. One bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, code commit `9ca68506ff26ff55116e37ab323c7515c4c4a9e3`. Reuse restored Day02 pending NPC event but supply **zero payables**; `DurableDayAdvance.PrepareDay` must reject due unhandled event and preserve DayIndex=1, cash=750 kopecks, one pending event, zero completed events, ledger=3 and receipts=4. This independently covers event-only atomic fail-closed (previous test combined event and payable). No production API or other subsystem changed.
- Previous exact HEAD `3294206c84a21db1a05d68c2804abf9201d50aa1` has [Core .NET 8 golden CI #44822800543](https://github.com/rixed318/Probably-TAU/actions/runs/44822800543) completed/success. New code SHA CI **not yet verified**; independent QA/00 review required.
- Local probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` not found; `java` and `javac` present. No local C# tests, Unity/Windows build, or Android APK; Android BLOCKED until genuine Unity 6.3 LTS + Android Build Support/SDK/NDK/OpenJDK.
- P0 typed due-event handler still requires 02/00 approved contract; no main push/merge or Issue closure.


## Core 01 — 21.11.2026: event-only Day02 Restore guard (#19)

- One bounded C# golden regression committed to `Tests/Core/GameStateSnapshotGoldenTests.cs` on `feature/core-2`: `5bc3a830ee474a37b475ab5f9f01ec5e4f977194`. After restoring a Day02 snapshot with one due NPC event and zero payables, `DurableDayAdvance.PrepareDay` must still reject EndDay. Assert no mutation of day index (1), cash (750 kopecks), pending/completed events (1/0), ledger (3), receipts (4), or payables (0). No production/public contract or foreign subsystem changes.
- Environment checked: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. Local .NET/Unity tests NOT RUN; Windows/Android builds BLOCKED. Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK required for real APK. CI for this new SHA not yet confirmed; independent QA/00 review required.
- P0: typed NPC due-event atomic EndDay remains pending approved 02/00 handler contract. No main push, merge, or Issue closure.

## Core 01 — 21.11.2026: exact-head CI verification

- Verified `feature/core-2` commit `edce8253d9b08c15fa38720240cf716ac303b320` via GitHub Actions **Core .NET 8 golden tests** run [#44847648970](https://github.com/rixed318/Probably-TAU/actions/runs/44847648970), `completed/success` (run number 295). This verifies the previously added restored event-only Day02 fail-closed regression at that SHA, **not** Unity compilation.
- Scope this pass: CI verification and handoff only. No production C# or public contract modifications; typed NPC due-event EndDay implementation still requires approved 02/00 contract and independent QA review.
- Local toolchain probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` available. Local C# tests NOT RUN; Unity/Windows build and Android APK BLOCKED pending genuine Unity 6.3 LTS + Android Build Support/SDK/NDK/OpenJDK.
- No push to main, no merge, no Issue closure. CI for this documentation-only commit must be checked separately.

## Core 01 — 21.11.2026: Day03 payable deferral at Day02 EndDay (#19)

- One bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, commit `ab3ad2fb8eeea93b8a9badb97b302261b633b714`, branch `feature/core-2`, PR #9. A Day03 payable (DueDayIndex=2, 200 kopecks) must remain outstanding after Day02 EndDay (expectedDayIndex=1); no ledger posting, cash remains 750, exactly one EndDay receipt is appended, input snapshot unchanged. No production/public API or foreign subsystem changes.
- Prior HEAD `6bad6c4119a8dca32896fe2c56d5073fbe92a1a8` GitHub Actions Core .NET 8 golden run #44857076887 completed/success. **New test SHA CI not verified**; requires exact-SHA check and QA/00 review.
- Runtime probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. No local .NET tests or Unity/Windows/Android builds. Android APK BLOCKED pending genuine Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- P0 typed NPC due-event atomic EndDay still awaits approved 02/00 handler contract. No main push, merge, or Issue closure.

## Core 01 — 21.11.2026: Day03 deferred payable settlement and replay regression (#19)

- Bounded C# golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, code SHA `09a7afa766bb6eac199e4d201a4546937349ad0b` on `feature/core-2`, PR #9. Extends existing Day02 future-payable deferral fixture: when Day03 closes (expectedDayIndex=2), 200 kopecks are settled exactly once (750→550), payable removed, one ledger posting `payable:day03-future`, one EndDay receipt, source snapshot unchanged. Restore + stale Day03 replay rejected without mutation. No public contract or foreign subsystem changed.
- New exact-SHA CI not yet verified; independent QA/00 review required. Local environment: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java` and `javac` present. No local .NET tests, Unity/Windows builds or APK; Android APK BLOCKED pending real Unity 6.3 LTS Editor and Android toolchain.
- P0 typed NPC due-event atomic EndDay still pending approved 02/00 handler contract. No main push/merge or Issue closure.

## Core 01 — 21.11.2026: Day03 stale-revision guard (#19)

- One bounded golden regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, commit `94e20ba405085020eff30ab5fcb08ff276a966e7`, PR #9. With a deferred Day03 payable of 200 kopecks and cash 750, `DurableDayAdvance.PrepareDay` must reject a stale revision despite the correct Day03 day token. Assert source DayIndex=2, cash=750, one outstanding payable, ledger=3 and receipts=5; no partial settlement. No production API or other subsystem changes.
- Previous HEAD `aee8fe26e5ef6ca640f92e758b3b16b37d921ae6` standalone .NET 8 CI run [#44873632934](https://github.com/rixed318/Probably-TAU/actions/runs/44873632934) completed/success. New test SHA CI unverified at writing; QA/00 to review exact new HEAD.
- Local probe: no `dotnet`, Unity Editor, `adb`, `sdkmanager` or `ndk-build`; `java`/`javac` available. Local .NET tests NOT RUN; Windows/Unity builds and Android APK BLOCKED pending real Unity 6.3 LTS and Android toolchain.
- Typed NPC event EndDay integration still depends on approved 02/00 contract. No main push/merge or Issue closure.


## Core 01 — 21.11.2026: weekly EndDay payable fail-closed regression

- **Issue:** #19 (Core EndDay integration); related #2/#3. **Branch:** `feature/core-2`; **PR:** #9.
- **Code commit:** `72c9b45ee4bb016fc3247b7adcd603782180c8e1`.
- **Scope:** one golden regression `WeeklyPayableFailClosedGolden` in `Tests/Core/GameStateSnapshotGoldenTests.cs`, invoked by `RunAll`.
- **Invariant:** on DayIndex=4, an affordable due payable must not be charged while the weekly settlement handler is missing. `PrepareDay` rejects; source day/revision, cash, payable, ledger, receipts and processed-day keys remain unchanged. No public API/contract change, no NPC/UI/Save edits.
- **Verification:** prior branch SHA `d4918bddba341ad58ef7951ea0d3519f025fa051` GitHub Actions Core .NET 8 golden runner completed/success (run #44881650967); this is **not** evidence for the new test. New SHA CI must be checked separately. Local `dotnet` and Unity Editor unavailable; `adb`, `sdkmanager`, `ndk-build` missing; Java/javac present. Android APK and Windows/Unity builds **BLOCKED** (Unity 6.3 LTS Editor + Android Build Support/SDK/NDK required).
- **QA/00:** review test against C0-v2 / ADR-C0-001; keep weekly settlement blocked until an approved handler is integrated. Do not merge PR or close Issues without acceptance criteria.


## Core 01 — 21.11.2026: weekly quiet EndDay fail-closed regression (#19)

- Branch `feature/core-2`, PR #9; bounded C# golden regression `WeeklyQuietFailClosedGolden` in `Tests/Core/GameStateSnapshotGoldenTests.cs`, code commit `10c3cb1e73ca625a2a589adccddf4f4797452f4c`.
- On DayIndex=4 with **no payables and no due events**, `DurableDayAdvance.PrepareDay` must reject closing the week while weekly settlement handler is not integrated. Assert unchanged day/revision, cash 1000 kopecks, zero payables, zero ledger postings, zero receipts, zero processed-day keys. Complements the existing affordable-payable weekly rejection test; no production code, public contract or foreign subsystem changed.
- Previous HEAD `a0b2aea7e0be232661c2cc97b604f89c6719bf11`: GitHub Actions Core .NET 8 golden runner [#44889269473](https://github.com/rixed318/Probably-TAU/actions/runs/44889269473) completed/success. **New code commit CI not yet verified**; QA/00 must independently review the new exact SHA.
- Local probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java`/`javac` present. Local C# tests NOT RUN. Unity/Windows build and real Android APK BLOCKED until Unity 6.3 LTS Editor and Android Build Support/SDK/NDK are available.
- P0 still requires an approved 02/00 typed NPC due-event integration contract and actual Unity toolchain. No main push/merge, no Issue closure.


## Core 01 — 21.11.2026: second weekly boundary fail-closed regression (#19)

- **Scope:** one bounded standalone C# regression `SecondWeekQuietFailClosedGolden` in `Tests/Core/GameStateSnapshotGoldenTests.cs`, wired into `RunAll`; code commit `c9bb26a189c6b65047488bd648a7aa04bcc07f47` on `feature/core-2`, PR #9. On DayIndex=9 (second weekly settlement boundary), even without due events or payables, `DurableDayAdvance.PrepareDay` must reject while weekly settlement is unimplemented. The source retains day/revision, cash=1000 kopecks, no events/payables, no ledger/receipts/processed-day keys. No production API or foreign subsystem changed.
- **Verified prior HEAD:** `033a0adfb6baa8d9aad019ec7cff1c85b9fd11dd` standalone Core .NET 8 golden CI [#44896709168](https://github.com/rixed318/Probably-TAU/actions/runs/44896709168) `completed/success`. This is **not** proof of new test success; verify exact new SHA separately.
- **Environment:** `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java` and `javac` available. Local C# tests NOT RUN. Unity/Windows build and Android APK BLOCKED pending genuine Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK.
- **Next:** verify CI of new HEAD and request independent QA/00 review. Typed NPC due-event EndDay integration still needs approved 02/00 contract. No main push/merge or Issue closure.


## Core 01 — 21.11.2026: Day10 direct-payable weekly gate regression (#19)

- One bounded C# regression `SecondWeekPayableDirectEntryFailClosedGolden` added to `Tests/Core/GameStateSnapshotGoldenTests.cs` and `RunAll`, code SHA `dde5c44b1a25ecc8791191a9ee71ea3416866d07`, branch `feature/core-2`, PR #9.
- On DayIndex=9 with a due affordable 200-kopeck payable and cash 1000, both `PreparePayablesDay` (direct public entry) and `PrepareDay` must fail closed until the weekly settlement handler is implemented. Assert original cash, payable, day, revision, ledger, receipts, command/day keys unchanged. No production API, other subsystem or main changes.
- Prior exact HEAD `74deb6b07ff20cbd0279b6249402e4ed2736b6e8` standalone Core .NET 8 CI [#44905322562](https://github.com/rixed318/Probably-TAU/actions/runs/44905322562) completed/success. This does **not** verify the new test; check CI for new HEAD separately. QA/00 independent review required.
- Runtime probe: no `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build`; `java`/`javac` available. Local C# tests NOT RUN; Unity/Windows build and real Android APK BLOCKED pending genuine Unity 6.3 LTS + Android Build Support/SDK/NDK/OpenJDK. Typed NPC event handler still requires approved 02/00 contract. No merge or Issue closure.


## AutoDev 01 — Day10 quiet direct-entry weekly guard (21.11.2026)

**Scope:** Issue #19, one regression only; `feature/core-2`, PR #9. Code commit `377027b5b71694c59d760d84f79327594a5a989f`.
**File:** `Tests/Core/GameStateSnapshotGoldenTests.cs`, new `SecondWeekQuietDirectEntryFailClosedGolden()` registered in `RunAll()`.
**Invariant:** on DayIndex=9 (Day10), with zero pending events, zero payables and cash=1000, both `PreparePayablesDay` and canonical `PrepareDay` must fail closed until the weekly settlement stage is integrated. No mutation to day/revision, cash, ledger, receipts, processed command/day keys.
**Contracts:** unchanged. No production code, UI/NPC/QA files or `main` modified.
**Verification:** previous HEAD `d57e726f866c1d8dcedc7ae3b625654b12ebbf21` has completed/success PR workflow runs 44913341532 and 44913341152; **these do not validate the new test**. New code SHA workflow result not confirmed at handoff time. Local `dotnet`, Unity Editor, adb, sdkmanager, ndk-build absent; javac present. Local .NET, Unity Editor/Windows and Android APK tests/builds **NOT RUN / BLOCKED**.
**Next:** QA/00 review exact SHA CI; implement typed due-event/weekly handlers only under agreed 02/00 contracts, then independently verify Unity 6.3 Editor and Android toolchain before APK claim.


## Core 01 — 21.11.2026: exact-head CI validation of Day10 weekly direct-entry guard

- **Scope:** verify the existing `SecondWeekQuietDirectEntryFailClosedGolden` at exact branch HEAD `c3f12b5f86fb8ccc0b36dac122c2dab81be61ec9` (Issue #19, PR #9); no additional regression or production code introduced.
- **Result:** GitHub Actions `Core .NET 8 golden tests` workflow run [#44922245824](https://github.com/rixed318/Probably-TAU/actions/runs/44922245824), run number 329, `completed/success`, associated with exact SHA `c3f12b5f86fb8ccc0b36dac122c2dab81be61ec9`. This verifies the standalone .NET golden runner, **not** Unity compilation or Android APK.
- **Local toolchain checked again:** `dotnet`, `unity`/`Unity`, `adb`, `sdkmanager`, `ndk-build` unavailable; `/usr/bin/java` and `/usr/bin/javac` available. No local .NET test or Unity/Windows/Android build performed. **Android build BLOCKED** until actual Unity 6.3 LTS Editor + Android Build Support/SDK/NDK/OpenJDK are available.
- **Integration:** typed NPC due-event and weekly settlement EndDay handlers remain blocked on approved 02/00 contract; no unsafe bypass of C0/ADR fail-closed rules. QA/00 review still required. This pass only records exact-head verification; no public contracts or other subsystems changed. No main push, merge or Issue closure.


## Core 01 — 21.11.2026: verify current documentation HEAD CI (#19)

- **Bounded task:** verify exact `feature/core-2` HEAD `b61a374452bcca7dde967fa10e39365347ae393a` after prior handoff update; no new C# or public API changes.
- **Actual result:** GitHub Actions `Core .NET 8 golden tests`, run [#44931010824](https://github.com/rixed318/Probably-TAU/actions/runs/44931010824), status `completed`, conclusion `success`, returned for that exact SHA. Standalone CI only; not Unity compile, Android APK or Windows build.
- **Local toolchain probe:** no `dotnet`, `unity`/`Unity`, `adb`, `sdkmanager`, `ndk-build`; `/usr/bin/java` and `/usr/bin/javac` present. Local .NET tests, Unity/Windows and Android APK **NOT RUN / BLOCKED**.
- **Integration still BLOCKED:** typed NPC due-event and weekly EndDay handlers need approved 02/00 integration contract and independent QA review; do not bypass fail-closed C0/ADR requirements. No `main` push, merge or Issue closure.


## Core 01 — 22.11.2026: Day01 stale precondition regression (#19)

- **Scope:** one bounded C# regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`, code commit `6ee7485a07069ace57547276e0dc03d84484cdc1`, branch `feature/core-2`, PR #9. In existing `Day01FlowGolden`, reject stale expected revision and wrong expected day before any of the three document effects are staged; assert original snapshot revision/day/cash/ledger/receipts unchanged. No production API or foreign subsystem changes.
- **Verified previous HEAD:** `be2acbe7e3696e15dc0eba1fe8a0c8c5d1e95eec`, Core .NET 8 golden runner GitHub Actions run [#44940399711](https://github.com/rixed318/Probably-TAU/actions/runs/44940399711) completed/success. This **does not validate new code**; CI for new SHA must be checked separately. Local `dotnet` absent; C# tests NOT RUN locally.
- **Toolchain:** Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. Unity/Windows and Android APK **BLOCKED** pending genuine Unity 6.3 LTS Editor and Android Build Support/SDK/NDK. No fabricated builds.
- **Dependencies:** typed NPC due-event and weekly settlement EndDay still require approved 02/00 contracts and QA review. No `main` push, merge or Issue closure.


## Core 01 — 2026-11-22: Day01 negative precondition regression

- Issue #19; branch `feature/core-2`; code commit `3b0f905957878e8ab513340eb66e237331be23cb`; PR #9.
- One bounded C# test-only change in `Tests/Core/GameStateSnapshotGoldenTests.cs`: `Day01FlowGolden` now rejects negative expected revision (`-1`) and negative expected day (`-1`) before any of three document decisions; existing source-immutability assertion covers cash, day/revision, Ledger and receipts. No public API, production behavior, or foreign subsystem changes.
- Prior exact HEAD `31a6f6f6b82e199cf91409ee217200f28b8df4c3`: Core .NET 8 PR workflow run `44948940279` completed/success. This does NOT verify the new code commit. New SHA CI not yet confirmed.
- Local environment: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java` and `javac` available. Local .NET tests, Unity/Windows build and Android APK NOT RUN/BLOCKED. No fake Unity project/APK.
- QA/00: review new exact SHA and its CI. NPC due-event / weekly settlement EndDay integration remains dependent on approved 02/00 contract. No main push/merge or Issue closure.

## Core 01 — exact-head CI audit, 2026-11-22

- Scope: verify the latest Day01 negative precondition regression at exact branch SHA `d62a22bdcdb8df1163f021071384e331f1d3a35d` (Issue #19); no new C# or public contract changes.
- GitHub Actions PR run [44957751033](https://github.com/rixed318/Probably-TAU/actions/runs/44957751033) returned `completed/success` for that SHA (standalone Core tests only). This is not a Unity compile or Android build.
- Local environment check: `dotnet`, `Unity`, `unity-editor`, `adb`, `sdkmanager`, `ndk-build` not on PATH; `java` and `javac` present. Local .NET/Unity tests and real Android APK are BLOCKED by unavailable toolchain.
- Day01 typed NPC due-event and weekly settlement production integration remain gated on approved cross-subsystem handler contracts and QA/00 review. Avoid inventing handlers or mutating foreign subsystems.
- No main push, merge or Issue closure. This handoff-only commit requires its own exact-SHA CI check; do not attribute prior success to the new HEAD.

## 2026-11-22 — Day01 deterministic replay regression (#19)

- Branch: `feature/core-2`; code commit `ba19956f31ee8cde0677e2f5c682f85f98177d98`; PR #9.
- One bounded change in `Tests/Core/GameStateSnapshotGoldenTests.cs`: `Day01FlowGolden` now runs the same three document choices twice from the same immutable starting snapshot and compares durable day/revision/balance, ordered ledger posting IDs and amounts, and command receipt IDs/fingerprints. This is an exact-input deterministic replay assertion, not NPC/weekly integration.
- No production API or external subsystem modifications; no Unity project or APK generated.
- Validation: local `dotnet` and Unity Editor unavailable; this code commit's CI result must be checked at its exact SHA before calling tests successful. Prior HEAD `f474b54cd807d7f3c0837ffedcddad26052971b0` had Core .NET 8 golden tests success (GitHub Actions run 44966363794).
- BLOCKED: real Unity 6.3 Editor + Android Build Support/SDK/NDK/OpenJDK toolchain not available in current environment (Java alone is insufficient); Unity/Windows/Android builds and local C# execution not performed. Due-event and weekly EndDay handler integration awaits approved 02/00 contracts and independent QA.
- Next: verify exact code SHA CI and ask QA/00 to review; never merge to main without owner approval.


## 2026-11-22 — Day01 receipt posting-key deterministic replay regression (#19)

- Scope: one C# test-only increment in `Tests/Core/GameStateSnapshotGoldenTests.cs` (`Day01FlowGolden`), commit `3dec925ccfa6a67e20007d3ebc9ce5964893b786` on `feature/core-2`.
- Replaying the same three Day01 choices against the same immutable input now compares each receipt's `PostingKeys` count and ordered key values, in addition to existing command ID/fingerprint and ledger assertions. No production contract, NPC/UI/Save changes.
- Local environment checked: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` missing; `java` and `javac` present. Local .NET tests and Unity/Windows/Android builds NOT RUN/BLOCKED. CI of new commit must be checked separately; no claim of success without exact SHA run.
- Remaining: typed due-NPC event and weekly EndDay settlement require approved 02/00 contract; real Unity 6.3 Android build requires Editor, Android Build Support, SDK/NDK/OpenJDK and QA validation. No main push, merge or issue closure.


## 2026-11-22 — exact-head Day01 receipt replay CI verification (#19)

- Scope: verify the previous Day01 `PostingKeys` deterministic replay regression at exact `feature/core-2` HEAD `db4def7a24fe32a3651c88c1dc2aa31c1bae89cf`; no new C# changes or public contract modifications in this pass.
- GitHub Actions `Core .NET 8 golden tests` run [44981634736](https://github.com/rixed318/Probably-TAU/actions/runs/44981634736) returned `completed/success` for that exact commit SHA. This confirms standalone Core .NET 8 CI only, not Unity compilation, Windows build, or Android APK. Any new handoff commit needs its own CI result.
- Environment probe: `dotnet`, `unity`/`Unity`, `adb`, `sdkmanager`, `ndk-build` unavailable on PATH; `java` and `javac` present. Local .NET tests and actual Unity/Android/Windows builds NOT RUN/BLOCKED. Android needs real Unity 6.3 LTS Editor, Android Build Support, SDK/NDK/OpenJDK.
- Integration remains gated: typed NPC due-event and weekly settlement handlers require agreed 02/00 contract and QA/00 review. Preserve fail-closed C0-v2 / ADR-C0-001 and shared deterministic Core. No main push, PR merge or Issue closure.

## 2026-11-22 — Day01 reserved EndDay command-ID guard (#19)

- Scope: one bounded production Core integration safety fix in `Runtime/Core/DurableDay01Flow.cs`: reject any document choice whose CommandId equals the canonical `end-day:<CampaignId>:0` ID **before** staging three choices. Prevents a document from reserving the durable EndDay receipt ID; no new public API, cross-system contract, or changed successful-path semantics.
- Code SHA `8a02a41df656296d924a0f3dd5b01d924c9cd250`; golden regression SHA `ad4e28f23ec17e707f3c63777951697f2de7b8e0` in `Tests/Core/GameStateSnapshotGoldenTests.cs` checks rejection and unchanged source revision/day/cash/ledger/receipts. PR #9; branch `feature/core-2` only.
- Previous exact branch HEAD `51dc7a0b36418c864676f58a00bfc725e01ab07f`: GitHub Actions Core .NET 8 run `44988822125` completed/success; **not** evidence that these new commits pass. New SHA CI requires independent confirmation.
- Runtime probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` available. Local C# tests, Unity Windows build, Unity Android APK NOT RUN/BLOCKED. Real Unity 6.3 LTS Editor and Android Build Support/SDK/NDK/OpenJDK are required for Android.
- Remaining: typed NPC due-event and weekly EndDay settlement require approved 02/00 handler contract and QA review; no speculative implementation. No main push, merge, or Issue closure.

## 2026-11-22 — exact-SHA CI confirmation of reserved EndDay ID guard (#19)

- Verified GitHub Actions Core .NET 8 golden tests for production/test SHA `ad4e28f23ec17e707f3c63777951697f2de7b8e0`: run [44996380149](https://github.com/rixed318/Probably-TAU/actions/runs/44996380149), `completed/success`.
- Verified previous handoff HEAD `acf749adbde6c051d44457471eb0245db9ca5f77`: run [44996388180](https://github.com/rixed318/Probably-TAU/actions/runs/44996388180), `completed/success`. Both are standalone Core .NET 8 CI, not Unity Editor/Windows/Android evidence.
- No new C# or public API change in this pass. Local environment has no `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build`; `javac` available. Local tests and Unity/Android builds BLOCKED. Next: 02/00 approved typed due-event/weekly EndDay contract, QA review and real Unity toolchain. Do not merge to main or close issues.


## 2026-11-22 — CI audit of exact Core HEAD (#19)

- Audited `feature/core-2` HEAD `cfb3a065eb8f0ef598aa5ff24b133dd48516be90`: GitHub Actions **Core .NET 8 golden tests** run [45003864299](https://github.com/rixed318/Probably-TAU/actions/runs/45003864299) is `completed/success` (run number 361). This is standalone .NET evidence, **not** Unity Editor, Windows player or Android APK validation.
- No C# code or public contracts changed in this pass. Local probe found `java`/`javac`, but no `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build`. Local .NET/Unity tests and Android APK are BLOCKED. Android Build Support/SDK/NDK/OpenJDK not verified as an installed complete Unity toolchain.
- Day01 typed NPC due-event and weekly EndDay production integration remains BLOCKED pending approved 02/00 contract and independent QA. No main push/merge, no Issue closure. The handoff-only commit created by this update needs its own CI confirmation.


### Core 01 — CI verification (22.11.2026, Issue #19)
- Exact previous branch HEAD `d68a1ca3644689740bb2f56d181656bced6da248`: GitHub Actions Core .NET 8 golden tests [run #45011808560](https://github.com/rixed318/Probably-TAU/actions/runs/45011808560) completed/success (run number 363). This is standalone .NET CI, not a Unity/Windows/Android build.
- This pass changes handoff only. No new C# changes or public contract changes; no independent NPC/weekly EndDay integration without 02/00 approval and QA.
- Local runtime probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `javac` present. Local .NET tests, Windows Unity build and Android APK NOT RUN/BLOCKED. New documentation HEAD CI not yet verified.
- No main push, merge or Issue closure.


## AutoDev 01 — 22.11.2026: malformed Day01 batch preflight (#19)

- One bounded regression in `Tests/Core/GameStateSnapshotGoldenTests.cs`: exactly-three Day01 choices with a null third item, and an incomplete two-choice batch, must reject with `ArgumentException` before any document effect is staged. Assertions verify source revision/day/cash/ledger/receipts remain unchanged.
- Code commit: `b40e2e33e02ac624476eca5787cc81398b6416ea` on `feature/core-2`; PR #9 remains Draft. No public API or production implementation changes.
- Local toolchain probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java` and `javac` present. Therefore local C# execution, Unity Windows build and Android APK are **BLOCKED / NOT RUN**. New SHA CI result not verified at time of handoff.
- Pending: independent QA/00 review and approved NPC/weekly EndDay integration contract. Do not merge to main or close Issues.


## AutoDev 01 — Day01 persisted EndDay ID preflight (22.11.2026)

- Scope #19, one bounded Core change: `DurableDay01Flow.Prepare` rejects a reserved `end-day:<campaign>:0` ID already present in persisted `ProcessedCommandIds`, before staging document choices. No public API change.
- Production commit `57cd543d0d87635f601558d89d20fcb29d27a7b0`; regression commit `5a2e47c9e22233b4a22c9de1cf05ed328dc51b79` in `feature/core-2` only.
- `Day01FlowGolden` now constructs a valid restored-like state with the reserved command ID and asserts fail-closed without modifying balance, ledger, day, revision or receipts.
- Local verification: .NET SDK and Unity Editor unavailable; C# tests and Unity/Windows/Android builds **NOT RUN** locally. Java compiler present, Android SDK/NDK tools unavailable. GitHub Actions for new SHA must be checked separately; no success claimed in advance.
- EndDay due-event and weekly integration still requires the 02/00 contract; no NPC/UI/Save changes. PR #9 remains draft, no main merge.

## 2026-11-23 — persisted document command ID preflight (#19)

- Branch `feature/core-2`, PR #9. One bounded production change: `DurableDay01Flow.Prepare` rejects any of the three document CommandIds already present in `current.Header.ProcessedCommandIds` before staging the first document. Existing EndDay reserved ID preflight remains intact; public API and foreign subsystems unchanged.
- Core commit `c28f3ba4f65376f69245686f9a50e3624fd6be57`; regression commit `fb48b98e8a35b49ff7c35c923150e0d4616b0645`. Golden regression constructs a valid restored-like DayIndex=0 snapshot with previously processed third-document ID and asserts rejection without changing revision/day/cash/Ledger/receipts.
- Local probe: `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` unavailable; `java`/`javac` present. Local .NET tests, Unity Windows build and Android APK **NOT RUN / BLOCKED**. New exact-SHA CI status must be checked independently; do not claim it passed before verification.
- Typed NPC due-event/weekly EndDay settlement still requires approved 02/00 contract and QA review. No main push, merge, or Issue closure.


## AutoDev 01 — RNG rejection regression (23.11.2026)

Issue #2: one bounded test-only increment in `Tests/Core/Pcg32GoldenTests.cs`. Invalid `NextUInt32(0)`, `NextInt(0)`, and `NextInt(-1)` must leave `State` and `Increment` unchanged; the next draw must match an exact restored snapshot. Invalid even-increment `Restore` is rejected without mutating an existing instance. No public API, production Core, or foreign subsystem changes.

Test commit: `55b7185d3ce006aba0d3ed6f8667a98f7034a37c` on `feature/core-2`. Local `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` absent; `java`/`javac` present. C# tests **NOT RUN locally**; Unity Windows/Android APK **BLOCKED**. Exact-SHA GitHub CI needs separate verification. No main push/merge or issue closure. QA/00: please review this isolated RNG regression and its CI.


## Core 01 — 23.11.2026: persisted EndDay ID preflight (#19)

- `DurableDayAdvance.PrepareQuietDay` and `PreparePayablesDay` now reject a canonical `end-day:<campaign>:<day>` ID already present in `ProcessedCommandIds`, even when `ProcessedDayKeys` is missing that key. This prevents staging duplicate EndDay receipts/postings on restored-like snapshots. No public API changes; existing processed-day guard retained.
- Regression in `GameStateSnapshotGoldenTests.Day01FlowGolden` covers both the quiet and payable direct paths plus canonical `PrepareDay` and verifies the input cash, ledger, receipts, revision and outstanding payable remain unchanged.
- Core commit `ab7ee3859a2f8389e45fcb2525ea4528ec2ca440`; test commit `5583b4398db42f9e994b5a61872bcd1e29ec8a4a` on `feature/core-2` only. Exact-SHA CI must be checked separately; no local .NET/Unity/Android builds were run.
- Environment: `java` and `javac` available; `dotnet`, Unity Editor, `adb`, `sdkmanager`, `ndk-build` not found. Android APK and Windows Unity build BLOCKED. Typed NPC due-events and weekly settlement still require 02/00 approved integration contract. No main push/merge/Issue closure.


## Core 01 — 23.11.2026: direct payable EndDay must have due liabilities (#19)

- Scope: `DurableDayAdvance.PreparePayablesDay` now rejects a direct call when `CaptureDueStage(dayIndex)` is empty. This prevents a caller bypassing the canonical quiet-day `end-day-quiet-v1` receipt fingerprint via a payable-path `end-day-payables-v1:` receipt. `PrepareDay` still chooses the appropriate path; an unaffordable *due* payable is still processed through the payable path and remains outstanding. No public signature or NPC/weekly contract changes.
- Production commit: `c18b5fc05f938fb6d16779c42fb441b5bc384d36`; regression commit: `29064084234f74cdca8e0a5906a121b9010a119e`; branch `feature/core-2`, PR #9. Regression in `QuietDayAdvanceGolden` checks rejection and unchanged original revision, day, cash, ledger, receipts, command/day keys.
- Validation: local `dotnet` unavailable, so C# golden tests NOT RUN locally. Exact-SHA GitHub Actions CI for these new commits must be checked separately. Unity Editor/Android Build Support/SDK/NDK unavailable in this runtime; Windows Unity build and Android APK BLOCKED/NOT RUN. No fabricated Unity project or APK.
- Remaining: typed NPC due-event and weekly EndDay settlement need 02/00-approved contract and QA; Unity 6.3 LTS Android/Windows build needs a real Editor toolchain. No `main` push, merge or Issue closure.


## Core 01 — due-event EndDay fail-closed regression (23.11.2026)

- **Issue:** #19; **branch:** `feature/core-2`; **PR:** #9.
- **Scope:** one standalone golden regression, no production/API/contract changes. `Tests/Core/GameStateSnapshotGoldenTests.cs` adds `DueEventEndDayFailClosedGolden` to `RunAll`. A persisted due NPC event with a simultaneously due payable must reject canonical `PrepareDay` and both direct EndDay entrypoints; checks unchanged day/revision/cash, command/day keys, ledger, receipts, pending event and payable.
- **Test commit:** `390f6e6688577ae1bafc7ede4f686aef53ed58e8`. This handoff update follows as a separate commit.
- **Verification:** local .NET tests **NOT RUN** (.NET SDK unavailable); no Unity Editor or Android SDK/NDK, so Windows Unity/Android APK **BLOCKED**. Exact-SHA GitHub Actions result must be checked separately; do not infer success from earlier runs.
- **Dependencies:** typed NPC due-event and weekly settlement still require approved 02/00 integration contracts. No NPC/UI/Save edits, no `main` push, PR merge or issue closure.
