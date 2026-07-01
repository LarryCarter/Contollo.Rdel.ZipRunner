# RDEL Package Naming Standard

## Purpose

RDEL package names must be predictable across projects, AI providers, and future tooling.

A package filename should identify:

- that it is an RDEL package
- package version
- target project
- short description

## Required ZIP Filename Format

Use lowercase kebab-case:

```text
rdel-{version}-{project}-{description}.zip
```

## Examples

```text
rdel-1.4.1-contollo-rdel-ziprunner-package-naming-ai-docs.zip
rdel-1.4.2-contollo-rdel-ziprunner-validation-profiles.zip
rdel-2.0.0-myproject-initial-rdel-context.zip
```

## Required Package Root Folder

The ZIP should contain one root folder.

The root folder should normally match the ZIP filename without `.zip`, but may be shortened when path length is a concern.

Preferred:

```text
rdel-1.4.1-contollo-rdel-ziprunner-package-naming-ai-docs/
```

Allowed short root:

```text
r141/
```

## Required Manifest Name Format

Use a readable title with version and project context:

```text
RDEL 1.4.1 — Contollo.Rdel.ZipRunner — Package Naming and AI Docs
```

For short project-specific updates, this is also acceptable:

```text
RDEL 1.4.1 — Package Naming and AI Documentation
```

## Required Manifest Identity Fields

Every future package should include:

```json
{
  "PackageId": "contollo.rdel.package-naming-ai-docs",
  "PackageVersion": "1.4.1-preview",
  "Author": "Contollo",
  "Company": "Contollo",
  "Category": "DocumentationAndPrompting",
  "Tags": ["rdel"],
  "ManifestVersion": "1.3-preview",
  "Compatibility": {
    "MinimumRunnerVersion": "0.3.1-preview",
    "RequiresDocOps": true,
    "RequiresGit": true
  }
}
```

## Why This Matters

Without the project name and version in the ZIP filename, packages become hard to sort, audit, reuse, and apply safely across multiple repositories.

## AI Rule

When an AI creates an RDEL package, it must name the output ZIP using:

```text
rdel-{version}-{project}-{description}.zip
```

If the target project is unknown, the AI must ask or use a clear placeholder such as:

```text
rdel-1.0.0-unknown-project-description.zip
```

## Path Length Rule

Windows path length can break extraction when package roots and internal paths are too long.

Prefer clear ZIP names, but keep internal package paths short when needed:

```text
r141/
  contollo-rdel.json
  README.md
  .rdel-docops/context/name.md
```
