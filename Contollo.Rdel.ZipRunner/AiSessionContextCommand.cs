using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        { this.package = package ?? throw new ArgumentNullException(nameof(package)); this.kind = kind; var id = new CommandID(CommandSet, commandId); var item = new MenuCommand(async (s, a) => await ExecuteAsync(), id); commandService.AddCommand(item); }

        public static async Task InitializeAsync(AsyncPackage package)
        { await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); OleMenuCommandService service = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService; if (service == null) return; new AiSessionContextCommand(package, service, CopyInitializeCommandId, AiSessionContextKind.Initialize); new AiSessionContextCommand(package, service, CopyRehydrateCommandId, AiSessionContextKind.Rehydrate); new AiSessionContextCommand(package, service, CopyContinueCommandId, AiSessionContextKind.Continue); }

        private async Task ExecuteAsync()
        { await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); try { DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2; var builder = new AiSessionContextBuilder(); string text = builder.Build(kind, dte); WinForms.Clipboard.SetText(text); await ShowMessageAsync(kind + " AI session context copied to clipboard."); } catch (Exception ex) { await ShowMessageAsync("Could not copy AI session context: " + ex.Message); } }

        private async Task ShowMessageAsync(string message)
        { await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); VsShellUtilities.ShowMessageBox(package, message, "Contollo RDEL AI Session", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST); }
    }
}
