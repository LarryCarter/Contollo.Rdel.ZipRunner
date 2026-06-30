# Engineering Note: Add Visible RDEL Versioning

## Summary

The project needed a visible way to know where it is after many RDEL packages.

## Problem

Package history existed in Git and cumulative docs, but there was no simple About or Version file that summarized the current baseline.

## Fix

Add:

- `docs/ABOUT-RDEL.md`
- `docs/VERSION.json`
- `docs/CHANGELOG.md`
- `docs/versioning/RDEL-VERSIONING.md`

## Reusable Lesson

AI-native tooling should expose its current state in a machine-readable version file and a human-readable About file.
