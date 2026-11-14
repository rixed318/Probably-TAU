# Архитектура Unity / C#

Документ — целевая архитектура, **не существующая реализованная кодовая база**.

## Слои
```text
Unity Presentation (UI Toolkit / scene / sound / animations)
          ↓ commands, queries
Application (CommandProcessor, services, use-cases)
          ↓
Core Simulation (GameState, Economy, Events, NPC, Document, Projects)
          ↓ ports/interfaces
Infrastructure (versioned saves, data loading, diagnostics)
```

- **Core:** обычная библиотека C# без UnityEngine. Тесты без сцены/рендера.
- **Application:** выполняет команду после проверки пред- и постусловий; возвращает результат и domain events.
- **Presentation:** исключительно отображение model/view-model, отправка команд, анимации и ввод.
- **Infrastructure:** JSON snapshot, настройки, локальный файловый доступ, каталог ресурсов.
- **Content:** ScriptableObject definitions или сериализованные data assets без mutable игровой сессии.

## GameState — разделы
`WorldTime`, `EconomyState`, `StaffState`, `StudentsState`, `ReputationState`, `DocumentQueue`, `ProjectState`, `VisitorQueue`, `EventState`, `InventoryState`, `RandomState`, `Flags`. Конкретная схема и названия публичных классов будут закреплены перед кодом.

## Команды (примеры)
`InspectDocument`, `RequestClarification`, `ApproveProposal`, `RejectProposal`, `CounterOffer`, `AdvanceDay`, `StartProject`, `MoveInventoryItem`. Каждый обработчик проверяет: существование сущностей, текущий игровой этап, дедлайны, ресурсные ограничения и отсутствие повторного применения.

## События домена
`DocumentReviewed`, `BudgetChanged`, `ReputationChanged`, `ProjectStarted`, `VisitorDeparted`, `DayAdvanced`. UI подписывается на итоги, но не является владельцем состояния.

## Порядок AdvanceDay
1. Финализация команд текущего дня.
2. Выплата/начисление обязательных платежей через ledger.
3. Тики проектов и дедлайны.
4. Разрешение запланированных последствий.
5. Проверка поражения/победы и ограничений.
6. Сдвиг календаря и детерминированное формирование следующего дня.
7. Создание контрольного checkpoint и вечернего/утреннего отчёта.

Порядок после тестов может быть изменён только через зафиксированное решение.

## Сохранения
Версионированный snapshot `schemaVersion`, ID сущностей, положение PRNG, текущая очередь событий, ledger, флаги, календарь, уникальные ключи применённых команд. Запись в временный файл и атомарная замена, backup/миграции. После загрузки нельзя повторно начислить расходы и нельзя менять уже сгенерированного посетителя.

## Тестирование
Unit: операции и инварианты бюджета, репутация с границами, проверки заявок, генерация с seed, повторы команд; integration: AdvanceDay 30 раз, Save→Load→Save, сценарии с финансовым кризисом. PlayMode: UI, переход сцены, быстрые открытия/закрытия окон.

## Стратегия Unity-папок (предложение)
```text
unity/
  Assets/_Project/Runtime/Core/
  Assets/_Project/Runtime/Application/
  Assets/_Project/Runtime/Presentation/
  Assets/_Project/Runtime/Infrastructure/
  Assets/_Project/Content/
  Assets/_Project/Tests/EditMode/
  Assets/_Project/Tests/PlayMode/
```
Unity .meta-файлы коммитить. Не коммитить Library/Temp/Obj/Builds/UserSettings.
