# About Contollo RDEL Zip Runner

## Product

**Contollo RDEL Zip Runner** is the current Visual Studio 2022 extension and package runner for **RDEL**.

RDEL means:

```text
Repository Delta Evolution Loop
```

## Current Product Version

```text
0.3.0-preview
```

## Current Phase

```text
Phase 1.3 / AI Context Manager Foundation
```

## What RDEL Is

RDEL is a portable AI-assisted change workflow.

It is designed to package, apply, validate, audit, rollback, and explain repository changes created by humans, AI systems, or future Neuro Commander Studio tooling.

## Strategic Role

RDEL is the package/change engine for Neuro Commander Studio.

It is not the full Neuro Commander Studio product yet. It is the lower-level change engine and context bridge.

## Current Baseline

The current baseline includes:

- ZIP package runner
- safe extraction
- manifest support
- package SHA tracking
- full-file apply
- backup/rollback
- dry run
- Git checkpointing
- run history
- command validation
- Context Assembly Engine
- RDEL Session Protocol
- AI Session settings UI
- output window capture
- context package export
- schema/preflight tooling
- DocOps append layer
- Engineering Intelligence
- cumulative history recovery
- package history index

## Current Limitations

- no patch/hunk mode
- no delete/rename operation support
- no package signatures
- no dependency enforcement
- no compatibility refusal yet
- no package inspection UI yet
- no diff preview yet
- validation profiles are documented but not fully enforced
- this VSIX project should not default to plain `dotnet build`

## Important Rule

Cumulative docs must be updated through DocOps:

```text
.rdel-docops/context/
.rdel-docops/memory/
.rdel-docops/decisions/
```

Do not directly overwrite:

```text
docs/context.md
docs/memory.md
docs/DECISIONS.md
```
