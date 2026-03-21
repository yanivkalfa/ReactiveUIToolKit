# UITKX VS Code Extension — Publish Flow

## Prerequisites (one-time setup)

1. Create a publisher at https://marketplace.visualstudio.com/manage
2. Create an Azure DevOps org at https://dev.azure.com → create a PAT with **Marketplace → Manage** scope
3. Login (one-time, token is cached locally):

```powershell
cd "ide-extensions~\vscode"
node_modules\.bin\vsce login ReactiveUITK
# paste PAT when prompted
```

---

## Every release — full rebuild + publish

```powershell
# 1. Rebuild the LSP server and copy it into the extension's server/ folder
cd "ide-extensions~\lsp-server"
dotnet publish -c Release -o "../vscode/server" --self-contained false

# 2. Rebuild the TypeScript extension
cd "..\vscode"
cmd /d /c "npm run build"

# 3. Publish to Marketplace (runs vscode:prepublish → build → upload)
cmd /d /c "node_modules\.bin\vsce publish --no-dependencies"
```

---

## Just package locally (no upload) — e.g. for testing

```powershell
cd "ide-extensions~\vscode"
cmd /d /c "node_modules\.bin\vsce package --no-dependencies"
# produces uitkx-X.Y.Z.vsix in the same folder
# install locally with:
code --install-extension uitkx-X.Y.Z.vsix
```

---

## Bump version before publishing a new release

Edit `"version"` in `ide-extensions~/vscode/package.json` — the Marketplace rejects
re-uploading the same version number. Then run the full rebuild + publish steps above.

---

## Command summary

| Command | What it does |
|---------|--------------|
| `dotnet publish` | Compiles the C# LSP server (`UitkxLanguageServer.dll`) and copies it to `vscode/server/` so it is bundled inside the extension |
| `npm run build` | Compiles TypeScript → `out/extension.js` and copies the TextMate grammar into `syntaxes/` |
| `vsce publish` | Packages everything into a `.vsix` and uploads to the Marketplace; the `vscode:prepublish` hook re-runs build automatically |
| `vsce login` | Only needed once per machine (or when the PAT expires); the token is stored in the system credential store |

---

## Publisher details

- **Publisher ID:** `ReactiveUITK`
- **Extension ID:** `ReactiveUITK.uitkx`
- **Marketplace URL:** https://marketplace.visualstudio.com/items?itemName=ReactiveUITK.uitkx
- **Manage URL:** https://marketplace.visualstudio.com/manage/publishers/ReactiveUITK
