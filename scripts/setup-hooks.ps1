try { $null = git rev-parse --show-toplevel 2>$null } catch { }
# Derive package root from this script's location (robust regardless of cwd)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$pkg = Split-Path -Parent $scriptDir
$hooksAbs = Join-Path $pkg ".githooks"
Write-Host "Configuring local git hooks path to $hooksAbs ..."
git config core.hooksPath $hooksAbs | Out-Null

$src = Join-Path $pkg "Samples~"
$dst = Join-Path $pkg "Samples"
if (Test-Path $src) {
  Write-Host "Ensuring working copy has visible Samples folder ..."
  Move-Item -Force $src $dst
}

Write-Host "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."
Write-Host "Hooks directory: $hooksAbs"

