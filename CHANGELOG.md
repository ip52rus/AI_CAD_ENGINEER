# Changelog

Все значимые изменения опубликованного source snapshot.

Формат близок к Keep a Changelog. Версии после v0.12, относящиеся к поздней экспериментальной Eyes/Hands-архитектуре, описаны в [docs/PROJECT_HISTORY.md](docs/PROJECT_HISTORY.md), поскольку их исходники пока не полностью представлены в public `main`.

## v0.12.0 — Feature Graph and Hole Groups

- добавлен Engineering Feature Graph;
- FeatureNode / FeatureRelationship;
- metadata и отчёт графа;
- извлечение HoleFeature;
- HoleGroupBuilder;
- группировка отверстий;
- интеграция feature graph в drawing pipeline.

## v0.11.0 — Dimension Decision Engine

- DimensionDecisionCoordinator;
- DimensionDecisionResult;
- DimensionClassificationEngine;
- DimensionRuleEngine;
- OverallDimensionRoleResolver;
- ViewAxisMappingResolver;
- классификация размерных кандидатов;
- исключение дублей;
- отчёт решений.

## v0.10.x — Reporting

- отчёты исследования геометрии;
- отчёты решений;
- console/file reporting.

## v0.9.0 — Dimension classification

- статусы dimension candidates;
- Required / Duplicate / Optional;
- подготовка к отдельному Decision layer.

## v0.8.x — Role resolver

- физические X/Y/Z;
- роли Length / Width / Height;
- связь размеров с view axis mapping.

## v0.7.0 — Dimension candidates

- поиск кандидатов размеров;
- geometry-driven dimension research.

## v0.6.0 — Main view selection

- оценка стандартных проекций;
- автоматический выбор основного вида.

## v0.5.0 — Three-view layout

- генерация трёх проекций;
- размещение на листе;
- масштабирование.

## v0.4.0 — Center annotations

- центровые линии;
- центровые метки.

## v0.3.0 — Base drawing view

- создание базового вида детали.

## v0.2.0 — Empty drawing

- создание DrawingDocument.

## v0.1.0 — Initial Inventor integration

- подключение к Autodesk Inventor через COM;
- подключение к уже запущенному экземпляру;
- запуск Inventor при необходимости;
- active document readback;
- базовый command routing.
