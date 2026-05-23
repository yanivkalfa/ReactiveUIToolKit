---
name: rebuild-ide-extensions
description: Rebuild the VS Code and/or Visual Studio 2022 extensions locally for F5 / Extension-Development-Host testing. Use when the user says "rebuild for F5", "rebuild the extension", "build the LSP server locally", "test the extension change" or after editing files under `ide-extensions~/language-lib/`, `ide-extensions~/lsp-server/`, `ide-extensions~/vscode/`, `ide-extensions~/visual-studio/`, or `Editor/HMR/`. Covers the full local pipeline — language-lib build, LSP server emit, TS client bundle, VSIX server-binary copy — and the artifact-size verification step. Does NOT cover Marketplace releases — those are handled by `.github/workflows/publish.yml`.
---

# Rebuild IDE extensions for F5

Repo root (Windows, PowerShell): `C:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit`

Use this skill when the user wants to test changes to the VS Code or
VS 2022 extension by launching an Extension Development Host (F5) or the
VS 2022 experimental hive.

## Out of scope — Marketplace / OpenVSX releases

Releasing to the VS Code Marketplace, OpenVSX, or the VS 2022 Marketplace
is handled by the CI pipeline at
[.github/workflows/publish.yml](../../workflows/publish.yml). Do **not**
run `vsce publish`, `ovsx publish`, or any equivalent command from a
developer machine. If the user asks to "ship a release" or "push to
marketplace", point them at the CI workflow instead — this skill stops
at the F5-ready local artifacts.

## Decide which pipelines to rebuild

Pick the smallest set that covers the changed files:

- Edited `ide-extensions~/language-lib/**/*.cs` or
  `ide-extensions~/lsp-server/**/*.cs` → **LSP server** must be rebuilt
  (and copied into VS 2022's `UitkxVsix/server/` if VS 2022 is in scope).
- Edited `ide-extensions~/vscode/src/**/*.ts` or
  `ide-extensions~/vscode/package.json` → **VS Code TS client** must be
  rebuilt.
- Edited `ide-extensions~/grammar/**` → both extensions repackage
  (grammar is bundled by each VSIX).
- Edited `Editor/HMR/**` only → **no IDE rebuild needed** (HMR runs
  inside Unity Editor at play time).

The two extensions share the LSP server but have separate client wrappers.

## VS Code rebuild (full F5-ready)

Run from repo root unless noted. PowerShell-friendly (use `cmd /c` for
the npm step to dodge execution-policy blocks on `npm.ps1`).

```powershell
# 1. Build + emit the LSP server into the VS Code extension's server/ dir.
#    `dotnet publish` is the .NET CLI command for emit-to-folder — it is
#    NOT marketplace publishing.
dotnet publish ide-extensions~/lsp-server/UitkxLanguageServer.csproj `
  -c Debug --self-contained false `
  -o ide-extensions~/vscode/server

# 2. Build the TS client bundle (esbuild → extension.js)
cmd /c "cd /d ide-extensions~\vscode && npm run build"
```

**Verify the artifacts** before launching F5:

```powershell
Get-ChildItem ide-extensions~/vscode/dist/extension.js,
              ide-extensions~/vscode/server/UitkxLanguageServer.dll,
              ide-extensions~/vscode/server/ReactiveUITK.Language.dll |
  Select-Object Name, @{n='KB';e={[int]($_.Length/1KB)}}, LastWriteTime
```

Expected sizes (rough sanity check, drift-tolerant):
- `extension.js` ~ 700-900 KB
- `UitkxLanguageServer.dll` ~ 250-300 KB
- `ReactiveUITK.Language.dll` ~ 200-250 KB

If `extension.js` is < 50 KB the bundle is broken (esbuild silently
emitted a stub) — check `npm run build` output.

Then in VS Code: open the `ide-extensions~/vscode` folder, **F5**.
Close any prior Extension Development Host first; the LSP DLL is
file-locked while attached.

## VS 2022 rebuild (full F5-ready / experimental hive)

VS 2022's `UitkxVsix` bundles a **static copy** of the LSP server in
`UitkxVsix/server/` and `UitkxVsix/server/win-x64/`. These are **not**
auto-synced from `lsp-server/bin/`; the VSIX project copies them at
its own build time only when its `BeforeBuild` target runs cleanly.
The reliable sequence is:

```powershell
# 1. Build the LSP server (Debug, framework-dependent)
dotnet build ide-extensions~/lsp-server/UitkxLanguageServer.csproj -c Debug

# 2. Mirror server binaries into the VSIX
$src = "ide-extensions~/lsp-server/bin/Debug/net8.0"
$dst = "ide-extensions~/visual-studio/UitkxVsix/server"
foreach ($d in $dst, "$dst/win-x64") {
  Copy-Item "$src/UitkxLanguageServer.dll"  "$d/" -Force
  Copy-Item "$src/UitkxLanguageServer.pdb"  "$d/" -Force
  Copy-Item "$src/ReactiveUITK.Language.dll" "$d/" -Force
  Copy-Item "$src/ReactiveUITK.Language.pdb" "$d/" -Force
}

# 3. Build the VSIX (uses the wrapper script)
Push-Location ide-extensions~/visual-studio
.\build-local.ps1
Pop-Location
```

The wrapper produces `UitkxVsix/bin/Debug/UitkxVsix.vsix`. Open the
`.sln` in VS 2022 and press **F5** to launch the experimental instance,
or double-click the `.vsix` to install into the main hive.

## Source generator rebuild (Unity-side)

Independent of either IDE. Outputs to `Analyzers/` so Unity picks it up
on the next domain reload:

```powershell
dotnet build SourceGenerator~/ReactiveUITK.SourceGenerator.csproj -c Release
```

If the user is testing both an SG change *and* an IDE change in the
same session, rebuild the SG **first** so the next IDE rebuild's
parity contract tests run against the new SG.

## Common pitfalls

- **PowerShell execution policy** blocks `npm.ps1`. Always wrap npm in
  `cmd /c "..."`.
- **DLL file lock.** If the emit or copy step fails with "file in
  use", close the running Extension Development Host (or the VS 2022
  experimental hive) first.
- **Stale TS bundle.** `npm run build` is incremental; if the output
  looks wrong, `Remove-Item ide-extensions~/vscode/dist -Recurse -Force`
  and re-run.
- **VS 2022 sees an old server.** Almost always the binary copy step
  was skipped — re-run step 2 of the VS 2022 sequence.
- **VS Code `uitkx.server.path` setting** can override the bundled
  `server/` directory for a developer-local LSP build. Check user
  settings if behaviour differs from a fresh F5.

## After rebuild

- Reload window in the Extension Development Host (or restart it) to
  pick up the new LSP DLL — VS Code does not hot-swap server processes.
- Open a `.uitkx` file from a real Unity project (e.g.
  `c:\Users\neta\Pretty Ui\Assets\UI\…`) to verify diagnostics,
  hover, completion, and formatting work end-to-end.
- The VS Code extension's "Output → UITKX Language Server" pane shows
  the server's stdout/stderr; any unhandled exception there means the
  rebuild produced a binary with a broken dependency graph (most often
  `Microsoft.CodeAnalysis.*` mismatch).
