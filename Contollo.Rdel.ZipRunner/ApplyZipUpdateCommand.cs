using System;
using System.ComponentModel.Design;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

            OleMenuCommandService commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService != null)
            {
                new ApplyZipUpdateCommand(package, commandService);
            }
        }

        private async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var pane = await RdelOutputPane.CreateAsync(package);
            pane.WriteLine("Contollo RDEL Zip Runner started.");

            string zipPath = PickZipFile();
            if (string.IsNullOrWhiteSpace(zipPath))
            {
                pane.WriteLine("No zip selected.");
                return;
            }

            DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
            string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);
            if (string.IsNullOrWhiteSpace(solutionRoot))
            {
                await ShowMessageAsync("Open a solution first.");
                return;
            }

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
                PackageSha256 = ComputeSha256(zipPath),
                SolutionRoot = solutionRoot,
                SelectedProjectRoot = selectedProjectRoot,
                RunRoot = runRoot,
                StartedUtc = DateTime.UtcNow
            };

            var git = new RdelGitCheckpointService();

            try
            {
                pane.WriteLine("Package SHA256: " + record.PackageSha256);
                record.Git = git.CreateCheckpoint(solutionRoot, "RDEL checkpoint: before " + packageName, pane);

                var zipService = new RdelZipPackageService();
                var applyResult = zipService.Apply(zipPath, solutionRoot, selectedProjectRoot, runRoot, pane);

                record.Manifest = applyResult.Manifest;
                record.PackageMetadata = applyResult.PackageMetadata;
                record.TargetRoot = applyResult.TargetRoot;
                record.AppliedFiles = applyResult.AppliedFiles;
                record.BackupRoot = applyResult.BackupRoot;
                record.ApplySucceeded = true;

                var runner = new RdelCommandRunner();
                string[] commands = applyResult.Manifest != null && applyResult.Manifest.Commands != null && applyResult.Manifest.Commands.Length > 0
                    ? applyResult.Manifest.Commands
                    : RdelManifest.DefaultCommands;

                foreach (string command in commands)
                {
                    var result = runner.Run(command, record.TargetRoot, pane);
                    record.Commands.Add(result);
                }

                record.Verification = BuildVerificationSummary(record);
                record.ValidationSucceeded = record.Verification.CommandsSucceeded;

                record.Succeeded = record.ApplySucceeded && record.ValidationSucceeded;
                record.CompletedUtc = DateTime.UtcNow;

                RdelHistoryWriter.Write(solutionRoot, record);

                record.PostApplyGit = git.CreatePostApplyCommit(solutionRoot, record, pane);

                RdelHistoryWriter.Write(solutionRoot, record);

                pane.WriteLine("RDEL apply complete. ApplySucceeded: " + record.ApplySucceeded);
                pane.WriteLine("RDEL validation complete. ValidationSucceeded: " + record.ValidationSucceeded);
                pane.WriteLine("RDEL overall success: " + record.Succeeded);
                pane.WriteLine("History: " + Path.Combine(runRoot, "run-history.json"));

                string message = record.Succeeded
                    ? "RDEL run complete. Apply and validation succeeded."
                    : "RDEL apply completed, but validation had issues. See Output and run history.";

                await ShowMessageAsync(message);
            }
            catch (Exception ex)
            {
                record.CompletedUtc = DateTime.UtcNow;
                record.ApplySucceeded = false;
                record.ValidationSucceeded = false;
                record.Succeeded = false;
                record.Error = ex.ToString();

                RdelHistoryWriter.Write(solutionRoot, record);

                pane.WriteLine("RDEL failed:");
                pane.WriteLine(ex.ToString());

                await ShowMessageAsync("RDEL failed. See Visual Studio Output window.");
            }
        }

        private static RdelVerificationSummary BuildVerificationSummary(RdelRunRecord record)
        {
            var summary = new RdelVerificationSummary
            {
                CommandsSucceeded = true,
                ExpectedBuildPassed = true,
                ExpectedMinimumTestsSatisfied = true,
                ForbiddenChangedPathsClean = true
            };

            foreach (var command in record.Commands)
            {
                if (command.ExitCode != 0)
                {
                    summary.CommandsSucceeded = false;
                    summary.Notes.Add("Command failed: " + command.Command);
                }
            }

            if (record.Manifest != null && record.Manifest.ExpectedAfterApply != null)
            {
                var expected = record.Manifest.ExpectedAfterApply;
                if (expected.BuildMustPass && !summary.CommandsSucceeded)
                {
                    summary.ExpectedBuildPassed = false;
                    summary.Notes.Add("Expected build/validation commands to pass, but one or more commands failed.");
                }

                if (expected.ForbiddenChangedPaths != null && record.AppliedFiles != null)
                {
                    foreach (string forbidden in expected.ForbiddenChangedPaths)
                    {
                        foreach (string appliedFile in record.AppliedFiles)
                        {
                            if (appliedFile.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                summary.ForbiddenChangedPathsClean = false;
                                summary.Notes.Add("Forbidden changed path matched: " + appliedFile);
                            }
                        }
                    }
                }
            }

            if (summary.Notes.Count == 0)
            {
                summary.Notes.Add("All configured command-based checks passed.");
            }

            return summary;
        }

        private static string PickZipFile()
        {
            string downloads = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Title = "Select Contollo RDEL update package";
                dialog.Filter = "Zip files (*.zip)|*.zip|All files (*.*)|*.*";
                dialog.InitialDirectory = Directory.Exists(downloads)
                    ? downloads
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                return dialog.ShowDialog() == WinForms.DialogResult.OK
                    ? dialog.FileName
                    : null;
            }
        }

        private async Task ShowMessageAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            VsShellUtilities.ShowMessageBox(
                package,
                message,
                "Contollo RDEL Zip Runner",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static string ComputeSha256(string filePath)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
