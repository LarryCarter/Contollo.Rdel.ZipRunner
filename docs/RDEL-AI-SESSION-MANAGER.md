# RDEL AI Session Manager

## Purpose

AI Session Manager creates AI-ready prompts from installed RDEL documentation and live Visual Studio context.

It bridges today's copy/paste AI workflow and the future Neuro Commander Studio embedded chat window.

## Session Modes

### Initialize

Use when the AI session does not know RDEL, Neuro Commander Studio, or this repository.

### Rehydrate

Use when switching AI providers or recovering context.

### Continue

Use during normal development.

## Current Implementation

The first implementation adds menu commands under `Tools -> Contollo RDEL`:

- Copy Initialize AI Session
- Copy Rehydrate AI Session
- Copy Continue AI Session

Each command copies a generated prompt to the clipboard.

## Future Direction

Later versions should add preview window, token estimate, selectable context sources, output window capture, build/test output capture, export context package, and direct injection into embedded AI chat.
