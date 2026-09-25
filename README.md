# AI CAD ENGINEER

**Experimental Autodesk Inventor automation runtime for AI-assisted CAD workflows.**

AI CAD ENGINEER is a C#/.NET project that exposes Autodesk Inventor through a structured JSON command layer. The main technical result is a working external control surface for reading Inventor state and performing deterministic CAD actions from an AI agent or another automation client.

The project also investigated a harder question: whether an LLM could use that control layer to autonomously produce production-quality engineering drawings. The automation layer proved viable; the fully autonomous drawing-engineer hypothesis did not generalize reliably across different classes of parts and assemblies.

## Status

**Final research checkpoint: v0.68**

- **219** registered JSON commands
- **219** unique command names
- live Autodesk Inventor 2027 E2E validation throughout development
- architecture: external reasoning + atomic **Eyes** and **Hands**
- source-model integrity checks built into the development process
- autonomous-drafting research track concluded after real benchmarks

The project was not stopped because Inventor could not be controlled. The opposite was demonstrated: the runtime can inspect and manipulate a broad set of Inventor objects. The limiting factor was the consistency of higher-level AI judgement for view selection, dimension completeness, drawing composition and visual drafting quality.

## Architecture

```text
External LLM / automation client
            │ JSON
            ▼
        Program.cs
            ▼
    Core/Application.cs
            ▼
InventorCommandDispatcher
            ▼
     IInventorCommand
            ▼
 CommandSupport / ReadSupport
            ▼
    Autodesk Inventor API
```

### Eyes

Read-only factual operations for documents, sheets, views, dimensions, title blocks, notes, symbols, tables, model features, parameters, sketches, BRep, assemblies, previews and layout data.

### Hands

One explicit Inventor action per command: create/move/delete views, create/edit dimensions, notes and symbols, edit title-block fields, tables, per-view occurrence visibility, save/export and other deterministic operations.

The external caller decides **what should be done**. Runtime decides only **how to perform the requested Inventor API operation safely**.

## Why the architecture changed

Versions before v0.15 contained an internal `EngineeringBrain`, `DrawingManager`, view scoring and dimension-decision logic.

That path was intentionally removed at v0.15. Decisions such as “best view”, “required dimension” and “correct sheet layout” depend heavily on design intent, manufacturing context and visual judgement. The project therefore moved reasoning outside Runtime.

The tag `v0.15-before-cleanup` preserves the earlier architecture.

## Implemented capability areas

At v0.68 the dispatcher contains 219 unique JSON commands covering:

- Inventor connectivity and document lifecycle
- drawing sheets, borders and GOST title blocks
- base/projected/section/detail/auxiliary views and view breaks
- drawing curves and model references
- linear, diameter, radius, angular, ordinate, baseline and chain dimensions
- dimension formatting, styles, layers and tolerance modes
- center marks and centerlines
- hole/thread, general and leader notes
- feature control frames
- surface texture and welding symbols
- sketched symbols
- revision clouds and revision tables
- edge, transition, bend, chamfer and punch annotations
- balloons and parts lists
- CustomTables and HoleTables
- PDF/DWG/DXF export
- drawing-sheet and model PNG previews
- normalized drawing layout map
- model feature/parameter/sketch/BRep Eyes
- assembly occurrence/BOM/reference Eyes
- referenced-part targeting from assembly context
- DrawingView-local occurrence visibility
- atomic CustomTable cell and column-width editing

Exact dispatcher inventory: [docs/COMMAND_REFERENCE.md](docs/COMMAND_REFERENCE.md)

Verification map: [CAPABILITY_MAP.md](CAPABILITY_MAP.md)

## Example commands

```json
{"command":"get_active_document"}
```

```json
{"command":"get_assembly_occurrences"}
```

```json
{
  "command":"set_drawing_view_occurrence_visibility",
  "sheetName":"Лист:1",
  "viewName":"ВИД1",
  "occurrencePath":"Frame:1/Profile:3",
  "visible":false
}
```

```json
{
  "command":"set_custom_table_cell_value",
  "sheetName":"Лист:1",
  "customTableIndex":1,
  "row":1,
  "column":1,
  "value":"D01"
}
```

## Development stack

### Products and tools

The project was developed and tested with:

- **Autodesk Inventor Professional 2027** — target CAD system and live E2E environment
- **Autodesk Inventor API / COM Automation** — programmatic integration layer
- **Visual Studio Community 2026** — primary C# development environment
- **.NET SDK 10.0.302** — application runtime/toolchain
- **MSBuild 18.6.11** — validated build system
- **Windows 10 (10.0.19045)** — development and Inventor host OS
- **Git / GitHub** — version control, milestones and public project history
- **ChatGPT / Codex / Codex CLI** — AI-assisted architecture, research, code iteration and Inventor workflow experiments

The AI tools were used during development and research; they are not required as an embedded runtime dependency. The stable integration boundary of the application is the JSON command interface.

### Languages and formats

- **C#** — primary implementation language
- **JSON** — command/response protocol between the external agent and Runtime
- **PowerShell** — build, test and automation scripts used during development
- **Markdown** — architecture, research and project documentation

### Main technologies

- **.NET 10**
- **Autodesk Inventor COM interop**
- **Autodesk Inventor object model / API**
- **Windows COM / Running Object Table**
- **OpenAI Responses API** — experimental client adapter present in the repository, not required by the active Runtime architecture

## Build and run

Tested environment:

- Windows 10
- Autodesk Inventor Professional 2027
- .NET 10
- Visual Studio / classic MSBuild
- Autodesk Inventor COM reference

Validated build path:

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
```

Interactive mode:

```powershell
AI_CAD_ENGINEER.exe
```

Single-command automation:

```powershell
AI_CAD_ENGINEER.exe --json-file ".\command.json"
```

or:

```powershell
AI_CAD_ENGINEER.exe --json "{\"command\":\"ping\"}"
```

Single-command mode is attach-only: it connects to an already running Inventor instance and emits one JSON response with a deterministic exit code.

## Development method

```text
Capability Audit
      ↓
Is new code actually required?
      ↓
minimal generic Eye / Hand
      ↓
classic MSBuild
      ↓
live Inventor E2E
      ↓
direct readback
      ↓
source-model integrity check
      ↓
checkpoint / tag
```

If existing capabilities were sufficient, no new code was added.

See [docs/DEVELOPMENT_PROCESS.md](docs/DEVELOPMENT_PROCESS.md).

## Research benchmarks

### Benchmark #1 — turned shaft

Reference-aided planning combined real drawings of the same part class, a factual model dossier, manufacturing requirements, candidate plans, execution and visual QA. The content architecture improved substantially, but good final composition still depended on human correction.

### Benchmark #2 — welded chair frame

A real profile-tube assembly tested generalization to a multi-level welded/bolted product. The runtime recovered:

- 26 structural metal occurrences
- 24 tube members + 2 plates
- 14 manufacturing-equivalent detail types
- 12 × Ø9 through-profile holes for M8 bolts
- 12 × Ø11.1 one-wall holes for M8 threaded rivet nuts
- 5°, 10°, 45° and square end conditions
- differences between Frame Generator `B_L` and final BRep geometry
- four manufacturing units: left side, right side, seat and backrest

A multi-sheet documentation architecture was planned and technically executable. The first real rendered set still required substantial correction in view choice, dimension completeness and layout. That result triggered the stop criterion for the original autonomous-drafting goal.

Full findings: [docs/RESEARCH_FINDINGS.md](docs/RESEARCH_FINDINGS.md)

## What the project demonstrated

The project **did** demonstrate that an external agent can be given a substantial, structured and testable control layer over Autodesk Inventor.

It **did not** demonstrate that a current LLM can reliably replace an experienced drafter/constructor for arbitrary production drawings without significant review.

The runtime remains useful as a foundation for:

- CAD copilots
- model interrogation
- drawing/model review
- fabrication-data extraction
- batch Inventor automation
- supervised drawing assistance
- agent-controlled repetitive CAD workflows

## Repository map

```text
AI/
Core/
InventorControl/
  Commands/
  InventorCommandDispatcher.cs

AGENTS.md
ARCHITECTURE.md
CAPABILITY_MAP.md
CURRENT_STATE.md
ESKD_DRAWING_POLICY.md
PROJECT_REVIEW.md
ROADMAP.md
CHANGELOG.md
CONTRIBUTING.md

docs/
  COMMAND_REFERENCE.md
  DEVELOPMENT_PROCESS.md
  PROJECT_HISTORY.md
  RESEARCH_FINDINGS.md
  MILESTONES.md
```

## AI-agent rules

[AGENTS.md](AGENTS.md) is the binding guide for coding/CAD agents.

Key rules:

- Runtime = factual Eyes + atomic Hands
- audit existing capability before adding code
- do not invent missing design intent
- final BRep is the primary source of final geometric truth
- never mutate source-model state as a drawing fallback
- live Inventor E2E is required before a capability is called verified
- API success is not proof of drawing quality

## ESKD policy

[ESKD_DRAWING_POLICY.md](ESKD_DRAWING_POLICY.md) contains the external reasoning policy used during the drawing experiments. It is deliberately not embedded as automatic Runtime behaviour.

## Version history

- v0.1–v0.12 — embedded analysis / Engineering Brain experiments
- `v0.15-before-cleanup` — checkpoint before architectural cleanup
- v0.15 — legacy decision layer removed
- v0.16–v0.64 — systematic Eyes/Hands expansion
- v0.65–v0.66 — referenced-part targeting from assemblies
- v0.67 — DrawingView occurrence visibility
- v0.68 — atomic CustomTable editing

See [CHANGELOG.md](CHANGELOG.md), [docs/PROJECT_HISTORY.md](docs/PROJECT_HISTORY.md) and [docs/MILESTONES.md](docs/MILESTONES.md).

## Limitations

- Autodesk Inventor is required; this is not a standalone CAD kernel.
- Windows/COM is part of the architecture.
- Some commands are verified only for specific Inventor object/context combinations.
- A small number of legacy analysis commands remain for compatibility.
- Engineering intent such as tolerances, exact weld requirements, fastener specification and official designations cannot safely be inferred from geometry alone.
- Drawing quality still requires engineering and visual review.

## License

No explicit open-source license has been selected yet. Public visibility alone does not define reuse rights.
