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
      <!-- This invisible group places the Contollo RDEL submenu under Tools. -->
      <Group guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" priority="0x0600">
        <Parent guid="guidSHLMainMenu" id="IDM_VS_MENU_TOOLS"/>
      </Group>

      <!-- This group contains the actual commands inside the Contollo RDEL submenu. -->
      <Group guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelSubMenuGroup" priority="0x0100">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelMenu"/>
      </Group>
    </Groups>

    <Menus>
      <Menu guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelMenu" priority="0x0100" type="Menu">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup"/>
        <Strings>
          <ButtonText>Contollo RDEL</ButtonText>
          <CommandName>Contollo RDEL</CommandName>
        </Strings>
      </Menu>
    </Menus>

    <Buttons>
      <Button guid="guidContolloRdelZipRunnerCommandSet" id="ApplyZipUpdateCommandId" priority="0x0100" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelSubMenuGroup" />
        <Strings>
          <ButtonText>Apply Patch</ButtonText>
          <CommandName>Apply Patch</CommandName>
        </Strings>
      </Button>

      <Button guid="guidContolloRdelZipRunnerCommandSet" id="DryRunZipUpdateCommandId" priority="0x0110" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelSubMenuGroup" />
        <Strings>
          <ButtonText>Test Run</ButtonText>
          <CommandName>Test Run</CommandName>
        </Strings>
      </Button>

      <Button guid="guidContolloRdelZipRunnerCommandSet" id="RollbackLastRunCommandId" priority="0x0120" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelSubMenuGroup" />
        <Strings>
          <ButtonText>Rollback Last Patch</ButtonText>
          <CommandName>Rollback Last Patch</CommandName>
        </Strings>
      </Button>
    </Buttons>
  </Commands>

  <Symbols>
    <GuidSymbol name="guidContolloRdelZipRunnerPackage" value="{a1361a6f-005b-4060-a6ef-3389916ce837}" />

    <GuidSymbol name="guidContolloRdelZipRunnerCommandSet" value="{50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d}">
      <IDSymbol name="ContolloRdelToolsGroup" value="0x1020" />
      <IDSymbol name="ContolloRdelMenu" value="0x1021" />
      <IDSymbol name="ContolloRdelSubMenuGroup" value="0x1022" />

      <IDSymbol name="ApplyZipUpdateCommandId" value="0x0100" />
      <IDSymbol name="DryRunZipUpdateCommandId" value="0x0101" />
      <IDSymbol name="RollbackLastRunCommandId" value="0x0102" />
    </GuidSymbol>
  </Symbols>
</CommandTable>
'@

Write-TextFile $VsctFile $VsctContent

$content = Get-Content $ProjectFile -Raw

if (!$content.Contains('<VSCTCompile Include="Contollo.Rdel.ZipRunner.vsct">')) {
    $vsctItemGroup = @'

  <ItemGroup>
    <VSCTCompile Include="Contollo.Rdel.ZipRunner.vsct">
      <ResourceName>Menus.ctmenu</ResourceName>
    </VSCTCompile>
  </ItemGroup>
'@

    $content = $content -replace '\s*</Project>\s*$', ($vsctItemGroup + "`r`n</Project>")
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

Remove-Item -Recurse -Force (Join-Path $ProjectDir "bin") -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force (Join-Path $ProjectDir "obj") -ErrorAction SilentlyContinue

Write-Host "RDEL 0008 applied."
Write-Host ""
Write-Host "Tools menu will now be:"
Write-Host "  Contollo RDEL"
Write-Host "    Apply Patch"
Write-Host "    Test Run"
Write-Host "    Rollback Last Patch"
Write-Host ""
Write-Host "Next:"
Write-Host "  1. Close all Experimental Visual Studio windows."
Write-Host "  2. Clean Solution."
Write-Host "  3. Rebuild Solution."
Write-Host "  4. Press F5."
Write-Host "  5. Check Tools -> Contollo RDEL."
