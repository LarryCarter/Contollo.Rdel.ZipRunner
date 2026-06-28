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
            result.PackageMetadata = ReadPackageMetadata(payloadRoot, result.Manifest);
            result.TargetRoot = ResolveTargetRoot(solutionRoot, selectedProjectRoot, result.Manifest);
            result.BackupRoot = backupRoot;
            pane.WriteLine("Package extracted to: " + extractRoot);
            pane.WriteLine("Payload root: " + payloadRoot);
            pane.WriteLine("Target root: " + result.TargetRoot);
            WritePackageAwareness(result.PackageMetadata, pane);
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
                    if (Path.IsPathRooted(entry.FullName)) { throw new InvalidOperationException("Blocked absolute zip path: " + entry.FullName); }
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
            metadata.HumanReadmePreview = metadata.HasHumanReadme ? Preview(File.ReadAllText(readmePath)) : null;

            metadata.HasAiContext = File.Exists(contextPath);
            metadata.AiContextPath = metadata.HasAiContext ? contextPath : null;
            metadata.AiContextPreview = metadata.HasAiContext ? Preview(File.ReadAllText(contextPath)) : null;

            return metadata;
        }

        private static void WritePackageAwareness(RdelPackageMetadata metadata, RdelOutputPane pane)
        {
            if (metadata == null) { return; }
            pane.WriteLine("Package metadata:");
            pane.WriteLine("  Manifest: " + metadata.HasManifest);
            pane.WriteLine("  Human README: " + metadata.HasHumanReadme);
            pane.WriteLine("  AI context: " + metadata.HasAiContext);
            if (!string.IsNullOrWhiteSpace(metadata.HumanReadmePreview)) { pane.WriteLine("  README preview: " + metadata.HumanReadmePreview); }
            if (!string.IsNullOrWhiteSpace(metadata.AiContextPreview)) { pane.WriteLine("  Context preview: " + metadata.AiContextPreview); }
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
                if (RdelPath.IsPackageMetadataPath(relativePath)) { pane.WriteLine("Package metadata skipped: " + relativePath); continue; }
                if (RdelPath.IsBlockedPath(relativePath)) { pane.WriteLine("Blocked: " + relativePath); continue; }
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

        private static string Preview(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { return string.Empty; }
            text = text.Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length <= 180 ? text : text.Substring(0, 180) + "...";
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
