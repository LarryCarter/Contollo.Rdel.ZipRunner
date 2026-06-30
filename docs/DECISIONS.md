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

