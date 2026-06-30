# Engineering Note: Version State Reconciliation

## Summary

After adding the About RDEL menu item, the repository version state needed to be reconciled.

## Problem

`docs/VERSION.json` still pointed to the prior package even though the About RDEL menu had been applied.

## Fix

Update `docs/VERSION.json`, `docs/ABOUT-RDEL.md`, `docs/CHANGELOG.md`, and `docs/versioning/RDEL-VERSIONING.md`.

## Reusable Lesson

When a tool reads a version file at runtime, the version file must be part of the release/update lifecycle.
