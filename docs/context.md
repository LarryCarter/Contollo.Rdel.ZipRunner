# Contollo.Rdel.ZipRunner — AI Project Context

## AI Session Manager Update

The Visual Studio extension now begins adding an AI Session Manager.

The initial implementation adds copy-to-clipboard commands for:

```text
Initialize AI Session
Rehydrate AI Session
Continue AI Session
```

These commands create AI-ready prompts so external AI chats can understand RDEL format, project context, plugin limits, validation rules, and current task context.

## Session Strategy

### Initialize

Use once at the start of a new AI session when the model does not know RDEL or Neuro Commander Studio.

### Rehydrate

Use when switching AI providers or recovering after context loss.

### Continue

Use for normal daily work after the AI already understands the project.

## Current Implementation Limits

The first version only copies generated text to the clipboard.

Future versions should add preview/edit before copy, selectable context sources, output window capture, build/test output capture, context package export, and embedded chat injection.
