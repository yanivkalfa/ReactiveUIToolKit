<#
    Build script for ReactiveUITK.SourceGenerator
    ─────────────────────────────────────────────
    Run this from any directory; it resolves paths relative to its own location.

    Usage:
        .\scripts\build-generator.ps1               # Release build (default)
        .\scripts\build-generator.ps1 -Debug        # Debug build

    What this does:
      1. Builds SourceGenerator~/ReactiveUITK.SourceGenerator.csproj in Release
         (or Debug) configuration.
      2. The OutputPath in the .csproj places the DLL directly into
         Assets/ReactiveUIToolKit/Analyzers/ — no manual copy step needed.
      3. Unity detects the new/changed DLL on the next asset refresh (focus
         the Unity window, or click Assets → Refresh) and recompiles all
         assemblies with the updated generator.

    AFTER FIRST BUILD:
      Commit Analyzers/ReactiveUITK.SourceGenerator.dll and its .meta file
      so team members get the generator without having to build it themselves.

    REMINDER:
      The .meta file for the DLL must exist at
        Assets/ReactiveUIToolKit/Analyzers/ReactiveUITK.SourceGenerator.dll.meta
      with the 'RoslynAnalyzer' label. The pre-created .meta file already has
      this label. If Unity ever regenerates it and loses the label, re-add:
          labels:
          - RoslynAnalyzer
#>
param(
    [switch] $Debug
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir     = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot      = Split-Path -Parent $scriptDir
$generatorDir  = Join-Path $repoRoot "SourceGenerator~"
$analyzersDir  = Join-Path $repoRoot "Analyzers"
$csproj        = Join-Path $generatorDir "ReactiveUITK.SourceGenerator.csproj"

if (-not (Test-Path $csproj)) {
    Write-Error "Cannot find $csproj — is the SourceGenerator~ folder present?"
}

$configuration = if ($Debug) { "Debug" } else { "Release" }

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ReactiveUITK.SourceGenerator — building ($configuration)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Project  : $csproj"
Write-Host "  Output   : $analyzersDir"
Write-Host ""

Push-Location $generatorDir
try {
    # Restore NuGet packages (netstandard2.0 + Microsoft.CodeAnalysis.CSharp 4.3.1)
    Write-Host "── dotnet restore ──────────────────────────────────────────────" -ForegroundColor DarkCyan
    dotnet restore $csproj --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed (exit $LASTEXITCODE)" }

    # Build
    Write-Host "── dotnet build ────────────────────────────────────────────────" -ForegroundColor DarkCyan
    dotnet build $csproj `
        --configuration $configuration `
        --no-restore `
        --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }
}
finally {
    Pop-Location
}

$dll = Join-Path $analyzersDir "ReactiveUITK.SourceGenerator.dll"
if (Test-Path $dll) {
    $size = (Get-Item $dll).Length
    Write-Host ""
    Write-Host "✔ Build succeeded" -ForegroundColor Green
    Write-Host "  DLL     : $dll"
    Write-Host "  Size    : $("{0:N0}" -f $size) bytes"
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Focus the Unity Editor window (Assets → Refresh) to trigger recompilation."
    Write-Host "  2. Check the Console for UITKX0000 info messages confirming .uitkx files"
    Write-Host "     are received by the generator."
    Write-Host "  3. Commit Analyzers/ReactiveUITK.SourceGenerator.dll + its .meta file."
} else {
    Write-Error "Build appeared to succeed but DLL not found at $dll"
}
