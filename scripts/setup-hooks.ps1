Write-Host "Configuring local git hooks path to .githooks ..."
git config core.hooksPath .githooks | Out-Null

$pkg = "Assets/ReactiveUIToolKit"
$src = Join-Path $pkg "Samples~"
$dst = Join-Path $pkg "Samples"
if (Test-Path $src) {
  Write-Host "Ensuring working copy has visible Samples folder ..."
  Move-Item -Force $src $dst
}

Write-Host "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."

