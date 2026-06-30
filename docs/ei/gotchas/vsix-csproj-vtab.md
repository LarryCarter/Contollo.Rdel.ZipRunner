# Known Gotcha: VSIX .csproj VSToolsPath Backslash-v Can Become XML Vertical Tab 0x0B

## Symptom

Visual Studio or MSBuild reports that the project file could not be loaded because hexadecimal value `0x0B` is an invalid character.

## Root Cause

The intended MSBuild path contained a backslash immediately before `v$(VisualStudioVersion)`.

During AI generation, copy/paste, escaping, or script generation, that sequence can become a literal vertical-tab control character.

## Fix

Use the safe forward-slash version:

```xml
$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)
```

## Prevention Rule

When generating `.csproj` XML for this VSIX project, preserve the forward-slash `VSToolsPath` form.

Do not emit a backslash immediately before `v$(VisualStudioVersion)`.

## Related Files

```text
Contollo.Rdel.ZipRunner/Contollo.Rdel.ZipRunner.csproj
```

## Reusable Across Projects

Yes.
