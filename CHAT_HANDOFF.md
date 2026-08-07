# AI CAD ENGINEER — CHAT HANDOFF

## Current State

- Branch: `cleanup/legacy-architecture`
- Latest checkpoint before this commit: `13c02c9 v0.19 complete Feature Control Frames Eye`
- Latest completed package: Package 20A - Surface Texture Symbols Eye
- Current milestone: Surface Texture Symbols Eye is implemented, built, registered, Inventor-validated, and marked `VERIFIED`
- Live command inventory after Package 20A: `116 registered / 116 unique`, no duplicate command names, no unregistered command classes

## Architecture Rules

- Runtime = Eyes + Hands.
- Eyes read Autodesk Inventor facts.
- Hands execute one explicit atomic Inventor action.
- The external LLM performs reasoning, interpretation, selection, planning, and engineering decisions.
- Runtime does not make engineering decisions.
- Runtime must not restore `EngineeringBrain`, `DrawingManager`, `Planning`, `Decision`, or similar embedded reasoning systems.
- Do not add automatic layout, optimization, semantic classification, or best-choice logic to C# Runtime.
- Do not describe GOST `CustomTable` objects as `PartsList` objects.

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

## Drawing Text Objects Coverage

`get_drawing_text_objects` reads Inventor API facts from:

- `Sheet.DrawingNotes.GeneralNotes`;
- `Sheet.DrawingNotes.LeaderNotes`;
- `Sheet.DrawingNotes.HoleThreadNotes`;
- `Sheet.DrawingNotes.BendNotes`;
- `Sheet.DrawingNotes.ChamferNotes`;
- `Sheet.DrawingNotes.PunchNotes`;
- `Sheet.Sketches` / `DrawingSketch.TextBoxes`;
- `Sheet.SketchedSymbols`.

The command returns document/sheet metadata, `usedActiveSheet`, collection counts, items, reference keys where available, selector snapshots, and diagnostics.

## Feature Control Frames Coverage

`get_feature_control_frames` reads Inventor API facts from:

- `Sheet.FeatureControlFrames`;
- `FeatureControlFrame` metadata;
- `FeatureControlFrameRows`;
- tolerance fields;
- datum fields;
- reference keys;
- diagnostics.

The command does not interpret tolerances, validate GOST/ESKD compliance, or perform GD&T semantic analysis.

## Surface Texture Symbols Coverage

`get_surface_texture_symbols` reads Inventor API facts from:

- `Sheet.SurfaceTextureSymbols`;
- `SurfaceTextureSymbol` metadata;
- position;
- layer;
- style;
- leader;
- roughness fields;
- production fields;
- sampling fields;
- definition data;
- reference keys;
- diagnostics.

The command does not interpret roughness values, validate GOST/ESKD compliance, or perform surface texture semantic analysis.

## Known Limitations

- Semantic text understanding is not implemented in Runtime.
- GOST interpretation is not implemented in Runtime.
- TT/TU recognition is not implemented in Runtime.
- GD&T semantic interpretation is outside Runtime.
- Surface texture semantic interpretation is outside Runtime.
- Engineering interpretation remains the responsibility of the external LLM.
- Typed detailed Eye for `Sheet.HoleTables` is still missing.
- Welding symbols still need a typed Eye if selected by the next capability check.
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
Capability Check - Welding Symbols Eye
```

Start the next chat by reading `AGENTS.md`, `CURRENT_STATE.md`, `CAPABILITY_MAP.md`, and this `CHAT_HANDOFF.md`. Then audit the live repository before proposing or writing code.

Do not implement the next capability until the audit proves the gap and the user authorizes implementation.
