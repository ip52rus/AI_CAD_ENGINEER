# AGENTS.md — AI CAD ENGINEER

## 1. Project purpose

AI CAD ENGINEER is a local bridge between an AI agent and Autodesk Inventor.

The application provides:

- **Eyes**: atomic read-only commands that expose Inventor data.
- **Hands**: atomic write commands that perform one explicit Inventor action.

The external AI agent performs engineering reasoning, chooses actions, analyzes geometry, and decides what to do next. The C# runtime must not replace the AI with embedded engineering decision logic.

## 2. Non-negotiable architecture rule

Before creating or modifying code, ask:

```text
Does this code give the AI an Eye or an atomic Hand?
```

Proceed only when the answer is yes.

Do not implement in C#:

- automatic engineering decisions;
- automatic selection of drawing views, dimensions, tables, notes, or formats;
- automatic layout policies presented as final engineering decisions;
- hidden planning systems that decide instead of the AI;
- EngineeringBrain, AIDecision, Planning, DrawingManager, or similar runtime decision systems;
- duplicate high-level "create the whole drawing" brains.

Programmatic automation may be added only after explicit user approval and only where an AI cannot reliably perform the task through atomic tools.

## 3. Current runtime architecture

The active runtime path is:

```text
LLM
→ JSON
→ Program.cs
→ Core/Application.cs
→ InventorControl/InventorCommandDispatcher.cs
→ IInventorCommand implementations
→ CommandSupport / ReadSupport
→ Autodesk Inventor API
```

Do not recreate or restore the removed legacy path:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
```

## 4. Mandatory pre-change audit

Before adding any command:

1. Read `InventorControl/InventorCommandDispatcher.cs`.
2. Search the entire repository for the proposed JSON command name.
3. Search for equivalent command classes and support methods.
4. Read current project status documents, when present:
   - `CURRENT_STATE.md`
   - `CAPABILITY_MAP.md`
   - `PROJECT_REVIEW.md`
   - `AI_CAPABILITIES.md`
   - `COMMANDS.md`
   - `IMPLEMENTATION_STATUS.md`
   - `ROADMAP.md`
   - `ARCHITECTURE.md`
5. Classify the requested capability:
   - already implemented and tested;
   - already implemented but untested;
   - partially implemented;
   - genuinely missing;
   - experimental / non-runtime if it contains analysis or automatic decisions.
6. Report this classification before writing code.

Creating duplicate commands is forbidden.

## 5. Command design rules

Every JSON command must perform exactly one of these:

- one atomic read operation;
- one atomic write operation.

Command requirements:

- stable snake_case JSON command name;
- one command class implementing `IInventorCommand`;
- explicit input validation;
- structured JSON success response;
- structured JSON error response;
- useful diagnostics without hiding Inventor API errors;
- no silent fallback that changes engineering intent;
- preserve raw enum values when useful;
- also expose readable enum names where possible.

## 6. Autodesk Inventor constraints

Target environment:

```text
Windows 10
Visual Studio Community 2026
.NET 10
Autodesk Inventor Professional 2027
Autodesk Inventor COM reference
```

Important:

- Do not validate this project with `dotnet build`.
- `dotnet build` fails on `ResolveComReference` with MSB4803.
- Build through Visual Studio or classic .NET Framework MSBuild.
- Inventor COM signatures may expose interfaces such as `Inventor._Document`.
- Match the actual installed Inventor 2027 interop signatures.
- Prefer compile-safe explicit COM casts when required.
- Do not guess API members; inspect existing working code, local interop metadata, and compiler errors.

## 7. Build and verification workflow

Before changes:

```text
git status
```

After code changes:

1. Show changed files.
2. Show `git diff`.
3. Build with classic Visual Studio MSBuild:

   ```text
   MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
   ```

4. Do not use `dotnet build`.
5. Provide exact JSON test commands.
6. Wait for real Inventor responses.
7. Only after successful tests update status documentation.
8. Commit only after explicit user permission.

Do not run `git push`, destructive Git commands, file deletion, broad refactors, `reset`, `clean`, `checkout`, or `stash` without explicit approval.

## 8. File editing rules

- Preserve the existing namespace and formatting style.
- Prefer full, complete files over partial fragments when replacing a file.
- Reuse existing support classes when appropriate.
- Do not introduce a second support layer that duplicates an existing one.
- Do not rename public JSON commands without explicit approval.
- Do not remove working commands merely because they look imperfect.
- Make the smallest coherent change.
- One package/milestone should solve one clearly defined capability.

## 9. Documentation rules

When a command is added and verified, update the applicable status documentation.

Do not claim a command is verified until it has:

1. Build PASS.
2. Real Inventor PASS.
3. User-confirmed behavior.

Use these status labels:

```text
VERIFIED
IMPLEMENTED_UNTESTED
PARTIAL
EXPERIMENTAL
DEPRECATED
MISSING
OUT_OF_SCOPE
```

Do not describe a GOST `CustomTable` as a `PartsList`. Inventor exposes these through different API collections.

## 10. Legacy experimental commands

The following commands exist but contain programmatic analysis/automation beyond pure Eyes or atomic Hands:

```text
analyze_dimension_layout
auto_arrange_dimensions
check_annotation_collisions
auto_resolve_annotation_collisions
analyze_view_dimension_candidates
```

Rules:

- keep them for compatibility;
- mark them experimental;
- do not expand them without explicit user approval;
- do not use them as architectural examples for new work.

## 11. Current milestone

Current milestone addendum after Package 19A:

```text
Package 19A complete — Feature Control Frames Eye checkpoint
```

Current confirmed drawing symbol command:

```text
get_feature_control_frames
```

Current Package 19A status:

- `get_feature_control_frames` is VERIFIED;
- feature control frame reading covers `Sheet.FeatureControlFrames`, frame metadata, rows, tolerance fields, datum fields, reference keys, and diagnostics;
- the runtime still does not perform tolerance interpretation, GOST validation, GD&T semantic analysis, or engineering interpretation.

Next correct action:

1. run a Capability Check for the next drawing symbol layer;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 18A and Package 17A notes below are superseded by this addendum where they conflict.

Current milestone addendum after Package 18A:

```text
Package 18A complete — Drawing Text Objects Eye checkpoint
```

Current confirmed drawing text command:

```text
get_drawing_text_objects
```

Current Package 18A status:

- `get_drawing_text_objects` is VERIFIED;
- drawing text object reading covers DrawingNotes collections, DrawingSketch TextBoxes, and SketchedSymbols;
- the runtime still does not perform semantic text analysis, TT/TU recognition, GOST interpretation, or engineering interpretation.

Next correct action:

1. run a Capability Check for the next engineering-layer boundary;
2. verify whether sufficient atomic Eyes/Hands already exist;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye or Hand is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

The older Package 17A notes below are superseded by this addendum where they conflict.

Current milestone:

```text
Package 17A complete — typed CustomTables Eye checkpoint
```

Current confirmed drawing table commands:

```text
get_parts_lists
get_revision_tables
get_drawing_table_collections
get_custom_tables
```

Current status:

- `get_parts_lists` is VERIFIED for reading Inventor API `Sheet.PartsLists`;
- `get_revision_tables` is VERIFIED;
- `get_drawing_table_collections` is VERIFIED;
- `get_custom_tables` is VERIFIED;
- the tested GOST table is exposed by Inventor as `Sheet.CustomTables` / `kCustomTableObject`, not as `Sheet.PartsLists`;
- detailed reading of `Sheet.CustomTables` metadata, columns, rows, cells, merged cells, and reference keys is implemented and verified;
- the runtime still does not classify tables as specification, BOM, or GOST.

Next correct action:

1. run a Capability Audit for Drawing Text / Notes Eye;
2. verify whether sufficient drawing text and note reading already exists;
3. inspect actual Inventor 2027 interop signatures before proposing new code;
4. decide whether a new atomic Eye is needed;
5. do not write implementation code until the audit proves a gap and the user authorizes implementation.

## 12. Required response style for development tasks

For every requested feature, respond in this order:

```text
AUDIT
- what already exists;
- what is missing;
- whether the change fits Eyes and Hands.

PLAN
- exact files to create;
- exact files to modify;
- why each is required.

IMPLEMENTATION
- make the smallest coherent change.

VERIFICATION
- changed files;
- git diff summary;
- exact MSBuild step;
- exact JSON test command;
- no commit until approved.
```

## 13. Safety stop conditions

Stop and ask before proceeding when:

- the requested capability may duplicate existing functionality;
- the change would implement engineering reasoning inside C#;
- the required Inventor API signature is uncertain;
- the change requires deleting or broadly restructuring files;
- current tracked changes are unrelated to the requested task;
- build errors reveal that the plan was based on the wrong API surface;
- the user asks for a full automation brain rather than Eyes or atomic Hands.

## 14. Definition of done

A capability is complete only when all applicable items are true:

1. no duplicate command exists;
2. implementation fits the Eyes/Hands architecture;
3. the project builds with the required MSBuild workflow;
4. F5 launches the current executable when UI launch is required;
5. the JSON command succeeds in real Inventor;
6. returned data/action is visually or structurally confirmed;
7. documentation is updated;
8. the user explicitly approves the commit.
