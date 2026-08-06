# CODEX_WORKFLOW.md

## Starting a session

From the repository root:

```cmd
codex
```

First prompt:

```text
Read AGENTS.md and CURRENT_STATE.md.
Follow them as binding repository instructions.
Inspect the live repository before proposing changes.
```

## Standard feature workflow

```text
1. Audit repository.
2. Confirm no duplicate exists.
3. Classify as eye, atomic hand, or forbidden embedded reasoning.
4. Propose minimal files.
5. Wait for approval.
6. Edit.
7. Show git diff.
8. Rebuild in Visual Studio.
9. Run exact JSON test in Inventor.
10. Update documentation after verification.
11. Commit only after explicit approval.
```

## Returning to a session

Use:

```cmd
codex resume
```

Repository state and `AGENTS.md` remain the source of truth. Do not rely solely on remembered conversation context.

## Model and reasoning

For repository audits, COM interop problems, architecture changes, and cross-file implementation work, use a stronger reasoning setting than `low`.

Inside Codex:

```text
/model
```

Choose an available GPT-5.6 coding model with medium or high reasoning when the task is nontrivial.

## Permissions

Use:

```text
/permissions
```

Recommended initial policy:

- read the repository freely;
- allow edits only inside the repository;
- ask before network access;
- ask before commands outside the workspace;
- ask before destructive commands;
- never allow automatic `git push`;
- commit only after explicit instruction.
