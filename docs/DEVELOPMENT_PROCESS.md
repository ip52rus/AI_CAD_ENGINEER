# Development Process

## Capability Audit

Every package starts by asking whether new code is needed at all.

- identify the real blocked workflow;
- inspect registry and existing support;
- inspect Inventor API;
- distinguish missing capability from missing design intent or reasoning.

Allowed/preferred result:

```text
CODE_REQUIRED = NO
```

## Minimal package

If code is required:

- add the minimum generic Eye/Hand;
- reuse existing support;
- preserve contracts;
- avoid product-specific semantics.

## Build

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
```

## Registry

Final v0.68 inventory: 219 registered / 219 unique.

## Live Inventor E2E

Compilation is insufficient.

Verify:

- real command input;
- real Inventor response;
- direct readback;
- error cases;
- document identity;
- dirty state;
- modal behaviour where relevant.

## Drawing QA

```text
facts
→ create
→ layout map
→ preview
→ external visual critic
→ explicit correction
→ preview
```

## Checkpoint

After E2E PASS:

- diff audit;
- final build;
- commit;
- tag for meaningful milestones.

## Stop rules

Stop coding when the remaining blocker is no longer API access but design intent, visual composition or engineering judgement.
