# AI CAD ENGINEER - CHAT HANDOFF

## Current State

- Branch: `cleanup/legacy-architecture`.
- Current checkpoint: `v0.62 add detail view annotation text control`.
- Latest commit after checkpoint commit: see `git log -1 --oneline --decorate`.
- Latest completed checkpoint: Package 65A - detail view annotation text control.
- Registry after Package 65A: `215 registered / 215 unique`, `0` duplicate command names.
- Single-command automation is available via `AI_CAD_ENGINEER.exe --json-file "<command.json>"`; `--json-file` is recommended for PowerShell/automation.
- Single-command mode writes exactly one complete JSON response to stdout, uses deterministic exit codes, reuses `InventorCommandDispatcher`, and is attach-only.
- Single-command Inventor attach uses `CLSIDFromProgIDEx("Inventor.Application")`, fallback `CLSIDFromProgID`, then `oleaut32!GetActiveObject`; the program entrypoint is `[STAThread]`.
- Single-command mode does not call `Activator.CreateInstance` and does not launch Inventor. Autonomous agents must invoke it from a process context with access to the same user/session COM ROT as Inventor; the managed Codex sandbox may not expose that ROT.
- `capture_drawing_sheet_preview` is live-verified for full-sheet PNG visual QA. It uses `Application.ActiveView.Fit(true)` and `View.SaveAsBitmap(...)`, preserves sheet aspect ratio, performs no image interpretation or document-content mutation, and works through attach-only `--json-file`. Known limitation: active view zoom/pan camera state is touched and not restored.
- `get_drawing_layout_map` is live-verified as a read-only normalized factual sheet-space Eye for external post-content-freeze global layout reasoning. It returns sheet geometry, reserved areas, drawing views, dimensions, notes, tables, center annotations, symbols, common layout envelopes where available, stable reference keys where Inventor exposes them, and isolated diagnostics. Package 65A extends drawing views with `DrawingView.ViewAnnotation` and detail-definition facts. It performs no collision detection, layout scoring, automatic placement, annotation optimization, visual interpretation, or ESKD/GOST judgment. Limitations: hole/thread note leader geometry is not always exposed, dimension parent-view linkage remains partial, view label bounds are not reliably available, some symbol subtypes may lack bounds, and `DrawingViewAnnotation.RangeBox` / referenceKey may be unavailable.
- `move_drawing_view_annotation_text` is live-verified as an atomic Hand for moving one explicit drawing-view annotation text slot. It sets `DrawingViewAnnotation.TextPosition` for `textSlot="primary"` or `SecondTextPosition` for `textSlot="second"`. Live validation on detail view `РЕЦЕСС` moved the visible source-side primary text `(8,13.4) -> (5,13.4)`, restored it to `(8,13.4)`, and preserved the detail fence, detail view position, source/parent view, and source IPT `dirty=false`.
- `capture_model_preview` is live-verified for active `PartDocument` model PNG previews. It captures one requested Inventor/model standard orientation per invocation, uses `Application.ActiveView.Camera` with `Camera.Fit()` and `View.SaveAsBitmap(...)`, restores Eye/Target/UpVector/Perspective/PerspectiveAngle, works through attach-only `--json-file`, does not mutate the model, and performs no visual interpretation. Supported orientations: `front`, `back`, `top`, `bottom`, `left`, `right`, `iso_top_right`, `iso_top_left`, `iso_bottom_right`, `iso_bottom_left`, `current`.
- `get_model_feature_tree` is live-verified for active `PartDocument`; existing DrawingDocument referenced-model behavior is preserved.
- `get_model_parameters` is live-verified for active `PartDocument`; existing DrawingDocument referenced-model behavior is preserved.
- Existing model Eyes for holes, threads, surface bodies, body faces, face edges, feature details, sketches, sketch geometry/constraints/dimensions, and work features are live-verified for active `PartDocument`; DrawingDocument referenced-model paths remain supported. `get_sketches` preserves pre-existing Inventor edit state and derives `isActive` from `Application.ActiveEditObject` identity, not `PlanarSketch.Edit()`.
- Explicit save/export Hands `save_document`, `save_document_as`, `export_pdf`, `export_dwg`, and `export_dxf` use scoped `Application.SilentOperation`, restore the previous value in `finally`, and report dirty snapshots. Live tests produced no modal Save dialog; referenced source IPT remained `dirty=false`; no unrelated open document was saved. `close_document` modal semantics remain unaudited/deferred.
- Working tree is expected to be clean after the v0.62 checkpoint commit.

## Architecture Rules

- Runtime = Eyes + atomic Hands.
- Eyes read Autodesk Inventor facts.
- Hands execute one explicit Autodesk Inventor API action.
- External LLM performs reasoning, interpretation, selection, planning, and engineering decisions.
- Runtime must not restore `EngineeringBrain`, `DrawingManager`, `Planning`, `Decision`, or similar embedded reasoning systems.
- Runtime must not add automatic layout, optimization, semantic classification, GOST validation, or best-choice logic.
- Runtime must not describe GOST `CustomTable` objects as `PartsList` objects.

## Verified Capabilities

- Drawing Sheets.
- Drawing Views.
- Drawing Dimensions.
- PartsLists API: `get_parts_lists` is verified for `Sheet.PartsLists`.
- RevisionTables: `get_revision_tables`.
- RevisionTable lifecycle Hands: `create_revision_table`, `move_revision_table`, `delete_revision_table`.
- Drawing Table Collections: `get_drawing_table_collections`.
- CustomTables Discovery: `get_drawing_table_collections`.
- CustomTables Detailed Reading: `get_custom_tables`.
- CustomTable lifecycle Hands: `create_custom_table`, `move_custom_table`, `delete_custom_table`.
- HoleTables Detailed Reading: `get_hole_tables`.
- HoleTable lifecycle Hands: `create_hole_table`, `move_hole_table`, `delete_hole_table`.
- Drawing Text Objects: `get_drawing_text_objects`.
- Feature Control Frames Eye: `get_feature_control_frames`.
- Surface Texture Symbols: `get_surface_texture_symbols`, `create_surface_texture_symbol`, `move_surface_texture_symbol`, `delete_surface_texture_symbol`.
- Welding Symbols: `get_welding_symbols`, `create_welding_symbol`, `move_welding_symbol`, `delete_welding_symbol`.
- SketchedSymbols: `get_sketched_symbol_definitions`, `create_sketched_symbol`, `move_sketched_symbol`, `delete_sketched_symbol`.
- RevisionClouds: `get_revision_clouds`, `create_revision_cloud`, `move_revision_cloud`, `delete_revision_cloud`.
- EdgeSymbols: `get_edge_symbols`, `create_edge_symbol`, `move_edge_symbol`, `delete_edge_symbol`.
- TransitionSymbols: `get_transition_symbols`, `create_transition_symbol`, `move_transition_symbol`, `delete_transition_symbol`.
- BendNotes: `get_drawing_text_objects`, `get_drawing_curves` with `DrawingCurve.EdgeType`, `create_bend_note`, `move_bend_note`, `delete_bend_note`.
- ChamferNotes: `get_drawing_text_objects`, `get_drawing_curves`, `create_chamfer_note`, `move_chamfer_note`, `delete_chamfer_note`.
- PunchNotes: `get_drawing_text_objects`, `get_drawing_curves`, `create_punch_note`, `move_punch_note`, `delete_punch_note`.
- create_drawing_document Hand: `create_drawing_document`.
- Drawing View Break Hand: `add_drawing_view_break`.
- PDF Export Hand: `export_pdf`.
- DWG Export Hand: `export_dwg`.
- DXF Export Hand: `export_dxf`.
- Drawing sheet preview Eye: `capture_drawing_sheet_preview`.
- Drawing layout map Eye: `get_drawing_layout_map`.
- Detail view annotation text Hand: `move_drawing_view_annotation_text`.
- Model preview Eye: `capture_model_preview`.
- PartsList Eye: `get_parts_lists`.
- PartsList creation Hand: `create_parts_list`.
- PartsList move Hand: `move_parts_list`.
- PartsList delete Hand: `delete_parts_list`.
- Balloon Eye: `get_balloons` with referenceKey support.
- Balloon creation Hand: `create_balloon`.
- Balloon move Hand: `move_balloon`.
- Balloon delete Hand: `delete_balloon`.
- Angular dimension Hand: `create_angular_dimension`.
- Ordinate dimension Hand: `create_ordinate_dimension`.
- DrawingView OriginIndicator Eye: `get_drawing_view_origin_indicator`.
- DrawingView OriginIndicator Hand: `create_drawing_view_origin_indicator`.
- Baseline dimension Hand: `create_baseline_dimension`.
- Chain dimension Hand: `create_chain_dimension`.
- General dimension formatted text Hand: `set_general_dimension_formatted_text`.
- General dimension hide value Hand: `set_general_dimension_hide_value`.
- General dimension precision Hand: `set_general_dimension_precision`.
- General dimension model value override Hand: `set_general_dimension_model_value_override`.
- General dimension clear model value override Hand: `clear_general_dimension_model_value_override`.
- General dimension style Hand: `set_general_dimension_style`.
- General dimension layer Hand: `set_general_dimension_layer`.
- General dimension tolerance Eye: `get_general_dimension_tolerance`.
- General dimension tolerance default Hand: `set_general_dimension_tolerance_default`.
- General dimension tolerance basic Hand: `set_general_dimension_tolerance_basic`.
- General dimension tolerance reference Hand: `set_general_dimension_tolerance_reference`.
- General dimension tolerance symmetric Hand: `set_general_dimension_tolerance_symmetric`.
- General dimension tolerance deviation Hand: `set_general_dimension_tolerance_deviation`.
- General dimension tolerance limits Hand: `set_general_dimension_tolerance_limits`.
- General dimension tolerance fits Hand: `set_general_dimension_tolerance_fits`.
- Center marks Eye: `get_center_marks` with referenceKey support.
- Centerlines Eye: `get_centerlines` with referenceKey support.
- Center mark Hand: `create_center_mark`.
- Centerline bisector Hand: `create_centerline_bisector`.
- Centerline centered-pattern Hand: `create_centerline_centered_pattern`.
- Center mark delete Hand: `delete_center_mark`.
- Centerline delete Hand: `delete_centerline`.
- Hardened HoleFeature Eye: `get_hole_features`.
- Standalone ThreadFeature Eye: `get_thread_features`.
- Hole/thread note Hand: `create_hole_thread_note`.
- Hardened hole/thread note Eye: `get_hole_thread_notes`.
- General note Hand: `create_general_note_fitted`.
- General note edit Hand: `set_general_note_formatted_text`.
- General note move Hand: `move_general_note`.
- General note delete Hand: `delete_general_note`.
- Leader note Hand: `create_leader_note`.
- Leader note edit Hand: `set_leader_note_formatted_text`.
- Leader note move Hand: `move_leader_note`.
- Leader note delete Hand: `delete_leader_note`.
- Feature Control Frame Eye: `get_feature_control_frames`.
- Feature Control Frame Hand: `create_feature_control_frame`.
- Feature Control Frame move Hand: `move_feature_control_frame`.
- Feature Control Frame delete Hand: `delete_feature_control_frame`.
- Surface Texture Symbol Hand: `create_surface_texture_symbol`.
- Surface Texture Symbol move Hand: `move_surface_texture_symbol`.
- Surface Texture Symbol delete Hand: `delete_surface_texture_symbol`.
- Welding Symbol Hand: `create_welding_symbol`.
- Welding Symbol move Hand: `move_welding_symbol`.
- Welding Symbol delete Hand: `delete_welding_symbol`.
- SketchedSymbol definitions Eye: `get_sketched_symbol_definitions`.
- SketchedSymbol Hand: `create_sketched_symbol`.
- SketchedSymbol move Hand: `move_sketched_symbol`.
- SketchedSymbol delete Hand: `delete_sketched_symbol`.

## CenterMark / Centerline Delete Lifecycle

Verified commands:

```text
get_center_marks
delete_center_mark
get_centerlines
delete_centerline
```

`delete_center_mark` uses native `Centermark.Delete()`. Live validation used
existing `create_center_mark` with `sheetName = "Лист:1"`, `viewName = "ВИД1"`,
and `curveIndex = 12`. `get_center_marks` returned `count = 1`;
`delete_center_mark` returned `success = true`, a readable deleted snapshot,
referenceKey, and `remainingCenterMarkCount = 0`.

`delete_centerline` uses native `Centerline.Delete()`. Live validation used
existing `create_centerline_centered_pattern`, producing
`centerlineType = kCenteredPatternCenterlineType`. `get_centerlines` returned
`count = 1`; `delete_centerline` returned `success = true`, a readable deleted
snapshot, referenceKey, and `remainingCenterlineCount = 0`.

CenterMark move is deferred because local Inventor Interop confirms
`Centermark.Position` is read-only.

Generic `set_centerline_endpoints` is not shipped. Local Interop confirms
`Centerline.StartPoint` and `Centerline.EndPoint` are writable, but live
testing proved their semantics are `CenterlineType`-dependent:

- `kCenteredPatternCenterlineType` normalized caller values;
- `kBisectorCenterlineType` constrained/transformed caller values.

Do not treat these setters as a generic absolute sheet-coordinate endpoint
contract.

`get_drawing_curves` supports optional read-only `startIndex` and `count`
range parameters. Omitted parameters preserve prior behavior; ranged reads
preserve original 1-based curve indices, return only the requested slice, and
include `totalRawCount` / `totalCount`.

## PunchNote Lifecycle

Verified commands:

```text
get_drawing_curves
get_drawing_text_objects
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

`create_punch_note` uses native `Sheet.CreateGeometryIntent(drawingCurve)`
followed by `PunchNotes.Add(position, geometryIntent, Type.Missing)`. Live
validation used document `500х85х120-1`, sheet `Лист:1`, view `ВИД1`, and
`curveIndex = 20` with factual `DrawingCurve.EdgeType = kPunchUpEdge` raw
`82696`. Created note facts included native text `"ВВЕРХ 270° "`, readable
`PunchEdge`, readable attachment facts, referenceKey, and natively inherited
dimension style.

`create_punch_note` accepts only explicit caller-selected DrawingCurves whose
factual `EdgeType` is `kPunchUpEdge` or `kPunchDownEdge`. Known non-punch
curves are rejected before `PunchNotes.Add`. Runtime performs no punch
geometry detection, curve search, punch-feature inference, text generation,
style selection, or standards interpretation.

`move_punch_note` mutates only `PunchNote.Position = Point2d(x,y)`. Caller
`x/y` are requested native placement input, not a guaranteed final exact
readback. Inventor may normalize/reflow PunchNote text placement while
preserving identity, `PunchEdge`, attachment, and text. `Leader.RootNode.Position`
is not a requested-placement proxy for PunchNote.

Runtime returns `requestedPosition`, factual `actualPosition`, and
`positionNormalizedByInventor`. Normalization is not failure; Runtime does not
compensate coordinates or mutate leader nodes.

`delete_punch_note` uses native `PunchNote.Delete()` and final readback
returned remaining PunchNote count `0`.

Automatic punch detection, geometry inference, punch feature search, arbitrary
`kUnknownEdge` fallback, punch text editing, leader editing, style/layer
mutation, automatic placement, and GOST/ESKD interpretation remain deferred.

## ChamferNote Lifecycle

Verified commands:

```text
get_drawing_text_objects
get_drawing_curves
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

`create_chamfer_note` uses native
`ChamferNotes.Add(Point2d, ChamferEdgeOne, ChamferEdgeTwo, Type.Missing)`.
Live validation used two explicit caller-selected linear DrawingCurves from
the same DrawingView: `chamferEdgeOneCurveIndex = 9` and
`chamferEdgeTwoCurveIndex = 10`. Created note facts included native text
`"10 x 45°"`, `formattedText = "<ChamferNote/>"`, referenceKey,
attachedEntity, and natively inherited GOST dimension style.

Runtime does not auto-detect chamfers, search connected curves, swap edge
order, retry pairs, calculate chamfer angle, calculate chamfer distance, or
interpret GOST/ESKD semantics.

`move_chamfer_note` mutates only `ChamferNote.Position = Point2d(x,y)`.
Caller `x/y` are requested native placement input, not a guaranteed final
exact readback. Inventor may normalize/reflow ChamferNote text placement while
preserving identity, attachment, and text. Neither `ChamferNote.Position`
readback nor `Leader.RootNode.Position` is guaranteed to equal the requested
point after native layout.

Runtime returns `requestedPosition`, factual `actualPosition`, and
`positionNormalizedByInventor`. Normalization is not failure; Runtime does not
compensate coordinates or mutate leader nodes.

`delete_chamfer_note` uses native `ChamferNote.Delete()` and final readback
returned remaining ChamferNote count `0`.

SketchLine-based ChamferNote creation, automatic chamfer detection, edge-pair
search, pair swapping/retry, angle/distance calculation, text editing, leader
editing, style/layer mutation, automatic placement, and GOST/ESKD
interpretation remain deferred.

## BendNote Lifecycle

Verified commands:

```text
get_drawing_text_objects
get_drawing_curves
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

`get_drawing_curves` now exposes factual `DrawingCurve.EdgeType` as
`edgeTypeRaw` and `edgeType`, including bend-edge values such as
`kBendUpEdge` and `kBendDownEdge`. Native `BendNotes.Add(...)` requires a bend
edge; arbitrary non-bend curves can return `E_FAIL`. Runtime exposes the fact
and does not auto-select or retry geometry.

`create_bend_note` uses native `BendNotes.Add(DrawingCurve, Type.Missing)`.
Live validation used `viewName = "ВИД1"`, `curveIndex = 98`,
`edgeType = kBendDownEdge`, and returned native text `"ВНИЗ 90° R1,5"` with
`formattedText = "<BendNote/>"`, `GeometryIntent` attached entity, preserved
attachment point, referenceKey, and natively inherited GOST dimension style.
Runtime generated no bend semantics.

`move_bend_note` mutates only `BendNote.Position = Point2d(x,y)`. Inventor may
natively create/reroute a leader. If `Leader.HasRootNode == true` and
`Leader.RootNode.Position` is readable, Runtime verifies effective placement
against `Leader.RootNode.Position`; otherwise it verifies against
`BendNote.Position`. `BendNote.Position` may differ from requested coordinates
after native layout. Runtime does not reroute or edit the leader.

`delete_bend_note` uses native `BendNote.Delete()` and final readback returned
remaining BendNote count `0`.

Automatic bend-edge selection, bend geometry inference, bend angle/radius
interpretation, text editing, leader editing, style/layer mutation, automatic
placement, and GOST/ESKD interpretation remain deferred.

## RevisionTable Lifecycle

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

Live Inventor validation confirmed native `RevisionTables.Add(Point2d)`,
caller-requested initial placement, title `ЖУРНАЛ ИЗМЕНЕНИЙ`, referenceKey,
selectorSnapshot, one row, five columns, readable row/cell data, and native
columns `ЗОНА`, `ИЗМ`, `ОПИСАНИЕ`, `ДАТА`, `УТВЕРЖДЕНО`.

Inventor/template behavior generated revision row content including revision
value `1` and date `14.08.2026`. Runtime did not generate revision numbering or
date content.

`move_revision_table` uses `RevisionTable.Position = Point2d` and preserved
referenceKey, title, structure, revision row/cell content, style/layer, and
rotation. `delete_revision_table` uses `RevisionTable.Delete()` and final
`get_revision_tables` returned `count = 0`.

Observed `MaximumRows` E_FAIL remains a property-level diagnostic.

Runtime does not invent revision numbers, dates, or descriptions, decide when a
revision is required, edit revision rows, apply revision numbering policy,
create revision clouds automatically, interpret GOST/ESKD revision semantics,
or mutate style/layer automatically.

## RevisionCloud Lifecycle

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

Live Inventor validation confirmed native
`RevisionClouds.CreateRevisionCloudDefinition(...)` and
`RevisionClouds.Add(...)`, exactly four caller-defined control points,
`controlPointCount = 4`, `inverted = false`, referenceKey, selectorSnapshot,
readable definition, natively inherited layer, and generated native name such
as `Пометочное_облако1`.

Runtime did not generate extra points, reorder points, close or repair
topology, select a layer, or associate the cloud with a revision.

`move_revision_cloud` uses `RevisionCloud.Position = Point2d`. Live validation
confirmed factual position `(20,18)`, same referenceKey, and
`controlPointCount = 4`. Inventor translated the native cloud/control-point
coordinates as part of native `RevisionCloud.Position`; Runtime did not edit
individual `RevisionCloudControlPoint.Position` values.

`delete_revision_cloud` uses `RevisionCloud.Delete()` and final
`get_revision_clouds` returned `count = 0`.

Control-point editing and automatic revision association remain deferred.

## TransitionSymbol Lifecycle

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

Live Inventor validation confirmed native
`TransitionSymbols.CreateDefinition(...)`, `TransitionSymbols.Add(...)`, and
created-object `TransitionSymbol.Position = Point2d(x,y)` for explicit free
symbol placement.

Caller `leaderPoints` supplied to `TransitionSymbols.Add(...)` do not factually
determine free `kNoAttachmentType` TransitionSymbol placement. Runtime requires
explicit caller `x/y` and does not infer placement from leader points.

Valid free `kNoAttachmentType` TransitionSymbols may have `Leader`,
`Leader.HasRootNode = false`, and unavailable/null `Leader.AllNodes`. Missing
leader-node count is nullable/diagnostic only; Runtime does not fabricate zero
and does not fail movement solely because node count is unavailable.

`move_transition_symbol` uses `TransitionSymbol.Position = Point2d`; live
validation confirmed factual position change, visible/native symbol movement,
same referenceKey, unchanged attachmentType, unchanged definition, and unchanged
indicationType.

`delete_transition_symbol` uses `TransitionSymbol.Delete()` and final
`get_transition_symbols` returned `count = 0`.

GeometryIntent attachment, drawing-view attachment, edge/face attachment,
automatic geometry selection, definition editing after creation, leader editing,
layer/style mutation, automatic placement, and GOST/ESKD interpretation remain
deferred.

## EdgeSymbol Lifecycle

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

Live Inventor validation confirmed native `EdgeSymbols.CreateDefinition(...)`
and `EdgeSymbols.Add(...)`, factual definition values
`valuePositionType = kEdgeSymbolValueNoValues` and
`indicationType = kAllEdgesIndicationType`, referenceKey, selectorSnapshot,
readable native definition, and natively inherited layer/style.

Package 54A creation uses explicit caller-supplied `Point2d` leader points only.
Runtime did not select drawing geometry, create a `GeometryIntent`, or apply
standards semantics.

`move_edge_symbol` uses `EdgeSymbol.Position = Point2d`; live validation
confirmed factual position `(20,18)`, same referenceKey, and unchanged
definition. `delete_edge_symbol` uses `EdgeSymbol.Delete()` and final
`get_edge_symbols` returned `count = 0`.

GeometryIntent attachment, automatic geometry selection, definition editing
after creation, leader editing, layer/style mutation, automatic placement, and
GOST/ESKD interpretation remain deferred.

## CustomTable Lifecycle

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

Live Inventor validation confirmed explicit native CustomTable creation with
title `TEST TABLE`, position `(10,20)`, two rows, two columns, column 1 `A`,
column 2 `B`, referenceKey, and selectorSnapshot. `move_custom_table` uses
`CustomTable.Position = Point2d` and preserved identity/referenceKey.
`delete_custom_table` uses `CustomTable.Delete()` and final
`get_custom_tables` returned `count = 0`.

Package 51A intentionally does not support caller-supplied `Contents`.
Runtime passes `Type.Missing` for `Contents`, `ColumnWidths`, `RowHeights`, and
`MoreInfo`, and performs no post-create cell population.

CustomTable cell metadata correction is verified: `Cell.Row` and `Cell.Column`
are zero-based for live CustomTable cells, while `CustomTable.Columns` metadata
is one-based. Runtime preserves raw `rowIndex` and `columnIndex`, but resolves
cell metadata with `metadataColumnIndex = native Cell.Column + 1` only for
CustomTable cells.

Runtime does not decide whether a CustomTable is needed, invent content or
column titles, choose row/column counts or placement, populate cells
automatically, sort, merge, resize automatically, apply GOST/ESKD semantics, or
interpret arbitrary table content.

## HoleTable Detailed Eye

Verified command:

```text
get_hole_tables
```

Live Inventor validation used a real manually-created `HoleTable`.

Verified factual coverage includes title, position, origin, rangeBox, parent
view / referenced view, `HoleTableType`, style, layer, title/header/data text
styles, `ShowTitle`, HoleTable-specific factual flags, rows, columns, cells,
`HoleTags`, generic `ReferencedHole` metadata, referenceKey, selectorSnapshot,
and propertyDiagnostics.

Rows expose holeTag, cells, referencedHole, height, and count. Columns expose
title, width, propertyType, and unitsFormatting. Cells expose text,
formattedText, and stackedTextPosition. HoleTags expose text, position,
rangeBox, visible, showLeader, layer, and dimensionStyle where Inventor returns
them.

`HoleTable.GetReferenceKey(...)` is exposed using the established Runtime
reference-key shape. Live validation returned `byteCount = 62`.

Unavailable COM properties such as `DeleteTagsOnRollup`,
`SecondaryTagModifierOnRollup`, and limited `ReferencedHole` metadata such as
`Name` are property-level diagnostics only.

Runtime does not decide whether a HoleTable is required, choose the DrawingView,
placement, columns, tags, numbering, or sorting, interpret hole semantics, apply
GOST/ESKD HoleTable rules, edit cells automatically, or modify model holes.

## HoleTable Lifecycle

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

Live Inventor validation confirmed native creation from an explicit
caller-selected DrawingView and placement, `HoleTable.Position` movement to
`(12,23)`, stable referenceKey after move, unchanged parentView `ВИД4`,
readable rows/columns/cells/HoleTags after move, `HoleTable.Delete()`, and
final `get_hole_tables` count `0`.

Runtime performs no tag renumbering, table-content mutation, sorting,
formatting, GOST/ESKD logic, view selection, or automatic placement.

## PartsList Lifecycle

Verified PartsList commands:

```text
get_parts_lists
create_parts_list
move_parts_list
delete_parts_list
```

Verified lifecycle:

```text
get_parts_lists
-> create_parts_list
-> get_parts_lists
-> move_parts_list
-> get_parts_lists
-> delete_parts_list
-> get_parts_lists
```

Verified facts:

- `move_parts_list` uses native `PartsList.Position = Point2d`.
- Factual resulting position was `(32,18)`.
- Same PartsList identity/referenceKey was preserved.
- Rows, columns, and cell values were unchanged.
- Referenced document/view facts were unchanged.
- `delete_parts_list` uses native `PartsList.Delete()`.
- Deleted PartsList snapshot was preserved.
- `remainingPartsListCount = 0`.
- Final `get_parts_lists` returned `count = 0`.

Runtime does not decide PartsList placement, sort rows semantically, renumber
item numbers, edit BOM-derived cells, override BOM facts, modify visibility
automatically, modify BOMView or assembly BOM, export PartsLists unless
explicitly audited later, or apply GOST/ESKD table-placement logic.

## Balloon Lifecycle

Verified Balloon commands:

```text
get_balloons
create_balloon
move_balloon
delete_balloon
```

Verified pipeline:

```text
get_drawing_curves
-> create_balloon
-> get_balloons
-> move_balloon
-> get_balloons
-> delete_balloon
-> get_balloons
```

Verified facts:

- `get_balloons` exposes native `Balloon.GetReferenceKey(...)` output.
- Created balloon was `kBalloonObject`, attached to parent view `ВИД1`.
- Created position was `(30,20)`.
- Move requested `(32,18)` and factual readback reported `Balloon.Position = (32,18)` and `Leader.RootNode.Position = (32,18)`.
- Same referenceKey was preserved after move.
- `value = "1"` and `itemNumber = "1"` were unchanged.
- Geometry attachment was preserved.
- Delete returned deleted balloon snapshot and `remainingBalloonCount = 0`.
- Final `get_balloons` returned `count = 0`.

Runtime does not choose which components require balloons, choose balloon
placement, route leaders, renumber/sort BOM, modify `BalloonValueSet.Value`,
`BalloonValueSet.OverrideValue`, item numbering, PartsLists, assembly/model
structure, or apply GOST/ESKD layout logic.

## SketchedSymbol Primitives

Verified SketchedSymbol commands:

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

Verified free pipeline:

```text
get_sketched_symbol_definitions
-> create_sketched_symbol [SketchedSymbols.Add]
-> get_drawing_text_objects
-> move_sketched_symbol [SketchedSymbol.Position]
-> get_drawing_text_objects
-> delete_sketched_symbol
-> get_drawing_text_objects
```

Verified leader/attached pipeline:

```text
get_sketched_symbol_definitions
-> get_drawing_curves
-> create_sketched_symbol [SketchedSymbols.AddWithLeader]
-> optional GeometryIntent attachment
-> get_drawing_text_objects
```

Inventor 2027 `TextBox` Interop exposes `Text` and `FormattedText`, but no
dedicated prompted-entry flag was confirmed. Runtime does not infer prompt
semantics from `<Prompt>` or formatted text and does not fabricate prompted
values.

Runtime does not know what a sketched symbol means, choose symbol definitions,
perform fuzzy definition-name matching, assign datum/base semantics, generate
GOST/ESKD geometry, select attachment geometry, route leaders automatically,
choose placement, or perform drawing-layout intelligence.

## Welding Symbol Primitives

Verified Welding Symbol commands:

```text
get_welding_symbols
create_welding_symbol
move_welding_symbol
delete_welding_symbol
```

Verified pipeline:

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
  working definition initialization pattern.
- `Type.Missing` for `TargetIndex` was not usable in live Inventor 2027 and
  produced `DrawingWeldingSymbols.Add` `E_FAIL`.
- Leader-based welding symbol movement uses
  `DrawingWeldingSymbol.Leader.RootNode.Position`.
- `E_FAIL` while reading properties not applicable to a specific weld symbol
  type remains property-level diagnostics and does not mean the object failed.

Runtime does not decide whether welding is required, choose weld type,
calculate weld size, choose length/pitch, choose arrow/other side semantics,
choose contour/process/method, choose field/all-around state, choose geometry,
attachment, or placement, interpret GOST/ESKD/AWS/ISO welding rules, or modify
model weld geometry.

## Surface Texture Symbol Primitives

Verified Surface Texture Symbol commands:

```text
get_surface_texture_symbols
create_surface_texture_symbol
move_surface_texture_symbol
delete_surface_texture_symbol
```

Verified free pipeline:

```text
explicit native roughness fields
-> SurfaceTextureSymbols.Add(...)
-> native SurfaceTextureSymbol
-> get_surface_texture_symbols
```

Verified attached pipeline:

```text
DrawingView
-> DrawingCurve
-> Sheet.CreateGeometryIntent(...)
-> caller Point2d leader points
-> GeometryIntent LAST
-> SurfaceTextureSymbols.Add(...)
```

Verified native facts include `kSurfaceTextureSymbolObject`,
`kMaterialRemovalRequiredSurfaceType`, maximum roughness `"Ra 3.2"`,
`kParallelToPlaneOfProjection`, `kSurfaceTextureGOSTDefinitionObject`,
style `Шероховатость (ГОСТ)`, and referenceKey.

Move is verified through `SurfaceTextureSymbol.Leader.RootNode.Position` when
a leader root exists. Delete is verified through
`SurfaceTextureSymbol.Delete()`, with final read `count = 0`.

Runtime does not decide roughness values, choose Ra/Rz, infer machining
process, choose material-removal requirement, choose geometry, choose
placement, or apply GOST/ESKD semantics. External LLM supplies all engineering
decisions.

## Feature Control Frame Primitives

Verified Feature Control Frame commands:

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

Verified Inventor placement behavior: for leader-based drawing
`FeatureControlFrame`, setting `FeatureControlFrame.Position` may not move the
object. The verified native placement primitive is
`FeatureControlFrame.Leader.RootNode.Position`.

Runtime does not choose GD&T characteristics, calculate tolerances, assign
datums, interpret MMC/LMC/RFS, choose geometry or placement, or apply
GOST/ESKD engineering logic.

## General and Leader Note Primitives

Verified General Note pipeline:

```text
get_general_notes
-> create_general_note_fitted
-> set_general_note_formatted_text
-> move_general_note
-> delete_general_note
```

Manual Inventor validation confirmed:

- creation works;
- Inventor GOST text style/layer are used by native/default behavior;
- formatted text editing works;
- movement works;
- deletion works;
- final `get_general_notes` count is `0`.

Verified Leader Note pipeline:

```text
get_leader_notes / get_drawing_text_objects
-> create_leader_note
-> optional GeometryIntent attachment
-> set_leader_note_formatted_text
-> move_leader_note
-> delete_leader_note
```

Free LeaderNote lifecycle is verified. `attachedEntity = null` is a valid
factual state for a free LeaderNote and null optional `GeometryIntent` is
handled without failure.

Attached LeaderNote is verified with:

```text
viewName = ВИД1
curveIndex = 14
intent = mid
```

Confirmed:

- `attached = true`;
- `GeometryIntent` creation succeeds;
- `attachmentSelector` is preserved;
- `pointOnSheet = (23.65, 15.6)`, matching the selected DrawingCurve midpoint;
- final leader node is attached to the `GeometryIntent`;
- `get_leader_notes` reads `attachedEntity`;
- `referenceKey` exists;
- property diagnostics are non-blocking.

Observed Inventor API behavior: setting `LeaderNote.Position` may not produce
an actual `noteAfter.position` exactly equal to the requested coordinates
because Inventor may constrain/reposition the note according to leader
geometry. Runtime reports requested and actual values without compensation.

Runtime does not generate technical requirement text, choose technical
requirements, number requirements semantically, decide GOST/ESKD content,
automatically choose geometry, automatically place annotations, or interpret
drawing meaning.

## Hole and Thread Annotation Pipeline

Verified commands and Eye hardening:

- `get_hole_features`
- `get_thread_features`
- `create_hole_thread_note`
- `get_hole_thread_notes`

Package 42A verified `get_hole_features` hardening:

- expanded factual `HoleFeature` fields;
- `HoleFeature` referenceKey support;
- tapped-hole `ThreadInfo` facts;
- property-level diagnostics for optional COM property failures.

Package 42B verified `get_thread_features` for standalone Inventor
`ThreadFeature` facts from the `PartDocument` referenced by an explicit
`DrawingView`. Inventor validation read a real external thread:

```text
designation = M15x1.5
threadClass = 6g
referenceKey returned
```

Standalone thread drawing identification was verified through
`get_curve_model_reference`:

```text
curveIndex 21 -> edgeType = kThreadEdge
curveIndex 22 -> edgeType = kThreadEdge
```

Existing `create_hole_thread_note` is verified as sufficient for standalone
`ThreadFeature` drawing annotation. Inventor generated:

```text
text = M15x1.5 - 6g
isHoleNote = false
attached = true
```

Package 42D verified `get_hole_thread_notes` hardening:

- previous whole-command `E_FAIL` on standalone thread notes is eliminated;
- `success = true`;
- `text = M15x1.5 - 6g`;
- `isHoleNote = false`;
- `attached = true`;
- `referenceKey` present;
- unavailable optional COM properties such as `Intent` and
  `RightHandedThread` are isolated in `propertyDiagnostics`.

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

Runtime does not parse thread designations, choose curves automatically,
reconstruct hole/thread note text, modify the model, or perform GOST/ESKD
decisions. Inventor remains responsible for native hole/thread note text.

## Center Mark and Centerline Pipeline

Verified commands and Eye extensions:

- `get_center_marks` referenceKey support
- `get_centerlines` referenceKey support
- `create_center_mark`
- `create_centerline_bisector`
- `create_centerline_centered_pattern`

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
- return factual resulting state/reference keys.

Runtime does not:

- detect holes automatically;
- detect bolt-circle patterns;
- select pattern centers;
- select member holes;
- infer symmetry;
- reorder geometry;
- choose annotations based on engineering meaning;
- make GOST/ESKD decisions;
- optimize annotation/layout placement.

Deferred:

- generic `create_centerline`;
- `Centerlines.AddByWorkFeature` centerline;
- delete centerline / delete center mark commands.

## General Dimension Tolerance Pipeline

Verified commands:

- `get_general_dimension_tolerance`
- `set_general_dimension_tolerance_default`
- `set_general_dimension_tolerance_basic`
- `set_general_dimension_tolerance_reference`
- `set_general_dimension_tolerance_symmetric`
- `set_general_dimension_tolerance_deviation`
- `set_general_dimension_tolerance_limits`
- `set_general_dimension_tolerance_fits`

Coverage:

- `GeneralDimension.Tolerance`;
- `Tolerance.ToleranceType`;
- `Tolerance.Upper`;
- `Tolerance.Lower`;
- `Tolerance.HoleTolerance`;
- `Tolerance.ShaftTolerance`;
- `Tolerance.SetToDefault()`;
- `Tolerance.SetToBasic()`;
- `Tolerance.SetToReference()`;
- `Tolerance.SetToSymmetric(...)`;
- `Tolerance.SetToDeviation(...)`;
- `Tolerance.SetToLimits(...)`;
- `Tolerance.SetToFits(...)`;
- before/after tolerance facts;
- reference keys where supported;
- diagnostics.

Verified fits behavior:

```text
dimensionIndex = 2
toleranceType = limits_fits_stacked
holeTolerance = H7
shaftTolerance = g6
result toleranceType = kLimitsFitsStackedTolerance
diagnostics = []
```

Observed Inventor behavior:

After `Tolerance.SetToDefault()`, Inventor changes `ToleranceType` to
`kDefaultTolerance` and resets upper/lower values to `0`, but previously
assigned `HoleTolerance` / `ShaftTolerance` strings may remain readable.
Runtime does not compensate for or clear those strings.

Runtime boundary:

- caller selects the dimension;
- caller selects tolerance mode;
- caller supplies numeric values;
- caller supplies fit strings;
- Runtime performs no unit conversion;
- Runtime performs no sign normalization;
- Runtime performs no upper/lower reordering;
- Runtime performs no fit validation or engineering selection;
- Runtime performs no GOST/ESKD tolerance decisions.

## Drawing Dimensions Creation and Editing Pipeline

Verified commands:

- `create_angular_dimension`
- `create_ordinate_dimension`
- `get_drawing_view_origin_indicator`
- `create_drawing_view_origin_indicator`
- `create_baseline_dimension`
- `create_chain_dimension`
- `set_general_dimension_formatted_text`
- `set_general_dimension_hide_value`
- `set_general_dimension_precision`
- `set_general_dimension_model_value_override`
- `clear_general_dimension_model_value_override`
- `set_general_dimension_style`
- `set_general_dimension_layer`

Verified ordinate pipeline:

```text
DrawingView geometry
-> GeometryIntent
-> DrawingView.CreateOriginIndicator(...)
-> OrdinateDimensions.Add(...)
```

Baseline and chain dimensions were verified with three explicit
`GeometryIntent` selectors and created two dimensions each.

Dimension editing Hands operate only on explicitly selected
`GeneralDimension` objects. Runtime does not choose geometry, placement,
style, layer, tolerance, precision, text, or model-value override values.

## Parts List + Balloon Pipeline

Verified commands:

- `create_parts_list`
- `create_balloon`

Verified pipelines:

```text
DrawingView
-> Parts List

DrawingView geometry
-> GeometryIntent
-> Balloon
```

Runtime coverage:

- `Sheet.PartsLists.Add`;
- `Sheet.CreateGeometryIntent`;
- `Sheet.Balloons.Add`;
- explicit DrawingView selection;
- explicit DrawingCurve index selection;
- explicit leader points;
- PartsList and Balloon reference keys;
- Balloon value sets;
- structured diagnostics.

Runtime does not modify BOM data, choose item numbering automatically, choose
balloon placement, select geometry, optimize layout, or perform
engineering/GOST decisions.

## Drawing Generation / Export Hands

Verified commands:

- `create_drawing_document`
- `create_base_view`
- `create_projected_view`
- `create_section_line`
- `create_section_view`
- `create_detail_view`
- `create_auxiliary_view`
- `add_drawing_view_break`
- `export_pdf`
- `export_dwg`
- `export_dxf`

Verified end-to-end pipeline:

```text
DrawingDocument
-> Base/Projected/Section/Detail/Auxiliary Views
-> Drawing View Break
-> PDF/DWG/DXF Export
```

## DWG/DXF Export Coverage

`export_dwg` and `export_dxf` export the active Autodesk Inventor
`DrawingDocument` through the DWG/DXF Translator Add-Ins.

Confirmed translators:

```text
DWG ClientId: {C24E3AC2-122E-11D5-8E91-0010B541CD80}
DXF ClientId: {C24E3AC4-122E-11D5-8E91-0010B541CD80}
HasSaveCopyAsOptions = true
TranslatorAvailable = true
SupportsSaveCopyAs = true
```

Implementation verified:

- Build PASS.
- Command Registry PASS.
- Inventor PASS.

Runtime coverage:

- active `DrawingDocument` validation;
- caller-supplied DWG/DXF INI validation;
- `TranslationContext`;
- `NameValueMap`;
- `NameValueMap.Value["Export_Acad_IniFile"]` assignment;
- `DataMedium`;
- `TranslatorAddIn.SaveCopyAs`;
- overwrite protection;
- `overwrite=true` support;
- output file existence verification;
- file size / timestamp facts;
- unsaved `DrawingDocument` support;
- structured diagnostics;
- no interactive dialog.

DXF translator behavior:

- The public Inventor `exportdxf.ini` had `USE TRANSMITTAL=Yes`.
- That setting produced a ZIP package containing the DXF.
- Direct DXF output was verified with a caller-supplied DXF INI where
  transmittal is disabled.
- Runtime does not compensate for or reinterpret transmittal packaging.

## PDF Export Coverage

`export_pdf` exports the active Autodesk Inventor `DrawingDocument` through the PDF Translator Add-In.

Confirmed translator:

```text
ClientId: {0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}
DisplayName: Translator: PDF / Translyator: PDF
supportsSaveCopyAs = true
translatorAvailable = true
```

Implementation verified:

- Build PASS.
- Command Registry PASS.
- Inventor PASS.

Runtime coverage:

- active `DrawingDocument` validation;
- PDF Translator Add-In resolution;
- `TranslationContext`;
- `NameValueMap`;
- `DataMedium`;
- `TranslatorAddIn.SaveCopyAs`;
- overwrite protection;
- `overwrite=true` support;
- output file existence verification;
- file size / timestamp facts;
- structured diagnostics.

Verified runtime tests:

- New PDF export: `success = true`, `fileExists = true`, `fileSizeBytes = 78737`.
- Existing destination with `overwrite=false`: `success = false`, `error = Destination PDF already exists.`
- Existing destination with `overwrite=true`: `success = true`, existing file overwritten without pre-delete, `lastWriteUtc` changed.

## Known Limitations

- Print workflow is not implemented yet.
- PDF option tuning is not implemented in Runtime.
- DWG/DXF option tuning is not implemented in Runtime.
- Runtime does not generate export INI files.
- Runtime does not choose AutoCAD versions, mappings/layers, or transmittal behavior.
- Runtime does not choose output paths.
- Runtime does not create output directories.
- Runtime does not overwrite unless `overwrite=true`.
- Runtime does not choose dimension geometry, placement, style, layer,
  tolerance, precision, text, or model-value overrides.
- Runtime does not convert tolerance units, normalize signs, reorder
  upper/lower values, validate fit strings, or apply GOST/ESKD tolerance
  decisions.
- Semantic text understanding is outside Runtime.
- GOST interpretation and validation are outside Runtime.
- GD&T semantic interpretation is outside Runtime.
- Surface texture semantic interpretation is outside Runtime.
- Welding semantic interpretation is outside Runtime.
- Drawing symbol semantic interpretation is outside Runtime.
- Engineering interpretation remains the responsibility of the external LLM.
- RevisionCloud control-point editing and revision association remain deferred.
- TransitionSymbol GeometryIntent/drawing-view attachment, edge/face
  attachment, definition editing, leader editing, layer/style mutation,
  automatic placement, and GOST/ESKD interpretation remain deferred.
- Some older annotation commands remain implemented but not separately Inventor-verified.
- Experimental commands remain compatibility-only and must not be expanded as Runtime architecture examples:
  - `analyze_dimension_layout`
  - `auto_arrange_dimensions`
  - `check_annotation_collisions`
  - `auto_resolve_annotation_collisions`
  - `analyze_view_dimension_candidates`

## Next Step

Next Capability Check:

```text
Capability Audit - remaining drawing annotation lifecycle gaps
```

Start the next chat by reading `AGENTS.md`, `CURRENT_STATE.md`,
`CAPABILITY_MAP.md`, and this `CHAT_HANDOFF.md`. Then audit the live
registry/capability map and remaining native drawing annotation collections
before proposing or writing code.

Do not implement the next capability until the audit proves the gap and the
user authorizes implementation.
