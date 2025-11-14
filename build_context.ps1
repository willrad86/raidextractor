<#
================================================================================
 build_context.ps1 — Lightweight Universal Snapshot (Versioned)
 Skips heavy folders like node_modules, ios, android, .expo, .git
================================================================================
#>

param(
    [string]$ProjectRoot = ".",
    [string]$Output = $null
)

Write-Host "Generating lightweight project snapshot..."

if (-not $Output) {
    $ts = (Get-Date).ToString("yyyy-MM-dd_HH-mm-ss")
    $Output = "project_state_$ts.json"
}

# Folders to exclude from scanning
$exclude = @(
    "node_modules",
    "android",
    "ios",
    ".expo",
    ".git",
    "build",
    "dist",
    "assets"
)

function SafeRead {
    param([string]$Path)
    try { return Get-Content $Path -Raw -ErrorAction Stop }
    catch { return "" }
}

function ExtractFunctions {
    param([string]$Code)

    $patterns = @(
        'function\s+([A-Za-z0-9_]+)\s*\(',
        'const\s+([A-Za-z0-9_]+)\s*=\s*\(',
        'export\s+function\s+([A-Za-z0-9_]+)\s*\(',
        'export\s+const\s+([A-Za-z0-9_]+)\s*='
    )

    $results = @()
    foreach ($p in $patterns) {
        $matches = [regex]::Matches($Code, $p)
        foreach ($m in $matches) { $results += $m.Groups[1].Value }
    }

    return $results | Sort-Object -Unique
}

function ExtractSchemas {
    param([string]$Code)
    $regex = "CREATE TABLE[\s\S]+?\;"
    $matches = [regex]::Matches($Code, $regex)
    $schemas = @()
    foreach ($m in $matches) { $schemas += $m.Value.Trim() }
    return $schemas
}

function DetectCategory {
    param([string]$Path, [string]$Code)

    if ($Path -match "trip" -or $Code -match "startTrip|stopTrip|addPoint") { return "trip" }
    if ($Code -match "hash|signature|crypto|previousHash|storedHash") { return "crypto" }
    if ($Code -match "GPS|Location|watchPositionAsync") { return "gps" }
    if ($Code -match "export|shareAsync|FileSystem") { return "export" }
    if ($Code -match "CREATE TABLE|SQLite") { return "database" }
    if ($Path -match "navigation") { return "navigation" }
    if ($Path -match "service") { return "service" }
    if ($Path -match "context") { return "context" }
    if ($Path -match "hook") { return "hook" }
    if ($Path -match "util") { return "util" }
    if ($Path -match "component") { return "component" }

    return "other"
}

# Only scan app code folders
$files = Get-ChildItem -Path $ProjectRoot -Recurse -Include *.ts, *.tsx, *.js -File |
    Where-Object {
        foreach ($ex in $exclude) {
            if ($_.FullName -match [regex]::Escape($ex)) { return $false }
        }
        return $true
    }

$fileReports = @()

foreach ($file in $files) {

    $content = SafeRead $file.FullName

    $fileReports += [PSCustomObject]@{
        path       = $file.FullName.Replace("$ProjectRoot\", "")
        category   = DetectCategory -Path $file.FullName -Code $content
        functions  = ExtractFunctions $content
        schemas    = ExtractSchemas  $content
    }
}

$packageJson = if (Test-Path "$ProjectRoot\package.json") {
    Get-Content "$ProjectRoot\package.json" -Raw | ConvertFrom-Json
} else { $null }

$appJson = if (Test-Path "$ProjectRoot\app.json") {
    Get-Content "$ProjectRoot\app.json" -Raw | ConvertFrom-Json
} else { $null }

$timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")

$report = [PSCustomObject]@{
    buildTimestamp = $timestamp
    outputFile     = $Output
    projectRoot    = (Resolve-Path $ProjectRoot).Path

    metadata = [PSCustomObject]@{
        name            = $appJson.expo.name
        slug            = $appJson.expo.slug
        version         = $appJson.expo.version
        sdkVersion      = $appJson.expo.sdkVersion
        dependencies    = $packageJson.dependencies
        devDependencies = $packageJson.devDependencies
    }

    files = $fileReports
}

$report | ConvertTo-Json -Depth 6 | Out-File $Output -Encoding utf8

Write-Host "Snapshot created:"
Write-Host $Output
