# CAPABILITY MAP

Date: 2026-08-07

Repository: `C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER`

Branch: `cleanup/legacy-architecture`

Checkpoint before this documentation sync:

```text
4fd8b75 v0.18 complete Drawing Text Objects Eye
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
| Document operations | PARTIAL | `get_open_documents`, `open_document`, `activate_document`, `update_active_document`, `save_document`, `save_document_as`, `close_document` | not separately recorded | - | create new drawing from template, export/print workflows | P1 |
| Drawing sheets | VERIFIED | `get_drawing_sheets`, `get_sheets`, `get_sheet`, `activate_sheet`, `rename_sheet`, `create_sheet`, `delete_sheet`, `set_sheet_size`, `set_sheet_orientation` | `get_drawing_sheets` | - | no additional missing items confirmed by Package audits | P0 maintained |
| Borders and title blocks | PARTIAL | `get_border_definitions`, `get_sheet_border`, `set_sheet_border`, `remove_sheet_border`, `get_title_block_definitions`, `get_sheet_title_block`, `set_sheet_title_block`, `remove_sheet_title_block`, `get_title_block_fields`, `set_title_block_field`, `set_title_block_field_by_name`, `fill_title_block`, `get_title_block_definition_text`, `set_title_block_definition_text`, `get_title_block_binding`, `get_title_block_bindings`, `get_title_block_field_map` | not separately recorded | - | no missing items confirmed by Package 13-17 audits | P1 |
| Drawing views | PARTIAL | `get_drawing_views`, `get_drawing_view`, `get_drawing_views_detailed`, `get_drawing_view_relationships`, `get_drawing_curves`, `get_view_model_references`, `get_curve_model_reference`, `create_base_view`, `create_projected_view`, `move_drawing_view`, `delete_drawing_view`, `rename_drawing_view`, `rotate_drawing_view`, `set_drawing_view_scale`, `set_drawing_view_style`, `set_drawing_view_label_visibility`, `set_drawing_view_scale_inheritance`, `set_drawing_view_alignment`, `set_drawing_view_suppressed` | drawing views / relationships / curve geometry recorded as verified areas; exact PASS command list not recorded for `create_base_view` | - | section/detail/auxiliary/break/crop views not covered by confirmed commands | P1 |
| Drawing dimensions | PARTIAL | `get_drawing_dimensions`, `get_general_dimensions_detailed`, `get_dimension_geometry`, `create_linear_dimension`, `create_diameter_dimension`, `create_radius_dimension`, `move_drawing_dimension`, `move_general_dimension_text`, `move_linear_dimension`, `center_general_dimension_text`, `delete_drawing_dimension`, `delete_general_dimension` | not separately recorded | `analyze_dimension_layout`, `auto_arrange_dimensions`, `analyze_view_dimension_candidates` | angular/ordinate/baseline/chain/symmetric/chamfer dimensions and additional atomic format/read variants | P1 |
| Hole/thread notes | IMPLEMENTED_UNTESTED | `get_hole_thread_notes`, `create_hole_thread_note`, `move_hole_thread_note`, `delete_hole_thread_note`, `set_hole_thread_note_format` | not separately recorded | - | stable selector variants are still missing; current commands use indexes | P1 |
| Basic drawing annotation Eyes | IMPLEMENTED_UNTESTED | `get_general_notes`, `get_leader_notes`, `get_balloons`, `get_center_marks`, `get_centerlines` | not recorded after Package 15A | - | Inventor PASS still required | P0 test when needed |
| Drawing Text Objects | VERIFIED | `get_drawing_text_objects` | `get_drawing_text_objects`, `get_drawing_text_objects` with `sheetName` | - | - | P0 maintained |
| Drawing Text semantic analysis | MISSING | - | - | - | semantic text understanding, GOST interpretation, TT/TU recognition; belongs to external LLM, not Runtime | no Runtime priority |
| Annotation summary and bounds | PARTIAL | `get_drawing_annotation_summary`, `get_annotation_bounds` | annotation summary recorded as verified area; exact PASS command not separately recorded | - | typed bounds for all annotation classes; current bounds coverage is incomplete | P1 |
| Annotation collision/layout logic | EXPERIMENTAL | - | not applicable | `check_annotation_collisions`, `auto_resolve_annotation_collisions` | should not be expanded as Runtime coverage | no priority |
| Surface texture symbols | MISSING | count only through `get_drawing_annotation_summary` | not separately recorded | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Weld symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Datum identifiers | MISSING | placeholder count only; no confirmed API coverage | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Datum target symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Feature Control Frames | VERIFIED | `get_feature_control_frames` | `get_feature_control_frames`, `get_feature_control_frames` with `sheetName` | - | atomic create/move/delete/format Hands are not confirmed | P1 maintained |
| GD&T semantic analysis | MISSING | - | - | - | tolerance interpretation, GOST validation, engineering analysis; belongs to external LLM, not Runtime | no Runtime priority |
| Revision symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Sketched symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Parts Lists | VERIFIED | `get_parts_lists`, legacy aggregate coverage through `get_drawing_tables` | `get_parts_lists` verified for `Sheet.PartsLists`; tested sheet had `Sheet.PartsLists.Count = 0` | - | create/edit/delete/move/format parts list Hands are not confirmed | P1 maintained |
| Revision Tables | VERIFIED | `get_revision_tables`, legacy aggregate coverage through `get_drawing_tables` | `get_revision_tables` found one revision table with columns, rows, cells, and metadata | - | create/edit/delete/move/format revision table Hands are not confirmed | P1 maintained |
| Drawing Table Collections Diagnostics | VERIFIED | `get_drawing_table_collections` | confirmed counts and metadata for `CustomTables`, `HoleTables`, `PartsLists`, and `RevisionTables` | - | not intended to read full row/cell content | P0 maintained |
| CustomTables Discovery | VERIFIED | `get_drawing_table_collections` | confirmed GOST table is `Sheet.CustomTables` / `kCustomTableObject` | - | - | P0 maintained |
| CustomTables Detailed Reading | VERIFIED | `get_custom_tables` | reads CustomTable metadata, Columns, Rows, Cells, MergedCells, and reference keys | - | write Hands are not confirmed | P0 maintained |
| Hole Tables detailed Eye | MISSING | - | - | - | typed detailed Eye for `Sheet.HoleTables` | P2 |
| Legacy drawing table aggregate | PARTIAL | `get_drawing_tables` | legacy parts/revision table aggregate exists; does not cover `CustomTables` or `HoleTables` | - | not a complete all-table reader | compatibility |
| Document properties | PARTIAL | `get_document_properties`, `get_document_property`, `get_document_property_by_id`, `get_document_property_sets`, `set_document_property`, `set_document_property_by_id` | not separately recorded | - | no missing items confirmed by Package 13-17 audits | P2 |
| Model feature Eyes | VERIFIED | `get_model_feature_tree`, `get_hole_features`, `get_feature_details`, `get_model_parameters` | feature tree, feature details, parameters recorded as verified areas; exact PASS command list not recorded | - | write commands for model features are not part of confirmed Runtime scope | P0 maintained |
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
{"command":"get_revision_tables","sheetName":"Лист:1"}
{"command":"get_parts_lists"}
{"command":"get_parts_lists","sheetName":"Лист:1"}
{"command":"get_drawing_table_collections"}
{"command":"get_drawing_table_collections","sheetName":"Лист:1"}
{"command":"get_custom_tables"}
{"command":"get_drawing_text_objects"}
{"command":"get_feature_control_frames"}
{"command":"get_feature_control_frames","sheetName":"Лист:1"}
{"command":"get_drawing_text_objects","sheetName":"Лист:1"}
{"command":"get_custom_tables","sheetName":"Лист:1"}
```

`get_parts_lists` is verified for reading `Sheet.PartsLists`, not for reading GOST custom specification tables.

`get_custom_tables` is verified for reading `Sheet.CustomTables`, including CustomTable metadata, columns, rows, cells, merged cells, and reference keys.

`get_drawing_text_objects` is verified for reading DrawingNotes collections, DrawingSketch TextBoxes, and SketchedSymbols as Inventor API facts.

`get_feature_control_frames` is verified for reading `Sheet.FeatureControlFrames`, frame metadata, rows, tolerance fields, datum fields, reference keys, and diagnostics.

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

## Priority notes

- P0: continue the next drawing symbol capability check before implementation.
- P1: continue atomic Drawing Views / Drawing Dimensions / HoleThreadNotes improvements only where audits confirmed gaps.
- P2: add specialized annotation/table capabilities only after typed Eyes define reliable selector snapshots.

Do not add automatic scenarios such as auto-placement, collision resolution, best-view selection, dimension selection, BOM generation, specification parsing, or complete drawing generation into the C# Runtime.
