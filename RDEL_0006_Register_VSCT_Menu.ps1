param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    if ($directory -and !(Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.Encoding]::UTF8)
}

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$ProjectFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.csproj"
$VsctFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.vsct"

if (!(Test-Path $ProjectFile)) {
    throw "Could not find project file: $ProjectFile"
}

$VsctContent = @'
<?xml version="1.0" encoding="utf-8"?>
<CommandTable xmlns="http://schemas.microsoft.com/VisualStudio/2005-10-18/CommandTable" xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <Extern href="stdidcmd.h"/>
  <Extern href="vsshlids.h"/>

  <Commands package="guidContolloRdelZipRunnerPackage">
    <Groups>
      <Group guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" priority="0x0600">
        <Parent guid="guidSHLMainMenu" id="IDM_VS_MENU_TOOLS"/>
      </Group>
    </Groups>

    <Buttons>
      <Button guid="guidContolloRdelZipRunnerCommandSet" id="ApplyZipUpdateCommandId" priority="0x0100" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Strings>
          <ButtonText>Contollo RDEL: Apply Zip Update</ButtonText>
        </Strings>
      </Button>

      <Button guid="guidContolloRdelZipRunnerCommandSet" id="DryRunZipUpdateCommandId" priority="0x0110" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Strings>
          <ButtonText>Contollo RDEL: Dry Run Zip Update</ButtonText>
        </Strings>
      </Button>

      <Button guid="guidContolloRdelZipRunnerCommandSet" id="RollbackLastRunCommandId" priority="0x0120" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Strings>
          <ButtonText>Contollo RDEL: Rollback Last Run</ButtonText>
        </Strings>
      </Button>
    </Buttons>
  </Commands>

  <Symbols>
    <GuidSymbol name="guidContolloRdelZipRunnerPackage" value="{a1361a6f-005b-4060-a6ef-3389916ce837}" />

    <GuidSymbol name="guidContolloRdelZipRunnerCommandSet" value="{50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d}">
      <IDSymbol name="ContolloRdelToolsGroup" value="0x1020" />
      <IDSymbol name="ApplyZipUpdateCommandId" value="0x0100" />
      <IDSymbol name="DryRunZipUpdateCommandId" value="0x0101" />
      <IDSymbol name="RollbackLastRunCommandId" value="0x0102" />
    </GuidSymbol>
  </Symbols>
</CommandTable>
'@

Write-TextFile $VsctFile $VsctContent

$content = Get-Content $ProjectFile -Raw

$content = $content -replace '(?s)\s*<ItemGroup>\s*<VSCTCompile Include="Contollo\.Rdel\.ZipRunner\.vsct">.*?</VSCTCompile>\s*</ItemGroup>', ''

if (!$content.Contains('<VSCTCompile Include="Contollo.Rdel.ZipRunner.vsct">')) {
    $insert = @'

  <ItemGroup>
    <VSCTCompile Include="Contollo.Rdel.ZipRunner.vsct">
      <ResourceName>Menus.ctmenu</ResourceName>
    </VSCTCompile>
  </ItemGroup>
'@

    $anchor = '  <ItemGroup>
    <None Include="source.extension.vsixmanifest">'
    if (!$content.Contains($anchor)) {
        throw "Could not find manifest ItemGroup anchor."
    }

    $content = $content.Replace($anchor, $insert + "`r`n" + $anchor)
}

Set-Content -Path $ProjectFile -Value $content -Encoding UTF8

Write-Host "RDEL 0006 applied."
Write-Host "The VSCT menu file is now included in the project and the missing bitmap reference was removed."
Write-Host ""
Write-Host "Next:"
Write-Host "  1. Close all Experimental Visual Studio windows."
Write-Host "  2. In the main VSIX solution: Clean Solution."
Write-Host "  3. Rebuild Solution."
Write-Host "  4. Press F5."
Write-Host "  5. Check Tools menu again."
