# CURRENT_STATE.md — AI CAD ENGINEER

## Repository

Expected local path:

```text
C:\AI_CAD_ENGINEER\AI_CAD_ENGINEER
```

Current working branch:

```text
cleanup/legacy-architecture
```

Current checkpoint before the Package 16D documentation sync:

```text
a07b8fe docs: add Capability Map after Package 15A
```

## Runtime architecture

The active runtime path is:

```text
LLM
→ JSON
→ Program.cs
→ Core/Application.cs
→ InventorControl/InventorCommandDispatcher.cs
→ IInventorCommand
→ CommandSupport / ReadSupport
→ Autodesk Inventor API
```

The C# runtime is only Eyes and atomic Hands. Engineering decisions, planning, automatic layout, optimization, and best-choice logic remain outside the runtime and are performed by the external LLM.

Do not restore the removed legacy architecture:

```text
CommandProcessor
→ DrawingManager
→ EngineeringBrain
→ Analysis / Decision / Planning
```

## Dispatcher and command inventory

Live command audit after Packages 13A, 15A, 16A, and 16C:

```text
112 registered JSON commands
112 unique registered JSON commands
0 duplicate registered command names
0 duplicate command Name properties
0 unregistered command classes
```

Always recalculate from the live repository before relying on these numbers.

## Confirmed baseline commands

Confirmed post-cleanup commands:

```json
{"command":"ping"}
```

```json
{"command":"get_active_document"}
```

```json
{"command":"get_drawing_sheets"}
```

## Package 13A status

Package 13A added atomic base drawing view creation:

```text
InventorControl/Commands/DrawingViews/BaseViewCommandSupport.cs
InventorControl/Commands/DrawingViews/CreateBaseViewCommand.cs
```

Command:

```json
{"command":"create_base_view"}
```

Status:

- implemented and registered;
- uses explicit `Inventor._Document` cast required by `DrawingViews.AddBaseView`;
- included in the latest successful MSBuild checkpoint;
- Inventor PASS is not recorded in this document.

## Package 15A status

Package 15A added basic Drawing Annotation Eyes:

```json
{"command":"get_general_notes"}
{"command":"get_leader_notes"}
{"command":"get_balloons"}
{"command":"get_center_marks"}
{"command":"get_centerlines"}
```

Status:

- implemented and registered;
- included in the latest successful MSBuild checkpoint;
- Inventor PASS is not recorded in this document.

## Package 16A status

Package 16A added typed Drawing Table Eyes:

```json
{"command":"get_parts_lists"}
{"command":"get_revision_tables"}
```

Status:

- `get_revision_tables`: VERIFIED.
- `get_parts_lists`: VERIFIED for reading Autodesk Inventor API objects exposed through `Sheet.PartsLists`.

Confirmed Inventor results:

- `get_revision_tables` found one `RevisionTable` titled `ЖУРНАЛ ИЗМЕНЕНИЙ`;
- revision table metadata, columns, rows, and cells were read;
- no `sheetName` uses `DrawingDocument.ActiveSheet`;
- explicit `sheetName` selects the requested sheet;
- invalid `sheetName` returns a structured error;
- on the tested drawing sheet, `Sheet.PartsLists.Count = 0`;
- `get_parts_lists` returned `count = 0`;
- `get_drawing_annotation_summary.annotations.partsListCount = 0`;
- legacy `get_drawing_tables.tables.partsListCount = 0`;
- document save and `Document.Update` did not change the result;
- diagnostics were empty;
- no defect in the `get_parts_lists` implementation was proven.

Important distinction:

The visible GOST table named like a parts list in the tested document is not an Inventor `Sheet.PartsLists` object.

## Package 16C status

Package 16C added Drawing Table Collections Diagnostics:

```json
{"command":"get_drawing_table_collections"}
```

Status: VERIFIED.

Confirmed on `Sborka1.idw`, `Лист:1`:

- `CustomTables.rawCount = 1`, `CustomTables.itemCount = 1`;
- `HoleTables.rawCount = 0`, `HoleTables.itemCount = 0`;
- `PartsLists.rawCount = 0`, `PartsLists.itemCount = 0`;
- `RevisionTables.rawCount = 1`, `RevisionTables.itemCount = 1`;
- `rawCount` matched `itemCount`;
- diagnostics were empty;
- call without `sheetName` returned `usedActiveSheet = true`;
- call with `sheetName` returned `usedActiveSheet = false`.

Confirmed cause of the visual mismatch:

The visible table `Спецификация` / `Список деталей` in the tested GOST drawing is represented by Inventor API as:

```text
collection: Sheet.CustomTables
objectType: kCustomTableObject
title: "Список деталей по ГОСТ: Sborka1.iam"
rowCount: 15
columnCount: 7
```

It is not a `Sheet.PartsLists` object.

## Verified eyes

Major verified read areas include:

- active document;
- sheets and drawing views;
- drawing-view relationships;
- annotation summary;
- `Sheet.PartsLists` typed Eye;
- `Sheet.RevisionTables` typed Eye;
- drawing table collection diagnostics;
- view and curve geometry;
- model feature tree;
- feature details;
- parameters;
- surface bodies;
- faces and edges;
- sketches, sketch geometry, constraints, and dimensions;
- work features;
- assembly summary;
- assembly occurrences;
- assembly constraints;
- assembly BOM;
- assembly referenced documents.

## Known gaps

- Detailed row/column/cell reading for `Sheet.CustomTables` is not implemented yet.
- Typed detailed Eye for `Sheet.HoleTables` is not implemented yet.
- Do not describe GOST `CustomTable` objects as `PartsList` objects.

## Next task

Next capability audit:

```text
Typed CustomTables Eye
```

Goal:

- check whether sufficient `Sheet.CustomTables` reading already exists;
- inspect actual Inventor 2027 API for custom table columns, rows, and cells;
- decide whether a new `get_custom_tables` command is needed;
- do not write code until the audit confirms a real gap.
