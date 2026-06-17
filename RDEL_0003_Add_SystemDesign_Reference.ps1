param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

function Add-ReferenceOnce {
    param(
        [string]$ProjectFile,
        [string]$ReferenceLine
    )

    $content = Get-Content $ProjectFile -Raw
    if ($content.Contains($ReferenceLine)) {
        return
    }

    $anchor = '    <Reference Include="System" />'
    if (!$content.Contains($anchor)) {
        throw "Could not find System reference anchor in project file."
    }

    $content = $content.Replace($anchor, $anchor + "`r`n" + $ReferenceLine)
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$ProjectFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.csproj"

if (!(Test-Path $ProjectFile)) {
    throw "Could not find project file: $ProjectFile"
}

Add-ReferenceOnce $ProjectFile '    <Reference Include="System.Design" />'

Write-Host "Added System.Design reference."
Write-Host "Now Clean Solution and Rebuild Solution."
