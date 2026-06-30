# File Engineering History: Contollo.Rdel.ZipRunner.csproj

## Known Gotchas

### VSToolsPath vertical-tab corruption

Use:

```xml
$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)
```

Do not generate a backslash before `v$(VisualStudioVersion)`.
