# Architecture

## 1. Назначение

AI CAD ENGINEER исследует архитектуру внешнего программного управления Autodesk Inventor и применение LLM к инженерным CAD workflow.

Архитектура менялась по мере экспериментов. Важно различать:

1. архитектуру опубликованного v0.12 snapshot;
2. позднюю исследовательскую архитектуру External LLM + atomic Eyes/Hands.

## 2. Опубликованный snapshot: layered engineering pipeline

Текущий public source содержит классическую многоуровневую схему:

```text
Autodesk Inventor
      │
      ▼
Core / Import
      │
      ▼
Engineering
 ├─ Analysis
 ├─ Geometry
 ├─ Research
 └─ Decision
      │
      ▼
Drawing
      │
      ▼
Infrastructure / Reporting
```

### Core

Отвечает за жизненный цикл приложения и соединение с Inventor.

Ключевые задачи:

- подключиться к уже запущенному Inventor через COM;
- при необходимости запустить Inventor;
- получить активный документ;
- определить тип документа;
- принять пользовательскую команду.

### Import / Inventor

Получает факты из Inventor.

В опубликованном snapshot сюда входят:

- ModelAnalyzer;
- HoleAnalyzer;
- ViewCandidateGenerator.

Import layer не должен принимать решения об оформлении.

### Engineering / Analysis

Преобразует CAD-данные в инженерные метрики и кандидаты:

- анализ видов;
- статистика геометрии;
- кандидаты размеров;
- габаритные размеры;
- необходимость вида.

### Engineering / Geometry

В v0.12 появился Engineering Feature Graph:

- FeatureNode;
- FeatureRelationship;
- FeatureGraph;
- HoleFeatureGraphExtractor;
- HoleGroupBuilder.

Цель слоя — уйти от рассуждения только по линиям DrawingView и работать с инженерными объектами модели.

### Engineering / Decision

Содержит ранний встроенный Engineering Brain:

- выбор главного вида;
- выбор дополнительных видов;
- Dimension Decision Engine;
- определение ролей Length / Width / Height;
- классификация Required / Duplicate / Optional и др.

### Engineering / Research

Исследует уже созданную DrawingView-геометрию:

- DrawingCurve;
- линии;
- дуги;
- окружности;
- реальные границы вида;
- связи с физическими осями.

### Drawing

Исполняет готовые решения:

- создаёт DrawingDocument;
- создаёт виды;
- рассчитывает масштаб;
- размещает виды;
- добавляет центровые;
- создаёт размеры.

### Infrastructure

Вспомогательные сервисы:

- отчёты;
- файловые пути;
- console/file reporting.

## 3. Почему архитектура была изменена

Эксперименты показали, что инженерная логика быстро становится слишком сложной для набора жёстко зашитых правил.

Главные проблемы:

- выбор вида зависит от design intent, а не только от числа линий;
- один и тот же BRep может требовать разной документации в разных производственных контекстах;
- assembly hierarchy не всегда совпадает с технологической;
- важные элементы могут быть созданы не «правильным» feature типом;
- layout и размерная архитектура плохо обобщаются между классами изделий.

Поэтому поздняя R&D-фаза перенесла reasoning из Runtime во внешнюю LLM.

## 4. Поздняя архитектура: External LLM + Eyes/Hands

```text
                 External LLM
        engineering interpretation
         planning / validation
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
      Eyes                Hands
 atomic reads        atomic actions
        │                   │
        └─────────┬─────────┘
                  ▼
           Autodesk Inventor
```

### Eyes

Eye должен отвечать на один фактический вопрос.

Примеры исследованных категорий:

- active document;
- occurrences;
- referenced documents;
- surface bodies;
- BRep faces/edges;
- feature tree/details;
- model parameters;
- drawing views and curves;
- model references;
- dimensions;
- notes;
- balloons;
- tables;
- title block;
- layout map.

Eye не должен интерпретировать геометрию как «правильную деталь кресла» или принимать технологическое решение.

### Hands

Hand выполняет одно однозначное действие Inventor API.

Примеры исследованных категорий:

- create/move drawing view;
- section/detail views;
- dimensions;
- center marks/centerlines;
- notes;
- balloons;
- welding/surface symbols;
- sheet/border/title block;
- table operations;
- per-view occurrence visibility;
- export.

Один Hand не должен реализовывать «создать чертёж боковины» или «построить спецификацию кресла». Семантика остаётся снаружи.

## 5. Архитектурные инварианты

### Runtime is not the engineer

Runtime читает и действует. Он не решает design intent.

### One atomic action

Каждый Hand должен менять минимально возможную единицу состояния.

### Read before write

Перед любым write workflow внешний агент обязан собрать достаточные факты.

### Source-model integrity

Drawing workflow не должен неожиданно изменять исходную деталь/сборку.

### No silent fallback

Если Inventor API не позволяет выполнить действие в заданном контексте, Runtime должен вернуть ошибку, а не изменять source model альтернативным способом.

### Final BRep over feature naming

Для фактической конечной геометрии BRep важнее имени feature. Это особенно критично для:

- mirrored/generated parts;
- imported geometry;
- extrusion-cut holes;
- Frame Generator members после split/trim.

### External uncertainty

Если нужное значение отсутствует в модели и не задано пользователем, оно остаётся unresolved.

## 6. Пример: почему это важно

В Benchmark #2 отверстия под крепёж были созданы не HoleFeature, а обычным sketch + extrusion cut.

Feature-oriented проверка сначала пропустила их.

BRep-аудит восстановил:

- 12 × Ø9 through-profile;
- 12 × Ø11.1 one-wall;
- направление осей;
- положение;
- принадлежность к profile member;
- функциональное различие.

Это стало важным архитектурным уроком: инженерное чтение CAD не должно зависеть только от feature taxonomy.

## 7. Контроль качества runtime package

Каждый новый capability package проходил:

1. Capability Audit.
2. Решение: нужен ли код.
3. Минимальная реализация.
4. classic MSBuild.
5. registry uniqueness check.
6. live Inventor E2E.
7. source-model dirty-state check.
8. Git checkpoint/tag.

Если шаг 2 давал «существующего capability достаточно», реализация запрещалась.

## 8. Что оказалось за пределами Runtime

Runtime успешно решал API/automation часть.

Хуже всего обобщались:

- выбор оптимального набора видов;
- выбор полного, но неизбыточного набора размеров;
- композиция листа;
- визуальная иерархия;
- нормоконтроль, требующий design intent;
- переход между разными классами изделий.

Эти задачи нельзя считать решёнными одной только богатой Inventor API surface.

## 9. Практический вывод

Архитектура Eyes/Hands остаётся полезной как foundation для:

- supervised CAD assistants;
- model interrogation;
- drawing audit;
- batch automation;
- fabrication-data extraction;
- LLM-controlled deterministic workflows.

Полностью автономный drawing engineer не следует считать доказанной возможностью этой кодовой базы.
