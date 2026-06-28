# Contollo.Rdel.ZipRunner — AI Project Context

## Current AI Operating Layer

RDEL now has explicit AI-facing documentation under `docs/`.

AI sessions should read:

```text
docs/context.md
docs/RDEL-AI-OPERATOR-GUIDE.md
docs/RDEL-PACKAGE-AUTHORING-GUIDE.md
docs/RDEL-VISUAL-STUDIO-PLUGIN-USAGE.md
docs/RDEL-AI-SPEC.md
docs/ai-instructions.md
docs/DECISIONS.md
docs/memory.md
```

## Current Rule

AI should not be expected to infer how RDEL works from chat history.

The repository must carry its own protocol explanation.

## Important Clarification

`context.md` alone is not enough.

RDEL now separates the AI communication layer:

- `docs/context.md` = project state
- `docs/memory.md` = durable project memory
- `docs/DECISIONS.md` = architecture decisions
- `docs/ai-instructions.md` = AI behavior rules
- `docs/RDEL-AI-SPEC.md` = formal communication specification
- `docs/RDEL-AI-OPERATOR-GUIDE.md` = step-by-step AI operating manual
- `docs/RDEL-PACKAGE-AUTHORING-GUIDE.md` = how to build packages
- `docs/RDEL-VISUAL-STUDIO-PLUGIN-USAGE.md` = how to use the plugin
- `docs/RDEL-AI-PROMPT-TEMPLATES.md` = prompts for other AI models

## Decision Added

A future AI must be able to read the repository documentation and produce a compliant RDEL package without needing the original ChatGPT conversation.
