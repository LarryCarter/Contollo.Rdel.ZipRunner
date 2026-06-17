param(
    [string]$RepoRoot = "C:\Users\larry\source\repos\Contollo.Rdel.ZipRunner"
)

$ErrorActionPreference = "Stop"

function Write-TextFile {
    param([string]$Path,[string]$Content)
    $directory = Split-Path -Parent $Path
    if ($directory -and !(Test-Path $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.Encoding]::UTF8)
}

function Add-ProjectLineOnce {
    param([string]$ProjectFile,[string]$Line,[string]$BeforePattern)
    $content = Get-Content $ProjectFile -Raw
    if ($content.Contains($Line)) { return }
    $content = $content -replace [regex]::Escape($BeforePattern), ($Line + "`r`n" + $BeforePattern)
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

function Add-PackageReferenceOnce {
    param([string]$ProjectFile,[string]$PackageLine)
    $content = Get-Content $ProjectFile -Raw
    if ($content.Contains($PackageLine)) { return }
    $anchor = '    <PackageReference Include="Microsoft.VisualStudio.SDK" Version="17.0.32112.339" ExcludeAssets="runtime" NoWarn="NU1604" />'
    $content = $content.Replace($anchor, $PackageLine + "`r`n" + $anchor)
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

function Add-ReferenceOnce {
    param([string]$ProjectFile,[string]$ReferenceLine)
    $content = Get-Content $ProjectFile -Raw
    if ($content.Contains($ReferenceLine)) { return }
    $anchor = '    <Reference Include="System" />'
    $content = $content.Replace($anchor, $anchor + "`r`n" + $ReferenceLine)
    Set-Content -Path $ProjectFile -Value $content -Encoding UTF8
}

$ProjectDir = Join-Path $RepoRoot "Contollo.Rdel.ZipRunner"
$ProjectFile = Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.csproj"
if (!(Test-Path $ProjectFile)) { throw "Could not find project file: $ProjectFile" }
Write-Host "Applying RDEL 0001 to $ProjectDir"

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
        <Strings>
          <ButtonText>Contollo RDEL: Apply Zip Update</ButtonText>
        </Strings>
      </Button>
    </Buttons>
  </Commands>
  <Symbols>
    <GuidSymbol name="guidContolloRdelZipRunnerPackage" value="{a1361a6f-005b-4060-a6ef-3389916ce837}" />
    <GuidSymbol name="guidContolloRdelZipRunnerCommandSet" value="{50fbcc4e-c89d-48ef-aa3a-3c1f8f3f9d4d}">
      <IDSymbol name="ContolloRdelToolsGroup" value="0x1020" />
      <IDSymbol name="ApplyZipUpdateCommandId" value="0x0100" />
    </GuidSymbol>
  </Symbols>
</CommandTable>
'@

$commandCs = @'
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
'@

$outputPaneCs = @'
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelOutputPane
    {
        private static readonly Guid PaneGuid = new Guid("255c735f-979c-4193-9d52-2e39056bf9bb");
        private readonly IVsOutputWindowPane pane;
        private RdelOutputPane(IVsOutputWindowPane pane) { this.pane = pane; }

        public static async Task<RdelOutputPane> CreateAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsOutputWindow outputWindow = await package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
            outputWindow.CreatePane(ref PaneGuid, "Contollo RDEL", 1, 1);
            outputWindow.GetPane(ref PaneGuid, out IVsOutputWindowPane pane);
            pane.Activate();
            return new RdelOutputPane(pane);
        }

        public void WriteLine(string message)
        {
            pane.OutputStringThreadSafe("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        }
    }
}
'@

$modelsCs = @'
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
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public RdelGitCheckpoint Git { get; set; }
        public RdelManifest Manifest { get; set; }
        public List<string> AppliedFiles { get; set; } = new List<string>();
        public List<RdelCommandResult> Commands { get; set; } = new List<RdelCommandResult>();
    }

    internal sealed class RdelGitCheckpoint
    {
        public bool IsGitRepository { get; set; }
        public bool HadChanges { get; set; }
        public string HeadBefore { get; set; }
        public string HeadAfter { get; set; }
        public string CommitMessage { get; set; }
        public string StatusBefore { get; set; }
        public string StatusAfter { get; set; }
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
        public static readonly string[] DefaultCommands = { "dotnet restore", "dotnet build --no-restore", "dotnet test --no-build" };
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

$solutionLocatorCs = @'
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelSolutionLocator
    {
        public static string GetSolutionRoot(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (dte == null || dte.Solution == null || string.IsNullOrWhiteSpace(dte.Solution.FullName)) { return null; }
            return Path.GetDirectoryName(dte.Solution.FullName);
        }

        public static string GetSelectedProjectRoot(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (dte == null || dte.ToolWindows == null) { return null; }
            UIHierarchy solutionExplorer = dte.ToolWindows.SolutionExplorer;
            object[] selectedItems = solutionExplorer.SelectedItems as object[];
            if (selectedItems == null || selectedItems.Length == 0) { return null; }

            foreach (object item in selectedItems)
            {
                UIHierarchyItem hierarchyItem = item as UIHierarchyItem;
                Project project = hierarchyItem?.Object as Project;
                if (project == null)
                {
                    ProjectItem projectItem = hierarchyItem?.Object as ProjectItem;
                    project = projectItem?.ContainingProject;
                }
                if (project != null && !string.IsNullOrWhiteSpace(project.FullName)) { return Path.GetDirectoryName(project.FullName); }
            }
            return null;
        }
    }
}
'@

$pathCs = @'
using System.IO;
using System.Text.RegularExpressions;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelPath
    {
        public static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return "package"; }
            foreach (char c in Path.GetInvalidFileNameChars()) { value = value.Replace(c, '-'); }
            return Regex.Replace(value, @"\s+", "-");
        }

        public static bool IsBlockedPath(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimStart('/');
            return normalized.StartsWith(".git/") || normalized.StartsWith(".vs/") || normalized.StartsWith("bin/") || normalized.StartsWith("obj/") || normalized.StartsWith(".contollo/") || normalized.Contains("/.git/") || normalized.Contains("/.vs/") || normalized.Contains("/bin/") || normalized.Contains("/obj/");
        }
    }
}
'@

$zipServiceCs = @'
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelZipPackageService
    {
        public RdelApplyResult Apply(string zipPath, string solutionRoot, string selectedProjectRoot, string runRoot, RdelOutputPane pane)
        {
            var result = new RdelApplyResult();
            string packageRoot = Path.Combine(runRoot, "package");
            string extractRoot = Path.Combine(packageRoot, "extracted");
            string backupRoot = Path.Combine(runRoot, "backup");
            Directory.CreateDirectory(packageRoot);
            Directory.CreateDirectory(extractRoot);
            Directory.CreateDirectory(backupRoot);
            File.Copy(zipPath, Path.Combine(packageRoot, Path.GetFileName(zipPath)), true);

            SafeExtract(zipPath, extractRoot, pane);
            string payloadRoot = DetectPayloadRoot(extractRoot);
            result.Manifest = ReadManifest(payloadRoot);
            result.TargetRoot = ResolveTargetRoot(solutionRoot, selectedProjectRoot, result.Manifest);
            result.BackupRoot = backupRoot;
            pane.WriteLine("Package extracted to: " + extractRoot);
            pane.WriteLine("Payload root: " + payloadRoot);
            pane.WriteLine("Target root: " + result.TargetRoot);
            ApplyFiles(payloadRoot, result.TargetRoot, backupRoot, result.AppliedFiles, pane);
            return result;
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
                    pane.WriteLine("Extracted: " + entry.FullName);
                }
            }
        }

        private static string DetectPayloadRoot(string extractRoot)
        {
            string[] entries = Directory.GetFileSystemEntries(extractRoot);
            if (entries.Length == 1 && Directory.Exists(entries[0])) { return entries[0]; }
            return extractRoot;
        }

        private static RdelManifest ReadManifest(string payloadRoot)
        {
            string manifestPath = Path.Combine(payloadRoot, "contollo-rdel.json");
            if (!File.Exists(manifestPath))
            {
                return new RdelManifest { Name = Path.GetFileName(payloadRoot), Description = "No contollo-rdel.json manifest found.", Target = "solution", Commands = RdelManifest.DefaultCommands };
            }
            string json = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<RdelManifest>(json);
            if (manifest.Commands == null || manifest.Commands.Length == 0) { manifest.Commands = RdelManifest.DefaultCommands; }
            return manifest;
        }

        private static string ResolveTargetRoot(string solutionRoot, string selectedProjectRoot, RdelManifest manifest)
        {
            string target = manifest?.Target ?? "solution";
            if (target.Equals("selected-project", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(selectedProjectRoot)) { return selectedProjectRoot; }
            return solutionRoot;
        }

        private static void ApplyFiles(string payloadRoot, string targetRoot, string backupRoot, List<string> appliedFiles, RdelOutputPane pane)
        {
            foreach (string sourceFile in Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories))
            {
                string relativePath = MakeRelativePath(payloadRoot, sourceFile);
                if (relativePath.Equals("contollo-rdel.json", StringComparison.OrdinalIgnoreCase) || relativePath.Equals("contollo-rdel.txt", StringComparison.OrdinalIgnoreCase) || RdelPath.IsBlockedPath(relativePath)) { pane.WriteLine("Skipped: " + relativePath); continue; }
                string destinationFile = Path.Combine(targetRoot, relativePath);
                string backupFile = Path.Combine(backupRoot, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                if (File.Exists(destinationFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                    File.Copy(destinationFile, backupFile, true);
                    pane.WriteLine("Backed up: " + relativePath);
                }
                File.Copy(sourceFile, destinationFile, true);
                appliedFiles.Add(relativePath);
                pane.WriteLine("Applied: " + relativePath);
            }
        }

        private static string MakeRelativePath(string root, string file)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
            Uri fileUri = new Uri(file);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }
    }
}
'@

$gitServiceCs = @'
namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelGitCheckpointService
    {
        public RdelGitCheckpoint CreateCheckpoint(string repositoryRoot, string commitMessage, RdelOutputPane pane)
        {
            var checkpoint = new RdelGitCheckpoint { CommitMessage = commitMessage };
            try
            {
                var inside = RunGit(repositoryRoot, "rev-parse --is-inside-work-tree");
                checkpoint.IsGitRepository = inside.ExitCode == 0 && inside.Output.Trim().Equals("true");
                if (!checkpoint.IsGitRepository) { pane.WriteLine("Git repository not detected. Skipping checkpoint."); return checkpoint; }
                checkpoint.HeadBefore = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();
                checkpoint.StatusBefore = RunGit(repositoryRoot, "status --porcelain").Output;
                checkpoint.HadChanges = !string.IsNullOrWhiteSpace(checkpoint.StatusBefore);
                if (checkpoint.HadChanges)
                {
                    pane.WriteLine("Git has pending changes. Creating checkpoint commit.");
                    RunGit(repositoryRoot, "add -A");
                    var commit = RunGit(repositoryRoot, "commit -m \"" + Escape(commitMessage) + "\"");
                    pane.WriteLine(commit.Output);
                }
                else { pane.WriteLine("Git working tree clean. Existing HEAD is checkpoint."); }
                checkpoint.HeadAfter = RunGit(repositoryRoot, "rev-parse HEAD").Output.Trim();
                checkpoint.StatusAfter = RunGit(repositoryRoot, "status --porcelain").Output;
            }
            catch (System.Exception ex)
            {
                checkpoint.Error = ex.ToString();
                pane.WriteLine("Git checkpoint failed: " + ex.Message);
            }
            return checkpoint;
        }
        private static string Escape(string value) { return value.Replace("\"", "\\\""); }
        private static RdelCommandResult RunGit(string workingDirectory, string arguments) { return RdelProcess.Run("git " + arguments, workingDirectory); }
    }
}
'@

$runnerCs = @'
namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelCommandRunner
    {
        public RdelCommandResult Run(string command, string workingDirectory, RdelOutputPane pane)
        {
            pane.WriteLine("Running: " + command);
            var result = RdelProcess.Run(command, workingDirectory);
            pane.WriteLine(result.Output);
            pane.WriteLine("Exit code: " + result.ExitCode);
            return result;
        }
    }
}
'@

$processCs = @'
using System;
using System.Diagnostics;
using System.Text;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelProcess
    {
        public static RdelCommandResult Run(string command, string workingDirectory)
        {
            var result = new RdelCommandResult { Command = command, WorkingDirectory = workingDirectory, StartedUtc = DateTime.UtcNow };
            var output = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) => { if (args.Data != null) { output.AppendLine(args.Data); } };
                process.ErrorDataReceived += (sender, args) => { if (args.Data != null) { output.AppendLine(args.Data); } };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                result.ExitCode = process.ExitCode;
                result.CompletedUtc = DateTime.UtcNow;
                result.Output = output.ToString();
            }
            return result;
        }
    }
}
'@

$historyCs = @'
using System.IO;
using Newtonsoft.Json;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelHistoryWriter
    {
        public static void Write(string solutionRoot, RdelRunRecord record)
        {
            Directory.CreateDirectory(record.RunRoot);
            string historyPath = Path.Combine(record.RunRoot, "run-history.json");
            string logPath = Path.Combine(record.RunRoot, "run-output.log");
            string indexPath = Path.Combine(solutionRoot, ".contollo", "rdel", "history", "index.jsonl");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath));
            File.WriteAllText(historyPath, JsonConvert.SerializeObject(record, Formatting.Indented));
            using (var writer = new StreamWriter(logPath, false))
            {
                writer.WriteLine("Contollo RDEL Run: " + record.RunId);
                writer.WriteLine("Package: " + record.PackageName);
                writer.WriteLine("Target: " + record.TargetRoot);
                writer.WriteLine("Succeeded: " + record.Succeeded);
                writer.WriteLine();
                foreach (var command in record.Commands)
                {
                    writer.WriteLine("COMMAND: " + command.Command);
                    writer.WriteLine("WORKING DIRECTORY: " + command.WorkingDirectory);
                    writer.WriteLine("EXIT CODE: " + command.ExitCode);
                    writer.WriteLine(command.Output);
                    writer.WriteLine();
                }
                if (!string.IsNullOrWhiteSpace(record.Error)) { writer.WriteLine("ERROR:"); writer.WriteLine(record.Error); }
            }
            File.AppendAllText(indexPath, JsonConvert.SerializeObject(new { record.RunId, record.PackageName, record.StartedUtc, record.CompletedUtc, record.Succeeded, record.TargetRoot, HistoryPath = historyPath }) + System.Environment.NewLine);
        }
    }
}
'@

Write-TextFile (Join-Path $ProjectDir "Contollo.Rdel.ZipRunnerPackage.cs") $packageCs
Write-TextFile (Join-Path $ProjectDir "Contollo.Rdel.ZipRunner.vsct") $vsct
Write-TextFile (Join-Path $ProjectDir "ApplyZipUpdateCommand.cs") $commandCs
Write-TextFile (Join-Path $ProjectDir "RdelOutputPane.cs") $outputPaneCs
Write-TextFile (Join-Path $ProjectDir "RdelModels.cs") $modelsCs
Write-TextFile (Join-Path $ProjectDir "RdelSolutionLocator.cs") $solutionLocatorCs
Write-TextFile (Join-Path $ProjectDir "RdelPath.cs") $pathCs
Write-TextFile (Join-Path $ProjectDir "RdelZipPackageService.cs") $zipServiceCs
Write-TextFile (Join-Path $ProjectDir "RdelGitCheckpointService.cs") $gitServiceCs
Write-TextFile (Join-Path $ProjectDir "RdelCommandRunner.cs") $runnerCs
Write-TextFile (Join-Path $ProjectDir "RdelProcess.cs") $processCs
Write-TextFile (Join-Path $ProjectDir "RdelHistoryWriter.cs") $historyCs

Add-ProjectLineOnce $ProjectFile '    <Compile Include="ApplyZipUpdateCommand.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelOutputPane.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelModels.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelSolutionLocator.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelPath.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelZipPackageService.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelGitCheckpointService.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelCommandRunner.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelProcess.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'
Add-ProjectLineOnce $ProjectFile '    <Compile Include="RdelHistoryWriter.cs" />' '    <Compile Include="Contollo.Rdel.ZipRunnerPackage.cs" />'

Add-ProjectLineOnce $ProjectFile @'
  <ItemGroup>
    <VSCTCompile Include="Contollo.Rdel.ZipRunner.vsct">
      <ResourceName>Menus.ctmenu</ResourceName>
    </VSCTCompile>
  </ItemGroup>
'@ '  <ItemGroup>
    <Reference Include="System" />
  </ItemGroup>'

Add-ReferenceOnce $ProjectFile '    <Reference Include="EnvDTE" />'
Add-ReferenceOnce $ProjectFile '    <Reference Include="EnvDTE80" />'
Add-ReferenceOnce $ProjectFile '    <Reference Include="System.IO.Compression" />'
Add-ReferenceOnce $ProjectFile '    <Reference Include="System.IO.Compression.FileSystem" />'
Add-ReferenceOnce $ProjectFile '    <Reference Include="System.Windows.Forms" />'
Add-PackageReferenceOnce $ProjectFile '    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />'

Write-Host "RDEL 0001 files written."
Write-Host "Next: Reload project if asked, restore packages, clean, rebuild, press F5."
Write-Host "Experimental VS: Tools -> Contollo RDEL: Apply Zip Update."
