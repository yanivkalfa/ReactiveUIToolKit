# Automation & AI Tooling

This project includes automation tooling for maintaining Unity version compatibility.

## When a New Unity Version is Released

### For AI (Copilot Chat)

In VS Code, open Copilot Chat and type:

```
/add-unity-version Unity 6.5 (6000.5) has been released
```

This runs a structured prompt that walks through discovery, classification, and implementation.
The prompt is at `.github/prompts/add-unity-version.prompt.md`.

### For Humans

1. **Run the diff script** to see what changed in UI Toolkit:

   ```powershell
   .\automation~\unity-api-diff.ps1 -From 6000.4 -To 6000.5 -OutFile .\automation~\diff-reports\6000.4-to-6000.5.json
   ```

   Both Unity versions must be installed via Unity Hub. The script uses assembly
   reflection — no web scraping, no fragile HTML parsing. 100% accurate.

2. **Review the JSON report** — it lists every added/removed/changed IStyle property,
   VisualElement subclass, enum, and struct.

3. **Follow the implementation checklists** in `Plans~/VERSIONING_PROCESS.md` §4.

## Folder Layout

| Path | What | Audience |
|------|------|----------|
| `.github/prompts/add-unity-version.prompt.md` | Copilot prompt — full runbook | AI |
| `automation~/unity-api-diff.ps1` | Assembly diff script (PowerShell) | Both |
| `automation~/diff-reports/` | Generated JSON reports (gitignored) | Both |
| `automation~/README.md` | Detailed usage docs | Both |
| `Plans~/VERSIONING_PROCESS.md` | Full reference: sources, checklists, matrix | Human |
