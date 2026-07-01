# RDEL Existing Project Intake

## Purpose

When RDEL is used on an existing project, the AI needs the lay of the land before producing packages.

The AI should not guess project structure from a vague description.

## Existing Project Intake Checklist

Before producing a package for an existing project, collect or generate:

```text
Project name
Repository root
Solution/project files
Directory tree
Important source files
Build system
Runtime/framework
Package/dependency files
Validation commands
Current errors/output
Existing docs
Git status
Known constraints
```

## Minimum Intake Bundle

At minimum, an AI should receive:

```text
README.md
docs/context.md if present
docs/VERSION.json if present
project/solution file
directory tree
current error/output text
active file or affected files
```

## Copilot / Single-File Limitation

Some AI tools are primitive and can only accept one file at a time or do not reliably read ZIP files.

For those tools, RDEL should support a single-file project intake format.

## SourcePack Lessons To Carry Forward

SourcePack packed a directory into a portable representation with:

- directory structure
- text file contents
- binary references
- versioning
- ignore rules
- list mode
- unpack/reconstruct behavior

RDEL should reuse these ideas for context intake, but aim it at AI context rather than deployment.

## Proposed Future Format

A future RDEL intake file may use one of these names:

```text
project.rdelctx
project.spack
project.rdelpack.md
```

Recommended contents:

```text
manifest
project summary
directory tree
important file contents
hashes
binary placeholders
ignore rules
build/test commands
known errors
recent package history
```

## Difference Between RDEL and SourcePack

```text
SourcePack = portable project/file state
RDEL = portable repository change state
RDEL Context Pack = portable AI understanding state
```

## AI Rule

If an existing project has no RDEL context yet, the AI should first request or generate a project intake bundle before producing a change package.

If the user provides a SourcePack/.spack-style file, the AI should use it as project context.
