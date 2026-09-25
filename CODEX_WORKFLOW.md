# AI Agent Workflow

Read first:

1. `AGENTS.md`
2. `CURRENT_STATE.md`
3. `CAPABILITY_MAP.md`

Then inspect the live repository.

## Standard workflow

```text
1. Audit repository.
2. Confirm no equivalent command exists.
3. Classify Eye / atomic Hand / external reasoning.
4. Decide whether code is required.
5. Propose minimal change.
6. Edit.
7. Show diff.
8. Build with classic Visual Studio MSBuild.
9. Run exact JSON test against real Inventor.
10. Verify direct readback and source-model integrity.
11. Update documentation.
12. Commit/push only with explicit authorization.
```

Preferred automation entrypoint:

```powershell
AI_CAD_ENGINEER.exe --json-file ".\command.json"
```

Never move engineering judgement into C# merely because an external LLM workflow is difficult.
