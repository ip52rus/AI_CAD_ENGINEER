# Capability Map

Checkpoint: **v0.68**

Dispatcher audit:

- **219** registered JSON commands
- **219** unique command names
- **0** duplicate registered names

Status vocabulary:

- **VERIFIED** — recorded live Inventor PASS exists;
- **PARTIAL** — useful operations exist but the domain is not complete;
- **EXPERIMENTAL** — contains analysis/automation beyond strict atomic Eyes/Hands.

| Capability area | Status | Notes |
|---|---|---|
| Runtime connectivity | VERIFIED | interactive JSON, `--json`, `--json-file` |
| Document lifecycle | PARTIAL | open/activate/update/save/save-as/close; save/export hardened against modal dialogs |
| Drawing sheets | VERIFIED | read/create/delete/activate/rename/size/orientation |
| Borders / title blocks | PARTIAL | definitions, set/remove, fields, bindings, fill |
| Drawing views | VERIFIED core | base/projected/section/detail/auxiliary, breaks, move/rotate/scale/style/alignment |
| DrawingView relationships | VERIFIED | detailed readback and model references |
| Per-view occurrence visibility | VERIFIED | v0.67 source-safe set/get visibility |
| Drawing dimensions | VERIFIED broad | linear/diameter/radius/angular/ordinate/baseline/chain + edits |
| Dimension tolerances | VERIFIED | default/basic/reference/symmetric/deviation/limits/fits |
| Center marks / centerlines | VERIFIED | create/read/delete + pattern/bisector |
| Hole/thread notes | VERIFIED | create/read/move/delete/format |
| General / leader notes | VERIFIED | lifecycle support |
| Feature control frames | VERIFIED | lifecycle support |
| Surface texture symbols | VERIFIED | lifecycle support |
| Welding symbols | VERIFIED | lifecycle support |
| Sketched symbols | VERIFIED | definitions + lifecycle |
| Revision clouds / tables | VERIFIED | lifecycle support |
| Edge / transition symbols | VERIFIED | lifecycle support |
| Bend / chamfer / punch notes | VERIFIED | explicit-geometry lifecycle support |
| Balloons | VERIFIED | lifecycle support |
| Parts lists | VERIFIED | lifecycle support |
| CustomTables | VERIFIED core | lifecycle + v0.68 cell/column editing |
| HoleTables | VERIFIED | lifecycle support |
| Drawing preview | VERIFIED | full-sheet raster preview |
| Model preview | VERIFIED | standard active-Part orientations |
| Drawing layout map | VERIFIED | normalized sheet-space factual map |
| Model features / parameters | VERIFIED | feature tree, details, parameters |
| Part BRep | VERIFIED | bodies, faces, edges |
| Sketch Eyes | VERIFIED read-only | sketches, geometry, constraints, dimensions |
| Assembly Eyes | VERIFIED | summary, recursive occurrences, constraints, BOM, references |
| Referenced-part geometry | VERIFIED | v0.65/v0.66 `target.occurrencePath` |
| Drawing export | VERIFIED | PDF, DWG, DXF |
| Programmatic analysis/layout helpers | EXPERIMENTAL | retained legacy helper commands |
| Autonomous engineering judgement | OUTSIDE RUNTIME | external LLM/user responsibility |

## Late-stage proof points

### v0.65 / v0.66

Existing Part Eyes were reused against nested assembly occurrences without activating/saving referenced parts.

### v0.67

Conceptual manufacturing groups could be isolated in a DrawingView using repeated atomic visibility calls, with source assembly state preserved.

### v0.68

CustomTable workflow:

```text
create table
→ write one cell
→ read back
→ repair one cell
→ set one column width
→ layout map
→ preview
```

## Exact command inventory

See [docs/COMMAND_REFERENCE.md](docs/COMMAND_REFERENCE.md).

## Boundary

The Runtime can create and modify substantial drawing content. The benchmark stop criterion was reached in engineering/visual judgement, not in Autodesk Inventor API access.
