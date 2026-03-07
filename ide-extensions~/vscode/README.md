# UITKX — VS Code Extension

Adds language support for `.uitkx` ReactiveUIToolKit component templates.

## Features

- **Syntax highlighting** — directives, control flow, element tags, attributes, embedded C# expressions
- **Completions** — element tags, attribute names, `@` directives
- **Hover documentation** — element descriptions, attribute types and descriptions
- **Bracket matching and auto-close** for `{`, `(`, `"`, `<!--`
- **Folding** for element hierarchies

## Requirements

- **.NET 8+** runtime on PATH (`dotnet --version` must succeed)
- The bundled `server/UitkxLanguageServer.dll` is included in the extension

## Extension Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `uitkx.server.path` | `""` | Override the path to `UitkxLanguageServer.dll` |
| `uitkx.server.dotnetPath` | `"dotnet"` | Path to the `dotnet` executable |
| `uitkx.trace.server` | `"off"` | LSP trace level (`off` / `messages` / `verbose`) |

## Quick Start

1. Install the extension
2. Open any `.uitkx` file — syntax highlighting activates immediately
3. Type `<` to get element tag completions
4. Type `@` at the start of a line for directive completions
5. Hover any tag name for its props type and available attributes

## Known Limitations

- Diagnostics (red squiggles) are produced by the Roslyn source generator, not
  the LSP server. Open the Unity Editor to see generator diagnostics in the
  Unity console.
- The LSP server does not yet support Go-To-Definition or Find References.

## Development

```bash
npm install
npm run watch          # incremental TypeScript build
# Press F5 in VS Code to launch the Extension Development Host
```

## Release Flow (Store + Local)

Use this flow for every VS Code release so backend + extension stay in sync.

### 1) Update changelog (required)

Before version bump, add a new top section in `CHANGELOG.md` with:
- version number
- user-facing changes/fixes
- any migration or known limitations

### 2) Bump extension version

From `ide-extensions~/vscode`:

```powershell
npm.cmd version patch --no-git-tag-version
```

### 3) Rebuild bundled LSP server payload

From repo root:

```powershell
dotnet publish ide-extensions~/lsp-server/UitkxLanguageServer.csproj -c Release -o ide-extensions~/vscode/server
```

### 4) Build extension frontend

From `ide-extensions~/vscode`:

```powershell
npm.cmd run build
```

### 5) Package VSIX

```powershell
npm.cmd run package
```

Expected output: `uitkx-x.y.z.vsix`

### 6) Publish to VS Code Marketplace

```powershell
npm.cmd run publish
```

### 7) Install locally (required)

```powershell
code.cmd --install-extension uitkx-x.y.z.vsix --force
```

### 8) Verify installed version

```powershell
code.cmd --list-extensions --show-versions | Select-String "reactiveuitk.uitkx"
```

### 9) Smoke check

Validate at least:
- extension activates on `.uitkx`
- formatting works
- latest completion/hover/diagnostic fix is present

## CI Release Option

Tag push `ide-vX.Y.Z` triggers workflow publish in `.github/workflows/publish-vscode.yml`.
Even when CI is used, keep changelog and local smoke verification discipline.

## License

MIT
