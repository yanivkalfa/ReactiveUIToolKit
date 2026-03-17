# UITKX Visual Studio Extension Installer
# Installs UitkxVsix.vsix to VS 2022 (legacy) and VS 2026 (manual extract)
# Usage: .\install.ps1

param(
    [string]$VsixPath = "$PSScriptRoot\UitkxVsix\UitkxVsix.vsix"
)

if (-not (Test-Path $VsixPath)) {
    Write-Error "VSIX not found at: $VsixPath"
    Write-Host "Run the build first:  cd UitkxVsix; msbuild UitkxVsix.csproj /p:Configuration=Release ..."
    exit 1
}

Write-Host "Installing UITKX from: $VsixPath"
Write-Host ""

# ── VS 2022 (legacy installer) ───────────────────────────────────────────────
$vsix2022 = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\VSIXInstaller.exe"
if (Test-Path $vsix2022) {
    Write-Host "[VS 2022] Installing via legacy VSIXInstaller..."
    $proc = Start-Process -FilePath $vsix2022 -ArgumentList "/quiet `"$VsixPath`"" -Wait -PassThru
    if ($proc.ExitCode -eq 0) {
        Write-Host "[VS 2022] Installed successfully." -ForegroundColor Green
    } else {
        Write-Host "[VS 2022] Install returned code $($proc.ExitCode) (may already be installed or VS was open)" -ForegroundColor Yellow
    }
} else {
    Write-Host "[VS 2022] Not found, skipping."
}

Write-Host ""

# ── VS 2026 (manual extract + system-level DLL path) ─────────────────────────
# VS 2026 changed extension architecture:
#   - Metadata/registration: %LOCALAPPDATA%\...\Extensions\<name>\  (manifest.json, catalog.json)
#   - Binaries (DLLs, assets): [VS installdir]\Common7\IDE\Extensions\<name>\  (system path, needs admin)
# The MEF scanner ONLY scans the system-level path.
$vs2026Root = Get-ChildItem "$env:LOCALAPPDATA\Microsoft\VisualStudio" -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "^18\." } | Select-Object -First 1

if ($vs2026Root) {
    $userExtDir  = Join-Path $vs2026Root.FullName "Extensions\UitkxVsix"
    $sysExtDir   = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\UitkxVsix"
    $devenv2026  = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\devenv.exe"

    Write-Host "[VS 2026] Installing..."
    Write-Host "  User metadata dir: $userExtDir"
    Write-Host "  System binary dir: $sysExtDir"

    # -- User-local metadata dir (no admin needed) ----------------------------
    if (Test-Path $userExtDir) { Remove-Item $userExtDir -Recurse -Force }
    New-Item -ItemType Directory -Path $userExtDir -Force | Out-Null

    Add-Type -Assembly System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($VsixPath, $userExtDir)

    # Patch manifest.json extensionDir to the SYSTEM path so VS MEF scanning works
    $manifestPath = Join-Path $userExtDir "manifest.json"
    if (Test-Path $manifestPath) {
        $mfRaw   = Get-Content $manifestPath -Raw
        $escaped = $sysExtDir.Replace('\', '\\')
        $mfFixed = $mfRaw -replace '"extensionDir":"[^"]*"', """extensionDir"":""$escaped"""
        $mfFixed | Set-Content $manifestPath -Encoding UTF8 -NoNewline
        Write-Host "[VS 2026] Patched manifest.json extensionDir to system path."
    }

    # -- System binary dir (admin needed) -------------------------------------
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")
    if (-not $isAdmin) {
        Write-Host "[VS 2026] Elevating to copy binaries to Program Files..." -ForegroundColor Yellow
        $copyScript = @"
if (Test-Path '$sysExtDir') { Remove-Item '$sysExtDir' -Recurse -Force }
Copy-Item '$userExtDir' '$sysExtDir' -Recurse -Force
Write-Host 'Copied to system extension path'
"@
        Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -Command `"$copyScript`"" -Verb RunAs -Wait
    } else {
        if (Test-Path $sysExtDir) { Remove-Item $sysExtDir -Recurse -Force }
        Copy-Item $userExtDir $sysExtDir -Recurse -Force
        Write-Host "[VS 2026] Copied to system extension path (running as admin)."
    }

    # -- Clear MEF cache and update configuration ------------------------------
    $mefCache = Join-Path $vs2026Root.FullName "ComponentModelCache"
    if (Test-Path $mefCache) {
        Remove-Item "$mefCache\*" -Force -ErrorAction SilentlyContinue
        Write-Host "[VS 2026] MEF cache cleared."
    }

    if (Test-Path $devenv2026) {
        Write-Host "[VS 2026] Updating extension configuration (processes pkgdef)..."
        $proc = Start-Process -FilePath $devenv2026 -ArgumentList "/updateconfiguration" -Wait -PassThru
        if ($proc.ExitCode -eq 0) {
            Write-Host "[VS 2026] Configuration updated successfully." -ForegroundColor Green
        } else {
            Write-Host "[VS 2026] Configuration update returned code $($proc.ExitCode)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "[VS 2026] Not found, skipping."
}

Write-Host ""
Write-Host "Done. Restart Visual Studio to activate the extension."
