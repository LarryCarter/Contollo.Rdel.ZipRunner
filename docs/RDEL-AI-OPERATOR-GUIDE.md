# RDEL AI Operator Guide

## Purpose

This document tells an AI model how to operate inside the RDEL workflow.

It is written for ChatGPT, Claude, Gemini, Copilot, Grok, local models, and future AI providers.

## The Most Important Rule

Do not merely suggest code.

Account for code.

Every change must have:

- exact file path
- file status
- reason
- relationship to existing files
- validation plan
- rollback expectation
- documentation/context update when appropriate

## Required Output

When asked to make repository changes, produce a complete RDEL ZIP package unless the user asks otherwise.

Required package root files:

```text
contollo-rdel.json
README.md
```

## Required ZIP Naming

Every generated package ZIP must use:

```text
rdel-{version}-{project}-{description}.zip
```

Example:

```text
rdel-1.4.1-contollo-rdel-ziprunner-package-naming-ai-docs.zip
```

Use lowercase kebab-case.

Include the project name so the package can be identified outside the current chat.

## Required Manifest Identity

Every future package should include:

```json
{
  "PackageId": "contollo.rdel.package-name",
  "PackageVersion": "1.x.x-preview",
  "Author": "Contollo",
  "Company": "Contollo",
  "Category": "Feature",
  "Tags": ["rdel"],
  "ManifestVersion": "1.3-preview",
  "Compatibility": {
    "MinimumRunnerVersion": "0.3.1-preview",
    "RequiresDocOps": true,
    "RequiresGit": true
  }
}
```

## Current Metadata Skip Rule

The runner skips package metadata files at package root:

```text
contollo-rdel.json
contollo-rdel.txt
README.md
context.md
manifest.json
.rdel-docops/
```

Therefore, root `README.md` explains the package and is not applied to the repo.

## Cumulative Docs Rule

Do not directly overwrite:

```text
docs/context.md
docs/memory.md
docs/DECISIONS.md
```

Use DocOps:

```text
.rdel-docops/context/
.rdel-docops/memory/
.rdel-docops/decisions/
```

## Existing Project Rule

If the target project is existing and the AI does not already have context, ask for or generate project intake first.

Minimum intake:

```text
README
directory tree
solution/project file
important source files
build/test commands
current output/errors
docs/context.md if present
docs/VERSION.json if present
```

For AI tools that cannot reliably read ZIP files, prefer a single-file intake bundle such as a SourcePack-style `.spack` or future `.rdelctx`.

## File Status Rules

Use:

```text
New
Updated
Deleted
No Change
```

Current runner supports new and updated files.

Current runner does not support delete operations.

## Blocked Paths

Never include:

```text
.git/
.vs/
.contollo/
bin/
obj/
.env
secrets.json
appsettings.Production.json
*.pfx
*.pem
*.key
*.cer
*.crt
*.snk
```

Do not include secrets, credentials, production logs, customer data, tokens, certificates, or private keys.

## Validation Rules

The manifest must define validation through `Commands`.

For documentation-only packages:

```json
{
  "ValidationProfile": "GitOnly",
  "Commands": ["git status --porcelain"]
}
```

For normal SDK-style .NET projects:

```json
{
  "ValidationProfile": "DotNetCli",
  "Commands": [
    "dotnet restore",
    "dotnet build --no-restore",
    "dotnet test --no-build"
  ]
}
```

For this VSIX repository, do not use plain `dotnet build` unless explicitly requested.

## Package Explanation Format

Before providing the ZIP, summarize:

```text
Package:
ZIP filename:
PackageId:
PackageVersion:
Purpose:
Validation:
Files:
- New:
- Updated:
Known risks:
Rollback:
```

## AI Grounding Checklist

Before finishing, verify:

- ZIP filename follows `rdel-{version}-{project}-{description}.zip`
- Manifest includes identity/version fields
- All referenced files are included or known to exist
- New files use correct paths
- Metadata files are not expected to apply
- Cumulative docs use DocOps
- Validation matches project type
- Blocked paths are avoided
- Package README is human-readable
- The next AI session will understand the change
