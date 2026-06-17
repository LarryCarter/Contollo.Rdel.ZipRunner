using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ContolloRdelZipRunnerPackage.PackageGuidString)]
    public sealed class ContolloRdelZipRunnerPackage : AsyncPackage
    {
        public const string PackageGuidString = "a1361a6f-005b-4060-a6ef-3389916ce837";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ApplyZipUpdateCommand.InitializeAsync(this);
        }
    }
}