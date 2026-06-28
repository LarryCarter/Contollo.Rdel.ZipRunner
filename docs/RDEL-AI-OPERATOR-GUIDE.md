# RDEL AI Operator Guide

## Purpose

This document tells an AI model exactly how to operate inside the RDEL workflow.

It is written for ChatGPT, Claude, Gemini, Copilot, local models, or any future AI that is asked to help with this repository.

The AI should read this document before generating code or an RDEL package.

## The Most Important Rule

Do not merely suggest code.

Account for code.

Every change must have:

- a file path
- a file status
- a reason
- a relationship to existing files
- a validation plan
- a rollback expectation
- a context/documentation update when architecture changes

## What RDEL Is

RDEL means Repository Delta Evolution Loop.

RDEL is a package-based workflow for AI-assisted software changes.

The Visual Studio extension applies RDEL ZIP packages into a repository through a controlled process.

## Current Runner Workflow

The runner:

1. Lets the developer select the ZIP package.
2. Extracts it safely.
3. Reads `contollo-rdel.json`.
4. Computes the package SHA-256.
5. Previews package metadata in the Output pane.
6. Applies non-blocked files.
7. Backs up overwritten files.
8. Runs validation commands.
9. Writes run history.
10. Creates Git checkpoints and commits.
11. Supports rollback of the last run.

## What AI Should Produce

The AI must produce a complete ZIP package, not loose suggestions.

Required package root files:

```text
contollo-rdel.json
README.md
```

Recommended files when context changes:

```text
docs/context.md
docs/DECISIONS.md
docs/memory.md
docs/ai-instructions.md
docs/RDEL-AI-SPEC.md
```

## Current Metadata Skip Rule

The current runner skips package metadata files at package root:

```text
contollo-rdel.json
contollo-rdel.txt
README.md
context.md
manifest.json
```

Therefore, package root `README.md` explains the package and is not applied to the repo.

Repository context should currently be placed at:

```text
docs/context.md
```

Future MetadataExport support may allow intentional publishing of root metadata.

## File Status Rules

When describing the package, the AI should list each file as:

```text
New
Updated
Deleted
No Change
```

Current runner supports new and updated files.

Current runner does not support delete operations.

## Do Not Include Blocked or Dangerous Paths

Never include:

```text
.git/
.vs/
.contollo/
bin/
obj/
.env
secrets.json
appsettings.Production.json
*.pfx
*.pem
*.key
*.cer
*.crt
*.snk
```

Do not include secrets, credentials, production logs, customer data, tokens, certificates, or private keys.

## Validation Rules

The manifest must define validation through `Commands`.

The current runner still executes `Commands` directly.

`ValidationProfile` is documented but not yet first-class behavior.

For documentation-only packages:

```json
{
  "ValidationProfile": "GitOnly",
  "Commands": [
    "git status --porcelain"
  ]
}
```

For normal SDK-style .NET projects:

```json
{
  "ValidationProfile": "DotNetCli",
  "Commands": [
    "dotnet restore",
    "dotnet build --no-restore",
    "dotnet test --no-build"
  ]
}
```

For this VSIX repository, do not use `dotnet build` unless explicitly requested.

## How AI Should Explain an RDEL Package

Before providing the ZIP, the AI should summarize:

```text
Package name:
Purpose:
Validation:
Files:
- New:
- Updated:
- Not changed:
Known risks:
Rollback:
```

## AI Grounding Checklist

Before finishing, the AI must verify:

- Are all referenced files included or already known to exist?
- Are all new files placed at correct paths?
- Are package metadata files not mistakenly expected to apply?
- Is validation appropriate for the repo type?
- Are docs/context updated when architecture changes?
- Are blocked paths avoided?
- Is the package deterministic?
- Is the package README clear enough for a human?
- Is `docs/context.md` updated enough for the next AI?

## Failure Handling

If the AI cannot know something, it must say so.

Examples:

```text
I cannot verify the current file exists from the information provided.
I am assuming the repo root is ...
I am using GitOnly validation because this is a docs-only package.
I am not deleting files because the current runner does not support delete operations.
```
