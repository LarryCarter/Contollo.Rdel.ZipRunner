using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Contollo.Rdel.ZipRunner.AI.Export;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class ExportAiContextPackageCommand
    {
        public const int CommandId = 0x0106;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");
        private readonly AsyncPackage package;
        private ExportAiContextPackageCommand(AsyncPackage package, OleMenuCommandService service) { this.package = package; service.AddCommand(new MenuCommand(async (s,e) => await ExecuteAsync(), new CommandID(CommandSet, CommandId))); }
        public static async Task InitializeAsync(AsyncPackage package) { await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); var s = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService; if (s != null) new ExportAiContextPackageCommand(package, s); }
        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try { var dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2; string root = RdelSolutionLocator.GetSolutionRoot(dte); string path = new AiContextPackageExporter().Export(root); Show("AI context package exported:\n" + path); }
            catch (Exception ex) { Show("Could not export AI context package: " + ex.Message); }
        }
        private void Show(string m) { VsShellUtilities.ShowMessageBox(package, m, "Contollo RDEL AI Context Package", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST); }
    }
}
