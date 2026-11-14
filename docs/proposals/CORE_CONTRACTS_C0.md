# Gate C0 — предложение контрактов Simulation Core (ЧАТ 01)

**Статус:** REVISED PROPOSAL v2 после замечаний координатора 00 к PR #9; **повторный Gate C0 review обязателен**. Не является утверждённым ADR и не реализован в коде. Порядок EndDay и недостаток средств в этой редакции приведены в соответствие с комментарием координатора; это пока **кандидат** на фиксацию.  
**Основание:** Issues #2, #3; README, AGENTS, GDD v0.1, ARCHITECTURE, MECHANICS, DECISIONS, CHAT_WORKFLOW.  
**Ветка:** feature/core-2. **Область:** Core/Application/Economy; без UI/NPC/контента/файлового persistence.  
**Версия предложения:** C0-v2, 14.11.2026. Review: https://github.com/rixed318/Probably-TAU/pull/9#issuecomment-6877397075. Смежные проекты: QA PR #10, Content PR #11, UI PR #12.

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
      ContentVersion: string                          // версия закреплённого набора игровых definitions
      Clock: GameClockState { DayIndex: int, Phase: Active|Finished }
      Economy: EconomyState { OpeningBalance, Balance, Ledger[], Obligations[], Payables[] }
      Metrics: AcademyMetrics { EducationQuality, StudentSatisfaction,
                               StaffMorale, EmployerTrust, PublicReputation }
      // НЕ создавать второе mutable ReputationState в v1. S/M/E/R уже являются source of truth
      Random: RandomState                             // именованные RNG-потоки
      Commands: AppliedCommandReceipt[]               // ID + canonical fingerprint + outcome
      ProcessedDayKeys: string[]                       // уникальные campaignId/dayIndex для EndDay
      DayReports: DayReport[]                          // либо ограниченный журнал с последним отчётом
      Flags: deterministic sorted key-value scalars
      Events: EventState                              // резерв schema для чата 02
      Documents: DocumentState                        // резерв schema для чата 02/04
      Visitors: VisitorState                          // резерв schema для чата 02
      Projects: ProjectState                          // резерв schema для будущего проектного сервиса
      Inventory: InventoryState                        // резерв schema; не реализовывать в Issue #2

Типы резервных секций, их поля и version будут отдельно согласованы владельцами подсистем. Пустые секции допускаются в первом vertical slice, но **нельзя подменять их UnityEngine.Object, Dictionary<object,object> или произвольными сериализуемыми C#-объектами**. В C0 фиксируются имена ключей и ответственность, а не контентная схема.

**Метрики и репутация v1 — единый source of truth:** ровно пять целочисленных Q/S/M/E/R от 0 до 100 с clamp(0,100): **Q=EducationQuality** (качество, **не** отношение); **S=StudentSatisfaction** (студенты); **M=StaffMorale** (преподаватели и сотрудники); **E=EmployerTrust** (работодатели и партнёры); **R=PublicReputation** (общественное мнение/публичный имидж). Budget — отдельный шестой отображаемый показатель в Int64 копейках, **не** метрика 0..100. **Не хранить отдельное mutable ReputationState** в vertical slice: отношения студентов/сотрудников/партнёров/публики проецируются только из S/M/E/R, без дублирующей записи. Конкретные новые группы (абитуриенты/родители и т.д.) появятся в поздней versioned секции с миграцией; до этого read model может показывать только четыре эти группы. Обновление Q/S/M/E/R производится из одного транзакционного плана; история причин — в domain events/DayReport.

**IDs:** стабильные строковые ID определений (например event.lab_upgrade), стабильные UUID-формат ID экземпляров. Детерминированный Core ID-allocator получает campaignSeed + namespace + монотонный счётчик или другой устойчивый вход; запрещены Guid.NewGuid()/DateTime.Now() внутри симуляции. CommandId задаёт вызывающая сторона и передаётся как вход симуляции; одинаковое воспроизведение предполагает одинаковые команды, включая их ID.

**Версии и совместимость:** GameState.SchemaVersion (domаin payload: начальная версия 1) **отдельна** от SaveEnvelope.schemaVersion (внешний формат; начальная версия 1), GameState.ContentVersion (например `dayone-draft-1`, реальный content pack определяется чатом 04/00) и SaveEnvelope.buildVersion (диагностическая метка сборки). Изменение схемы состояния ≠ изменение envelope ≠ обновление контента. ЧАТ 01 отвечает за сериализуемый domain DTO/snapshot и проверку инвариантов; ЧАТ 05 — envelope, checksum, файл, атомарность, миграции внешней схемы. Контентные ID должны разрешаться через утверждённый реестр; неизвестные/переименованные ID — явная ошибка или согласованная миграция, никогда молчаливая подмена. В snapshot входят ledger, debts, AppliedCommandReceipt (полный fingerprint + original outcome), ProcessedDayKeys, source/due IDs, RngState всех потоков, pending/done event IDs, queue, deadlines, flags и отчёты. DTO не содержит Unity-объектов. При Save→Load→Save без команд canonical domain bytes логически совпадают, если сохранён contentVersion; миграции допускают явно задокументированную канонизацию.

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

**Предлагаемый read API (ещё не C# implementation):**

    IGameQueryService
      GetHeader()       -> GameHeaderView { CampaignId, Revision, DifficultyId, ContentVersion,
                                            DayIndex, DayNumber, Phase, Q,S,M,E,R }
      GetFinance()      -> FinanceView { BalanceKopecks, PayablesTotalKopecks, DueObligations[],
                                         RecentLedgerRows[], ExpectedIncome[] }
      GetCalendar()     -> CalendarView { DayIndex, DayNumber, WeekIndex, WeekDayNumber,
                                          NextWeekSettlementDayIndex, CanAdvance, BlockedReasons[] }
      GetDayReport(closedDayIndex) -> DayReportView { ClosedDayIndex, OpeningBalanceKopecks,
                                          ClosingBalanceKopecks, PostedEntries[], DebtWarnings[],
                                          MetricDeltas[], EffectSummaries[], ReasonCodes[] }
      GetOffice()       -> OfficeDashboardView { Header, FinanceSummary, Calendar,
                                                  VisitorIds[], DocumentIds[], AvailableActions[] }
      GetDocument(id)   -> DocumentDetailView { DocumentId, DefinitionId, DeadlineDayIndex,
                                                VisibleFields[], Observations[], Actions[], DenialReasons[] }

Все view — immutable snapshot/deep copy; коллекции возвращаются в фиксированном порядке, идентификаторы имеют стабильный тип, деньги возвращаются в копейках (форматирование RUB — только Presentation). `GetOffice`/`GetDocument` лишь **формы интеграционных DTO**: фактические поля NPC/DocumentState уточняет чат 02, тексты — чат 04, UI не получает скрытую truth. ЧАТ 02 получает `IGameStateReader` для eligibility и явный scoped `IRandomSource` на фазе Prepare. ЧАТ 05 получает `IGameSnapshotPort.CaptureCommittedState() : GameStateSnapshotDto` (глубокая копия, без RNG draw/IO) и `IGameStateRestorer.ValidateThenRestore(dto)` с атомарной активацией только после успешной валидации. Вся презентационная анимация строится из событий/проекций, не инициирует проводки.

## 5. День и GameClock

- Игровое время дискретно: один ход = один игровой день; **5 игровых дней в неделе**, без выходных в clock. День индексируется с 0. WeekIndex = DayIndex / 5; WeekDayIndex = DayIndex % 5; **DayNumber = DayIndex + 1**. То есть `DayIndex=0` = интерфейсный **«День 01»**.
- Не использовать deltaTime, DateTime.Now, реальную временную зону или Unity Time для расчётов. Видимая дата, если появится, вычисляется из зафиксированного fictional start-date; не является вторым источником истины.
- AdvanceDayCommand(CommandId, ExpectedDayIndex, ExpectedRevision) выполняется атомарно; current day n закрывается только один раз; новый рабочий день n+1 появляется только после успешного полного pipeline.
- Платежи «в конце недели» начисляются при закрытии дней с `DayIndex=4,9,14,…` (**«День 05», «День 10», «День 15»**). Идемпотентный ключ закрытия дня = `campaignId/dayIndex`; ключ обязательства = `obligationId/periodIndex` (PeriodIndex=DayIndex/5). Все ключи durable и сохраняются даже при недостатке cash. Проверять both command receipt и ProcessedDayKeys, чтобы новая CommandId не обходила single-endday guard.
- При завершении кампании переход в Finished; никакого несуществующего дополнительного рабочего дня.
- DayReport содержит закрытый DayIndex, детерминированные изменения денег/метрик, открытые долги, проведённые обязательства, отложенные эффекты и причины; сохранённые отчёты не вычисляются заново из UI.
- На успешном коммите AdvanceDay публикуется CheckpointRequested для инфраструктурного SaveService (ЧАТ 05); IO не включать в транзакцию домена.

**Canonical pipeline v2, предложенный координатором 00 (PR #9 review; повторное утверждение C0 требуется):**
1. **Finalize** текущий DayIndex n: завершить валидные решения и закрыть неразрешённые кейсы/дедлайны согласно объявленной policy. Действия с deadline n остаются разрешены в активной фазе n; при закрытии n просроченные без решения получают явный outcome. Уведомления UI не разрешают решение после границы.
2. **Projects**: применить ровно один progress tick к каждому активному проекту за закрываемый n, отсортированному по ProjectId. Проект, запущенный в День n, не получает полный tick в день запуска (для P02 нужны три **полных** EndDay после запуска); факт старта/прогресса durable.
3. **Due scheduled outcomes**: обработать отложенные эффекты с `DueDayIndex <= n` по (`DueDayIndex`, `Priority` descending, `StableEventId` ascending), с durable executed-effect ID. Состояния проектов и последствия могут сделать доход **eligible**, но сами не проводят `LedgerEntry`.
4. **Due income / obligations**: только **после** этапов 2–3 провести доказанный/условно-заслуженный доход и регулярные/разовые обязательства через единый ledger. Для DayIndex 4,9,14… обрабатывать недельные платежи ровно один раз; внутри этапа сортировать постинги по (DueDayIndex, SchedulePriority, SourceStableId, PostingType). Подписанное партнёрское письмо ≠ earned revenue: партнёрский доход за День 04/05 появляется только по реально выполненному событию и признанному `earnedId`, а не в день согласия.
5. **Metrics & terminal**: применить объяснимые Q/S/M/E/R delta с clamp, долги и debt warnings; оценить победу/поражение по зафиксированной policy. Обновить последнюю закрытую дату и обработанные ключи.
6. **Next-day generation only if not terminal**: если кампания продолжается, `DayIndex=n+1`, обновить cooldown, сформировать очередь обязательные → срочные → случайные; если terminal — `Phase=Finished`, **не** генерировать несуществующий следующий день и не расходовать RNG на него.
7. **Report/commit/checkpoint**: сформировать `DayReport` для закрытого n, согласованный с ledger; атомарно commit всей новой state/revision/receipts/processed-day/effect keys; только после commit опубликовать `DayReportProduced`, `DayAdvanced` (если есть переход), `CheckpointRequested`. Save I/O — отдельно, без внутридоменной мутации.

**Граница дня:** ни один промежуточный этап не сохраняется и не становится видимым другим писателям; если любой этап падает, ledger, проекты, RNG, день, отчёт и processed keys откатываются целиком. Scheduled effects с due n, созданные внутри текущего шага до этапа 3, требуют одной фиксированной политики: выполнять один проход по snapshot due queue на входе этапа 3, вновь созданные due n переносить на n+1 либо явно запретить создание due<=n; рекомендация — запрет создания `DueDayIndex <= n` в этапе 2/3 во избежание циклов. Дедлайны фиксируются на финализации, а событие «доход заработан» должно возникнуть не позже этапа 3 данного перехода, чтобы попасть в этап 4.

**Причина изменения:** в GDD §Г6 проекты идут раньше платежей, в ARCHITECTURE.md наоборот. Координатор 00 выбрал последовательность проектов → эффектов → проводок, но до фиксации в docs/DECISIONS.md она остаётся PR-кандидатом.

## 6. Money, EconomyService, LedgerService

- Внутренний тип Money = checked signed Int64 **в копейках** (не float/double/decimal с округлением); 1 игровой рубль = 100 копеек. Все доходы/расходы и статические стоимости используют единый формат.
- EconomyState: OpeningBalanceKopecks, CurrentBalanceKopecks, упорядоченный массив LedgerEntry, ScheduledObligation[], Payable[], ProcessedDueKeys. Не менять баланс прямыми setter из событий, проектов или Presentation.
- LedgerEntry: PostingId (unique), TransactionId/OriginCommandId, DayIndex, Category/ReasonCode, SourceEntityId, DeltaKopecks (signed), BalanceAfterKopecks. Уникальный PostingId детерминирован из TransactionId и номер проводки; ledger append-only, дубликат ключа отвергается.
- Дельта ledger считается через checked arithmetic; сумма в transaction может включать несколько записей. Инвариант: CurrentBalance = OpeningBalance + Sum(LedgerEntry.DeltaKopecks), без переполнений. Все операции команды проходят одним commit вместе с изменениями нефинансовых секций.
- Произвольные списания по отдельному UI-вызову запрещены; сторонний handler формирует санкционированный план. EconomyService рассчитывает доступность средств, формат и обязательства; LedgerService проверяет уникальность/фиксирует проводки.
- У обязательства разные состояния: NotDue, AccruedUnpaid, Paid. Уплата и начисление долга — **разные** факты. Платёж без средств не должен выдавать BudgetChanged на несуществующее списание. Один due-key создаёт ровно одно начисление.
- **Предложенная координатором политика v1 (PR #9):** добровольная покупка без cash → `Rejected(InsufficientFunds)`; обязательный платёж без cash → `AccruedUnpaid`/Payable с warning и **без** отрицательного cash/ложной проводки. Числовой долг не вычитается из cash повторно; его последующее погашение — одна уникальная ledger posting. `CurrentBalanceKopecks >= 0` во всех committed v1 GameState, `PayablesTotalKopecks >= 0`. `checked` arithmetic и fail-closed on overflow. Овердрафт и частичная выплата — **вне v1**, только отдельное решение/механика с тестами. DifficultyProfile определяет стартовые ресурсы/обязательства, но не изменяет это базовое правило без новой утверждённой policy.
- Начальный баланс создаётся как OpeningBalance в GameState, а не «случайная доходная проводка». Стартовые суммы и тарифы — предмет чата 04/чата 00, в #2/#3 не зашивать цифры из GDD.

## 7. RNG / воспроизводимость

- Выбрать и зафиксировать реализацию **PCG32** (state uint64, odd increment uint64) с golden vectors, алгоритм/версия = pcg32-v1. System.Random, UnityEngine.Random и Guid.NewGuid внутри доменных правил запрещены.
- CampaignSeed явно задаётся при создании игры; отдельные именованные потоки (например core.day, npc.visitors, economy.events) из фиксированного derivation. RngState каждого использованного потока сериализуется. Доступ по стабильному имени, без зависимости от порядка регистрации систем.
- Random.NextInt(maxExclusive) — unbiased bounded sampling; сортировать candidates по стабильному ID перед weighted select. Никакой случайности в read models/UI queries. Отклонённая команда не расходует PRNG.
- **Предлагаемый domain wire-format:** `MoneyKopecks`, `Revision` и 64-bit counters — десятичные JSON **strings**, распознаваемые строго как Int64 с `InvariantCulture` и проверкой границ; PCG32 `state` и `increment` — строки из 16 lower-case hexadecimal символов, плюс `algorithmId="pcg32-v1"` и `streamName`. Serialize deterministic sorted arrays для применённых команд, потоков RNG и ключей, сохранить исходный упорядоченный ledger/event queue. Никаких 64-битных величин через float/double. `SaveEnvelope.schemaVersion` и checksum/канонизация внешнего файла принадлежат ЧАТ 05; пример числовых полей из draft SAVE_SPEC не утверждён и может требовать адаптации.
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
- **C0-02:** GameState v1: Q/S/M/E/R как единый mutable source of truth, без дублирующего ReputationState; Budget отдельно, stable IDs и revisions.
- **C0-03:** Command ID/fingerprint/expected revision/day, copy-on-write prepare/apply, no re-emit duplicate events.
- **C0-04:** GameClock: DayIndex 0-based, 5 рабочих дней/неделя, end-of-week = 4/9/14, Finished.
- **C0-05:** согласовать скорректированную схему EndDay: finalize → projects → due outcomes → earned income/obligations → metrics/terminal → conditional next-day → report/atomic commit/checkpoint; Deadline/priority rules.
- **C0-06:** Money = checked Int64 копейки, append-only ledger и due-key idempotency.
- **C0-07:** proposed v1: voluntary reject, mandatory accrue payable + warning, no cash overdraft, checked overflow; расширенная политика O-004 позже.
- **C0-08:** PCG32-v1 и именованные потоки; сохраняемый RNG state.
- **C0-09:** разделить GameState.SchemaVersion, SaveEnvelope.schemaVersion, ContentVersion, buildVersion; утвердить domain wire-format receipts/ledger/events/RNG и Save 05 IO/envelope/migrations.
- **C0-10:** какие root placeholders входят в v1, их версии и владельцы; расширение через отдельный schema-PR.


## 11. Конкретные контрольные сценарии для совместного QA и интеграции

**GT-01. Номер дня и недельный платёж.** Исходно `DayIndex=0`, UI «День 01», `Balance=2_000_000_00` копеек (2 000 000 ₽), weekly expense 400 000 ₽ с due DayIndex=4, weekly income 340 000 ₽ (если **условия действительно выполнены**). Четыре успешных AdvanceDay закрывают дни 0–3 без недельных операций; пятый закрывает DayIndex=4 («День 05»), создаёт ровно один `obligationId/0`, один `earnedId/0` при фактическом выполнении условий. При полном проведении доходов/расходов cash = 1 940 000 ₽; при невыполненном доходе cash = 1 600 000 ₽. Пример — *тестовый fixture*, не утверждённый баланс игры. Повтор того же или нового CommandId с ExpectedDayIndex=4 не изменяет эти суммы.

**GT-02. Payable при недостатке средств.** Start cash=50 000 коп., обязательный expense=100 000 коп. на закрываемый день: cash остаётся 50 000 коп.; payable +100 000 коп., предупреждение о задолженности; ledger cash entry на -100 000 **не** создаётся. На повторном EndDay/после Save→Load debt не удваивается; при будущей оплате обязательства достаточно средств — ровно одна запись -100 000 коп., payable уменьшается на 100 000 коп. (частичной оплаты в v1 нет).

**GT-03. Конфликт добровольной операции.** Cash=100 000 коп., voluntary spending=100 001 коп. → Rejected(InsufficientFunds), без ledger, mutation, receipt, RNG draws или изменения Revision. `long.MaxValue + 1` в любой денежной операции → structured overflow error и полный rollback.

**GT-04. Отложенный доход.** Партнёр согласился в День 01, доход 120 000 ₽ предусмотрен только после успешного мероприятия на Дне 04 и признания результата по `earnedId`. Согласие не создаёт доходной posting. В конце Дня 04 outcome в этапе 3 ставит earned marker; лишь после этого этап 4 допускает +120 000 ₽. Если мероприятие не состоялось, posting отсутствует. Повторное воспроизведение command/effect/Save→Load не удваивает его.

**GT-05. Транзакционный rollback.** Два project ticks, due effect и weekly payment составляют одну EndDay transaction. Если проверка последней записи из-за overflow/invalid reference падает, никакой из первых проектных tick, event consume, ledger, RNG, processed-day keys и DayIndex не изменяется.

**GT-06. Finish без следующего дня.** Если terminal condition выполнена при закрытии n, получить `DayReport(ClosedDayIndex=n)`, `Phase=Finished`, без next-day visitor generation и без RNG consumption на этот этап; повторное завершение отклоняется без мутаций.

**GT-07. PRNG golden output.** Для reference PCG32 XSH-RR с 64-bit state, multiplier `6364136223846793005`, seed `initstate=42`, stream `initseq=54`, increment `(54<<1)|1=109`, стандартная инициализация (`state=0; step(); state += initstate; step()`), первые шесть unsigned 32-bit выходов:
`0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e`.
Отдельно зафиксировать версию `name→stream seed derivation` и golden fixtures для игровых названий потоков до реализации; reference PCG-вектор не подменяет derivation test.

**GT-08. Стабильный Save→Load.** Сохранить payload с четырьмя GameState metric-reputation группами S/M/E/R, Q отдельно, ledger cash entries, 1 payable, 1 command receipt с fingerprint, 1 processed-day key, 1 executed-effect marker, PCG32 stream state. `CaptureCommittedState→serialize→load→restore→capture` сохраняет логическое равенство всех полей; без команды не происходит новый ledger event, reward, NPC spawn или RNG draw. Unknown future payload schema/content ID → structured rejection без подмены активного состояния.

**GT-09. Reproducible ordered events.** При одинаковом `CampaignSeed`, `ContentVersion`, `GameState`, ordered command stream и definition registry должны совпасть `EventId`, `EventType`, `Revision`, ordered ledger/DayReport и RNG states; изолированные NPC random draws не меняют PCG32 других named streams.

**Gate C0 check:** ЧАТ 00 подтверждает/уточняет эти acceptance cases; ЧАТ 05 расширяет их до QA-001…QA-034 без дублирования логики. ЧАТ 04 сохраняет гипотезу 2 000 000 ₽ в своём Content, а ЧАТ 03 заменит HTML mock 1 500 000 ₽ на согласованную FinanceView при Unity-интеграции. Отдельный `ReputationState` для v1 не создавать.

Пока C0 не принят, изменение контрактов в этом файле считается review-предложением, а не изменением архитектуры проекта.
