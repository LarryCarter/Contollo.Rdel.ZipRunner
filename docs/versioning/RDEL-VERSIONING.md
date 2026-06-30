# RDEL Versioning

## Purpose

RDEL needs visible versioning so humans and AI can quickly understand the current project state.

## Version Layers

### ProductVersion

The overall Visual Studio extension / runner product version.

Current:

```text
0.3.1-preview
```

### PackageVersion

The version of an individual RDEL package.

Current version state package:

```text
1.3.7-preview
```

### ManifestVersion

The manifest structure version.

Current:

```text
1.2-preview
```

### PackageFormatVersion

The compatible runner package structure version.

Current:

```text
1.1-current-compatible
```

### RdelSessionProtocolVersion

The AI/session communication protocol version.

Current:

```text
1.2-preview
```

## Package Title Convention

Going forward, package names should include a version when useful:

```text
RDEL 1.3.7 — Version State Reconciliation
RDEL 1.4.0 — Package Identity and Compatibility
RDEL 1.4.1 — Package Inspection UI
RDEL 1.4.2 — Validation Profiles / VisualStudioMsBuild
```

The manifest `Name` should remain readable, but README titles and package docs should include version numbers.

## Semantic Meaning

For preview versions:

```text
0.x.y-preview = product still pre-1.0
1.x.y-preview = RDEL package/update stream
```

Recommended product version movement:

```text
0.1.x = core runner
0.2.x = AI Session / context manager
0.3.x = DocOps / Engineering Intelligence / history recovery / about/versioning
0.4.x = package identity and compatibility
0.5.x = package inspection and diff preview
```

## Required Files

Every future meaningful package should update or append:

```text
docs/VERSION.json
docs/CHANGELOG.md
docs/RDEL-PACKAGE-HISTORY.md
```

For cumulative docs, use DocOps.
