$ProjectFile = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner\Contollo.Rdel.ZipRunner\Contollo.Rdel.ZipRunner.csproj"
$BackupFile = "$ProjectFile.vstoolspath-broken-" + (Get-Date -Format "yyyyMMdd-HHmmss")

if (!(Test-Path $ProjectFile)) {
    Write-Host "Project file not found:"
    Write-Host $ProjectFile
    exit 1
}

Copy-Item $ProjectFile $BackupFile -Force
Write-Host "Backed up current project file to:"
Write-Host $BackupFile

$content = [System.IO.File]::ReadAllText($ProjectFile)

# Remove XML-invalid control characters except tab, CR, LF.
$cleanBuilder = New-Object System.Text.StringBuilder
for ($i = 0; $i -lt $content.Length; $i++) {
    $ch = $content[$i]
    $code = [int][char]$ch

    if (($code -lt 32) -and ($code -ne 9) -and ($code -ne 10) -and ($code -ne 13)) {
        Write-Host ("Removed invalid XML control character at index {0}: 0x{1:X2}" -f $i, $code)
        continue
    }

    [void]$cleanBuilder.Append($ch)
}

$content = $cleanBuilder.ToString()

# Replace the entire VSToolsPath element with a safe forward-slash version.
# Using / avoids the \v sequence ever being interpreted as vertical-tab by any tool.
$safeLine = '    <VSToolsPath Condition="''$(VSToolsPath)'' == ''''">$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)</VSToolsPath>'

$pattern = '(?s)\s*<VSToolsPath\s+Condition="''\$\((?:VSToolsPath)\)'' == ''''">.*?</VSToolsPath>'
if ($content -match $pattern) {
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, "`r`n$safeLine", 1)
    Write-Host "Replaced existing VSToolsPath element with safe forward-slash path."
}
else {
    # Insert after MinimumVisualStudioVersion if missing.
    $content = $content -replace '(\s*<MinimumVisualStudioVersion>.*?</MinimumVisualStudioVersion>)', "`$1`r`n$safeLine"
    Write-Host "Inserted safe VSToolsPath element."
}

# Remove accidental root-level bad project file if present.
$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
$RootLevelProject = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner.csproj"
if (Test-Path $RootLevelProject) {
    $RootBackup = "$RootLevelProject.accidental-root-" + (Get-Date -Format "yyyyMMdd-HHmmss")
    Copy-Item $RootLevelProject $RootBackup -Force
    Remove-Item $RootLevelProject -Force
    Write-Host "Removed accidental root-level project file. Backup:"
    Write-Host $RootBackup
}

[System.IO.File]::WriteAllText($ProjectFile, $content, [System.Text.UTF8Encoding]::new($false))

# Verify no invalid XML control characters remain.
$verify = [System.IO.File]::ReadAllText($ProjectFile)
$badChars = @()
for ($i = 0; $i -lt $verify.Length; $i++) {
    $code = [int][char]$verify[$i]
    if (($code -lt 32) -and ($code -ne 9) -and ($code -ne 10) -and ($code -ne 13)) {
        $badChars += ("Index {0}: 0x{1:X2}" -f $i, $code)
    }
}

if ($badChars.Count -gt 0) {
    Write-Host "Invalid XML control characters remain:"
    $badChars | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host ""
Write-Host "Confirmed VSToolsPath line:"
Select-String -Path $ProjectFile -Pattern "VSToolsPath" | ForEach-Object { Write-Host $_.Line }

Write-Host ""
Write-Host "Repair complete. Close Visual Studio completely, reopen the solution, then build from the Visual Studio Build menu."
