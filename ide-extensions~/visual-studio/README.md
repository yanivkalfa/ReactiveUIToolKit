# UITKX — Visual Studio Extension (VSIX)

Adds language support for `.uitkx` ReactiveUIToolKit component templates in **Visual Studio 2022**.

## Features

- `.uitkx` file association (content type `uitkx`)
- Completions and hover documentation via the shared UITKX LSP server
- TextMate syntax highlighting (`uitkx.tmLanguage.json` bundled)

## Requirements

- **Visual Studio 2022** (17.0+)
- **.NET 8+** runtime on PATH

## Installation

1. Download `UitkxVsix.vsix` from the [Releases page](https://github.com/your-org/ReactiveUIToolKit/releases)
2. Double-click the `.vsix` file or run:
   ```powershell
   vsixinstaller.exe UitkxVsix.vsix
   ```
3. Restart Visual Studio

## Server Location

After installation the VSIX bundle contains `server/UitkxLanguageServer.dll`.  
Visual Studio automatically launches it when you open a `.uitkx` file.

## Development

Open `UitkxVsix.csproj` in Visual Studio 2022 with the **Visual Studio extension
development** workload installed, then press **F5** to launch an Experimental
instance with the extension loaded.

## Release Flow (Store + Local)

Use this flow for every Visual Studio store release.

### 1) Update changelog (required)

Before version bump, add/update release notes (recommended file: `visual-studio/CHANGELOG.md`) with:
- version number
- user-facing changes/fixes
- known limitations

### 2) Bump VSIX version

Edit `visual-studio/UitkxVsix/source.extension.vsixmanifest`:
- `Metadata -> Identity -> Version`

### 3) Rebuild bundled LSP server payload

From repo root:

```powershell
dotnet publish ide-extensions~/lsp-server/UitkxLanguageServer.csproj -c Release --runtime win-x64 --self-contained false -o ide-extensions~/visual-studio/UitkxVsix/server
```

### 4) Build VSIX

From `ide-extensions~/visual-studio/UitkxVsix`:

```powershell
dotnet restore
msbuild UitkxVsix.csproj /p:Configuration=Release /p:DeployExtension=false
```

Expected output: `UitkxVsix.vsix` (under build output folders).

### 5) Install locally (required)

Recommended:

```powershell
powershell -ExecutionPolicy Bypass -File ide-extensions~/visual-studio/install.ps1
```

Alternative:

```powershell
vsixinstaller.exe ide-extensions~/visual-studio/UitkxVsix/UitkxVsix.vsix
```

### 6) Verify locally

Open Visual Studio and validate:
- `.uitkx` files open with UITKX language service
- completions/hover work
- latest fix is observable

### 7) Publish to Visual Studio Marketplace

Automated route (recommended): push tag `ide-vX.Y.Z` (workflow `.github/workflows/publish-vsix.yml`).

Manual route (if needed): upload the built VSIX using marketplace publishing token/process (same endpoint/token model used by CI workflow).

### 8) Post-publish sanity

- Confirm listing version matches manifest version
- Keep VSIX artifact for rollback
- Record release notes/changelog link in release tracking

## License

MIT
