# RDEL Package Checklist

## Required

- [ ] Package has a single clean root folder.
- [ ] Root contains `contollo-rdel.json`.
- [ ] Root contains `README.md`.
- [ ] `Target` is `solution` or `selected-project`.
- [ ] `Commands` is present and appropriate.
- [ ] No blocked paths are included.
- [ ] No secrets or production configs are included.
- [ ] File paths are repository-relative.
- [ ] Package README explains purpose, files, validation, risks, and rollback.

## If Architecture / Context Changes

- [ ] Update `docs/context.md`.
- [ ] Update `docs/DECISIONS.md`.
- [ ] Update `docs/memory.md` when stable memory changed.

## If Documentation Only

- [ ] Use `ValidationProfile: GitOnly`.
- [ ] Use `Commands: ["git status --porcelain"]`.

## If VSIX Code Changes

- [ ] Do not use plain `dotnet build`.
- [ ] State: build from Visual Studio Build menu.
- [ ] Preserve safe forward-slash VSToolsPath.
