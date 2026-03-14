<#
.SYNOPSIS
    Build, package, and publish the UITKX Visual Studio extension.

.DESCRIPTION
    Full publish pipeline for the UITKX VS2022+ extension:
      1. (Optional) Bump the patch version in source.extension.vsixmanifest
      2. Prepend a changelog entry to overview.md
      3. dotnet publish  -- builds the LSP server into UitkxVsix/server/
      4. MSBuild Build   -- compiles UitkxVsix.dll (net472)
      5. MSBuild CreateVsixContainer -- packages UitkxVsix.vsix
      6. VSIXInstaller   -- installs locally to VS2022 (skipped with -SkipLocalInstall)
      7. VsixPublisher   -- publishes to VS Marketplace (skipped with -LocalOnly)

    The PAT is taken from the -PAT parameter, then from the VSCE_PAT environment
    variable, then from publisher-secrets.json at the repo root.

    IMPORTANT BUILD NOTES (hard-won lessons):
      - The VSIX project uses Microsoft.VSSDK.BuildTools NuGet package.
      - After a clean, you must run Build and CreateVsixContainer as SEPARATE
        MSBuild invocations. CreateVsixContainer is not available as a target
        in the same invocation that does Restore (the VSSDK targets import
        depends on NuGet-generated props from obj/).
      - The MSBuild from VS2022 (v17.x) must be used, NOT a preview VS (v18.x)
        which silently skips the VSIX container creation.
      - VsixPublisher.exe reports "Uploaded" even when the VSIX embedded version
        doesn't match what you expect. Always verify the VSIX contents before
        publishing.

.PARAMETER PAT
    Azure DevOps Personal Access Token with Marketplace -> Manage scope.
    Falls back to VSCE_PAT env var and publisher-secrets.json if omitted.

.PARAMETER ChangelogEntry
    One-line summary of what changed. Appended to overview.md changelog.
    If omitted you will be prompted interactively.

.PARAMETER LocalOnly
    Build and install locally. Do not publish to the Marketplace.

.PARAMETER SkipBuild
    Skip MSBuild compilation (assumes DLL is already built).

.PARAMETER SkipServerBuild
    Skip the dotnet publish step for the LSP server.

.PARAMETER SkipLocalInstall
    Skip local VSIXInstaller step.

.PARAMETER BumpVersion
    Increment the patch version before building.

.EXAMPLE
    .\scripts\publish-vsix.ps1 -BumpVersion -ChangelogEntry "Add new feature X"

.EXAMPLE
    .\scripts\publish-vsix.ps1 -LocalOnly

.EXAMPLE
    .\scripts\publish-vsix.ps1 -BumpVersion -ChangelogEntry "Fix Y" -SkipServerBuild
#>

[CmdletBinding()]
param(
    [string] $PAT              = '',
    [string] $ChangelogEntry   = '',
    [switch] $LocalOnly,
    [switch] $SkipBuild,
    [switch] $SkipServerBuild,
    [switch] $SkipLocalInstall,
    [switch] $BumpVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── paths ─────────────────────────────────────────────────────────────────────

$scriptDir     = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot      = Split-Path -Parent $scriptDir
$vsixProjDir   = Join-Path $repoRoot 'ide-extensions~\visual-studio\UitkxVsix'
$lspServerDir  = Join-Path $repoRoot 'ide-extensions~\lsp-server'
$manifestPath  = Join-Path $vsixProjDir 'source.extension.vsixmanifest'
$overviewPath  = Join-Path $vsixProjDir 'overview.md'
$publishManifest = Join-Path $vsixProjDir 'publishManifest.json'
$secretsPath   = Join-Path $repoRoot 'publisher-secrets.json'

if (-not (Test-Path $vsixProjDir)) { Write-Error "VSIX project directory not found: $vsixProjDir"; exit 1 }

# ── find tools ────────────────────────────────────────────────────────────────

# MSBuild — must use VS2022 (v17.x). VS2026/preview (v18.x) silently skips VSIX creation.
$msbuildCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)
$msbuild = $msbuildCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $msbuild) { Write-Error "VS2022 MSBuild.exe not found. Install Visual Studio 2022 with the VS extension development workload."; exit 1 }

# VsixPublisher
$vsixPubCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"
)
$vsixPublisher = $vsixPubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

# VSIXInstaller (for local install)
$vsixInstallerCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\VSIXInstaller.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\VSIXInstaller.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\VSIXInstaller.exe"
)
$vsixInstaller = $vsixInstallerCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

# ── helpers ───────────────────────────────────────────────────────────────────

function Write-Step([string] $msg) { Write-Host ''; Write-Host "-- $msg" -ForegroundColor Cyan }

function Get-ManifestVersion([string] $path) {
    $xml = [xml](Get-Content $path -Raw)
    $ns  = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $ns.AddNamespace('v', 'http://schemas.microsoft.com/developer/vsx-schema/2011')
    $identity = $xml.SelectSingleNode('//v:Identity', $ns)
    return $identity.GetAttribute('Version')
}

function Set-ManifestPatchBump([string] $path) {
    $text    = Get-Content $path -Raw
    $current = Get-ManifestVersion $path
    $parts   = $current -split '\.'
    $next    = "$($parts[0]).$($parts[1]).$([int]$parts[2] + 1)"
    $text    = $text -replace "Version=`"$([regex]::Escape($current))`"", "Version=`"$next`""
    [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding $true))
    return $next
}

function Resolve-PAT {
    if (-not [string]::IsNullOrWhiteSpace($PAT)) { return $PAT }
    if (-not [string]::IsNullOrWhiteSpace($env:VSCE_PAT)) { return $env:VSCE_PAT }
    if (Test-Path $secretsPath) {
        try {
            $token = (Get-Content $secretsPath -Raw | ConvertFrom-Json).vscePatToken
            if (-not [string]::IsNullOrWhiteSpace($token)) { return $token }
        } catch { }
    }
    return ''
}

function Verify-VsixVersion([string] $vsixPath, [string] $expectedVersion) {
    $zipPath = $vsixPath -replace '\.vsix$', '_verify.zip'
    $tmpDir  = Join-Path (Split-Path $vsixPath) '_vsix_verify'
    try {
        Copy-Item $vsixPath $zipPath -Force
        if (Test-Path $tmpDir) { Remove-Item $tmpDir -Recurse -Force }
        Expand-Archive $zipPath $tmpDir -Force
        $embeddedManifest = Join-Path $tmpDir 'extension.vsixmanifest'
        $xml = [xml](Get-Content $embeddedManifest -Raw)
        $ns  = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
        $ns.AddNamespace('v', 'http://schemas.microsoft.com/developer/vsx-schema/2011')
        $identity = $xml.SelectSingleNode('//v:Identity', $ns)
        $embedded = $identity.GetAttribute('Version')
        if ($embedded -ne $expectedVersion) {
            Write-Error "VSIX version mismatch! Expected v$expectedVersion but VSIX contains v$embedded. Did the build use cached obj/? Try deleting obj/ and rebuilding."
            exit 1
        }
        Write-Host "  Verified: VSIX contains v$embedded" -ForegroundColor Green
    } finally {
        Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
        Remove-Item $tmpDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# ── changelog ─────────────────────────────────────────────────────────────────

if ([string]::IsNullOrWhiteSpace($ChangelogEntry)) {
    Write-Host ''
    Write-Host 'Enter a changelog summary for this version:' -ForegroundColor Yellow
    $ChangelogEntry = Read-Host '  Entry'
    if ([string]::IsNullOrWhiteSpace($ChangelogEntry)) { $ChangelogEntry = 'Minor improvements and bug fixes.' }
}

# ── version bump ──────────────────────────────────────────────────────────────

$publishVersion = Get-ManifestVersion $manifestPath
if ($BumpVersion) {
    Write-Step 'Bumping patch version in source.extension.vsixmanifest'
    $publishVersion = Set-ManifestPatchBump $manifestPath
    Write-Host "  -> v$publishVersion" -ForegroundColor Green
}

# ── prepend changelog entry to overview.md ────────────────────────────────────

Write-Step "Updating overview.md changelog for v$publishVersion"
$today    = (Get-Date).ToString('yyyy-MM-dd')
$newEntry = "- $ChangelogEntry"
$overview = Get-Content $overviewPath -Raw

if ($overview -match "### \[$([regex]::Escape($publishVersion))\]") {
    # Version section already exists — append the entry
    $overview = $overview -replace "(### \[$([regex]::Escape($publishVersion))\][^\n]*\n)", "`$1$newEntry`n"
    Write-Host "  Appended entry to existing v$publishVersion section." -ForegroundColor Green
} else {
    # Insert new version section after "## Changelog"
    $changelogHeader = "## Changelog"
    $section = "`n### [$publishVersion] - $today`n$newEntry`n"
    $idx = $overview.IndexOf($changelogHeader)
    if ($idx -ge 0) {
        $afterHeader = $idx + $changelogHeader.Length
        $overview = $overview.Substring(0, $afterHeader) + "`n" + $section + $overview.Substring($afterHeader)
    } else {
        $overview += "`n`n## Changelog`n" + $section
    }
    Write-Host "  Added new section for v$publishVersion." -ForegroundColor Green
}
[System.IO.File]::WriteAllText($overviewPath, $overview, [System.Text.Encoding]::UTF8)

# ── banner ────────────────────────────────────────────────────────────────────

Write-Host ''
Write-Host '======================================================' -ForegroundColor Yellow
Write-Host "  UITKX Visual Studio Extension   v$publishVersion"     -ForegroundColor Yellow
Write-Host "  LocalOnly       : $LocalOnly"                         -ForegroundColor Yellow
Write-Host "  SkipBuild       : $SkipBuild"                         -ForegroundColor Yellow
Write-Host "  SkipServerBuild : $SkipServerBuild"                   -ForegroundColor Yellow
Write-Host "  SkipLocalInstall: $SkipLocalInstall"                  -ForegroundColor Yellow
Write-Host '======================================================' -ForegroundColor Yellow

# ── 1. dotnet publish (LSP server) ────────────────────────────────────────────

if (-not $SkipServerBuild) {
    Write-Step 'Publishing LSP server (dotnet publish)'
    $serverOutDir = Join-Path $vsixProjDir 'server'
    Push-Location $lspServerDir
    try {
        $dotnetOut = & dotnet publish -c Release --self-contained false -o $serverOutDir --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            $dotnetOut | ForEach-Object { Write-Host "  $_" }
            Write-Error 'dotnet publish failed.'; exit 1
        }
        Write-Host '  Server published to UitkxVsix/server/' -ForegroundColor Green
    } finally { Pop-Location }
}

# ── 2. MSBuild: clean obj to avoid stale intermediate manifests ───────────────

if (-not $SkipBuild) {
    Write-Step 'Cleaning intermediate outputs'
    $objDir = Join-Path $vsixProjDir 'obj'
    if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }
    Write-Host '  Removed obj/' -ForegroundColor Green

    # Restore NuGet (generates obj/UitkxVsix.csproj.nuget.g.props needed for VSSDK import)
    Write-Step 'Restoring NuGet packages'
    & $msbuild (Join-Path $vsixProjDir 'UitkxVsix.csproj') /t:Restore /v:quiet 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error 'NuGet restore failed.'; exit 1 }
    Write-Host '  Restored.' -ForegroundColor Green

    # Build (compiles UitkxVsix.dll)
    Write-Step 'Building UitkxVsix.dll (MSBuild)'
    $buildOut = & $msbuild (Join-Path $vsixProjDir 'UitkxVsix.csproj') /p:Configuration=Release /p:DeployExtension=false /v:minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        $buildOut | ForEach-Object { Write-Host "  $_" }
        Write-Error 'MSBuild Build failed.'; exit 1
    }
    Write-Host '  Build succeeded.' -ForegroundColor Green
}

# ── 3. Create VSIX container (separate invocation — required!) ────────────────

Write-Step 'Creating VSIX container'
$csprojPath = Join-Path $vsixProjDir 'UitkxVsix.csproj'
$containerOut = & $msbuild $csprojPath /p:Configuration=Release /p:DeployExtension=false /t:CreateVsixContainer /v:minimal 2>&1
if ($LASTEXITCODE -ne 0) {
    $containerOut | ForEach-Object { Write-Host "  $_" }
    Write-Error 'CreateVsixContainer failed.'; exit 1
}

$vsixPath = Join-Path $vsixProjDir 'UitkxVsix.vsix'
if (-not (Test-Path $vsixPath)) { Write-Error "VSIX not found at $vsixPath after build."; exit 1 }

$vsixSize = [math]::Round((Get-Item $vsixPath).Length / 1MB, 1)
Write-Host "  Packaged: $vsixPath ($vsixSize MB)" -ForegroundColor Green

# ── 4. Verify VSIX version matches ───────────────────────────────────────────

Write-Step 'Verifying VSIX embedded version'
Verify-VsixVersion $vsixPath $publishVersion

# ── 5. Local install ─────────────────────────────────────────────────────────

if (-not $SkipLocalInstall) {
    Write-Step 'Installing locally via VSIXInstaller'
    if ($vsixInstaller) {
        $proc = Start-Process -FilePath $vsixInstaller -ArgumentList "/quiet `"$vsixPath`"" -Wait -PassThru
        if ($proc.ExitCode -eq 0) {
            Write-Host '  Installed to VS2022. Restart Visual Studio to activate.' -ForegroundColor Green
        } elseif ($proc.ExitCode -eq 1001) {
            Write-Host '  Extension already installed at this version.' -ForegroundColor Yellow
        } else {
            Write-Host "  VSIXInstaller returned code $($proc.ExitCode) (may need to close VS first)" -ForegroundColor Yellow
        }
    } else {
        Write-Host '  VSIXInstaller not found — install manually via Extensions > Manage Extensions.' -ForegroundColor Yellow
    }
}

# ── 6. Publish to Marketplace ────────────────────────────────────────────────

if (-not $LocalOnly) {
    $resolvedPAT = Resolve-PAT
    if ([string]::IsNullOrWhiteSpace($resolvedPAT)) {
        Write-Host 'No PAT found. Cannot publish.' -ForegroundColor Red
        Write-Host 'Options: -PAT "<token>"   |   $env:VSCE_PAT   |   publisher-secrets.json { vscePatToken }'
        exit 1
    }
    if (-not $vsixPublisher) {
        Write-Error "VsixPublisher.exe not found. Install the VS SDK workload in Visual Studio 2022."
        exit 1
    }

    Write-Step 'Publishing to VS Marketplace (VsixPublisher)'
    $pubOut = Join-Path $vsixProjDir 'pub-stdout.txt'
    $pubErr = Join-Path $vsixProjDir 'pub-stderr.txt'
    try {
        $pubArgs = "publish -payload `"$vsixPath`" -publishManifest `"$publishManifest`" -personalAccessToken $resolvedPAT"
        $proc = Start-Process -FilePath $vsixPublisher -ArgumentList $pubArgs -NoNewWindow -Wait -PassThru -RedirectStandardOutput $pubOut -RedirectStandardError $pubErr
        $stdout = if (Test-Path $pubOut) { Get-Content $pubOut -Raw } else { '' }
        $stderr = if (Test-Path $pubErr) { Get-Content $pubErr -Raw } else { '' }

        if ($proc.ExitCode -ne 0 -or $stderr -match 'error') {
            Write-Host "  STDOUT: $stdout" -ForegroundColor Yellow
            Write-Host "  STDERR: $stderr" -ForegroundColor Red
            Write-Error "VsixPublisher failed with exit code $($proc.ExitCode)."
            exit 1
        }
        Write-Host "  $($stdout.Trim())" -ForegroundColor Green
    } finally {
        Remove-Item $pubOut -Force -ErrorAction SilentlyContinue
        Remove-Item $pubErr -Force -ErrorAction SilentlyContinue
    }
} else {
    Write-Host '  Marketplace publish skipped (-LocalOnly).' -ForegroundColor DarkGray
}

# ── done ──────────────────────────────────────────────────────────────────────

Write-Host ''
Write-Host '======================================================' -ForegroundColor Green
Write-Host "  Done  v$publishVersion" -ForegroundColor Green
Write-Host '======================================================' -ForegroundColor Green
