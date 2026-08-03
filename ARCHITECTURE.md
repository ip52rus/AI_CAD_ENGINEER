# AI CAD ENGINEER Architecture

## Overview

AI CAD ENGINEER is an engineering decision engine.

The project is built around one idea:

> Engineering decisions must be independent from any CAD system.

Inventor is only used as:

- geometry source
- drawing generator

All engineering decisions are made inside the Engineering layer.

---

# High Level Architecture

```
                    User
                      │
                      ▼
              CommandProcessor
                      │
                      ▼
              EngineeringBrain
                      │
      ┌───────────────┼────────────────┐
      │               │                │
      ▼               ▼                ▼
 Import          Engineering      Drawing
      │               │                │
      ▼               ▼                ▼
 Inventor      Decision Engine   Inventor API
```

---

# Layers

## Import

Responsible for obtaining information from CAD.

Contains:

- ModelAnalyzer
- HoleAnalyzer
- ViewCandidateGenerator

Responsibilities:

- Read Inventor model
- Read geometry
- Read features
- Produce engineering models

Must NOT:

- make engineering decisions
- create drawings
- print reports

---

## Engineering.Models

Contains project domain objects.

Examples:

- PartAnalysis
- ViewStatistics
- ViewCandidate
- DrawingPlan

These classes contain data only.

---

## Engineering.Analysis

Responsible for analysing engineering data.

Examples:

- ViewAnalyzer
- ViewScoreCalculator
- ViewNecessityAnalyzer

Responsibilities:

- analyse geometry
- calculate statistics
- calculate scores

Must NOT:

- create drawings
- communicate with Inventor
- decide engineering strategy

---

## Engineering.Decision

Responsible for engineering decisions.

Contains:

- EngineeringBrain

Future:

- Knowledge Base
- Rule Engine

Responsibilities:

- choose main view
- choose scale
- choose additional views
- determine sections
- determine dimensions
- determine centerlines

---

## Drawing

Responsible only for drawing creation.

Contains:

- DrawingManager

Responsibilities:

- create drawing document
- create drawing views
- arrange views
- create dimensions
- create sections

Must NOT:

- analyse geometry
- choose engineering strategy

---

## Infrastructure

Responsible for technical services.

Future:

- ConsoleReporter
- Logger
- Configuration
- Export
- PDF
- HTML

Infrastructure must never contain engineering logic.

---

# Data Flow

```
3D Model
    │
    ▼
Import
    │
    ▼
PartAnalysis
    │
    ▼
ViewCandidateGenerator
    │
    ▼
ViewCandidate[]
    │
    ▼
EngineeringBrain
    │
    ▼
DrawingPlan
    │
    ▼
DrawingManager
    │
    ▼
Inventor Drawing
```

---

# Dependency Rules

Allowed:

Import
    ↓
Engineering

Engineering
    ↓
Drawing

Drawing
    ↓
Inventor

Forbidden:

Drawing → Engineering Analysis

Engineering → Inventor API

Models → Inventor API

Infrastructure → Engineering Decisions

---

# Design Principles

Every class should have a single responsibility.

Business logic must never depend on Inventor.

Engineering decisions must be separated from drawing generation.

Data models must remain independent.

All future CAD systems should use the same Engineering layer.

---

# Long-term Goal

The Engineering layer should become completely CAD-independent.

Only the Import and Drawing layers should know anything about Autodesk Inventor.