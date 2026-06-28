# RDEL Architecture Decisions

## ADR-001 — ZipRunner evolved into RDEL

ZipRunner began as a Visual Studio extension for applying ZIP update packages. It evolved into RDEL: Repository Delta Evolution Loop.

## ADR-002 — RDEL is a protocol, not just a ZIP format

RDEL packages need to carry files, intent, context, validation, trust, compatibility, rollback, audit history, and AI instructions.

## ADR-003 — README and context.md have different audiences

README is human-oriented. context.md is AI-oriented.

## ADR-004 — context.md must evolve with every meaningful update

The project cannot rely on chat history. Every meaningful RDEL update should update context, memory, or decision documents.

## ADR-005 — Validation must be profile-based

`dotnet build` remains the default for normal .NET projects, but VSIX projects require Visual Studio MSBuild or IDE build validation.

## ADR-006 — GitHub Pages provides public AI access

GitHub Pages should be used for public-safe AI context.

## ADR-007 — RDEL absorbs SourcePack principles

SourcePack deterministic state, checksum, ignore, and deployment record concepts should be carried forward.

## ADR-008 — RDEL absorbs the AI Anti-Hallucination Protocol

AHP becomes RDEL's AI Grounding layer.

## ADR-009 — Package metadata is skipped by default

Current runner skips metadata files such as README.md and context.md. MetadataExport is needed to intentionally publish package metadata into the repository.
