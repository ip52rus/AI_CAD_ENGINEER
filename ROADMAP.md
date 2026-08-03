# AI CAD ENGINEER

## Vision

AI CAD ENGINEER — интеллектуальная инженерная система, автоматически создающая профессиональные чертежи по ЕСКД на основе 3D-модели.

Цель проекта — не автоматизация Inventor, а создание инженерного ядра, принимающего решения так же, как опытный инженер-конструктор.

---

# Architecture

```
3D Model
    │
    ▼
Import
    │
    ▼
Engineering Models
    │
    ▼
Engineering Analysis
    │
    ▼
Engineering Decision
    │
    ▼
Drawing Plan
    │
    ▼
Drawing Engine
    │
    ▼
Inventor Drawing
```

---

# Completed

## v0.1

- Connection to Autodesk Inventor
- Command processor
- Automatic drawing creation

---

## v0.2

- Automatic projection generation
- Automatic scale selection

---

## v0.3

- View analysis
- Projection statistics

---

## v0.4

- View scoring
- Intelligent main view selection

---

## v0.5

- View necessity analysis
- Engineering report

---

## v0.6

- Improved view scoring
- Better layout algorithm

---

## v0.7

- Hole analysis
- Part analysis
- Sheet metal support
- Feature recognition

---

## v0.8

- Accurate hole detection
- Hole diameter
- Hole depth
- Through-hole recognition
- Multiple hole instances
- Model analysis

---

## v0.9

### Architecture Refactoring

Completed:

- Engineering layer
- Drawing layer
- Import layer
- EngineeringBrain
- DrawingPlan
- ViewCandidate
- ViewCandidateGenerator
- Separation of responsibilities

---

# Current Version

## v0.10

### Engineering Pipeline

Completed

- EngineeringBrain
- ViewCandidateGenerator
- DrawingPlan
- ViewCandidate

In Progress

- Reporting layer
- ConsoleReporter
- Remove Console.WriteLine() from business logic

---

# Next Versions

## v0.11

Engineering Knowledge

Planned:

- Knowledge Base
- Rule Engine
- Main View Rule
- Scale Rule
- Centerline Rule
- Section Rule
- Dimension Rule

---

## v0.12

Automatic Dimensioning

Planned:

- Centerlines
- Hole dimensions
- Linear dimensions
- Radius dimensions
- Diameter dimensions

---

## v0.13

Section Generator

Planned:

- Full Section
- Half Section
- Local Section
- Broken-out Section

---

## v0.14

Sheet Metal Intelligence

Planned:

- Bend recognition
- Sheet thickness analysis
- Bend direction
- Flat Pattern support

---

## v0.15

Weldments

Planned:

- Weld symbols
- Weld tables
- Weld annotations

---

# Version 1.0

AI CAD ENGINEER

Goals

- Automatic drawing generation
- ESKD compliance
- Intelligent engineering decisions
- Automatic dimension placement
- Automatic centerlines
- Automatic sections
- Technical requirements
- Title block filling
- Production-ready documentation

---

# Long-term Vision

Future support:

- Autodesk Inventor
- SolidWorks
- Fusion 360
- STEP AP242
- NX
- CATIA

The engineering core must remain independent of any CAD system.