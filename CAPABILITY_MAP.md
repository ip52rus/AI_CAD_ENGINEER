# CAPABILITY MAP

Date: 2026-08-11

Repository: `C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER`

Branch: `cleanup/legacy-architecture`

Checkpoint after this documentation sync:

```text
v0.48 complete transition symbol lifecycle
```

## Ground rules

This map is intentionally conservative.

Status meanings:

- `VERIFIED`: explicitly recorded as passing or verified against Autodesk Inventor.
- `IMPLEMENTED_UNTESTED`: implemented and registered/buildable, but no Inventor PASS is recorded.
- `PARTIAL`: some atomic Eyes/Hands exist, but the capability is incomplete.
- `EXPERIMENTAL`: command contains analysis, heuristics, automatic selection, layout, or optimization.
- `DEPRECATED`: kept for compatibility, not a preferred implementation path.
- `MISSING`: no confirmed implementation.
- `OUT_OF_SCOPE`: not part of the current audited capability boundary.

Do not treat `IMPLEMENTED_UNTESTED` as Inventor-verified.

Do not merge `CustomTables` and `PartsLists` into one capability. In Inventor API these are separate sheet collections.

## Capability table

| Capability | Status | Implemented commands | Inventor PASS confirmed | Experimental | Missing | Priority |
|---|---|---|---|---|---|---|
| Runtime connectivity | VERIFIED | `ping`, `get_active_document` | `ping`, `get_active_document` | - | - | P0 maintained |
| Document operations | PARTIAL | `get_open_documents`, `open_document`, `activate_document`, `update_active_document`, `save_document`, `save_document_as`, `close_document`, `create_drawing_document`, `export_pdf`, `export_dwg`, `export_dxf` | `create_drawing_document`, `export_pdf`, `export_dwg`, `export_dxf` | - | print workflow | P1 |
| Drawing sheets | VERIFIED | `get_drawing_sheets`, `get_sheets`, `get_sheet`, `activate_sheet`, `rename_sheet`, `create_sheet`, `delete_sheet`, `set_sheet_size`, `set_sheet_orientation` | `get_drawing_sheets` | - | no additional missing items confirmed by Package audits | P0 maintained |
| Borders and title blocks | PARTIAL | `get_border_definitions`, `get_sheet_border`, `set_sheet_border`, `remove_sheet_border`, `get_title_block_definitions`, `get_sheet_title_block`, `set_sheet_title_block`, `remove_sheet_title_block`, `get_title_block_fields`, `set_title_block_field`, `set_title_block_field_by_name`, `fill_title_block`, `get_title_block_definition_text`, `set_title_block_definition_text`, `get_title_block_binding`, `get_title_block_bindings`, `get_title_block_field_map` | not separately recorded | - | no missing items confirmed by Package 13-17 audits | P1 |
| Drawing views | PARTIAL | `get_drawing_views`, `get_drawing_view`, `get_drawing_views_detailed`, `get_drawing_view_relationships`, `get_drawing_curves`, `get_view_model_references`, `get_curve_model_reference`, `create_base_view`, `create_projected_view`, `create_auxiliary_view`, `add_drawing_view_break`, `move_drawing_view`, `delete_drawing_view`, `rename_drawing_view`, `rotate_drawing_view`, `set_drawing_view_scale`, `set_drawing_view_style`, `set_drawing_view_label_visibility`, `set_drawing_view_scale_inheritance`, `set_drawing_view_alignment`, `set_drawing_view_suppressed` | drawing views / relationships / curve geometry, `create_auxiliary_view`, and `add_drawing_view_break` are verified areas | - | detail/crop view improvements not covered by confirmed commands | P1 |
| Drawing Generation Hands | PARTIAL | `create_drawing_document`, `create_sheet`, `create_base_view`, `create_projected_view`, `create_section_line`, `create_section_view`, `create_detail_view`, `create_auxiliary_view`, `add_drawing_view_break`, `export_pdf`, `export_dwg`, `export_dxf` | `create_drawing_document`, `create_section_line`, `create_section_view`, `create_detail_view`, `create_auxiliary_view`, `add_drawing_view_break`, `export_pdf`, `export_dwg`, `export_dxf` | - | print workflow and other drawing-generation Hands not yet audited | P1 |
| Drawing Export Hands | VERIFIED | `export_pdf`, `export_dwg`, `export_dxf` | `export_pdf`, `export_dwg`, `export_dxf` | - | print workflow is not implemented | P0 maintained |
| Drawing dimensions | VERIFIED | `get_drawing_dimensions`, `get_general_dimensions_detailed`, `get_dimension_geometry`, `create_linear_dimension`, `create_diameter_dimension`, `create_radius_dimension`, `create_angular_dimension`, `create_ordinate_dimension`, `get_drawing_view_origin_indicator`, `create_drawing_view_origin_indicator`, `create_baseline_dimension`, `create_chain_dimension`, `set_general_dimension_formatted_text`, `set_general_dimension_hide_value`, `set_general_dimension_precision`, `set_general_dimension_model_value_override`, `clear_general_dimension_model_value_override`, `set_general_dimension_style`, `set_general_dimension_layer`, `get_general_dimension_tolerance`, `set_general_dimension_tolerance_default`, `set_general_dimension_tolerance_basic`, `set_general_dimension_tolerance_reference`, `set_general_dimension_tolerance_symmetric`, `set_general_dimension_tolerance_deviation`, `set_general_dimension_tolerance_limits`, `set_general_dimension_tolerance_fits`, `move_drawing_dimension`, `move_general_dimension_text`, `move_linear_dimension`, `center_general_dimension_text`, `delete_drawing_dimension`, `delete_general_dimension` | linear/diameter/radius plus Package 31-39 commands listed in explicit PASS section | `analyze_dimension_layout`, `auto_arrange_dimensions`, `analyze_view_dimension_candidates` | symmetric/chamfer dimensions are not confirmed | P0 maintained |
| General Dimension Tolerance Pipeline | VERIFIED | `get_general_dimension_tolerance`, `set_general_dimension_tolerance_default`, `set_general_dimension_tolerance_basic`, `set_general_dimension_tolerance_reference`, `set_general_dimension_tolerance_symmetric`, `set_general_dimension_tolerance_deviation`, `set_general_dimension_tolerance_limits`, `set_general_dimension_tolerance_fits` | all Package 37-39 tolerance commands | - | no automatic tolerance selection, no fit validation, no GOST/ESKD tolerance decisions in Runtime | P0 maintained |
| Hole/thread notes | VERIFIED | `get_hole_thread_notes`, `create_hole_thread_note`, `move_hole_thread_note`, `delete_hole_thread_note`, `set_hole_thread_note_format` | `create_hole_thread_note` verified for standalone `ThreadFeature` thread edge annotation; `get_hole_thread_notes` hardened and verified with referenceKey support | - | stable selector variants are still missing; current commands use indexes | P0 maintained |
| Center Marks / Centerlines | VERIFIED | `get_center_marks`, `get_centerlines`, `create_center_mark`, `create_centerline_bisector`, `create_centerline_centered_pattern` | Package 40-41 commands and referenceKey support | - | generic `create_centerline`, work-feature centerline, delete centerline/center mark deferred | P0 maintained |
| Basic drawing annotation Eyes | VERIFIED | `get_general_notes`, `get_leader_notes`, `get_balloons`, `get_center_marks`, `get_centerlines` | `get_general_notes`, `get_leader_notes`, `get_balloons` referenceKey support, center marks/centerlines, and balloon creation/read coverage are verified areas | - | additional typed Eyes only if future audits prove a gap | P0 maintained |
| Drawing Text Objects | VERIFIED | `get_drawing_text_objects` | `get_drawing_text_objects`, `get_drawing_text_objects` with `sheetName` | - | - | P0 maintained |
| General Notes / Technical Requirements primitives | VERIFIED | `get_general_notes`, `get_drawing_text_objects`, `create_general_note_fitted`, `set_general_note_formatted_text`, `move_general_note`, `delete_general_note` | Package 43 full create/read/edit/read/move/read/delete/read lifecycle; final `get_general_notes` count = 0 | - | rectangular GeneralNotes, text style/layer setters, automatic technical requirement generation are not Runtime scope for this checkpoint | P0 maintained |
| Leader Notes primitives | VERIFIED | `get_leader_notes`, `get_drawing_text_objects`, `create_leader_note`, `set_leader_note_formatted_text`, `move_leader_note`, `delete_leader_note` | Package 44 free and attached LeaderNote lifecycle; attached `GeometryIntent` with `curveIndex=14`, `intent=mid`; referenceKey and non-blocking diagnostics verified | - | leader path editing, style/layer setters, automatic leader routing are deferred | P0 maintained |
| Drawing Text semantic analysis | MISSING | - | - | - | semantic text understanding, GOST interpretation, TT/TU recognition; belongs to external LLM, not Runtime | no Runtime priority |
| Annotation summary and bounds | PARTIAL | `get_drawing_annotation_summary`, `get_annotation_bounds` | annotation summary recorded as verified area; exact PASS command not separately recorded | - | typed bounds for all annotation classes; current bounds coverage is incomplete | P1 |
| Annotation collision/layout logic | EXPERIMENTAL | - | not applicable | `check_annotation_collisions`, `auto_resolve_annotation_collisions` | should not be expanded as Runtime coverage | no priority |
| Surface Texture Symbols | VERIFIED | `get_surface_texture_symbols`, `create_surface_texture_symbol`, `move_surface_texture_symbol`, `delete_surface_texture_symbol` | `get_surface_texture_symbols`, `get_surface_texture_symbols` with `sheetName`, Package 46 free and attached SurfaceTextureSymbol create/move/delete validation | - | content-edit/style/layer Hands are not confirmed | P0 maintained |
| Surface texture semantic interpretation | MISSING | - | - | - | roughness interpretation, GOST validation, engineering analysis; belongs to external LLM, not Runtime | no Runtime priority |
| Welding Symbols | VERIFIED | `get_welding_symbols`, `create_welding_symbol`, `move_welding_symbol`, `delete_welding_symbol` | `get_welding_symbols`, `get_welding_symbols` with `sheetName`, Package 47 create/move/delete validation | - | content-edit/style/layer Hands are not confirmed | P0 maintained |
| Welding semantic analysis | MISSING | - | - | - | weld interpretation, GOST validation, engineering analysis; belongs to external LLM, not Runtime | no Runtime priority |
| Sketched Symbols | VERIFIED | `get_sketched_symbol_definitions`, `get_drawing_text_objects`, `create_sketched_symbol`, `move_sketched_symbol`, `delete_sketched_symbol` | Package 48 definition discovery; free `SketchedSymbols.Add` create/move/delete; leader/attached `SketchedSymbols.AddWithLeader` with optional GeometryIntent LAST | - | prompt result editing and definition copy/import/create are deferred | P0 maintained |
| Drawing Symbol Layer | VERIFIED | `get_feature_control_frames`, `get_surface_texture_symbols`, `get_welding_symbols`, `get_revision_clouds`, `create_revision_cloud`, `move_revision_cloud`, `delete_revision_cloud`, `get_edge_symbols`, `create_edge_symbol`, `move_edge_symbol`, `delete_edge_symbol`, `get_transition_symbols`, `create_transition_symbol`, `move_transition_symbol`, `delete_transition_symbol` | typed symbol Eyes plus Package 45-47 and Package 53A-55A lifecycle primitives where implemented | - | semantic interpretation is outside Runtime; geometry attachment for TransitionSymbols is not implemented | P0 maintained |
| RevisionClouds | VERIFIED | `get_revision_clouds`, `create_revision_cloud`, `move_revision_cloud`, `delete_revision_cloud` | Package 53A complete lifecycle: native `RevisionClouds.CreateRevisionCloudDefinition(...)`, `RevisionClouds.Add(...)`, `RevisionCloud.Position`, `RevisionCloud.Delete`, referenceKey preservation, and native position move translating cloud/control-point coordinates | - | control-point editing, revision association, layer/style mutation, revision policy/GOST semantics are not Runtime scope | P0 maintained |
| EdgeSymbols | VERIFIED | `get_edge_symbols`, `create_edge_symbol`, `move_edge_symbol`, `delete_edge_symbol` | Package 54A complete lifecycle: native `EdgeSymbols.CreateDefinition(...)`, `EdgeSymbols.Add(...)`, `EdgeSymbol.Position`, `EdgeSymbol.Delete`, referenceKey preservation, and unchanged definition through move | - | GeometryIntent attachment, definition editing, leader editing, layer/style mutation, automatic placement, geometry selection, and GOST/ESKD semantics are not Runtime scope | P0 maintained |
| TransitionSymbols | VERIFIED | `get_transition_symbols`, `create_transition_symbol`, `move_transition_symbol`, `delete_transition_symbol` | Package 55A complete lifecycle: native `TransitionSymbols.CreateDefinition(...)`, `TransitionSymbols.Add(...)`, explicit created-object `TransitionSymbol.Position` placement, `TransitionSymbol.Position` move, `TransitionSymbol.Delete`, referenceKey preservation, and null-safe leader-node diagnostics | - | GeometryIntent/drawing-view/edge/face attachment, definition editing, leader editing, layer/style mutation, automatic placement, geometry selection, and GOST/ESKD semantics are not Runtime scope | P0 maintained |
| Drawing symbol semantic interpretation | MISSING | - | - | - | symbol interpretation, GOST/ISO validation, correctness checking, and engineering analysis belong to external LLM, not Runtime | no Runtime priority |
| Datum identifiers | MISSING | placeholder count only; no confirmed API coverage | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Datum target symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Feature Control Frames | VERIFIED | `get_feature_control_frames`, `create_feature_control_frame`, `move_feature_control_frame`, `delete_feature_control_frame` | `get_feature_control_frames`, `get_feature_control_frames` with `sheetName`, Package 45 free and attached FCF create/move/delete validation | - | row/content/style/layer edit Hands are not confirmed | P0 maintained |
| GD&T semantic analysis | MISSING | - | - | - | tolerance interpretation, GOST validation, engineering analysis; belongs to external LLM, not Runtime | no Runtime priority |
| Revision symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Parts Lists | VERIFIED | `get_parts_lists`, `create_parts_list`, `move_parts_list`, `delete_parts_list`, legacy aggregate coverage through `get_drawing_tables` | Package 49B complete PartsList lifecycle: get/create/get/move/get/delete/get; move preserves referenceKey, rows, columns, cells, and referenced document/view facts | - | export/sort/renumber/cell editing/BOM mutation are not Runtime scope for this checkpoint | P0 maintained |
| Balloons | VERIFIED | `get_balloons`, `create_balloon`, `move_balloon`, `delete_balloon` | Package 49A complete balloon lifecycle: create/read/move/read/delete/read; `get_balloons` referenceKey support; move preserves referenceKey/value/itemNumber/geometry attachment | - | value override/edit and automatic item-number operations are not Runtime scope | P0 maintained |
| Parts List + Balloon Pipeline | VERIFIED | `get_parts_lists`, `create_parts_list`, `move_parts_list`, `delete_parts_list`, `create_balloon`, `get_balloons`, `move_balloon`, `delete_balloon` | DrawingView -> PartsList lifecycle; DrawingView geometry -> GeometryIntent -> Balloon lifecycle | - | no BOM modification, automatic numbering, automatic placement, geometry selection, layout optimization, or engineering/GOST decisions in Runtime | P0 maintained |
| Revision Tables | VERIFIED | `get_revision_tables`, `create_revision_table`, `move_revision_table`, `delete_revision_table`, legacy aggregate coverage through `get_drawing_tables` | Package 52A complete lifecycle: native `RevisionTables.Add(Point2d)`, `RevisionTable.Position`, `RevisionTable.Delete`, referenceKey preservation, rows/columns/cells readable; Inventor/template may generate native revision row content | - | revision row editing/numbering policy/revision property mutation/style-formatting/GOST logic are not Runtime scope | P0 maintained |
| Drawing Table Collections Diagnostics | VERIFIED | `get_drawing_table_collections` | confirmed counts and metadata for `CustomTables`, `HoleTables`, `PartsLists`, and `RevisionTables` | - | not intended to read full row/cell content | P0 maintained |
| CustomTables Discovery | VERIFIED | `get_drawing_table_collections` | confirmed GOST table is `Sheet.CustomTables` / `kCustomTableObject` | - | - | P0 maintained |
| CustomTables | VERIFIED | `get_custom_tables`, `create_custom_table`, `move_custom_table`, `delete_custom_table` | Package 17A detailed CustomTable Eye; Package 51A complete lifecycle with explicit title/placement/row count/column count/column titles, `CustomTable.Position` move, `CustomTable.Delete`, referenceKey preservation, and corrected zero-based Cell.Column -> one-based column metadata lookup | - | caller-supplied Contents/cell editing/sorting/merging/formatting/GOST logic are not Runtime scope | P0 maintained |
| Hole Tables | VERIFIED | `get_hole_tables`, `create_hole_table`, `move_hole_table`, `delete_hole_table` | Package 50A detailed HoleTable Eye; Package 50B complete lifecycle with explicit view/placement create, `HoleTable.Position` move, `HoleTable.Delete`, stable referenceKey after move, and no tag/content mutation by Runtime | - | sorting/row merging/tag renumbering/cell editing/formatting/GOST logic are not Runtime scope | P0 maintained |
| Legacy drawing table aggregate | PARTIAL | `get_drawing_tables` | legacy parts/revision table aggregate exists; does not cover `CustomTables` or `HoleTables` | - | not a complete all-table reader | compatibility |
| Document properties | PARTIAL | `get_document_properties`, `get_document_property`, `get_document_property_by_id`, `get_document_property_sets`, `set_document_property`, `set_document_property_by_id` | not separately recorded | - | no missing items confirmed by Package 13-17 audits | P2 |
| Model feature Eyes | VERIFIED | `get_model_feature_tree`, `get_hole_features`, `get_thread_features`, `get_feature_details`, `get_model_parameters` | `get_hole_features` hardened and verified; `get_thread_features` verified for standalone external ThreadFeature; feature tree, feature details, parameters recorded as verified areas | - | write commands for model features are not part of confirmed Runtime scope | P0 maintained |
| Model geometry Eyes | VERIFIED | `get_surface_bodies`, `get_body_faces`, `get_face_edges` | surface bodies, faces, edges recorded as verified areas; exact PASS command list not recorded | - | no missing items confirmed by Package 13-17 audits | P0 maintained |
| Sketch / constraint / work feature Eyes | VERIFIED | `get_sketches`, `get_sketch_geometry`, `get_sketch_constraints`, `get_sketch_dimensions`, `get_work_features` | sketches, sketch geometry, constraints, dimensions, work features recorded as verified areas; exact PASS command list not recorded | - | model/sketch constraint writes are outside Package 15 audit scope | P0 maintained |
| Assembly Eyes | VERIFIED | `get_assembly_summary`, `get_assembly_occurrences`, `get_assembly_constraints`, `get_assembly_bom`, `get_assembly_referenced_documents` | assembly summary, occurrences, constraints, BOM, referenced documents recorded as verified areas; exact PASS command list not recorded | - | assembly Hands are not confirmed | P1 |
| Diagnostics / metadata | PARTIAL | `get_application_addins`, `get_gost_metadata` | not separately recorded | - | no missing items confirmed by Package 13-17 audits | P2 |

## Commands with explicit Inventor PASS recorded

Exact commands explicitly confirmed:

```json
{"command":"ping"}
{"command":"get_active_document"}
{"command":"get_drawing_sheets"}
{"command":"get_revision_tables"}
{"command":"create_revision_table"}
{"command":"move_revision_table"}
{"command":"delete_revision_table"}
{"command":"get_revision_tables","sheetName":"Лист:1"}
{"command":"get_parts_lists"}
{"command":"get_parts_lists","sheetName":"Лист:1"}
{"command":"get_drawing_table_collections"}
{"command":"get_drawing_table_collections","sheetName":"Лист:1"}
{"command":"get_custom_tables"}
{"command":"create_custom_table"}
{"command":"move_custom_table"}
{"command":"delete_custom_table"}
{"command":"get_hole_tables"}
{"command":"get_surface_texture_symbols"}
{"command":"get_welding_symbols"}
{"command":"get_welding_symbols","sheetName":"Лист:1"}
{"command":"get_surface_texture_symbols","sheetName":"Лист:1"}
{"command":"get_drawing_text_objects"}
{"command":"get_feature_control_frames"}
{"command":"get_feature_control_frames","sheetName":"Лист:1"}
{"command":"get_drawing_text_objects","sheetName":"Лист:1"}
{"command":"get_custom_tables","sheetName":"Лист:1"}
{"command":"get_revision_clouds"}
{"command":"create_revision_cloud"}
{"command":"move_revision_cloud"}
{"command":"delete_revision_cloud"}
{"command":"get_revision_clouds","sheetName":"Лист:1"}
{"command":"get_edge_symbols"}
{"command":"create_edge_symbol"}
{"command":"move_edge_symbol"}
{"command":"delete_edge_symbol"}
{"command":"get_edge_symbols","sheetName":"Лист:1"}
{"command":"get_transition_symbols"}
{"command":"get_transition_symbols","sheetName":"Лист:1"}
{"command":"create_transition_symbol"}
{"command":"move_transition_symbol"}
{"command":"delete_transition_symbol"}
{"command":"create_drawing_document"}
{"command":"export_pdf"}
{"command":"export_dwg"}
{"command":"export_dxf"}
{"command":"create_parts_list"}
{"command":"move_parts_list"}
{"command":"delete_parts_list"}
{"command":"create_hole_table"}
{"command":"move_hole_table"}
{"command":"delete_hole_table"}
{"command":"create_balloon"}
{"command":"get_balloons"}
{"command":"move_balloon"}
{"command":"delete_balloon"}
{"command":"create_angular_dimension"}
{"command":"create_ordinate_dimension"}
{"command":"get_drawing_view_origin_indicator"}
{"command":"create_drawing_view_origin_indicator"}
{"command":"create_baseline_dimension"}
{"command":"create_chain_dimension"}
{"command":"set_general_dimension_formatted_text"}
{"command":"set_general_dimension_hide_value"}
{"command":"set_general_dimension_precision"}
{"command":"set_general_dimension_model_value_override"}
{"command":"clear_general_dimension_model_value_override"}
{"command":"set_general_dimension_style"}
{"command":"set_general_dimension_layer"}
{"command":"get_general_dimension_tolerance"}
{"command":"set_general_dimension_tolerance_default"}
{"command":"set_general_dimension_tolerance_basic"}
{"command":"set_general_dimension_tolerance_reference"}
{"command":"set_general_dimension_tolerance_symmetric"}
{"command":"set_general_dimension_tolerance_deviation"}
{"command":"set_general_dimension_tolerance_limits"}
{"command":"set_general_dimension_tolerance_fits"}
{"command":"create_center_mark"}
{"command":"create_centerline_bisector"}
{"command":"create_centerline_centered_pattern"}
{"command":"get_hole_features"}
{"command":"get_thread_features"}
{"command":"get_curve_model_reference"}
{"command":"create_hole_thread_note"}
{"command":"get_hole_thread_notes"}
{"command":"create_general_note_fitted"}
{"command":"set_general_note_formatted_text"}
{"command":"move_general_note"}
{"command":"delete_general_note"}
{"command":"create_leader_note"}
{"command":"set_leader_note_formatted_text"}
{"command":"move_leader_note"}
{"command":"delete_leader_note"}
{"command":"create_feature_control_frame"}
{"command":"move_feature_control_frame"}
{"command":"delete_feature_control_frame"}
{"command":"create_surface_texture_symbol"}
{"command":"move_surface_texture_symbol"}
{"command":"delete_surface_texture_symbol"}
{"command":"create_welding_symbol"}
{"command":"move_welding_symbol"}
{"command":"delete_welding_symbol"}
{"command":"get_sketched_symbol_definitions"}
{"command":"create_sketched_symbol"}
{"command":"move_sketched_symbol"}
{"command":"delete_sketched_symbol"}
```

`get_parts_lists` is verified for reading `Sheet.PartsLists`, not for reading GOST custom specification tables.

`get_custom_tables` is verified for reading `Sheet.CustomTables`, including CustomTable metadata, columns, rows, cells, merged cells, and reference keys.

`get_drawing_text_objects` is verified for reading DrawingNotes collections, DrawingSketch TextBoxes, and SketchedSymbols as Inventor API facts.

`get_feature_control_frames` is verified for reading `Sheet.FeatureControlFrames`, frame metadata, rows, tolerance fields, datum fields, reference keys, and diagnostics.

Package 45 verifies native drawing Feature Control Frame primitives:

```text
explicit leader points -> CreateFeatureControlFrameRows -> rows.Add(...) -> FeatureControlFrames.Add(...) -> native FeatureControlFrame -> get_feature_control_frames
explicit DrawingView/DrawingCurve -> Sheet.CreateGeometryIntent(...) -> GeometryIntent LAST -> FeatureControlFrames.Add(...)
```

Verified structured content includes `kPosition`, tolerance `"0.1"`, datum
references `A` and `B`, empty third datum, and referenceKey.

Observed Inventor behavior: for leader-based drawing `FeatureControlFrame`,
setting `FeatureControlFrame.Position` may not move the object. The verified
placement primitive is `FeatureControlFrame.Leader.RootNode.Position`. Runtime
reports factual state and does not compensate geometrically.

`get_surface_texture_symbols` is verified for reading `Sheet.SurfaceTextureSymbols`, SurfaceTextureSymbol metadata, surface texture fields, definition data, reference keys, and diagnostics.

Package 46 verifies native drawing Surface Texture Symbol primitives:

```text
explicit native roughness fields -> SurfaceTextureSymbols.Add(...) -> native SurfaceTextureSymbol -> get_surface_texture_symbols
DrawingView/DrawingCurve -> Sheet.CreateGeometryIntent(...) -> GeometryIntent LAST -> SurfaceTextureSymbols.Add(...)
SurfaceTextureSymbol.Leader.RootNode.Position -> get_surface_texture_symbols factual post-move state
SurfaceTextureSymbol.Delete() -> get_surface_texture_symbols count = 0
```

Verified native facts include `kSurfaceTextureSymbolObject`,
`kMaterialRemovalRequiredSurfaceType`, maximum roughness `"Ra 3.2"`,
`kParallelToPlaneOfProjection`, `kSurfaceTextureGOSTDefinitionObject`,
style `Шероховатость (ГОСТ)`, and referenceKey.

`get_welding_symbols` is verified for reading `Sheet.WeldingSymbols`, DrawingWeldingSymbol metadata, definition fields, weld symbol fields, reference keys, and diagnostics.

Package 47 verifies native drawing Welding Symbol primitives:

```text
get_welding_symbols -> create_welding_symbol -> optional GeometryIntent attachment -> move_welding_symbol -> delete_welding_symbol
DrawingWeldingSymbols.CreateDefinitions() -> DrawingWeldingSymbolDefinitions.Add(definitionIndex) -> DrawingWeldingSymbols.Add(...)
DrawingWeldingSymbol.Leader.RootNode.Position -> get_welding_symbols factual post-move state
DrawingWeldingSymbol.Delete()
```

Important Inventor API finding: `DrawingWeldingSymbolDefinitions.Add(definitionIndex)`
is the confirmed working initialization pattern. `Type.Missing` for
`TargetIndex` was not usable in live Inventor 2027 and produced
`DrawingWeldingSymbols.Add` `E_FAIL`. `E_FAIL` while reading properties that
are not applicable to a specific weld symbol type remains property-level
diagnostics and does not mean the symbol object failed.

Package 48 verifies generic drawing SketchedSymbol primitives:

```text
get_sketched_symbol_definitions -> external caller selects exact definitionName
get_sketched_symbol_definitions -> create_sketched_symbol [SketchedSymbols.Add] -> get_drawing_text_objects -> move_sketched_symbol [SketchedSymbol.Position] -> get_drawing_text_objects -> delete_sketched_symbol -> get_drawing_text_objects
get_sketched_symbol_definitions -> get_drawing_curves -> create_sketched_symbol [SketchedSymbols.AddWithLeader] -> optional GeometryIntent attachment -> get_drawing_text_objects
```

Verified native APIs include `DrawingDocument.SketchedSymbolDefinitions`,
`Sheet.SketchedSymbols`, `SketchedSymbols.Add(...)`,
`SketchedSymbols.AddWithLeader(...)`, `SketchedSymbol.Position`, and
`SketchedSymbol.Delete()`.

Prompt-status limitation is intentional: local Inventor 2027 `TextBox` Interop
exposes `Text` and `FormattedText`, but no dedicated prompted-entry flag was
confirmed. Runtime does not infer prompt semantics from `<Prompt>` or formatted
text and does not fabricate prompted values.

`get_revision_clouds` is verified for reading `Sheet.RevisionClouds`, RevisionCloud metadata, RevisionCloudDefinition data, control points, reference keys, and diagnostics.

Package 53A verifies native RevisionCloud lifecycle primitives:

```text
get_revision_clouds -> create_revision_cloud -> get_revision_clouds -> move_revision_cloud -> get_revision_clouds -> delete_revision_cloud -> get_revision_clouds
RevisionClouds.CreateRevisionCloudDefinition(...) -> RevisionClouds.Add(...)
RevisionCloud.Position -> get_revision_clouds factual post-move state
RevisionCloud.Delete()
```

Live Inventor validation confirmed four caller-supplied control points,
`inverted = false`, referenceKey, selectorSnapshot, readable definition, native
layer inheritance, and generated native name such as `Пометочное_облако1`.
`RevisionCloud.Position` translated the native cloud/control-point coordinates
as Inventor behavior; Runtime did not edit individual control points.
Control-point editing and revision association remain deferred.

`get_edge_symbols` is verified for reading `Sheet.EdgeSymbols`, EdgeSymbol metadata, EdgeSymbolDefinition data, reference keys, and diagnostics.

Package 54A verifies native EdgeSymbol lifecycle primitives:

```text
get_edge_symbols -> create_edge_symbol -> get_edge_symbols -> move_edge_symbol -> get_edge_symbols -> delete_edge_symbol -> get_edge_symbols
EdgeSymbols.CreateDefinition(...) -> EdgeSymbols.Add(...)
EdgeSymbol.Position -> get_edge_symbols factual post-move state
EdgeSymbol.Delete()
```

Live Inventor validation confirmed `valuePositionType =
kEdgeSymbolValueNoValues`, `indicationType = kAllEdgesIndicationType`,
referenceKey, selectorSnapshot, readable native definition, natively inherited
layer/style, and unchanged definition after move. Package 54A uses explicit
caller `Point2d` leader points only; it does not implement GeometryIntent
attachment or automatic geometry selection.

`get_transition_symbols` is verified for reading `Sheet.TransitionSymbols`, TransitionSymbol metadata, TransitionSymbolDefinition data, leader/attachment metadata, reference keys, and diagnostics.

Package 55A verifies native TransitionSymbol lifecycle primitives:

```text
get_transition_symbols -> create_transition_symbol -> get_transition_symbols -> move_transition_symbol -> get_transition_symbols -> delete_transition_symbol -> get_transition_symbols
TransitionSymbols.CreateDefinition(...) -> TransitionSymbols.Add(...) -> created TransitionSymbol.Position
TransitionSymbol.Position -> get_transition_symbols factual post-move state
TransitionSymbol.Delete()
```

Live Inventor validation confirmed that caller `leaderPoints` supplied to
`TransitionSymbols.Add(...)` do not factually determine free
`kNoAttachmentType` symbol placement. Package 55A therefore requires explicit
caller `x/y` for creation and sets the created object's
`TransitionSymbol.Position = Point2d(x,y)` as part of the native construction
sequence. Runtime does not infer placement from `leaderPoints`.

Valid free `kNoAttachmentType` TransitionSymbols may have `Leader`,
`Leader.HasRootNode = false`, and null/unavailable `Leader.AllNodes`. Runtime
records unavailable leader-node count as nullable diagnostic data and does not
fabricate zero or fail movement solely because leader node count is unavailable.
GeometryIntent/drawing-view attachment and automatic geometry selection remain
deferred.

`create_drawing_document` is verified for creating a new Autodesk Inventor `DrawingDocument` through `Application.Documents.Add` with an explicit `templatePath`.

`export_pdf` is verified for exporting the active Autodesk Inventor `DrawingDocument` through the PDF Translator Add-In and `TranslatorAddIn.SaveCopyAs`, with overwrite protection and output file verification.

`export_dwg` is verified for exporting the active Autodesk Inventor `DrawingDocument` through the DWG Translator Add-In and `TranslatorAddIn.SaveCopyAs`, with caller-supplied DWG INI, overwrite protection, output file verification, and no interactive dialog.

`export_dxf` is verified for exporting the active Autodesk Inventor `DrawingDocument` through the DXF Translator Add-In and `TranslatorAddIn.SaveCopyAs`, with caller-supplied DXF INI, overwrite protection, output file verification, and no interactive dialog.

`create_parts_list` is verified for creating exactly one Inventor `Sheet.PartsLists` object from an explicitly selected existing `DrawingView` and explicit placement point.

`create_balloon` is verified for creating exactly one Inventor `Balloon` from an explicitly selected `DrawingView` curve through `Sheet.CreateGeometryIntent` and `Sheet.Balloons.Add`.

Package 31-36 commands are verified for angular, ordinate, baseline, and chain
dimension creation plus explicit general dimension text, visibility,
precision, model-value override, style, and layer edits.

Ordinate dimension creation requires an existing DrawingView OriginIndicator:

```text
DrawingView geometry -> GeometryIntent -> DrawingView.CreateOriginIndicator(...) -> OrdinateDimensions.Add(...)
```

Baseline and chain dimension creation is verified with explicit ordered
`GeometryIntent` selectors. General dimension editing commands operate only on
explicitly selected `GeneralDimension` objects and do not choose formatting,
style, layer, tolerance, placement, or engineering semantics.

Package 37-39 commands are verified for reading and setting general dimension
tolerance state through explicit caller-selected `GeneralDimension` objects.
Coverage includes default, basic, reference, symmetric, deviation, limits, and
fits tolerance modes. Runtime performs no unit conversion, sign normalization,
upper/lower reordering, fit validation, engineering selection, or GOST/ESKD
tolerance decisions.

Observed Inventor API behavior: after `Tolerance.SetToDefault()`,
`ToleranceType` becomes `kDefaultTolerance` and `Upper` / `Lower` reset to `0`,
but previous `HoleTolerance` / `ShaftTolerance` strings may remain readable.
Runtime does not compensate for or clear those strings.

Package 40-41 commands are verified for center mark and centerline creation.
`get_center_marks` and `get_centerlines` now expose reference keys where
supported. `create_centerline_centered_pattern` is verified for explicit
pattern-center and member `GeometryIntent` objects passed through an
`ObjectCollection` to `Centerlines.AddCenteredPattern`.

Package 42 commands are verified for the Hole / Thread annotation pipeline.
`get_hole_features` is hardened for expanded `HoleFeature` facts, reference
keys, tapped-hole `ThreadInfo`, and property-level diagnostics.
`get_thread_features` is verified for standalone external Inventor
`ThreadFeature` facts, including the M15x1.5 / 6g test thread and reference
key. Existing `create_hole_thread_note` is verified as sufficient for
standalone `ThreadFeature` drawing annotation from an explicit `kThreadEdge`
drawing curve. `get_hole_thread_notes` is hardened and verified for the
created standalone thread note with non-blocking property diagnostics.

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
reconstruct note text, modify the model, or perform GOST/ESKD decisions.

Package 43-44 commands are verified for native Inventor GeneralNote and
LeaderNote primitives. GeneralNote lifecycle is verified as
`create -> read -> edit -> read -> move -> read -> delete -> read`, with final
`get_general_notes` count `0`. LeaderNote lifecycle is verified for both free
and attached leader notes. A free LeaderNote has `attachedEntity = null` as a
valid factual state. An attached LeaderNote is verified with explicit
`viewName = ВИД1`, `curveIndex = 14`, and `intent = mid`; Inventor returned
`pointOnSheet = (23.65, 15.6)` for the selected curve midpoint, leader
attachment metadata, and a reference key. Runtime reports requested and actual
positions without compensating for Inventor leader-geometry constraints.

## Experimental commands

These commands exist for compatibility only and must not be used as Runtime coverage for atomic Eyes/Hands:

- `analyze_dimension_layout`
- `auto_arrange_dimensions`
- `check_annotation_collisions`
- `auto_resolve_annotation_collisions`
- `analyze_view_dimension_candidates`

## Package 17 result

Package 17A confirmed that `get_custom_tables` reads Autodesk Inventor `Sheet.CustomTables` as a typed Eye:

```text
Sheet.CustomTables
CustomTable metadata
Columns
Rows
Cells
MergedCells
reference keys
```

This is not a specification parser and not a PartsList reader.

## Package 18 result

Package 18A confirmed that `get_drawing_text_objects` reads drawing text-like objects as a typed Eye:

```text
Sheet.DrawingNotes.GeneralNotes
Sheet.DrawingNotes.LeaderNotes
Sheet.DrawingNotes.HoleThreadNotes
Sheet.DrawingNotes.BendNotes
Sheet.DrawingNotes.ChamferNotes
Sheet.DrawingNotes.PunchNotes
Sheet.Sketches / DrawingSketch.TextBoxes
Sheet.SketchedSymbols
```

This is not semantic text analysis, TT/TU recognition, or GOST interpretation.

## Package 19 result

Package 19A confirmed that `get_feature_control_frames` reads Autodesk Inventor `Sheet.FeatureControlFrames` as a typed Eye:

```text
FeatureControlFrame metadata
FeatureControlFrameRows
tolerance fields
datum fields
reference keys
diagnostics
```

This is not tolerance interpretation, GOST validation, GD&T semantic analysis, or engineering analysis.

## Package 20 result

Package 20A confirmed that `get_surface_texture_symbols` reads Autodesk Inventor `Sheet.SurfaceTextureSymbols` as a typed Eye:

```text
SurfaceTextureSymbol metadata
position
layer
style
leader
roughness fields
production fields
sampling fields
definition data
reference keys
diagnostics
```

This is not roughness interpretation, GOST validation, surface texture semantic analysis, or engineering analysis.

## Package 21 result

Package 21A confirmed that `get_welding_symbols` reads Autodesk Inventor `Sheet.WeldingSymbols` as a typed Eye:

```text
DrawingWeldingSymbols collection
DrawingWeldingSymbol metadata
DrawingWeldingSymbolDefinition
WeldSymbolOne
WeldSymbolTwo
reference keys
diagnostics
```

This is not weld interpretation, GOST validation, welding semantic analysis, or engineering analysis.

## Package 22 result

Package 22 completed the Drawing Symbol Layer with three additional typed Eyes:

```text
Sheet.RevisionClouds
RevisionCloud metadata
RevisionCloudDefinition
RevisionCloudControlPoints
reference keys
diagnostics

Sheet.EdgeSymbols
EdgeSymbol metadata
EdgeSymbolDefinition
reference keys
diagnostics

Sheet.TransitionSymbols
TransitionSymbol metadata
TransitionSymbolDefinition
leader metadata
attachment metadata
reference keys
diagnostics
```

Together with Packages 19A, 20A, and 21A, the verified Drawing Symbol Layer is:

```text
FeatureControlFrames          VERIFIED
SurfaceTextureSymbols         VERIFIED
WeldingSymbols                VERIFIED
RevisionClouds                VERIFIED
EdgeSymbols                   VERIFIED
TransitionSymbols             VERIFIED
```

This is not symbol interpretation, GOST/ISO validation, correctness checking, or engineering analysis.

## Package 23 result

Package 23B confirmed that `create_drawing_document` creates a new Autodesk Inventor drawing document as an atomic Hand:

```text
Application.Documents.Add
DocumentTypeEnum.kDrawingDocumentObject
explicit templatePath
optional visible
created DrawingDocument metadata
```

This is not automatic template selection, drawing generation, view creation, title block logic, export, GOST/ESKD interpretation, or engineering analysis.

Next Drawing Generation Hands:

```text
create_section_view
create_detail_view
create_auxiliary_view
add_drawing_view_break
```

## Priority notes

- P0: choose the next practical engineering layer after verified PDF/DWG/DXF export.
- P1: continue atomic Drawing Views / Drawing Dimensions / HoleThreadNotes improvements only where audits confirmed gaps.
- P2: add specialized annotation/table capabilities only after typed Eyes define reliable selector snapshots.

Do not add automatic scenarios such as auto-placement, collision resolution, best-view selection, dimension selection, BOM generation, specification parsing, or complete drawing generation into the C# Runtime.

## Package 24 result

Drawing Generation Hands — Section View Pipeline: **VERIFIED**

Commands: `create_drawing_document`, `create_base_view`, `create_section_line`,
`create_section_view`.

Coverage is the explicit pipeline `DrawingDocument → Base View → Section Line →
Section View`. `create_section_line` uses `parentView.Sketches.Add()` and
`SheetToSketchSpace`; `create_section_view` uses `DrawingViews.AddSectionView2`.
No automatic placement, direction selection, model analysis, GOST/ESKD checks,
or engineering decisions are performed. Next capability check:
`create_detail_view` Hand.

## Package 25 result

Drawing Generation Hands — Detail View Pipeline: **VERIFIED**

Command: `create_detail_view`.

Coverage: circular `DrawingViews.AddDetailView`, explicit parent view,
position, circular fence center/radius, style, optional scale, label, name,
reference key, and diagnostics. Rectangular fences and automatic placement or
scale selection are not implemented. Next capability check:
`create_auxiliary_view` Hand.

## Package 27 result

Drawing Generation Hands — Drawing View Break: **VERIFIED**

Command: `add_drawing_view_break`.

Coverage: `DrawingView.BreakOperations.Add`, horizontal and vertical
orientation, rectangular style, BreakOperation reference key, actual returned
gap/number-of-symbols values, and diagnostics. Runtime preserves Inventor's
post-creation values and performs no automatic geometry or engineering logic.
Next capability check: Drawing Export Hands.

## Package 28 result

Drawing Export Hands - PDF Export Pipeline: **VERIFIED**

Command: `export_pdf`.

Coverage: active `DrawingDocument`, PDF Translator Add-In resolution,
`TranslationContext`, `NameValueMap`, `DataMedium`,
`TranslatorAddIn.SaveCopyAs`, overwrite protection, `overwrite=true`, output
file existence verification, file size/timestamp facts, and diagnostics.

Confirmed PDF Translator:

```text
ClientId: {0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}
DisplayName: Translator: PDF / Translyator: PDF
supportsSaveCopyAs = true
translatorAvailable = true
```

Runtime does not choose output paths, create folders, overwrite without
explicit permission, tune PDF options, regenerate drawing geometry, or perform
engineering/GOST logic.

Next capability check: Drawing Export Hands - DWG / DXF.

## Package 29 result

Drawing Export Hands - DWG/DXF Export Pipeline: **VERIFIED**

Commands:

```text
export_dwg
export_dxf
```

DWG coverage:

```text
DWG Translator ClientId {C24E3AC2-122E-11D5-8E91-0010B541CD80}
HasSaveCopyAsOptions
NameValueMap.Value["Export_Acad_IniFile"]
caller-supplied DWG INI
direct DWG output
overwrite=false protection
overwrite=true support
output file verification
unsaved DrawingDocument support
no interactive dialog
```

DXF coverage:

```text
DXF Translator ClientId {C24E3AC4-122E-11D5-8E91-0010B541CD80}
HasSaveCopyAsOptions
NameValueMap.Value["Export_Acad_IniFile"]
caller-supplied DXF INI
direct DXF output when transmittal is disabled
overwrite=false protection
overwrite=true support
output file verification
unsaved DrawingDocument support
no interactive dialog
```

The public Inventor `exportdxf.ini` had `USE TRANSMITTAL=Yes` and produced a
ZIP package containing the DXF. Direct DXF output was verified with a
caller-supplied DXF INI where transmittal is disabled. Runtime does not
generate INI files, choose AutoCAD versions, choose mappings/layers, choose
transmittal behavior, create directories, overwrite without explicit
permission, or perform engineering/GOST decisions.

Next capability check: choose the next practical engineering layer.

## Package 30 result

Parts List + Balloon Pipeline: **VERIFIED**

Commands:

```text
create_parts_list
create_balloon
```

Verified pipelines:

```text
DrawingView -> Parts List
DrawingView geometry -> GeometryIntent -> Balloon
```

Coverage:

```text
Sheet.PartsLists.Add
Sheet.CreateGeometryIntent
Sheet.Balloons.Add
Balloon reference keys
Balloon value sets
diagnostics
```

Runtime does not modify BOM data, choose item numbering automatically, choose
balloon placement, select geometry, optimize layout, or perform
engineering/GOST decisions.

Next capability check: choose the next practical engineering layer.

## Package 31-36 result

Drawing Dimensions Creation and Editing Pipeline: **VERIFIED**

Commands:

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

Coverage:

```text
GeneralDimensions.AddAngular
OrdinateDimensions.Add
DrawingView.HasOriginIndicator
DrawingView.OriginIndicator
DrawingView.CreateOriginIndicator
BaselineDimensionSets.Add
ChainDimensionSets.Add
DimensionText.FormattedText
GeneralDimension.HideValue
GeneralDimension.Precision
GeneralDimension.OverrideModelValue
GeneralDimension.ModelValueOverridden
GeneralDimension.Style
GeneralDimension.Layer
reference keys where supported
diagnostics
```

Runtime boundary remains explicit: no automatic geometry selection, no
automatic dimension placement, no automatic style/layer selection, no
tolerance decisions, no layout optimization, no engineering decisions, and no
GOST/ESKD reasoning inside Runtime.

Next capability check:

```text
Capability Audit - General Dimension Tolerance capabilities
```

## Package 37-39 result

General Dimension Tolerance Pipeline: **VERIFIED**

Commands:

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

Coverage:

```text
GeneralDimension.Tolerance
Tolerance.ToleranceType
Tolerance.Upper
Tolerance.Lower
Tolerance.HoleTolerance
Tolerance.ShaftTolerance
Tolerance.SetToDefault
Tolerance.SetToBasic
Tolerance.SetToReference
Tolerance.SetToSymmetric
Tolerance.SetToDeviation
Tolerance.SetToLimits
Tolerance.SetToFits
reference keys where supported
diagnostics
```

Runtime boundary remains explicit: caller selects the dimension and tolerance
mode, supplies numeric values and fit strings, and Runtime performs no unit
conversion, sign normalization, upper/lower reordering, fit validation,
engineering selection, or GOST/ESKD tolerance decisions.

Observed Inventor behavior: `SetToDefault()` sets `ToleranceType` to
`kDefaultTolerance` and resets upper/lower values, but previous hole/shaft
tolerance strings may remain readable. Runtime records factual API state only.

Next capability check:

```text
Capability Audit - next practical drawing engineering layer
```

## Package 40-41 result

Center Mark and Centerline Pipeline: **VERIFIED**

Commands:

```text
get_center_marks
get_centerlines
create_center_mark
create_centerline_bisector
create_centerline_centered_pattern
```

Coverage:

```text
Sheet.Centermarks
Centermark.GetReferenceKey
Sheet.Centerlines
Centerline.GetReferenceKey
Sheet.CreateGeometryIntent
Centermarks.Add
Centerlines.AddBisector
Centerlines.AddCenteredPattern
ObjectCollection of explicit GeometryIntent objects
diagnostics
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

Runtime boundary remains explicit: Runtime exposes facts, resolves caller
selectors, creates explicit `GeometryIntent` objects, calls one Inventor API
operation, and returns factual state/reference keys. Runtime does not detect
holes, detect bolt-circle patterns, select centers or members, infer symmetry,
reorder geometry, choose annotations by engineering meaning, optimize layout,
or make GOST/ESKD decisions.

Deferred:

```text
generic create_centerline
Centerlines.AddByWorkFeature centerline
delete centerline / center mark commands
```

Next capability check:

```text
Capability Audit - Hole / Thread annotation capabilities
```

## Package 26 result

Drawing Generation Hands — Auxiliary View Pipeline: **VERIFIED**

Command: `create_auxiliary_view`.

Coverage: explicit parent view, orientation curve index/snapshot, sheet
position, style, optional scale, label, name, reference key, and diagnostics
through `DrawingViews.AddAuxiliaryView`. Inventor may adjust the requested
position; Runtime preserves the returned actual position and does not
compensate automatically. Next capability: `add_drawing_view_break`.
