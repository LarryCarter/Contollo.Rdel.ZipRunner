param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$File = Join-Path $ProjectDir "RdelOutputPane.cs"

if (!(Test-Path $File)) {
    throw "Could not find file: $File"
}

$Content = @'
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SystemThreadingTasks = System.Threading.Tasks;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelOutputPane
    {
        private static readonly Guid PaneGuid = new Guid("255c735f-979c-4193-9d52-2e39056bf9bb");
        private readonly IVsOutputWindowPane pane;

        private RdelOutputPane(IVsOutputWindowPane pane)
        {
            this.pane = pane;
        }

        public static async SystemThreadingTasks.Task<RdelOutputPane> CreateAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsOutputWindow outputWindow =
                await package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid paneGuid = PaneGuid;

            outputWindow.CreatePane(ref paneGuid, "Contollo RDEL", 1, 1);
            outputWindow.GetPane(ref paneGuid, out IVsOutputWindowPane pane);
            pane.Activate();

            return new RdelOutputPane(pane);
        }

        public void WriteLine(string message)
        {
            pane.OutputStringThreadSafe("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        }
    }
}
'@

[System.IO.File]::WriteAllText($File, $Content, [System.Text.Encoding]::UTF8)

Write-Host "Fixed RdelOutputPane.cs"
Write-Host "Now Clean Solution and Rebuild Solution."
