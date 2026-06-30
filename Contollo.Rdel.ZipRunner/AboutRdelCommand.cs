using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class AboutRdelCommand
    {
        public const int CommandId = 0x0108;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");

        private readonly AsyncPackage package;

        private AboutRdelCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            var id = new CommandID(CommandSet, CommandId);
            commandService.AddCommand(new MenuCommand(async (s, e) => await ExecuteAsync(), id));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                new AboutRdelCommand(package, commandService);
            }
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
                string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);

                using (var dialog = new AboutRdelDialog(solutionRoot))
                {
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "Could not open About RDEL: " + ex.Message,
                    "About Contollo RDEL",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
