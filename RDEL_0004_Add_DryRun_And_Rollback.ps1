param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

function Write-TextFile {
    param([string]$Path, [string]$Content)
    $directory = Split-Path -Parent $Path
    if ($directory -and !(Test-Path $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.Encoding]::UTF8)
}

function Add-CompileOnce {
    param([string]$ProjectFile, [string]$CompileLine)
    $content = Get-Content $ProjectFile -Raw
    if ($content.Contains($CompileLine)) { return }
    $anchor = '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
    if (!$content.Contains($anchor)) { throw "Could not find compile anchor in project file." }
    $content = $content.Replace($anchor, $CompileLine + "`r`n" + $anchor)
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$ProjectFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.csproj"
if (!(Test-Path $ProjectFile)) { throw "Could not find project file: $ProjectFile" }

Write-Host "Applying RDEL 0004: Dry Run and Rollback commands"

$packageCs = @'
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
            await DryRunZipUpdateCommand.InitializeAsync(this);
            await RollbackLastRunCommand.InitializeAsync(this);
        }
    }
}
'@

$vsct = @'
<?xml version="1.0" encoding="utf-8"?>
<CommandTable xmlns="http://schemas.microsoft.com/VisualStudio/2005-10-18/CommandTable" xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <Extern href="stdidcmd.h"/>
  <Extern href="vsshlids.h"/>
  <Commands package="guidContolloRdelZipRunnerPackage">
    <Groups>
      <Group guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" priority="0x0600">
        <Parent guid="guidSHLMainMenu" id="IDM_VS_MENU_TOOLS"/>
      </Group>
    </Groups>
    <Buttons>
      <Button guid="guidContolloRdelZipRunnerCommandSet" id="ApplyZipUpdateCommandId" priority="0x0100" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Icon guid="guidImages" id="bmpPic1" />
        <Strings><ButtonText>Contollo RDEL: Apply Zip Update</ButtonText></Strings>
      </Button>
      <Button guid="guidContolloRdelZipRunnerCommandSet" id="DryRunZipUpdateCommandId" priority="0x0110" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Icon guid="guidImages" id="bmpPic1" />
        <Strings><ButtonText>Contollo RDEL: Dry Run Zip Update</ButtonText></Strings>
      </Button>
      <Button guid="guidContolloRdelZipRunnerCommandSet" id="RollbackLastRunCommandId" priority="0x0120" type="Button">
        <Parent guid="guidContolloRdelZipRunnerCommandSet" id="ContolloRdelToolsGroup" />
        <Icon guid="guidImages" id="bmpPic1" />
        <Strings><ButtonText>Contollo RDEL: Rollback Last Run</ButtonText></Strings>
      </Button>
    </Buttons>
    <Bitmaps>
      <Bitmap guid="guidImages" href="Resources\Images.png" usedList="bmpPic1"/>
    </Bitmaps>
  </Commands>
  <Symbols>
    <GuidSymbol name="guidContolloRdelZipRunnerPackage" value="{a1361a6f-005b-4060-a6ef-3389916ce837}" />
    <GuidSymbol name="guidContolloRdelZipRunnerCommandSet" value="{50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d}">
      <IDSymbol name="ContolloRdelToolsGroup" value="0x1020" />
      <IDSymbol name="ApplyZipUpdateCommandId" value="0x0100" />
      <IDSymbol name="DryRunZipUpdateCommandId" value="0x0101" />
      <IDSymbol name="RollbackLastRunCommandId" value="0x0102" />
    </GuidSymbol>
    <GuidSymbol name="guidImages" value="{2b860d9d-378c-4817-96ec-4a1c3d8cecd1}">
      <IDSymbol name="bmpPic1" value="1" />
    </GuidSymbol>
  </Symbols>
</CommandTable>
'@

$dryRunCs = @'
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
'@

$rollbackCs = @'
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
'@

$dryRunServiceCs = @'
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelDryRunService
    {
        public RdelDryRunResult Analyze(string zipPath, string solutionRoot, string selectedProjectRoot, string runRoot, RdelOutputPane pane)
        {
            var result = new RdelDryRunResult();
            string packageRoot = Path.Combine(runRoot, "package");
            string extractRoot = Path.Combine(packageRoot, "extracted");
            Directory.CreateDirectory(packageRoot);
            Directory.CreateDirectory(extractRoot);
            File.Copy(zipPath, Path.Combine(packageRoot, Path.GetFileName(zipPath)), true);
            SafeExtract(zipPath, extractRoot, pane);

            string payloadRoot = DetectPayloadRoot(extractRoot);
            result.Manifest = ReadManifest(payloadRoot);
            result.TargetRoot = ResolveTargetRoot(solutionRoot, selectedProjectRoot, result.Manifest);
            result.PayloadRoot = payloadRoot;

            pane.WriteLine("Dry run payload root: " + payloadRoot);
            pane.WriteLine("Dry run target root: " + result.TargetRoot);

            foreach (string sourceFile in Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories))
            {
                string relativePath = MakeRelativePath(payloadRoot, sourceFile);
                if (relativePath.Equals("contollo-rdel.json", StringComparison.OrdinalIgnoreCase) || relativePath.Equals("contollo-rdel.txt", StringComparison.OrdinalIgnoreCase))
                {
                    result.SkippedFiles.Add(relativePath);
                    continue;
                }

                if (RdelPath.IsBlockedPath(relativePath))
                {
                    result.BlockedFiles.Add(relativePath);
                    pane.WriteLine("Blocked: " + relativePath);
                    continue;
                }

                string destinationFile = Path.Combine(result.TargetRoot, relativePath);
                if (File.Exists(destinationFile))
                {
                    result.WouldOverwriteFiles.Add(relativePath);
                    pane.WriteLine("Would overwrite: " + relativePath);
                }
                else
                {
                    result.WouldCreateFiles.Add(relativePath);
                    pane.WriteLine("Would create: " + relativePath);
                }
                result.WouldApplyFiles.Add(relativePath);
            }

            string[] commands = result.Manifest != null && result.Manifest.Commands != null && result.Manifest.Commands.Length > 0 ? result.Manifest.Commands : RdelManifest.DefaultCommands;
            result.Commands.AddRange(commands);
            foreach (string command in commands) { pane.WriteLine("Would run: " + command); }
            return result;
        }

        public void WriteReport(string reportPath, RdelDryRunResult result)
        {
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("Contollo RDEL Dry Run Report");
                writer.WriteLine("============================");
                writer.WriteLine();
                writer.WriteLine("Payload Root: " + result.PayloadRoot);
                writer.WriteLine("Target Root: " + result.TargetRoot);
                writer.WriteLine();
                writer.WriteLine("Would Create:");
                foreach (string file in result.WouldCreateFiles) { writer.WriteLine("  + " + file); }
                writer.WriteLine();
                writer.WriteLine("Would Overwrite:");
                foreach (string file in result.WouldOverwriteFiles) { writer.WriteLine("  * " + file); }
                writer.WriteLine();
                writer.WriteLine("Blocked:");
                foreach (string file in result.BlockedFiles) { writer.WriteLine("  ! " + file); }
                writer.WriteLine();
                writer.WriteLine("Skipped:");
                foreach (string file in result.SkippedFiles) { writer.WriteLine("  - " + file); }
                writer.WriteLine();
                writer.WriteLine("Commands:");
                foreach (string command in result.Commands) { writer.WriteLine("  > " + command); }
            }
        }

        private static void SafeExtract(string zipPath, string extractRoot, RdelOutputPane pane)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractRoot, entry.FullName));
                    string extractRootFull = Path.GetFullPath(extractRoot);
                    if (!destinationPath.StartsWith(extractRootFull, StringComparison.OrdinalIgnoreCase)) { throw new InvalidOperationException("Blocked unsafe zip path: " + entry.FullName); }
                    if (string.IsNullOrEmpty(entry.Name)) { Directory.CreateDirectory(destinationPath); continue; }
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    entry.ExtractToFile(destinationPath, true);
                    pane.WriteLine("Dry-run extracted: " + entry.FullName);
                }
            }
        }

        private static string DetectPayloadRoot(string extractRoot)
        {
            string[] entries = Directory.GetFileSystemEntries(extractRoot);
            return entries.Length == 1 && Directory.Exists(entries[0]) ? entries[0] : extractRoot;
        }

        private static RdelManifest ReadManifest(string payloadRoot)
        {
            string manifestPath = Path.Combine(payloadRoot, "contollo-rdel.json");
            if (!File.Exists(manifestPath))
            {
                return new RdelManifest { Name = Path.GetFileName(payloadRoot), Description = "No contollo-rdel.json manifest found.", Target = "solution", Commands = RdelManifest.DefaultCommands };
            }
            var manifest = JsonConvert.DeserializeObject<RdelManifest>(File.ReadAllText(manifestPath));
            if (manifest.Commands == null || manifest.Commands.Length == 0) { manifest.Commands = RdelManifest.DefaultCommands; }
            return manifest;
        }

        private static string ResolveTargetRoot(string solutionRoot, string selectedProjectRoot, RdelManifest manifest)
        {
            string target = manifest?.Target ?? "solution";
            if (target.Equals("selected-project", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(selectedProjectRoot)) { return selectedProjectRoot; }
            return solutionRoot;
        }

        private static string MakeRelativePath(string root, string file)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
            Uri fileUri = new Uri(file);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
        private static string AppendDirectorySeparatorChar(string path) { return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar; }
    }

    internal sealed class RdelDryRunResult
    {
        public string PayloadRoot { get; set; }
        public string TargetRoot { get; set; }
        public RdelManifest Manifest { get; set; }
        public List<string> WouldApplyFiles { get; set; } = new List<string>();
        public List<string> WouldCreateFiles { get; set; } = new List<string>();
        public List<string> WouldOverwriteFiles { get; set; } = new List<string>();
        public List<string> BlockedFiles { get; set; } = new List<string>();
        public List<string> SkippedFiles { get; set; } = new List<string>();
        public List<string> Commands { get; set; } = new List<string>();
    }
}
'@

$rollbackServiceCs = @'
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelRollbackService
    {
        public string GetRollbackPreview(string solutionRoot)
        {
            RdelRunRecord record = GetLastRollbackableRun(solutionRoot);
            if (record == null) { return null; }
            string checkpoint = record.Git != null ? record.Git.HeadBefore : null;
            return "Last RDEL run:" + Environment.NewLine
                + "Run Id: " + record.RunId + Environment.NewLine
                + "Package: " + record.PackageName + Environment.NewLine
                + "Succeeded: " + record.Succeeded + Environment.NewLine
                + "Target: " + record.TargetRoot + Environment.NewLine
                + "Checkpoint: " + checkpoint + Environment.NewLine
                + "Backup: " + record.BackupRoot;
        }

        public string RollbackLastRun(string solutionRoot, RdelOutputPane pane)
        {
            RdelRunRecord record = GetLastRollbackableRun(solutionRoot);
            if (record == null) { throw new InvalidOperationException("No rollbackable RDEL run found."); }
            string report = "Rolling back RDEL run " + record.RunId + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(record.BackupRoot) && Directory.Exists(record.BackupRoot))
            {
                RestoreBackupFiles(record, pane);
                report += "Restored backup files from: " + record.BackupRoot + Environment.NewLine;
            }

            if (record.AppliedFiles != null && !string.IsNullOrWhiteSpace(record.TargetRoot))
            {
                foreach (string relativePath in record.AppliedFiles)
                {
                    string targetFile = Path.Combine(record.TargetRoot, relativePath);
                    string backupFile = !string.IsNullOrWhiteSpace(record.BackupRoot) ? Path.Combine(record.BackupRoot, relativePath) : null;
                    if (!File.Exists(backupFile) && File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                        pane.WriteLine("Deleted newly created file: " + relativePath);
                    }
                }
            }

            if (record.Git != null && record.Git.IsGitRepository && !string.IsNullOrWhiteSpace(record.Git.HeadBefore))
            {
                pane.WriteLine("Creating rollback commit.");
                RdelProcess.Run("git add -A", solutionRoot);
                RdelProcess.Run("git commit -m \"RDEL rollback: " + Escape(record.PackageName) + "\"", solutionRoot);
                report += "Created rollback commit. Original checkpoint was: " + record.Git.HeadBefore + Environment.NewLine;
                report += "Manual hard reset option: git reset --hard " + record.Git.HeadBefore + Environment.NewLine;
            }

            MarkRunRolledBack(record);
            return report;
        }

        private static void RestoreBackupFiles(RdelRunRecord record, RdelOutputPane pane)
        {
            foreach (string backupFile in Directory.GetFiles(record.BackupRoot, "*", SearchOption.AllDirectories))
            {
                string relativePath = MakeRelativePath(record.BackupRoot, backupFile);
                string targetFile = Path.Combine(record.TargetRoot, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(backupFile, targetFile, true);
                pane.WriteLine("Restored: " + relativePath);
            }
        }

        private static RdelRunRecord GetLastRollbackableRun(string solutionRoot)
        {
            string runsRoot = Path.Combine(solutionRoot, ".contollo", "rdel", "runs");
            if (!Directory.Exists(runsRoot)) { return null; }
            foreach (string runDir in Directory.GetDirectories(runsRoot).OrderByDescending(x => x))
            {
                string historyPath = Path.Combine(runDir, "run-history.json");
                string rolledBackMarker = Path.Combine(runDir, "rolled-back.txt");
                if (!File.Exists(historyPath) || File.Exists(rolledBackMarker)) { continue; }
                var record = JsonConvert.DeserializeObject<RdelRunRecord>(File.ReadAllText(historyPath));
                if (record == null || (record.RunId != null && record.RunId.Contains("dry-run"))) { continue; }
                if (record.Succeeded && record.AppliedFiles != null && record.AppliedFiles.Count > 0) { return record; }
            }
            return null;
        }

        private static void MarkRunRolledBack(RdelRunRecord record)
        {
            File.WriteAllText(Path.Combine(record.RunRoot, "rolled-back.txt"), "Rolled back at UTC " + DateTime.UtcNow.ToString("O"));
        }

        private static string Escape(string value) { return value == null ? string.Empty : value.Replace("\"", "\\\""); }
        private static string MakeRelativePath(string root, string file)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
            Uri fileUri = new Uri(file);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
        private static string AppendDirectorySeparatorChar(string path) { return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar; }
    }
}
'@

Write-TextFile (Join-Path $ProjectDir "Contollo.Rdel.ZipRunnerPackage.cs") $packageCs
Write-TextFile (Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.vsct") $vsct
Write-TextFile (Join-Path $ProjectDir "DryRunZipUpdateCommand.cs") $dryRunCs
Write-TextFile (Join-Path $ProjectDir "RollbackLastRunCommand.cs") $rollbackCs
Write-TextFile (Join-Path $ProjectDir "RdelDryRunService.cs") $dryRunServiceCs
Write-TextFile (Join-Path $ProjectDir "RdelRollbackService.cs") $rollbackServiceCs

Add-CompileOnce $ProjectFile '    <Compile Include="DryRunZipUpdateCommand.cs" />'
Add-CompileOnce $ProjectFile '    <Compile Include="RollbackLastRunCommand.cs" />'
Add-CompileOnce $ProjectFile '    <Compile Include="RdelDryRunService.cs" />'
Add-CompileOnce $ProjectFile '    <Compile Include="RdelRollbackService.cs" />'

Write-Host "RDEL 0004 applied."
Write-Host "Next: reload project if prompted, Clean Solution, Rebuild Solution, press F5."
Write-Host "Experimental VS Tools menu should include Apply, Dry Run, and Rollback commands."
