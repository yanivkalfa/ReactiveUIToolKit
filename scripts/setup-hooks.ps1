try {
  $repoRoot = (git rev-parse --show-toplevel) 2>$null
} catch {
  $repoRoot = $null
}
if (-not $repoRoot) {
  Write-Error "Not inside a git repository. Run this from within your repo root or any subfolder."
  exit 1
}

$hooksRel = "Assets/ReactiveUIToolKit/.githooks"
$hooksAbs = Join-Path $repoRoot $hooksRel
Write-Host "Configuring local git hooks path to $hooksRel ..."
git config core.hooksPath $hooksRel | Out-Null

$pkg = "Assets/ReactiveUIToolKit"
$src = Join-Path $pkg "Samples~"
$dst = Join-Path $pkg "Samples"
if (Test-Path $src) {
  Write-Host "Ensuring working copy has visible Samples folder ..."
  Move-Item -Force $src $dst
}

Write-Host "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."
Write-Host "Hooks directory: $hooksAbs"

