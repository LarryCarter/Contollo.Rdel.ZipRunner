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

