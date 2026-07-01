# Contollo.Rdel.ZipRunner — AI Project Context

## Context Store / Schema / Preflight Update

The repository now includes a repo-local RDEL context store:

```text
docs/.rdel-context/
```

Current context store files:

- `project-summary.md`
- `context-policy.md`
- `current-state.json`
- `snapshots/README.md`

The repository also includes:

- `contollo-rdel.schema.json`
- `tools/Validate-RdelPackage.ps1`
- `docs/templates/rdel-package-checklist.md`
- docs-only and code-change package skeletons

## Scope Decision

Branching, PR rules, CI/CD, Conventional Commits, and GitHub-specific workflows are intentionally out of scope for core RDEL.

Those belong in future organization/project policy modules if needed.

## Current Milestone

This package closes the host-agnostic AI Context Manager foundation.

Remaining future UI work:

- context preview window
- token estimate
- selectable output panes
- context snapshot generation from UI


<!-- RDEL-DOCOPS-ID: 70B6B67F3095EDBA -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/2026-06-29-ei.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 01:40:30Z -->

## 2026-06-29 — Engineering Intelligence Foundation

RDEL now adds an Engineering Intelligence layer.

Engineering Intelligence captures:

- why a change happened
- what failed
- what was learned
- what should be reused later
- known gotchas
- reusable patterns
- file-specific engineering history

This is separate from Git history. Git records what changed. Engineering Intelligence records why the decision was made.

Storage root:

```text
docs/ei/
```


<!-- RDEL-DOCOPS-ID: C192585E09BDF60D -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/2026-06-29-history-recovery.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:00:15Z -->

## 2026-06-29 — Recovered RDEL Project History

This entry recovers the major project history that was lost when earlier full-file RDEL packages overwrote the cumulative documentation files.

### Product Direction

The project began as `Contollo.Rdel.ZipRunner`, a Visual Studio 2022 extension for applying ZIP-based update packages. It evolved into **RDEL**: Repository Delta Evolution Loop.

RDEL is no longer just a ZIP runner. It is the package/change engine for Neuro Commander Studio.

### Neuro Commander Studio Direction

Neuro Commander Studio is the broader AI development workbench.

Its first practical client is the Visual Studio extension, but the long-term architecture includes:

- Visual Studio
- VS Code
- JetBrains
- browser extension
- Office/Teams/internal apps
- WebView provider workspaces
- local context engine
- approval workflow
- AI context management
- RDEL package apply/rollback/verification

Version 1 intentionally supports AI websites and web/app interfaces before direct LLM APIs.

### RDEL Core Engine History

The core package engine established:

- ZIP package selection
- safe extraction with ZipSlip protection
- solution root detection
- selected project root detection
- `contollo-rdel.json` manifest support
- full-file apply mode
- backup of overwritten files
- validation commands
- SHA-256 package hash
- run history
- Git checkpoints
- post-apply commits
- dry run
- rollback

### RDEL Package Philosophy

A package should become a complete, portable, self-describing, verifiable unit of work that can safely move between developers, AI models, and environments.

The package is not only code. It carries intent, validation, context, rollback expectations, package identity, and future trust information.

### SourcePack Inheritance

RDEL inherited key SourcePack principles:

- deterministic state
- checksums
- status tracking
- dependency chains
- no-op protection
- force/reset capability
- ignore rules
- deployment/run records
- future signing

The conceptual progression is:

```text
SourcePack = portable software state
RDEL = portable change state
RDEL Session Protocol / Context Bundle = portable AI session state
Engineering Intelligence = reusable engineering experience
```

### AI Anti-Hallucination Protocol Inheritance

RDEL also inherits the AI Anti-Hallucination Protocol principle:

> AI must not merely suggest code. AI must account for code.

That means AI-generated work must preserve:

- exact file paths
- file status
- reason for change
- relationship to existing files
- assumptions
- missing information
- validation plan
- rollback expectation

If an AI cannot account for where a file lives, how it connects, and how it is validated, the output is not a valid RDEL package.

### README vs Context

A key decision was that root package `README.md` and AI context are different audiences:

- root `README.md` is human/package metadata
- `docs/context.md` is repo AI context
- package root metadata is skipped by the runner
- repo documentation updates must go through repository paths or DocOps

### Validation Profiles

Validation profiles were defined conceptually:

- `GitOnly`
- `DotNetCli`
- `VisualStudioMsBuild`
- `Custom`

Current runner behavior still treats `Commands` as authoritative. ValidationProfile is documented but not fully enforced.

For this VSIX project, plain `dotnet build` is not the correct validation path by default. Visual Studio Build menu or future `VisualStudioMsBuild` validation should be used.

### VSIX Build Lesson

This project is a Visual Studio extension. Some Visual Studio SDK dependencies resolve correctly inside Visual Studio/MSBuild but not through plain `dotnet build`.

The project also had a serious `.csproj` corruption issue where `\v` could become a vertical-tab `0x0B`. The permanent rule is to preserve:

```xml
$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)
```

### GitHub Pages AI Access

GitHub Pages was selected as a public AI-readable access layer for public-safe docs because normal GitHub pages can be difficult for some AI providers to fetch.

Canonical public context URLs include:

- `docs/context.md`
- `docs/RDEL-AI-OPERATOR-GUIDE.md`
- `docs/RDEL-AI-SPEC.md`

### AI Session Manager / RSP

The AI Session Manager evolved into the **RDEL Session Protocol (RSP)**.

RSP is not merely a prompt. It is a session protocol rendered into clipboard text today and later into embedded chat messages, API payloads, or context packages.

Session types:

- Initialize
- Rehydrate
- Continue

RSP includes:

- Protocol
- AI Contract
- Failure Contract
- Response Contract
- Capability Matrix
- Canonical Package Skeleton
- Project pointers
- Documentation references
- Current task
- Git/output context

### Context Assembly Engine

The Context Assembly Engine replaced hardcoded prompt building.

The current architecture is:

```text
Context Assembly Engine
  -> Providers
  -> Templates
  -> Renderer
  -> Clipboard / Context Package / future embedded chat
```

Providers include project info, protocol, AI contract, failure contract, response contract, capability matrix, package skeleton, project docs, memory, decisions, active document, Git status, output context, and user request.

### Context Levels

Arbitrary truncation was replaced by configurable context levels:

- Reference
- Summary
- Full

This allows the same engine to support short daily continuation prompts, full first-session rehydration, and package exports for AI providers that cannot fetch the repository.

### Settings UI

A settings dialog was added for AI Session / RSP configuration:

- Initialize context level
- Rehydrate context level
- Continue context level
- Context package output directory

### Output Capture

Continue Session was improved to capture Visual Studio Output Window panes directly, reducing the need for manual copy/paste of build and RDEL output.

### Context Store / Preflight

A repo-local context store was added:

```text
docs/.rdel-context/
```

Along with:

- `contollo-rdel.schema.json`
- `tools/Validate-RdelPackage.ps1`
- package checklist
- docs-only package skeleton
- code-change package skeleton

### DocOps

DocOps was added because cumulative Markdown files became too long and were accidentally overwritten by full-file AI packages.

Future AI packages should not directly overwrite:

```text
docs/context.md
docs/memory.md
docs/DECISIONS.md
```

They should use:

```text
.rdel-docops/context/
.rdel-docops/memory/
.rdel-docops/decisions/
```

DocOps appends entries, backs up targets, and adds idempotent markers.

### Engineering Intelligence

Engineering Intelligence was added to preserve reasoning that Git cannot capture.

Git records what changed.

RDEL records the package and validation.

Engineering Intelligence records why the change happened, what failed, what was learned, and how the lesson can be reused.

The short-path storage root is:

```text
docs/ei/
```

### Current Known Good Baseline

After the successful Engineering Intelligence Safe2 package, the current baseline includes:

- RDEL core package runner
- AI Session / RSP
- Context Assembly Engine
- settings UI
- output capture
- context package export
- schema/preflight/tooling
- DocOps append layer
- Engineering Intelligence short-path store
- first known gotcha: VSIX `.csproj` `VSToolsPath` backslash-v / `0x0B`

### Still Missing

The main missing items are:

- package identity and compatibility enforcement
- package dependency checks
- package inspection UI
- package diff preview
- real validation profile enforcement
- VisualStudioMsBuild validation
- context preview window
- token estimator
- selectable output panes
- Phase 2 context builder package application
- knowledge graph integration
- Engineering Intelligence search/query UI


<!-- RDEL-DOCOPS-ID: 3D62180009E65567 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/2026-06-29-about-versioning.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:15:55Z -->

## 2026-06-29 — About and Versioning Added

RDEL now has visible project identity and version status files.

Added:

```text
docs/ABOUT-RDEL.md
docs/VERSION.json
docs/CHANGELOG.md
docs/versioning/RDEL-VERSIONING.md
```

Current product version:

```text
0.3.0-preview
```

Current package/versioning stream:

```text
1.3.5-preview
```

Purpose:

Future users and AI sessions should be able to quickly determine:

- what this project is
- what version/phase it is in
- what features exist
- what is missing
- what package/versioning convention to use


<!-- RDEL-DOCOPS-ID: C97A4A576982B3A4 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/2026-06-29-about-menu.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:33:53Z -->

## 2026-06-29 — About RDEL Menu Added

The Visual Studio extension now includes:

```text
Tools -> Contollo RDEL -> About RDEL
```

The About dialog reads version information from:

```text
docs/VERSION.json
```

It displays:

- Product name
- Protocol name
- Product version
- Package format version
- Manifest version
- RDEL Session Protocol version
- Current phase
- Baseline features
- Known missing features


<!-- RDEL-DOCOPS-ID: 025994329286D77B -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/2026-06-29-version-reconciliation.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 03:03:58Z -->

## 2026-06-29 — Version State Reconciled

The repository-visible version state was reconciled after the About RDEL menu package.

Updated files:

```text
docs/VERSION.json
docs/ABOUT-RDEL.md
docs/CHANGELOG.md
docs/versioning/RDEL-VERSIONING.md
```

Current product version:

```text
0.3.1-preview
```

Latest applied feature package:

```text
RDEL 1.3.6 — About Menu Version UI
```

Current version-state package:

```text
RDEL 1.3.7 — Version State Reconciliation
```

Next recommended feature package:

```text
RDEL 1.4.0 — Package Identity and Compatibility
```


<!-- RDEL-DOCOPS-ID: 0C1FD2FE47F51D65 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/name.md -->
<!-- RDEL-DOCOPS-UTC: 2026-07-01 04:36:03Z -->

## 2026-06-30 — Package Naming and Existing Project Intake

RDEL now has a required package ZIP naming convention:

```text
rdel-{version}-{project}-{description}.zip
```

AI prompts and operator docs were updated so future AI-generated packages include the project name and version in the ZIP filename and package title.

Existing-project intake guidance was also added. If an AI does not understand an existing project, it should request or generate project intake before producing a change package.

SourcePack lessons were captured for future context-pack design.


<!-- RDEL-DOCOPS-ID: F8DBE0C823D4D5B0 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/f.md -->
<!-- RDEL-DOCOPS-UTC: 2026-07-01 04:53:00Z -->

## 2026-06-30 — Provider Fix

Fixed `PackageSkeletonProvider.cs` C# string escaping.

This failure confirmed the need for:

- extraction path length fix
- package diff inspection
- claim-vs-change verification


<!-- RDEL-DOCOPS-ID: C7D4DDE21F72D579 -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/context/out.md -->
<!-- RDEL-DOCOPS-UTC: 2026-07-01 05:10:06Z -->

## 2026-06-30 — Output Preserve Dialog Fix

RDEL completion/failure dialogs now include the run-history path and reactivate the Contollo RDEL Output pane before and after the modal dialog.

This fixes the issue where clicking OK could make the RDEL output hard to inspect.

