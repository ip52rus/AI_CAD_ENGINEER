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

Current checkpoint after the Package 37-39 documentation sync:

```text
v0.32 complete general dimension tolerance pipeline
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

Live command audit after Package 37-39 checkpoint:

```text
152 registered JSON commands
152 unique registered JSON commands
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

- Typed detailed Eye for `Sheet.HoleTables` is not implemented yet.
- Drawing Generation Hands are still partial outside the verified view/export pipeline; print workflow is not implemented yet.
- Drawing Text semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- GD&T semantic analysis is not implemented in Runtime and must remain outside the C# layer.
- Surface texture semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Welding semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Drawing symbol semantic interpretation is not implemented in Runtime and must remain outside the C# layer.
- Do not describe GOST `CustomTable` objects as `PartsList` objects.

## Next task

Next capability check:

```text
Capability Audit - next practical drawing engineering layer
```

Goal:

- determine existing coverage before proposing implementation;
- run an audit before implementation;
- keep Runtime limited to Eyes and atomic Hands.

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
