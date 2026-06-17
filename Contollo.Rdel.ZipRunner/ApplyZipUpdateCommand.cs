using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using WinForms = System.Windows.Forms;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class ApplyZipUpdateCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");
        private readonly AsyncPackage package;

        private ApplyZipUpdateCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            var commandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(async (sender, args) => await ExecuteAsync(), commandId);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null) { new ApplyZipUpdateCommand(package, commandService); }
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var pane = await RdelOutputPane.CreateAsync(package);
            pane.WriteLine("Contollo RDEL Zip Runner started.");

            string zipPath = PickZipFile();
            if (string.IsNullOrWhiteSpace(zipPath)) { pane.WriteLine("No zip selected."); return; }

            DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
            string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);
            if (string.IsNullOrWhiteSpace(solutionRoot)) { await ShowMessageAsync("Open a solution first."); return; }

            string selectedProjectRoot = RdelSolutionLocator.GetSelectedProjectRoot(dte);
            string packageName = Path.GetFileNameWithoutExtension(zipPath);
            string runId = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + RdelPath.SanitizeName(packageName);
            string runRoot = Path.Combine(solutionRoot, ".contollo", "rdel", "runs", runId);
            Directory.CreateDirectory(runRoot);

            var record = new RdelRunRecord
            {
                RunId = runId,
                PackageName = packageName,
                ZipPath = zipPath,
                SolutionRoot = solutionRoot,
                SelectedProjectRoot = selectedProjectRoot,
                RunRoot = runRoot,
                StartedUtc = DateTime.UtcNow
            };

            try
            {
                var git = new RdelGitCheckpointService();
                record.Git = git.CreateCheckpoint(solutionRoot, "RDEL checkpoint: before " + packageName, pane);

                var zipService = new RdelZipPackageService();
                var applyResult = zipService.Apply(zipPath, solutionRoot, selectedProjectRoot, runRoot, pane);
                record.Manifest = applyResult.Manifest;
                record.TargetRoot = applyResult.TargetRoot;
                record.AppliedFiles = applyResult.AppliedFiles;
                record.BackupRoot = applyResult.BackupRoot;

                var runner = new RdelCommandRunner();
                string[] commands = applyResult.Manifest != null && applyResult.Manifest.Commands != null && applyResult.Manifest.Commands.Length > 0
                    ? applyResult.Manifest.Commands
                    : RdelManifest.DefaultCommands;

                foreach (string command in commands)
                {
                    var result = runner.Run(command, record.TargetRoot, pane);
                    record.Commands.Add(result);
                }

                record.CompletedUtc = DateTime.UtcNow;
                record.Succeeded = true;
                foreach (var command in record.Commands)
                {
                    if (command.ExitCode != 0) { record.Succeeded = false; break; }
                }

                RdelHistoryWriter.Write(solutionRoot, record);
                pane.WriteLine("RDEL run complete. Success: " + record.Succeeded);
                pane.WriteLine("History: " + Path.Combine(runRoot, "run-history.json"));
                await ShowMessageAsync("RDEL run complete. Success: " + record.Succeeded);
            }
            catch (Exception ex)
            {
                record.CompletedUtc = DateTime.UtcNow;
                record.Succeeded = false;
                record.Error = ex.ToString();
                RdelHistoryWriter.Write(solutionRoot, record);
                pane.WriteLine("RDEL failed:");
                pane.WriteLine(ex.ToString());
                await ShowMessageAsync("RDEL failed. See Visual Studio Output window.");
            }
        }

        private static string PickZipFile()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Title = "Select Contollo RDEL update package";
                dialog.Filter = "Zip files (*.zip)|*.zip|All files (*.*)|*.*";
                dialog.InitialDirectory = Directory.Exists(downloads) ? downloads : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return dialog.ShowDialog() == WinForms.DialogResult.OK ? dialog.FileName : null;
            }
        }

        private async Task ShowMessageAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.ShowMessageBox(package, message, "Contollo RDEL Zip Runner", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}