# RDEL Engineering Intelligence

## Purpose

Engineering Intelligence preserves the reasoning behind engineering work.

Git records what changed.

RDEL records the package and validation.

Engineering Intelligence records why the change happened, what failed, what was learned, and how future projects can reuse that knowledge.

## Two Memory Layers

### Project Memory

Repo-specific records:

- decisions
- engineering notes
- file history
- package history
- known local constraints

### Reusable Engineering Intelligence

Cross-project records:

- known gotchas
- reusable patterns
- failure signatures
- fix recipes
- warnings
- platform-specific lessons

## Core Rule

If solving a problem took investigation, failed attempts, AI discussion, or a non-obvious fix, capture it as Engineering Intelligence.

## Storage

To avoid Windows path-length issues, the current storage root is intentionally short:

```text
docs/ei/
```
