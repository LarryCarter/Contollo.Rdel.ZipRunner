# Contollo.Rdel.ZipRunner — AI Project Context

## Purpose

This is the primary AI-loadable project context for RDEL.

Future AI sessions should read this file first before generating code, reviewing architecture, or creating RDEL packages.

This file is not a chat transcript. It is durable project context: what the project is, how it evolved, what decisions matter, what constraints apply, and what must not be forgotten.

## AI Access

Preferred public context URL after GitHub Pages is enabled:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/context.md
```

Related documents:

```text
docs/memory.md
docs/DECISIONS.md
docs/ai-instructions.md
docs/RDEL-AI-SPEC.md
docs/RDEL-AI-ACCESS.md
```

## Project Identity

Repository:

```text
LarryCarter/Contollo.Rdel.ZipRunner
```

Product:

```text
Contollo RDEL ZipRunner
```

Current role:

```text
Visual Studio 2022 extension that applies RDEL ZIP packages.
```

Long-term role:

```text
RDEL becomes the verified portable AI-change state layer used by Neuro Commander Studio.
```

## Core Concept

RDEL means Repository Delta Evolution Loop.

Core analogy:

```text
Git        = portable source state
Docker     = portable runtime state
SourcePack = deterministic codebase state
RDEL       = portable AI-change state
```

RDEL is intended to carry not only changed files, but also intent, context, verification, trust, rollback, and AI-readable explanation.

## Relationship to SourcePack

SourcePack was the earlier deterministic codebase state system.

SourcePack principles carried forward into RDEL:

- source of truth is a package
- valid states must be reproducible
- file status is tracked as new, updated, or unchanged
- checksums expose unauthorized edits
- diffs depend on previous versions
- no-op protection avoids fake versions
- force and clean restore eliminate drift
- ignore rules reduce attack surface
- deployment records document what is running

SourcePack protected code integrity.

RDEL protects code integrity plus AI change integrity.

## Relationship to AI Anti-Hallucination Protocol

AHP was created to prevent AI-generated phantom code and unverifiable changes.

AHP principles carried forward into RDEL:

- every code block must declare exact file path
- every file must declare status: New, Updated, or No Change
- AI must state what exists, what is assumed, and what is missing
- every referenced type, function, or dependency must be traceable to an existing file or explicitly created
- no implicit logic or hidden dependencies
- generated output must be reproducible from declared files and dependencies
- AI must account for code, not merely suggest code

## Fundamental RDEL Principle

AI is not allowed to merely suggest code.

AI must account for code.

Every generated change must answer:

- Where does it live?
- Why does it belong there?
- What existing files does it depend on?
- What new files does it create?
- What files are unchanged?
- How is it validated?
- How is it rolled back?
- What architectural decision does it affect?
- What context must be updated?

## Current Runner Capabilities

The current runner can select RDEL ZIP packages, extract safely, read `contollo-rdel.json`, detect package README/context metadata, compute SHA-256, apply files, skip metadata, block dangerous paths, run validation commands, write run history, create Git checkpoints, create post-apply commits, roll back last run, and store verification summaries.

## Current Runner Limitations

- Delete operations are not supported.
- MetadataExport is not supported yet.
- Root `context.md` inside a package is skipped as package metadata.
- ValidationProfile is documented but not yet implemented as first-class behavior.
- Current validation still uses explicit `Commands`.
- Diff/hunk approval does not exist yet.
- Package signatures do not exist yet.
- Package inspection before apply is incomplete.
- Package dependencies and conflicts are not enforced yet.

## Build Rule for This Repository

This repository is a Visual Studio Extension / VSIX project.

Visual Studio Build menu works.

Plain `dotnet build` can fail because it does not resolve Visual Studio SDK assemblies such as EnvDTE, EnvDTE80, Microsoft.VisualStudio.Shell, AsyncPackage, OleMenuCommandService, IVsOutputWindowPane, and VSConstants.

For this repository, use GitOnly or VisualStudioMsBuild validation.

## Validation Profile Direction

RDEL should support validation profiles:

- DotNetCli — default for normal SDK-style .NET projects.
- GitOnly — docs/context/metadata/smoke tests.
- VisualStudioMsBuild — VSIX, EnvDTE, Visual Studio SDK projects.
- Custom — package-provided commands.

## GitHub Pages AI Access

GitHub Pages should publish public-safe AI context because AI models may not be able to fetch from `github.com` directly.

Root `robots.txt` should explicitly allow bots:

```text
User-agent: *
Allow: /
```

Only public-safe information should be published.

## Documentation Roles

- `README.md` — human package/project explanation.
- `context.md` — AI-loadable current project/task context.
- `memory.md` — durable project memory.
- `DECISIONS.md` — architecture decision records.
- `ai-instructions.md` — AI operating rules.
- `RDEL-AI-SPEC.md` — formal AI communication specification.

## Required Behavior for Future AI Updates

Every meaningful RDEL update should consider whether to update:

- `docs/context.md`
- `docs/memory.md`
- `docs/DECISIONS.md`
- `docs/RDEL-AI-SPEC.md`
- package README

## Current Roadmap Focus

Current focus:

```text
Epic 1 — Core Package Engine
Epic 1.2 — Package Identity, Compatibility, Discovery, and AI Communication
```

Near-term priorities:

1. Finish AI context/memory/spec documentation.
2. Add ValidationProfile support.
3. Add MetadataExport.
4. Add package inspection before apply.
5. Add delete operations and cleanup actions.
6. Add SourcePack/AHP-style grounding checks.
7. Add package signatures and trust policy.
