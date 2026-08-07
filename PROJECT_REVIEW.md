# PROJECT REVIEW

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
