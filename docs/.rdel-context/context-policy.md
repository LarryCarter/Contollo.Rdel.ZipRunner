# RDEL Context Policy

## Purpose

Defines what should be included in assembled AI context and context packages.

## Context Levels

### Reference

Include only local path and public URL when available.

### Summary

Include stable summary, headings, and current-state excerpts.

### Full

Include complete file text.

## Recommended Defaults

- Initialize: Reference
- Rehydrate: Summary
- Continue: Reference

## Must Include For Initialize

- Protocol
- AI Contract
- Failure Contract
- Response Contract
- Capability Matrix
- Package Skeleton
- Project summary
- Documentation references

## Must Include For Rehydrate

- Project summary
- Memory
- Decisions
- Current state
- Git status

## Must Include For Continue

- Current task
- Active document
- Output panes
- Git status
- RDEL run history path

## Public-Safe Rule

Do not include secrets, credentials, production logs, private keys, customer data, or PII in public GitHub Pages context.
