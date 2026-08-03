# CHANGELOG

Все значимые изменения проекта фиксируются в этом документе.

Формат основан на принципах Keep a Changelog.

---

# v0.11.0 — Dimension Decision Engine

Дата: 03.08.2026

---

## Added

### Engineering Decision Layer

- DimensionDecisionCoordinator
- DimensionDecisionResult
- DimensionClassificationEngine
- DimensionRuleEngine
- OverallDimensionRoleResolver
- ViewAxisMappingResolver

---

### Dimension Analysis

Добавлены:

- генерация кандидатов размеров;
- определение физических осей видов;
- определение ролей размеров;
- классификация размеров;
- поиск дублирующих размеров.

---

### Drawing Research

Добавлен модуль:

DrawingGeometryResearch

Возможности:

- анализ DrawingCurve;
- анализ геометрии вида;
- поиск габаритов;
- исследование реальных размеров вида.

---

### Reporting

Добавлена универсальная система отчётов.

Поддерживаются:

- Drawing Geometry Research Report
- Dimension Decision Report

Все отчёты автоматически сохраняются в папку Reports.

---

## Changed

Полностью переработан процесс нанесения размеров.

Ранее:

```
Поиск размеров

↓

Нанесение
```

Теперь:

```
Поиск размеров

↓

Role Resolver

↓

Dimension Classification

↓

Decision Result

↓

Нанесение
```

---

Drawing Layer теперь получает только размеры со статусом Required.

---

## Architecture

Добавлены новые подсистемы:

```
Engineering
    Decision

Engineering
    Research

Infrastructure
    Reporting
```

Архитектура стала многоуровневой.

Принятие инженерных решений полностью отделено от построения чертежей.

---

## Fixed

Исправлено:

- определение физических осей видов;
- выбор размеров для кубических деталей;
- определение ролей Length / Width / Height;
- исключение дублирующих размеров;
- стабильность исследований DrawingView.

---

## Result

Проект перешёл от генерации размеров к инженерной системе принятия решений.

Dimension Decision Engine стал самостоятельной подсистемой архитектуры AI CAD ENGINEER.

---

# Следующая версия

v0.12.0

Engineering Feature Graph