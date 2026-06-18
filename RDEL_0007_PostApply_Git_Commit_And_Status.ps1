param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    if ($directory -and !(Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.Encoding]::UTF8)
}

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"

if (!(Test-Path $ProjectDir)) {
    throw "Could not find project directory: $ProjectDir"
}

$ModelsCs = @'
using System;
using System.Collections.Generic;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelRunRecord
    {
        public string RunId { get; set; }
        public string PackageName { get; set; }
        public string ZipPath { get; set; }
        public string SolutionRoot { get; set; }
        public string SelectedProjectRoot { get; set; }
        public string TargetRoot { get; set; }
        public string RunRoot { get; set; }
        public string BackupRoot { get; set; }
        public DateTime StartedUtc { get; set; }
        public DateTime CompletedUtc { get; set; }

        public bool ApplySucceeded { get; set; }
        public bool ValidationSucceeded { get; set; }
        public bool Succeeded { get; set; }

        public string Error { get; set; }
        public RdelGitCheckpoint Git { get; set; }
        public RdelGitCheckpoint PostApplyGit { get; set; }
        public RdelManifest Manifest { get; set; }
        public List<string> AppliedFiles { get; set; } = new List<string>();
        public List<RdelCommandResult> Commands { get; set; } = new List<RdelCommandResult>();
    }

    internal sealed class RdelGitCheckpoint
    {
        public bool IsGitRepository { get; set; }
        public bool HadChanges { get; set; }
        public bool CommitCreated { get; set; }
        public string HeadBefore { get; set; }
        public string HeadAfter { get; set; }
        public string CommitMessage { get; set; }
        public string CommitBody { get; set; }
        public string StatusBefore { get; set; }
        public string StatusAfter { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
    }

    internal sealed class RdelCommandResult
    {
        public string Command { get; set; }
        public string WorkingDirectory { get; set; }
        public int ExitCode { get; set; }
        public DateTime StartedUtc { get; set; }
        public DateTime CompletedUtc { get; set; }
        public string Output { get; set; }
    }

    internal sealed class RdelManifest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Target { get; set; }
        public string[] Commands { get; set; }

        public static readonly string[] DefaultCommands =
        {
            "dotnet restore",
            "dotnet build --no-restore",
            "dotnet test --no-build"
        };
    }

    internal sealed class RdelApplyResult
    {
        public string TargetRoot { get; set; }
        public string BackupRoot { get; set; }
        public RdelManifest Manifest { get; set; }
        public List<string> AppliedFiles { get; set; } = new List<string>();
    }
}
'@

$PathCs = @'
using System.IO;
using System.Text.RegularExpressions;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelPath
    {
        public static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "package";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '-');
            }

            return Regex.Replace(value, @"\s+", "-");
        }

        public static bool IsBlockedPath(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimStart('/');

            return normalized.StartsWith(".git/")
                || normalized.StartsWith(".vs/")
                || normalized.StartsWith("bin/")
                || normalized.StartsWith("obj/")
                || normalized.StartsWith(".contollo/")
                || normalized.StartsWith("__pycache__/")
                || normalized.EndsWith(".pyc")
                || normalized.Contains("/.git/")
                || normalized.Contains("/.vs/")
                || normalized.Contains("/bin/")
                || normalized.Contains("/obj/")
                || normalized.Contains("/.contollo/")
                || normalized.Contains("/__pycache__/");
        }
    }
}
'@

$GitServiceCs = @'
using System;
using System.IO;
using System.Text;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelGitCheckpointService
    {
        public RdelGitCheckpoint CreateCheckpoint(string repositoryRoot, string commitMessage, RdelOutputPane pane)
        {
            var checkpoint = new RdelGitCheckpoint
            {
                CommitMessage = commitMessage
            };

            try
            {
                if (!IsGitRepository(repositoryRoot))
                {
                    checkpoint.IsGitRepository = false;
                    pane.WriteLine("Git repository not detected. Skipping checkpoint.");
                    return checkpoint;
                }

                checkpoint.IsGitRepository = true;
                EnsureLocalExcludes(repositoryRoot, pane);

                checkpoint.HeadBefore = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();
                checkpoint.StatusBefore = RunGit(repositoryRoot, "status --porcelain").Output;
                checkpoint.HadChanges = !string.IsNullOrWhiteSpace(checkpoint.StatusBefore);

                if (checkpoint.HadChanges)
                {
                    pane.WriteLine("Git has pending changes. Creating pre-run checkpoint commit.");
                    RunGit(repositoryRoot, "add -A");
                    var commit = RunGit(repositoryRoot, "commit -m \"" + Escape(commitMessage) + "\"");
                    checkpoint.Output = commit.Output;
                    checkpoint.CommitCreated = commit.ExitCode == 0;
                    pane.WriteLine(commit.Output);
                }
                else
                {
                    pane.WriteLine("Git working tree clean. Existing HEAD is pre-run checkpoint.");
                }

                checkpoint.HeadAfter = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();
                checkpoint.StatusAfter = RunGit(repositoryRoot, "status --porcelain").Output;
            }
            catch (Exception ex)
            {
                checkpoint.Error = ex.ToString();
                pane.WriteLine("Git checkpoint failed: " + ex.Message);
            }

            return checkpoint;
        }

        public RdelGitCheckpoint CreatePostApplyCommit(string repositoryRoot, RdelRunRecord record, RdelOutputPane pane)
        {
            string packageName = record.Manifest != null && !string.IsNullOrWhiteSpace(record.Manifest.Name)
                ? record.Manifest.Name
                : record.PackageName;

            string subject = record.ValidationSucceeded
                ? "RDEL apply: " + packageName
                : "RDEL apply with validation issues: " + packageName;

            string body = BuildCommitBody(record);

            var checkpoint = new RdelGitCheckpoint
            {
                CommitMessage = subject,
                CommitBody = body
            };

            try
            {
                if (!IsGitRepository(repositoryRoot))
                {
                    checkpoint.IsGitRepository = false;
                    pane.WriteLine("Git repository not detected. Skipping post-apply commit.");
                    return checkpoint;
                }

                checkpoint.IsGitRepository = true;
                EnsureLocalExcludes(repositoryRoot, pane);

                checkpoint.HeadBefore = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();

                RunGit(repositoryRoot, "add -A");
                checkpoint.StatusBefore = RunGit(repositoryRoot, "status --porcelain").Output;
                checkpoint.HadChanges = !string.IsNullOrWhiteSpace(checkpoint.StatusBefore);

                if (!checkpoint.HadChanges)
                {
                    pane.WriteLine("No Git changes detected after RDEL apply. Skipping post-apply commit.");
                    checkpoint.HeadAfter = checkpoint.HeadBefore;
                    return checkpoint;
                }

                string tempBodyPath = Path.Combine(Path.GetTempPath(), "contollo-rdel-commit-" + Guid.NewGuid().ToString("N") + ".txt");
                File.WriteAllText(tempBodyPath, body, Encoding.UTF8);

                try
                {
                    pane.WriteLine("Creating post-apply RDEL commit.");
                    var commit = RunGit(repositoryRoot, "commit -m \"" + Escape(subject) + "\" -F \"" + tempBodyPath + "\"");
                    checkpoint.Output = commit.Output;
                    checkpoint.CommitCreated = commit.ExitCode == 0;
                    pane.WriteLine(commit.Output);
                }
                finally
                {
                    try { File.Delete(tempBodyPath); } catch { }
                }

                checkpoint.HeadAfter = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();
                checkpoint.StatusAfter = RunGit(repositoryRoot, "status --porcelain").Output;
            }
            catch (Exception ex)
            {
                checkpoint.Error = ex.ToString();
                pane.WriteLine("Post-apply Git commit failed: " + ex.Message);
            }

            return checkpoint;
        }

        private static bool IsGitRepository(string repositoryRoot)
        {
            var inside = RunGit(repositoryRoot, "rev-parse --is-inside-work-tree");
            return inside.ExitCode == 0 && inside.Output.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureLocalExcludes(string repositoryRoot, RdelOutputPane pane)
        {
            string gitDir = RunGit(repositoryRoot, "rev-parse --git-dir").Output.Trim();
            if (string.IsNullOrWhiteSpace(gitDir))
            {
                return;
            }

            if (!Path.IsPathRooted(gitDir))
            {
                gitDir = Path.Combine(repositoryRoot, gitDir);
            }

            string infoDir = Path.Combine(gitDir, "info");
            string excludePath = Path.Combine(infoDir, "exclude");

            Directory.CreateDirectory(infoDir);

            string existing = File.Exists(excludePath)
                ? File.ReadAllText(excludePath)
                : string.Empty;

            var builder = new StringBuilder(existing);
            bool changed = false;

            changed |= AppendExcludeIfMissing(builder, existing, ".contollo/");
            changed |= AppendExcludeIfMissing(builder, existing, "__pycache__/");
            changed |= AppendExcludeIfMissing(builder, existing, "*.pyc");

            if (changed)
            {
                File.WriteAllText(excludePath, builder.ToString(), Encoding.UTF8);
                pane.WriteLine("Updated local Git exclude for RDEL artifacts.");
            }
        }

        private static bool AppendExcludeIfMissing(StringBuilder builder, string existing, string line)
        {
            if (existing.IndexOf(line, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            if (builder.Length > 0 && !builder.ToString().EndsWith(Environment.NewLine))
            {
                builder.AppendLine();
            }

            builder.AppendLine(line);
            return true;
        }

        private static string BuildCommitBody(RdelRunRecord record)
        {
            var builder = new StringBuilder();

            if (record.Manifest != null)
            {
                if (!string.IsNullOrWhiteSpace(record.Manifest.Description))
                {
                    builder.AppendLine(record.Manifest.Description);
                    builder.AppendLine();
                }
            }

            builder.AppendLine("RDEL run:");
            builder.AppendLine("- RunId: " + record.RunId);
            builder.AppendLine("- Package: " + record.PackageName);
            builder.AppendLine("- ApplySucceeded: " + record.ApplySucceeded);
            builder.AppendLine("- ValidationSucceeded: " + record.ValidationSucceeded);
            builder.AppendLine("- Succeeded: " + record.Succeeded);
            builder.AppendLine();

            builder.AppendLine("Applied files:");
            if (record.AppliedFiles != null && record.AppliedFiles.Count > 0)
            {
                foreach (string file in record.AppliedFiles)
                {
                    builder.AppendLine("- " + file);
                }
            }
            else
            {
                builder.AppendLine("- none");
            }

            builder.AppendLine();
            builder.AppendLine("Commands:");
            if (record.Commands != null && record.Commands.Count > 0)
            {
                foreach (var command in record.Commands)
                {
                    builder.AppendLine("- [" + command.ExitCode + "] " + command.Command);
                }
            }
            else
            {
                builder.AppendLine("- none");
            }

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Replace("\"", "\\\"");
        }

        private static RdelCommandResult RunGit(string workingDirectory, string arguments)
        {
            return RdelProcess.Run("git " + arguments, workingDirectory);
        }
    }
}
'@

$ApplyCommandCs = @'
using System;
using System.ComponentModel.Design;
using System.IO;
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
                SolutionRoot = solutionRoot,
                SelectedProjectRoot = selectedProjectRoot,
                RunRoot = runRoot,
                StartedUtc = DateTime.UtcNow
            };

            var git = new RdelGitCheckpointService();

            try
            {
                record.Git = git.CreateCheckpoint(solutionRoot, "RDEL checkpoint: before " + packageName, pane);

                var zipService = new RdelZipPackageService();
                var applyResult = zipService.Apply(zipPath, solutionRoot, selectedProjectRoot, runRoot, pane);

                record.Manifest = applyResult.Manifest;
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

                record.ValidationSucceeded = true;
                foreach (var command in record.Commands)
                {
                    if (command.ExitCode != 0)
                    {
                        record.ValidationSucceeded = false;
                        break;
                    }
                }

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
    }
}
'@

Write-TextFile (Join-Path $ProjectDir "RdelModels.cs") $ModelsCs
Write-TextFile (Join-Path $ProjectDir "RdelPath.cs") $PathCs
Write-TextFile (Join-Path $ProjectDir "RdelGitCheckpointService.cs") $GitServiceCs
Write-TextFile (Join-Path $ProjectDir "ApplyZipUpdateCommand.cs") $ApplyCommandCs

Write-Host "RDEL 0007 applied."
Write-Host ""
Write-Host "Changes:"
Write-Host "  - Splits apply success from validation success."
Write-Host "  - Creates a post-apply local Git commit with a useful commit message/body."
Write-Host "  - Adds local Git excludes for .contollo, __pycache__, and *.pyc."
Write-Host "  - Blocks __pycache__ and *.pyc during package apply."
Write-Host ""
Write-Host "Next:"
Write-Host "  1. Close Experimental Visual Studio."
Write-Host "  2. Clean Solution."
Write-Host "  3. Rebuild Solution."
Write-Host "  4. Press F5."
