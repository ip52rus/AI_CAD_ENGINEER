# Current State

## Final research checkpoint

```text
v0.68
commit 30ba86d
```

## Runtime inventory

Live audit of `InventorControl/InventorCommandDispatcher.cs`:

```text
registered JSON commands: 219
unique registered commands: 219
duplicate registered names: 0
```

## Runtime architecture

```text
external LLM / caller
→ JSON
→ Program.cs
→ Core/Application.cs
→ InventorCommandDispatcher
→ IInventorCommand
→ support/read helpers
→ Autodesk Inventor API
```

Runtime is Eyes + atomic Hands.

The removed pre-v0.15 `CommandProcessor → DrawingManager → EngineeringBrain` architecture must not be restored as the default runtime model.

## Final packages

### v0.65

Referenced-part targeting for assembly-context model Eyes.

### v0.66

Referenced targeting extended to geometry-detail Eyes including face edges, feature details and model parameters.

### v0.67

Added:

- `set_drawing_view_occurrence_visibility`
- `get_drawing_view_occurrence_visibility`

Nested occurrence hide/show was verified with direct readback while source assembly remained unchanged.

### v0.68

Added:

- `set_custom_table_cell_value`
- `set_custom_table_column_width`

Verified create → write → repair → readback → layout/preview workflow.

## Project status

The original autonomous-drawing research track is complete.

The stop decision applies to autonomous human-quality drawing generation, not to the Inventor automation runtime.

The current code remains a foundation for:

- model interrogation;
- drawing/model audit;
- supervised CAD actions;
- batch automation;
- fabrication-data extraction;
- CAD-agent research.

## Documentation

- [README.md](README.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [CAPABILITY_MAP.md](CAPABILITY_MAP.md)
- [docs/COMMAND_REFERENCE.md](docs/COMMAND_REFERENCE.md)
- [docs/PROJECT_HISTORY.md](docs/PROJECT_HISTORY.md)
- [docs/RESEARCH_FINDINGS.md](docs/RESEARCH_FINDINGS.md)
- [AGENTS.md](AGENTS.md)
