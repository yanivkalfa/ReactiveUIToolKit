# Release Operations Runbook

Step-by-step guide for publishing ReactiveUIToolKit releases.

---

## Prerequisites

- .NET 8+ SDK installed (`dotnet --version`)
- Node.js 20+ (`node --version`)
- Visual Studio 2022 with VSSDK workload (for VSIX builds)
- Azure DevOps PAT with Marketplace → Manage scope
- Accounts on: Unity Asset Store Publisher, Gumroad, Itch.io

---

## Automated (Tag-Triggered) — IDE Extensions

These happen automatically when you push a tag.

### VS Code + Visual Studio Extensions

```bash
# 1. Ensure all changes are merged to main
# 2. Bump version in ide-extensions~/vscode/package.json
# 3. Update CHANGELOG.md
# 4. Commit and push

git tag ide-v1.0.283
git push origin ide-v1.0.283
```

This triggers three GitHub Actions workflows:
- `publish-vscode.yml` → VS Code Marketplace + Open VSX
- `publish-vsix.yml` → Visual Studio Marketplace
- `publish-rider.yml` → JetBrains Marketplace (when ready)

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

The docs site is deployed automatically via GitHub Actions on merge to `main`
(the `deploy-docs.yml` workflow). To deploy manually:

```bash
cd ReactiveUIToolKitDocs~
npm ci
npm run build
# Deploy the dist/ folder to your hosting provider
```

---

## Pre-Release Checklist

Before tagging a release:

- [ ] All tests pass: `dotnet test SourceGenerator~/Tests/` (841+ tests)
- [ ] All LSP tests pass: `dotnet test ide-extensions~/lsp-server/Tests/` (22+ tests)
- [ ] VS Code extension builds: `cd ide-extensions~/vscode && npm ci && npm run build`
- [ ] Docs site builds: `cd ReactiveUIToolKitDocs~ && npm ci && npm run build`
- [ ] CHANGELOG.md updated with version and changes
- [ ] Version bumped in `package.json` (Unity), extension `package.json`, VSIX manifest
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
