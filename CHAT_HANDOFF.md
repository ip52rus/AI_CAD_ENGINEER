# AI CAD ENGINEER - CHAT HANDOFF

## Current State

- Branch: `cleanup/legacy-architecture`.
- Current checkpoint: `v0.43 complete hole table lifecycle`.
- Latest commit after checkpoint commit: see `git log -1 --oneline --decorate`.
- Latest completed checkpoint: Package 50B - HoleTable lifecycle.
- Registry after Package 50B: `185 registered / 185 unique`, `0` duplicate command names.
- Working tree is expected to be clean after the Package 50B checkpoint commit.

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
- Drawing Table Collections: `get_drawing_table_collections`.
- CustomTables Discovery: `get_drawing_table_collections`.
- CustomTables Detailed Reading: `get_custom_tables`.
- HoleTables Detailed Reading: `get_hole_tables`.
- HoleTable lifecycle Hands: `create_hole_table`, `move_hole_table`, `delete_hole_table`.
- Drawing Text Objects: `get_drawing_text_objects`.
- Feature Control Frames Eye: `get_feature_control_frames`.
- Surface Texture Symbols: `get_surface_texture_symbols`, `create_surface_texture_symbol`, `move_surface_texture_symbol`, `delete_surface_texture_symbol`.
- Welding Symbols: `get_welding_symbols`, `create_welding_symbol`, `move_welding_symbol`, `delete_welding_symbol`.
- SketchedSymbols: `get_sketched_symbol_definitions`, `create_sketched_symbol`, `move_sketched_symbol`, `delete_sketched_symbol`.
- RevisionClouds Eye: `get_revision_clouds`.
- EdgeSymbols Eye: `get_edge_symbols`.
- TransitionSymbols Eye: `get_transition_symbols`.
- create_drawing_document Hand: `create_drawing_document`.
- Drawing View Break Hand: `add_drawing_view_break`.
- PDF Export Hand: `export_pdf`.
- DWG Export Hand: `export_dwg`.
- DXF Export Hand: `export_dxf`.
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
- CustomTable lifecycle Hands are not implemented yet.
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
Capability Audit - CustomTable lifecycle
```

Start the next chat by reading `AGENTS.md`, `CURRENT_STATE.md`,
`CAPABILITY_MAP.md`, and this `CHAT_HANDOFF.md`. Then audit the live
repository before proposing or writing code.

Do not implement the next capability until the audit proves the gap and the
user authorizes implementation.
