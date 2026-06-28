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
            result.PackageMetadata = ReadPackageMetadata(payloadRoot, result.Manifest);

            pane.WriteLine("Dry run payload root: " + payloadRoot);
            pane.WriteLine("Dry run target root: " + result.TargetRoot);
            pane.WriteLine("Dry run package metadata: README=" + result.PackageMetadata.HasHumanReadme + ", context=" + result.PackageMetadata.HasAiContext);

            foreach (string sourceFile in Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories))
            {
                string relativePath = MakeRelativePath(payloadRoot, sourceFile);
                if (RdelPath.IsPackageMetadataPath(relativePath))
                {
                    result.SkippedFiles.Add(relativePath);
                    pane.WriteLine("Metadata skipped: " + relativePath);
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
                if (result.PackageMetadata != null)
                {
                    writer.WriteLine("Package Metadata:");
                    writer.WriteLine("  Manifest: " + result.PackageMetadata.HasManifest);
                    writer.WriteLine("  Human README: " + result.PackageMetadata.HasHumanReadme);
                    writer.WriteLine("  AI Context: " + result.PackageMetadata.HasAiContext);
                    writer.WriteLine();
                }
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
                    if (Path.IsPathRooted(entry.FullName)) { throw new InvalidOperationException("Blocked absolute zip path: " + entry.FullName); }
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

        private static RdelPackageMetadata ReadPackageMetadata(string payloadRoot, RdelManifest manifest)
        {
            var metadata = new RdelPackageMetadata();
            string manifestPath = Path.Combine(payloadRoot, "contollo-rdel.json");
            metadata.HasManifest = File.Exists(manifestPath);
            metadata.ManifestPath = metadata.HasManifest ? manifestPath : null;
            string readmeRelative = !string.IsNullOrWhiteSpace(manifest?.HumanReadmePath) ? manifest.HumanReadmePath : "README.md";
            string contextRelative = !string.IsNullOrWhiteSpace(manifest?.AiContextPath) ? manifest.AiContextPath : "context.md";
            string readmePath = Path.Combine(payloadRoot, readmeRelative);
            string contextPath = Path.Combine(payloadRoot, contextRelative);
            metadata.HasHumanReadme = File.Exists(readmePath);
            metadata.HumanReadmePath = metadata.HasHumanReadme ? readmePath : null;
            metadata.HasAiContext = File.Exists(contextPath);
            metadata.AiContextPath = metadata.HasAiContext ? contextPath : null;
            return metadata;
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
        public RdelPackageMetadata PackageMetadata { get; set; }
        public List<string> WouldApplyFiles { get; set; } = new List<string>();
        public List<string> WouldCreateFiles { get; set; } = new List<string>();
        public List<string> WouldOverwriteFiles { get; set; } = new List<string>();
        public List<string> BlockedFiles { get; set; } = new List<string>();
        public List<string> SkippedFiles { get; set; } = new List<string>();
        public List<string> Commands { get; set; } = new List<string>();
    }
}
