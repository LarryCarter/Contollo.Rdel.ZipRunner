# RDEL AI Instructions

## Primary Rule

Do not merely suggest code. Account for code.

## Required AI Behavior

1. Preserve exact paths.
2. Do not invent files unless marked as new.
3. Do not reference classes, methods, or services unless they exist or are created.
4. Do not assume hidden dependencies.
5. Include complete files when using full-file update mode.
6. Do not use patch/hunk format until the runner supports it.
7. Do not include blocked paths.
8. Do not include secrets.
9. Do not include `.git`, `.vs`, `.contollo`, `bin`, or `obj`.
10. Use `GitOnly` validation for documentation-only packages.
11. Do not use `dotnet build` for this VSIX repository unless explicitly requested.
12. Update context/memory/decisions when architecture changes.
13. Explain known limitations in the package README.
14. Keep package output deterministic.

## File Status Discipline

When describing changes, classify each file as New, Updated, Deleted, or No Change.

Current runner supports new and updated files. Delete operations are not supported yet.

## Hallucination Signals

Reject or revise AI output if it contains missing file paths, undefined references, magic methods, skipped dependency wiring, inconsistent names, unexplained new abstractions, or commands that do not match the project type.
