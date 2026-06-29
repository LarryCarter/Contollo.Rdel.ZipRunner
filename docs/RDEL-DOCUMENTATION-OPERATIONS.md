# RDEL Documentation Operations

## Purpose

RDEL Documentation Operations, or DocOps, prevent AI packages from overwriting cumulative documentation files.

The cumulative files are:

```text
docs/context.md
docs/memory.md
docs/DECISIONS.md
```

These files can become long. AI should not rewrite them directly.

Instead, AI packages should submit append operations under `.rdel-docops/`.

## Package Convention

```text
.rdel-docops/
  context/
    2026-06-29-feature-update.md
  memory/
    2026-06-29-memory-update.md
  decisions/
    ADR-021-docops.md
```

The runner auto-discovers these folders and appends entries to:

```text
.rdel-docops/context/*.md    -> docs/context.md
.rdel-docops/memory/*.md     -> docs/memory.md
.rdel-docops/decisions/*.md  -> docs/DECISIONS.md
```

## Manifest Operations

Packages may also declare operations explicitly:

```json
{
  "DocumentationOperations": [
    {
      "Type": "Append",
      "Target": "docs/context.md",
      "Source": ".rdel-docops/context/2026-06-29-update.md",
      "Section": "Evolution History"
    }
  ]
}
```

## Runner Behavior

The runner:

1. Extracts the package.
2. Applies normal files.
3. Skips `.rdel-docops/` as package metadata.
4. Discovers documentation operations.
5. Backs up the target cumulative doc.
6. Appends the doc operation entry.
7. Adds a marker so the same operation is not appended twice.

## AI Rule

Do not directly overwrite cumulative docs unless explicitly instructed.

Use DocOps for context, memory, and decisions updates.

## Current Limitation

The package that first installs DocOps cannot use DocOps to update the cumulative files because the old runner does not know DocOps yet. After this package is applied and rebuilt, future packages should use DocOps.
