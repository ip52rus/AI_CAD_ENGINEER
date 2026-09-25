# AGENTS.md — AI CAD ENGINEER

Binding instructions for AI coding/CAD agents.

## Project model

```text
External LLM
→ JSON
→ InventorCommandDispatcher
→ atomic Eye / Hand
→ Autodesk Inventor API
```

Runtime exposes facts and explicit actions. Engineering reasoning stays outside Runtime.

## Capability Audit before code

Before adding anything:

1. inspect `InventorControl/InventorCommandDispatcher.cs`;
2. search the proposed command name;
3. search equivalent commands/support helpers;
4. read `CURRENT_STATE.md` and `CAPABILITY_MAP.md`;
5. inspect real Inventor API/interops;
6. classify: already implemented / partial / genuinely missing / external reasoning;
7. decide whether code is required.

If existing capability is sufficient, **do not write code**.

## Eye contract

An Eye:

- reads one factual domain;
- does not mutate Inventor;
- does not infer engineering intent;
- returns structured JSON and useful traceability;
- preserves native values where useful;
- reports unavailable data explicitly.

## Hand contract

A Hand:

- executes one explicit Inventor action;
- receives target and requested value from caller;
- does not auto-select by engineering meaning;
- does not optimize;
- does not silently alter another object as fallback;
- returns direct factual readback where practical.

Good examples:

- `move_drawing_view`
- `set_general_dimension_precision`
- `set_drawing_view_occurrence_visibility`
- `set_custom_table_cell_value`

Bad examples:

- `create_correct_drawing`
- `isolate_welded_unit`
- `fix_eskd_automatically`

## Do not restore the legacy brain

Do not recreate:

- `EngineeringBrain`
- `DrawingManager`
- embedded Planning/Decision systems
- product-specific “make the whole drawing” commands

The pre-v0.15 architecture remains in Git history.

## Design intent

Never invent absent:

- material specification;
- tolerances/fits;
- weld size/process;
- fastener specification;
- coating;
- official designation;
- manufacturing sequence.

Unknown engineering content remains unresolved.

## Geometry evidence

For final geometry prefer:

1. final BRep;
2. explicit parameters / feature details;
3. feature taxonomy/name;
4. filename conventions.

Do not equate “not a HoleFeature” with “not a hole”.

## Referenced documents

Prefer occurrence/reference-context reads over activating referenced documents. Do not dirty/save a referenced part just to inspect it.

## Drawing safety

Audit/read mode:

- no drawing mutation;
- no model mutation;
- no save.

Write mode:

- mutate only the explicitly authorized document;
- never change source assembly visibility/suppression as a fallback for DrawingView-local behaviour;
- verify dirty states after E2E.

## Visual QA

A successful API call is not proof of a good drawing.

```text
create
→ factual readback
→ layout map
→ preview
→ external visual review
→ atomic correction
→ preview
```

## Build

Validated path:

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
```

Do not treat `dotnet build` as the authoritative validation path for this COM project.

## Verification

A new capability is not VERIFIED until:

- build passes;
- registry remains unique;
- live Inventor execution passes;
- direct readback confirms effect;
- source-model integrity is checked.

## Git

- inspect `git status` first;
- one coherent capability per checkpoint;
- no unrelated cleanup in capability commits;
- no temporary Inventor files/previews/payloads in Git;
- no destructive Git operation without explicit approval;
- no `git push` without explicit approval.

## Legacy experimental helpers

The following remain but are not architectural examples:

- `analyze_dimension_layout`
- `auto_arrange_dimensions`
- `check_annotation_collisions`
- `auto_resolve_annotation_collisions`
- `analyze_view_dimension_candidates`

## Research conclusion

Do not infer from “219 commands” that autonomous drafting is solved.

The project validated Inventor automation much more strongly than autonomous view/dimension/layout judgement.

Preferred future uses:

- model interrogation;
- drawing audit;
- fabrication extraction;
- batch automation;
- supervised CAD assistance.
