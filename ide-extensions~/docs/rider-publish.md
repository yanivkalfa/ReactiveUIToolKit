# UITKX Rider Plugin — Build & Publish Flow

## Prerequisites

- **JDK 17** on PATH (`java -version` must show 17+)
  Download from https://adoptium.net if needed
- **Gradle** — no install needed, the wrapper (`gradlew`) handles it automatically
- A **JetBrains Marketplace** account at https://plugins.jetbrains.com
  (separate from the VS Code / Visual Studio Marketplace)

---

## Step 1 — Publish the LSP server into the plugin's server/ folder

```powershell
cd "ide-extensions~\lsp-server"
dotnet publish -c Release -o "../rider/server" --self-contained false
```

This copies `UitkxLanguageServer.dll` + all dependency DLLs into `rider/server/`
which Gradle bundles into the plugin ZIP via the `server/` content include.

---

## Step 2 — Build the plugin

```powershell
cd "ide-extensions~\rider"
.\gradlew buildPlugin
```

Output ZIP is at:
```
build\distributions\UITKX-1.0.0.zip
```

---

## Step 3 — Test locally before publishing

```powershell
.\gradlew runIde
```

This launches a sandboxed Rider instance with the plugin already installed.
Open a `.uitkx` file — syntax highlighting and LSP features should activate.

Or install the built ZIP manually:
- Rider → Settings → Plugins → ⚙ → **Install Plugin from Disk…**
- Select `build\distributions\UITKX-1.0.0.zip`
- Restart Rider

---

## Step 4 — Publish to JetBrains Marketplace

### Get a JetBrains Marketplace token
1. Go to https://plugins.jetbrains.com
2. Sign in → click your avatar → **My Tokens**
3. **New Token** → copy it

### Publish via Gradle

```powershell
cd "ide-extensions~\rider"
$env:PUBLISH_TOKEN = "YOUR_TOKEN_HERE"
.\gradlew publishPlugin
```

The `PUBLISH_TOKEN` environment variable is read by `build.gradle.kts`:
```kotlin
publishing {
    token = providers.environmentVariable("PUBLISH_TOKEN")
}
```

> First-time submissions go through JetBrains review (usually 1–2 business days).
> Updates to existing plugins are approved faster.

### Publish via web UI (alternative)
1. Go to https://plugins.jetbrains.com/plugin/add
2. Upload `build\distributions\UITKX-1.0.0.zip`

---

## Step 5 — Sign the plugin (required for Marketplace listing)

JetBrains requires plugins to be signed. Generate a certificate once:

```powershell
.\gradlew signPlugin
```

This reads three environment variables (set these before publishing):
```powershell
$env:CERTIFICATE_CHAIN   = Get-Content "chain.crt" -Raw
$env:PRIVATE_KEY         = Get-Content "private.pem" -Raw
$env:PRIVATE_KEY_PASSWORD = "your-key-password"
```

To generate a self-signed certificate:
```bash
# Run in bash/WSL
openssl genrsa -out private.pem 4096
openssl req -new -x509 -key private.pem -out chain.crt -days 3650
```

The full sign + publish flow:
```powershell
$env:CERTIFICATE_CHAIN    = Get-Content "chain.crt" -Raw
$env:PRIVATE_KEY          = Get-Content "private.pem" -Raw
$env:PRIVATE_KEY_PASSWORD = "your-key-password"
$env:PUBLISH_TOKEN        = "YOUR_TOKEN_HERE"

.\gradlew signPlugin publishPlugin
```

---

## Bump version for a new release

Edit `pluginVersion` in `gradle.properties`:
```properties
pluginVersion = 1.0.1
```

Then rebuild and republish.

---

## Plugin details

| Field | Value |
|-------|-------|
| Plugin ID | `com.reactiveuitk.uitkx` |
| Target | Rider 2024.1+ (build 241+) |
| Marketplace | https://plugins.jetbrains.com/plugin/com.reactiveuitk.uitkx |
| Manage | https://plugins.jetbrains.com/author/me |
