param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$PackageFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunnerPackage.cs"

if (!(Test-Path $PackageFile)) {
    throw "Could not find package file: $PackageFile"
}

$Content = @'
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(ContolloRdelZipRunnerPackage.PackageGuidString)]
    public sealed class ContolloRdelZipRunnerPackage : AsyncPackage
    {
        public const string PackageGuidString = "a1361a6f-005b-4060-a6ef-3389916ce837";

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await ApplyZipUpdateCommand.InitializeAsync(this);
            await DryRunZipUpdateCommand.InitializeAsync(this);
            await RollbackLastRunCommand.InitializeAsync(this);
        }
    }
}
'@

[System.IO.File]::WriteAllText($PackageFile, $Content, [System.Text.Encoding]::UTF8)

Write-Host "RDEL 0005 applied: package now auto-loads in Experimental VS with or without a solution."
Write-Host ""
Write-Host "Next:"
Write-Host "  1. Close the Experimental Visual Studio instance."
Write-Host "  2. In the main Visual Studio instance: Clean Solution."
Write-Host "  3. Rebuild Solution."
Write-Host "  4. Press F5 again."
Write-Host "  5. In Experimental VS, check Tools menu for Contollo RDEL commands."
