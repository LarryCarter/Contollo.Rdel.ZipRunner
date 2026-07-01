# Engineering Note: Package Naming and Existing Project Intake

## Summary

RDEL needed stronger package filename rules and better existing-project intake guidance.

## Problem

Some AI tools generated packages without project names or version numbers. Existing-project work also requires a lay-of-the-land context pass before change generation.

## Fix

Add the package naming standard, update AI prompt providers, and document existing-project intake.

## SourcePack Lesson

SourcePack's single-file project representation is useful for AI tools that cannot read ZIPs well. RDEL should adopt a future `.rdelctx` or `.spack`-compatible intake format.
