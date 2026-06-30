# Engineering Note: About Menu Version Display

## Summary

Added an About RDEL menu item to expose version/status information inside Visual Studio.

## Problem

Version files existed in the repository, but the running extension did not expose that status.

## Fix

Add `AboutRdelCommand` and `AboutRdelDialog`, reading from `docs/VERSION.json`.

## Reusable Lesson

Machine-readable project status should have a user-facing display surface inside the tool.
