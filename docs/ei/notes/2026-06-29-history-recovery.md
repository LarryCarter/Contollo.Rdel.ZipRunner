# Engineering Note: Cumulative Documentation Recovery

## Summary

Earlier RDEL packages overwrote cumulative Markdown files because the runner only supported full-file updates.

## Problem

`docs/context.md`, `docs/memory.md`, and `docs/DECISIONS.md` lost earlier project history.

## Fix

Use DocOps append operations to recover the lost history without directly replacing the cumulative files.

## Reusable Lesson

Long-lived cumulative documentation should not be maintained through full-file AI rewrites.

Use append/merge operations owned by the runner.
