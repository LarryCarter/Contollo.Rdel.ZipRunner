# RDEL Versioning

## Purpose

RDEL needs visible versioning so humans and AI can quickly understand the current project state.

## Version Layers

### ProductVersion

The overall Visual Studio extension / runner product version.

Example:

```text
0.3.0-preview
```

### PackageVersion

The version of an individual RDEL package.

Example:

```text
1.3.5-preview
```

### ManifestVersion

The manifest structure version.

Example:

```text
1.2-preview
```

### PackageFormatVersion

The compatible runner package structure version.

Example:

```text
1.1-current-compatible
```

### RdelSessionProtocolVersion

The AI/session communication protocol version.

Example:

```text
1.2-preview
```

## Package Title Convention

Going forward, package names should include a version when useful:

```text
RDEL 1.3.5 — About Versioning DocOps
RDEL 1.3.6 — Phase 2 Context Builder
RDEL 1.4.0 — Package Identity Enforcement
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
0.3.x = DocOps / Engineering Intelligence / history recovery
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
