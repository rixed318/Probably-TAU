# Gate C0 — предложение контрактов Simulation Core (ЧАТ 01)

**Статус:** PROPOSED / требуется review чата 00 и владельца; **не является ADR и не реализован в коде**.  
**Основание:** Issues #2, #3; README, AGENTS, GDD v0.1, ARCHITECTURE, MECHANICS, DECISIONS, CHAT_WORKFLOW.  
**Ветка:** feature/core-2. **Область:** Core/Application/Economy; без UI/NPC/контента/файлового persistence.  
**Версия предложения:** C0-v1, 14.11.2026.

## 0. Обязательный порядок согласования

1. Чат 00 рассматривает разделы «Решения на утверждение» и уточняет конфликты с GDD.
2. После фиксации решения в docs/DECISIONS.md (владелец документа — чат 00) ЧАТ 01 реализует Core API и EditMode-тесты.
3. Чаты 02/03/04/05 используют только **принятый** контракт; до этого — mocks/спецификации.
4. Изменение публичного API после Gate C0 — отдельное согласованное ADR/PR, без тихого изменения типов.

## 1. Модули и границы зависимостей

    TAU.President.Core            // GameState, IDs, clock, metrics, domain events, RNG; без UnityEngine
    TAU.President.Application     // dispatch, handlers, command/result, end-day orchestration
    TAU.President.Economy         // Money, Ledger, obligations, financial policy; без UnityEngine
    TAU.President.Tests.EditMode  // Unity Test Framework; проверка pure C# модулей

Предлагаемая физическая структура: unity/Assets/_Project/Runtime/Core, /Application, /Economy и unity/Assets/_Project/Tests/EditMode. Для каждого модуля один asmdef; зависимости Core → ничего игрового, Economy → Core, Application → Core+Economy; Presentation/People/Content/Infrastructure зависят только от публичных контрактов. Замкнутые ссылки между asmdef запрещены.

**Инициализация Unity:** настоящий проект Unity 6.3 LTS (2D) через Unity Editor/Hub, с проверенными ProjectSettings/ProjectVersion.txt, Packages/manifest.json, packages-lock.json и .meta. Не подделывать файлы, которые обычно создаёт сам редактор. При отсутствии Unity в среде — подготовить независимые pure C# исходники/тесты и честно зафиксировать «Unity compilation: not verified»; исходный игровой проект считать незавершённым до editor validation.

## 2. GameState v1

**Единственный owner mutable session state:** GameState. Он не является MonoBehaviour или ScriptableObject. Состояние изменяется только применением валидированной транзакции через CommandProcessor. Чужие подсистемы не получают setter доступа к корню.

    GameState
      SchemaVersion: int = 1
      Revision: long                                  // увеличивается при успешной команде
      CampaignId: string                              // стабильный UUID-формат
      CampaignSeed: ulong
      DifficultyId: string                            // ключ неизменяемого definition
      Clock: GameClockState { DayIndex: int, Phase: Active|Finished }
      Economy: EconomyState { OpeningBalance, Balance, Ledger[], Obligations[], Payables[] }
      Metrics: AcademyMetrics { EducationQuality, StudentSatisfaction,
                               StaffMorale, EmployerTrust, PublicReputation }
      Reputation: ReputationState                     // по группам, собственная шкала
      Random: RandomState                             // именованные RNG-потоки
      Commands: AppliedCommandReceipt[]               // ID + canonical fingerprint + outcome
      DayReports: DayReport[]                          // либо ограниченный журнал с последним отчётом
      Flags: deterministic sorted key-value scalars
      Events: EventState                              // резерв schema для чата 02
      Documents: DocumentState                        // резерв schema для чата 02/04
      Visitors: VisitorState                          // резерв schema для чата 02
      Projects: ProjectState                          // резерв schema для будущего проектного сервиса
      Inventory: InventoryState                        // резерв schema; не реализовывать в Issue #2

Типы резервных секций, их поля и version будут отдельно согласованы владельцами подсистем. Пустые секции допускаются в первом vertical slice, но **нельзя подменять их UnityEngine.Object, Dictionary<object,object> или произвольными сериализуемыми C#-объектами**. В C0 фиксируются имена ключей и ответственность, а не контентная схема.

**Метрики:** пять немонетарных целочисленных значений от 0 до 100, изменения ограничены clamp(0,100); Budget — шестой отображаемый показатель, но не метрика 0..100. Репутационные группы не должны смешиваться с агрегированными метриками; отображаемые проекции строит UI.

**IDs:** стабильные строковые ID определений (например event.lab_upgrade), стабильные UUID-формат ID экземпляров. Детерминированный Core ID-allocator получает campaignSeed + namespace + монотонный счётчик или другой устойчивый вход; запрещены Guid.NewGuid()/DateTime.Now() внутри симуляции. CommandId задаёт вызывающая сторона и передаётся как вход симуляции; одинаковое воспроизведение предполагает одинаковые команды, включая их ID.

**Schema:** отдельные SchemaVersion GameState и версии секций. SaveService (ЧАТ 05) определяет внешний envelope/buildVersion/checksum/backup/migration, но не переименовывает полей состояния без Gate C0. В snapshot входят весь ledger, долги, ключи применённых команд и обязательств, RNG, очереди, дедлайны и завершённые/ожидающие события. DTO не содержит прямых ссылок на Unity-объекты.

## 3. Commands / CommandProcessor v1

Контракт в терминах C# (написание типов и namespace ещё предлагается, не утверждено):

    public interface IGameCommand {
        string CommandId { get; }          // уникальный ключ пользовательского намерения
        string Kind { get; }               // стабильный type ID, например "core.advance-day.v1"
        int Version { get; }                // wire-schema команды
        int ExpectedDayIndex { get; }       // защита от повторного завершения дня
        long ExpectedRevision { get; }      // защита от устаревших UI-моделей
    }
    public interface IGameStateReader { /* read-only queries/snapshots */ }
    public interface ICommandHandler<T> where T : IGameCommand {
        PreparationResult Prepare(IGameStateReader state, T command, IRandomSource rng);
    }
    public interface ICommandProcessor {
        CommandResult Execute(GameState state, IGameCommand command);
    }

Предложенные базовые команды: AdvanceDayCommand; в дальнейшем AcceptChoiceCommand (владелец выбора — NPC/Event), InspectDocumentCommand, StartProjectCommand. Не вводить обобщённую команду «изменить деньги» для UI. Экономическая операция появляется только как санкционированная часть domain transaction.

**Семантика Execute:**
1. Проверить CommandId на AppliedCommandReceipt **до** проверки ExpectedRevision/ExpectedDayIndex. Если ID и fingerprint совпадают — DuplicateAlreadyApplied, вернуть текущий state и **не переиздавать events**. Если ID совпал, но payload другой — CommandIdCollision без мутации.
2. Проверить type/version, текущую phase, revision, day, существование сущностей, сроки, разрешения и доступность ресурсов.
3. Выполнить Prepare на read-only view с **копией** RNG state. Результат — transaction plan/state delta, включая экономику, секции состояния, выделенные ID, rng-next-state, список доменных событий.
4. Применить **весь** план к отдельному draft/copy, проверить инварианты и лишь затем принять новый GameState c Revision+1, записью receipt и событиями. Любая ошибка — прежнее состояние полностью неизменно, включая RNG/ledger/events/IDs.
5. Результаты: Applied, DuplicateAlreadyApplied, Rejected (typed error code), CommandIdCollision, InvariantViolation. Для Rejected не резервировать CommandId; повтор с другим payload требует новый ID. Очерёдность доменных событий фиксирована.

**Идемпотентность:** одинаковый ID + одинаковое содержимое никогда не списывает деньги/не увеличивает день повторно; произвольная новая команда AdvanceDay со старым ExpectedDayIndex отвергается. AppliedCommandReceipt хранит CommandId, canonical fingerprint, revision и итоговый статус. Канонический fingerprint рассчитывается по строго определённым полям/порядку (не по порядку обхода Dictionary); схема command/version входит в fingerprint.

**Атомарность:** весь обработчик завершается до опубликования результатов, исключений из обработчика нельзя оставлять частично изменённый state. ICommandHandler не имеет доступов к сети, файлам, Unity, системному времени, сцене или UI. Диспетчер на одном сериализованном writer; события после commit читает UI/Infrastructure.

## 4. События домена и read models

    DomainEventEnvelope
      EventId: string                        // CommandId + ":" + 0-based sequence
      EventType: string                      // versioned "economy.balance-changed.v1"
      Version: int
      DayIndex: int
      StateRevision: long
      Payload: strongly typed domain DTO

Минимум: BudgetChanged, ObligationAccrued, ObligationPaid, DayAdvanced, DayReportProduced; позднее DocumentReviewed, ProposalAccepted, ProjectStarted, ReputationChanged. События UI получает после успешного commit; **они не являются командой и не меняют GameState повторно**. Игровые отложенные эффекты хранятся отдельно в persisted EventState с idempotency key; уведомления UI сами по себе не заменяют persisted queue.

Queries: immutable GameStateView / FinanceView / CalendarView; в первой версии — read-only проекция из snapshot (без Unity). Вся презентационная анимация строится из событий/проекций, не инициирует проводки.

## 5. День и GameClock

- Игровое время дискретно: один ход = один игровой день; **5 игровых дней в неделе**, без выходных в clock. День индексируется с 0. WeekIndex = DayIndex / 5; WeekDayIndex = DayIndex % 5.
- Не использовать deltaTime, DateTime.Now, реальную временную зону или Unity Time для расчётов. Видимая дата, если появится, вычисляется из зафиксированного fictional start-date; не является вторым источником истины.
- AdvanceDayCommand(CommandId, ExpectedDayIndex, ExpectedRevision) выполняется атомарно; current day n закрывается только один раз; новый рабочий день n+1 появляется только после успешного полного pipeline.
- Платежи «в конце недели» начисляются при закрытии дней 4, 9, 14 ...; ключ обязательства = ObligationId + PeriodIndex, сохраняется при закрытии дня даже при недостатке средств.
- При завершении кампании переход в Finished; никакого несуществующего дополнительного рабочего дня.
- DayReport содержит закрытый DayIndex, детерминированные изменения денег/метрик, открытые долги, проведённые обязательства, отложенные эффекты и причины; сохранённые отчёты не вычисляются заново из UI.
- На успешном коммите AdvanceDay публикуется CheckpointRequested для инфраструктурного SaveService (ЧАТ 05); IO не включать в транзакцию домена.

**Предлагаемый canonical pipeline (согласовать обязательно):**
1. Finalize текущий день, разрешить/закрыть незавершённые действия по policy.
2. Начислить/оплатить обязательные платежи (ledger + payables) и доходы текущего периода.
3. Применить прогресс проектов, дедлайны и их результаты.
4. Выполнить уже запланированные последствия/таймеры.
5. Обновить показатели, ограничения, победу/поражение и сформировать закрывающий отчёт.
6. Увеличить DayIndex, обновить cooldown; сформировать следующий день (обязательные → условные → случайные).
7. Commit + DayReportProduced/DayAdvanced/CheckpointRequested.

**Конфликт источников:** docs/ARCHITECTURE.md задаёт платежи до проектов; docs/GDD.md §Г6 — проекты до регулярных платежей. Это **не принятое** предложение, чат 00 должен утвердить один порядок в DECISIONS. Генерация следующего дня делает RNG draws только из отдельно именованного потока.

## 6. Money, EconomyService, LedgerService

- Внутренний тип Money = checked signed Int64 **в копейках** (не float/double/decimal с округлением); 1 игровой рубль = 100 копеек. Все доходы/расходы и статические стоимости используют единый формат.
- EconomyState: OpeningBalanceKopecks, CurrentBalanceKopecks, упорядоченный массив LedgerEntry, ScheduledObligation[], Payable[], ProcessedDueKeys. Не менять баланс прямыми setter из событий, проектов или Presentation.
- LedgerEntry: PostingId (unique), TransactionId/OriginCommandId, DayIndex, Category/ReasonCode, SourceEntityId, DeltaKopecks (signed), BalanceAfterKopecks. Уникальный PostingId детерминирован из TransactionId и номер проводки; ledger append-only, дубликат ключа отвергается.
- Дельта ledger считается через checked arithmetic; сумма в transaction может включать несколько записей. Инвариант: CurrentBalance = OpeningBalance + Sum(LedgerEntry.DeltaKopecks), без переполнений. Все операции команды проходят одним commit вместе с изменениями нефинансовых секций.
- Произвольные списания по отдельному UI-вызову запрещены; сторонний handler формирует санкционированный план. EconomyService рассчитывает доступность средств, формат и обязательства; LedgerService проверяет уникальность/фиксирует проводки.
- У обязательства разные состояния: NotDue, AccruedUnpaid, Paid. Уплата и начисление долга — **разные** факты. Платёж без средств не должен выдавать BudgetChanged на несуществующее списание. Один due-key создаёт ровно одно начисление.
- Рабочая policy до решения O-004: добровольная покупка при нехватке денег => Rejected(InsufficientFunds); обязательный платёж => AccruedUnpaid с кредиторской задолженностью, без отрицательного cash. Overdraft/частичная оплата **НЕ утверждены**; при изменении сложности применяется явный DifficultyPolicy, а не разрозненные if. Сумма задолженности не включается повторно в бюджет; отдельное её погашение даёт ledger posting.
- Начальный баланс создаётся как OpeningBalance в GameState, а не «случайная доходная проводка». Стартовые суммы и тарифы — предмет чата 04/чата 00, в #2/#3 не зашивать цифры из GDD.

## 7. RNG / воспроизводимость

- Выбрать и зафиксировать реализацию **PCG32** (state uint64, odd increment uint64) с golden vectors, алгоритм/версия = pcg32-v1. System.Random, UnityEngine.Random и Guid.NewGuid внутри доменных правил запрещены.
- CampaignSeed явно задаётся при создании игры; отдельные именованные потоки (например core.day, npc.visitors, economy.events) из фиксированного derivation. RngState каждого использованного потока сериализуется. Доступ по стабильному имени, без зависимости от порядка регистрации систем.
- Random.NextInt(maxExclusive) — unbiased bounded sampling; сортировать candidates по стабильному ID перед weighted select. Никакой случайности в read models/UI queries. Отклонённая команда не расходует PRNG.
- Runtime и Save DTO не преобразуют uint64 через floating-point JSON. Предлагаемый wire-формат RNG: фиксированные hex-строки, money Int64 — десятичная строка либо integer с проверкой 64-bit-safe сериализатора; решение с ЧАТ 05 до реализации SaveService.
- Golden tests: одинаковые seed+ordered commands → одинаковый hash состояния и ledger; продолжение после save/load совпадает с непрерывным; добавление NPC draws не меняет поток экономики.

## 8. Проверяемые инварианты и acceptance criteria (#2, #3)

| Проверка | Ожидание |
|---|---|
| Duplicate command ID | Нет второй мутации, новых domain events и RNG draws |
| Same ID + иной payload | CommandIdCollision, неизменный snapshot |
| Старый ExpectedDayIndex или Revision | Rejected без изменений |
| Неудачная составная операция | Ни одной из частичных ledger/metrics/project мутаций |
| Успешный экономический план | Сумма ledger = итог cash delta; уникальные PostingId |
| Недостаток cash по policy | Нет запрещённого отрицательного баланса, обязательство остаётся учтённым |
| Два AdvanceDay подряд для одного дня | Не более одного закрытия дня/недельного начисления |
| 30 рабочих дней | Нет пропущенных/дублированных периодов и событий |
| RNG golden + восстановление | Идентичные outcomes при одинаковом сохранённом состоянии |
| Метрики | В пределах 0–100 |
| Сборка | Core тестируется без сцены, Unity EditMode компилируется (после реального Unity project) |

## 9. Контракты для параллельных чатов

| Потребитель | Что получит после C0 | Пока не должен делать |
|---|---|---|
| 02 NPC | ICommandHandler<T>, typed DomainEventEnvelope, event state key, read-only queries, named RNG | Изменять GameState напрямую и создавать свои money-операции |
| 03 UI | IGameCommand, CommandResult/ErrorCode, immutable FinanceView/CalendarView, events | Инстанцировать чужой GameState и менять баланс из UI |
| 04 Content | стабильные money units, definition IDs, effects как declarative data, handler registry | Исполняемый код в контентном JSON и баланс в Core |
| 05 Save/QA | versioned GameState DTO, applied receipts, ledger, RNG state, due-keys и checkpoint event | Самостоятельно менять GameState schema или начислять при загрузке |

## 10. Решения на утверждение чатом 00

- **C0-01:** Unity 6.3 LTS; папка unity/Assets/_Project, assembly names/dependency direction.
- **C0-02:** GameState root schema v1, пять немонетарных метрик 0–100 + Budget, стабильные IDs и ревизии.
- **C0-03:** Command ID/fingerprint/expected revision/day, copy-on-write prepare/apply, no re-emit duplicate events.
- **C0-04:** GameClock: DayIndex 0-based, 5 рабочих дней/неделя, end-of-week = 4/9/14, Finished.
- **C0-05:** порядок EndDay (разрешить конфликт GDD vs ARCHITECTURE).
- **C0-06:** Money = checked Int64 копейки, append-only ledger и due-key idempotency.
- **C0-07:** policy недостатка денег (предложение: voluntary reject, mandatory accrue payable); O-004 пока открыт.
- **C0-08:** PCG32-v1 и именованные потоки; сохраняемый RNG state.
- **C0-09:** границы persistence: ЧАТ 01 владеет доменной DTO schema, ЧАТ 05 владеет IO/envelope/migrations.
- **C0-10:** какие root placeholders входят в v1, их версии и владельцы; расширение через отдельный schema-PR.

Пока C0 не принят, изменение контрактов в этом файле считается review-предложением, а не изменением архитектуры проекта.
