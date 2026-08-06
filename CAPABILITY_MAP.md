# CAPABILITY MAP

Date: 2026-08-06

Repository: `C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER`

Branch: `cleanup/legacy-architecture`

## Ground rules

This map is intentionally conservative.

Sources used:

- `CURRENT_STATE.md`
- `PROJECT_REVIEW.md`
- completed Package 13 / 14 / 15 capability audits
- live `InventorCommandDispatcher.cs`
- Package 15A build result

Status meanings:

- `VERIFIED`: explicitly recorded as passing or verified against Inventor.
- `IMPLEMENTED_UNTESTED`: implemented and registered/buildable, but no Inventor PASS is recorded.
- `PARTIAL`: some atomic Eyes/Hands exist, but the capability is incomplete.
- `EXPERIMENTAL`: command contains analysis, heuristics, automatic selection, layout, or optimization.
- `MISSING`: no confirmed implementation.
- `OUT_OF_SCOPE`: not part of the current audited capability boundary.

Do not treat `IMPLEMENTED_UNTESTED` as Inventor-verified.

## Capability table

| Capability | Status | Implemented commands | Inventor PASS confirmed | Experimental | Missing | Priority |
|---|---|---|---|---|---|---|
| Runtime connectivity | VERIFIED | `ping`, `get_active_document` | `ping`, `get_active_document` | - | - | P0 maintained |
| Document operations | PARTIAL | `get_open_documents`, `open_document`, `activate_document`, `update_active_document`, `save_document`, `save_document_as`, `close_document` | not separately recorded | - | create new drawing from template, export/print workflows | P1 |
| Drawing sheets | VERIFIED | `get_drawing_sheets`, `get_sheets`, `get_sheet`, `activate_sheet`, `rename_sheet`, `create_sheet`, `delete_sheet`, `set_sheet_size`, `set_sheet_orientation` | `get_drawing_sheets` | - | no additional missing items confirmed by Package audits | P0 maintained |
| Borders and title blocks | PARTIAL | `get_border_definitions`, `get_sheet_border`, `set_sheet_border`, `remove_sheet_border`, `get_title_block_definitions`, `get_sheet_title_block`, `set_sheet_title_block`, `remove_sheet_title_block`, `get_title_block_fields`, `set_title_block_field`, `set_title_block_field_by_name`, `fill_title_block`, `get_title_block_definition_text`, `set_title_block_definition_text`, `get_title_block_binding`, `get_title_block_bindings`, `get_title_block_field_map` | not separately recorded | - | no missing items confirmed by Package 13-15 audits | P1 |
| Drawing views | PARTIAL | `get_drawing_views`, `get_drawing_view`, `get_drawing_views_detailed`, `get_drawing_view_relationships`, `get_drawing_curves`, `get_view_model_references`, `get_curve_model_reference`, `create_base_view`, `create_projected_view`, `move_drawing_view`, `delete_drawing_view`, `rename_drawing_view`, `rotate_drawing_view`, `set_drawing_view_scale`, `set_drawing_view_style`, `set_drawing_view_label_visibility`, `set_drawing_view_scale_inheritance`, `set_drawing_view_alignment`, `set_drawing_view_suppressed` | drawing views / relationships / curve geometry recorded as verified areas; exact PASS command list not recorded | - | section/detail/auxiliary/break/crop views not covered by confirmed commands | P1 |
| Drawing dimensions | PARTIAL | `get_drawing_dimensions`, `get_general_dimensions_detailed`, `get_dimension_geometry`, `create_linear_dimension`, `create_diameter_dimension`, `create_radius_dimension`, `move_drawing_dimension`, `move_general_dimension_text`, `move_linear_dimension`, `center_general_dimension_text`, `delete_drawing_dimension`, `delete_general_dimension` | not separately recorded | `analyze_dimension_layout`, `auto_arrange_dimensions`, `analyze_view_dimension_candidates` | angular/ordinate/baseline/chain/symmetric/chamfer dimensions and additional atomic format/read variants | P1 |
| Hole/thread notes | IMPLEMENTED_UNTESTED | `get_hole_thread_notes`, `create_hole_thread_note`, `move_hole_thread_note`, `delete_hole_thread_note`, `set_hole_thread_note_format` | not separately recorded | - | stable selector variants are still missing; current commands use indexes | P1 |
| Basic drawing annotation Eyes | IMPLEMENTED_UNTESTED | `get_general_notes`, `get_leader_notes`, `get_balloons`, `get_center_marks`, `get_centerlines` | not recorded yet after Package 15A | - | Inventor PASS still required | P0 test next |
| Annotation summary and bounds | PARTIAL | `get_drawing_annotation_summary`, `get_annotation_bounds` | annotation summary recorded as verified area; exact PASS command not separately recorded | - | typed bounds for all annotation classes; current bounds coverage is incomplete | P1 |
| Annotation collision/layout logic | EXPERIMENTAL | - | not applicable | `check_annotation_collisions`, `auto_resolve_annotation_collisions` | should not be expanded as Runtime coverage | no priority |
| Surface texture symbols | MISSING | count only through `get_drawing_annotation_summary` | not separately recorded | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Weld symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Datum identifiers | MISSING | placeholder count only; no confirmed API coverage | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Datum target symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Feature control frames | PARTIAL | count only through `get_drawing_annotation_summary` | not separately recorded | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Revision symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Sketched symbols | MISSING | - | - | - | typed Eye; atomic create/move/delete/format Hands | P2 |
| Tables and parts lists | PARTIAL | `get_drawing_tables` | tables and parts lists recorded as verified area; exact PASS command not separately recorded | - | create/edit table and parts list commands are not confirmed | P2 |
| Document properties | PARTIAL | `get_document_properties`, `get_document_property`, `get_document_property_by_id`, `get_document_property_sets`, `set_document_property`, `set_document_property_by_id` | not separately recorded | - | no missing items confirmed by Package 13-15 audits | P2 |
| Model feature Eyes | VERIFIED | `get_model_feature_tree`, `get_hole_features`, `get_feature_details`, `get_model_parameters` | feature tree, feature details, parameters recorded as verified areas; exact PASS command list not recorded | - | write commands for model features are not part of confirmed Runtime scope | P0 maintained |
| Model geometry Eyes | VERIFIED | `get_surface_bodies`, `get_body_faces`, `get_face_edges` | surface bodies, faces, edges recorded as verified areas; exact PASS command list not recorded | - | no missing items confirmed by Package 13-15 audits | P0 maintained |
| Sketch / constraint / work feature Eyes | VERIFIED | `get_sketches`, `get_sketch_geometry`, `get_sketch_constraints`, `get_sketch_dimensions`, `get_work_features` | sketches, sketch geometry, constraints, dimensions, work features recorded as verified areas; exact PASS command list not recorded | - | model/sketch constraint writes are outside Package 15 audit scope | P0 maintained |
| Assembly Eyes | VERIFIED | `get_assembly_summary`, `get_assembly_occurrences`, `get_assembly_constraints`, `get_assembly_bom`, `get_assembly_referenced_documents` | assembly summary, occurrences, constraints, BOM, referenced documents recorded as verified areas; exact PASS command list not recorded | - | assembly Hands are not confirmed | P1 |
| Diagnostics / metadata | PARTIAL | `get_application_addins`, `get_gost_metadata` | not separately recorded | - | no missing items confirmed by Package 13-15 audits | P2 |

## Commands with explicit Inventor PASS recorded

The only exact JSON commands explicitly listed as confirmed in `CURRENT_STATE.md` are:

```json
{"command":"ping"}
```

```json
{"command":"get_active_document"}
```

```json
{"command":"get_drawing_sheets"}
```

`CURRENT_STATE.md` also records verified read areas, but does not list every exact JSON command that produced those PASS results.

## Experimental commands

These commands exist for compatibility only and must not be used as Runtime coverage for atomic Eyes/Hands:

- `analyze_dimension_layout`
- `auto_arrange_dimensions`
- `check_annotation_collisions`
- `auto_resolve_annotation_collisions`
- `analyze_view_dimension_candidates`

## Package 15A status

Package 15A added basic drawing annotation Eyes:

- `get_general_notes`
- `get_leader_notes`
- `get_balloons`
- `get_center_marks`
- `get_centerlines`

Build passed after implementation.

Inventor PASS is not recorded yet, so these commands remain `IMPLEMENTED_UNTESTED`.

## Priority notes

- P0: keep verified core Eyes working; run Inventor PASS for Package 15A basic annotation Eyes next.
- P1: continue atomic Drawing Views / Drawing Dimensions / HoleThreadNotes improvements only where audits confirmed gaps.
- P2: add specialized annotation and metadata capabilities only after typed Eyes define reliable selector snapshots.

Do not add automatic scenarios such as auto-placement, collision resolution, best-view selection, dimension selection, or complete drawing generation into the C# Runtime.
