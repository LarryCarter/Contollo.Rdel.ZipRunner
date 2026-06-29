# RDEL Architecture Decisions

## ADR-010 — AI Session Manager becomes Context Assembly Engine

Refactor AI Session Manager so session text is assembled from context providers and templates instead of hardcoded StringBuilder blocks.

## ADR-011 — Preserve forward-slash VSToolsPath

Use `$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)` to avoid the `\v` sequence becoming invalid XML character 0x0B.
