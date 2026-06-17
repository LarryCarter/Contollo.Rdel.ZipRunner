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
    internal sealed class DryRunZipUpdateCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d");
        private readonly AsyncPackage package;

        private DryRunZipUpdateCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            var commandId = new CommandID(CommandSet, CommandId);
            commandService.AddCommand(new MenuCommand(async (sender, args) => await ExecuteAsync(), commandId));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null) { new DryRunZipUpdateCommand(package, commandService); }
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var pane = await RdelOutputPane.CreateAsync(package);
            pane.WriteLine("Contollo RDEL dry run started.");

            string zipPath = PickZipFile();
            if (string.IsNullOrWhiteSpace(zipPath)) { pane.WriteLine("No zip selected."); return; }

            DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
            string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);
            if (string.IsNullOrWhiteSpace(solutionRoot)) { await ShowMessageAsync("Open a solution first."); return; }

            string selectedProjectRoot = RdelSolutionLocator.GetSelectedProjectRoot(dte);
            string packageName = Path.GetFileNameWithoutExtension(zipPath);
            string runId = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-dry-run-" + RdelPath.SanitizeName(packageName);
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
                var dryRunService = new RdelDryRunService();
                var result = dryRunService.Analyze(zipPath, solutionRoot, selectedProjectRoot, runRoot, pane);
                record.Manifest = result.Manifest;
                record.TargetRoot = result.TargetRoot;
                record.AppliedFiles = result.WouldApplyFiles;
                record.CompletedUtc = DateTime.UtcNow;
                record.Succeeded = true;

                RdelHistoryWriter.Write(solutionRoot, record);
                string reportPath = Path.Combine(runRoot, "dry-run-report.txt");
                dryRunService.WriteReport(reportPath, result);

                pane.WriteLine("Dry run complete.");
                pane.WriteLine("Report: " + reportPath);
                await ShowMessageAsync("Dry run complete. See Contollo RDEL output and dry-run-report.txt.");
            }
            catch (Exception ex)
            {
                record.CompletedUtc = DateTime.UtcNow;
                record.Succeeded = false;
                record.Error = ex.ToString();
                RdelHistoryWriter.Write(solutionRoot, record);
                pane.WriteLine("Dry run failed:");
                pane.WriteLine(ex.ToString());
                await ShowMessageAsync("Dry run failed. See Visual Studio Output window.");
            }
        }

        private static string PickZipFile()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Title = "Select Contollo RDEL update package for dry run";
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