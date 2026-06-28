# RDEL 2.0 Upgrade Notes

## Purpose

RDEL 2.0 should turn the current apply runner into a verified portable AI-change state system.

The goal is:

```text
Git = portable source state
Docker = portable runtime state
RDEL = portable AI-change state
```

## Current Format

The current package format uses:

```json
{
  "Name": "Package name",
  "Description": "Package description",
  "Target": "solution",
  "Commands": [
    "dotnet restore",
    "dotnet build --no-restore",
    "dotnet test --no-build"
  ]
}
```

## Future Packet Layout

```text
package.rdel.zip
  contollo-rdel.json
  README.md
  context.md
  changes/
  verification/
  rollback/
  audit/
```

## File Roles

### README.md

Human understanding:

- What is this package?
- What problem does it solve?
- What changed?
- How do I apply it?
- How do I verify it?
- How do I roll it back?

### context.md

AI/model understanding:

- Current project state
- Assumptions
- Architecture rules
- Coding style
- Important files/classes
- Do-not-change rules
- Developer intent
- Prior decisions that must carry forward

### contollo-rdel.json

Machine contract:

- Schema version
- Package type
- Target
- Commands
- Baseline
- Expected after apply
- Trust level
- Source provider
- Required files
- Blocked paths
- Verification contracts

## Next Required Improvements

1. Formal RDEL 2.0 schema.
2. Human README package standard.
3. AI context package standard.
4. Baseline/expected verification contract.
5. Package hash and trust metadata.
6. Expanded security policy.
7. Diff preview.
8. Intent Merge Mode.
9. Script safety model.
10. Neuro Commander Studio integration.
