# AI CAD ENGINEER

# ROADMAP

---

# Статус проекта

Текущая версия:

```
v0.11.0
```

---

# Завершённые версии

## ✅ v0.1

Основа проекта

- подключение к Inventor
- создание первого чертежа
- анализ модели
- выбор главного вида

---

## ✅ v0.2

View Analyzer

- анализ шести стандартных видов
- оценка информативности
- автоматический выбор главного вида

---

## ✅ v0.3

Drawing Generator

- создание трёх видов
- автоматический масштаб
- автоматическое размещение

---

## ✅ v0.4

Center Annotation

- центровые линии
- центровые метки

---

## ✅ v0.5

Hole Analysis

- анализ отверстий
- анализ листового металла

---

## ✅ v0.6

Geometry Research

- исследование DrawingView
- анализ DrawingCurve
- поиск габаритов

---

## ✅ v0.7

Dimension Candidates

- поиск кандидатов размеров
- анализ физических осей

---

## ✅ v0.8

Role Resolver

- Length
- Width
- Height

---

## ✅ v0.9

Dimension Classification

- Required
- Duplicate
- Optional

---

## ✅ v0.10

Reporting

- отчёты исследований
- отчёты решений

---

## ✅ v0.11

Dimension Decision Engine

- DimensionDecisionCoordinator
- DimensionDecisionResult
- классификация размеров
- исключение дублей
- передача только Required размеров
- физические оси видов
- инженерная архитектура Decision Layer

---

# Текущая разработка

## 🚧 v0.12

Engineering Feature Graph

Планируется:

- FeatureNode
- FeatureGraph
- FeatureExtractor
- GeometryAnalyzer

Поддержка:

- Hole
- Pocket
- Boss
- Slot
- Chamfer
- Fillet
- Bend
- Flange

Результат:

полное инженерное описание детали.

---

## 🔵 v0.13

Engineering Dimension Graph

Будут реализованы:

- полный поиск всех размерных кандидатов
- граф зависимостей размеров
- инженерные базы
- размерные цепочки

---

## 🔵 v0.14

Dimension Decision Engine 2.0

Добавится:

- Redundant
- Reference
- Recommended
- Grouped

Инженерные правила ЕСКД.

---

## 🔵 v0.15

Automatic Dimensioning

Автоматическое нанесение:

- отверстий
- радиусов
- фасок
- пазов
- вырезов
- гибов

---

## 🔵 v0.16

Automatic Sections

Автоматическое построение:

- разрезов
- сечений
- местных разрезов

---

## 🔵 v0.17

Assembly Drawings

Поддержка сборок.

---

## 🔵 v0.18

Specification Generator

Автоматическое создание спецификаций.

---

## 🔵 v0.19

Sheet Metal

Полная поддержка листового металла.

---

## 🔵 v1.0

AI CAD ENGINEER

Первая стабильная версия.

Полностью автоматическое создание конструкторской документации по ЕСКД.

---

# Главная цель проекта

Создать интеллектуальную инженерную систему, которая принимает инженерные решения аналогично опытному инженеру-конструктору и автоматически выпускает комплект конструкторской документации.