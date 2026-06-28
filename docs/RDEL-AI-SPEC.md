# RDEL AI Communication Specification

## Status

Draft v1.2-preview.

## Purpose

This document defines how AI systems should understand, generate, review, and consume RDEL packages.

RDEL is a protocol for portable, verified, AI-assisted software change.

## Audience

This document is for ChatGPT, Claude, Gemini, Copilot, local LLMs, future Neuro Commander Studio agents, and human developers reviewing AI packages.

## Core Rule

AI must account for code, not merely suggest code.

## Required Reading Order for AI

An AI working on this repository should read:

1. `docs/context.md`
2. `docs/RDEL-AI-OPERATOR-GUIDE.md`
3. `docs/RDEL-PACKAGE-AUTHORING-GUIDE.md`
4. `docs/ai-instructions.md`
5. `docs/DECISIONS.md`

## Package Layout

Current-format RDEL package:

```text
package-name/
  contollo-rdel.json
  README.md
  docs/context.md
  target files...
```

## Required Manifest Fields

```json
{
  "Name": "Package Name",
  "Description": "Package description",
  "Target": "solution",
  "Commands": [
    "git status --porcelain"
  ]
}
```

## Current Runner Warning

The current runner still executes `Commands`.

`ValidationProfile` is currently a documented forward-compatible field.

Therefore, commands must be correct even if `ValidationProfile` is present.

## Metadata Skip Rule

Current runner skips root package metadata:

- `contollo-rdel.json`
- `README.md`
- `context.md`
- `manifest.json`

To update repository context today, use:

```text
docs/context.md
```

## AI Grounding Protocol

Every AI-generated package must satisfy:

1. Every changed file has an exact path.
2. Every changed file has a status.
3. Every referenced symbol exists or is created.
4. Every assumption is declared.
5. Every dependency is explicit.
6. Every validation method matches the project type.
7. Every architecture-impacting change updates context.
8. Every package can be reproduced from its contents.

## Non-Goals in Current Runner

Current runner does not yet support delete operations, patch/hunk approval, package signatures, dependency enforcement, metadata export, diff previews, or enterprise policy.
