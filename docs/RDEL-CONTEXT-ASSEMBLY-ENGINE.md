# RDEL Context Assembly Engine

The Context Assembly Engine replaces hardcoded prompt generation with providers, templates, and renderers.

Flow:

Template -> Providers -> ContextAssemblyEngine -> Renderer -> Clipboard / Chat / Context Package

Current providers: Protocol, AIContract, Project, ProjectDocs, Memory, Decisions, ActiveDocument, GitStatus, OutputPlaceholder, UserRequest.

Current templates: Initialize, Rehydrate, Continue.

Current renderer: MarkdownContextRenderer.
