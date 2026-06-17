using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RollbackLastRunCommand
    {
        public const int CommandId = 0x0102;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");
        private readonly AsyncPackage package;

        private RollbackLastRunCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            var commandId = new CommandID(CommandSet, CommandId);
            commandService.AddCommand(new MenuCommand(async (sender, args) => await ExecuteAsync(), commandId));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null) { new RollbackLastRunCommand(package, commandService); }
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var pane = await RdelOutputPane.CreateAsync(package);
            pane.WriteLine("Contollo RDEL rollback started.");

            DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
            string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);
            if (string.IsNullOrWhiteSpace(solutionRoot)) { await ShowMessageAsync("Open a solution first."); return; }

            var rollback = new RdelRollbackService();
            string preview = rollback.GetRollbackPreview(solutionRoot);
            if (string.IsNullOrWhiteSpace(preview)) { await ShowMessageAsync("No rollbackable RDEL run found."); return; }

            int result = VsShellUtilities.ShowMessageBox(
                package,
                preview + Environment.NewLine + Environment.NewLine + "Rollback last RDEL run now?",
                "Contollo RDEL Rollback",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);

            if (result != (int)VSConstants.MessageBoxResult.IDYES) { pane.WriteLine("Rollback cancelled."); return; }

            try
            {
                string rollbackReport = rollback.RollbackLastRun(solutionRoot, pane);
                pane.WriteLine("Rollback complete.");
                pane.WriteLine(rollbackReport);
                await ShowMessageAsync("Rollback complete. See Contollo RDEL output.");
            }
            catch (Exception ex)
            {
                pane.WriteLine("Rollback failed:");
                pane.WriteLine(ex.ToString());
                await ShowMessageAsync("Rollback failed. See Visual Studio Output window.");
            }
        }

        private async Task ShowMessageAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.ShowMessageBox(package, message, "Contollo RDEL Zip Runner", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}