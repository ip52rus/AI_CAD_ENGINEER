# Capabilities

Этот документ разделяет возможности текущего public v0.12 snapshot и возможности, подтверждённые в поздней экспериментальной фазе.

## Public v0.12 source

### Inventor connection

- connect to running Inventor;
- start Inventor;
- active document;
- document type.

### Model analysis

- dimensions/extents;
- mass-related/model facts;
- hole analysis;
- sheet-metal-related facts;
- view candidate generation.

### Drawing generation

- create drawing;
- base/projection views;
- scale selection;
- layout;
- center annotations;
- overall dimensions.

### Drawing research

- DrawingCurve analysis;
- geometric statistics;
- candidate dimensions.

### Decision layer

- main view scoring;
- view necessity;
- physical axis mapping;
- dimension role resolution;
- dimension classification.

### Engineering Feature Graph

- graph nodes;
- relationships;
- hole feature extraction;
- hole groups;
- reporting.

## Later experimental runtime

Поздняя архитектура была существенно шире и стала command-oriented.

Ключевые исследованные categories:

### Documents

- read active document;
- open/activate/navigation workflows;
- document properties;
- save/export.

### Assembly

- occurrences;
- nested occurrence paths;
- referenced documents;
- transforms;
- BOM/context data.

### Part geometry

- surface bodies;
- faces;
- edges;
- feature tree;
- feature details;
- parameters.

### Drawings

- sheets;
- base/projected/auxiliary/section/detail views;
- scales/styles/alignment;
- drawing curves and model references;
- dimensions;
- notes;
- balloons;
- center marks/centerlines;
- symbols;
- border/title block;
- tables;
- layout map;
- PNG/PDF/DWG/DXF.

### Important late packages

#### v0.65

Referenced Part targeting from Assembly context.

#### v0.66

Referenced targeting for geometry detail Eyes.

#### v0.67

Per-DrawingView occurrence visibility with direct readback.

#### v0.68

Atomic CustomTable cell and column-width editing.

Experimental registry at v0.68: 219 unique operations.

## Capability boundary

Даже широкий runtime не делает сам по себе:

- engineering intent;
- production decisions;
- complete dimension strategy;
- human-quality sheet composition.

Эти задачи требуют внешнего reasoning и human validation.
