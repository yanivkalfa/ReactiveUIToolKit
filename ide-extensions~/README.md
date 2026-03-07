# UITKX IDE Extensions

Language tooling for **`.uitkx`** — the ReactiveUIToolKit component template format.  
Three IDE wrappers share a single LSP server and a single TextMate grammar.

```
ide-extensions~/
  grammar/
    uitkx.tmLanguage.json   ← shared TextMate grammar (all IDEs)
    uitkx-schema.json       ← element / attribute schema (LSP + grammar source)
  lsp-server/               ← shared C# LSP server (net8.0)
  vscode/                   ← VS Code extension   (TypeScript)
  visual-studio/            ← Visual Studio VSIX  (C# / net472)
  rider/                    ← Rider plugin         (Kotlin / Gradle)
```

## Features

| Feature | VS Code | Visual Studio | Rider |
|---------|:---:|:---:|:---:|
| Syntax highlighting (TextMate) | ✅ | ✅¹ | ✅¹ |
| Tag completions (`<button…`) | ✅ | ✅ | ✅ |
| Attribute completions | ✅ | ✅ | ✅ |
| Directive completions (`@namespace…`) | ✅ | ✅ | ✅ |
| Hover documentation | ✅ | ✅ | ✅ |

¹ via `.tmLanguage.json` bundled in the extension package

## How It Works

```
Editor (VS Code / Visual Studio / Rider)
       │  LSP JSON-RPC over stdin/stdout
       ▼
UitkxLanguageServer.dll   (dotnet run)
       │  loads
       ▼
uitkx-schema.json         (embedded resource)
```

The editor launches `dotnet UitkxLanguageServer.dll` as a child process.  
The server reads `uitkx-schema.json` (embedded at build time) to respond to  
`textDocument/completion` and `textDocument/hover` requests.

## Prerequisites

- **.NET 8 Runtime** on PATH (`dotnet` command available)  
- The IDE extension for your editor (see per-IDE README below)

## Building Locally

### LSP server
```bash
cd lsp-server
dotnet build -c Release
```

### VS Code extension
```bash
cd vscode
npm install
npm run build        # compiles TypeScript + copies grammar
npm run package      # creates uitkx-x.x.x.vsix
```

### Rider plugin
```bash
cd rider
./gradlew buildPlugin   # output in build/distributions/
```

### Visual Studio VSIX
Open `visual-studio/UitkxVsix/UitkxVsix.csproj` in Visual Studio 2022 and
press **Build → Build Solution**, or run:
```powershell
msbuild visual-studio/UitkxVsix/UitkxVsix.csproj /p:Configuration=Release
```

## Publishing

Push a tag matching `ide-v*` (e.g. `ide-v1.0.0`) to trigger all three
GitHub Actions publish workflows simultaneously:

| Workflow | Target |
|----------|--------|
| `publish-vscode.yml` | VS Code Marketplace + Open VSX |
| `publish-vsix.yml`   | Visual Studio Marketplace |
| `publish-rider.yml`  | JetBrains Marketplace |

### Required secrets

| Secret | Used by |
|--------|---------|
| `VSCE_PAT` | VS Code Marketplace publish token |
| `OVSX_TOKEN` | Open VSX publish token |
| `VS_MARKETPLACE_TOKEN` | Visual Studio Marketplace PAT |
| `JETBRAINS_MARKETPLACE_TOKEN` | JetBrains Marketplace token |
| `JETBRAINS_CERTIFICATE_CHAIN` | Signing certificate (PEM) |
| `JETBRAINS_PRIVATE_KEY` | Signing private key (PEM) |
| `JETBRAINS_PRIVATE_KEY_PASSWORD` | Key passphrase |

## Updating the Schema

If you add or change elements or props in ReactiveUIToolKit, regenerate
`grammar/uitkx-schema.json` by re-running the PowerShell extraction script
(if one is added) or by editing the JSON directly.

The schema is embedded in the LSP server at build time (`EmbeddedResource`),
so rebuild and redistribute the server after any schema change.
