<#
.SYNOPSIS
    Compares Unity UIElements API between two Unity versions using assembly reflection.

.DESCRIPTION
    Loads UnityEngine.UIElementsModule.dll from two Unity installations and diffs
    the public API surface relevant to ReactiveUIToolKit:
      - IStyle properties (name + type)
      - VisualElement subclasses
      - Enums in UnityEngine.UIElements namespace
      - Structs in UnityEngine.UIElements namespace (with constructors, methods, properties)

    Output is structured JSON suitable for both human review and AI consumption.

.PARAMETER From
    The "old" Unity version string, e.g. "6000.2" or "6000.2.0f1".
    The script searches Unity Hub for an installed version matching this prefix.

.PARAMETER To
    The "new" Unity version string, e.g. "6000.3" or "6000.3.0f1".

.PARAMETER FromDll
    Explicit path to the old UnityEngine.UIElementsModule.dll. Overrides -From.

.PARAMETER ToDll
    Explicit path to the new UnityEngine.UIElementsModule.dll. Overrides -To.

.PARAMETER OutFile
    Optional path to write the JSON report. If omitted, writes to stdout.

.PARAMETER IncludeUnchanged
    If set, includes unchanged items in the report (verbose mode).

.EXAMPLE
    .\unity-api-diff.ps1 -From 6000.2 -To 6000.3
    .\unity-api-diff.ps1 -From 6000.2 -To 6000.3 -OutFile .\diff-reports\6000.2-to-6000.3.json
    .\unity-api-diff.ps1 -FromDll "C:\...\6000.2.0f1\...\dll" -ToDll "C:\...\6000.3.0f1\...\dll"
#>

[CmdletBinding()]
param(
    [string]$From,
    [string]$To,
    [string]$FromDll,
    [string]$ToDll,
    [string]$OutFile,
    [switch]$IncludeUnchanged
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Unity Hub discovery ──────────────────────────────────────────────────────

function Find-UnityInstall([string]$Version) {
    # Normalize: "6000.3" → search for "6000.3*"
    $searchPattern = if ($Version -match '^\d+\.\d+$') { "$Version*" } else { $Version }

    $hubPaths = @(
        "C:\Program Files\Unity\Hub\Editor"
        "C:\Program Files\Unity"
        "C:\Program Files (x86)\Unity"
    )

    if ($env:OS -ne "Windows_NT") {
        # macOS / Linux fallback (PowerShell 7+ / pwsh)
        if (Test-Path "/Applications/Unity") {
            $hubPaths = @(
                "/Applications/Unity/Hub/Editor"
                "/Applications/Unity"
            )
        }
        else {
            $hubPaths = @(
                "/opt/unity/hub/editor"
                "$($env:HOME)/Unity/Hub/Editor"
            )
        }
    }

    foreach ($hubPath in $hubPaths) {
        if (-not (Test-Path $hubPath)) { continue }
        $matches = Get-ChildItem -Path $hubPath -Directory -Filter $searchPattern |
                   Sort-Object Name -Descending |
                   Select-Object -First 1
        if ($matches) {
            return $matches.FullName
        }
    }
    return $null
}

function Find-UIElementsDll([string]$UnityInstall) {
    $managedDir = Join-Path (Join-Path (Join-Path (Join-Path $UnityInstall "Editor") "Data") "Managed") "UnityEngine"
    $dllPath = Join-Path $managedDir "UnityEngine.UIElementsModule.dll"
    if (Test-Path $dllPath) { return $dllPath }

    # Fallback: search recursively
    $managedRoot = Join-Path (Join-Path (Join-Path $UnityInstall "Editor") "Data") "Managed"
    $found = Get-ChildItem -Path $managedRoot -Filter "UnityEngine.UIElementsModule.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { return $found.FullName }

    return $null
}

function Resolve-Dll([string]$Version, [string]$ExplicitDll, [string]$Label) {
    if ($ExplicitDll) {
        if (-not (Test-Path $ExplicitDll)) {
            throw "Explicit DLL path not found: $ExplicitDll"
        }
        return $ExplicitDll
    }

    if (-not $Version) {
        throw "Either -$Label or -${Label}Dll must be specified."
    }

    $install = Find-UnityInstall $Version
    if (-not $install) {
        throw "Could not find Unity $Version installation. Checked Unity Hub paths. Use -${Label}Dll for an explicit path."
    }

    $dll = Find-UIElementsDll $install
    if (-not $dll) {
        throw "Found Unity install at '$install' but could not locate UnityEngine.UIElementsModule.dll."
    }

    Write-Host "  $Label : $dll" -ForegroundColor DarkGray
    return $dll
}

# ── Assembly introspection ───────────────────────────────────────────────────
#
# Each DLL is loaded in a SEPARATE PowerShell child process (via a temp .ps1
# file) to avoid assembly identity conflicts. Uses Unity.Cecil.dll (Mono.Cecil)
# from the Unity installation for reliable metadata reading — this avoids the
# type-resolution failures that ReflectionOnlyLoadFrom has with .NET Standard 2.1
# assemblies on .NET Framework / PS 5.1.

function Get-ApiSurface([string]$DllPath) {
    $tempFile = [System.IO.Path]::GetTempFileName() -replace '\.tmp$', '.ps1'
    try {
        @'
param([string]$DllPath)
$ErrorActionPreference = 'Stop'

# Locate Unity.Cecil.dll relative to the UIElements DLL:
#   DLL:   .../Editor/Data/Managed/UnityEngine/UnityEngine.UIElementsModule.dll
#   Cecil: .../Editor/Data/Managed/Unity.Cecil.dll
$managedRoot = Split-Path (Split-Path $DllPath)
$cecilPath = Join-Path $managedRoot "Unity.Cecil.dll"
if (-not (Test-Path $cecilPath)) {
    throw "Unity.Cecil.dll not found at $cecilPath"
}
Add-Type -Path $cecilPath

$reader = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($DllPath)
$module = $reader.MainModule
$allTypes = @($module.Types | Where-Object {
    $_.IsPublic -and $_.Namespace -and $_.Namespace.StartsWith("UnityEngine.UIElements")
})
# Include nested public types
$nested = @($allTypes | ForEach-Object {
    $_.NestedTypes | Where-Object { $_.IsNestedPublic }
})
$allTypes = @($allTypes) + @($nested)

# Helper: format a Cecil TypeReference as a friendly name
function Format-TypeName($typeRef) {
    if ($null -eq $typeRef) { return "void" }
    $name = $typeRef.Name
    if ($typeRef.IsGenericInstance) {
        $git = [Mono.Cecil.GenericInstanceType]$typeRef
        $baseName = ($name -replace '`\d+$', '')
        $args = ($git.GenericArguments | ForEach-Object { Format-TypeName $_ }) -join ", "
        return "${baseName}<${args}>"
    }
    if ($name -match '`\d+$') {
        $baseName = ($name -replace '`\d+$', '')
        $gp = $typeRef.GenericParameters
        if ($gp -and $gp.Count -gt 0) {
            $args = ($gp | ForEach-Object { $_.Name }) -join ", "
            return "${baseName}<${args}>"
        }
        return $baseName
    }
    return $name
}

# IStyle properties
$istyle = $allTypes | Where-Object { $_.Name -eq "IStyle" -and $_.IsInterface } | Select-Object -First 1
$istyleProps = @{}
if ($istyle) {
    foreach ($prop in $istyle.Properties) {
        $istyleProps[$prop.Name] = Format-TypeName $prop.PropertyType
    }
}

# VisualElement subclasses: walk base types via Cecil's Resolve()
function Test-IsVE($typeDef) {
    $current = $typeDef
    while ($current.BaseType) {
        if ($current.BaseType.Name -eq "VisualElement") { return $true }
        try { $current = $current.BaseType.Resolve() } catch { return $false }
        if ($null -eq $current) { return $false }
    }
    return $false
}
$elements = @($allTypes | Where-Object {
    $_.IsClass -and -not $_.IsAbstract -and (Test-IsVE $_)
} | ForEach-Object { $_.Name } | Sort-Object)

# Enums
$enums = @{}
$allTypes | Where-Object { $_.IsEnum } | ForEach-Object {
    $eName = $_.Name
    $members = @($_.Fields | Where-Object { $_.IsStatic -and $_.IsPublic } |
        ForEach-Object { $_.Name } | Sort-Object)
    $enums[$eName] = $members
}

# Structs — name + public API surface (constructors, methods, properties)
$structs = [ordered]@{}
$allTypes | Where-Object {
    $_.IsValueType -and -not $_.IsEnum -and -not $_.Name.StartsWith("<")
} | Sort-Object Name | ForEach-Object {
    $sName = $_.Name

    $ctors = @($_.Methods | Where-Object {
        $_.IsConstructor -and $_.IsPublic -and $_.HasParameters
    } | ForEach-Object {
        $params = ($_.Parameters | ForEach-Object { "$(Format-TypeName $_.ParameterType) $($_.Name)" }) -join ", "
        "ctor($params)"
    })

    $methods = @($_.Methods | Where-Object {
        $_.IsPublic -and -not $_.IsConstructor -and
        -not $_.IsGetter -and -not $_.IsSetter -and
        -not $_.Name.StartsWith("op_") -and
        -not ($_.Name -in @("Equals","GetHashCode","ToString","GetType"))
    } | ForEach-Object {
        $retType = Format-TypeName $_.ReturnType
        $params = ($_.Parameters | ForEach-Object { "$(Format-TypeName $_.ParameterType) $($_.Name)" }) -join ", "
        if ($_.IsStatic) { "static $retType $($_.Name)($params)" } else { "$retType $($_.Name)($params)" }
    })

    $props = @($_.Properties | Where-Object {
        ($_.GetMethod -and $_.GetMethod.IsPublic) -or ($_.SetMethod -and $_.SetMethod.IsPublic)
    } | ForEach-Object {
        $pType = Format-TypeName $_.PropertyType
        $acc = ""
        if ($_.GetMethod -and $_.GetMethod.IsPublic) { $acc += "get; " }
        if ($_.SetMethod -and $_.SetMethod.IsPublic) { $acc += "set; " }
        "$pType $($_.Name) { $($acc.TrimEnd()) }"
    })

    $structs[$sName] = [ordered]@{
        constructors = $ctors
        methods      = $methods
        properties   = $props
    }
}

$reader.Dispose()

@{
    IStyleProperties = $istyleProps
    Elements         = $elements
    Enums            = $enums
    Structs          = $structs
} | ConvertTo-Json -Depth 6 -Compress
'@ | Set-Content -Path $tempFile -Encoding UTF8

        $result = powershell -ExecutionPolicy Bypass -NoProfile -File $tempFile -DllPath $DllPath 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Child process failed for '$DllPath': $result"
        }
        $jsonLine = ($result | Where-Object { $_ -is [string] -and $_.Trim().StartsWith('{') }) | Select-Object -Last 1
        if (-not $jsonLine) {
            throw "No JSON output from child process for '$DllPath'. Output: $result"
        }
        $parsed = $jsonLine | ConvertFrom-Json

        # ConvertFrom-Json returns PSCustomObject; convert to Hashtable for diff engine.
        $istyleHt = @{}
        if ($parsed.IStyleProperties) {
            $parsed.IStyleProperties.PSObject.Properties | ForEach-Object { $istyleHt[$_.Name] = $_.Value }
        }
        $enumsHt = @{}
        if ($parsed.Enums) {
            $parsed.Enums.PSObject.Properties | ForEach-Object { $enumsHt[$_.Name] = @($_.Value) }
        }
        $structsHt = @{}
        if ($parsed.Structs) {
            $parsed.Structs.PSObject.Properties | ForEach-Object {
                $structsHt[$_.Name] = @{
                    constructors = @($_.Value.constructors)
                    methods      = @($_.Value.methods)
                    properties   = @($_.Value.properties)
                }
            }
        }

        return @{
            IStyleProperties = $istyleHt
            Elements         = @($parsed.Elements)
            Enums            = $enumsHt
            Structs          = $structsHt
        }
    }
    finally {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }
}

# ── Diff engine ──────────────────────────────────────────────────────────────

function Compare-Dictionaries($Old, $New, $Label) {
    $added = @()
    $removed = @()
    $changed = @()
    $unchanged = @()

    foreach ($key in $New.Keys | Sort-Object) {
        if (-not $Old.ContainsKey($key)) {
            $added += @{ name = $key; type = $New[$key] }
        }
        elseif ($Old[$key] -ne $New[$key]) {
            $changed += @{ name = $key; oldType = $Old[$key]; newType = $New[$key] }
        }
        else {
            $unchanged += $key
        }
    }

    foreach ($key in $Old.Keys | Sort-Object) {
        if (-not $New.ContainsKey($key)) {
            $removed += @{ name = $key; type = $Old[$key] }
        }
    }

    $result = [ordered]@{
        added   = $added
        removed = $removed
        changed = $changed
    }
    if ($IncludeUnchanged) {
        $result.unchanged = $unchanged
    }
    return $result
}

function Compare-Lists($Old, $New) {
    $oldSet = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($item in $Old) { [void]$oldSet.Add($item) }
    $newSet = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($item in $New) { [void]$newSet.Add($item) }

    $added = $New | Where-Object { -not $oldSet.Contains($_) } | Sort-Object
    $removed = $Old | Where-Object { -not $newSet.Contains($_) } | Sort-Object

    $result = [ordered]@{
        added   = @($added)
        removed = @($removed)
    }
    if ($IncludeUnchanged) {
        $result.unchanged = @($Old | Where-Object { $newSet.Contains($_) } | Sort-Object)
    }
    return $result
}

function Compare-Enums($OldEnums, $NewEnums) {
    $newEnumNames = Compare-Lists ([string[]]($OldEnums.Keys | Sort-Object)) ([string[]]($NewEnums.Keys | Sort-Object))

    $memberChanges = [ordered]@{}
    foreach ($enumName in $NewEnums.Keys | Sort-Object) {
        if ($OldEnums.ContainsKey($enumName)) {
            $diff = Compare-Lists $OldEnums[$enumName] $NewEnums[$enumName]
            if ($diff.added.Count -gt 0 -or $diff.removed.Count -gt 0) {
                $memberChanges[$enumName] = $diff
            }
        }
    }

    return [ordered]@{
        types   = $newEnumNames
        members = $memberChanges
    }
}

function Compare-Structs($OldStructs, $NewStructs) {
    $oldNames = [string[]]($OldStructs.Keys | Sort-Object)
    $newNames = [string[]]($NewStructs.Keys | Sort-Object)
    $namesDiff = Compare-Lists $oldNames $newNames

    # For new structs, include full API surface for review
    $newStructDetails = [ordered]@{}
    foreach ($name in $namesDiff.added) {
        $newStructDetails[$name] = $NewStructs[$name]
    }

    # For existing structs, check if API changed
    $memberChanges = [ordered]@{}
    foreach ($name in $NewStructs.Keys | Sort-Object) {
        if (-not $OldStructs.ContainsKey($name)) { continue }
        $old = $OldStructs[$name]
        $new = $NewStructs[$name]

        $ctorDiff = Compare-Lists $old.constructors $new.constructors
        $methodDiff = Compare-Lists $old.methods $new.methods
        $propDiff = Compare-Lists $old.properties $new.properties

        $hasChanges = ($ctorDiff.added.Count + $ctorDiff.removed.Count +
                       $methodDiff.added.Count + $methodDiff.removed.Count +
                       $propDiff.added.Count + $propDiff.removed.Count) -gt 0
        if ($hasChanges) {
            $memberChanges[$name] = [ordered]@{
                constructors = $ctorDiff
                methods      = $methodDiff
                properties   = $propDiff
            }
        }
    }

    return [ordered]@{
        types          = $namesDiff
        newStructApi   = $newStructDetails
        memberChanges  = $memberChanges
    }
}

# ── Main ─────────────────────────────────────────────────────────────────────

Write-Host "`nUnity UIElements API Diff" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

Write-Host "`nResolving assemblies..." -ForegroundColor Yellow
$fromDllPath = Resolve-Dll -Version $From -ExplicitDll $FromDll -Label "From"
$toDllPath   = Resolve-Dll -Version $To   -ExplicitDll $ToDll   -Label "To"

Write-Host "`nLoading API surface (old)..." -ForegroundColor Yellow
$oldApi = Get-ApiSurface $fromDllPath

Write-Host "Loading API surface (new)..." -ForegroundColor Yellow
$newApi = Get-ApiSurface $toDllPath

Write-Host "`nComputing diff..." -ForegroundColor Yellow

$report = [ordered]@{
    meta = [ordered]@{
        from       = if ($From) { $From } else { $fromDllPath }
        to         = if ($To)   { $To }   else { $toDllPath }
        fromDll    = $fromDllPath
        toDll      = $toDllPath
        generatedAt = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        tool       = "automation~/unity-api-diff.ps1"
    }
    istyle   = Compare-Dictionaries $oldApi.IStyleProperties $newApi.IStyleProperties "IStyle"
    elements = Compare-Lists $oldApi.Elements $newApi.Elements
    enums    = Compare-Enums $oldApi.Enums $newApi.Enums
    structs  = Compare-Structs $oldApi.Structs $newApi.Structs
}

# ── Summary ──────────────────────────────────────────────────────────────────

$json = $report | ConvertTo-Json -Depth 10

Write-Host "`n─── Summary ───" -ForegroundColor Green
Write-Host "  IStyle:    +$($report.istyle.added.Count) added, -$($report.istyle.removed.Count) removed, ~$($report.istyle.changed.Count) changed"
Write-Host "  Elements:  +$($report.elements.added.Count) added, -$($report.elements.removed.Count) removed"
Write-Host "  Enums:     +$($report.enums.types.added.Count) new types, $($report.enums.members.Count) types with member changes"
Write-Host "  Structs:   +$($report.structs.types.added.Count) added, -$($report.structs.types.removed.Count) removed, $($report.structs.memberChanges.Count) with API changes"

if ($OutFile) {
    $dir = Split-Path $OutFile
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    $json | Set-Content -Path $OutFile -Encoding UTF8
    Write-Host "`nReport written to: $OutFile" -ForegroundColor Green
}
else {
    Write-Host "`n─── JSON Report ───" -ForegroundColor Green
    $json
}
