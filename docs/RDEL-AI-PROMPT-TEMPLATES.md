# RDEL AI Prompt Templates

## Start a New AI Session

```text
You are helping with the Contollo RDEL ZipRunner project.

First read:
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/context.md
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-OPERATOR-GUIDE.md
https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-SPEC.md

Generate changes as a current-format RDEL package.

Use:
- contollo-rdel.json
- README.md
- docs/context.md when context changes
- docs/DECISIONS.md when architecture decisions change

Use GitOnly validation for documentation-only updates.

Do not use dotnet build for this VSIX repository unless explicitly requested.
```

## Ask AI To Create a Docs Package

```text
Create a current-format RDEL package that updates documentation only.

Requirements:
- Use Target: solution
- Use ValidationProfile: GitOnly
- Use Commands: ["git status --porcelain"]
- Include package README.md
- Update docs/context.md if project context changes
- Update docs/DECISIONS.md if a design decision is made
- Do not include blocked paths
- Do not include root context.md because the current runner skips package metadata
```

## Ask AI To Create a Code Package

```text
Create a current-format RDEL package for this code change.

Before generating the package:
1. State what files you believe exist.
2. State assumptions.
3. State missing information.
4. List every file you will create or update.
5. Explain validation.

Package rules:
- Use full-file updates.
- Preserve exact paths.
- Do not invent references.
- Update docs/context.md if architecture changes.
- For this VSIX repository, avoid dotnet build validation unless explicitly requested.
```

## Ask AI To Review a Package

```text
Review this RDEL package for compliance.

Check:
- manifest correctness
- file paths
- blocked paths
- validation commands
- README clarity
- context updates
- hallucinated references
- package metadata skip issues
- whether any file is accidentally placed at repo root
```
