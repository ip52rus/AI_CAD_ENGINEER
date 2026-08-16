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

Current checkpoint after End-to-End Model Eyes hardening:

```text
v0.55 support active part model eyes
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

Live command audit after Package 59A checkpoint:

```text
211 registered JSON commands
211 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Always recalculate from the live repository before relying on these numbers.

## End-to-End Blocker 01 checkpoint

During the first ESKD end-to-end test, `get_model_feature_tree` was fixed and
live-verified for an active `PartDocument`.

Verified command:

```json
{"command":"get_model_feature_tree"}
```

Live validation on active `вал тестовый.ipt` returned `success = true`,
`documentType = kPartDocumentObject`, `source = activePartDocument`,
`featureCount = 9`, and factual feature-tree data.

The active part path is:

```text
Application.ActiveDocument
-> kPartDocumentObject
-> ModelFeatureReadSupport.ReadFeatureTree(...)
-> PartDocument.ComponentDefinition.Features
```

Existing DrawingDocument referenced-model behavior is preserved.

## End-to-End Model Eyes hardening checkpoint

During the first ESKD end-to-end test, the existing model Eyes were hardened
and live-verified to support active `PartDocument` directly where their
semantic purpose is reading model/PartDocument facts.

Verified active PartDocument commands:

```text
get_hole_features
get_thread_features
get_surface_bodies
get_body_faces
get_face_edges
get_feature_details
get_sketches
get_sketch_geometry
get_sketch_constraints
get_sketch_dimensions
get_work_features
```

These commands preserve their existing DrawingDocument referenced-model paths.
No new commands were added. Drawing-context reference Eyes such as
`get_curve_model_reference` and `get_view_model_references` remain
DrawingDocument/DrawingView commands.

## End-to-End Blocker 02 checkpoint

During the first ESKD end-to-end test, `get_model_parameters` was fixed and
live-verified for an active `PartDocument`.

Verified command:

```json
{"command":"get_model_parameters"}
```

Live validation on active `вал тестовый.ipt` returned `success = true` with
active PartDocument parameter data, including factual expressions, values,
units, and tolerances.

The active part path is:

```text
Application.ActiveDocument
-> kPartDocumentObject
-> PartDocument
-> ModelGeometryReadSupport.ReadAllParameters(...)
-> PartDocument.ComponentDefinition.Parameters
```

Existing DrawingDocument referenced-model behavior is preserved.

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

## Package 21A status

Package 21A added Welding Symbols Eye:

```json
{"command":"get_welding_symbols"}
```

Status: VERIFIED.

Confirmed scope:

- reads `Sheet.WeldingSymbols`;
- reads `DrawingWeldingSymbols` collection metadata;
- reads `DrawingWeldingSymbol` metadata;
- reads position;
- reads layer;
- reads style;
- reads leader metadata;
- reads retrieved state and retrieved source metadata where available;
- reads `DrawingWeldingSymbolDefinition`;
- reads `WeldSymbolOne`;
- reads `WeldSymbolTwo`;
- returns reference keys where available;
- returns selector snapshots and diagnostics.

Explicit non-scope:

- no weld interpretation;
- no weld-type engineering classification;
- no GOST validation;
- no welding semantic analysis;
- no engineering conclusions.

## Package 22 status

Package 22 completed the remaining Drawing Symbol Layer Eyes:

```json
{"command":"get_revision_clouds"}
{"command":"get_edge_symbols"}
{"command":"get_transition_symbols"}
```

Status: VERIFIED.

Drawing Symbol Layer verified commands:

```text
get_feature_control_frames
get_surface_texture_symbols
get_welding_symbols
get_revision_clouds
get_edge_symbols
get_transition_symbols
```

Confirmed scope:

- reads `Sheet.FeatureControlFrames`;
- reads `Sheet.SurfaceTextureSymbols`;
- reads `Sheet.WeldingSymbols`;
- reads `Sheet.RevisionClouds`;
- reads `Sheet.EdgeSymbols`;
- reads `Sheet.TransitionSymbols`;
- returns Inventor API facts, reference keys where available, selector snapshots, and diagnostics.

Explicit non-scope:

- no symbol interpretation;
- no GOST validation;
- no engineering analysis;
- no semantic classification of drawing symbols.

## Package 23 status

Package 23 added the first atomic Drawing Generation Hand:

```json
{"command":"create_drawing_document"}
```

Status: VERIFIED.

Confirmed scope:

- requires explicit `templatePath`;
- accepts optional `visible`;
- creates a new drawing document through `Application.Documents.Add`;
- uses `DocumentTypeEnum.kDrawingDocumentObject`;
- returns created `DrawingDocument` metadata including document name, document type, template path, visibility, sheet count, and dirty state.

Explicit non-scope:

- no automatic template selection;
- no drawing generation scenario;
- no view creation;
- no dimension creation;
- no title block logic;
- no export;
- no GOST/ESKD interpretation;
- no engineering decisions.

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
- `Sheet.HoleTables` detailed typed Eye;
- drawing text objects;
- feature control frames;
- surface texture symbols;
- welding symbols;
- revision clouds;
- edge symbols;
- transition symbols;
- `create_drawing_document` atomic Hand;
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

- RevisionCloud control-point editing and revision association are not implemented.
- Drawing Generation Hands are still partial outside the verified view/export pipeline; print workflow is not implemented yet.
- Drawing Text semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- GD&T semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- Surface texture semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Welding semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Drawing symbol semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Do not describe GOST `CustomTable` objects as `PartsList` objects.

## Next task

## Package 50A checkpoint

Package 50A complete: detailed HoleTable Eye is VERIFIED.

Verified command:

```json
{"command":"get_hole_tables","sheetName":"Лист:1"}
```

Live Inventor validation used a real manually-created `HoleTable`.

Verified factual coverage includes:

- title, position, origin, rangeBox, parent view / referenced view, `HoleTableType`, style, layer, title/header/data text styles, `ShowTitle`, and HoleTable-specific factual flags;
- rows, columns, cells, `HoleTags`, generic `ReferencedHole` metadata, referenceKey, selectorSnapshot, and propertyDiagnostics;
- row facts including holeTag, cells, referencedHole, height, and count;
- column facts including title, width, propertyType, and unitsFormatting;
- cell facts including text, formattedText, and stackedTextPosition;
- HoleTag facts including text, position, rangeBox, visible, showLeader, layer, and dimensionStyle.

`HoleTable.GetReferenceKey(...)` is exposed using the established Runtime reference-key shape. Live validation returned `byteCount = 62`.

Observed unavailable COM properties such as `DeleteTagsOnRollup`,
`SecondaryTagModifierOnRollup`, and limited `ReferencedHole` metadata such as
`Name` are isolated as property-level diagnostics and do not invalidate the Eye.

Runtime does not decide whether a HoleTable is required, choose the DrawingView,
placement, columns, tags, numbering, or sorting, interpret hole semantics, apply
GOST/ESKD HoleTable rules, edit cells automatically, or modify model holes.

## Package 50B checkpoint

Package 50B complete: native HoleTable lifecycle primitives are VERIFIED.

Verified commands:

```text
get_hole_tables
create_hole_table
move_hole_table
delete_hole_table
```

Verified lifecycle:

```text
get_hole_tables
-> create_hole_table
-> get_hole_tables
-> move_hole_table
-> get_hole_tables
-> delete_hole_table
-> get_hole_tables
```

Live Inventor validation confirmed:

- `create_hole_table` creates a native `HoleTable` from an explicit caller-selected `DrawingView` and explicit placement;
- the created table remains readable through detailed snapshots with rows, columns, cells, HoleTags, referenceKey, and selectorSnapshot;
- `move_hole_table` uses native `HoleTable.Position`; factual position after move was `(12,23)`;
- referenceKey remained stable after move;
- parentView remained `ВИД4`;
- table structure and HoleTags remained associated;
- Runtime performed no tag renumbering or table-content mutation;
- `delete_hole_table` uses native `HoleTable.Delete()`;
- `remainingHoleTableCount = 0`;
- final `get_hole_tables` returned `count = 0`.

Known non-blocking diagnostics remain property-level only: `DeleteTagsOnRollup`
E_FAIL, `SecondaryTagModifierOnRollup` E_FAIL, and limited generic
`ReferencedHole` COM metadata.

Runtime still does not decide whether a HoleTable is needed, choose view or
placement, interpret engineering meaning, apply GOST/ESKD decisions, choose
numbering/tag strategy, sort, format, edit cells, or mutate model holes.

## Package 51A checkpoint

Package 51A complete: native CustomTable lifecycle primitives are VERIFIED.

Verified commands:

```text
get_custom_tables
create_custom_table
move_custom_table
delete_custom_table
```

Verified lifecycle:

```text
get_custom_tables
-> create_custom_table
-> get_custom_tables
-> move_custom_table
-> get_custom_tables
-> delete_custom_table
-> get_custom_tables
```

Live Inventor validation confirmed:

- `create_custom_table` creates a native `CustomTable` with explicit title, placement, row count, column count, and column titles;
- tested table `TEST TABLE` was created at `(10,20)` with `numberOfColumns = 2`, `numberOfRows = 2`, and `columnTitles = ["A", "B"]`;
- factual readback returned title `TEST TABLE`, position `(10,20)`, `rowCount = 2`, `columnCount = 2`, column 1 `A`, column 2 `B`, referenceKey, and selectorSnapshot;
- `move_custom_table` uses native `CustomTable.Position = Point2d` and factual readback confirmed requested movement while preserving identity/referenceKey;
- `delete_custom_table` uses native `CustomTable.Delete()`, returned a deleted snapshot, and final `get_custom_tables` returned `count = 0`.

Current Package 51A creation scope:

- caller-supplied `Contents` are not supported;
- Runtime passes `Type.Missing` for `Contents`, `ColumnWidths`, `RowHeights`, and `MoreInfo`;
- no post-create cell population occurs.

CustomTable cell metadata correction:

- live Inventor data confirmed `Cell.Row` and `Cell.Column` are zero-based for CustomTable cells;
- `CustomTable.Columns` metadata is one-based;
- Runtime preserves raw/native `rowIndex` and `columnIndex` values;
- CustomTable cell column metadata lookup now uses `metadataColumnIndex = native Cell.Column + 1`;
- this correction is scoped only to CustomTable cell metadata lookup and is not applied to PartsList or HoleTable serializers.

Runtime does not decide whether a CustomTable is needed, invent content or
column titles, choose row/column counts or placement, populate cells
automatically, sort, merge, resize automatically, apply GOST/ESKD semantics,
perform engineering calculations, or interpret arbitrary table content.

## Package 52A checkpoint

Package 52A complete: native RevisionTable lifecycle primitives are VERIFIED.

Verified commands:

```text
get_revision_tables
create_revision_table
move_revision_table
delete_revision_table
```

Verified lifecycle:

```text
get_revision_tables
-> create_revision_table
-> get_revision_tables
-> move_revision_table
-> get_revision_tables
-> delete_revision_table
-> get_revision_tables
```

Live Inventor validation confirmed:

- `create_revision_table` calls native `RevisionTables.Add(Point2d)`;
- created table title was `ЖУРНАЛ ИЗМЕНЕНИЙ`;
- initial position matched caller-requested placement;
- referenceKey and selectorSnapshot were present;
- created table had `rowCount = 1`, `columnCount = 5`, and columns `ЗОНА`, `ИЗМ`, `ОПИСАНИЕ`, `ДАТА`, `УТВЕРЖДЕНО`;
- row/cell data was readable;
- Inventor/template behavior generated native revision row content including revision value `1` and date `14.08.2026`;
- Runtime did not generate revision numbering or date content;
- `move_revision_table` uses native `RevisionTable.Position = Point2d`; factual position after move was `(12,18)`;
- same referenceKey was preserved, with unchanged title, structure, revision row/cell content, style/layer, and rotation;
- `delete_revision_table` uses native `RevisionTable.Delete()`;
- deleted table snapshot was returned, `remainingRevisionTableCount = 0`, and final `get_revision_tables` returned `count = 0`.

Observed `MaximumRows` E_FAIL remains a property-level diagnostic and does not invalidate the lifecycle.

Runtime does not invent revision numbers, dates, or descriptions, decide when a
revision is required, edit revision rows, apply revision numbering policy,
create revision clouds automatically, interpret GOST/ESKD revision semantics,
or mutate style/layer automatically.

## Package 53A checkpoint

Package 53A complete: native RevisionCloud lifecycle primitives are VERIFIED.

Verified commands:

```text
get_revision_clouds
create_revision_cloud
move_revision_cloud
delete_revision_cloud
```

Verified lifecycle:

```text
get_revision_clouds
-> create_revision_cloud
-> get_revision_clouds
-> move_revision_cloud
-> get_revision_clouds
-> delete_revision_cloud
-> get_revision_clouds
```

Live Inventor validation confirmed:

- `create_revision_cloud` uses native `RevisionClouds.CreateRevisionCloudDefinition(...)` and `RevisionClouds.Add(...)`;
- Runtime supplied exactly four caller-defined control points;
- native cloud readback returned `controlPointCount = 4`, `inverted = false`, referenceKey, selectorSnapshot, readable definition, and natively inherited layer;
- Inventor generated a native name such as `Пометочное_облако1`;
- Runtime did not generate extra points, reorder points, close or repair topology, choose a layer, or associate the cloud with a revision;
- `move_revision_cloud` uses native `RevisionCloud.Position = Point2d`;
- factual position after move was `(20,18)`;
- same referenceKey was preserved;
- `controlPointCount` remained `4`, with control-point topology/order intact;
- Inventor translated the native cloud/control-point coordinates as part of the `RevisionCloud.Position` mutation;
- Runtime did not mutate individual `RevisionCloudControlPoint.Position` values;
- `delete_revision_cloud` uses native `RevisionCloud.Delete()`;
- deleted cloud snapshot was returned, `remainingRevisionCloudCount = 0`, and final `get_revision_clouds` returned `count = 0`.

Control-point editing remains deferred.

Runtime does not decide whether a revision cloud is required, associate clouds
with revision rows automatically, create revision numbers, interpret revision
semantics, generate cloud geometry automatically, edit control points
automatically, choose layers, apply GOST/ESKD revision policy, or modify
RevisionTables.

## Package 54A checkpoint

Package 54A complete: native EdgeSymbol lifecycle primitives are VERIFIED.

Verified commands:

```text
get_edge_symbols
create_edge_symbol
move_edge_symbol
delete_edge_symbol
```

Verified lifecycle:

```text
get_edge_symbols
-> create_edge_symbol
-> get_edge_symbols
-> move_edge_symbol
-> get_edge_symbols
-> delete_edge_symbol
-> get_edge_symbols
```

Live Inventor validation confirmed:

- `create_edge_symbol` uses native `EdgeSymbols.CreateDefinition(...)` and `EdgeSymbols.Add(...)`;
- verified factual definition values were `valuePositionType = kEdgeSymbolValueNoValues` and `indicationType = kAllEdgesIndicationType`;
- created symbol returned referenceKey, selectorSnapshot, readable native definition, and natively inherited layer/style;
- Runtime did not select drawing geometry, create a `GeometryIntent`, or apply standards semantics;
- Package 54A creation uses explicit caller-supplied `Point2d` leader points only;
- `move_edge_symbol` uses native `EdgeSymbol.Position = Point2d`;
- factual position after move was `(20,18)`;
- same referenceKey was preserved and the definition remained unchanged;
- `delete_edge_symbol` uses native `EdgeSymbol.Delete()`;
- deleted symbol snapshot was returned, `remainingEdgeSymbolCount = 0`, and final `get_edge_symbols` returned `count = 0`.

Package 54A does not implement GeometryIntent attachment, automatic geometry
selection, definition editing after creation, leader editing, layer/style
mutation, automatic placement, or GOST/ESKD interpretation.

## Package 55A checkpoint

Package 55A complete: native TransitionSymbol lifecycle primitives are VERIFIED.

Verified commands:

```text
get_transition_symbols
create_transition_symbol
move_transition_symbol
delete_transition_symbol
```

Verified lifecycle:

```text
get_transition_symbols
-> create_transition_symbol
-> get_transition_symbols
-> move_transition_symbol
-> get_transition_symbols
-> delete_transition_symbol
-> get_transition_symbols
```

Live Inventor validation confirmed:

- `create_transition_symbol` uses native `TransitionSymbols.CreateDefinition(...)`, `TransitionSymbols.Add(...)`, then `createdTransitionSymbol.Position = Point2d(x,y)` as the explicit free-symbol placement step;
- free `kNoAttachmentType` creation requires caller-supplied `x/y` placement;
- caller `leaderPoints` are passed to `TransitionSymbols.Add(...)` but are not treated as factual placement coordinates;
- creation readback returned `position = (20,18)`, `attachmentType = kNoAttachmentType`, `indicationType = kForAllTransitionsSymbolIndication`, referenceKey, selectorSnapshot, readable definition, and natively inherited layer/style;
- Package 55A does not create `GeometryIntent`, drawing-view attachment, or automatic geometry selection;
- valid free `kNoAttachmentType` TransitionSymbols may have `Leader`, `Leader.HasRootNode = false`, and unavailable/null `Leader.AllNodes`;
- leader node count is nullable/unknown when unavailable, no zero is fabricated, and missing leader-node facts are diagnostic only;
- `move_transition_symbol` uses native `TransitionSymbol.Position = Point2d`;
- factual position after move matched the requested coordinates, visible/native symbol movement was confirmed, and referenceKey, attachmentType, definition, and indicationType were preserved;
- `delete_transition_symbol` uses native `TransitionSymbol.Delete()`;
- deleted symbol snapshot was returned, `remainingTransitionSymbolCount = 0`, and final `get_transition_symbols` returned `count = 0`.

Package 55A does not implement GeometryIntent attachment, drawing-view
attachment, edge/face attachment, automatic geometry selection, leader editing,
definition editing after creation, layer/style mutation, or GOST/ESKD
interpretation.

## Package 56A checkpoint

Package 56A complete: native BendNote lifecycle primitives are VERIFIED.

Verified existing Eye and prerequisite Eye enhancement:

```text
get_drawing_text_objects
get_drawing_curves
```

`get_drawing_text_objects` BendNote read coverage is sufficient for lifecycle
verification. `get_drawing_curves` now exposes factual
`DrawingCurve.EdgeType` as `edgeTypeRaw` and `edgeType`, enabling deterministic
caller selection of bend edges such as `kBendUpEdge` / `kBendDownEdge`.

Verified commands:

```text
create_bend_note
move_bend_note
delete_bend_note
```

Verified lifecycle:

```text
get_drawing_curves
-> deterministic bend-edge selection
-> create_bend_note
-> move_bend_note
-> delete_bend_note
```

Live Inventor validation confirmed:

- `create_bend_note` uses native `BendNotes.Add(DrawingCurve, Type.Missing)`;
- validation used `viewName = "ВИД1"`, `curveIndex = 98`, `edgeType = kBendDownEdge`;
- arbitrary non-bend `DrawingCurve` indexes produced native `E_FAIL`, matching Autodesk's bend-edge requirement;
- created BendNote text was native Inventor/template output: `"ВНИЗ 90° R1,5"` with `formattedText = "<BendNote/>"`;
- attached entity was a `GeometryIntent`, attachment point remained on the bend edge, referenceKey was present, and native GOST dimension style was inherited;
- Runtime generated no bend semantics and selected no geometry automatically.

Move semantics:

- mutation uses native `BendNote.Position = Point2d(x,y)`;
- Inventor may natively create/reroute a leader when `BendNote.Position` is set;
- when `Leader.HasRootNode == true` and `Leader.RootNode.Position` is readable, move verification uses `Leader.RootNode.Position` as the effective requested placement;
- otherwise move verification uses factual `BendNote.Position`;
- `BendNote.Position` itself may differ from requested coordinates after native layout;
- Runtime does not reroute or edit the leader.

Live move verified requested `(22,19)` with:

```text
effectivePosition = (22,19)
verificationSource = Leader.RootNode.Position
BendNote.Position after layout = (22.25,19)
```

ReferenceKey, text, and attached bend entity / attachment point were preserved.

`delete_bend_note` uses native `BendNote.Delete()`, returned a deleted BendNote
snapshot, and final remaining BendNote count was `0`.

Package 56A does not implement automatic bend-edge selection, bend geometry
inference, bend angle/radius interpretation, text editing, leader editing,
style/layer mutation, automatic placement, or GOST/ESKD interpretation.

## Package 57A checkpoint

Package 57A complete: native ChamferNote lifecycle primitives are VERIFIED.

Verified existing Eyes:

```text
get_drawing_text_objects
get_drawing_curves
```

`get_drawing_text_objects` ChamferNote read coverage is sufficient for lifecycle
verification. `get_drawing_curves` supplies the caller-visible DrawingCurve facts
used for explicit edge selection.

Verified commands:

```text
create_chamfer_note
move_chamfer_note
delete_chamfer_note
```

Verified lifecycle:

```text
get_drawing_curves
-> explicit two-DrawingCurve selection
-> create_chamfer_note
-> move_chamfer_note
-> delete_chamfer_note
```

Live Inventor validation confirmed:

- `create_chamfer_note` uses native `ChamferNotes.Add(Point2d, ChamferEdgeOne, ChamferEdgeTwo, Type.Missing)`;
- validation used `viewName = "ВИД1"`, `chamferEdgeOneCurveIndex = 9`, and `chamferEdgeTwoCurveIndex = 10`;
- both supplied edges were explicit caller-selected linear `DrawingCurve` objects from the same `DrawingView`;
- created ChamferNote text was native Inventor/template output: `"10 x 45°"` with `formattedText = "<ChamferNote/>"`;
- referenceKey and attachedEntity were present, and native GOST dimension style was inherited;
- Runtime generated no chamfer semantics, performed no chamfer detection, performed no edge-pair search, did not swap order, and did not retry pairs.

Move semantics:

- mutation uses native `ChamferNote.Position = Point2d(x,y)`;
- `x/y` are treated as requested native placement input, not a guaranteed final `ChamferNote.Position` readback contract;
- Inventor may normalize/reflow ChamferNote text placement through native leader/text layout;
- neither `ChamferNote.Position` readback nor `Leader.RootNode.Position` is guaranteed to equal caller-requested coordinates after native layout;
- `move_chamfer_note` returns `requestedPosition`, factual `actualPosition`, and `positionNormalizedByInventor`;
- `positionNormalizedByInventor = true` when factual `actualPosition` differs from requested position by more than `0.0001`;
- normalization is reported factually and is not a failure;
- Runtime does not compensate coordinates and does not mutate leader nodes.

Live move verified requested `(24,20)` with factual resulting
`ChamferNote.Position = (24.381451470168425,18.175)`,
unchanged `Leader.RootNode.Position = (19.825,18.175)`, unchanged
`AttachedEntity.PointOnSheet = (18.75,17.1)`, unchanged text
`"10 x 45°"`, and unchanged referenceKey.

`delete_chamfer_note` uses native `ChamferNote.Delete()`, returned a deleted
snapshot, and final remaining ChamferNote count was `0`.

Package 57A does not implement SketchLine-based ChamferNote creation,
automatic chamfer detection, edge-pair search, pair swapping/retry, chamfer
angle calculation, chamfer distance calculation, note text editing, leader
editing, style/layer mutation, automatic placement, or GOST/ESKD interpretation.

## Package 59A checkpoint

Package 59A complete: native CenterMark and Centerline delete lifecycle
primitives are VERIFIED.

Verified commands:

```text
get_center_marks
delete_center_mark
get_centerlines
delete_centerline
```

Live CenterMark delete validation used existing `create_center_mark` with
`sheetName = "Лист:1"`, `viewName = "ВИД1"`, and `curveIndex = 12`.
Creation succeeded, `get_center_marks` returned `count = 1`, and
`delete_center_mark` returned `success = true`, a readable deleted snapshot,
referenceKey, and `remainingCenterMarkCount = 0`.

Native CenterMark delete mutation:

```text
Centermark.Delete()
```

CenterMark status after Package 59A:

- `get_center_marks` detailed read is sufficient;
- `create_center_mark` already existed and remains sufficient;
- `delete_center_mark` is VERIFIED;
- `move_center_mark` remains deferred because local Inventor Interop confirms `Centermark.Position` is read-only.

Live Centerline delete validation used existing `create_centerline_centered_pattern`.
It created a `Centerline` with `centerlineType = kCenteredPatternCenterlineType`.
`get_centerlines` returned `count = 1`, and `delete_centerline` returned
`success = true`, a readable deleted snapshot, referenceKey, and
`remainingCenterlineCount = 0`.

Native Centerline delete mutation:

```text
Centerline.Delete()
```

Centerline status after Package 59A:

- `get_centerlines` detailed read is sufficient;
- specific creation commands already exist: `create_centerline_bisector` and `create_centerline_centered_pattern`;
- `delete_centerline` is VERIFIED;
- generic `set_centerline_endpoints` is NOT shipped.

Endpoint audit result:

- local Inventor Interop confirms `Centerline.StartPoint` and `Centerline.EndPoint` are writable;
- live testing proved their semantics are `CenterlineType`-dependent;
- for `kCenteredPatternCenterlineType`, requested `(10,10)` and `(20,10)` read back as normalized vectors `(0.7071067811865476,0.7071067811865476)` and `(0.8944271909999159,0.447213595499958)`;
- for `kBisectorCenterlineType`, requested `(10,10)` and `(20,10)` read back as constrained/transformed points `(12.45257603775818,12.623335927389201)` and `(18.504311927996742,12.172299960051632)`;
- therefore a generic absolute-coordinate endpoint mutation contract is deferred.

During Package 59A validation, `get_drawing_curves` received optional
read-only range support:

```json
{
  "command": "get_drawing_curves",
  "sheetName": "Лист:1",
  "viewName": "ВИД1",
  "startIndex": 1,
  "count": 10
}
```

Range support preserves existing behavior when omitted, preserves original
1-based curve indices, returns only the requested slice, exposes
`totalRawCount` / `totalCount`, and performs no filtering or geometry
inference.

Package 59A does not implement CenterMark move, generic Centerline creation,
Centerline endpoint mutation, work-feature Centerline creation, automatic
center placement, geometry inference, style/layer mutation, or GOST/ESKD
interpretation.

## Package 58A checkpoint

Package 58A complete: native PunchNote lifecycle primitives are VERIFIED.

Verified existing Eyes:

```text
get_drawing_curves
get_drawing_text_objects
```

Verified commands:

```text
create_punch_note
move_punch_note
delete_punch_note
```

Verified lifecycle:

```text
get_drawing_curves
-> explicit kPunchUpEdge/kPunchDownEdge DrawingCurve selection
-> create_punch_note
-> move_punch_note
-> delete_punch_note
```

Live Inventor validation confirmed:

- validation used document `500х85х120-1`, sheet `Лист:1`, view `ВИД1`, and `curveIndex = 20`;
- the selected `DrawingCurve.EdgeType` was `kPunchUpEdge` with raw value `82696`;
- `create_punch_note` uses native `Sheet.CreateGeometryIntent(drawingCurve)` followed by `PunchNotes.Add(position, geometryIntent, Type.Missing)`;
- `PunchNotes.Count` changed from `0` to `1`, `createdPunchNoteIndex = 1`, `PunchEdge` was readable, referenceKey was present, and native text was `"ВВЕРХ 270° "`;
- native dimension style was inherited and attachment facts were readable;
- Runtime performed no punch geometry detection, curve search, punch-feature inference, text generation, style selection, or standards interpretation.

Punch curve contract:

- creation accepts only an explicit caller-selected `DrawingCurve` whose factual `EdgeType` is `kPunchUpEdge` or `kPunchDownEdge`;
- known non-punch curves are rejected before `PunchNotes.Add`;
- prerequisite live fixture investigation confirmed the flat-pattern `DrawingView` contained punch curves at indexes `20..41`;
- no weakening of `EdgeType` validation is required.

Move semantics:

- mutation uses native `PunchNote.Position = Point2d(x,y)`;
- `x/y` are requested native placement input, not a guaranteed final exact `PunchNote.Position` readback contract;
- Inventor may normalize/reflow final PunchNote text placement through native annotation layout;
- `Leader.RootNode.Position` is not a requested-placement proxy for PunchNote;
- `move_punch_note` returns `requestedPosition`, factual `actualPosition`, and `positionNormalizedByInventor`;
- `positionNormalizedByInventor = true` when factual `actualPosition` differs from requested position by more than `0.0001`;
- normalization is reported factually and is not a failure;
- Runtime does not compensate coordinates and does not mutate leader nodes.

Live move verified requested `(24,20)` with factual resulting
`PunchNote.Position = (24.47213595499958,18)`,
unchanged `Leader.RootNode.Position = (20,18)`, unchanged
`AttachedEntity.PointOnSheet = (20.869186813661702,15.177104129571754)`,
unchanged `PunchEdge`, unchanged text `"ВВЕРХ 270° "`, and unchanged referenceKey.

`delete_punch_note` uses native `PunchNote.Delete()`, returned a deleted
snapshot with referenceKey and readable PunchEdge/attachedEntity facts, and
final remaining PunchNote count was `0`.

Package 58A does not implement automatic punch detection, geometry inference,
punch feature search, arbitrary `kUnknownEdge` fallback, punch text editing,
leader editing, style/layer mutation, automatic placement, or GOST/ESKD
interpretation.

## Package 42 checkpoint

Package 42 complete: Hole and Thread Annotation Pipeline is VERIFIED.

Verified Package 42A Eye hardening:

- `get_hole_features` preserves existing fields and now exposes expanded factual `HoleFeature` data;
- `HoleFeature` reference keys are returned where available;
- tapped-hole `ThreadInfo` facts are returned where Inventor exposes them;
- optional COM property failures are isolated in `propertyDiagnostics`.

Verified Package 42B Eye:

```json
{"command":"get_thread_features"}
```

`get_thread_features` reads standalone Inventor `ThreadFeature` objects from the
`PartDocument` referenced by an explicit drawing view. Inventor validation
confirmed a standalone external thread:

```text
designation = M15x1.5
threadClass = 6g
referenceKey returned
```

Verified standalone-thread drawing identification:

```text
get_curve_model_reference
curveIndex 21 -> edgeType = kThreadEdge
curveIndex 22 -> edgeType = kThreadEdge
```

Existing `create_hole_thread_note` is VERIFIED as sufficient for standalone
`ThreadFeature` drawing annotation. Inventor generated the native annotation
text from explicit caller-selected thread geometry:

```text
text = M15x1.5 - 6g
isHoleNote = false
attached = true
```

Verified Package 42D Eye hardening:

- `get_hole_thread_notes` no longer fails the whole command when one optional
  COM property throws `E_FAIL`;
- standalone thread note reading returns `success = true`;
- `text = M15x1.5 - 6g`;
- `isHoleNote = false`;
- `attached = true`;
- `referenceKey` present;
- unavailable COM properties such as `Intent` and `RightHandedThread` are
  reported as non-blocking `propertyDiagnostics`.

Verified standalone-thread annotation pipeline:

```text
get_thread_features
-> get_drawing_curves / get_curve_model_reference
-> explicit kThreadEdge selection by external caller
-> create_hole_thread_note
-> Inventor-generated annotation
-> get_hole_thread_notes
```

No dedicated `ThreadFeature` to `DrawingCurve` mapper is currently required:
`get_curve_model_reference` exposes `kThreadEdge` sufficiently for explicit
caller-driven selection.

Runtime boundary:

- Runtime does not parse thread designations;
- Runtime does not perform GOST/ESKD decision logic;
- Runtime does not select curves automatically;
- Runtime does not reconstruct hole/thread note text;
- Runtime does not modify the model;
- Inventor remains responsible for generating native hole/thread note text.

Next capability check:

```text
Capability Audit - General Notes / Leader Notes / technical requirements
```

Goal:

- audit existing Runtime coverage first;
- determine whether General Notes / Leader Notes / technical requirements
  capabilities are already sufficient;
- run an audit before implementation;
- keep Runtime limited to Eyes and atomic Hands.

## Package 43-44 checkpoint

Package 43-44 complete: General Note and Leader Note primitives are VERIFIED.

Verified General Note pipeline:

```text
get_general_notes
-> create_general_note_fitted
-> set_general_note_formatted_text
-> move_general_note
-> delete_general_note
```

Inventor validation confirmed the full lifecycle:

```text
create -> read -> edit -> read -> move -> read -> delete -> read
final get_general_notes count = 0
```

Confirmed GeneralNote facts:

- native fitted `GeneralNote` creation works;
- Inventor GOST text style/layer are used by native/default behavior;
- formatted text editing works;
- movement works;
- deletion works.

Verified Leader Note pipeline:

```text
get_leader_notes / get_drawing_text_objects
-> create_leader_note
-> optional GeometryIntent attachment
-> set_leader_note_formatted_text
-> move_leader_note
-> delete_leader_note
```

Free LeaderNote lifecycle is VERIFIED:

```text
create -> get_leader_notes -> edit -> move -> delete -> get_leader_notes
final get_leader_notes count = 0
```

Confirmed free LeaderNote facts:

- free `LeaderNote` creation works;
- `attachedEntity = null` is a valid factual state;
- null optional `GeometryIntent` is handled without failure;
- edit, move, and delete work.

Observed Inventor API behavior:

- setting `LeaderNote.Position` does not necessarily produce an actual
  `noteAfter.position` exactly equal to requested `x/y`;
- Inventor may constrain or reposition the note according to leader geometry;
- Runtime reports `requestedPosition` and actual `noteAfter.position` without
  compensation.

Attached LeaderNote is VERIFIED with:

```text
viewName = ВИД1
curveIndex = 14
intent = mid
```

Confirmed attached LeaderNote facts:

- `attached = true`;
- `GeometryIntent` creation succeeds;
- `attachmentSelector` is preserved;
- `pointOnSheet = (23.65, 15.6)`, matching the selected DrawingCurve midpoint;
- final leader node is attached to the `GeometryIntent`;
- `get_leader_notes` reads `attachedEntity`;
- `referenceKey` exists;
- property diagnostics are non-blocking.

Runtime boundary:

- Runtime does not generate technical requirement text;
- Runtime does not choose technical requirements;
- Runtime does not number requirements semantically;
- Runtime does not decide GOST/ESKD content;
- Runtime does not automatically choose geometry;
- Runtime does not automatically place annotations;
- Runtime does not interpret drawing meaning.

Next capability check completed by Package 45, Package 46, Package 47, Package
48, Package 49, and Package 50A. Current next capability check:

```text
Capability Audit - HoleTable lifecycle
```

## Package 48 checkpoint

Package 48 complete: generic drawing SketchedSymbol definition discovery and
inserted-symbol primitives are VERIFIED.

Verified commands:

```text
get_sketched_symbol_definitions
create_sketched_symbol
move_sketched_symbol
delete_sketched_symbol
```

Verified definition discovery:

```text
get_sketched_symbol_definitions
-> external caller selects exact definitionName
```

Verified free SketchedSymbol pipeline:

```text
get_sketched_symbol_definitions
-> create_sketched_symbol [SketchedSymbols.Add]
-> get_drawing_text_objects
-> move_sketched_symbol [SketchedSymbol.Position]
-> get_drawing_text_objects
-> delete_sketched_symbol
-> get_drawing_text_objects
```

Verified leader/attached SketchedSymbol pipeline:

```text
get_sketched_symbol_definitions
-> get_drawing_curves
-> create_sketched_symbol [SketchedSymbols.AddWithLeader]
-> optional GeometryIntent attachment
-> get_drawing_text_objects
```

Confirmed native APIs:

```text
DrawingDocument.SketchedSymbolDefinitions
Sheet.SketchedSymbols
SketchedSymbols.Add(...)
SketchedSymbols.AddWithLeader(...)
SketchedSymbol.Position
SketchedSymbol.Delete()
```

Live Inventor validation confirmed free creation with definition
`ГОСТ - Доп. графы 4`, position `(30,20)`, rotation `0`, scale `1`,
referenceKey, readable resultTexts, move to `(32,18)` through
`SketchedSymbol.Position`, and delete with correct remaining count.

Live Inventor validation confirmed leader/attached creation through
`SketchedSymbols.AddWithLeader(...)` using caller Point2d leader points and
`GeometryIntent` appended LAST. The created symbol was visible through
`get_drawing_text_objects`, with `leaderVisible = true`,
`symbolClipping = true`, unique referenceKey, readable resultTexts, and no
blocking diagnostics.

Prompt-status limitation:

- Inventor 2027 `TextBox` Interop exposes `Text` and `FormattedText`.
- No dedicated prompted-entry flag was confirmed.
- Runtime does not infer prompt semantics from `<Prompt>` or formatted text.
- Runtime does not fabricate prompted values.

Runtime does not know what a sketched symbol means, choose symbol definitions,
perform fuzzy definition matching, assign datum/base semantics, generate
GOST/ESKD geometry, select attachment geometry, route leaders automatically,
choose placement, or perform drawing-layout intelligence. External LLM supplies
all definition, value, geometry, and placement choices.

Next capability check completed by Package 49. Current next capability check:

```text
Capability Audit - HoleTable lifecycle
```

## Package 47 checkpoint

Package 47 complete: native drawing Welding Symbol primitives are VERIFIED.

Verified commands:

```text
get_welding_symbols
create_welding_symbol
move_welding_symbol
delete_welding_symbol
```

Verified welding symbol pipeline:

```text
get_welding_symbols
-> create_welding_symbol
-> optional GeometryIntent attachment
-> move_welding_symbol
-> delete_welding_symbol
```

Verified native creation pipeline:

```text
Sheet.WeldingSymbols
-> DrawingWeldingSymbols.CreateDefinitions()
-> DrawingWeldingSymbolDefinitions.Add(definitionIndex)
-> DrawingWeldingSymbolDefinition / WeldSymbolOne / WeldSymbolTwo fields
-> caller Point2d leader points
-> optional GeometryIntent LAST
-> DrawingWeldingSymbols.Add(...)
-> native DrawingWeldingSymbol
-> get_welding_symbols
```

Important Inventor API findings:

- `DrawingWeldingSymbolDefinitions.Add(definitionIndex)` is the confirmed
  working initialization pattern for definitions.
- `Type.Missing` for the `TargetIndex` argument was not usable in live
  Inventor 2027 creation and produced `DrawingWeldingSymbols.Add` `E_FAIL`.
- Leader-based welding symbol movement is performed through
  `DrawingWeldingSymbol.Leader.RootNode.Position`.
- `E_FAIL` from reading properties that are not applicable to a specific weld
  symbol type remains property-level diagnostics and does not mean the
  `DrawingWeldingSymbol` object failed.

Runtime does not decide whether welding is required, choose weld type,
calculate weld size, choose length or pitch, choose arrow/other side semantics,
choose contour, choose process/method, choose field/all-around state, choose
geometry, choose attachment, choose placement, interpret GOST/ESKD/AWS/ISO
welding rules, or modify model weld geometry. External LLM supplies all
engineering decisions.

Next capability check completed by Package 48:

```text
Capability Audit - Drawing Sketched Symbols / Template Symbol Insertion
```

## Package 45 checkpoint

Package 45 complete: native drawing Feature Control Frame primitives are
VERIFIED.

Verified commands:

```text
get_feature_control_frames
create_feature_control_frame
move_feature_control_frame
delete_feature_control_frame
```

Verified free FCF pipeline:

```text
explicit leader points
-> CreateFeatureControlFrameRows
-> rows.Add(...)
-> FeatureControlFrames.Add(...)
-> native Inventor FeatureControlFrame
-> get_feature_control_frames
```

Verified attached FCF pipeline:

```text
explicit DrawingView
-> explicit DrawingCurve
-> Sheet.CreateGeometryIntent(...)
-> caller Point2d leader vertices
-> GeometryIntent LAST
-> FeatureControlFrames.Add(...)
```

Verified structured content:

- `geometricCharacteristic = kPosition`;
- `tolerance = "0.1"`;
- `datumOne = "A"`;
- `datumTwo = "B"`;
- `datumThree = ""`;
- referenceKey present.

Verified Inventor placement behavior:

- `FeatureControlFrame.Position = Point2d` did not move the factual/visible
  leader-based FCF;
- the verified native placement primitive is
  `FeatureControlFrame.Leader.RootNode.Position = Point2d`;
- after the fix, requested `(32,18)` resulted in `frame.position = (32,18)`
  and `leader.rootNodePosition = (32,18)`;
- `get_feature_control_frames` independently confirmed the new position.

Runtime does not choose GD&T characteristic, calculate tolerances, assign
datums, interpret MMC/LMC/RFS, choose geometry, choose placement, or apply
GOST/ESKD engineering logic. External LLM supplies all engineering decisions.

Next capability check completed by Package 48:

```text
Capability Audit - Drawing Sketched Symbols / Template Symbol Insertion
```

## Package 46 checkpoint

Package 46 complete: native drawing Surface Texture Symbol primitives are
VERIFIED.

Verified commands:

```text
get_surface_texture_symbols
create_surface_texture_symbol
move_surface_texture_symbol
delete_surface_texture_symbol
```

Verified free surface texture pipeline:

```text
explicit native roughness fields
-> SurfaceTextureSymbols.Add(...)
-> native SurfaceTextureSymbol
-> get_surface_texture_symbols
```

Verified attached surface texture pipeline:

```text
DrawingView
-> DrawingCurve
-> Sheet.CreateGeometryIntent(...)
-> caller Point2d leader points
-> GeometryIntent LAST
-> SurfaceTextureSymbols.Add(...)
```

Verified native facts:

- `objectType = kSurfaceTextureSymbolObject`;
- `surfaceTextureType = kMaterialRemovalRequiredSurfaceType`;
- `maximumRoughness = "Ra 3.2"`;
- `layDirection = kParallelToPlaneOfProjection`;
- `definition = kSurfaceTextureGOSTDefinitionObject`;
- `style = Шероховатость (ГОСТ)`;
- referenceKey present.

Verified move behavior:

- `SurfaceTextureSymbol.Leader.RootNode.Position` is used when a leader root
  node exists;
- factual post-move state is confirmed by `get_surface_texture_symbols`.

Verified delete behavior:

- `SurfaceTextureSymbol.Delete()` removes the native symbol;
- final `get_surface_texture_symbols` read returned `count = 0`.

Runtime does not decide roughness values, choose Ra/Rz, infer machining
process, choose material-removal requirement, choose geometry, choose
placement, or apply GOST/ESKD semantics. External LLM supplies all engineering
decisions.

Next capability check:

```text
Capability Audit - Datum identifiers / Datum Target Symbols
```

## Package 24 checkpoint

Package 24 complete: Section View Pipeline is VERIFIED.

Verified atomic Hands: `create_drawing_document`, `create_base_view`,
`create_section_line`, and `create_section_view`.

Verified pipeline: `DrawingDocument → Base View → Section Line → Section View`.
The section line is created through `parentView.Sketches.Add()` and explicit
sheet coordinates are converted with `DrawingSketch.SheetToSketchSpace()`;
`DrawingViews.AddSectionView2` then consumes that parent-view-owned sketch.
This ownership requirement was confirmed after `Sheet.Sketches.Add()` caused
Inventor `E_FAIL`.

Inventor PASS, Build PASS, Registry PASS, and runtime pipeline PASS are
recorded for Package 24. Runtime receives all coordinates explicitly and does
not choose placement or direction, analyze the model, make engineering
decisions, or perform GOST analysis.

Next capability check: `create_detail_view` Hand.

## Package 25 checkpoint

Package 25 complete: Detail View Pipeline is VERIFIED.

Verified Hand: `create_detail_view`.

Verified pipeline:

```text
DrawingDocument → Base View → Detail View
```

The Hand uses `DrawingViews.AddDetailView()` and supports only circular detail
fences. Inventor validation confirmed `DetailDrawingView`,
`kDetailDrawingViewType`, scale `2 : 1`, a reference key, `dirty = true`, and
empty diagnostics.

Fence center, radius, scale, position, and parent view are supplied by the
caller. Rectangular fences, automatic fence generation, automatic scale,
automatic positioning, attachment selection, engineering analysis, and GOST
analysis remain outside Runtime.

Next capability check: `create_auxiliary_view` Hand.

## Package 26 checkpoint

Package 26 complete: Auxiliary View Pipeline is VERIFIED.

Verified Drawing View Hands:

```text
create_base_view
create_projected_view
create_section_line
create_section_view
create_detail_view
create_auxiliary_view
```

`create_auxiliary_view` calls `DrawingViews.AddAuxiliaryView()` with an
explicit parent view and orientation curve index. Inventor validation
confirmed `kAuxiliaryDrawingViewType`, parent view `ВИД1`, curve index `22`, a
reference key, `dirty = true`, and empty diagnostics. Inventor adjusted the
requested position `(35, 20)` to approximately `(35, 15)`; this is recorded as
API behavior and is not compensated by Runtime.

Next capability: `add_drawing_view_break`.

## Package 27 checkpoint

Package 27 complete: Drawing View Break Hand is VERIFIED.

Verified command: `add_drawing_view_break`.

Coverage includes `DrawingView.BreakOperations.Add`, horizontal and vertical
orientation, rectangular style, BreakOperation reference keys, factual
post-creation values, and diagnostics. Inventor returned a requested gap of
`1.0` as approximately `1.00076` and requested `numberOfSymbols = 1` as
`numberOfSymbols = 0`; Runtime preserves these actual API values.

Runtime does not choose break location, orientation, style, or geometry and
does not perform engineering or GOST decisions.

Next capability check: Drawing Export Hands.

## Package 28 checkpoint

Package 28 complete: PDF Export Pipeline is VERIFIED.

Verified command: `export_pdf`.

Coverage:

```text
active DrawingDocument
PDF Translator Add-In resolution
TranslationContext
NameValueMap
DataMedium
TranslatorAddIn.SaveCopyAs
overwrite protection
overwrite=true support
output file existence verification
file size / timestamp facts
structured diagnostics
```

Confirmed PDF Translator:

```text
ClientId: {0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}
DisplayName: Translator: PDF / Translyator: PDF
supportsSaveCopyAs = true
translatorAvailable = true
```

Verified runtime tests:

```text
new PDF export: success = true, fileExists = true, fileSizeBytes = 78737
existing destination with overwrite=false: success = false
existing destination with overwrite=true: success = true, existing file overwritten without pre-delete
```

Runtime does not choose output paths, create directories, overwrite
automatically, tune PDF options, modify drawings, regenerate views, or perform
engineering/GOST decisions.

Verified end-to-end pipeline:

```text
DrawingDocument
-> Base/Projected/Section/Detail/Auxiliary Views
-> Drawing View Break
-> PDF Export
```

Next capability check: Drawing Export Hands - DWG / DXF.

## Package 29 checkpoint

Package 29 complete: DWG/DXF Export Pipeline is VERIFIED.

Verified commands:

```json
{"command":"export_dwg"}
{"command":"export_dxf"}
```

DWG verified behavior:

```text
Translator ClientId: {C24E3AC2-122E-11D5-8E91-0010B541CD80}
HasSaveCopyAsOptions = true
Export_Acad_IniFile assigned through NameValueMap.Value setter
caller-supplied DWG INI
direct DWG output
overwrite=false protection
overwrite=true support
output file verification
unsaved DrawingDocument supported
no interactive dialog
```

DXF verified behavior:

```text
Translator ClientId: {C24E3AC4-122E-11D5-8E91-0010B541CD80}
HasSaveCopyAsOptions = true
Export_Acad_IniFile assigned through NameValueMap.Value setter
caller-supplied DXF INI
direct DXF output when transmittal is disabled
overwrite=false protection
overwrite=true support
output file verification
unsaved DrawingDocument supported
no interactive dialog
```

Important DXF translator behavior:

The public Inventor `exportdxf.ini` contained `USE TRANSMITTAL=Yes` and
therefore produced a ZIP package containing the DXF. Direct DXF output was
verified with a caller-supplied DXF INI where transmittal is disabled. Runtime
does not compensate for or reinterpret transmittal packaging.

Verified Drawing Export Hands:

```text
export_pdf
export_dwg
export_dxf
```

Verified end-to-end pipeline:

```text
DrawingDocument
-> Base/Projected/Section/Detail/Auxiliary Views
-> Drawing View Break
-> PDF/DWG/DXF Export
```

Runtime does not choose output formats automatically, generate INI files,
choose AutoCAD versions, choose mappings/layers, choose transmittal behavior,
create directories, overwrite without explicit permission, or perform
engineering/GOST decisions.

Next capability check: choose the next practical engineering layer.

## Package 30 checkpoint

Package 30 complete: Parts List + Balloon Pipeline is VERIFIED.

Verified commands:

```json
{"command":"create_parts_list"}
{"command":"create_balloon"}
```

Verified pipelines:

```text
DrawingView
-> Parts List

DrawingView geometry
-> GeometryIntent
-> Balloon
```

`create_parts_list` creates exactly one Inventor `Sheet.PartsLists` object from
an explicitly selected existing `DrawingView` and explicit placement point.
It does not create or modify BOM data, choose item numbering automatically,
sort rows, format columns, optimize placement, or perform engineering/GOST
decisions.

`create_balloon` creates exactly one Inventor `Balloon` from an explicitly
selected `DrawingView` curve. Runtime creates `Sheet.CreateGeometryIntent` for
the selected `DrawingCurve`, builds the Inventor `LeaderPoints`
`ObjectCollection` from caller-supplied sheet points followed by the
`GeometryIntent`, and calls `Sheet.Balloons.Add`. It does not choose geometry,
choose balloon placement, optimize leaders, renumber items, modify BOM data,
or perform engineering/GOST decisions.

Next capability check: choose the next practical engineering layer.

## Package 31-36 checkpoint

Package 31-36 complete: Drawing Dimensions Creation and Editing Pipeline is
VERIFIED.

Verified creation Eyes/Hands:

```json
{"command":"create_angular_dimension"}
{"command":"create_ordinate_dimension"}
{"command":"get_drawing_view_origin_indicator"}
{"command":"create_drawing_view_origin_indicator"}
{"command":"create_baseline_dimension"}
{"command":"create_chain_dimension"}
```

Verified general dimension editing Hands:

```json
{"command":"set_general_dimension_formatted_text"}
{"command":"set_general_dimension_hide_value"}
{"command":"set_general_dimension_precision"}
{"command":"set_general_dimension_model_value_override"}
{"command":"clear_general_dimension_model_value_override"}
{"command":"set_general_dimension_style"}
{"command":"set_general_dimension_layer"}
```

Ordinate dimension pipeline:

```text
DrawingView geometry
-> GeometryIntent
-> DrawingView.CreateOriginIndicator(...)
-> OrdinateDimensions.Add(...)
```

Baseline and chain dimensions were manually verified with three explicit
`GeometryIntent` selectors and created two dimensions each.

Dimension editing Hands operate only on explicitly selected
`GeneralDimension` objects. Runtime does not choose geometry, placement,
style, layer, tolerance, precision, text, or model-value override values.

Runtime boundary remains unchanged:

- no automatic geometry selection;
- no automatic dimension placement;
- no automatic style/layer selection;
- no tolerance decisions;
- no layout optimization;
- no engineering decisions;
- no GOST/ESKD reasoning inside Runtime.

Next capability check:

```text
Capability Audit - General Dimension Tolerance capabilities
```

## Package 37-39 checkpoint

Package 37-39 complete: General Dimension Tolerance Pipeline is VERIFIED.

Verified Eye:

```json
{"command":"get_general_dimension_tolerance"}
```

Verified Hands:

```json
{"command":"set_general_dimension_tolerance_default"}
{"command":"set_general_dimension_tolerance_basic"}
{"command":"set_general_dimension_tolerance_reference"}
{"command":"set_general_dimension_tolerance_symmetric"}
{"command":"set_general_dimension_tolerance_deviation"}
{"command":"set_general_dimension_tolerance_limits"}
{"command":"set_general_dimension_tolerance_fits"}
```

Confirmed scope:

- reads `GeneralDimension.Tolerance` facts;
- reads tolerance type, upper/lower values, hole tolerance, and shaft tolerance;
- calls `Tolerance.SetToDefault()`;
- calls `Tolerance.SetToBasic()`;
- calls `Tolerance.SetToReference()`;
- calls `Tolerance.SetToSymmetric(...)`;
- calls `Tolerance.SetToDeviation(...)`;
- calls `Tolerance.SetToLimits(...)`;
- calls `Tolerance.SetToFits(...)`;
- returns before/after tolerance state, reference keys where available, and diagnostics.

Verified fits behavior:

```text
dimensionIndex = 2
toleranceType = limits_fits_stacked
holeTolerance = H7
shaftTolerance = g6
result toleranceType = kLimitsFitsStackedTolerance
diagnostics = []
```

Observed Inventor API behavior:

After `Tolerance.SetToDefault()`, Inventor changes `ToleranceType` to
`kDefaultTolerance` and resets `Upper` / `Lower` to `0`, but previously assigned
`HoleTolerance` / `ShaftTolerance` strings may remain readable on the
`Tolerance` object. Runtime records this fact and does not compensate or clear
those strings.

Runtime boundary:

- caller selects the dimension;
- caller selects the tolerance mode;
- caller supplies numeric tolerance values;
- caller supplies fit strings;
- Runtime performs no unit conversion;
- Runtime performs no sign normalization;
- Runtime performs no upper/lower reordering;
- Runtime performs no fit validation or engineering selection;
- Runtime performs no GOST/ESKD tolerance decisions.

Next capability check:

```text
Capability Audit - next practical drawing engineering layer
```

## Package 40-41 checkpoint

Package 40-41 complete: Center Mark and Centerline Pipeline is VERIFIED.

Verified Eye extensions:

```json
{"command":"get_center_marks"}
{"command":"get_centerlines"}
```

Confirmed additions:

- `get_center_marks` returns `referenceKey` for `Centermark` items where available;
- `get_centerlines` returns `referenceKey` for `Centerline` items where available;
- reference-key extraction diagnostics are non-blocking property diagnostics.

Verified Hands:

```json
{"command":"create_center_mark"}
{"command":"create_centerline_bisector"}
{"command":"create_centerline_centered_pattern"}
```

Verified centered-pattern pipeline:

```text
DrawingView drawing curves
-> explicit pattern-center GeometryIntent
-> explicit member GeometryIntents
-> ObjectCollection
-> Centerlines.AddCenteredPattern
-> Centerline
```

Inventor validation for `create_centerline_centered_pattern` confirmed:

- `success = true`;
- `centerlineType = kCenteredPatternCenterlineType`;
- `geometryType = kCircleCurve`;
- `visible = true`;
- `attached = true`;
- `referenceKey` returned;
- `get_centerlines` reads the created object;
- `patternCenter` is present;
- circular centerline is visually correct in Inventor.

Runtime does:

- expose factual drawing geometry;
- resolve caller-supplied sheet/view/curve selectors;
- create explicit `GeometryIntent` objects;
- execute atomic Inventor API operations;
- return factual resulting state and reference keys.

Runtime does not:

- detect holes automatically;
- detect bolt-circle patterns;
- select pattern centers;
- select member holes;
- infer symmetry;
- reorder geometry;
- choose annotations based on engineering meaning;
- make GOST/ESKD decisions;
- optimize annotation or layout placement.

Deferred capabilities:

- generic `create_centerline`;
- `Centerlines.AddByWorkFeature` centerline;
- delete centerline / delete center mark commands.

Next capability check:

```text
Capability Audit - Hole / Thread annotation capabilities
```
