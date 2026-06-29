param(
    [Parameter(Mandatory=$true)]
    [string]$PackagePath
)

$ErrorActionPreference = "Stop"

$blocked = @(
    ".git/",
    ".vs/",
    ".contollo/",
    "bin/",
    "obj/",
    ".env",
    "secrets.json",
    "appsettings.Production.json",
    ".pfx",
    ".pem",
    ".key",
    ".cer",
    ".crt",
    ".snk"
)

function Fail($message) {
    Write-Host "RDEL PREFLIGHT ERROR: $message" -ForegroundColor Red
    exit 1
}

function Warn($message) {
    Write-Host "RDEL PREFLIGHT WARNING: $message" -ForegroundColor Yellow
}

function Info($message) {
    Write-Host "RDEL PREFLIGHT: $message" -ForegroundColor Cyan
}

$temp = $null

try {
    if (!(Test-Path $PackagePath)) {
        Fail "Package path not found: $PackagePath"
    }

    $target = Get-Item $PackagePath

    if ($target.PSIsContainer) {
        $root = $target.FullName
    }
    else {
        $temp = Join-Path ([System.IO.Path]::GetTempPath()) ("rdel-preflight-" + [guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Path $temp | Out-Null
        Expand-Archive -Path $target.FullName -DestinationPath $temp -Force

        $children = Get-ChildItem $temp
        if ($children.Count -eq 1 -and $children[0].PSIsContainer) {
            $root = $children[0].FullName
        }
        else {
            $root = $temp
        }
    }

    $manifestPath = Join-Path $root "contollo-rdel.json"
    $readmePath = Join-Path $root "README.md"

    if (!(Test-Path $manifestPath)) {
        Fail "Missing contollo-rdel.json"
    }

    if (!(Test-Path $readmePath)) {
        Fail "Missing README.md"
    }

    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

    foreach ($field in @("Name", "Description", "Target", "Commands")) {
        if ($null -eq $manifest.$field) {
            Fail "Missing required manifest field: $field"
        }
    }

    if ($manifest.Target -ne "solution" -and $manifest.Target -ne "selected-project") {
        Fail "Invalid Target: $($manifest.Target)"
    }

    if ($manifest.Commands.Count -eq 0) {
        Warn "Commands is empty. Current runner uses Commands as authoritative validation."
    }

    $files = Get-ChildItem $root -Recurse -File
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($root.Length).TrimStart("\", "/").Replace("\", "/")
        foreach ($pattern in $blocked) {
            if ($relative.ToLowerInvariant().Contains($pattern.ToLowerInvariant()) -or $relative.ToLowerInvariant().EndsWith($pattern.ToLowerInvariant())) {
                Fail "Blocked path detected: $relative"
            }
        }
    }

    if ($manifest.ChangeIntent -match "architecture|context|memory|decision") {
        $hasContext = Test-Path (Join-Path $root "docs/context.md")
        $hasDecision = Test-Path (Join-Path $root "docs/DECISIONS.md")
        if (!$hasContext) { Warn "ChangeIntent suggests context impact but docs/context.md is not included." }
        if (!$hasDecision) { Warn "ChangeIntent suggests architecture decision impact but docs/DECISIONS.md is not included." }
    }

    $sha = $null
    if (!$target.PSIsContainer) {
        $sha = (Get-FileHash $target.FullName -Algorithm SHA256).Hash
        Info "Package SHA-256: $sha"
    }

    Info "Manifest: $($manifest.Name)"
    Info "Target: $($manifest.Target)"
    Info "ValidationProfile: $($manifest.ValidationProfile)"
    Info "Commands:"
    $manifest.Commands | ForEach-Object { Write-Host "  $_" }

    Write-Host ""
    Write-Host "RDEL preflight passed." -ForegroundColor Green
}
finally {
    if ($temp -and (Test-Path $temp)) {
        Remove-Item $temp -Recurse -Force
    }
}
