# Roadmap

## Статус

Исходная исследовательская программа завершена.

Главная цель раннего roadmap — полностью автономный выпуск КД по ЕСКД из произвольной модели Inventor — была доведена до реальных benchmark-экспериментов, но не показала требуемой устойчивости качества между разными классами изделий.

При этом программная интеграция с Inventor и automation foundation были подтверждены.

## Завершённый публичный этап

### v0.1 — Inventor integration

- COM connection;
- active document detection;
- базовый command routing.

### v0.2 — Drawing creation

- создание DrawingDocument.

### v0.3 — Base view

- создание базового DrawingView.

### v0.4–v0.6 — View generation and layout

- три вида;
- layout;
- automatic main-view selection.

### v0.7–v0.10 — Dimension research

- DrawingCurve research;
- dimension candidates;
- physical axes;
- dimension roles;
- reporting.

### v0.11 — Dimension Decision Engine

- классификация dimension candidates;
- Required / Duplicate / Optional и связанные статусы;
- передача выбранных размеров в Drawing layer.

### v0.12 — Engineering Feature Graph

- FeatureGraph;
- FeatureNode;
- relationships;
- HoleFeature extraction;
- HoleGroup construction.

## Поздний исследовательский этап

После v0.12 проект сменил архитектуру с встроенного Engineering Brain на внешний LLM + atomic Eyes/Hands.

Ключевые подтверждённые этапы:

- чтение assembly hierarchy и BRep;
- работа с nested referenced parts без переключения активного документа;
- drawing-view occurrence isolation;
- atomic CustomTable editing;
- specification proof;
- model/drawing integrity checks;
- reference-aided planning;
- реальные benchmarks на детали и сварной сборке.

Последний исследовательский checkpoint: v0.68, 219 зарегистрированных runtime operations.

Эти поздние исходники пока не отражены полностью в public `main`; история и результаты сохранены в документации.

## Исследовательский stop-criterion

Разработка исходной идеи остановлена не из-за Inventor API.

Причина остановки:

- автономный выбор видов не обобщается достаточно надёжно;
- размерная полнота требует производственного контекста;
- layout требует многократного visual feedback;
- на новом классе изделия снова нужен существенный prompt/review цикл;
- стоимость доведения одного комплекта до хорошего состояния не подтверждает ценность полностью автономного режима.

## Если проект продолжать

Наиболее рациональные направления:

### 1. Model interrogation

Natural-language запросы к реальной модели/сборке:

- какие профили используются;
- какие детали имеют заданные отверстия;
- какие элементы отличаются только зеркальностью;
- какие материалы не заполнены;
- какие детали геометрически одинаковы.

### 2. Drawing audit

Проверка уже созданного человеком чертежа:

- пропущенные dimensions;
- duplicate dimensions;
- title block;
- collisions;
- missing hole/thread notes;
- cross-check model ↔ drawing.

### 3. Fabrication extraction

- cut list;
- final BRep lengths;
- end cuts;
- hole patterns;
- per-member operations;
- CSV/XLSX/PDF export.

### 4. Batch automation

- document properties;
- export;
- renaming;
- title-block population;
- repetitive drawing operations.

### 5. Supervised drawing assistant

Человек определяет engineering intent и layout, AI выполняет локальные CAD-команды.

## Что не рекомендуется считать ближайшей целью

Без нового фундаментального подхода не следует возвращаться к формулировке:

> «полностью автоматически выпускать красивый и производственно полный комплект КД для любой модели Inventor».

Benchmark-результаты не подтверждают устойчивость такого режима.
