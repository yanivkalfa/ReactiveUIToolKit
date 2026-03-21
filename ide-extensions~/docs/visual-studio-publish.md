# UITKX Visual Studio Extension — Build & Publish Flow

## Prerequisites

- **Visual Studio 2022** with the **Visual Studio extension development** workload installed
  (Installer → Modify → Workloads → "Visual Studio extension development")
- **.NET SDK** (any recent version) for publishing the LSP server
- A **Visual Studio Marketplace** publisher account at https://marketplace.visualstudio.com/manage
  (same Microsoft account as your VS Code publisher if you want them grouped)

---

## Step 1 — Publish the LSP server into the VSIX's server/ folder

```powershell
cd "ide-extensions~\lsp-server"
dotnet publish -c Release -o "../visual-studio/UitkxVsix/server" --self-contained false
```

This copies `UitkxLanguageServer.dll` + all dependency DLLs into the `server\` folder
that the `.csproj` bundles into the VSIX via:
```xml
<Content Include="server\**\*.*">
  <IncludeInVSIX>true</IncludeInVSIX>
```

---

## Step 2 — Build the VSIX

```powershell
cd "ide-extensions~\visual-studio\UitkxVsix"
dotnet build -c Release
```

The output VSIX is produced at:
```
bin\Release\net472\UitkxVsix.vsix
```

---

## Step 3 — Test locally before publishing

Install directly into Visual Studio:

```powershell
# Double-click the .vsix, or use the command line:
start "" "bin\Release\net472\UitkxVsix.vsix"
```

This opens the VSIX Installer. Click **Install**. Restart Visual Studio.
Open a `.uitkx` file — syntax highlighting and LSP features should activate.

To uninstall: Visual Studio → Extensions → Manage Extensions → search UITKX → Uninstall.

---

## Step 4 — Publish to Marketplace

### Via web UI (simplest)
1. Go to https://marketplace.visualstudio.com/manage/publishers/ReactiveUITK
2. Click **+ New Extension** → **Visual Studio**
3. Drag and drop `bin\Release\net472\UitkxVsix.vsix` onto the upload area
4. Click **Upload** — extension is live within minutes

### Via command line (using `VsixPublisher`)
`VsixPublisher.exe` ships with Visual Studio. Find it at:
```
C:\Program Files\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe
```

```powershell
$vsixPublisher = "C:\Program Files\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"

& $vsixPublisher publish `
  -payload "bin\Release\net472\UitkxVsix.vsix" `
  -publishManifest "publishManifest.json" `
  -personalAccessToken "YOUR_PAT_HERE"
```

The `publishManifest.json` is a small file (see below).

#### publishManifest.json
```json
{
  "extensionId": "UitkxVsix.ReactiveUITK",
  "version": "1.0.0",
  "categories": [ "Coding" ],
  "overview": "README.md",
  "assetFiles": []
}
```

> **Note:** The same PAT used for VS Code (`Marketplace → Manage` scope) works here too.

---

## Bump version for a new release

Edit `Version` in `source.extension.vsixmanifest`:
```xml
<Identity Id="UitkxVsix.ReactiveUITK"
          Version="1.0.1"   ← bump this
          .../>
```

Then rebuild and republish.

---

## Extension details

| Field | Value |
|-------|-------|
| Extension ID | `UitkxVsix.ReactiveUITK` |
| Target | Visual Studio 2022+ (version 17.x) |
| Framework | net472 |
| Marketplace | https://marketplace.visualstudio.com/items?itemName=ReactiveUITK.UitkxVsix |
| Manage | https://marketplace.visualstudio.com/manage/publishers/ReactiveUITK |
