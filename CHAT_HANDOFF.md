# AI CAD ENGINEER - CHAT HANDOFF

## Current State

- Branch: `cleanup/legacy-architecture`.
- Current checkpoint: `v0.33 complete center mark and centerline pipeline`.
- Latest commit after checkpoint commit: see `git log -1 --oneline --decorate`.
- Latest completed checkpoint: Package 40-41 - Center Mark and Centerline Pipeline.
- Registry after Package 40-41: `155 registered / 155 unique`, `0` duplicate command names.
- Working tree is expected to be clean after the Package 40-41 checkpoint commit.

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
- Drawing Text Objects: `get_drawing_text_objects`.
- Feature Control Frames Eye: `get_feature_control_frames`.
- Surface Texture Symbols Eye: `get_surface_texture_symbols`.
- Welding Symbols Eye: `get_welding_symbols`.
- RevisionClouds Eye: `get_revision_clouds`.
- EdgeSymbols Eye: `get_edge_symbols`.
- TransitionSymbols Eye: `get_transition_symbols`.
- create_drawing_document Hand: `create_drawing_document`.
- Drawing View Break Hand: `add_drawing_view_break`.
- PDF Export Hand: `export_pdf`.
- DWG Export Hand: `export_dwg`.
- DXF Export Hand: `export_dxf`.
- Parts List creation Hand: `create_parts_list`.
- Balloon creation Hand: `create_balloon`.
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
- Typed detailed Eye for `Sheet.HoleTables` is still missing.
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
Capability Audit - Hole / Thread annotation capabilities
```

Start the next chat by reading `AGENTS.md`, `CURRENT_STATE.md`,
`CAPABILITY_MAP.md`, and this `CHAT_HANDOFF.md`. Then audit the live
repository before proposing or writing code.

Do not implement the next capability until the audit proves the gap and the
user authorizes implementation.
