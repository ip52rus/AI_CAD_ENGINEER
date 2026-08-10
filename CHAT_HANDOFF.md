# AI CAD ENGINEER - CHAT HANDOFF

## Current State

- Branch: `cleanup/legacy-architecture`.
- Current checkpoint: `v0.28 complete PDF export pipeline`.
- Latest commit after checkpoint commit: see `git log -1 --oneline --decorate`.
- Latest completed package: Package 28 - `export_pdf` Hand.
- Registry after Package 28: `127 registered / 127 unique`, `0` duplicate command names.
- Working tree is expected to be clean after the Package 28 checkpoint commit.

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

Verified end-to-end pipeline:

```text
DrawingDocument
-> Base/Projected/Section/Detail/Auxiliary Views
-> Drawing View Break
-> PDF Export
```

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

- DWG export Hand is not implemented yet.
- DXF export Hand is not implemented yet.
- Print workflow is not implemented yet.
- PDF option tuning is not implemented in Runtime.
- Runtime does not choose output paths.
- Runtime does not create output directories.
- Runtime does not overwrite unless `overwrite=true`.
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
Capability Audit - Drawing Export Hands (DWG / DXF)
```

Start the next chat by reading `AGENTS.md`, `CURRENT_STATE.md`,
`CAPABILITY_MAP.md`, and this `CHAT_HANDOFF.md`. Then audit the live
repository before proposing or writing code.

Do not implement the next capability until the audit proves the gap and the
user authorizes implementation.
