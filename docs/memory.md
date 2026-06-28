# RDEL Project Memory

## Purpose

This file stores durable project memory: stable project knowledge, long-term design principles, and decisions that should remain true across AI sessions.

## Stable Principles

### RDEL is portable AI-change state

RDEL is not just a ZIP runner. It is becoming the portable state protocol for AI-assisted software change.

### SourcePack principles remain important

RDEL inherits SourcePack deterministic state philosophy: reproducible state, checksums, status tracking, dependency chains, no-op protection, forced reset, ignore rules, deployment records, and future signatures.

### AI must account for code

RDEL inherits the AHP principle that AI output is invalid unless grounded in declared files, traceable references, explicit assumptions, and reproducible output.

### Documentation is part of the change

A code change that alters architecture must update context, memory, or decision documentation.

### Validation must be project-aware

`dotnet build` is a good default for normal .NET projects, but not every project can be validated by `dotnet build`.

### GitHub Pages is the public AI access layer

For public repositories, GitHub Pages can expose AI-readable context without requiring AI tools to scrape GitHub directly.

## Long-Term Architecture

RDEL will become a subsystem of Neuro Commander Studio.

Neuro Commander Studio will eventually provide WebView AI workspace, provider management, local context engine, knowledge vault, edit orchestration, approval workflow, diff/patch approval, and RDEL package generation/application.

RDEL remains the change engine.
