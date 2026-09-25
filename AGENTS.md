# AGENTS.md

Правила для AI coding/CAD agents, работающих с AI CAD ENGINEER.

## 1. Роль агента

Агент может:

- исследовать текущие capabilities;
- проектировать минимальное расширение;
- писать код;
- собирать проект;
- выполнять контролируемые Inventor E2E tests;
- создавать отчёты.

Агент не должен подменять отсутствующий engineering intent догадкой.

## 2. Eyes / Hands contract

### Eye

Одна атомарная операция чтения.

Eye:

- не меняет Inventor document;
- не принимает решение «как правильно»;
- возвращает факты и traceability.

### Hand

Одно атомарное действие.

Hand:

- не объединяет несколько engineering decisions;
- не скрывает побочные изменения;
- возвращает factual readback результата.

Плохо:

`create_correct_chair_drawing`

Хорошо:

`create_base_view`

`set_drawing_view_occurrence_visibility`

`set_custom_table_cell_value`

## 3. Capability Audit before code

Перед реализацией нового capability:

1. проверить registry;
2. изучить существующие Eyes/Hands;
3. проверить Inventor API;
4. решить, нужен ли код вообще.

Если capability уже достаточен:

**не писать новый код**.

## 4. Design intent

Нельзя самостоятельно придумывать:

- материал;
- точную марку/ГОСТ;
- допуски;
- покрытия;
- сварочные размеры;
- крепёж;
- технологию;
- designation;
- manufacturing rule.

Неизвестное значение должно быть помечено как unresolved и вынесено пользователю.

## 5. Геометрия

При конфликте источников:

1. final BRep;
2. параметры/feature details;
3. feature names;
4. filename conventions.

Имя feature не является доказательством геометрического смысла.

Пример из benchmark: реальные отверстия были сделаны Extrude Cut, а не HoleFeature.

## 6. Referenced parts

Не активировать и не сохранять referenced part только ради чтения, если факт можно получить через occurrence/reference context.

Source assembly должна оставаться неизменной.

## 7. Drawing safety

Read/audit mode:

- no drawing edits;
- no save;
- no model edits.

Write mode:

- менять только явно разрешённый DrawingDocument;
- не изменять source model как fallback;
- после каждого значимого шага проверять dirty state.

## 8. Visual QA

CAD API success не означает качественный чертёж.

Для drawing workflow обязателен цикл:

```text
create
→ factual readback
→ render
→ visual review
→ correction
→ render
```

Нельзя объявлять лист готовым только потому, что API вызовы завершились без исключений.

## 9. Build

Использовать classic Visual Studio MSBuild, если это соответствует текущей среде проекта.

После build:

- PASS;
- command registry unique;
- no unintended warnings introduced.

## 10. E2E

Новый Inventor capability считается готовым только после live E2E.

Отчёт должен включать:

- input;
- command;
- direct readback;
- document dirty state;
- source integrity;
- error cases.

## 11. Git

- один capability package = один логичный checkpoint;
- не коммитить temp Inventor files/PNGs/payloads;
- не использовать force push без отдельного разрешения;
- не смешивать unrelated cleanup с capability commit.

## 12. Scope discipline

Не добавлять «полезные на будущее» возможности.

Каждое изменение должно отвечать на доказанный blocker.

## 13. Research conclusion

Не предполагать, что большая command surface автоматически решает autonomous drafting.

Проект показал, что Inventor automation layer масштабируется лучше, чем визуально-инженерная логика полного чертежа.

Предпочтительные новые сценарии:

- interrogation;
- audit;
- extraction;
- batch automation;
- supervised CAD actions.
