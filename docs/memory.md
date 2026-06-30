# RDEL Project Memory

## Context Manager Foundation Closed

The AI Context Manager foundation now includes:

- RDEL Session Protocol
- Context Assembly Engine
- context levels
- settings UI
- output capture
- context package export
- repo-local context store
- schema and preflight tooling
- package templates

## Boundary

RDEL core remains host-agnostic. Engineering workflows such as branching, PR rules, and CI/CD are not core RDEL.


<!-- RDEL-DOCOPS-ID: FDACD54B8FF8E3B0 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/memory/2026-06-29-ei-memory.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 01:40:30Z -->

## 2026-06-29 — Engineering Intelligence Memory

Stable rule:

If solving a problem required investigation, failed attempts, AI discussion, or a non-obvious fix, create an Engineering Intelligence record.

Current first known gotcha:

- VSIX `.csproj` `VSToolsPath` backslash-v / `0x0B` vertical-tab corruption.

Engineering Intelligence should become searchable and reusable across projects.


<!-- RDEL-DOCOPS-ID: D5AEBA2BA2DA3D82 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/memory/2026-06-29-history-recovery-memory.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:00:15Z -->

## 2026-06-29 — Recovered Durable RDEL Memory

### Stable Project Identity

This repository is `Contollo.Rdel.ZipRunner`, but the product direction is RDEL: Repository Delta Evolution Loop.

RDEL is the package/change engine for Neuro Commander Studio.

### Stable Principle: Account for Code

AI must not merely suggest code. AI must account for code.

Every meaningful change should identify:

- exact file path
- file status
- reason
- relationship to existing files
- assumptions
- validation plan
- rollback expectation

### Stable Principle: Cumulative Docs Use DocOps

Do not directly overwrite cumulative documentation files:

```text
docs/context.md
docs/memory.md
docs/DECISIONS.md
```

Use DocOps append files instead.

### Stable Principle: RDEL Is Host-Agnostic

Branching, PR rules, Conventional Commits, GitHub Flow, and host-specific CI/CD are not core RDEL.

Those belong in future organization/project policy modules.

### Stable Principle: Commands Are Authoritative

Until validation profiles are fully implemented, `Commands` are the authoritative validation mechanism.

For documentation-only packages, use:

```json
"Commands": ["git status --porcelain"]
```

### Stable Principle: VSIX Validation Is Special

This is a Visual Studio extension project.

Do not assume plain `dotnet build` is valid for this repository.

Future validation should use `VisualStudioMsBuild`.

### Stable Principle: Safe VSToolsPath

When generating or editing the VSIX `.csproj`, preserve:

```xml
$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)
```

Do not emit a backslash immediately before `v$(VisualStudioVersion)`.

### Stable Principle: SourcePack Heritage

RDEL inherits SourcePack concepts:

- portable state
- checksums
- deterministic reconstruction
- status tracking
- no-op protection
- ignore rules
- audit records
- future signing

### Stable Principle: Project Context Is the Product

The prompt is not the product.

Context is the product.

The Context Assembly Engine should assemble reusable context that can be rendered to clipboard, exported as a package, injected into embedded chat, or sent to an API later.

### Stable Principle: Engineering Intelligence

If solving a problem required investigation, AI discussion, failed attempts, or a non-obvious fix, create an Engineering Intelligence record.

Known gotchas and reusable patterns should be reusable across projects when possible.

### Current Baseline Memory

The project currently has:

- RDEL package runner
- dry run
- rollback
- Git checkpointing
- run history
- AI Session / RSP
- Context Assembly Engine
- settings UI
- output capture
- context package export
- schema/preflight tooling
- DocOps
- Engineering Intelligence

### Current Missing Memory

The next high-value work is:

- recover/apply Phase 2 context builder safely
- add package identity/compatibility enforcement
- add package inspection UI
- add context preview/token estimate


<!-- RDEL-DOCOPS-ID: A598A8CF8024E8C2 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/memory/2026-06-29-versioning-memory.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:15:55Z -->

## 2026-06-29 — Versioning Memory

Stable rule:

RDEL should maintain visible version state in:

```text
docs/VERSION.json
docs/CHANGELOG.md
docs/ABOUT-RDEL.md
```

Future package titles should include version numbers when useful.

Current product baseline:

```text
0.3.0-preview
```

Current versioning phase:

```text
Phase 1.3 / AI Context Manager Foundation
```

Cumulative docs still must use DocOps.


<!-- RDEL-DOCOPS-ID: 3AB5872713FE3411 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/memory/2026-06-29-about-menu-memory.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:33:53Z -->

## 2026-06-29 — About Menu Memory

Stable rule:

The Visual Studio extension should expose current project/version status from `docs/VERSION.json`.

The About dialog is the first user-facing version/status surface.

Future version updates should keep `docs/VERSION.json` current so the About dialog remains accurate.


<!-- RDEL-DOCOPS-ID: F7F8E04DABE9A084 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/memory/2026-06-29-version-reconciliation-memory.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 03:03:58Z -->

## 2026-06-29 — Version State Reconciliation Memory

Stable rule:

After user-facing feature packages, update `docs/VERSION.json` so the About RDEL dialog reflects the real latest baseline.

Current next recommended feature:

```text
RDEL 1.4.0 — Package Identity and Compatibility
```

Reason:

Package identity and compatibility should be enforced before adding more package inspection, diff preview, or validation profile features.

