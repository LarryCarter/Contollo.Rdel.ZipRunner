param(
    [Parameter(Mandatory=$true)]
    [string]$Title,

    [string]$OutputDirectory = "docs/ei/notes"
)

$slug = $Title.ToLowerInvariant()
$slug = $slug -replace '[^a-z0-9]+', '-'
$slug = $slug.Trim('-')

$date = Get-Date -Format "yyyy-MM-dd"
$fileName = "$date-$slug.md"

if (!(Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$path = Join-Path $OutputDirectory $fileName

$template = @"
# Engineering Memory: $Title

## Summary

TODO

## Problem

TODO

## Context

TODO

## What Failed

TODO

## Root Cause

TODO

## Final Decision / Fix

TODO

## Reusable Lesson

TODO

## Future Warning

TODO
"@

Set-Content -Path $path -Value $template -Encoding UTF8

Write-Host "Created engineering memory:"
Write-Host $path
