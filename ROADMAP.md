# Roadmap

## Status

The original R&D program is complete at **v0.68**.

The software goal — expose a broad Autodesk Inventor control surface to an external agent — was achieved.

The higher-level goal — reliable fully autonomous human-quality production drawings across different product classes — did not meet the required generalization threshold.

## Phase A — embedded engineering logic

### v0.1–v0.12

- Inventor COM integration
- drawing creation
- view generation/scoring
- dimension candidates and decision logic
- Engineering Feature Graph
- hole grouping

### v0.15-before-cleanup

Checkpoint preserving the original embedded decision architecture.

## Phase B — atomic Inventor runtime

### v0.15

Removed `CommandProcessor → DrawingManager → EngineeringBrain`.

### v0.16–v0.22

Typed table/text/symbol Eyes and drawing symbol coverage.

### v0.23–v0.30

Drawing creation, section/detail/auxiliary/break pipelines, PDF/DWG/DXF, parts lists and balloons.

### v0.31–v0.40

Dimension creation/editing/tolerance pipelines, hole/thread notes, center annotations, notes, FCF, surface texture, welding, symbols and balloon lifecycle.

### v0.41–v0.52

PartsList/HoleTable/CustomTable/RevisionTable lifecycles, revision clouds, edge/transition symbols, bend/chamfer/punch notes, center annotation deletes.

### v0.53–v0.62

Active-part Eyes, single-command runtime, model/sheet previews, read-only sketch hardening, modal-safe save/export, layout map and detail annotation movement.

### v0.63–v0.64

External ESKD policy and feature-coverage hardening.

### v0.65–v0.66

Referenced-part targeting from assembly context.

### v0.67

DrawingView-local occurrence visibility.

### v0.68

Atomic CustomTable cell/column editing.

## Stop criterion

Two product-class benchmarks showed that the remaining bottleneck was not missing Inventor API access.

The unstable layer remained:

- choosing the best representation;
- complete/non-redundant dimension strategy;
- readable sheet composition;
- generalization to a new product class.

## Potential future directions

- model interrogation;
- drawing/model audit;
- fabrication-data extraction;
- batch Inventor automation;
- supervised CAD assistant;
- JSON tool layer for other agent frameworks.

## Not an active goal

Without a substantially different reasoning/visual approach, the project should not return to fully autonomous production-quality drawing generation for arbitrary models.
