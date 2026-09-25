# Project History

## 1. Initial hypothesis

The project began with a practical question:

> Can software analyze an Autodesk Inventor 3D model and automatically create an ESKD-style engineering drawing?

## 2. v0.1–v0.12 — embedded engineering pipeline

Early versions implemented:

- Inventor COM connectivity;
- active-document detection;
- drawing creation;
- standard projected views;
- view scoring/main-view selection;
- model and hole analysis;
- drawing-curve research;
- dimension candidates and decisions;
- Engineering Feature Graph;
- hole grouping.

This proved Inventor control, but hard-coded engineering judgement became increasingly brittle.

## 3. v0.15 — architecture reset

The old path was preserved at `v0.15-before-cleanup`, then removed:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
```

New rule:

> Runtime reads and acts; the external LLM reasons.

## 4. v0.16–v0.52 — Eyes/Hands expansion

The runtime systematically added drawing/data capabilities: tables, text, symbols, drawing creation, section/detail/auxiliary views, exports, dimensions/tolerances, notes, center annotations, balloons, PartsLists, revision objects and manufacturing annotations.

## 5. v0.53–v0.62 — agent-oriented runtime

Added:

- active-part Eyes;
- deterministic single-command mode;
- drawing/model previews;
- read-only sketch hardening;
- modal-safe save/export;
- drawing layout map;
- detail annotation text movement.

## 6. v0.63–v0.64 — external ESKD policy

Drawing rules were formalized for the external reasoning layer rather than embedded into Runtime.

## 7. Benchmark #1 — turned shaft

A real shaft with steps, thread, holes, countersink, recess, chamfers and radius was used for reference-aided planning.

Real drawings from the same part class improved the plan and content. The best result still needed human visual layout correction.

## 8. v0.65–v0.66 — referenced-part targeting

Benchmark #2 required reading nested part geometry without activating referenced documents.

Existing Part Eyes gained `target.occurrencePath` support through the active assembly.

## 9. Benchmark #2 — welded chair frame

The assembly contained profile-tube welded units connected by M8 bolts/rivnuts.

The system recovered:

- 32 total assembly occurrences;
- 26 structural metal occurrences;
- 24 tubes + 2 plates;
- 50×25×2 and 25×25×2 profiles;
- square, 5°, 10° and 45° end conditions.

### Hole discrepancy

The first sweep missed connection holes because they were not HoleFeatures. They were circular sketch + Extrude Cut features.

A targeted BRep audit recovered:

- 12 × Ø9 through-profile axes;
- 12 × Ø11.1 one-wall axes;
- axis/location facts;
- distinction from profile corner radii.

### Manufacturing equivalence

26 structural occurrences were reduced to 14 detail types using final BRep, end geometry, hole patterns, through/one-wall state, handedness and rotation/mirror equivalence.

The audit also showed that Frame Generator `B_L` is not always final fabrication length.

## 10. v0.67 — DrawingView-local isolation

Added generic per-view occurrence visibility, allowing conceptual welded groups to be isolated without mutating source assembly visibility.

## 11. v0.68 — CustomTable editing

Added atomic cell value and column width writes to support externally planned schedules/specification experiments.

## 12. Documentation planning experiment

Twenty real welded/profile-frame references were studied. A modular documentation architecture was selected:

- top assembly;
- four welded-unit drawings;
- fabrication schedule;
- selective detail atlas;
- M8/rivnut interface sheet;
- specifications.

## 13. Real execution and stop criterion

The runtime technically created the planned sheets and data structures, but the rendered result still showed:

- weak view orientation in places;
- incomplete manufacturing dimensioning;
- overlaps;
- uneven use of sheet space;
- need for substantial visual correction.

These were not primarily missing API capabilities.

The project therefore concluded that the Inventor control layer was successful, while the fully autonomous drawing-engineer hypothesis had not generalized sufficiently.

The project was frozen at v0.68 as a reusable CAD automation / AI-agent research foundation.
