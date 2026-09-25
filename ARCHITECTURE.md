# Architecture

## Purpose

AI CAD ENGINEER is a local bridge between an external reasoning agent and Autodesk Inventor.

The final architecture separates reasoning, facts, actions and CAD state.

```text
External LLM / caller
        │ JSON
        ▼
Program.cs
        ▼
Core/Application.cs
        ▼
InventorControl/InventorCommandDispatcher.cs
        ▼
IInventorCommand
        ▼
CommandSupport / ReadSupport
        ▼
Autodesk Inventor COM/API
```

The v0.68 dispatcher registers **219 unique JSON command names**.

## Eyes

An Eye is an atomic read operation.

Requirements:

- no document mutation;
- no engineering decision;
- factual structured output;
- traceability to the source object where practical;
- native Inventor values/enums preserved when useful;
- unavailable facts reported instead of invented.

Typical domains: documents, sheets, views, drawing curves, dimensions, title blocks, annotations, tables, model features, parameters, sketches, BRep, assembly occurrences, referenced documents, BOM, previews and layout facts.

## Hands

A Hand performs one explicit action.

Requirements:

- caller chooses the target;
- caller supplies requested value/position/geometry;
- no hidden optimization;
- no automatic engineering choice;
- no fallback that changes source-model intent;
- direct factual readback where practical.

Examples:

- move one DrawingView;
- create one section view;
- set one title-block field;
- create one dimension;
- change one tolerance mode;
- hide one occurrence in one DrawingView;
- set one CustomTable cell.

## Legacy architecture

Before v0.15 the project used:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
→ Drawing
```

That code attempted to choose views, dimensions and layout inside C#.

It was removed at v0.15. The tag `v0.15-before-cleanup` preserves the old phase.

## Why the architecture changed

CAD facts are not the same thing as engineering intent.

Observed examples:

- real holes may be `ExtrudeFeature`, not `HoleFeature`;
- native assembly hierarchy may not match manufacturing hierarchy;
- Frame Generator `B_L` may differ from final cut geometry;
- mirrored parts may or may not be manufacturing-equivalent;
- the most line-dense projection may not be the best main view.

Therefore Runtime exposes facts rather than semantic conclusions.

## Geometry evidence hierarchy

For final manufacturing geometry:

1. final BRep;
2. explicit parameters / feature details;
3. feature taxonomy/name;
4. filename conventions.

Feature history remains useful, but cannot override the final body shape.

## Referenced-part targeting

v0.65/v0.66 introduced reusable nested-part resolution:

```text
active AssemblyDocument
→ occurrencePath
→ ComponentOccurrence
→ Definition.Document
→ PartDocument
→ existing Eye logic
```

This avoided document switching and preserved source assembly state.

## DrawingView-local occurrence control

v0.67 added per-view visibility through `DrawingView.SetVisibility` / `GetVisibility`.

This allows an external agent to represent a conceptual manufacturing group without suppressing/hiding occurrences in the source assembly.

## CustomTable editing

v0.68 added atomic:

- cell value write;
- column width write.

A semantic command such as “create fabrication schedule” was intentionally not added.

## Visual QA loop

API success is not drawing quality.

```text
create
→ factual readback
→ get_drawing_layout_map
→ capture_drawing_sheet_preview
→ external visual review
→ atomic correction
→ render again
```

## Source-model integrity

Drawing workflows repeatedly verified:

- source assembly/part dirty state;
- referenced document dirty state;
- modal-dialog behaviour;
- absence of unintended saves;
- drawing-local rather than model-global mutations.

## Experimental exceptions

Legacy commands that go beyond strict Eyes/Hands remain for compatibility:

- `analyze_dimension_layout`
- `auto_arrange_dimensions`
- `check_annotation_collisions`
- `auto_resolve_annotation_collisions`
- `analyze_view_dimension_candidates`

They are not the pattern for new Runtime development.

## Embedded OpenAI client

`AI/OpenAiClient.cs` is an experimental adapter. The active Runtime boundary is JSON and does not depend on an embedded model client, keeping orchestration provider-agnostic.

## Conclusion

The Inventor control layer generalized well.

The fully autonomous drawing-decision layer did not.

Future development should strengthen factual CAD access and supervised workflows rather than rebuild a hidden engineering brain inside Runtime.
