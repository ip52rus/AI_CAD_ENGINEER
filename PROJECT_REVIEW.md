# PROJECT REVIEW

## Package 42 checkpoint - current state

Package 42 closes the verified Hole and Thread Annotation Pipeline. The runtime
now exposes hardened factual model hole/thread Eyes and robust drawing
hole/thread note reading while preserving the existing native Inventor
`create_hole_thread_note` Hand.

Current live command inventory after Package 42:

```text
156 registered JSON commands
156 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
```

Verified commands and Eye hardening:

```text
get_hole_features
get_thread_features
create_hole_thread_note
get_hole_thread_notes
```

Package 42A verified `get_hole_features` hardening:

- expanded factual `HoleFeature` fields;
- `HoleFeature` referenceKey support;
- tapped-hole `ThreadInfo` facts;
- property-level diagnostics for unavailable optional COM properties.

Package 42B verified `get_thread_features` for standalone `ThreadFeature`
facts. Inventor validation read a real external thread:

```text
designation = M15x1.5
threadClass = 6g
referenceKey returned
```

Standalone thread drawing identification is verified through
`get_curve_model_reference`:

```text
curveIndex 21 -> edgeType = kThreadEdge
curveIndex 22 -> edgeType = kThreadEdge
```

Existing `create_hole_thread_note` is verified as sufficient for standalone
`ThreadFeature` annotation. Inventor generated:

```text
text = M15x1.5 - 6g
isHoleNote = false
attached = true
```

Package 42D verified `get_hole_thread_notes` hardening: the previous whole
command `E_FAIL` on standalone thread notes is eliminated. Unavailable optional
COM properties such as `Intent` and `RightHandedThread` are isolated in
`propertyDiagnostics` and do not invalidate the Eye.

Verified standalone-thread annotation pipeline:

```text
get_thread_features
-> get_drawing_curves / get_curve_model_reference
-> explicit kThreadEdge selection by external caller
-> create_hole_thread_note
-> Inventor-generated annotation
-> get_hole_thread_notes
```

Runtime boundaries remain explicit: no thread designation parsing, no
automatic curve selection, no note-text reconstruction, no model modification,
and no GOST/ESKD decisions. Inventor remains responsible for generating native
hole/thread note text.

Next capability check: General Notes / Leader Notes / technical requirements.

## Package 40-41 checkpoint

Package 40-41 closes the verified Center Mark and Centerline Pipeline. The
commands create center marks, bisector centerlines, and centered-pattern
centerlines from explicit caller-selected drawing geometry.

Current live command inventory after Package 40-41:

```text
155 registered JSON commands
155 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
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

Runtime boundaries remain explicit: no hole detection, no bolt-circle pattern
detection, no automatic center/member selection, no symmetry inference, no
geometry reordering, no engineering-meaning selection, no layout optimization,
and no GOST/ESKD decisions.

Deferred: generic `create_centerline`, work-feature centerline creation, and
delete centerline / center mark commands.

Next capability check: Hole / Thread annotation capabilities.

## Package 37-39 checkpoint - current state

Package 37-39 closes the verified General Dimension Tolerance Pipeline. The
new commands read and set `GeneralDimension.Tolerance` state on explicitly
selected `GeneralDimension` objects.

Current live command inventory after Package 37-39:

```text
152 registered JSON commands
152 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
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

Verified coverage includes `Tolerance.SetToDefault`, `SetToBasic`,
`SetToReference`, `SetToSymmetric`, `SetToDeviation`, `SetToLimits`, and
`SetToFits`.

Observed Inventor API behavior: after `SetToDefault()`, `ToleranceType`
becomes `kDefaultTolerance` and upper/lower values reset to `0`, but previous
hole/shaft tolerance strings may remain readable. Runtime does not compensate
for or clear these strings.

Runtime boundaries remain explicit: no automatic tolerance mode selection, no
unit conversion, no sign normalization, no upper/lower reordering, no fit
validation or engineering selection, and no GOST/ESKD tolerance decisions.

Next capability check: choose the next practical drawing engineering layer
through Capability Audit first.

## Package 31-36 checkpoint - current state

Package 31-36 closes the verified Drawing Dimensions Creation and Editing
Pipeline. The new commands create angular, ordinate, baseline, and chain
dimensions from explicit caller inputs and edit explicitly selected
`GeneralDimension` objects.

Current live command inventory after Package 31-36:

```text
144 registered JSON commands
144 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
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

Verified ordinate pipeline:

```text
DrawingView geometry
-> GeometryIntent
-> DrawingView.CreateOriginIndicator(...)
-> OrdinateDimensions.Add(...)
```

Baseline and chain dimensions were verified with three explicit
`GeometryIntent` selectors and created two dimensions each.

Runtime boundaries remain explicit: no automatic geometry selection, no
automatic dimension placement, no automatic style/layer selection, no
tolerance decisions, no layout optimization, and no engineering/GOST/ESKD
decisions.

Next capability check: General Dimension Tolerance capabilities.

## Package 30 checkpoint - current state

Package 30 closes the verified Parts List + Balloon Pipeline. The commands
`create_parts_list` and `create_balloon` create one Inventor PartsList and one
Inventor Balloon from explicit caller inputs.

Current live command inventory after Package 30:

```text
131 registered JSON commands
131 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
```

Verified pipelines:

```text
DrawingView -> Parts List
DrawingView geometry -> GeometryIntent -> Balloon
```

Runtime boundaries remain explicit: no BOM modification, no automatic item
numbering, no automatic balloon placement, no geometry selection, no layout
optimization, and no engineering/GOST decisions.

Next capability check: choose the next practical engineering layer.

## Package 29 checkpoint - current state

Package 29 closes the verified DWG/DXF Export Pipeline. The commands
`export_dwg` and `export_dxf` export the active `DrawingDocument` through the
confirmed Inventor DWG/DXF Translator Add-Ins and `TranslatorAddIn.SaveCopyAs`.

Current live command inventory after Package 29:

```text
129 registered JSON commands
129 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
```

Verified Drawing Export Hands:

```text
export_pdf
export_dwg
export_dxf
```

DWG uses translator ClientId `{C24E3AC2-122E-11D5-8E91-0010B541CD80}`.
DXF uses translator ClientId `{C24E3AC4-122E-11D5-8E91-0010B541CD80}`.
Both require caller-supplied INI files and assign
`Export_Acad_IniFile` through the `NameValueMap.Value` setter. Both support
overwrite protection, `overwrite=true`, output file verification, unsaved
`DrawingDocument` export, and non-interactive export.

DXF note: the public Inventor `exportdxf.ini` had `USE TRANSMITTAL=Yes` and
therefore produced a ZIP package containing the DXF. Direct DXF output was
verified with a caller-supplied DXF INI where transmittal is disabled.
Runtime does not compensate for this translator setting.

Next capability check: choose the next practical engineering layer.

## Package 28 checkpoint - current state

Package 28 closes the verified PDF Export Pipeline. The command `export_pdf`
exports the active `DrawingDocument` through the confirmed Inventor PDF
Translator Add-In and `TranslatorAddIn.SaveCopyAs`.

Coverage includes translator resolution, `TranslationContext`, `NameValueMap`,
`DataMedium`, overwrite protection, `overwrite=true`, output file existence
verification, file size/timestamp facts, and structured diagnostics.

Current live command inventory after Package 28:

```text
127 registered JSON commands
127 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
```

Next capability check: Drawing Export Hands - DWG / DXF.

## Package 27 checkpoint - current state

Package 27 closes the verified Drawing View Break Hand. The command
`add_drawing_view_break` uses `DrawingView.BreakOperations.Add` and supports
horizontal/vertical orientations and rectangular style. Actual Inventor
post-creation values are preserved without compensation.

Next capability check: Drawing Export Hands (`PDF`, `DWG`, `DXF`).

## Package 26 checkpoint - current state

Package 26 closes the verified Auxiliary View pipeline. The current Drawing
Generation Hands include `create_base_view`, `create_projected_view`,
`create_section_line`, `create_section_view`, `create_detail_view`, and
`create_auxiliary_view`.

`create_auxiliary_view` uses `DrawingViews.AddAuxiliaryView()` with explicit
parent view and orientation curve selector. Inventor may adjust the requested
position; the actual returned position is preserved. Next capability:
`add_drawing_view_break`.

## Package 25 checkpoint - current state

Package 25 closes the verified Detail View generation pipeline. The current
atomic Drawing Generation Hands include `create_drawing_document`,
`create_base_view`, `create_section_line`, `create_section_view`, and
`create_detail_view`.

`create_detail_view` supports only an explicit circular fence through
`DrawingViews.AddDetailView()`. The next capability check is
`create_auxiliary_view` Hand.

## Package 24 checkpoint - current state

Package 24 closes the verified Section View generation pipeline. The current
atomic Drawing Generation Hands are `create_drawing_document`,
`create_base_view`, `create_section_line`, and `create_section_view`.

The section-line sketch must belong to the parent view (`parentView.Sketches.Add()`)
and caller-provided sheet coordinates are converted with `SheetToSketchSpace`.
The next capability check is the `create_detail_view` Hand.

## Package 23C addendum - current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 23B:

```text
123 registered JSON commands
123 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 23 status:

- `create_drawing_document` is VERIFIED.
- It creates a new Inventor `DrawingDocument` through `Application.Documents.Add`.
- It requires an explicit `templatePath`.
- It accepts optional `visible`.
- It returns created drawing document metadata.
- It does not select templates, create views, create dimensions, fill title blocks, export, validate GOST/ESKD, or perform engineering decisions.

Next recommended capability audit:

```text
Capability Audit - create_section_view Hand
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 22E addendum - current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 22D:

```text
120 registered JSON commands
120 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 22 status:

- `get_revision_clouds` is VERIFIED.
- `get_edge_symbols` is VERIFIED.
- `get_transition_symbols` is VERIFIED.
- Drawing Symbol Layer is now VERIFIED for:
  - `get_feature_control_frames`;
  - `get_surface_texture_symbols`;
  - `get_welding_symbols`;
  - `get_revision_clouds`;
  - `get_edge_symbols`;
  - `get_transition_symbols`.
- The runtime reads Inventor API facts only.
- It does not perform symbol interpretation, GOST/ISO validation, correctness checking, or engineering conclusions.

Next recommended capability check:

```text
Capability Check - choose next engineering layer
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 21B addendum - current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 21A:

```text
117 registered JSON commands
117 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 21A status:

- `get_welding_symbols` is VERIFIED.
- It reads Inventor API `Sheet.WeldingSymbols` as a typed Eye.
- It covers DrawingWeldingSymbol metadata, DrawingWeldingSymbolDefinition fields, WeldSymbolOne, WeldSymbolTwo, reference keys, and diagnostics.
- It does not perform weld interpretation, GOST validation, welding semantic analysis, or engineering conclusions.

Next recommended capability check:

```text
Capability Check - Drawing Symbol Layer completion review
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 20B addendum - current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 20A:

```text
116 registered JSON commands
116 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 20A status:

- `get_surface_texture_symbols` is VERIFIED.
- It reads Inventor API `Sheet.SurfaceTextureSymbols` as a typed Eye.
- It covers SurfaceTextureSymbol metadata, position, layer, style, leader, roughness fields, production fields, sampling fields, definition data, reference keys, and diagnostics.
- It does not perform roughness interpretation, GOST validation, surface texture semantic analysis, or engineering conclusions.

Next recommended capability check:

```text
Capability Check - Welding Symbols Eye
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 19B addendum — current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 19A:

```text
115 registered JSON commands
115 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 19A status:

- `get_feature_control_frames` is VERIFIED.
- It reads Inventor API `Sheet.FeatureControlFrames` as a typed Eye.
- It covers FeatureControlFrame metadata, FeatureControlFrameRows, tolerance fields, datum fields, reference keys, and diagnostics.
- It does not perform tolerance interpretation, GOST validation, GD&T semantic analysis, or engineering conclusions.

Next recommended capability check:

```text
Capability Check — next drawing symbol layer
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 18B addendum — current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 18A:

```text
114 registered JSON commands
114 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 18A status:

- `get_drawing_text_objects` is VERIFIED.
- It reads Inventor API drawing text-like objects as a typed Eye.
- It covers DrawingNotes collections, DrawingSketch TextBoxes, and SketchedSymbols.
- It does not perform semantic text analysis, GOST interpretation, TT/TU recognition, or engineering conclusions.

Next recommended capability check:

```text
Capability Check — next engineering layer
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 16D addendum — current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Packages 13A, 15A, 16A, and 16C:

```text
112 registered JSON commands
112 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 16A status:

- `get_parts_lists` is VERIFIED for reading Inventor API `Sheet.PartsLists`.
- `get_revision_tables` is VERIFIED.

Package 16C status:

- `get_drawing_table_collections` is VERIFIED.
- In the tested GOST drawing, the visible `Список деталей` table is `Sheet.CustomTables` / `kCustomTableObject`, not `Sheet.PartsLists`.
- Detailed row/column/cell reading for `Sheet.CustomTables` is not implemented yet.

Next recommended capability audit:

```text
Typed CustomTables Eye
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

## Package 17B addendum — current checkpoint

This addendum supersedes older command counts and next-step notes below where they conflict.

Current live command inventory after Package 17A:

```text
113 registered JSON commands
113 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Package 17A status:

- `get_custom_tables` is VERIFIED.
- It reads Inventor API `Sheet.CustomTables` as a typed Eye.
- It covers CustomTable metadata, Columns, Rows, Cells, MergedCells, and reference keys.
- It does not classify tables as specification, BOM, GOST, or PartsList.

Next recommended capability audit:

```text
Drawing Text / Notes Eye
```

Do not reintroduce EngineeringBrain, DrawingManager, Planning, Decision, or automatic engineering logic into the runtime.

Дата анализа: 2026-08-06
Рабочий каталог: `C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER`
Git-ветка: `cleanup/legacy-architecture`

## Важное состояние рабочей копии

До создания этого файла рабочее дерево уже не было чистым:

```text
 M InventorControl/InventorCommandDispatcher.cs
?? AGENTS.md
?? CODEX_START_PROMPT.txt
?? CODEX_WORKFLOW.md
?? CURRENT_STATE.md
?? InventorControl/Commands/DrawingViews/BaseViewCommandSupport.cs
?? InventorControl/Commands/DrawingViews/CreateBaseViewCommand.cs
```

`InventorCommandDispatcher.cs` уже содержит регистрацию `create_base_view`. Два файла Package 13A присутствуют в рабочем дереве, но пока не отслеживаются Git.

## 1. Текущая архитектура

Фактический runtime сейчас такой:

```text
LLM
-> JSON
-> Program.cs
-> Core/Application.cs
-> Core/InventorManager.cs
-> InventorControl/InventorCommandDispatcher.cs
-> IInventorCommand
-> CommandSupport / ReadSupport
-> Autodesk Inventor API
```

Приложение работает как консольный JSON-мост к Inventor:

- `Program.cs` создает `Core.Application` и запускает runtime.
- `Application.cs` подключается к Autodesk Inventor, печатает минимальную справку и читает JSON-команды из консоли.
- `InventorManager.cs` подключается к запущенному Inventor через COM или стартует новый экземпляр.
- `InventorCommandDispatcher.cs` парсит JSON, читает поле `command`, вручную выбирает команду через `switch` и возвращает JSON-ответ.
- Каждая команда реализует `IInventorCommand` и получает `JsonElement`.
- Support-классы выполняют общую работу для домена: поиск листа/вида/документа, валидация параметров, чтение Inventor-объектов, сериализация результата.

Отдельных рабочих папок `Json` и `Runtime` нет. JSON-слой реализован напрямую через `System.Text.Json` в dispatcher, командах и support-классах.

Папки `Infrastructure` и `Import` существуют, но сейчас пустые.

## 2. Структура папок

Ключевые директории:

- `AI` - содержит `OpenAiClient.cs`, в текущем runtime не используется.
- `Core` - запуск приложения и подключение к Inventor.
- `InventorControl` - dispatcher и все JSON-команды.
- `InventorControl/Commands/Annotations` - команды анализа и авторазмещения аннотаций.
- `InventorControl/Commands/AssemblyEyes` - read-only команды по сборкам.
- `InventorControl/Commands/Border` - рамки листа.
- `InventorControl/Commands/Diagnostics` - диагностика add-ins и GOST metadata.
- `InventorControl/Commands/Dimensions` - размеры, геометрия размеров, layout-анализ.
- `InventorControl/Commands/Documents` - открыть/активировать/сохранить/закрыть документы.
- `InventorControl/Commands/DrawingStructure` - read-only структура чертежа.
- `InventorControl/Commands/DrawingViews` - операции с видами чертежа.
- `InventorControl/Commands/HoleThreadNotes` - hole/thread notes.
- `InventorControl/Commands/Intelligence` - экспериментальная логика кандидатов размеров.
- `InventorControl/Commands/ModelConstraints` - read-only эскизы, зависимости, work features.
- `InventorControl/Commands/ModelFeatures` - read-only дерево фич и отверстия.
- `InventorControl/Commands/ModelGeometry` - read-only геометрия модели.
- `InventorControl/Commands/ModelReferences` - связи drawing curve/view с моделью.
- `InventorControl/Commands/Properties` - свойства документов.
- `InventorControl/Commands/Sheets` - операции с листами.
- `InventorControl/Commands/TitleBlock` - основная надпись и поля.
- `Audit` - результат cleanup-аудита.

Сводка по `.cs` без `bin/obj`:

- всего исходных `.cs`: 136;
- `.cs` в `InventorControl/Commands`: 131;
- support/read-support файлов: 26;
- `Create*Command.cs`: 7.

## 3. JSON-команды

Зарегистрировано 104 JSON-команды.

Проверка соответствия:

- `public string Name` найдено: 104;
- уникальных `Name`: 104;
- регистраций в dispatcher: 104;
- дублей JSON-имён: 0;
- command-классов с `Name`, отсутствующих в dispatcher: 0;
- dispatcher-регистраций без соответствующего `Name`: 0.

Полный список команд:

```text
activate_document
activate_sheet
analyze_dimension_layout
analyze_view_dimension_candidates
auto_arrange_dimensions
auto_resolve_annotation_collisions
center_general_dimension_text
check_annotation_collisions
close_document
create_base_view
create_diameter_dimension
create_hole_thread_note
create_linear_dimension
create_projected_view
create_radius_dimension
create_sheet
delete_drawing_dimension
delete_drawing_view
delete_general_dimension
delete_hole_thread_note
delete_sheet
fill_title_block
get_active_document
get_annotation_bounds
get_application_addins
get_assembly_bom
get_assembly_constraints
get_assembly_occurrences
get_assembly_referenced_documents
get_assembly_summary
get_body_faces
get_border_definitions
get_curve_model_reference
get_dimension_geometry
get_document_properties
get_document_property
get_document_property_by_id
get_document_property_sets
get_drawing_annotation_summary
get_drawing_curves
get_drawing_dimensions
get_drawing_sheets
get_drawing_tables
get_drawing_view
get_drawing_view_relationships
get_drawing_views
get_drawing_views_detailed
get_face_edges
get_feature_details
get_general_dimensions_detailed
get_gost_metadata
get_hole_features
get_hole_thread_notes
get_model_feature_tree
get_model_parameters
get_open_documents
get_sheet
get_sheet_border
get_sheet_title_block
get_sheets
get_sketch_constraints
get_sketch_dimensions
get_sketch_geometry
get_sketches
get_surface_bodies
get_title_block_binding
get_title_block_bindings
get_title_block_definition_text
get_title_block_definitions
get_title_block_field_map
get_title_block_fields
get_view_model_references
get_work_features
move_drawing_dimension
move_drawing_view
move_general_dimension_text
move_hole_thread_note
move_linear_dimension
open_document
ping
remove_sheet_border
remove_sheet_title_block
rename_drawing_view
rename_sheet
rotate_drawing_view
save_document
save_document_as
set_document_property
set_document_property_by_id
set_drawing_view_alignment
set_drawing_view_label_visibility
set_drawing_view_scale
set_drawing_view_scale_inheritance
set_drawing_view_style
set_drawing_view_suppressed
set_hole_thread_note_format
set_sheet_border
set_sheet_orientation
set_sheet_size
set_sheet_title_block
set_title_block_definition_text
set_title_block_field
set_title_block_field_by_name
update_active_document
```

Разбивка по типам:

- `get_*`: 51;
- `create_*`: 7;
- `set_*`: 16;
- `move_*`: 5;
- `delete_*`: 5;
- `remove_*`: 2;
- прочие операции: 17;
- `ping`: 1.

## 4. Уже реализованные области

Реализованы:

- подключение к Inventor и чтение активного документа;
- базовая работа с документами: список, открыть, активировать, сохранить, сохранить как, закрыть;
- чтение листов и видов чертежа;
- создание, удаление, активация, переименование листов;
- настройка размера и ориентации листов;
- чтение и изменение рамки листа;
- чтение и изменение основной надписи;
- заполнение полей основной надписи;
- чтение свойств документа и изменение свойств по имени или id;
- создание базового вида `create_base_view` - зарегистрировано, но требует сборки и проверки в Inventor;
- создание проекционного вида `create_projected_view`;
- чтение, перемещение, переименование, поворот, suppression/style/label/scale/alignment для drawing views;
- чтение drawing curves;
- создание линейного, диаметрального и радиального размера;
- чтение, перемещение и удаление размеров;
- чтение geometry/bounds текста размеров;
- hole/thread notes: чтение, создание, перемещение, удаление, формат;
- диагностика add-ins и GOST metadata;
- read-only данные модели: параметры, surface bodies, faces, edges, feature tree, hole features, feature details;
- read-only sketch/constraint/work-feature данные;
- read-only данные сборок: summary, occurrences, constraints, BOM, referenced documents;
- read-only связи вида/кривой чертежа с моделью.

## 5. Отсутствующие команды

Точного `COMMANDS.md` со списком целевых команд нет, поэтому ниже перечислены отсутствующие области относительно текущего фактического покрытия и заявленной архитектуры "eyes and hands".

Отсутствуют или не видны в dispatcher:

- создание нового drawing document из шаблона;
- экспорт/печать чертежей, PDF/DWG/DXF/STP;
- создание section/detail/auxiliary/break/crop views;
- создание и управление centerlines/centermarks;
- создание angular/ordinate/baseline/chain/symmetric/chamfer dimensions;
- создание/удаление/изменение leader text, balloons, surface texture, weld symbols, datum/feature control frames;
- создание parts list и произвольных таблиц;
- атомарные команды редактирования модели: параметры, sketches, features;
- атомарные write-команды для сборок: constraints, occurrences, patterns;
- команды выбора/подсветки объектов в UI Inventor;
- единая команда описания возможностей runtime, например `get_commands` или `get_capabilities`.

Эти команды не следует добавлять как "автоматические сценарии". Если они нужны, каждая должна быть отдельным атомарным eye/hand.

## 6. CommandSupport, Create*Command, ReadSupport

Support/read-support слой фактически существует по доменам:

- `AnnotationCollisionSupport`
- `AssemblyReadSupport`
- `AddInDiagnosticSupport`
- `GostMetadataSupport`
- `AutomaticDimensionLayoutSupport`
- `DimensionCommandSupport`
- `DimensionGeometrySupport`
- `DimensionPositionSupport`
- `DocumentCommandSupport`
- `DrawingStructureReadSupport`
- `BaseViewCommandSupport`
- `DrawingViewCommandSupport`
- `HoleThreadNoteCommandSupport`
- `ViewDimensionCandidateSupport`
- `ModelConstraintReadSupport`
- `ModelFeatureReadSupport`
- `ModelGeometryReadSupport`
- `ModelReferenceReadSupport`
- `PropertyByIdSupport`
- `PropertyCommandSupport`
- `SheetCommandSupport`
- `FillTitleBlockSupport`
- `TitleBlockBindingSupport`
- `TitleBlockDefinitionTextSupport`
- `TitleBlockFieldMapperSupport`
- `TitleBlockFieldSupport`

`Create*Command`:

- `CreateProjectedViewCommand.cs` -> `create_projected_view`
- `CreateBaseViewCommand.cs` -> `create_base_view`
- `CreateDiameterDimensionCommand.cs` -> `create_diameter_dimension`
- `CreateLinearDimensionCommand.cs` -> `create_linear_dimension`
- `CreateRadiusDimensionCommand.cs` -> `create_radius_dimension`
- `CreateHoleThreadNoteCommand.cs` -> `create_hole_thread_note`
- `CreateSheetCommand.cs` -> `create_sheet`

Read-support файлы в основном соответствуют "eyes" архитектуре. Исключение по духу архитектуры: `ViewDimensionCandidateSupport` и layout/collision support содержат эвристику анализа/авторазмещения, поэтому их надо считать экспериментальными и не использовать как образец для новых команд.

## 7. Мертвый код и устаревшие файлы

Кандидаты:

- `AI/OpenAiClient.cs` - tracked файл, но нет ссылок из runtime. Нарушает целевую модель, где LLM внешняя, а C# приложение только глаза/руки.
- `Import/` - пустая директория.
- `Infrastructure/` - пустая директория.
- `ARCHITECTURE.md`, `README.md`, `ROADMAP.md` - описывают старую архитектуру с `EngineeringBrain`, `Decision`, `Drawing Layer`, `Reports`, хотя код уже очищен.
- `README_CLEANUP.md`, `cleanup_manifest.json`, `Remove-LegacyArchitecture.ps1` - полезны как cleanup-артефакты, но после завершения cleanup являются устаревающей служебной историей.
- `AI_CAD_ENGINEER.csproj` содержит `<Folder Include="InventorControl\Commands\Diagnostics\" />`, хотя в этой папке уже есть файлы. Это не критично, но элемент выглядит лишним.

## 8. Дублирование

Основные повторения:

- `CreateSuccess`, `CreateError`, `CreateJsonOptions` повторяются в большом числе команд и support-классов.
- `GetActiveDrawingDocument`, `FindSheet`, `TryGetRequiredString`, `TryGetRequiredDouble`, `GetOptionalBoolean` повторяются по доменам.
- Старые flat-команды в `InventorControl/Commands` частично дублируют новые доменные команды по смыслу: `get_drawing_views` vs `get_drawing_views_detailed`, `get_drawing_dimensions` vs `get_general_dimensions_detailed`.
- `CreateProjectedViewCommand` содержит собственные private helpers вместо использования общего drawing-view support.
- Properties имеют параллельные support-классы для доступа по имени и по id; это допустимо, но часть сериализации и нормализации значения повторяется.

Рекомендованное упрощение без изменения поведения: ввести небольшой общий JSON response/validation helper для новых команд. Старые команды лучше не переписывать массово без отдельного решения, чтобы не создавать риск.

## 9. Нарушения архитектуры

По целевой архитектуре проблемные зоны:

- `AI/OpenAiClient.cs` держит клиент OpenAI внутри приложения. Для текущей архитектуры это лишний слой.
- Команды `analyze_dimension_layout`, `auto_arrange_dimensions`, `check_annotation_collisions`, `auto_resolve_annotation_collisions`, `analyze_view_dimension_candidates` содержат анализ/эвристику/авторазмещение. Их надо оставить только как compatibility/experimental и не расширять.
- `InventorCommandDispatcher` вручную хранит поля для всех команд, вручную создает экземпляры и вручную держит `switch`. Это не нарушает runtime, но масштабируется плохо.
- `Application.cs` печатает только три доступные команды, хотя фактически зарегистрировано 104.
- Документация противоречит коду и целевому правилу "глаза и руки".

## 10. Потенциальные ошибки Inventor API

Риски, которые требуют проверки только через Visual Studio и живой Inventor:

- `dotnet build` не является валидной проверкой из-за COM `ResolveComReference`/MSB4803. Проверять нужно Visual Studio или подходящий MSBuild с COM support.
- `create_base_view` использует явный cast `Inventor.Document -> Inventor._Document` перед `AddBaseView`. Это соответствует ранее найденной проблеме компиляции, но команда еще не подтверждена JSON-тестом.
- `create_base_view` передает `modelViewName` как строку, по умолчанию пустую. Если Inventor 2027 ожидает отсутствующий optional argument вместо пустой строки, возможна runtime-ошибка или выбор не того представления. Проверить на реальной модели.
- `create_base_view` проверяет только точку вставки внутри листа, но не проверяет габариты вида после масштаба. Вид может частично выйти за лист.
- Многие read-support классы используют `dynamic` и bare `catch`. Это помогает читать нестабильные COM-свойства, но может скрывать реальные проблемы API и возвращать неполные данные без явного признака.
- Команды размеров используют `dynamic` для `AddLinear`, `AddDiameter`, `AddRadius` и fallback-перегрузки. Это снижает compile-time контроль сигнатур.
- Доступ к `drawingView.Camera.ViewOrientationType`, `RangeBox`, `Text.RangeBox`, `Aligned` может бросать исключения для некоторых типов видов/аннотаций; часть read-support кода это глушит.
- Команды, работающие по индексам DrawingCurve, зависят от текущего порядка кривых Inventor. После перестроения вида индексы могут измениться.

## 11. Возможные упрощения

Без изменения архитектурного принципа:

- добавить атомарную read-only команду `get_commands`, которая возвращает список зарегистрированных команд и минимальные схемы входа/выхода;
- вынести общий JSON response helper для новых команд;
- для новых команд использовать один support на домен вместо private helper в каждой команде;
- постепенно помечать legacy/experimental команды в документации, не удаляя их;
- обновить `ARCHITECTURE.md`, `README.md`, `ROADMAP.md` после подтвержденной проверки runtime;
- держать новые команды строго в форме: одна команда, одно действие, полный JSON-ответ;
- не добавлять EngineeringBrain/AIDecision/Planning в C#.

## 12. Рекомендованный следующий шаг

Сейчас код менять не нужно.

Для Package 13A следующий безопасный шаг:

1. собрать проект в Visual Studio;
2. запустить F5;
3. открыть модель и чертеж;
4. выполнить JSON:

```json
{
  "command": "create_base_view",
  "modelDocument": "имя_открытой_модели.ipt",
  "x": 10.0,
  "y": 10.0,
  "scale": 1.0,
  "orientation": "front",
  "style": "hidden_line_removed"
}
```

После успешной проверки в Inventor можно обновлять статусную документацию. Код без отдельного подтверждения менять не следует.
