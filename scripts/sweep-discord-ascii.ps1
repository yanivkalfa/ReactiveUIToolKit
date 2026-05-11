$path = "Plans~\DISCORD_CHANGELOG.md"
$t = [System.IO.File]::ReadAllText($path)

$emDash = [string][char]0x2014
$rArrow = [string][char]0x2192
$biArrow = [string][char]0x2194
$middot = [string][char]0x00B7
$fffd = [string][char]0xFFFD

$t = $t.Replace($emDash, '-')
$t = $t.Replace($rArrow, '->')
$t = $t.Replace($biArrow, '<->')
$t = $t.Replace($middot, '|')

$t = [regex]::Replace($t, "(\d)\s+$fffd\s+(\d)", '$1 -> $2')
$t = [regex]::Replace($t, "\*\*\s+$fffd\s+", '** | ')
$t = $t.Replace($fffd, '-')

[System.IO.File]::WriteAllText($path, $t, [System.Text.UTF8Encoding]::new($false))

$check = [System.IO.File]::ReadAllText($path)
$remaining = ($check.ToCharArray() | Where-Object { [int]$_ -gt 127 }).Count
Write-Host "Remaining non-ASCII chars: $remaining"

$headers = [regex]::Matches($check, '## \[\d+\.\d+\.\d+\]') | ForEach-Object { @{ Version=$_.Value; Start=$_.Index } }
for ($i=0; $i -lt $headers.Count; $i++) {
  $start = $headers[$i].Start
  $end = if ($i+1 -lt $headers.Count) { $headers[$i+1].Start } else { $check.Length }
  $body = $check.Substring($start, $end - $start) -replace "(?s)\r?\n---\s*\r?\n\s*$", ""
  $ok = if ($body.Length -le 2000) { "OK" } else { "OVER" }
  Write-Host "$($headers[$i].Version)  chars=$($body.Length)  $ok"
}
