# RDEL Architecture Decisions

## ADR-018 — Keep engineering workflow policy out of core RDEL

Decision:

Do not include branching, PR rules, GitHub Flow, Conventional Commits, or host-specific CI/CD in core RDEL.

Reason:

RDEL must remain repository-host agnostic.

Consequence:

Those concerns may later become organization/project policy modules, but not core protocol.

## ADR-019 — Add repo-local context store

Decision:

Add `docs/.rdel-context/` as the durable repo-local context store.

Reason:

AI providers often lose project scope. A repo-local context store gives every provider the same source of truth.

## ADR-020 — Add schema and preflight tooling

Decision:

Add `contollo-rdel.schema.json` and `tools/Validate-RdelPackage.ps1`.

Reason:

Malformed packages should be caught before apply. Preflight must be host-agnostic and runnable locally.


<!-- RDEL-DOCOPS-ID: 33EF5B89AD1E1CED -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/decisions/ADR-021-ei.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 01:40:30Z -->

## ADR-021 — Add Engineering Intelligence

Decision:

Add Engineering Intelligence as a first-class RDEL/NCS subsystem.

Reason:

Important engineering reasoning happens in AI chat but is difficult to recover later. Git commits record what changed, but not the full reasoning, rejected approaches, reusable lessons, or future warnings.

Consequence:

The repository now includes `docs/ei/` for decision memory, known gotchas, reusable patterns, and file-specific engineering history.


<!-- RDEL-DOCOPS-ID: ABD85F81F306D90E -->
<!-- RDEL-DOCOPS-SOURCE: .rdel-docops/decisions/ADR-001-017-recovery.md -->
<!-- RDEL-DOCOPS-UTC: 2026-06-30 02:00:15Z -->

## ADR-001 — ZipRunner evolved into RDEL

Decision:

ZipRunner began as a Visual Studio extension for applying ZIP update packages. It evolved into RDEL: Repository Delta Evolution Loop.

Reason:

The system is not merely applying ZIP files. It is becoming a portable, traceable, verifiable workflow for AI-assisted repository evolution.

Consequence:

The project name may remain `Contollo.Rdel.ZipRunner`, but the architecture should treat RDEL as the real protocol/product.

## ADR-002 — RDEL is a protocol, not just a ZIP format

Decision:

RDEL packages need to carry files, intent, context, validation, trust, compatibility, rollback, audit history, and AI instructions.

Reason:

AI-generated changes need more than raw file replacement. They need accountability and reproducibility.

Consequence:

The manifest and package format should continue growing toward package identity, compatibility, trust, verification, and inspection.

## ADR-003 — README and context have different audiences

Decision:

Root package `README.md` is human/package metadata. Repo AI context belongs in `docs/context.md` or DocOps.

Reason:

The runner skips package metadata files. Root README explains the package and should not be confused with repository memory.

Consequence:

Repository context updates should not be placed only in root package metadata.

## ADR-004 — Cumulative context must evolve with meaningful updates

Decision:

Meaningful RDEL updates should update context, memory, or decision documentation.

Reason:

The project cannot rely on chat history.

Consequence:

DocOps now exists to safely append these updates without overwriting long files.

## ADR-005 — Validation must be profile-based

Decision:

Validation should support profiles such as GitOnly, DotNetCli, VisualStudioMsBuild, and Custom.

Reason:

Different project types require different validation paths.

Consequence:

`dotnet build` remains useful for many projects, but this VSIX project requires Visual Studio/MSBuild-aware validation.

## ADR-006 — GitHub Pages provides public AI access

Decision:

Use GitHub Pages for public-safe AI-readable context.

Reason:

Some AI providers cannot reliably fetch normal GitHub repository pages.

Consequence:

Public-safe docs can be exposed through Pages while private/secrets remain blocked.

## ADR-007 — RDEL absorbs SourcePack principles

Decision:

RDEL inherits SourcePack deterministic state, checksum, ignore, no-op, dependency-chain, and deployment-record concepts.

Reason:

Portable state was already the correct direction. RDEL applies that discipline to AI-assisted change.

Consequence:

Future RDEL should include stronger hashes, package dependencies, signing, and reconstruction.

## ADR-008 — RDEL absorbs the AI Anti-Hallucination Protocol

Decision:

AHP becomes RDEL's AI grounding layer.

Reason:

AI output is invalid unless grounded in declared files, traceable references, explicit assumptions, and reproducible output.

Consequence:

AI instructions should require file paths, statuses, assumptions, validation plans, and rollback expectations.

## ADR-009 — Package metadata is skipped by default

Decision:

Current runner skips package metadata such as `contollo-rdel.json`, root `README.md`, root `context.md`, and `.rdel-docops/`.

Reason:

These files control package behavior and should not usually be copied directly into the repository.

Consequence:

Intentional repo documentation updates must use repo paths or DocOps.

## ADR-010 — Context Assembly Engine replaces hardcoded prompt builders

Decision:

AI session context should be assembled from providers/templates/renderers instead of hardcoded strings.

Reason:

The same context can later be copied to clipboard, exported, injected into chat, or sent to APIs.

Consequence:

Context Assembly Engine is the foundation for AI Session Manager and future embedded chat.

## ADR-011 — Preserve safe forward-slash VSToolsPath

Decision:

Use:

```xml
$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)
```

Reason:

A backslash before `v` can be corrupted into a vertical-tab `0x0B`.

Consequence:

All AI packages touching `.csproj` must preserve this safe form.

## ADR-012 — Add Failure Contract

Decision:

AI sessions must tell the AI what to do when information is missing.

Reason:

The biggest hallucination risk is when AI fills gaps with plausible inventions.

Consequence:

AI should state missing information, list assumptions, mark uncertainty, and ask for clarification when needed.

## ADR-013 — Add Response Contract

Decision:

AI implementation responses should follow a consistent structure.

Reason:

Different AI providers should return comparable, reviewable, accountable outputs.

Consequence:

Responses should include summary, affected files, statuses, architecture impact, documentation updates, validation, rollback, assumptions, and the RDEL package.

## ADR-014 — Context Levels replace arbitrary truncation

Decision:

Use Reference, Summary, and Full context levels instead of arbitrary character truncation.

Reason:

Truncation can remove critical protocol rules without the AI knowing what was omitted.

Consequence:

Templates/settings control context detail intentionally.

## ADR-015 — Context Package export is first-class

Decision:

Add context package export for AI providers that cannot access the repository.

Reason:

Users need a single portable AI context bundle.

Consequence:

Clipboard is only one renderer. Context packages become another.

## ADR-016 — Continue Session captures Output Window panes

Decision:

Continue Session should capture Visual Studio Output Window panes.

Reason:

Manual copying build/test/RDEL output was a major user pain point.

Consequence:

Output capture is part of the provider layer and can later support pane selection.

## ADR-017 — AI Session Settings must be user-editable

Decision:

Add Visual Studio settings UI for context levels and output folder.

Reason:

A settings backend without UI was incomplete.

Consequence:

Users can configure AI session behavior from Visual Studio.

## ADR-018 — Keep engineering workflow policy out of core RDEL

Decision:

Do not include branching, PR rules, GitHub Flow, Conventional Commits, or host-specific CI/CD in core RDEL.

Reason:

RDEL must remain repository-host agnostic.

Consequence:

These concerns may later become organization/project policy modules.

## ADR-019 — Add repo-local context store

Decision:

Add `docs/.rdel-context/` as the durable repo-local context store.

Reason:

AI providers often lose project scope.

Consequence:

Every provider gets the same source of truth.

## ADR-020 — Add schema and preflight tooling

Decision:

Add `contollo-rdel.schema.json` and `tools/Validate-RdelPackage.ps1`.

Reason:

Malformed packages should be caught before apply.

Consequence:

Preflight remains host-agnostic and runnable locally.

## ADR-021 — Add Engineering Intelligence

Decision:

Add Engineering Intelligence as a first-class RDEL/NCS subsystem.

Reason:

Important engineering reasoning happens in AI chat but is difficult to recover later.

Consequence:

The repository now includes `docs/ei/` for decision memory, known gotchas, reusable patterns, and file-specific engineering history.

