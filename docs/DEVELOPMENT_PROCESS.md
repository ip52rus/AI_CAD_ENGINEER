# Development Process

## Цель процесса

Развивать Inventor automation только под доказанные реальные blockers и не превращать Runtime в набор случайных специальных функций.

## 1. Capability Audit

Перед каждым package:

- сформулировать пользовательскую задачу;
- проверить текущий registry;
- изучить существующие implementation paths;
- проверить Inventor API;
- определить root cause.

Результат должен ответить:

```text
CODE_REQUIRED = YES / NO
```

Если NO — следующий шаг выполняется существующими capabilities.

## 2. Минимальный package

Если код нужен:

- реализуется минимальная generic capability;
- запрещена предметная семантика;
- существующие команды не меняют поведение без необходимости.

Пример:

не `isolate_left_chair_side`, а `set_drawing_view_occurrence_visibility`.

## 3. Build

Использовался classic MSBuild Visual Studio.

Ожидание:

- build PASS;
- registry unique;
- unrelated warnings не появляются.

## 4. Live Inventor E2E

Unit-level API reasoning недостаточен.

Каждый package проверяется в реальном Inventor:

- реальные документы;
- реальные occurrence paths;
- direct readback;
- error cases.

## 5. Integrity gate

После E2E:

- source model dirty=false, если workflow не должен её менять;
- referenced docs dirty=false;
- no unexpected save;
- no modal dialogs;
- temporary test docs не попадают в Git.

## 6. Checkpoint

Только после успешного E2E:

- diff audit;
- final build;
- commit;
- local tag.

## 7. Drawing workflow

Чертёжный объект считается созданным, но не качественным, пока нет render review.

```text
facts
→ plan
→ create
→ layout map
→ PNG
→ visual critic
→ correction
→ QA
```

## 8. Design-intent policy

Отсутствующие данные нельзя восстанавливать «по здравому смыслу» без явной маркировки.

Примеры:

- weld leg;
- material suffix;
- fastener class;
- coating;
- tolerance class;
- designation.

Вместо догадки:

```text
DESIGN_INTENT_REQUIRED
UNRESOLVED
```

## 9. Stop conditions

Package останавливается, когда доказан требуемый capability.

Не следует расширять API «раз уж мы здесь».

Исследовательский трек останавливается, если bottleneck перестал быть software capability и перешёл в область, где дальнейший код не подтверждает продуктовую ценность.
