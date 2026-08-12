# AGENTS.md — AI CAD ENGINEER

## 1. Project purpose

AI CAD ENGINEER is a local bridge between an AI agent and Autodesk Inventor.

The application provides:

- **Eyes**: atomic read-only commands that expose Inventor data.
- **Hands**: atomic write commands that perform one explicit Inventor action.

The external AI agent performs engineering reasoning, chooses actions, analyzes geometry, and decides what to do next. The C# runtime must not replace the AI with embedded engineering decision logic.

## 2. Non-negotiable architecture rule

Before creating or modifying code, ask:

```text
Does this code give the AI an Eye or an atomic Hand?
```

Proceed only when the answer is yes.

Do not implement in C#:

- automatic engineering decisions;
- automatic selection of drawing views, dimensions, tables, notes, or formats;
- automatic layout policies presented as final engineering decisions;
- hidden planning systems that decide instead of the AI;
- EngineeringBrain, AIDecision, Planning, DrawingManager, or similar runtime decision systems;
- duplicate high-level "create the whole drawing" brains.

Programmatic automation may be added only after explicit user approval and only where an AI cannot reliably perform the task through atomic tools.

## 3. Current runtime architecture

The active runtime path is:

```text
LLM
→ JSON
→ Program.cs
→ Core/Application.cs
→ InventorControl/InventorCommandDispatcher.cs
→ IInventorCommand implementations
→ CommandSupport / ReadSupport
→ Autodesk Inventor API
```

Do not recreate or restore the removed legacy path:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
```

## 4. Mandatory pre-change audit

Before adding any command:

1. Read `InventorControl/InventorCommandDispatcher.cs`.
2. Search the entire repository for the proposed JSON command name.
3. Search for equivalent command classes and support methods.
4. Read current project status documents, when present:
   - `CURRENT_STATE.md`
   - `CAPABILITY_MAP.md`
   - `PROJECT_REVIEW.md`
   - `AI_CAPABILITIES.md`
   - `COMMANDS.md`
   - `IMPLEMENTATION_STATUS.md`
   - `ROADMAP.md`
   - `ARCHITECTURE.md`
5. Classify the requested capability:
   - already implemented and tested;
   - already implemented but untested;
   - partially implemented;
   - genuinely missing;
   - experimental / non-runtime if it contains analysis or automatic decisions.
6. Report this classification before writing code.

Creating duplicate commands is forbidden.

## 5. Command design rules

Every JSON command must perform exactly one of these:

- one atomic read operation;
- one atomic write operation.

Command requirements:

- stable snake_case JSON command name;
- one command class implementing `IInventorCommand`;
- explicit input validation;
- structured JSON success response;
- structured JSON error response;
- useful diagnostics without hiding Inventor API errors;
- no silent fallback that changes engineering intent;
- preserve raw enum values when useful;
- also expose readable enum names where possible.

## 6. Autodesk Inventor constraints

Target environment:

```text
Windows 10
Visual Studio Community 2026
.NET 10
Autodesk Inventor Professional 2027
Autodesk Inventor COM reference
```

Important:

- Do not validate this project with `dotnet build`.
- `dotnet build` fails on `ResolveComReference` with MSB4803.
- Build through Visual Studio or classic .NET Framework MSBuild.
- Inventor COM signatures may expose interfaces such as `Inventor._Document`.
- Match the actual installed Inventor 2027 interop signatures.
- Prefer compile-safe explicit COM casts when required.
- Do not guess API members; inspect existing working code, local interop metadata, and compiler errors.

## 7. Build and verification workflow

Before changes:

```text
git status
```

After code changes:

1. Show changed files.
2. Show `git diff`.
3. Build with classic Visual Studio MSBuild:

   ```text
   MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
   ```

4. Do not use `dotnet build`.
5. Provide exact JSON test commands.
6. Wait for real Inventor responses.
7. Only after successful tests update status documentation.
8. Commit only after explicit user permission.

Do not run `git push`, destructive Git commands, file deletion, broad refactors, `reset`, `clean`, `checkout`, or `stash` without explicit approval.

## 8. File editing rules

- Preserve the existing namespace and formatting style.
- Prefer full, complete files over partial fragments when replacing a file.
- Reuse existing support classes when appropriate.
- Do not introduce a second support layer that duplicates an existing one.
- Do not rename public JSON commands without explicit approval.
- Do not remove working commands merely because they look imperfect.
- Make the smallest coherent change.
- One package/milestone should solve one clearly defined capability.

## 9. Documentation rules

When a command is added and verified, update the applicable status documentation.

Do not claim a command is verified until it has:

1. Build PASS.
2. Real Inventor PASS.
3. User-confirmed behavior.

Use these status labels:

```text
VERIFIED
IMPLEMENTED_UNTESTED
PARTIAL
EXPERIMENTAL
DEPRECATED
MISSING
OUT_OF_SCOPE
```

Do not describe a GOST `CustomTable` as a `PartsList`. Inventor exposes these through different API collections.

## 10. Legacy experimental commands

The following commands exist but contain programmatic analysis/automation beyond pure Eyes or atomic Hands:

```text
analyze_dimension_layout
auto_arrange_dimensions
check_annotation_collisions
auto_resolve_annotation_collisions
analyze_view_dimension_candidates
```

Rules:

- keep them for compatibility;
- mark them experimental;
- do not expand them without explicit user approval;
- do not use them as architectural examples for new work.

## 11. Current milestone

Current milestone addendum after Package 40-41:

```text
Package 40-41 complete - Center Mark and Centerline Pipeline checkpoint
```

Verified commands and Eye extensions:

```text
get_center_marks referenceKey support
get_centerlines referenceKey support
create_center_mark
create_centerline_bisector
create_centerline_centered_pattern
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

Runtime exposes factual drawing geometry, resolves explicit caller selectors,
creates explicit `GeometryIntent` objects, executes atomic Inventor API
operations, and returns factual state/reference keys. Runtime must not detect
holes, detect bolt-circle patterns, select pattern centers or member holes,
infer symmetry, reorder geometry, choose annotations by engineering meaning,
optimize layout, or make GOST/ESKD decisions.

Deferred: generic `create_centerline`, work-feature centerline creation, and
delete centerline / center mark commands.

Next correct action: run a Capability Audit for Hole / Thread annotation
capabilities before writing code.

Current milestone addendum after Package 37-39:

```text
Package 37-39 complete - General Dimension Tolerance Pipeline checkpoint
```

Verified commands:

```text
get_general_dimension_tolerance
set_general_dimension_tolerance_default
set_general_dimension_tolerance_basic
set_general_dimension_tolerance_reference
set_general_dimension_tolerance_symmetric
set_general_dimension_tolerance_deviation
set_general_dimension_tolerance_limits
set_general_dimension_tolerance_fits
```

Runtime reads and writes `GeneralDimension.Tolerance` facts only on explicitly
selected `GeneralDimension` objects. Caller selects dimension, tolerance mode,
numeric values, and fit strings. Runtime performs no unit conversion, sign
normalization, upper/lower reordering, fit validation, engineering selection,
or GOST/ESKD tolerance decisions.

Observed Inventor behavior: after `Tolerance.SetToDefault()`, `ToleranceType`
becomes `kDefaultTolerance` and upper/lower values reset, but previous
`HoleTolerance` / `ShaftTolerance` strings may remain readable. Runtime must
not compensate for or clear those strings.

Next correct action: run a Capability Audit to choose the next practical
drawing engineering layer before writing code.

Current milestone addendum after Package 31-36:

```text
Package 31-36 complete - Drawing Dimensions Creation and Editing Pipeline checkpoint
```

Verified commands:

```text
create_angular_dimension
create_ordinate_dimension
get_drawing_view_origin_indicator
create_drawing_view_origin_indicator
create_baseline_dimension
create_chain_dimension
set_general_dimension_formatted_text
set_general_dimension_hide_value
set_general_dimension_precision
set_general_dimension_model_value_override
clear_general_dimension_model_value_override
set_general_dimension_style
set_general_dimension_layer
```

Ordinate dimension creation requires an existing DrawingView OriginIndicator:

```text
DrawingView geometry
-> GeometryIntent
-> DrawingView.CreateOriginIndicator(...)
-> OrdinateDimensions.Add(...)
```

Dimension editing Hands operate only on explicitly selected
`GeneralDimension` objects. Runtime still does not choose geometry, dimension
placement, style, layer, tolerance, precision, text, model-value overrides, or
perform engineering/GOST/ESKD decisions.

Next correct action: run a Capability Audit for General Dimension Tolerance
capabilities before writing code.

Current milestone addendum after Package 30:

```text
Package 30 complete - Parts List + Balloon Pipeline checkpoint
```

Verified commands:

```text
create_parts_list
create_balloon
```

Verified pipelines:

```text
DrawingView -> Parts List
DrawingView geometry -> GeometryIntent -> Balloon
```

`create_parts_list` creates one Inventor PartsList from an explicit existing
DrawingView and explicit placement point. `create_balloon` creates one Inventor
Balloon from an explicit DrawingView curve, `Sheet.CreateGeometryIntent`, and
caller-supplied leader points.

Runtime still does not modify BOM data, choose item numbering automatically,
choose balloon placement, select geometry, optimize layout, or perform
engineering/GOST decisions.

Next correct action: run a Capability Check to choose the next practical
engineering layer.

Current milestone addendum after Package 29:

```text
Package 29 complete - DWG/DXF Export Pipeline checkpoint
```

Verified Drawing Export Hands:

```text
export_pdf
export_dwg
export_dxf
```

`export_dwg` and `export_dxf` are VERIFIED. They export the active Autodesk
Inventor `DrawingDocument` through the DWG/DXF Translator Add-Ins and
`TranslatorAddIn.SaveCopyAs`. Runtime requires explicit output paths and
caller-supplied INI files, assigns `Export_Acad_IniFile` through
`NameValueMap.Value`, blocks existing destinations unless `overwrite=true`,
verifies output files, and does not open interactive dialogs.

DXF translator behavior recorded by Package 29: the public Inventor
`exportdxf.ini` had `USE TRANSMITTAL=Yes` and produced a ZIP package
containing DXF. Direct DXF output was verified with a caller-supplied DXF INI
where transmittal is disabled. Runtime must not compensate for or reinterpret
translator packaging choices.

Runtime still does not generate INI files, choose AutoCAD versions, choose
mappings/layers, choose transmittal behavior, create directories, overwrite
without explicit permission, or perform engineering/GOST decisions.

Next correct action: run a Capability Check to choose the next practical
engineering layer.

Current milestone addendum after Package 28:

```text
Package 28 complete - PDF Export Pipeline checkpoint
```

`export_pdf` is VERIFIED. It exports the active Autodesk Inventor
`DrawingDocument` through the PDF Translator Add-In and
`TranslatorAddIn.SaveCopyAs`.

Runtime requires an explicit output path, does not create folders, does not
overwrite without `overwrite=true`, uses default translator options, verifies
that the output file exists, and reports file size/timestamp facts and
diagnostics. Runtime does not tune PDF options, regenerate drawing geometry,
or perform engineering/GOST decisions.

Next correct action: run a Capability Audit for Drawing Export Hands - DWG /
DXF.

Current milestone addendum after Package 23:

```text
Package 23 complete - create_drawing_document Hand checkpoint
```

Current verified Drawing Generation Hand:

```text
create_drawing_document
```

Current Package 23 status:

- `create_drawing_document` is VERIFIED.
- It creates a new Autodesk Inventor `DrawingDocument` through `Application.Documents.Add`.
- It requires explicit `templatePath` and accepts optional `visible`.
- It returns created drawing document metadata.
- The runtime still does not choose templates, generate drawings, create views, fill title blocks, export, validate GOST/ESKD, or perform engineering decisions.

Next correct action:

1. run a Capability Audit for `create_section_view`;
2. verify whether sufficient atomic Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing any new code;
4. keep all section line and placement inputs explicit;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 22, Package 21A, Package 20A, Package 19A, Package 18A, and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 22:

```text
Package 22 complete - Drawing Symbol Layer checkpoint
```

Current verified drawing symbol commands:

```text
get_feature_control_frames
get_surface_texture_symbols
get_welding_symbols
get_revision_clouds
get_edge_symbols
get_transition_symbols
```

Current Package 22 status:

- Drawing Symbol Layer is VERIFIED.
- `get_revision_clouds` reads `Sheet.RevisionClouds`, RevisionCloud metadata, RevisionCloudDefinition, control points, reference keys, and diagnostics.
- `get_edge_symbols` reads `Sheet.EdgeSymbols`, EdgeSymbol metadata, EdgeSymbolDefinition, reference keys, and diagnostics.
- `get_transition_symbols` reads `Sheet.TransitionSymbols`, TransitionSymbol metadata, leader/attachment metadata, TransitionSymbolDefinition, reference keys, and diagnostics.
- the runtime still does not perform symbol interpretation, GOST/ISO validation, correctness checking, semantic analysis, or engineering interpretation.

Next correct action:

1. run a Capability Check to choose the next engineering layer;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing any new code;
4. decide whether another atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 21A, Package 20A, Package 19A, Package 18A, and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 21A:

```text
Package 21A complete - Welding Symbols Eye checkpoint
```

Current confirmed drawing symbol commands:

```text
get_feature_control_frames
get_surface_texture_symbols
get_welding_symbols
```

Current Package 21A status:

- `get_welding_symbols` is VERIFIED;
- welding symbol reading covers `Sheet.WeldingSymbols`, DrawingWeldingSymbols collection metadata, DrawingWeldingSymbol metadata, DrawingWeldingSymbolDefinition, WeldSymbolOne, WeldSymbolTwo, reference keys, and diagnostics;
- the runtime still does not perform weld interpretation, GOST validation, welding semantic analysis, or engineering interpretation.

Next correct action:

1. run a Capability Check for Drawing Symbol Layer completion review;
2. verify which symbol Eyes are now covered and which are still missing;
3. inspect actual Inventor 2027 interop signatures before proposing any new code;
4. decide whether another atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 20A, Package 19A, Package 18A, and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 20A:

```text
Package 20A complete - Surface Texture Symbols Eye checkpoint
```

Current confirmed drawing symbol commands:

```text
get_feature_control_frames
get_surface_texture_symbols
```

Current Package 20A status:

- `get_surface_texture_symbols` is VERIFIED;
- surface texture symbol reading covers `Sheet.SurfaceTextureSymbols`, SurfaceTextureSymbol metadata, position, layer, style, leader, roughness fields, production fields, sampling fields, definition data, reference keys, and diagnostics;
- the runtime still does not perform roughness interpretation, GOST validation, surface texture semantic analysis, or engineering interpretation.

Next correct action:

1. run a Capability Check for Welding Symbols Eye;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 19A, Package 18A, and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 19A:

```text
Package 19A complete — Feature Control Frames Eye checkpoint
```

Current confirmed drawing symbol command:

```text
get_feature_control_frames
```

Current Package 19A status:

- `get_feature_control_frames` is VERIFIED;
- feature control frame reading covers `Sheet.FeatureControlFrames`, frame metadata, rows, tolerance fields, datum fields, reference keys, and diagnostics;
- the runtime still does not perform tolerance interpretation, GOST validation, GD&T semantic analysis, or engineering interpretation.

Next correct action:

1. run a Capability Check for the next drawing symbol layer;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 18A and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 18A:

```text
Package 18A complete — Drawing Text Objects Eye checkpoint
```

Current confirmed drawing text command:

```text
get_drawing_text_objects
```

Current Package 18A status:

- `get_drawing_text_objects` is VERIFIED;
- drawing text object reading covers DrawingNotes collections, DrawingSketch TextBoxes, and SketchedSymbols;
- the runtime still does not perform semantic text analysis, TT/TU recognition, GOST interpretation, or engineering interpretation.

Next correct action:

1. run a Capability Check for the next engineering-layer boundary;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 17A notes below are superseded by this addendum where they conflict.

Current milestone:

```text
Package 17A complete — typed CustomTables Eye checkpoint
```

Current confirmed drawing table commands:

```text
get_parts_lists
get_revision_tables
get_drawing_table_collections
get_custom_tables
```

Current status:

- `get_parts_lists` is VERIFIED for reading Inventor API `Sheet.PartsLists`;
- `get_revision_tables` is VERIFIED;
- `get_drawing_table_collections` is VERIFIED;
- `get_custom_tables` is VERIFIED;
- the tested GOST table is exposed by Inventor as `Sheet.CustomTables` / `kCustomTableObject`, not as `Sheet.PartsLists`;
- detailed reading of `Sheet.CustomTables` metadata, columns, rows, cells, merged cells, and reference keys is implemented and verified;
- the runtime still does not classify tables as specification, BOM, or GOST.

Next correct action:

1. run a Capability Audit for Drawing Text / Notes Eye;
2. verify whether sufficient drawing text and note reading already exists;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

## 12. Required response style for development tasks

For every requested feature, respond in this order:

```text
AUDIT
- what already exists;
- what is missing;
- whether the change fits Eyes and Hands.

PLAN
- exact files to create;
- exact files to modify;
- why each is required.

IMPLEMENTATION
- make the smallest coherent change.

VERIFICATION
- changed files;
- git diff summary;
- exact MSBuild step;
- exact JSON test command;
- no commit until approved.
```

## 13. Safety stop conditions

Stop and ask before proceeding when:

- the requested capability may duplicate existing functionality;
- the change would implement engineering reasoning inside C#;
- the required Inventor API signature is uncertain;
- the change requires deleting or broadly restructuring files;
- current tracked changes are unrelated to the requested task;
- build errors reveal that the plan was based on the wrong API surface;
- the user asks for a full automation brain rather than Eyes or atomic Hands.

## 14. Definition of done

A capability is complete only when all applicable items are true:

1. no duplicate command exists;
2. implementation fits the Eyes/Hands architecture;
3. the project builds with the required MSBuild workflow;
4. F5 launches the current executable when UI launch is required;
5. the JSON command succeeds in real Inventor;
6. returned data/action is visually or structurally confirmed;
7. documentation is updated;
8. the user explicitly approves the commit.

## Current milestone addendum after Package 24

```text
Package 24 complete - Section View Pipeline checkpoint
```

Verified Drawing Generation Hands:

```text
create_drawing_document
create_base_view
create_section_line
create_section_view
```

The section pipeline is verified in Inventor. `create_section_line` creates a
sketch owned by the selected parent view and converts explicit sheet
coordinates with `SheetToSketchSpace`; `create_section_view` uses that sketch
with `DrawingViews.AddSectionView2`. Runtime does not choose placement or
direction, analyze the model, or perform engineering/GOST decisions.

Next correct action: run a Capability Audit for `create_detail_view` before
writing code.

## Current milestone addendum after Package 42

```text
Package 42 complete - Hole and Thread Annotation Pipeline checkpoint
```

Verified capabilities:

```text
get_hole_features
get_thread_features
create_hole_thread_note
get_hole_thread_notes
```

`get_hole_features` is hardened for expanded factual HoleFeature data,
HoleFeature reference keys, tapped-hole ThreadInfo facts, and non-blocking
property diagnostics.

`get_thread_features` is a VERIFIED read-only Eye for standalone Inventor
ThreadFeature facts from the PartDocument referenced by an explicit DrawingView.
Inventor validation read an external `M15x1.5` / class `6g` thread and returned
a reference key.

Existing `create_hole_thread_note` is VERIFIED as sufficient for standalone
ThreadFeature drawing annotation when the external caller explicitly selects a
`kThreadEdge` drawing curve. Inventor generated the native note text
`M15x1.5 - 6g`.

`get_hole_thread_notes` is hardened so unavailable optional COM properties do
not fail the entire Eye. Standalone thread notes are read with text,
`isHoleNote`, attachment state, reference keys, and property diagnostics.

Verified standalone-thread pipeline:

```text
get_thread_features
-> get_drawing_curves / get_curve_model_reference
-> explicit kThreadEdge selection by external caller
-> create_hole_thread_note
-> Inventor-generated annotation
-> get_hole_thread_notes
```

Runtime does not parse thread designations, choose curves automatically,
reconstruct hole/thread note text, modify the model, or perform GOST/ESKD
decisions. Inventor remains responsible for native hole/thread note text.

Next correct action: run a Capability Audit for General Notes / Leader Notes /
technical requirements before writing code.

## Current milestone addendum after Package 43-44

```text
Package 43-44 complete - General and Leader Note primitives checkpoint
```

Verified General Note primitives:

```text
get_general_notes
create_general_note_fitted
set_general_note_formatted_text
move_general_note
delete_general_note
```

Verified General Note lifecycle:

```text
create -> read -> edit -> read -> move -> read -> delete -> read
final get_general_notes count = 0
```

Verified Leader Note primitives:

```text
get_leader_notes
get_drawing_text_objects
create_leader_note
set_leader_note_formatted_text
move_leader_note
delete_leader_note
```

Free LeaderNote lifecycle is verified. `attachedEntity = null` is a valid
factual state for free leader notes. Attached LeaderNote is verified through an
explicit `GeometryIntent` using `viewName = ВИД1`, `curveIndex = 14`, and
`intent = mid`; `pointOnSheet = (23.65, 15.6)` matched the selected curve
midpoint; referenceKey and non-blocking diagnostics were returned.

Observed Inventor behavior: setting `LeaderNote.Position` can produce an
actual position different from the requested coordinates because Inventor may
constrain/reposition the note according to leader geometry. Runtime reports
both requested and actual values and does not compensate.

Runtime does not generate technical requirement text, choose requirements,
number requirements semantically, decide GOST/ESKD content, automatically
choose geometry, automatically place annotations, or interpret drawing meaning.

Next correct action completed by Package 49B. Current next correct action: run
a Capability Audit for Drawing Tables / Revision Tables / Hole Tables lifecycle
before writing code.

## Current milestone addendum after Package 49B

```text
Package 49B complete - PartsList lifecycle checkpoint
```

Verified PartsList commands:

```text
get_parts_lists
create_parts_list
move_parts_list
delete_parts_list
```

Verified PartsList lifecycle:

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

## Current milestone addendum after Package 49A

```text
Package 49A complete - Balloon lifecycle checkpoint
```

Verified Balloon commands:

```text
get_balloons
create_balloon
move_balloon
delete_balloon
```

Verified Balloon pipeline:

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

- `get_balloons` exposes native `Balloon.GetReferenceKey(...)`.
- Created balloon was `kBalloonObject`, attached to parent view `ВИД1`, with
  position `(30,20)`.
- `referenceKey` was present with byteCount `62`.
- Move requested `(32,18)` and factual readback reported
  `Balloon.Position = (32,18)` and `Leader.RootNode.Position = (32,18)`.
- Same referenceKey was preserved after move.
- `value = "1"` and `itemNumber = "1"` were unchanged.
- Geometry attachment was preserved.
- Delete returned deleted balloon snapshot and `remainingBalloonCount = 0`.
- Final `get_balloons` returned `count = 0`.

Runtime does not choose which components require balloons, choose balloon
placement, route leaders, renumber/sort BOM, modify `BalloonValueSet.Value`,
`BalloonValueSet.OverrideValue`, item numbering, PartsLists, assembly/model
structure, or apply GOST/ESKD layout logic.

## Current milestone addendum after Package 48

```text
Package 48 complete - SketchedSymbol primitives checkpoint
```

Verified SketchedSymbol primitives:

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

Important Inventor API finding: local Inventor 2027 `TextBox` Interop exposes
`Text` and `FormattedText`, but no dedicated prompted-entry flag was confirmed.
Runtime does not parse `<Prompt>` heuristically and does not fabricate prompted
values.

Runtime does not know what a sketched symbol means, choose symbol definitions,
perform fuzzy definition-name matching, assign datum/base semantics, generate
GOST/ESKD geometry, choose attachment geometry, route leaders automatically,
choose placement, or perform drawing-layout intelligence.

Next correct action completed by Package 49A.

## Current milestone addendum after Package 47

```text
Package 47 complete - Welding Symbol primitives checkpoint
```

Verified Welding Symbol primitives:

```text
get_welding_symbols
create_welding_symbol
move_welding_symbol
delete_welding_symbol
```

Verified Welding Symbol pipeline:

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
choose contour, choose process/method, choose field/all-around state, choose
geometry, attachment, or placement, interpret GOST/ESKD/AWS/ISO welding rules,
or modify model weld geometry.

Next correct action: run a Capability Audit for Datum identifiers / Datum
Target Symbols before writing code.

## Current milestone addendum after Package 46

```text
Package 46 complete - Surface Texture Symbol primitives checkpoint
```

Verified Surface Texture Symbol primitives:

```text
get_surface_texture_symbols
create_surface_texture_symbol
move_surface_texture_symbol
delete_surface_texture_symbol
```

Verified free Surface Texture Symbol pipeline:

```text
explicit native roughness fields
-> SurfaceTextureSymbols.Add(...)
-> native SurfaceTextureSymbol
-> get_surface_texture_symbols
```

Verified attached Surface Texture Symbol pipeline:

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

Verified move uses `SurfaceTextureSymbol.Leader.RootNode.Position` when a
leader root exists, with factual post-move state confirmed by
`get_surface_texture_symbols`. Verified delete uses
`SurfaceTextureSymbol.Delete()`, and final read returned `count = 0`.

Runtime does not decide roughness values, choose Ra/Rz, infer machining
process, choose material-removal requirement, choose geometry, choose
placement, or apply GOST/ESKD semantics. External LLM supplies all engineering
decisions.

Next correct action: run a Capability Audit for Weld Symbols / Welding
Annotations before writing code.

## Current milestone addendum after Package 45

```text
Package 45 complete - Feature Control Frame primitives checkpoint
```

Verified Feature Control Frame primitives:

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

Verified structured row content includes `kPosition`, tolerance `"0.1"`, datum
references `A` and `B`, empty third datum, and referenceKey.

Observed Inventor behavior: for leader-based drawing FeatureControlFrame
objects, setting `FeatureControlFrame.Position` may not move the object. The
verified native placement primitive is
`FeatureControlFrame.Leader.RootNode.Position`.

Runtime does not choose GD&T characteristics, calculate tolerances, assign
datums, interpret MMC/LMC/RFS, choose geometry or placement, or apply
GOST/ESKD engineering logic.

Next correct action: run a Capability Audit for Surface Texture / Surface
Finish symbols before writing code.

## Current milestone addendum after Package 26

```text
Package 26 complete - Auxiliary View Pipeline checkpoint
```

`create_auxiliary_view` is a VERIFIED atomic Hand using
`DrawingViews.AddAuxiliaryView()`. Runtime preserves Inventor's returned
position and does not compensate for API adjustments or make placement,
direction, scale, engineering, or GOST decisions.

Next correct action: run a Capability Audit for `add_drawing_view_break`.

## Current milestone addendum after Package 27

```text
Package 27 complete - Drawing View Break Hand checkpoint
```

`add_drawing_view_break` is VERIFIED for horizontal and vertical breaks using
`DrawingView.BreakOperations.Add`. Runtime preserves factual Inventor values
and does not choose break geometry, style, orientation, or engineering/GOST
decisions.

Next correct action: run a Capability Audit for Drawing Export Hands.

## Current milestone addendum after Package 25

```text
Package 25 complete - Detail View Pipeline checkpoint
```

`create_detail_view` is a VERIFIED atomic Hand. It calls
`DrawingViews.AddDetailView()` with an explicit circular fence. Runtime does
not generate fences, choose scale or placement, select attachments, analyze
models, or perform engineering/GOST decisions.

Next correct action: run a Capability Audit for `create_auxiliary_view` before
writing code.
