# automation~/

Tooling for **AI-assisted and automated workflows** in the ReactiveUIToolKit project.

The `~` suffix means Unity ignores this folder (same convention as `ide-extensions~`,
`SourceGenerator~`, `Plans~`). Contents are version-controlled.

## Contents

| Path | Purpose |
|------|---------|
| `unity-api-diff.ps1` | Compare UIElements API between two Unity versions via assembly reflection |
| `diff-reports/` | Output folder for diff reports (gitignored JSON files) |

## Quick Start

### Run an API diff between Unity versions

```powershell
# Compare 6.2 → 6.3 (auto-discovers Unity Hub installs)
.\automation~\unity-api-diff.ps1 -From 6000.2 -To 6000.3

# Specify explicit DLL paths
.\automation~\unity-api-diff.ps1 `
  -FromDll "C:\Program Files\Unity\Hub\Editor\6000.2.0f1\Editor\Data\Managed\UnityEngine\UnityEngine.UIElementsModule.dll" `
  -ToDll   "C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data\Managed\UnityEngine\UnityEngine.UIElementsModule.dll"

# Save output to diff-reports/
.\automation~\unity-api-diff.ps1 -From 6000.2 -To 6000.3 -OutFile .\automation~\diff-reports\6000.2-to-6000.3.json
```

### Use the AI prompt to add version support

In VS Code Copilot Chat, type `/` and select **add-unity-version**, or:

```
/add-unity-version Unity 6.5 (6000.5) has been released
```

The prompt will guide the AI through discovery, diff, classification, and implementation.

## Related Files

| File | Description |
|------|-------------|
| `.github/prompts/add-unity-version.prompt.md` | Copilot prompt — AI runbook for adding version support |
| `AUTOMATION.md` (project root) | Human-readable overview |
| `Plans~/VERSIONING_PROCESS.md` | Full reference: discovery sources, checklists, version matrix |
