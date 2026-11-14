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
