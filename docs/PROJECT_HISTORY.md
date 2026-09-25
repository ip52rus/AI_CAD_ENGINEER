# Project History

## 1. Исходная идея

AI CAD ENGINEER начался как попытка ответить на практический вопрос:

> Можно ли по готовой 3D-модели Autodesk Inventor автоматически создать комплект конструкторской документации по ЕСКД, максимально приблизившись к логике инженера-конструктора?

Первый подход был детерминированным: код сам анализирует модель, выбирает главный вид, масштаб, дополнительные виды и размеры.

## 2. v0.1–v0.3: доступ к Inventor

Были решены базовые интеграционные задачи:

- COM connection;
- подключение к уже запущенному Inventor;
- предотвращение лишнего второго экземпляра;
- active document;
- document type;
- создание DrawingDocument;
- base DrawingView.

На этом этапе был доказан фундаментальный факт: внешняя программа может надёжно управлять Inventor.

## 3. v0.4–v0.6: генерация листа

Добавлены:

- несколько стандартных проекций;
- центровые;
- автоматическое размещение;
- стандартные масштабы;
- оценка информативности;
- automatic main-view selection.

## 4. v0.7–v0.11: размерная логика

Проект перешёл от «нарисовать несколько видов» к попытке формализовать инженерное решение.

Появились:

- DrawingCurve research;
- DimensionCandidate;
- physical axis mapping;
- Length / Width / Height roles;
- Duplicate filtering;
- Dimension Decision Engine;
- отчёты решений.

Этот этап показал, что даже простая размерная логика быстро выходит за пределы набора универсальных эвристик.

## 5. v0.12: Engineering Feature Graph

Чтобы перестать рассуждать только по линиям чертежа, был добавлен feature-level graph:

- feature nodes;
- relationships;
- hole features;
- hole groups;
- metadata/reporting.

Это последний этап, полностью представленный текущим public source snapshot.

## 6. Архитектурный поворот

Дальнейшие эксперименты показали, что жёстко кодировать «инженера» внутри программы неэффективно.

Архитектура была изменена:

- Runtime = Eyes + Hands;
- внешняя LLM = analysis/planning/decision;
- пользователь = владелец design intent и финальный контролёр.

Это позволило резко расширять Inventor capability без связывания API-кода с конкретным изделием.

## 7. Atomic runtime

В поздней R&D-фазе Runtime был расширен большим количеством атомарных операций чтения и действия.

К исследованным категориям относились:

- documents;
- parts;
- assemblies;
- occurrences;
- referenced documents;
- BRep bodies/faces/edges;
- features/parameters;
- drawing sheets/views;
- drawing curves/model references;
- dimensions;
- sections/details;
- notes;
- balloons;
- tables;
- title blocks;
- borders;
- welding/surface symbols;
- center annotations;
- layout map;
- export.

К v0.68 registry достиг 219 операций.

## 8. Benchmark #1 — вал

Целью было проверить не отдельный API call, а полный reasoning workflow.

Модель содержала:

- ступенчатую геометрию;
- резьбу;
- отверстия;
- выборку;
- фаски;
- радиусы.

Был применён reference-aided planning:

- реальные производственные чертежи того же класса;
- factual model dossier;
- manufacturing requirements;
- несколько candidate plans;
- выбор;
- execution;
- render;
- visual correction.

Результат:

- содержание чертежа стало существенно лучше;
- выбранная структура видов была разумной;
- после ручной перестановки нескольких элементов был получен лучший результат эксперимента;
- layout всё ещё требовал human visual judgement.

## 9. Benchmark #2 — сварной каркас кресла

Второй benchmark специально выбрал другой класс изделия: assembly из профильной трубы.

Design intent:

- две сварные боковины;
- сварное сиденье;
- сварная спинка;
- между четырьмя узлами болтовые соединения M8;
- Ø9 through holes;
- Ø11,1 one-wall holes под threaded rivnut M8;
- Ст3.

### Geometry audit

Runtime восстановил:

- 32 assembly occurrences;
- 26 конструктивных металлических occurrences;
- 24 трубы;
- 2 пластины;
- профили 50×25×2 и 25×25×2;
- final BRep lengths;
- 5° / 10° / 45° / square cuts.

### Hole discrepancy

Первичный анализ ошибочно решил, что отверстий нет, потому что они не являлись HoleFeature.

После targeted audit было доказано:

- отверстия сделаны sketch + extrusion cut;
- 12 осей Ø9 проходят через профиль;
- 12 осей Ø11,1 проходят только одну стенку;
- BRep позволяет отличить отверстия от corner radii профиля.

Это стало важным R&D результатом.

### Fabrication content freeze

26 structural occurrences были сведены к 14 manufacturing detail types D01…D14 с учётом:

- profile;
- final BRep;
- end geometry;
- holes;
- handedness;
- mirror equivalence.

Также выяснилось, что Frame Generator parameter `B_L` нельзя автоматически принимать за final fabrication length.

### Runtime packages

Для execution были доказаны и добавлены минимальные generic capabilities.

#### v0.65

Referenced Part targeting для assembly Eyes.

#### v0.66

Расширение referenced targeting на geometry detail Eyes.

#### v0.67

DrawingView occurrence visibility:

- per-view hide/show;
- прямой GetVisibility readback;
- возможность изолировать conceptual welded unit без изменения source assembly.

#### v0.68

Atomic CustomTable editing:

- set one cell value;
- set one column width;
- readback/layout verification.

Последний registry count: 219 / 219 unique.

## 10. Reference-aided planning

Для Benchmark #2 было изучено 20 reference sources по сварным/профильным конструкциям.

Планировщик предложил три архитектуры документации.

Выбран modular hybrid:

- top assembly;
- four welded-unit drawings;
- fabrication schedule;
- selective detail atlas;
- M8/rivnut interface sheet;
- specifications.

Архитектура документации была логичной и производственно объяснимой.

## 11. Initial execution

Runtime технически смог:

- создать листы;
- изолировать occurrence sets;
- создать таблицы;
- заполнить specification data;
- создать interface views;
- сохранить source assembly неизменной.

Но visual critic обнаружил системные проблемы:

- слабый выбор/ориентация некоторых видов;
- отсутствие достаточного набора manufacturing dimensions;
- overlaps;
- плохое использование площади листа;
- необходимость повторного layout pass;
- formal spec form требовал дополнительного внимания.

## 12. Stop criterion

После двух разных классов изделий стало ясно:

- API integration работает;
- geometry extraction работает;
- generic CAD actions работают;
- planning иногда даёт хорошую структуру;
- качество полного листа не обобщается достаточно стабильно.

Для каждого нового класса изделия снова требовались:

- отдельный reference study;
- prompt tuning;
- multiple render/review loops;
- substantial human visual judgement.

Поэтому исходная продуктовая гипотеза — полностью автономный human-quality drawing engineer — была остановлена.

## 13. Что считается успешным результатом

Проект нельзя сводить к результату генератора чертежей.

Подтверждены:

- работающий Inventor integration layer;
- управляемый CAD runtime;
- separation of reasoning from execution;
- atomic read/write model;
- safe referenced-document reading;
- BRep-centric geometry analysis;
- source integrity discipline;
- practical LLM-controlled CAD workflows.

## 14. Текущее состояние

Активное развитие исходной autonomous-drafting цели прекращено.

Код и документация опубликованы как foundation для дальнейших экспериментов и более узких CAD-assistant сценариев.
