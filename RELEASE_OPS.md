# Release Operations Runbook

Step-by-step guide for publishing ReactiveUIToolKit releases.

---

## Version Files Reference

Each artifact versions independently. Bump only the ones you're releasing.

| Artifact | File | Property | Current |
|---|---|---|---|
| **Unity Package** | `package.json` | `version` | `0.2.23` |
| **VS Code Extension** | `ide-extensions~/vscode/package.json` | `version` | `1.0.282` |
| **VS 2022 Extension** | `ide-extensions~/visual-studio/UitkxVsix/source.extension.vsixmanifest` | `Identity Version` | `1.0.58` |
| **Rider Plugin** *(disabled)* | `ide-extensions~/rider/gradle.properties` | `pluginVersion` | `1.0.0` |
| **LSP Server** | `ide-extensions~/lsp-server/UitkxLanguageServer.csproj` | `<Version>` | `1.0.0` |
| **VS Code Changelog** | `ide-extensions~/vscode/CHANGELOG.md` | Entry header | — |

> **Note:** The `v*` tag (e.g. `v0.2.24`) must match the Unity `package.json` version.
> IDE extension versions are checked independently via their own git tags
> (`vscode-v{ver}`, `vs2022-v{ver}`). If a version was already published,
> that job is skipped automatically.

---

## Prerequisites

- .NET 8+ SDK installed (`dotnet --version`)
- Node.js 20+ (`node --version`)
- Visual Studio 2022 with VSSDK workload (for VSIX builds)
- Azure DevOps PAT with Marketplace → Manage scope
- Accounts on: Unity Asset Store Publisher, Gumroad, Itch.io

---

## Automated (CI) — `publish.yml`

A single workflow (`publish.yml`) runs all publish jobs. It triggers on:
- **`v*` tag push** — runs dist deploy + docs + all IDE extension jobs
- **`workflow_dispatch`** — runs docs + IDE extension jobs (no dist)

### Releasing a new version

```bash
# 1. Bump versions in the files listed above (only the ones that changed)
# 2. Update CHANGELOG.md entries as needed
# 3. Commit and push to main

# 4. Tag with the Unity package version (must match package.json)
git tag v0.2.24
git push origin v0.2.24
```

This triggers 4 parallel jobs:

| Job | What it does | Skip condition |
|---|---|---|
| **deploy-dist** | Builds source generator DLLs, packages dist, pushes to `dist` branch | Only runs on `v*` tags |
| **deploy-docs** | Builds docs site, pushes to `documentations` branch | Never skipped |
| **publish-vscode** | Builds LSP server + VS Code extension, publishes to marketplace | Skipped if `vscode-v{ver}` tag exists |
| **publish-vs2022** | Builds LSP server + VSIX, publishes via VsixPublisher.exe | Skipped if `vs2022-v{ver}` tag exists |

Each IDE extension job auto-tags on success (e.g. `vscode-v1.0.283`, `vs2022-v1.0.59`).

### Required GitHub Secrets

| Secret | Purpose |
|---|---|
| `VSCE_PAT` | Azure DevOps PAT for VS Code Marketplace publishing |
| `VS_MARKETPLACE_TOKEN` | Azure DevOps PAT for VS 2022 Marketplace publishing |

Monitor progress at: `https://github.com/<org>/ReactiveUIToolKit/actions`

---

## Manual — Unity Package

### Unity Asset Store

1. Open [Unity Publisher Dashboard](https://publisher.unity.com/)
2. Navigate to your package draft
3. Upload the latest `.unitypackage`:
   - In Unity Editor: right-click the `Assets/ReactiveUIToolKit` folder
   - Select **Export Package…**
   - Uncheck files listed in `config.json` → `pathsToOmitFromDist`
     (CICD, Diagnostics, scripts, docs, publisher-secrets, PDBs, deps.json)
   - Export as `ReactiveUIToolKit-v1.0.0.unitypackage`
4. Fill in / update:
   - Version number
   - Release notes (copy from CHANGELOG.md)
   - Screenshots (if changed)
   - Description and metadata
5. Submit for review
6. Unity review typically takes 5–10 business days

### Gumroad

Gumroad has a REST API that can be automated. For manual uploads:

1. Log in to [Gumroad Dashboard](https://app.gumroad.com/products)
2. Navigate to your product (or create one for the first release)
3. Click **Edit** → **Content** tab
4. Upload `ReactiveUIToolKit-v1.0.0.unitypackage`
5. Update the version in the product description
6. Update the price if needed
7. Save and publish

**API automation** (for future CI integration):
```bash
curl -X PUT https://api.gumroad.com/v2/products/{product_id} \
  -d "access_token=YOUR_TOKEN" \
  -F "file=@ReactiveUIToolKit-v1.0.0.unitypackage"
```

### Itch.io

Itch.io supports automation via the `butler` CLI tool.

**Manual upload:**
1. Log in to [Itch.io Dashboard](https://itch.io/dashboard)
2. Navigate to your project page
3. Upload `ReactiveUIToolKit-v1.0.0.unitypackage`
4. Set the download type and pricing
5. Publish

**Automated upload** (for future CI integration):
```bash
# Install butler: https://itch.io/docs/butler/
butler push ReactiveUIToolKit-v1.0.0.unitypackage your-account/reactiveuitoolkit:unity --userversion 1.0.0
```

---

## Manual — Documentation Site

The docs site is deployed automatically by `publish.yml` (pushes to the
`documentations` branch on every publish run). To deploy manually:

```bash
cd ReactiveUIToolKitDocs~
npm ci
npm run build
# The dist/ folder is then pushed to the documentations branch
```

---

## Pre-Release Checklist

Before tagging a release:

- [ ] All tests pass: `dotnet test SourceGenerator~/Tests/` (841+ tests)
- [ ] All LSP tests pass: `dotnet test ide-extensions~/lsp-server/Tests/` (22+ tests)
- [ ] VS Code extension builds: `cd ide-extensions~/vscode && npm ci && npm run build`
- [ ] Docs site builds: `cd ReactiveUIToolKitDocs~ && npm ci && npm run build`
- [ ] CHANGELOG.md updated with version and changes
- [ ] Versions bumped in relevant files (see **Version Files Reference** table above)
- [ ] No uncommitted changes: `git status` is clean
- [ ] Branch is up to date with `main`

---

## Post-Release Verification

After publishing:

- [ ] VS Code Marketplace listing shows new version
- [ ] `ext install ReactiveUITK.uitkx` installs successfully
- [ ] Open VSX listing shows new version (if published)
- [ ] Visual Studio Marketplace listing shows new version (check manually)
- [ ] Unity Asset Store package is submitted (review pending)
- [ ] Gumroad product page shows updated file
- [ ] Itch.io project page shows updated file
- [ ] Documentation site is live and current
- [ ] GitHub Release created with artifacts attached
