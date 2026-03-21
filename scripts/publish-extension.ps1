<#
.SYNOPSIS
    Build, package, and publish the UITKX VS Code extension.

.DESCRIPTION
    Full publish pipeline for the UITKX VS Code extension:
      1. Prepend a changelog entry to CHANGELOG.md
      2. (Optional) Bump the patch version in the extension's package.json
      3. npm run build   -- bundles extension.ts via esbuild
      4. vsce package   -- produces uitkx-{version}.vsix
      5. vsce publish --pat <token>  -- pushes to VS Marketplace (skipped with -LocalOnly)

    The PAT is taken from the -PAT parameter, then from the VSCE_PAT environment
    variable, then from publisher-secrets.json at the repo root.

.PARAMETER PAT
    Azure DevOps Personal Access Token with Marketplace -> Manage scope.
    Falls back to VSCE_PAT env var and publisher-secrets.json if omitted.

.PARAMETER ChangelogEntry
    One-line summary of what changed. Prepended to CHANGELOG.md.
    If omitted you will be prompted interactively.

.PARAMETER LocalOnly
    Package only. Do not publish to the Marketplace.

.PARAMETER SkipBuild
    Skip the npm build step.

.PARAMETER SkipServerBuild
    Skip the dotnet publish step for the LSP server.

.PARAMETER BumpVersion
    Increment the patch version before building.

.EXAMPLE
    .\scripts\publish-extension.ps1 -PAT "G8j1..." -ChangelogEntry "Add Roslyn IntelliSense"

.EXAMPLE
    .\scripts\publish-extension.ps1 -LocalOnly
#>

[CmdletBinding()]
param(
    [string] $PAT            = '',
    [string] $ChangelogEntry = '',
    [switch] $LocalOnly,
    [switch] $SkipBuild,
    [switch] $SkipServerBuild,
    [switch] $BumpVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── paths ─────────────────────────────────────────────────────────────────────

$scriptDir    = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot     = Split-Path -Parent $scriptDir
$extensionDir = Join-Path $repoRoot 'ide-extensions~\vscode'
$pkgJsonPath  = Join-Path $extensionDir 'package.json'
$changelogPath= Join-Path $extensionDir 'CHANGELOG.md'
$secretsPath  = Join-Path $repoRoot 'publisher-secrets.json'

if (-not (Test-Path $extensionDir)) { Write-Error "Extension directory not found: $extensionDir"; exit 1 }

# ── helpers ───────────────────────────────────────────────────────────────────

function Write-Step([string] $msg) { Write-Host ''; Write-Host "-- $msg" -ForegroundColor Cyan }

function Get-PkgVersion([string] $path) { return (Get-Content $path -Raw | ConvertFrom-Json).version }

function Set-PkgPatchBump([string] $path) {
    $text    = Get-Content $path -Raw
    $current = ($text | ConvertFrom-Json).version
    $parts   = $current -split '\.'
    $next    = "$($parts[0]).$($parts[1]).$([int]$parts[2] + 1)"
    $text    = $text -replace '"version"\s*:\s*"[^"]+"', "`"version`": `"$next`""
    [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding $false))
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

function Invoke-Npm([string[]] $argList, [string] $cwd) {
    Write-Host "   > npm $($argList -join ' ')" -ForegroundColor DarkGray
    $prev = $PWD; Set-Location $cwd
    try {
        cmd.exe /c "npm $($argList -join ' ')"
        if ($LASTEXITCODE -ne 0) { throw "npm exited $LASTEXITCODE" }
    } finally { Set-Location $prev }
}

function Invoke-Vsce([string[]] $argList, [string] $cwd) {
    # Use the locally installed vsce binary directly so --pat is passed without env-var issues.
    $vsceBin = Join-Path $cwd 'node_modules\.bin\vsce.cmd'
    if (-not (Test-Path $vsceBin)) { $vsceBin = $null }
    Write-Host "   > vsce $($argList -join ' ')" -ForegroundColor DarkGray
    $prev = $PWD; Set-Location $cwd
    try {
        if ($vsceBin) {
            cmd.exe /c "`"$vsceBin`" $($argList -join ' ')"
        } else {
            cmd.exe /c "npx @vscode/vsce $($argList -join ' ')"
        }
        if ($LASTEXITCODE -ne 0) { throw "vsce exited $LASTEXITCODE" }
    } finally { Set-Location $prev }
}

# ── changelog ─────────────────────────────────────────────────────────────────

if ([string]::IsNullOrWhiteSpace($ChangelogEntry)) {
    Write-Host ''
    Write-Host 'Enter a changelog summary for this version:' -ForegroundColor Yellow
    $ChangelogEntry = Read-Host '  Entry'
    if ([string]::IsNullOrWhiteSpace($ChangelogEntry)) { $ChangelogEntry = 'Minor improvements and bug fixes.' }
}

# ── version bump ──────────────────────────────────────────────────────────────

$publishVersion = Get-PkgVersion $pkgJsonPath
if ($BumpVersion) {
    Write-Step 'Bumping patch version'
    $publishVersion = Set-PkgPatchBump $pkgJsonPath
    Write-Host "  -> v$publishVersion" -ForegroundColor Green
}

# ── prepend changelog entry ───────────────────────────────────────────────────

Write-Step "Updating CHANGELOG.md for v$publishVersion"
$today       = (Get-Date).ToString('yyyy-MM-dd')
$newEntry    = "## [$publishVersion] - $today`n- $ChangelogEntry`n"
$existing    = if (Test-Path $changelogPath) { Get-Content $changelogPath -Raw } else { "# Changelog`n" }
if ($existing -match '^# Changelog') {
    $nl      = $existing.IndexOf("`n") + 1
    $updated = $existing.Substring(0, $nl) + "`n" + $newEntry + $existing.Substring($nl)
} else {
    $updated = "# Changelog`n`n" + $newEntry + $existing
}
[System.IO.File]::WriteAllText($changelogPath, $updated, [System.Text.Encoding]::UTF8)
Write-Host "  Prepended entry for v$publishVersion." -ForegroundColor Green

# ── banner ────────────────────────────────────────────────────────────────────

Write-Host ''
Write-Host '======================================================' -ForegroundColor Yellow
Write-Host "  UITKX VS Code Extension   v$publishVersion"          -ForegroundColor Yellow
Write-Host "  LocalOnly  : $LocalOnly"                              -ForegroundColor Yellow
Write-Host "  SkipBuild  : $SkipBuild"                              -ForegroundColor Yellow
Write-Host "  SkipServer : $SkipServerBuild"                        -ForegroundColor Yellow
Write-Host '======================================================' -ForegroundColor Yellow

# ── 1. dotnet publish (LSP server) ────────────────────────────────────────────

$lspServerDir = Join-Path $PSScriptRoot '..\ide-extensions~\lsp-server'
$lspServerDir = (Resolve-Path $lspServerDir).Path

if (-not $SkipServerBuild) {
    Write-Step 'Publishing LSP server (dotnet publish)'
    Push-Location $lspServerDir
    try {
        $dotnetOut = & dotnet publish -c Release --self-contained false -o '../vscode/server' --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            $dotnetOut | ForEach-Object { Write-Host "  $_" }
            Write-Error 'dotnet publish failed.'; exit 1
        }
        Write-Host '  Server published successfully.' -ForegroundColor Green
    } finally {
        Pop-Location
    }
}

# ── 2. npm build ──────────────────────────────────────────────────────────────

if (-not $SkipBuild) {
    Write-Step 'Building extension (npm run build)'
    Invoke-Npm @('run', 'build') $extensionDir
    Write-Host '  Build succeeded.' -ForegroundColor Green
}

# ── 3. package ────────────────────────────────────────────────────────────────

Write-Step 'Packaging extension (vsce package)'
Invoke-Vsce @('package') $extensionDir

$vsixPath = Join-Path $extensionDir "uitkx-$publishVersion.vsix"
if (-not (Test-Path $vsixPath)) {
    $found = Get-ChildItem $extensionDir -Filter '*.vsix' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -ne $found) { $vsixPath = $found.FullName; Write-Host "  Using $($found.Name)" -ForegroundColor Yellow }
    else { Write-Error "VSIX not found."; exit 1 }
}
Write-Host "  Packaged: $vsixPath" -ForegroundColor Green

# ── 3. local install ─────────────────────────────────────────────────────────
# Install the VSIX via the VS Code CLI so it is immediately available on reload.

Write-Step 'Installing locally via VS Code CLI'
$codeCandidates = @(
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code\bin\code.cmd",
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code\bin\code",
    'code.cmd',
    'code'
)
$codeCli = $codeCandidates | Where-Object { (Get-Command $_ -ErrorAction SilentlyContinue) -or (Test-Path $_) } | Select-Object -First 1

if ($codeCli) {
    $installOut = & cmd.exe /c "`"$codeCli`" --install-extension `"$vsixPath`" --force" 2>&1
    $installOut | ForEach-Object { Write-Host "  $_" }
    Write-Host '  Reload VS Code window (Ctrl+Shift+P > Developer: Reload Window) to activate.' -ForegroundColor DarkGray
} else {
    Write-Host '  VS Code CLI not found — skipping local install. Install manually via Extensions > ... > Install from VSIX.' -ForegroundColor Yellow
}

# ── 4. publish ────────────────────────────────────────────────────────────────

if (-not $LocalOnly) {
    $resolvedPAT = Resolve-PAT
    if ([string]::IsNullOrWhiteSpace($resolvedPAT)) {
        Write-Host 'No PAT found. Cannot publish.' -ForegroundColor Red
        Write-Host 'Options: -PAT "<token>"   |   $env:VSCE_PAT   |   publisher-secrets.json { vscePatToken }'
        exit 1
    }

    Write-Step 'Publishing to VS Marketplace (vsce publish --pat ...)'
    Invoke-Vsce @('publish', '--pat', $resolvedPAT, '--packagePath', "`"$vsixPath`"") $extensionDir
    Write-Host "  Published v$publishVersion to VS Marketplace." -ForegroundColor Green
} else {
    Write-Host '  Marketplace publish skipped (-LocalOnly).' -ForegroundColor DarkGray
}

# ── done ──────────────────────────────────────────────────────────────────────

Write-Host ''
Write-Host '======================================================' -ForegroundColor Green
Write-Host "  Done  v$publishVersion" -ForegroundColor Green
Write-Host '======================================================' -ForegroundColor Green