# CURRENT_STATE.md — AI CAD ENGINEER

## Repository

Expected local path:

```text
C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER
```

Current working branch:

```text
cleanup/legacy-architecture
```

Current checkpoint before the Package 20B documentation sync:

```text
13c02c9 v0.19 complete Feature Control Frames Eye
```

## Runtime architecture

The active runtime path is:

```text
LLM
→ JSON
→ Program.cs
→ Core/Application.cs
→ InventorControl/InventorCommandDispatcher.cs
→ IInventorCommand
→ CommandSupport / ReadSupport
→ Autodesk Inventor API
```

The C# runtime is only Eyes and atomic Hands. Engineering decisions, planning, automatic layout, optimization, and best-choice logic remain outside the runtime and are performed by the external LLM.

Do not restore the removed legacy architecture:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
```

## Dispatcher and command inventory

Live command audit after Package 20A:

```text
116 registered JSON commands
116 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Always recalculate from the live repository before relying on these numbers.

## Confirmed baseline commands

Confirmed post-cleanup commands:

```json
{"command":"ping"}
```

```json
{"command":"get_active_document"}
```

```json
{"command":"get_drawing_sheets"}
```

## Package 13A status

Package 13A added atomic base drawing view creation:

```text
InventorControl/Commands/DrawingViews/BaseViewCommandSupport.cs
InventorControl/Commands/DrawingViews/CreateBaseViewCommand.cs
```

Command:

```json
{"command":"create_base_view"}
```

Status:

- implemented and registered;
- uses explicit `Inventor._Document` cast required by `DrawingViews.AddBaseView`;
- included in successful MSBuild checkpoints;
- Inventor PASS is not recorded in this document.

## Package 15A status

Package 15A added basic Drawing Annotation Eyes:

```json
{"command":"get_general_notes"}
{"command":"get_leader_notes"}
{"command":"get_balloons"}
{"command":"get_center_marks"}
{"command":"get_centerlines"}
```

Status:

- implemented and registered;
- included in successful MSBuild checkpoints;
- Inventor PASS is not recorded in this document.

## Package 16A status

Package 16A added typed Drawing Table Eyes:

```json
{"command":"get_parts_lists"}
{"command":"get_revision_tables"}
```

Status:

- `get_revision_tables`: VERIFIED.
- `get_parts_lists`: VERIFIED for reading Autodesk Inventor API objects exposed through `Sheet.PartsLists`.

Important distinction:

The visible GOST table named like a parts list in the tested document is not an Inventor `Sheet.PartsLists` object.

## Package 16C status

Package 16C added Drawing Table Collections Diagnostics:

```json
{"command":"get_drawing_table_collections"}
```

Status: VERIFIED.

Confirmed on `Sborka1.idw`, `Лист:1`:

- `CustomTables.rawCount = 1`, `CustomTables.itemCount = 1`;
- `HoleTables.rawCount = 0`, `HoleTables.itemCount = 0`;
- `PartsLists.rawCount = 0`, `PartsLists.itemCount = 0`;
- `RevisionTables.rawCount = 1`, `RevisionTables.itemCount = 1`;
- diagnostics were empty.

Confirmed cause of the visual mismatch:

The visible table `Спецификация` / `Список деталей` in the tested GOST drawing is represented by Inventor API as:

```text
collection: Sheet.CustomTables
objectType: kCustomTableObject
title: "Список деталей по ГОСТ: Sborka1.iam"
rowCount: 15
columnCount: 7
```

It is not a `Sheet.PartsLists` object.

## Package 17A status

Package 17A added a typed CustomTables Eye:

```json
{"command":"get_custom_tables"}
```

Status: VERIFIED.

Confirmed scope:

- reads `Sheet.CustomTables`;
- reads `CustomTable` metadata;
- reads reference keys;
- reads `Columns` / `Column`;
- reads `Rows` / `Row`;
- reads `Cell` values;
- reads merged cell ranges through `MergedCells`;
- preserves diagnostics for getter, count, enumeration, and property failures;
- does not classify a table as specification, BOM, GOST, or PartsList.

Inventor API distinction:

- CustomTables are `Sheet.CustomTables`.
- CustomTables are not `Sheet.PartsLists`.
- The runtime reads facts from Inventor API only; external LLM decides how to interpret those facts.

## Package 18A status

Package 18A added Drawing Text Objects Eye:

```json
{"command":"get_drawing_text_objects"}
```

Status: VERIFIED.

Confirmed scope:

- reads `Sheet.DrawingNotes.GeneralNotes`;
- reads `Sheet.DrawingNotes.LeaderNotes`;
- reads `Sheet.DrawingNotes.HoleThreadNotes`;
- reads `Sheet.DrawingNotes.BendNotes`;
- reads `Sheet.DrawingNotes.ChamferNotes`;
- reads `Sheet.DrawingNotes.PunchNotes`;
- reads `Sheet.Sketches` / `DrawingSketch.TextBoxes`;
- reads `Sheet.SketchedSymbols`;
- returns Inventor API facts, reference keys where available, selector snapshots, and diagnostics.

Explicit non-scope:

- no semantic text analysis;
- no GOST interpretation;
- no TT/TU recognition;
- no engineering conclusions.

## Package 19A status

Package 19A added Feature Control Frames Eye:

```json
{"command":"get_feature_control_frames"}
```

Status: VERIFIED.

Confirmed scope:

- reads `Sheet.FeatureControlFrames`;
- reads `FeatureControlFrame` metadata;
- reads `FeatureControlFrameRows`;
- returns tolerance fields as raw Inventor API strings;
- returns datum reference fields as raw Inventor API strings;
- returns reference keys where available;
- returns selector snapshots and diagnostics.

Explicit non-scope:

- no tolerance interpretation;
- no GOST validation;
- no GD&T semantic analysis;
- no engineering conclusions.

## Package 20A status

Package 20A added Surface Texture Symbols Eye:

```json
{"command":"get_surface_texture_symbols"}
```

Status: VERIFIED.

Confirmed scope:

- reads `Sheet.SurfaceTextureSymbols`;
- reads `SurfaceTextureSymbol` metadata;
- reads position;
- reads layer;
- reads style;
- reads leader metadata;
- reads roughness fields;
- reads production fields;
- reads sampling fields;
- reads definition data;
- returns reference keys where available;
- returns selector snapshots and diagnostics.

Explicit non-scope:

- no roughness interpretation;
- no GOST validation;
- no surface texture semantic analysis;
- no engineering conclusions.

## Verified eyes

Major verified read areas include:

- active document;
- sheets and drawing views;
- drawing-view relationships;
- annotation summary;
- `Sheet.PartsLists` typed Eye;
- `Sheet.RevisionTables` typed Eye;
- drawing table collection diagnostics;
- `Sheet.CustomTables` detailed typed Eye;
- drawing text objects;
- feature control frames;
- surface texture symbols;
- view and curve geometry;
- model feature tree;
- feature details;
- parameters;
- surface bodies;
- faces and edges;
- sketches, sketch geometry, constraints, and dimensions;
- work features;
- assembly summary;
- assembly occurrences;
- assembly constraints;
- assembly BOM;
- assembly referenced documents.

## Known gaps

- Typed detailed Eye for `Sheet.HoleTables` is not implemented yet.
- Drawing Text semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- GD&T semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- Surface texture semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Do not describe GOST `CustomTable` objects as `PartsList` objects.

## Next task

Next capability check:

```text
Capability Check - Welding Symbols Eye
```

Goal:

- select the next capability boundary;
- run an audit before implementation;
- keep Runtime limited to Eyes and atomic Hands.
