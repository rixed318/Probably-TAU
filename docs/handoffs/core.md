# ЧАТ 01 — handoff: Simulation Core / Economy

**Дата:** 14.11.2026  
**Issues:** #2 (Unity 6 + GameState/Clock/Commands), #3 (Economy + Ledger).  
**Ветка:** feature/core-2 (создана от main d673322cd9d0d366a27a57b81056504dfb26c731).  
**Draft PR:** https://github.com/rixed318/Probably-TAU/pull/9  
**Контрактный commit:** 4ccf57c08faca0518e2c8ceb8f5d0748eaea6dab.  
**Статус:** AUDITED → C0 CONTRACTS PROPOSED → WAITING FOR COORDINATOR 00 REVIEW. **Код/Unity-проект ещё не созданы.**

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
- Конфликт порядка начислений между GDD §Г6 и ARCHITECTURE: предложен вариант ARCHITECTURE, должен подтвердить чат 00.
- Открытая задача O-004: запрет/разрешение отрицательного баланса, обработка долга и частичных выплат.

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

**Следующий шаг:** координатор 00 читает и согласует https://github.com/rixed318/Probably-TAU/pull/9; после утверждения ЧАТ 01 переходит к реализации только в feature/core-2 и обновляет handoff с результатами реальных тестов.
