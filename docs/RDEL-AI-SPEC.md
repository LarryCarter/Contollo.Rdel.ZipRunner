# RDEL AI Communication Specification

## Status

Draft v1.2-preview.

## Purpose

RDEL is a protocol for portable, verified, AI-assisted software change.

It is designed to reduce context loss, hallucinated code, unverifiable changes, hidden assumptions, rogue edits, and environment drift.

## Required Concepts

An RDEL package should carry files, intent, context, validation, trust, compatibility, history, rollback information, and AI instructions.

## AI Grounding Protocol

AI output must be grounded in repository reality.

Rules:

1. Every file change must have a path.
2. Every file change must have a status.
3. Every referenced symbol must exist or be created.
4. Every assumption must be declared.
5. Every dependency must be explicit.
6. Every output must be reproducible.
7. Every architecture-impacting change must update context.

## Human vs AI Documents

- README.md — human-facing explanation.
- context.md — AI-facing project/task context.
- memory.md — durable project knowledge.
- DECISIONS.md — architecture decision history.
- ai-instructions.md — AI operating rules.

## Manifest Direction

Current manifest fields include Name, Description, Target, and Commands.

Forward-compatible fields include SchemaVersion, ManifestVersion, PackageVersion, PackageType, ChangeIntent, TrustLevel, SourceProvider, ValidationProfile, HumanReadmePath, and AiContextPath.

## Validation Profiles

- DotNetCli — default for normal .NET projects.
- GitOnly — docs/context/package smoke tests.
- VisualStudioMsBuild — VSIX and Visual Studio SDK projects.
- Custom — package-defined command validation.

## SourcePack Compatibility

RDEL should preserve SourcePack principles: deterministic state, checksums, status tracking, dependency chains, no-op protection, forced reset, ignore rules, deployment records, and future signatures.

## Package Lifecycle

Expected lifecycle:

```text
Create
Package
Hash
Inspect
Verify
Approve
Apply
Validate
Commit
Archive
Rollback if needed
```

## Future MetadataExport

Current runner skips root package metadata.

Future RDEL should support:

```json
{
  "MetadataExport": {
    "PublishContext": true,
    "ContextTargetPath": "context.md",
    "PublishReadme": false
  }
}
```

## AI Access URLs

Future manifest fields may include AiContextUrl and AiSpecUrl pointing to GitHub Pages URLs.

## Non-Goals for Current Runner

Current runner does not yet support delete operations, patch/hunk approval, package signatures, dependency enforcement, metadata export, diff previews, or enterprise policy.
