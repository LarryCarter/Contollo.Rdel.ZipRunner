# RDEL AI Access via GitHub Pages

## Purpose

AI models often cannot reliably fetch content from `github.com` because `github.com/robots.txt` may block automated access.

GitHub Pages provides a simple workaround for public repositories.

When GitHub Pages is enabled, repository content can be served from:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/
```

This gives AI systems a stable, model-accessible way to load project documentation and context.

## Setup

In GitHub:

1. Open the repository.
2. Go to **Settings**.
3. Open **Pages**.
4. Set source to the `master` branch.
5. Set folder to `/ (root)`.
6. Save.

After publishing, the repository root README should be available at:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/
```

## Canonical Context URLs

AI project context:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/context.md
```

RDEL AI access documentation:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-ACCESS.md
```

Compliance summary:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-COMPLIANCE-SUMMARY.md
```

Upgrade notes:

```text
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-2.0-UPGRADE-NOTES.md
```

## Why This Matters

RDEL is intended to transfer verified project state between:

- humans
- AI models
- IDEs
- machines
- future Neuro Commander Studio clients

A public AI-accessible context URL means an AI model can begin a session with:

```text
Read the RDEL project context from:
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/context.md
```

That reduces repeated explanation, context loss, and conversation overhead.

## Manifest Direction

Future RDEL manifests should support:

```json
{
  "AiContextUrl": "https://larrycarter.github.io/Contollo.Rdel.ZipRunner/context.md"
}
```

Possible related fields:

```json
{
  "HumanReadmeUrl": "https://larrycarter.github.io/Contollo.Rdel.ZipRunner/",
  "AiSpecUrl": "https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-SPEC.md",
  "MemoryUrl": "https://larrycarter.github.io/Contollo.Rdel.ZipRunner/memory.md"
}
```

## AI Usage Pattern

A developer can start a new AI session with:

```text
Use this project context:
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/context.md

Generate changes as a current-format RDEL package.
Use GitOnly validation unless code changes require a stronger profile.
```

## Safety Note

Only publish public-safe context through GitHub Pages.

Do not publish:

- secrets
- production configs
- customer data
- private work source
- tokens
- certificates
- personal data
- internal-only business information

Public GitHub Pages context should contain architecture and project state, not sensitive data.
