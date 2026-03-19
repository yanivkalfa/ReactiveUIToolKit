# UITKX Visual Studio Extension — Local Build
# Builds the LSP server + VSIX for local testing / debug.
#
# Usage:
#   .\build-local.ps1              # Build only
#   .\build-local.ps1 -Install     # Build + install to VS 2022 / VS 2026
#   .\build-local.ps1 -Debug       # Build + launch VS experimental instance
#
# Prerequisites:
#   - Visual Studio 2022 with "Visual Studio extension development" workload
#   - .NET 8+ SDK on PATH

param(
    [switch]$Install,
    [switch]$Debug,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot  = Resolve-Path "$PSScriptRoot\..\.."
$vsDir     = "$PSScriptRoot\UitkxVsix"
$serverOut = "$vsDir\server"
$lspProj   = "$repoRoot\ide-extensions~\lsp-server\UitkxLanguageServer.csproj"
$vsixProj  = "$vsDir\UitkxVsix.csproj"

# ── Locate VS 2022 ───────────────────────────────────────────────────────────
$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vsPath  = & $vsWhere -version "[17.0,18.0)" -property installationPath -latest 2>$null
if (-not $vsPath) {
    Write-Error "Visual Studio 2022 not found. Install it or adjust the version range."
}
$msbuild = "$vsPath\MSBuild\Current\Bin\MSBuild.exe"
$devenv  = "$vsPath\Common7\IDE\devenv.exe"
$devCmd  = "$vsPath\Common7\Tools\VsDevCmd.bat"

Write-Host "=== UITKX VS Extension — Local Build ===" -ForegroundColor Cyan
Write-Host "  Repo root:  $repoRoot"
Write-Host "  VS 2022:    $vsPath"
Write-Host "  Config:     $Configuration"
Write-Host ""

# ── Step 1: Publish LSP server ───────────────────────────────────────────────
Write-Host "[1/3] Publishing LSP server..." -ForegroundColor Yellow
dotnet publish $lspProj -c $Configuration --runtime win-x64 --self-contained false -o $serverOut
if ($LASTEXITCODE -ne 0) { Write-Error "LSP server publish failed." }
Write-Host "[1/3] LSP server published." -ForegroundColor Green
Write-Host ""

# ── Step 2: Build VSIX ───────────────────────────────────────────────────────
Write-Host "[2/3] Building VSIX..." -ForegroundColor Yellow

# Clean obj to force VSIX regeneration (incremental build sometimes skips VSIX creation)
$objDir = "$vsDir\obj\$Configuration"
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }

# Use Developer Command Prompt environment for full VS SDK target support
cmd /c "call `"$devCmd`" -no_logo && msbuild `"$vsixProj`" /p:Configuration=$Configuration /p:DeployExtension=false /t:Rebuild /v:minimal"
if ($LASTEXITCODE -ne 0) { Write-Error "VSIX build failed." }

$vsixPath = "$vsDir\UitkxVsix.vsix"
if (-not (Test-Path $vsixPath)) {
    Write-Error "VSIX not found at $vsixPath after build."
}

$size = [math]::Round((Get-Item $vsixPath).Length / 1024)
Write-Host "[2/3] VSIX built: $vsixPath (${size}KB)" -ForegroundColor Green
Write-Host ""

# ── Step 3: Install or Debug ─────────────────────────────────────────────────
if ($Install) {
    Write-Host "[3/3] Installing..." -ForegroundColor Yellow
    & "$PSScriptRoot\install.ps1" -VsixPath $vsixPath
} elseif ($Debug) {
    Write-Host "[3/3] Launching VS 2022 Experimental Instance..." -ForegroundColor Yellow
    Write-Host "       Open a .uitkx file to activate the extension."
    Write-Host "       Server logs: %TEMP%\uitkx-*.log"
    Start-Process $devenv -ArgumentList "/rootsuffix Exp"
} else {
    Write-Host "[3/3] Build complete. Next steps:" -ForegroundColor Yellow
    Write-Host "  Install:  .\build-local.ps1 -Install"
    Write-Host "  Debug:    .\build-local.ps1 -Debug"
    Write-Host "  Manual:   Open UitkxVsix.csproj in VS2022, press F5"
}

Write-Host ""
Write-Host "Done." -ForegroundColor Cyan
