<#
Build script for ReactiveUITK.SourceGenerator

Usage:
  .\scripts\build-generator.ps1           # Release build (default)
  .\scripts\build-generator.ps1 -Debug    # Debug build

What this does:
  1) Builds SourceGenerator~/ReactiveUITK.SourceGenerator.csproj.
  2) OutputPath from the csproj writes the DLL to Assets/ReactiveUIToolKit/Analyzers/.
  3) Unity picks up the updated analyzer on refresh/recompile.
#>
param(
    [switch] $Debug
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir    = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot     = Split-Path -Parent $scriptDir
$generatorDir = Join-Path $repoRoot "SourceGenerator~"
$analyzersDir = Join-Path $repoRoot "Analyzers"
$csproj       = Join-Path $generatorDir "ReactiveUITK.SourceGenerator.csproj"

if (-not (Test-Path $csproj)) {
    throw "Cannot find $csproj"
}

$configuration = if ($Debug) { "Debug" } else { "Release" }

Write-Host ""
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "  ReactiveUITK.SourceGenerator - building ($configuration)" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "  Project  : $csproj"
Write-Host "  Output   : $analyzersDir"
Write-Host ""

Push-Location $generatorDir
try {
    Write-Host "-- dotnet restore ------------------------------------------------" -ForegroundColor DarkCyan
    dotnet restore $csproj --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed (exit $LASTEXITCODE)" }

    Write-Host "-- dotnet build --------------------------------------------------" -ForegroundColor DarkCyan
    dotnet build $csproj --configuration $configuration --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }
}
finally {
    Pop-Location
}

$dll = Join-Path $analyzersDir "ReactiveUITK.SourceGenerator.dll"
if (Test-Path $dll) {
    $size = (Get-Item $dll).Length
    Write-Host ""
    Write-Host "Build succeeded" -ForegroundColor Green
    Write-Host "  DLL     : $dll"
    Write-Host "  Size    : $("{0:N0}" -f $size) bytes"
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. In Unity, run Assets -> Refresh or refocus the editor window."
    Write-Host "  2. Reproduce the UITKX compile error and click the red console line."
} else {
    throw "Build appeared to succeed but DLL not found at $dll"
}
