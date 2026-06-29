# RDEL Preflight and Schema

## Purpose

RDEL now includes a host-agnostic package schema and preflight script.

## Files

- `contollo-rdel.schema.json`
- `tools/Validate-RdelPackage.ps1`

## Usage

```powershell
./tools/Validate-RdelPackage.ps1 ./path-to-package.zip
```

or

```powershell
./tools/Validate-RdelPackage.ps1 ./package-folder
```

## What Preflight Checks

- package exists
- manifest exists
- README exists
- manifest JSON is valid
- required fields exist
- target is valid
- blocked paths are not included
- context/decision docs are present when intent indicates architecture or context impact
- package SHA-256 is reported for ZIPs

## Scope

This is repo-local and host-agnostic.

It does not define branching, PR, CI/CD, or GitHub-specific policy.
