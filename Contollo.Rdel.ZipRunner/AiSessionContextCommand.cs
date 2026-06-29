using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Contollo.Rdel.ZipRunner.AI.Engine;
using Task = System.Threading.Tasks.Task;
using WinForms = System.Windows.Forms;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class AiSessionContextCommand
    {
        public const int CopyInitializeCommandId = 0x0103;
        public const int CopyRehydrateCommandId = 0x0104;
        public const int CopyContinueCommandId = 0x0105;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");
        private readonly AsyncPackage package;
        private readonly AiSessionContextKind kind;

        private AiSessionContextCommand(AsyncPackage package, OleMenuCommandService commandService, int commandId, AiSessionContextKind kind)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.kind = kind;
            var id = new CommandID(CommandSet, commandId);
            commandService.AddCommand(new MenuCommand(async (s, e) => await ExecuteAsync(), id));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null) return;
            new AiSessionContextCommand(package, commandService, CopyInitializeCommandId, AiSessionContextKind.Initialize);
            new AiSessionContextCommand(package, commandService, CopyRehydrateCommandId, AiSessionContextKind.Rehydrate);
            new AiSessionContextCommand(package, commandService, CopyContinueCommandId, AiSessionContextKind.Continue);
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
                string text = new AiSessionContextBuilder().Build(kind, dte);
                WinForms.Clipboard.SetText(text);
                await ShowMessageAsync(kind + " AI session context copied to clipboard.");
            }
            catch (Exception ex) { await ShowMessageAsync("Could not copy AI session context: " + ex.Message); }
        }

        private async Task ShowMessageAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.ShowMessageBox(package, message, "Contollo RDEL AI Session", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
