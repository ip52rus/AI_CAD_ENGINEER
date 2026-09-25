# Contributing

AI CAD ENGINEER is an experimental Autodesk Inventor automation runtime.

Useful contributions strengthen factual CAD access, deterministic atomic actions, safety, diagnostics or supervised workflows.

## Before code

Run a Capability Audit:

1. identify the blocked workflow;
2. search dispatcher/repository;
3. confirm no equivalent command exists;
4. inspect Inventor API;
5. decide whether code is actually needed.

## Rules

- Runtime contains Eyes and atomic Hands.
- Engineering judgement stays outside Runtime.
- One command performs one explicit read or action.
- Do not hide fallbacks that change engineering meaning.
- Do not infer absent design intent.
- Prefer reusable generic commands over product-specific workflows.

## Build

```powershell
MSBuild.exe AI_CAD_ENGINEER.csproj /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
```

## Verification

Call a capability VERIFIED only after:

- build PASS;
- unique command registration;
- real Inventor execution PASS;
- direct readback;
- source/reference integrity check where relevant.

## Pull requests

Describe:

- blocker;
- why existing capability is insufficient;
- Inventor API path;
- changed files;
- JSON test;
- E2E result;
- source-model integrity;
- limitations.

## Test data

Do not commit proprietary CAD files, confidential drawings, credentials or API keys.

## AI agents

Follow [AGENTS.md](AGENTS.md).

## License

No explicit repository license has been selected yet.
