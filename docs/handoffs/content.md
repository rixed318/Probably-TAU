# Handoff — CHAT 04 / Content & Balance

**Дата:** 2026-11-14  
**Исполнитель:** чат 04 — геймдизайн/сценарии  
**Ветка:** feature/content-7  
**Issue:** [#7 — Content](https://github.com/rixed318/Probably-TAU/issues/7); связанный [#1 — первый день](https://github.com/rixed318/Probably-TAU/issues/1)  
**PR:** [#11 — Первый день, 13 событий и баланс (DRAFT)](https://github.com/rixed318/Probably-TAU/pull/11); координатор 00 утверждает содержание и отвечает за merge.  
**Status:** content proposal 0.2, не готовые Runtime/Unity assets.

## Что сделано

- [Content/DAY_01.md](../../Content/DAY_01.md): подробный оригинальный первый день (15–25 минут), 3 вымышленных NPC, 3 типа документов, 2 исследования, 3 существенных решения, неожиданный конфликт расписания, 4-й экстренный выбор, объяснимый вечерний отчёт и 10 сценарных acceptance checks.
- [Content/EVENT_CHAINS.md](../../Content/EVENT_CHAINS.md): 13 связанных событий **после первого дня** (E02–E14) до Дня 10; исходы и fallback для каждой ветви; 2 проекта развития P01/P02, затраты, длительность, условия завершения и отложенные последствия.
- [Content/BALANCE_V0_1.md](../../Content/BALANCE_V0_1.md): копеечная модель, 4 **предложенных** режима сложности, тестовые траектории, условия успеха/проигрыша, количественные критерии и план QA.
- [Content/CONTENT_SPEC_DRAFT.json](../../Content/CONTENT_SPEC_DRAFT.json): валидный JSON (не runtime schema) с черновыми ID NPC/документов/выборов/проектов, 13-event manifest, требованиями и правилами валидации.

Все финансовые показатели, организации, конфликты, имена и помещения явно обозначены **вымышленными**; не приписывать реальные нарушения и денежные отчёты ТАУ.

## Что НЕ сделано

- Никаких изменений Core, UI, NPC, Unity assets, реальных ScriptableObject, asmdef или сохранений.
- Никаких изменений общей архитектуры, GDD, DECISIONS или roadmap: они принадлежат чату 00.
- Отсутствует подтверждённая машинная content schema/DSL, команды/события/метрики и контракт GameState от чата 01.
- Нет прогонов Unity EditMode, PlayMode и 1000 симуляций кампании на 30 дней: контент второй половины ещё не создан.

## Предложения для утверждения 00

1. Утвердить завязку «Три подписи, одна аудитория» и акт Дня 01 как каноническую вертикальную демонстрацию, либо указать правки.
2. Утвердить 5 показателей Q/S/M/E/R, либо официальный маппинг на согласованный GameState, и максимально 2 углублённые проверки Дня 01.
3. Согласовать условные выплаты (по договору только после исполнения) и ссылку на ledger, запрет дублирующего начисления.
4. Согласовать слот аудитории и правила E02-A/E02-B/E02-C; NPC-реализацию передать 02, отображение 03.
5. Проверить баланс и предложенные концовки в QA 05; сложность Crisis отложить до реальных доходных событий Дней 11–30.

## Изменённые публичные контракты

**Никакие.** Флаги LAB/STUDENT/PARTNER/ROOM, Q/S/M/E/R, проверки, event IDs и поля JSON — контентный словарь **DRAFT**, не принимаются как реальные C# property names. Обновление после Gate C0 потребует отдельного согласования с 00, 01 и 02.

## Проверка

**Уже проверено вне Unity:**
- JSON корректно парсится, 3 документа, 3 визита, 13 последующих событий и 2 проекта.
- Уникальные ID; **27/27** сочетаний первых трёх выборов допустимы с лимитом ≤2 проверок, E02 обязательный fallback, нет недостижимого перехода уже в конце Дня 01.
- Финансовая нижняя граница День 01 на Normal среди 27 комбинаций — **1 715 000 ₽**, верхняя — 2 000 000 ₽; деньги по договору не начисляются в День 01.
- Примерные бюджетные траектории Дни 01–10 рассчитаны вручную, детализированы в BALANCE_V0_1.md; это не эквивалент теста симуляции.

**Проверить после Gate C0:**
- Deterministic seed + save/load на каждом выборе; повторные EndDay Дня 05/10 и один idempotency ledger key на транзакцию.
- Устойчивость к 0–1 активной броне, отказу от всех заявок, бюджету ниже цены спасения и отсутствию куратора.
- Разделение одного слота Л-204, предельные метрики 0–100, отсутствие фиктивного результата и фиктивного дохода.
- P02 завершает 3 **полных EndDay после дня запуска** и не получает доход раньше окончания.
- Реальный сценарный тест 15–25 минут на разрешении 1366×768 вместе с UI 03.

## Риски и зависимости

- **Gate C0:** Core GameState, time, money, commands, ledger, event queue и inspections ещё не утверждены. Нельзя импортировать JSON непосредственно в Unity.
- **NPC 02:** требуется согласовать event conditions, fallback branches, negotiations, stable IDs и переносы аудиторий.
- **UI 03:** должны быть видны в одном месте сумма, срок, помещение, статус брони, условность дохода, цена проверки и причины эффекта.
- **QA 05:** нужны проверки экономического моделирования и полноты графа по всем ветвям.
- **Правовые:** художественный сеттинг обязан явно отделять ТАУ от вымышленного игрового мира; для реальных логотипов/изображений требуется согласование.
- **Баланс:** профили Hard/Crisis и 30-дневные победные условия могут оказаться нерешаемыми без будущего контента, поэтому пока предложения.

## Следующий шаг

Координатор 00 рассматривает PR, утверждает или корректирует scope Дня 01 и передаёт согласованные IDs/контракт чату 02 и UI-потоки чату 03. **Не выполнять merge в main без явного одобрения владельца.**

## Автономный проход 04 — C0 DayIndex mapping (14.11.2026)

**Scope:** одна ограниченная задача — синхронизировать `Content/CONTENT_SPEC_DRAFT.json` с утверждённым Gate C0-v2 по календарю и финансам, **без изменения сценарных исходов и Core API**.

- Обновлён `meta.specVersion` до `content-draft-0.2`; добавлены явно нулевые `dayIndex` для старта, всех 13 событий E02–E14 и трёх видимых слотов документов. Старые поля `day` оставлены как **человекочитаемый номер дня**, чтобы не ломать черновые UI/mock ссылки.
- День 01 → `dayIndex=0`; конфликтный слот День 04 → `dayIndex=3`; E09 и первое недельное закрытие День 05 → `dayIndex=4`; E14 День 10 → `dayIndex=9`. Следующие недельные закрытия: 4, 9, ...
- Партнёрский условный доход не является cash при подписании: `revenueEarliestDayIndex=4`, требуется delivery и уникальная ledger posting; все суммы — Int64 копейки. У P02 описаны 3 **полных** EndDay после дня запуска.
- Сценарные IDs, суммы, 27 сочетаний выборов, исходы и тексты не менялись. Данные остаются **неисполняемой DRAFT спецификацией**, а не готовой EventDirector/Unity схемой.

**Проверено:** JSON.parse в JS, 13 уникальных ID, согласованность всех 13 `dayIndex=day-1`, E09=4, E14=9, 3 документа со слотом 3, 3 посетителя с тремя вариантами, partner earliest=4, целые денежные delta. **Не выполнялись:** Unity/C# компиляция, симуляция 27 маршрутов на реальном Core, тесты сохранения, полная финансовая регрессия.

**Следующая зависимость:** ЧАТ 02 должен согласовать с 04 typed event hooks/conditions/ledger posting keys; не использовать `triggerPhase` или `revenueRecognition` как уже утверждённые C# enum/API. ЧАТ 03 должен использовать текстовые labels отдельно от `dayIndex`. **Gate C0 разрешён для разработки в feature-ветках через PR #14, но ещё не слит в main.**

## Автономный проход 04 — typed event hooks proposal (14.11.2026)

**Одна задача:** декларативные typed conditions/effects fixtures для E02/E03/E09 и условного партнёрского дохода, не Runtime API.

- `Content/EVENT_HOOKS_DRAFT.json`: `definitionId` — неизменяемый ID определения; `instanceId` — отдельный стабильный Core-owned ID экземпляра; Core сохраняет receipts и контролирует ledger posting keys. Typed AST: `always`, `all`, `any`, `flagEquals`, `factExists`, `dayIndexAtLeast`. Effects: `presentEvent`, `showSettlementReport`, `proposeEarnedIncome`. Суммы дохода в Int64 decimal-string копейках; E02/E03 due=1, E09 due=4 и только отображает результат расчёта Core. Партнёрский доход возможен только после `E03_mentor_paid`, `E07_delivered`, не раньше dayIndex 4; уникальные `earnedId`/`postingKey` проверяет Core, не контент.
- `Content/validate-event-hooks.cjs`: воспроизводимая статическая проверка структуры, duplicate definition ID, раннего дохода, повторного расчёта E09 и некорректной суммы. Запуск: `node Content/validate-event-hooks.cjs`.
- Коммиты: `9732185499b2a3ccf436c1c5a97f2bda36cd03f8` (fixture), `0c99033a3b30510cf05e670da334e5d4b5aca11d` (validator).

**Фактически выполнено:** 5/5 эквивалентных JS-проверок функций валидатора на тех же данных в изолированном JS runtime; **Node CLI на checkout, Unity/C# и интеграция не запускались**. Не считать их проверенными. Новые JSON поля — proposal, не утверждённый API. Текущий `CONTENT_SPEC_DRAFT.json` не заменён, миграция не проводилась.

**BLOCKED на интеграцию:** разногласие Core C0-v2 vs NPC N0 по сортировке due queue; нужны конкретные Core GameState/EventDirector DTO и согласование 01/02/00. Не назначаем phase/priority для E02/E03 и не дублируем E09 settlement. Чат 02 должен дать review этого AST и шаблонов IDs. PR #11 остаётся Draft, `main` не изменён.


## Автономный проход 04 — cross-file content drift guard (14.11.2026)

**Одна ограниченная задача:** добавить независимую проверку согласованности двух уже существующих черновых JSON-файлов, не менять баланс/сюжет и не создавать Runtime EventDirector.

- Добавлен `Content/validate-content-consistency.cjs` (Node.js без зависимостей): сверяет E02/E03/E09 `dueDayIndex` с `eventManifest`, zero-based DayIndex, E09 как только показ Core settlement, оба варианта партнёрского дохода с выборами первого дня, условие фактической поставки, одинаковый шаблон postingKey с `{sourceInstanceId}` в earnedId.
- Коммит: `2c9479048191648cb5c92b24bd753657cd01bd8c`. Запуск в checkout: `node Content/validate-content-consistency.cjs`.
- **Фактически выполнено:** 5/5 проверок в изолированном JavaScript runtime над реальными JSON, полученными из GitHub: baseline, schedule drift, revenue drift, missing delivery, E09 duplicate settlement. **Node CLI на checkout и Unity/C# НЕ запускались**; Node-тест требует отдельного прогона QA.
- Не утверждает schema C0/N0 и не определяет сортировку очереди; по-прежнему ждём решения 01/02/00. Существующий `validate-event-hooks.cjs` не менялся.
- **Следующий шаг:** QA 05 запускает обе Node-команды из checkout, NPC 02 проверяет требования к stable IDs и earned-income facts; 00 решает конфликт очередности и интеграцию. Без merge в main.


## AutoDev 04 — canonical Int64 financial wire fields (14.11.2026, 23:15 +04)

**Scope:** one bounded Issue #7 data-schema cleanup; no gameplay/balance changes. The content schema remains a DRAFT and is not the Core Save wire DTO.

- Converted **21** existing fields with names ending in `Kopecks` in `Content/CONTENT_SPEC_DRAFT.json` from JSON numeric literals to **canonical signed Int64 decimal strings**. `meta.specVersion` changed to `content-draft-0.3`. All numerical values, choices, NPC, definitions, DayIndex and fictional narrative were preserved. Non-money integer fields (DayIndex, metric deltas, counts) remain JSON integers.
- Added `Content/validate-money-wire.cjs` with complete recursive money-field inventory (21) and **8 negative mutations**: JSON number, float, leading zero, negative zero, plus sign, above/below Int64, exponent notation; plus 3 valid boundary values. Run `node Content/validate-money-wire.cjs` after checkout. Its field inventory is intentionally fixed: new money fields require explicit test update/review.
- Existing `validate-content-consistency.cjs` compares partner amounts using `BigInt`, so it accepts the canonical strings without numeric conversion; existing `validate-event-hooks.cjs` already checks string earned-income values.
- Code commits: `162ab975937042294a07997ce887573acb4b7c77` (data), `a706337290e649da90222286cf16688a7369db6c` (validator). Handoff commit is separate.
- **Actually verified:** fresh GitHub fetch of the committed spec; 21/21 money fields are strings and within signed Int64 bounds; onsite/remote partner revenue amounts exactly match the typed hook amounts via `BigInt`. The before/after transform compared all 21 original integer values to the resulting decimal strings, with no drift. **NOT RUN:** Node CLI on an actual checkout, the new validator CLI, Unity/C# compile or integration tests; do not infer green CI from source inspection.
- **Review request to Chat 02:** confirm `E03_mentor_paid` and `E07_delivered` fact semantics and whether `partner/{sourceInstanceId}/delivery` uniquely denotes a delivered partner occurrence under ADR-C0-002 (PR #17). E09 remains display-only; no ledger posting at signature.
- **Blockers:** executable Core content adapter and receipt/ledger transaction still pending. Do not merge into `main` without owner approval.


## AutoDev 04 — ADR-C0-002 event-hook metadata alignment (15.11.2026, 00:55 +04)

**One bounded task (Issue #7):** align the existing *non-executable* event-hook instance contract metadata with the coordinator's ADR-C0-002 in Draft PR #17. No gameplay values, Definition IDs, event DayIndex, conditional revenue, or narrative changed.

- `Content/EVENT_HOOKS_DRAFT.json`: replaced outdated N0/C0 blocker text with explicit Core-owned stable `instanceId` = `StableEventId`, signed Int32 priority persisted on scheduling (default 0 assigned once), due sort (`dueDayIndex ASC, priority DESC, instanceId ASC` ordinal), snapshot after project ticks, atomic rejection for a newly scheduled child with `dueDayIndex <= closing n`, and durable receipt/posting ownership. Existing overdue instances remain eligible. E09 remains presentation-only.
- `Content/validate-event-hooks.cjs`: added static ADR metadata guards and four negative metadata drift mutations (queue comparator, priority, child due date policy, StableEventId semantics). This is *not* a scheduler implementation or a Core contract test.
- Code commits: `ed4bc8f149a83c794a14c42dd3dd1116486500ca` (hook metadata), `2113ab3edbaebed1f0c3d612666554c9933acc00` (validator).
- **Actually verified:** fetched both new GitHub blobs and parsed hook JSON; static assertions in isolated JS tool runtime passed for 8 required metadata fields, comparator/default/snapshot/child semantics, E09 report-only, and two conditional-income definitions. **NOT RUN:** `node Content/validate-event-hooks.cjs` in checkout, negative cases through Node CLI, C# compilation, Unity tests, or end-to-end scheduler. Do not interpret static inspection as a green Node run.
- **Remaining BLOCKED integration:** Core's complete event instance DTO and command transaction API, NPC adapter and verified C#/Unity test environment. ADR-C0-002 is approved for feature work only; PR #17 is not merged. Ask NPC 02 to review `E03_mentor_paid` / `E07_delivered` typed fact IDs and `partner/{sourceInstanceId}/delivery` posting identity before runtime implementation.
- **Next content step:** once NPC 02 responds, resolve any concrete fact-ID drift with a bounded fixture/validator change; otherwise expand cross-file consistency tests without altering Core or the approved ordering. PR #11 stays Draft; no merge/main writes.


## AutoDev 04 — typed AST cross-file guard (15.11.2026, 02:43 +04)

**One bounded task (Issue #7):** strengthen the existing cross-file content drift validator to check exact typed predicate nodes, not incidental substrings in serialized condition JSON. This addresses the NPC 02 review that `E03_mentor_paid` / `E07_delivered` must be durable typed facts, not free-text matches. No change to event fixtures, storyline, amounts, DayIndex, Core, NPC or EventDirector.

- File: `Content/validate-content-consistency.cjs`. Replaced `JSON.stringify(condition).includes(...)` checks with traversal of `all`/`any` groups and exact `factExists.factId` and `flagEquals.key/value` matching.
- Added two deterministic negative mutations: near-match `E07_delivered_fake` must fail, and correct `remote` flag value under wrong key `OTHER_PARTNER` must fail. Updated expected check count from 5 to 7.
- Code commit: `90be31ae93260efef9ca976278a90b0fbeacdb12`; handoff commit follows this entry.
- **Actually verified:** fetched the latest committed Content and Hook JSON through GitHub; isolated JS assertions for both partner modes confirm both exact fact IDs, correct PARTNER flag key/value, and rejection of similar fact ID / wrong flag key (2 branches x 4 checks). This is **not** execution of the full Node CLI script.
- **NOT RUN:** `node Content/validate-content-consistency.cjs` in a real checkout, other Node validators, C# or Unity compilation, EventDirector integration. QA 05 should run the real CLI on the feature branch and report logs.
- **BLOCKED for runtime integration:** typed fact emission/provenance and canonical agreement source instance remain Core/NPC-owned, as stated by NPC 02 review in PR #11. ADR-C0-002 PR #17 permits feature work only, not main merge.
- **Next:** review QA test logs and NPC 02's concrete fact-emission adapter when available. PR #11 stays Draft; no main changes or merges.


## AutoDev 04 — partner revenue OR-bypass regression guard (15.11.2026, 04:22 +04)

**One bounded Issue #7 task:** strengthen `Content/validate-content-consistency.cjs` so required partner facts and branch flag must hold on **every successful AST path**. The previous traversal accepted a required `factExists` or `flagEquals` anywhere beneath `any`, even if another OR branch was `always`; that could let a future adapter propose unearned revenue. New `guaranteedLeaves` computes conjunction union and disjunction intersection of mandatory exact typed leaves. Two negative mutations check bypassable delivery and partner-branch conditions. Test count 7→9. No changes to narrative, amounts, DayIndex, runtime, or hook JSON.

**Code commit:** `cb45cf175fc1b7db7feff882281b275a169074fe` in `feature/content-7`. **Actually verified:** 6 isolated JavaScript assertions on live GitHub hook JSON (both onsite/remote: baseline required facts/flags; bypassable delivery fact rejected; bypassable branch flag rejected). This independently exercises the same pure predicate guarantee logic, **not** the complete Node CLI validator. **NOT RUN:** `node Content/validate-content-consistency.cjs` from checkout (GitHub DNS unavailable in container), C#/Unity tests or EventDirector integration. QA 05 should run Node CLI on actual branch and inspect 9 checks.

**BLOCKED integration:** Core/NPC still own canonical durable fact emission/provenance and agreement source instance; content validator is only static draft guard. ADR-C0-002 PR #17 permits feature work only; no merge/main changes.


## AutoDev 04 — duplicate Definition ID guard (15.11.2026, 06:03 +04)

**One bounded task (Issue #7):** prevent silent Map overwrite when `CONTENT_SPEC_DRAFT.json.eventManifest[].id` or `EVENT_HOOKS_DRAFT.json.definitions[].definitionId` repeats. `Content/validate-content-consistency.cjs` now rejects duplicate or blank IDs before Map lookup; two negative fixtures duplicate identical E02 and E09 records, which previously would have been accepted. Existing story, monetary Int64 strings, DayIndex, E09 report-only behavior and event ordering are unchanged. This is static content validation only, not an EventDirector.

**Code commit:** `19e5bb5060d234abe743a2e2a3d5fb29e69ae76d`. Expected full CLI output now `PASS: 11 cross-file fixture/negative checks` if all pass.

**Actually verified:** fresh GitHub reads of committed source and both JSON fixtures; five isolated JavaScript assertions passed: baseline unique manifest and definitions, duplicated E02 and E09 detection, and presence of both negative fixtures in committed source. **NOT RUN:** full `node Content/validate-content-consistency.cjs` from checkout, C#/Unity, runtime integration; no green CLI claim. QA 05 should execute the real Node CLI and attach the log/run ID.

**Integration BLOCKED:** concrete Core/NPC fact provenance, stable agreement source instance and transactional EventDirector API still pending. NPC 02 should review typed fact IDs `E03_mentor_paid`, `E07_delivered` and earned identity `partner/{sourceInstanceId}/delivery`. ADR-C0-002 PR #17 is feature-work guidance only. No merge/main changes.


## AutoDev 04 — full manifest calendar drift guard (15.11.2026, 07:45 +04)

**One bounded Issue #7 task:** previous cross-file validator checked `dayIndex === day-1` only for E02/E03/E09. The remaining E04–E14 could drift silently from the legacy 1-based day field. Added a global `eventManifest` check in `Content/validate-content-consistency.cjs`: both `day` and `dayIndex` must be safe integers, `dayIndex >= 0`, `day === dayIndex + 1`. Two negative mutations exercise E14 calendar drift and fractional E10 dayIndex. Expected fixture count is now 13 (baseline + 12 negatives). No content values, fictional narrative, money, Core, NPC or scheduler changed.

**Code commit:** `5a7bd6c73afbf32f1dc95d561360687400cbfb51` on `feature/content-7`.

**Actually verified:** fresh GitHub reads of committed validator and spec; independently executed 3 JavaScript checks against live spec: baseline 13 entries pass, E14 mutation rejected, E10 fractional mutation rejected. This independent check replicates the new calendar predicate; it is **not** a full execution of the Node CLI. **NOT RUN:** `node Content/validate-content-consistency.cjs` from a checkout, all 13 script assertions together, C# / Unity tests. QA 05: run the exact Node command on this branch and record the Actions run ID or logs.

**BLOCKED for runtime integration:** still need Core/NPC event-instance, fact provenance and transactional earned-income contracts. ADR-C0-002 PR #17 is approved for feature work only. No merge/main changes. Next bounded work should be based on a real QA result or a concrete NPC fact-ID review, not another speculative runtime adapter.


## AutoDev 04 — feed reference and temporal drift guard (15.11.2026, 09:23 +04)

**One bounded Issue #7 task:** extend `Content/validate-content-consistency.cjs` to reject unknown `eventManifest[].feeds` targets, backward links to earlier-day events, self-links, duplicate feeds and invalid feed arrays/IDs. Targets resolve against known event IDs, existing project IDs (P01/P02), or the explicitly deferred `CHAPTER2_HOOK` marker. This is a *static narrative dependency check*, not an EventDirector or same-day execution scheduler. Added 3 negative fixtures: unknown E14 feed, backward E07→E02, duplicate E10→P02. Expected CLI total: **16** checks (13 existing + 3 new). Existing fictional story, money Int64 strings, event schedule and ledger policy unchanged.

**Code commit:** `9439c541e41c2c7fb63b88e61f5d955a79ec62aa` on `feature/content-7`.

**Actually verified:** fetched current GitHub `CONTENT_SPEC_DRAFT.json` and both related source files; executed four isolated JavaScript assertions in the GitHub-connected runtime: baseline feed graph has zero violations, unknown feed is rejected, backward feed is rejected, duplicate project feed is rejected. **NOT RUN:** the complete Node CLI from a real checkout, all 16 fixtures together, C#/Unity tests. QA 05: run `node Content/validate-content-consistency.cjs` on this branch and record CI logs; do not claim green until run is observed.

**BLOCKED for runtime integration:** Core/NPC still own actual event instance IDs, fact provenance, agreement source instance and atomic earned income. ADR-C0-002 PR #17 is approved for feature work only. No merge or `main` changes.


## AutoDev 04 — cyclic narrative feed guard (15.11.2026, 10:57 +04)

**One bounded Issue #7 task:** extend `Content/validate-content-consistency.cjs` to reject event-to-event cycles, including cycles entirely within one `DayIndex`. Previous checks covered unknown, duplicate, self and backward references, but allowed E02→E03→E02 because both events are on dayIndex=1. The new deterministic DFS visits only event targets, treats project IDs P01/P02 and `CHAPTER2_HOOK` as non-event terminal metadata, and reports `eventManifest: cyclic feed`. This validates narrative dependency metadata only; it is **not** EventDirector or a runtime scheduler.

- Code commits: `bd406649d8d355368e040d9b8367c8151319ad56` (guard + two negative fixtures), `b61442c2be138e8bf48c2aefa8c9a84ffc956f9e` (fix E02/E03 fixture to add both directions explicitly). Total cross-file checks declared in script: 18 (previously 16).
- **Actually verified:** read current source and content JSON via GitHub; independently executed 5 isolated JavaScript checks in the tool runtime: baseline event graph acyclic; E02↔E03 same-day cycle detected; E04↔E05 same-day cycle detected; committed source contains cycle guard; committed source declares 18 fixtures. These checks passed 5/5. An initial attempted test fixture lacked the reverse edge, was corrected and rechecked before handoff.
- **NOT RUN:** full `node Content/validate-content-consistency.cjs` from a checkout, full 18-case suite, C# or Unity tests. QA 05 should run Node CLI on `feature/content-7` and attach actual logs.
- **Unchanged:** scenario, amounts (Int64 decimal string kopecks), event dates (0-based DayIndex), stable Definition IDs, E09 report-only, Core/NPC code and main. Runtime integration still BLOCKED on Core/NPC event-instance/fact provenance contracts; ADR-C0-002 PR #17 only authorizes feature work. NPC 02 review of `E03_mentor_paid`, `E07_delivered`, and `partner/{sourceInstanceId}/delivery` remains requested.
- **Next:** review real QA Node logs and NPC feedback; no merge without owner approval.


## AutoDev 04 — unique choice definition IDs (15.11.2026, 12:37 +04)

**One bounded Issue #7 task:** `Content/validate-content-consistency.cjs` now checks stable, nonblank choice IDs across all Day 01 visit choices and E02–E14 event manifest choices. Duplicate IDs across visits/events would make choice references and save data ambiguous; no Core runtime choice/instance contract is introduced. Three negative fixtures added (cross-event duplicate, cross-visit duplicate, blank ID). Declared CLI suite: **21** checks (18 prior + 3 new).

- **Code commit:** `311205ae87ed7f53165784f76321036ae2dc7c23` on `feature/content-7`.
- **Actually verified:** fetched current GitHub content JSON and independently ran 4 JavaScript assertions in the connected tool runtime: 61 baseline choice IDs unique/nonblank, both duplicate mutations rejected, blank ID rejected (4/4). This is **not** a full execution of the committed Node CLI.
- **NOT RUN:** `node Content/validate-content-consistency.cjs` from checkout (container cannot resolve github.com), complete 21-case suite, C#/Unity and runtime integration.
- **Unchanged:** fictional scenario, Int64 decimal string kopecks, DayIndex 0-based, E09 report-only, ADR-C0-002 ordering, Core/NPC code, main. No merge.
- **BLOCKED runtime integration:** Core/NPC fact provenance, sourceInstanceId and atomic ledger posting contracts; NPC 02 review of typed `E03_mentor_paid`, `E07_delivered`, and `partner/{sourceInstanceId}/delivery` remains outstanding. QA 05: run all three Content Node validators from actual feature checkout and attach logs.


## AutoDev 04 — project definition ID guard (15.11.2026)

**Issue #7, one bounded task:** static cross-file validation now rejects blank or duplicate project IDs and collisions between project and event definition namespaces before resolving narrative feeds. Added three negative fixtures (duplicate P01, blank ID, project E02 collision); declared cross-file suite increases from 21 to 24 assertions. This does not schedule events or change gameplay, story, balance, Int64 kopeck amounts, 0-based DayIndex, or E09 report-only behavior.

**Code commit:** `cc1df7d75d0a9f6408a894b454ac64b8684c41c0` on `feature/content-7`. **Actually executed:** four independent JS baseline/mutation checks in the GitHub-connected tool runtime (2 distinct project IDs and three rejected mutations). **NOT RUN:** committed Node CLI full 24 assertions, Unity/C# or integration; QA 05 must execute `node Content/validate-content-consistency.cjs` from a real checkout and attach logs. NPC 02 review still requested for typed fact provenance `E03_mentor_paid` / `E07_delivered` and earned identity `partner/{sourceInstanceId}/delivery`. Runtime integration BLOCKED on Core/NPC contracts; no main writes or merge.


## AutoDev 04 — Day 01 visit ID uniqueness (15.11.2026)

**Issue #7, one bounded task:** cross-file validator now rejects blank or duplicate `day1.visits[].id` identifiers. Two negative fixtures duplicate the first visit and blank the first visit ID. Expected CLI suite now **26** checks (24 previous + 2 new). No story, balance, money, DayIndex, E09 or runtime changes.

**Code commit:** `a54c32a935709766dd27114adc694d2f06414c2c` on `feature/content-7`. **Actually verified:** four isolated JavaScript checks in the GitHub-connected runtime: baseline visit IDs valid, duplicate mutation rejected, blank mutation rejected, committed source declares 26 checks. **NOT RUN:** full Node CLI from checkout (container github.com DNS unavailable), Unity/C#, integration. QA 05 should run `node Content/validate-content-consistency.cjs` on branch and attach logs. NPC 02 typed fact provenance review remains outstanding. Runtime integration BLOCKED on Core/NPC contracts; no merge/main changes.


## AutoDev 04 — 2026-11-15: Day 01 document reference integrity

- Scope: one bounded change to `Content/validate-content-consistency.cjs` in `feature/content-7`; reject blank/duplicate `documentTypes[].id` and missing `day1.visits[].documentId` references. Three negative fixtures; declared cross-file CLI suite **26 → 29**.
- Code commit: `433b5556a0f4a8a93bf34054ec61a3245735a5e3`.
- Actually executed: **4/4 isolated JavaScript logical checks** against fresh GitHub content (baseline 3 documents/3 visits, duplicate document ID, blank ID, unresolved visit document reference). These checks mirror the new guard; **full committed Node CLI/29 cases NOT RUN**. C#/Unity/runtime integration NOT RUN.
- QA 05: run `node Content/validate-content-consistency.cjs` on the feature checkout and attach output. NPC 02: review typed facts `E03_mentor_paid`, `E07_delivered` and `partner/{sourceInstanceId}/delivery` provenance before adapter implementation.
- No narrative, financial values, Definition IDs, DayIndex, E09 report-only, Core or EventDirector modifications. Draft only; no main write or merge. Runtime integration remains blocked on Core/NPC contracts.


## AutoDev 04 — 2026-11-15: malformed Day 01 choice input guard

- **One bounded task (#7):** prevent validator crash when `day1.visits[].choices` is null/not an array or contains null entries. Invalid arrays now produce `choices must be an array`; null entries produce `invalid choice id`. No change to valid content, gameplay, money, IDs or narrative.
- **Code commit:** `04c2c5e97e433f6a8100f2aba28cc09c076b19ef` in `feature/content-7`. Added two negative fixtures; declared cross-file suite **29 → 31**.
- **Verification:** GitHub connector confirmed the commit SHA. Full Node CLI **NOT RUN** (container `git ls-remote` failed: `Could not resolve host: github.com`); the 31 fixtures are **not claimed passing**. Unity/C# NOT RUN. QA 05 should run `node Content/validate-content-consistency.cjs` from checkout and attach logs.
- **NPC 02 review remains requested:** typed fact IDs `E03_mentor_paid`, `E07_delivered`, earned provenance `partner/{sourceInstanceId}/delivery`. Runtime integration BLOCKED on Core/NPC instance/fact/ledger contracts. E09 report-only and ADR-C0-002 preserved. No main push/merge.


## AutoDev 04 — 2026-11-15: money-wire negative coverage (#7)

- One bounded task: expanded `Content/validate-money-wire.cjs` with five negative mutations (null, boolean, whitespace, decimal fraction and wrong JSON number in `opening.weeklyRevenueKopecks`) plus a positive exact Int64 decimal string above JavaScript's safe-integer range. No amounts, narrative, IDs, DayIndex or E09 behavior changed.
- Code commit: `0fa5313d91d0b2d1c27cdabded172706bb74afe0` on `feature/content-7`. Declared checks: 21 existing money paths, 13 negative mutations, 4 positive boundary assertions.
- **NOT RUN:** full committed Node CLI, Unity/C#, runtime integration. GitHub connector confirmed file update but not test execution. QA 05: run `node Content/validate-money-wire.cjs` and `node Content/validate-content-consistency.cjs` on checkout and attach output.
- NPC 02: review typed fact IDs `E03_mentor_paid`, `E07_delivered` and provenance `partner/{sourceInstanceId}/delivery` before runtime integration. ADR-C0-002 feature-only; E09 report-only. No main write or merge.


## AutoDev 04 — 2026-11-15: nested money-wire rejection (#7)

- **One bounded task:** add three negative mutations for nested financial fields in `Content/validate-money-wire.cjs`: `documentTypes[0].visible.amountKopecks` as JSON number, `documentTypes[2].visible.conditionalRevenueKopecks` as exponent string, and `day1.visits[0].choices[0].effects[0].deltaKopecks` as noncanonical signed string. Expected suite count: **16 negative mutations, 4 positive assertions, 21 financial fields**.
- **Code commit:** `2b6d683e2e20be002105cdf63aeef0662c4a832b` in `feature/content-7`. This is a test-only change; no story, amounts, DayIndex, Definition IDs, E09 report-only or Core/NPC runtime changes.
- **Actual verification:** GitHub confirmed the code commit and source update; **full Node CLI NOT RUN**, no assertion of 16 passing cases. Unity/C# NOT RUN. QA 05: execute `node Content/validate-money-wire.cjs` from branch checkout and provide logs.
- **NPC 02 review request remains:** typed `E03_mentor_paid`, `E07_delivered` and earned provenance `partner/{sourceInstanceId}/delivery` require confirmation before runtime adapter. ADR-C0-002 is feature-only. Runtime integration BLOCKED on Core/NPC contracts. No main writes or merge.


## P0 Issue #18 — canonical Day01 UI fixture (16.11.2026)

- **One deliverable:** `Content/DAY01_UI_FIXTURE.json`, content commit `5069cb9293d64ece7d6bebea0f3a9d82a1630f41` in `feature/content-7`. Consumer: Chat 03, PR #12, for offline browser prototype; no Core/EventDirector implementation.
- Derived from `CONTENT_SPEC_DRAFT.json` without balance changes: 3 stable visit IDs (D01_LAB, D01_STUDENT, D01_PARTNER), 3 DOC IDs (DOC-01/02/03), 9 choice IDs, two inspection tokens, disclosed document facts, Q/S/M/E/R immediate effects, Int64 decimal-string kopecks, opening 200000000 kopecks, DayIndex 0. All room claims are *requests* for fictional Л-204 on DayIndex 3 (Day 04), 14:00–17:00; no implicit reservation.
- Partner signing changes E score only; deferred 12000000/9000000 kopecks are conditional on delivered service and never immediate cash. E09 remains report-only. UI report must show actual chosen cash deltas, five scores, zero weekly settlement on DayIndex 0, and no earned partner income.
- **Actually checked:** in connector JavaScript, source JSON parsed, fixture has 3 documents, 3 visitors, 3 choices each, distinct visit IDs, all document references resolve, 18 `*Kopecks` fields validate as signed Int64 decimal strings, no partner choice has an immediate cash effect. GitHub confirmed content commit. **NOT RUN:** Node CLI validator from checkout, browser smoke tests, CI, Unity/C#, Core integration. This is mock fixture data, not proof of playable demo.
- **Coordination:** Chat 03 should consume this exact fixture or mirror its IDs/effects in offline bundle; Chat 05 must execute `node Content/validate-money-wire.cjs`, `node Content/validate-content-consistency.cjs`, `node Content/validate-event-hooks.cjs` and test UI interactions. Chat 02 must confirm typed fact producers `E03_mentor_paid` / `E07_delivered` and `partner/{sourceInstanceId}/delivery` earned provenance; fixture does not manufacture facts. No main merge.

## P0 Issue #18 — NPC 02 typed-fact provenance handoff (16.11.2026)

**Scope:** one content-contract clarification for the existing `DAY01_UI_FIXTURE.json`; no runtime implementation, new gameplay, or monetary change. Response to NPC 02's PR #11 comment (2026-11-15 20:38 UTC).

| Typed fact ID | Only permissible producer condition | Not a producer |
| --- | --- | --- |
| `E03_mentor_paid` | Core-confirmed, durable payment/ledger receipt for the mentor obligation associated with the **same partner agreement occurrence** | UI click, signature, inspection, NPC dialogue, merely scheduling E03 |
| `E07_delivered` | Core-confirmed completion/delivery of the contracted partner service for the **same agreement occurrence** | Agreement signature, visit choice, scheduled E07, unverified narrative flag |

**Identity:** `sourceInstanceId` in `partner/{sourceInstanceId}/delivery` is the stable **Core-assigned original partner agreement occurrence ID**, not `D01_PARTNER` (visit ID), `PARTNER-1/2` (choice ID), `E07` (definition ID), or an event instance for a later step. Both facts must be bound to that same source occurrence. `partner/{sourceInstanceId}/delivery` is a proposed durable earned/deduplication key for a **single** eligible delivery, with Core owning atomic posting and receipt uniqueness. Never infer earned income from either fact alone, a UI signature, or an E09 report.

**Mock/UI acceptance:** signing `PARTNER-1` or `PARTNER-2` changes E score only; `signingCashDeltaKopecks="0"`, `endDayReport.partnerEarnedIncomeKopecks="0"` at DayIndex 0. Deferred nominal amounts stay `"12000000"`/`"9000000"` Int64 decimal strings; no automatic revenue. `E09` remains report-only. Content does not mint typed facts; NPC 02 may define fact shapes, while Core must verify receipts/delivery and own earned ledger effects. Producer API and occurrence correlation are **proposed, not yet integrated or end-to-end tested**.

**Verification (source inspection, not runtime):** the fixture currently has 3 documents, 3 visitors, 9 choices; both partner options have no immediate `cashDelta`, and each has `signingCashDeltaKopecks="0"`; report earned income is `"0"`. **NOT RUN:** Node CLI, CI, browser, Unity, Core/NPC integration. QA 05 must run all Content validators and UI smoke tests. No `main` push or merge.

## P0 Issue #18 — deterministic triple-claim report oracle (16.11.2026)

- Added `acceptanceExample` to `Content/DAY01_UI_FIXTURE.json` for UI 03 / QA 05, **not** a runtime command or new balance rule. Inputs: DayIndex 0, no inspections, choices `LAB-1`, `STUDENT-1`, `PARTNER-1`, explicit `ROOM-4` unresolved (all three request fictional L-204, no booking).
- Expected report: starting `200000000` kopecks, exactly two immediate ledger entries (`-22000000`, `-6500000`), closing `171500000` kopecks; Q=55, S=51, M=52, E=47, R=48. No partner earned income (`"0"`), no weekly settlement, next definition `E02`. All money fields are decimal strings.
- **Actually executed:** six connector-JavaScript assertions over parsed fixture: 3/3 docs/visits, BigInt closing budget, five scores, exact two ledger entries / string format, explicit no-booking conflict, zero partner signature cash — **6/6 PASS**. **NOT RUN:** checkout Node CLI validators, browser/CI/Unity, Core integration; QA 05 must execute independent verification on committed SHA.
- Fixture commit: `507ca8f6cb8d6c0d3222549342310f1ee4bd1c50`. Scope limited to Content; no Core/EventDirector, scenario/balance changes, main write or merge. NPC 02 typed fact producer API remains pending.

## P0 Issue #18 — divergent inspected Day01 acceptance oracle (16.11.2026)

- `DAY01_UI_FIXTURE.json` now has `acceptanceExamples` array with the original triple-claim scenario and a second inspected alternative. UI 03 / QA 05 should run **both**: LAB-2 + STUDENT-2 + PARTNER-2, inspect DOC-01 and DOC-03 (2 tokens), ROOM-AUTO (no booking), ending 180500000 kopecks; Q=54, S=49, M=51, E=46, R=47. Two immediate cash entries: -16000000 and -3500000. No partner earned income at signature; E09 report-only, DayIndex=0.
- **Actual verification:** 2/2 connector-JavaScript oracle recalculations PASS using BigInt and independent score/ledger effect accumulation. **NOT RUN:** Node CLI, GitHub CI, browser, Unity. This is mock data, not proof of playable demo. UI 03 must adapt from singular `acceptanceExample` to plural `acceptanceExamples`; QA 05 must execute validators and browser smoke.
- Content-only; no Core/EventDirector edits, no balancing or `main` merge.


## P0 Issue #18 — QA money-wire diagnostic path unblock (16.11.2026)

**Scope:** fix the concrete QA 05 failure reported in PR #11 (Actions run 43772020964, cross-branch job 132989808026); no further money-wire negative cases, no story/balance/fixture/Core/EventDirector edits.

- `Content/validate-money-wire.cjs`: `failsAt()` now constructs the same JSON-pointer-like path used by `inspect()`: dot-separated object properties and bracketed array indices (for example `$.documentTypes[0].visible.amountKopecks`). The previous `path.join('.')` expectation incorrectly rejected the valid diagnostic `$.documentTypes[0]...` even when the money mutation itself was correctly rejected. Exact `startsWith(pointer + ':')` also avoids matching unrelated paths.
- Code commit: `5822ceb55a6d2c73f517fc57965af5fc2ede6c33` on `feature/content-7`.
- **Actually verified:** source replacement and nested pointer construction in isolated connector JavaScript; expected `$.day1.visits[0].choices[0].effects[0].deltaKopecks` matched. **NOT RUN:** complete Node CLI `node Content/validate-money-wire.cjs`, CI rerun, Unity, browser smoke. This is a source-level fix, not evidence that QA gate passed.
- **Next P0 gate:** QA 05 rerun all Content validators against this exact Content SHA and verify offline UI Day01 EndDay/report acceptance oracles; UI 03 to verify `acceptanceExamples`; NPC 02 to confirm typed fact producers and earned provenance. Status remains **BLOCKED / NOT READY_FOR_OWNER_TEST** until independent execution. No main push or merge.


## P0 Issue #18 — cross-role acceptance reconciliation (16.11.2026)

**One bounded task:** reconcile the current canonical Content acceptance oracles with the latest NPC 02 and UI 03 PR handoffs, without editing other roles' code or changing balance.

- NPC 02 explicitly acknowledged the Content fact/earned provenance in PR #16 (comment 6902336394): `E03_mentor_paid` only from Core-confirmed mentor payment; `E07_delivered` only from Core-confirmed delivery, both tied to the same original agreement occurrence; `partner/{sourceInstanceId}/delivery` uses that Core occurrence ID. This is **documented agreement, not an implemented/verified receipt API**. The NPC choice lookup was subsequently extended (PR #16 comment 6906388324), but the EventDirector and Core receipt integration remain blocked.
- UI 03 has an offline playable HTML artifact and successful own-branch CI for UI SHA `cc44bf037f2aad91c41718ec2f3f336b7a0cd464` (PR #12 comment 6903812592), but no independently verified Windows interactive launch or proof that its EndDay report matches the **two** Content `acceptanceExamples`. Its separate read-only model still advertises `content-draft-0.2`; this is a display-only legacy snapshot, **not** proof that the playable gateway consumes the current fixture.
- **Actually executed this pass:** fetched `Content/DAY01_UI_FIXTURE.json` from `feature/content-7`, independently recomputed both acceptance examples from chosen effects using JavaScript BigInt; **2/2 PASS** for closing budgets, Q/S/M/E/R, exact immediate postings, room-claim resolution mode and zero earned partner income. This is an isolated data-oracle check, not execution of the browser game.
- **NOT RUN:** committed Node CLI validators, cross-branch Actions rerun after money-wire fix `5822ceb55a6d2c73f517fc57965af5fc2ede6c33`, Windows double-click/browser EndDay smoke, Unity. QA 05 should pin Content HEAD plus UI HEAD, run all three Content validators and both playable EndDay scenarios, verify artifact contents and report PASS/FAIL with Actions URL. UI 03 should compare its actual gateway output with both `acceptanceExamples`; do not silently assume the read-only preview equals the playable implementation.
- **Status:** Content acceptance data internally consistent; overall Issue #18 remains **BLOCKED / NOT READY_FOR_OWNER_TEST** pending independent QA and real interactive verification. No Core, EventDirector, NPC, UI, fixture, monetary amount, story, `main`, or merge changes.


## P0 Issue #18 — executable acceptance oracle (16.11.2026)

- Added `Content/validate-day01-acceptance.cjs`: standalone Node built-in-only fixture validator. Run `node Content/validate-day01-acceptance.cjs` from checkout; verifies both canonical `acceptanceExamples`, three stable visits/docs, inspection requirements, exact BigInt financial postings and Q/S/M/E/R, L-204 no-booking and zero partner signing income. No fixture/balance/story changes.
- **Verified in this pass:** independently recomputed both fixture examples in connector JavaScript: 2/2 PASS (budgets 171500000 and 180500000 kopecks, scores, postings). **NOT RUN:** newly committed Node CLI script, complete Content validator suite, browser/Unity/CI. QA 05 must run the script from checkout plus existing validators and provide exact run SHA/artifact. This is not a browser EndDay pass.
- Remains **BLOCKED / NOT READY_FOR_OWNER_TEST** pending independent QA and UI 03 integration evidence; NPC 02/Core receipt producer remains unverified.


## P0 #18 — verified UI gateway vs canonical Content divergence (16.11.2026)

**One bounded review task:** compared current `feature/ui-5/prototypes/ui-day-one/day01-gameplay-gateway.cjs` and `test-day01-gameplay.cjs` with `feature/content-7/Content/DAY01_UI_FIXTURE.json` (read-only GitHub source inspection). This is a **confirmed source-level contract mismatch**, not a browser run.

- **Canonical triple claim** `LAB-1 + STUDENT-1 + PARTNER-1`: Content requires **171500000 kopecks**, **two** immediate ledger postings (LAB -22000000, STUDENT -6500000), scores **Q55/S51/M52/E47/R48**, and **ROOM-4** unresolved/no booking. UI gateway `full/full/onsite` instead charges **4000000** immediately for the partner, asserts **167500000** and **three** ledger postings; its score deltas also differ. UI does not expose the required explicit ROOM-4 decision.
- **Canonical inspected alternative** `LAB-2 + STUDENT-2 + PARTNER-2`: Content requires **180500000 kopecks**, **two** inspections (`DOC-01`, `DOC-03`), and **two** immediate postings. UI's `phase/mobile/remote` costs 16000000 + 3500000 + 2500000 = **22000000** (closing **178000000**), and UI test inspects all **three** docs without a two-token cap. UI also incorrectly requires DOC-02 inspection for mobile, while Content `STUDENT-2` does not.
- This is **not** a request to change game balance or Core. UI 03 owns the mock gateway; it should consume/map canonical Content IDs and choices, use exact Int64 kopeck strings, respect the two-token inspection budget, preserve partner signing income **zero**, show room conflict resolution explicitly and match **both** `acceptanceExamples`. QA 05 should add cross-branch oracle assertions against the actual gateway rather than treating independent green tests as content compatibility.
- **Actually checked:** GitHub file contents and arithmetic of both selected paths; expected 200000000-22000000-6500000=171500000 vs UI 200000000-22000000-6500000-4000000=167500000; expected 200000000-16000000-3500000=180500000 vs UI 200000000-16000000-3500000-2500000=178000000. **NOT RUN:** Node CLI, browser, CI, Unity. This finding remains a P0 blocker for canonical Day01 acceptance, even if UI's own tests and QA's pinned tests pass.
- **Ownership:** no edits to UI, NPC, Core, EventDirector, fixture or balance. Content handoff only; escalate to UI 03 and QA 05 via PR #11. **BLOCKED / NOT READY_FOR_OWNER_TEST** until same-SHA playable output is independently compared to both Content acceptance examples.


## P0 #18 — room resolution acceptance guard (16.11.2026)

- Strengthened `Content/validate-day01-acceptance.cjs`: a multi-claim fixture marked `unresolved-no-booking` must explicitly select `ROOM-4`; other priority IDs cannot silently masquerade as unresolved. This is a fixture validation guard, **not** runtime room allocation or a balance change.
- Connector-JS fixture check: canonical triple-claim ROOM-4 **PASS**; mutation replacing it with ROOM-1 **REJECTED**. The full Node CLI, cross-branch UI smoke, CI, Unity were **not run** in this pass.
- P0 still **BLOCKED / NOT READY_FOR_OWNER_TEST**: UI 03 must align playable gateway with canonical budgets, inspection cap and room choice; QA 05 must execute Content validators and compare both browser EndDay outcomes. No main merge.


## P0 #18 — QA green pin versus current Content acceptance (16.11.2026)

- **Verified GitHub evidence:** QA 05 reports Actions [#43824110415](https://github.com/rixed318/Probably-TAU/actions/runs/43824110415) as 4/4 jobs successful with exact pinned Content `c2d9ba26` and UI `8951e40e` (PR #10 comment [#6914206809](https://github.com/rixed318/Probably-TAU/pull/10#issuecomment-6914206809)). This is a **pinned baseline pass**, not validation of current Content HEAD `a241569d` or cross-branch fixture-to-UI gameplay parity. Do not conflate the successful independent UI tests with the two Content acceptance examples.
- **P0 owner-visible gap remains:** Content fixture expects LAB-1/STUDENT-1/PARTNER-1 = `171500000` kopecks, LAB-2/STUDENT-2/PARTNER-2 = `180500000`; UI mock gateway previously returned `167500000` / `178000000` with different signature/inspection/room semantics. No evidence of a later UI gameplay gateway fix (PR #12 still at `8951e40e`). The content validator `Content/validate-day01-acceptance.cjs` validates the fixture oracle, **not the UI gateway**.
- **Next accountable action:** UI 03 must reconcile gameplay gateway with both fixture oracles (including exactly two inspections, explicit ROOM-4 for unresolved L-204, zero partner earned income at signature); QA 05 must execute exact-SHA cross-branch comparison against UI behavior and offline extracted artifact. Content 04 will not edit UI/Core/EventDirector or change balancing to hide mismatch. Gate remains **BLOCKED / NOT READY_FOR_OWNER_TEST**.
- **Verification in this pass:** fetched GitHub main rules/workflow, Issues #1/#7/#18, PR #9/#11/#12/#14/#17, current Content acceptance validator, and latest QA/UI PR comments. Static evidence reconciliation only; **NOT RUN:** Node CLI, browser, Unity, CI on new HEAD. No main write or merge.


## P0 #18 — QA offline dependency-closure checkpoint (16.11.2026)

- **New GitHub evidence:** QA 05 PR #10 comment [#6916540830](https://github.com/rixed318/Probably-TAU/pull/10#issuecomment-6916540830) reports static offline HTML script/dependency closure checks added to `.github/workflows/qa-offline.yml` at QA commit `e84deb7499450127fb1922508b225b698feb6fd8`, followed by QA handoff commit `d07f32e605c0c403e7d9413c1f7724cd0d1070da`. **The new CI was NOT verified by QA at publication**, so previous [4/4 PASS #43824110415](https://github.com/rixed318/Probably-TAU/actions/runs/43824110415) cannot be used as proof of this added check.
- **Content acceptance remains blocked:** QA pinned Content `c2d9ba26`, while Content has newer acceptance validator and canonical examples; QA's static HTML dependency check does not compare the actual UI gateway EndDay outputs against both canonical `acceptanceExamples`. The previously identified UI budget/inspection/ROOM-4 discrepancies remain unresolved in the available evidence. UI 03 owns gateway changes; QA 05 owns exact-SHA cross-branch parity, money-wire validator execution, and extracted offline browser smoke.
- **This pass verification:** GitHub read of current main `AGENTS.md`, workflow, Issues #1/#7/#18, PR #9/#11/#12/#14, PR #10 QA comments, UI handoff and Content acceptance validator. No local Node CLI, browser, Unity, CI or ZIP execution performed. No fixture/balance/Core/EventDirector/UI changes.
- **Gate:** **BLOCKED / NOT READY_FOR_OWNER_TEST** until latest Content+UI exact-SHA oracle parity, independent QA validation and interactive offline demo are evidenced. No main write or merge.

## P0 #18 — deterministic cross-role reproduction protocol (16.11.2026)

**One bounded task:** publish an executable *test protocol* for UI 03 / QA 05 without altering balance or claiming browser verification. The latest PR #12 discussion still does not demonstrate parity against the two canonical `acceptanceExamples`. The latest PR #10 QA report confirms only the older pinned baseline and static dependency closure, not actual gateway parity.

1. Pin **both** exact Content and UI head SHAs in QA evidence; read `Content/DAY01_UI_FIXTURE.json` and run `node Content/validate-day01-acceptance.cjs` plus `node Content/validate-money-wire.cjs` and other existing Content validators from the pinned Content checkout. Record stdout/stderr and exit codes; do not report a PASS from source inspection.
2. In the *actual* UI mock gateway, start `dayIndex=0`, `budgetKopecks="200000000"`, inspection tokens 2. Apply `LAB-1, STUDENT-1, PARTNER-1`, resolve multi-claim L-204 explicitly as `ROOM-4`, EndDay once and attempt EndDay again. Assert closing budget `"171500000"`, Q55/S51/M52/E47/R48, immediate cash postings `"-22000000"` and `"-6500000"` **only**, partner earned/signing income `"0"`, no reservation and no duplicate posting.
3. Reset mock state, inspect `DOC-01` and `DOC-03` **only**, select `LAB-2, STUDENT-2, PARTNER-2`; assert closing `"180500000"`, exactly two inspections, two immediate cash postings `"-16000000"` and `"-3500000"`, zero partner signing income, and all remaining expected scores/room/report effects **read directly from the fixture** (do not infer or hardcode unverified values).
4. Package the exact tested UI SHA, extract the offline ZIP, and exercise both paths in a real browser from the extracted folder. Record operating system, launch method, console errors, visible report and outcome. Node gateway PASS alone is not Windows browser PASS.

**Failure classification:** a gateway result of `"167500000"` or `"178000000"` is **CONTENT_UI_PARITY_FAIL**, not a balance proposal; a third inspection is **INSPECTION_CAP_FAIL**; auto-booking L-204 or omitting explicit `ROOM-4` in multi-claim is **ROOM_CONFLICT_FAIL**; earned partner income at signature is **DEFERRED_REVENUE_FAIL**. Report the exact SHA and failing assertion to UI 03, with QA 05 independently rerunning. No role 04 modifications to Core, EventDirector or UI.

**This pass actually executed:** GitHub reads of main branch, AGENTS.md, workflow, Issue #1/#7/#18, PR #9/#11/#12/#14 and latest PR #10/#11/#12 comments; no Node CLI, browser, Unity, or CI execution. **Gate: BLOCKED / NOT READY_FOR_OWNER_TEST** pending cross-branch parity and independent interactive QA.


## 16.11.2026 — Cross-platform Day01 art mapping (#20/#21)

Added `Content/DAY01_ART_LINKS.json`: three canonical visit/visitor/document IDs mapped to six planned asset IDs already listed in UI 03's art manifest, with distinct original fictional portrait descriptions. `dayIndex=0`, logical world 320x180, platform-neutral Windows/Android. This is a Content mock reference, **not generated PNG**, Core API, or changed balance. UI 03 owns production and actual PNG commits; QA 05 owns cross-branch integration checks.

**Checks actually executed:** 5/5 static JavaScript invariants PASS against fresh GitHub fixture and UI manifest (visits, documents, art IDs, uniqueness, day index). **Not executed:** Node CLI, CI, browser, Android device, Unity. Existing Issue #18 UI parity gate remains BLOCKED. First commit: `5d731a2650051f7392f354c4f2194b23bfde5b71`.


## 17.11.2026 — Day01 standing sprite references (#20/#21)

**One bounded content-only task:** extend `Content/DAY01_ART_LINKS.json` with `standingAssetId` for all three fictional visitors, pointing to existing `feature/ui-5/docs/art/ASSET_MANIFEST_D01.csv` rows (`irina_korneeva_standing`, `dasha_polynskaya_standing`, `mark_sviridov_standing`). Neutral portraits and blank documents remain unchanged; no PNGs claimed or produced. UI 03 owns sprite production and responsive Windows/Android presentation. No Core/ledger/EventDirector changes, no balancing changes.

**Actually verified before commit:** fresh GitHub reads; all three visit/visitor/document tuples match the canonical Day01 fixture; all nine referenced portrait/document/standing asset IDs exist in UI manifest; standing IDs unique 3/3; `dayIndex=0`. These are isolated JS structural checks, **not** Node CLI, CI, browser, Android device or Unity tests. Issue #18 UI parity and independent QA remain BLOCKED.


## Автономный проход 04 — cross-platform art-link validation (17.11.2026)

**Scope / #21:** в `Content/validate-day01-acceptance.cjs` добавлена проверка `DAY01_ART_LINKS.json` против канонического `DAY01_UI_FIXTURE.json`: три точных соответствия visit/visitor/document ID, девять уникальных ссылок на portrait/standing/document, непустые визуальные описания, DayIndex=0, Android+Windows, ART A, 320x180. Проверка не требует UI-ветки или Unity и не доказывает наличие PNG. Баланс, выборы, Core и EventDirector не менялись.

**Запуск QA 05:** `node Content/validate-day01-acceptance.cjs` из корня репозитория. Нужен независимый запуск на текущем SHA. **Ограничение:** GitHub-сверка исходников не равна исполнению Node CLI/CI; наличие реальных PNG и UI-паритет проверяет 03/05.


## 17.11.2026 — Three original Day01 art prompts (#21)

Prepared `Content/DAY01_ART_PROMPTS.md` for exactly three review candidates (`irina_korneeva_standing`, `dasha_polynskaya_standing`, `mark_sviridov_standing`) tied to stable canonical visit/visitor/document IDs. Production and PNG upload remain owned by UI 03. No generated binaries, no changes to balances or runtime. Static validation before commit: 3/3 IDs match `DAY01_ART_LINKS.json`; all 3 canonical visit/visitor/document tuples match `DAY01_UI_FIXTURE.json`; no money/schema modifications. Node CLI, Unity, browser, Android and CI not executed. Issue #18 playable parity remains unverified.

## 17.11.2026 — Three blank document production briefs (#21)

One bounded Content task: appended three **192×240 text-free pixel-art document briefs** to `Content/DAY01_ART_PROMPTS.md` for existing UI manifest IDs `doc01_procurement_blank`, `doc02_student_proposal_blank`, `doc03_partner_letter_blank`. All gameplay text, room L-204, monetary Int64 strings and inspection results remain dynamically rendered from `DAY01_UI_FIXTURE.json` by UI; no logos, third-party IP, PNGs, Core, EventDirector or balancing changes. The three fictional NPC IDs remain unchanged; partner earned revenue still requires Core-confirmed `E03_mentor_paid` and `E07_delivered` for the original agreement, with deduplication by Core. **Checks in this pass:** read fresh GitHub UI manifest and fixture; verified 3/3 document IDs and 192x240 native dimensions against the manifest, and existing three visitor/document mappings; documentation-only checks, not Node CLI/CI/browser/Unity/Android. UI 03 owns PNG creation; QA 05 owns independent validation. Issue #18 owner playtest remains unverified.
