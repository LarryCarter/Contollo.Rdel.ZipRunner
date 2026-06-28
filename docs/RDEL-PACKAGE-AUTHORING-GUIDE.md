# RDEL Package Authoring Guide

## Purpose

This guide defines how to build a current-format RDEL package.

An AI model should be able to follow it to produce a package that the current Visual Studio RDEL runner can apply.

## Current Package Shape

An RDEL package is a ZIP file.

Inside the ZIP, there should usually be a single root folder:

```text
my-rdel-package/
  contollo-rdel.json
  README.md
  docs/context.md
  target files...
```

The runner detects the single root folder and uses it as the payload root.

## Required Manifest

File:

```text
contollo-rdel.json
```

Minimum current-format manifest:

```json
{
  "Name": "Package Name",
  "Description": "What this package does.",
  "Target": "solution",
  "Commands": [
    "git status --porcelain"
  ]
}
```

Recommended manifest:

```json
{
  "Name": "RDEL Example Package",
  "Description": "Adds an example documentation file.",
  "Target": "solution",
  "ValidationProfile": "GitOnly",
  "Commands": [
    "git status --porcelain"
  ],
  "SchemaVersion": "1.1-current-compatible",
  "ManifestVersion": "1.2-preview",
  "PackageVersion": "1.2.1-preview",
  "PackageType": "DocumentationAndContext",
  "ChangeIntent": "Explain why this package exists.",
  "TrustLevel": "ExternalGenerated",
  "SourceProvider": "Claude",
  "HumanReadmePath": "README.md",
  "AiContextPath": "docs/context.md"
}
```

## Target

Current supported values:

```text
solution
selected-project
```

Use `solution` when paths are relative to repository or solution root.

Use `selected-project` only when the developer intentionally wants files applied relative to the selected project root.

## Commands

Commands are run after apply.

The current runner does not yet implement `ValidationProfile` behavior directly, so commands are authoritative.

For docs-only:

```json
"Commands": [
  "git status --porcelain"
]
```

For this VSIX repository, do not use `dotnet build` unless explicitly requested.

## README.md

Package root `README.md` is for the human.

It should include:

```text
Purpose
Files Added
Files Updated
Validation
Risks
Rollback
Notes
```

It is package metadata and is skipped by the current runner.

## docs/context.md

Use `docs/context.md` for repository AI context updates.

Do not rely on package root `context.md` to update the repo because the current runner skips root metadata files.

## File Path Examples

Correct source file path:

```text
Contollo.Rdel.ZipRunner/RdelModels.cs
```

Correct docs path:

```text
docs/RDEL-AI-SPEC.md
```

Correct root Pages file:

```text
robots.txt
```

Incorrect if the real file is nested:

```text
Contollo.Rdel.ZipRunner.csproj
```

Correct nested project file path:

```text
Contollo.Rdel.ZipRunner/Contollo.Rdel.ZipRunner.csproj
```

## Current Limitations

The current runner does not support:

- delete operations
- rename operations
- patch/hunk updates
- metadata export
- package signatures
- package dependencies
- pre-apply inspection UI

Use full-file updates for now.

## Package Creation Checklist

Before delivering ZIP:

- Manifest exists.
- README exists.
- Paths are relative to target root.
- No blocked paths.
- Commands match project type.
- docs/context.md is updated if architecture changed.
- DECISIONS.md is updated if a decision was made.
- Package README explains risks and rollback.
- ZIP contains a single clean payload root.
