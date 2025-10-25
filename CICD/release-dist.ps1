param(
  [string]$Branch = "dist",
  [string]$Remote = "origin",
  [string]$Tag,
  [switch]$Force
)

function Exec($cmd) {
  Write-Host "[exec] $cmd"
  & cmd /c $cmd
  if ($LASTEXITCODE -ne 0) { throw "Command failed: $cmd" }
}

try {
  $gitv = & git --version 2>$null
  if ($LASTEXITCODE -ne 0) { throw "git is required in PATH" }
} catch { throw $_ }

# Determine package root (folder that contains this CICD directory)
$pkgRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$distRoot = Join-Path $pkgRoot "dist~"
try {
  $repoRoot = (& git rev-parse --show-toplevel).Trim()
} catch { throw "Failed to determine git repo root. Ensure this script runs inside a git repo." }
if (!(Test-Path $distRoot)) { throw "dist~ folder not found at $distRoot. Run Build Dist first." }

# Determine tag from dist/package.json if not provided
if (-not $Tag) {
  $pkgPath = Join-Path $distRoot "package.json"
  if (Test-Path $pkgPath) {
    try {
      $pkg = Get-Content -Raw $pkgPath | ConvertFrom-Json
      if ($pkg.version) { $Tag = "v$($pkg.version)" }
    } catch {}
  }
}

# Prepare worktree dir
$worktree = Join-Path $repoRoot "_dist_branch"
if (Test-Path $worktree) {
  try { Exec "git worktree remove -f `"$worktree`"" } catch {}
  Start-Sleep -Milliseconds 200
  Remove-Item -Recurse -Force $worktree -ErrorAction SilentlyContinue
}

Exec "git worktree add -B $Branch `"$worktree`" $Branch"

# Clean worktree contents
Get-ChildItem -Path $worktree -Force | Where-Object { $_.Name -ne ".git" } | ForEach-Object { Remove-Item -Recurse -Force $_.FullName }

# Copy dist into worktree
robocopy "$distRoot" "$worktree" /E /NFL /NDL /NJH /NJS /NP | Out-Null

Push-Location $worktree
try {
  Exec "git add -A"
  $status = (& git status --porcelain)
  if ($status) {
    Exec "git commit -m `"dist update $Tag`""
  } else {
    Write-Host "No changes to commit on $Branch"
  }

  if ($Tag) {
    $tagExists = (& git tag -l $Tag)
    if ($tagExists -and -not $Force) {
      Write-Host "Tag $Tag already exists. Use -Force to overwrite. Skipping tag create."
    } else {
      if ($tagExists) { Exec "git tag -d $Tag" }
      Exec "git tag $Tag"
    }
  }

  Exec "git push $Remote $Branch"
  if ($Tag) { Exec "git push $Remote $Tag" }
}
finally {
  Pop-Location
  try { Exec "git worktree remove -f `"$worktree`"" } catch {}
}

Write-Host "Done: pushed $Branch" -ForegroundColor Green
