# GitHub Pages AI Access

Enable GitHub Pages:

1. Settings → Pages
2. Branch: master
3. Folder: /(root)

Recommended public files:

- /robots.txt
- /context.md (after Metadata Export support)
- /docs/context.md (temporary)
- /docs/RDEL-AI-SPEC.md

Recommended robots.txt:

```
User-agent: *
Allow: /
```

Future manifest fields:

```json
{
  "AiContextUrl":"https://larrycarter.github.io/Contollo.Rdel.ZipRunner/context.md",
  "MetadataExport":{
    "PublishContext":true,
    "ContextTargetPath":"context.md"
  }
}
```
