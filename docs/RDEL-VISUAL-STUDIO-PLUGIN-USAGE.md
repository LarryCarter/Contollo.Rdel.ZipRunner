# RDEL Visual Studio Plugin Usage

## Purpose

This document explains how a developer uses the current Contollo RDEL ZipRunner Visual Studio extension.

## Current Commands

The extension registers a Tools menu:

```text
Tools
  Contollo RDEL
    Apply Patch
    Test Run
    Rollback Last Patch
```

## Apply Patch

Use this to apply an RDEL ZIP package.

Workflow:

1. Open the target solution in Visual Studio.
2. Select the appropriate project if the package targets `selected-project`.
3. Go to `Tools -> Contollo RDEL -> Apply Patch`.
4. Choose the RDEL ZIP package.
5. Watch the Visual Studio Output pane.
6. Review package SHA-256, metadata, extracted files, applied files, validation output, and Git output.
7. If validation succeeds, continue.
8. If validation fails, inspect the output and run history.

## Test Run

Use this to inspect what a package would do without applying it.

The dry run reports:

- files that would be created
- files that would be overwritten
- blocked files
- skipped package metadata
- commands that would run

Use this before applying packages from another AI.

## Rollback Last Patch

Use this to roll back the most recent RDEL run when rollback data exists.

Rollback restores backed-up files and deletes files that were newly created by the last run.

## Run History

RDEL writes run history under:

```text
.contollo/rdel/runs/{runId}
```

Typical files:

```text
run-history.json
run-output.log
package/
backup/
```

## Git Behavior

The runner creates Git checkpoints and post-apply commits.

The rebuilt extension must be loaded for source changes to affect runtime behavior.

## VSIX Build Warning

This repository is a VSIX project.

Visual Studio Build menu works.

Plain `dotnet build` may fail.

Do not judge the project broken only because CLI `dotnet build` fails.

## Recommended Developer Workflow

For AI-generated RDEL packages:

1. Ask AI to produce an RDEL ZIP.
2. Prefer packages with `GitOnly` validation for docs/context.
3. Run `Test Run`.
4. If the dry run looks correct, run `Apply Patch`.
5. Review Output pane.
6. Build manually in Visual Studio when package changes code.
7. Commit/push if satisfied.
