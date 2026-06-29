using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelDocumentationOperationService
    {
        private const string DocOpsRoot = ".rdel-docops";

        public void Apply(string payloadRoot, string targetRoot, string backupRoot, RdelManifest manifest, List<string> appliedFiles, RdelOutputPane pane)
        {
            List<RdelDocumentationOperation> operations = DiscoverOperations(payloadRoot, manifest);

            if (operations.Count == 0)
            {
                pane.WriteLine("DocOps: no documentation operations found.");
                return;
            }

            pane.WriteLine("DocOps: applying " + operations.Count + " documentation operation(s).");

            foreach (RdelDocumentationOperation operation in operations)
            {
                ApplyOperation(payloadRoot, targetRoot, backupRoot, operation, appliedFiles, pane);
            }
        }

        public List<RdelDocumentationOperation> DiscoverOperations(string payloadRoot, RdelManifest manifest)
        {
            var operations = new List<RdelDocumentationOperation>();

            if (manifest != null && manifest.DocumentationOperations != null)
            {
                operations.AddRange(manifest.DocumentationOperations.Where(o => o != null));
            }

            AddConventionalOperations(payloadRoot, operations, "context", "docs/context.md");
            AddConventionalOperations(payloadRoot, operations, "memory", "docs/memory.md");
            AddConventionalOperations(payloadRoot, operations, "decisions", "docs/DECISIONS.md");

            return operations
                .Where(o => !string.IsNullOrWhiteSpace(o.Source) && !string.IsNullOrWhiteSpace(o.Target))
                .GroupBy(o => RdelPath.Normalize(o.Source), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }

        private static void AddConventionalOperations(string payloadRoot, List<RdelDocumentationOperation> operations, string folderName, string target)
        {
            string folder = Path.Combine(payloadRoot, DocOpsRoot, folderName);
            if (!Directory.Exists(folder))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(folder, "*.md", SearchOption.TopDirectoryOnly).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                operations.Add(new RdelDocumentationOperation
                {
                    Type = "Append",
                    Source = MakeRelativePath(payloadRoot, file),
                    Target = target,
                    Section = folderName
                });
            }
        }

        private static void ApplyOperation(string payloadRoot, string targetRoot, string backupRoot, RdelDocumentationOperation operation, List<string> appliedFiles, RdelOutputPane pane)
        {
            string type = string.IsNullOrWhiteSpace(operation.Type) ? "Append" : operation.Type;
            if (!type.Equals("Append", StringComparison.OrdinalIgnoreCase))
            {
                pane.WriteLine("DocOps: unsupported operation type skipped: " + type);
                return;
            }

            string sourceRelative = RdelPath.Normalize(operation.Source);
            string targetRelative = RdelPath.Normalize(operation.Target);

            if (!targetRelative.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
            {
                pane.WriteLine("DocOps: blocked target outside docs/: " + targetRelative);
                return;
            }

            if (RdelPath.IsBlockedPath(sourceRelative) || RdelPath.IsBlockedPath(targetRelative))
            {
                pane.WriteLine("DocOps: blocked path: " + sourceRelative + " -> " + targetRelative);
                return;
            }

            string sourcePath = Path.Combine(payloadRoot, sourceRelative.Replace('/', Path.DirectorySeparatorChar));
            string targetPath = Path.Combine(targetRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(sourcePath))
            {
                pane.WriteLine("DocOps: source missing: " + sourceRelative);
                return;
            }

            string appendText = File.ReadAllText(sourcePath).Trim();
            if (string.IsNullOrWhiteSpace(appendText))
            {
                pane.WriteLine("DocOps: empty source skipped: " + sourceRelative);
                return;
            }

            string operationId = !string.IsNullOrWhiteSpace(operation.Id)
                ? operation.Id
                : CreateOperationId(sourceRelative, appendText);

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            string existing = File.Exists(targetPath) ? File.ReadAllText(targetPath) : string.Empty;
            string marker = "<!-- RDEL-DOCOPS-ID: " + operationId + " -->";

            if (existing.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pane.WriteLine("DocOps: already applied: " + operationId);
                return;
            }

            BackupTargetIfNeeded(backupRoot, targetRelative, targetPath);

            using (var writer = new StreamWriter(targetPath, true, new UTF8Encoding(false)))
            {
                if (!string.IsNullOrWhiteSpace(existing) && !existing.EndsWith(Environment.NewLine))
                {
                    writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine(marker);
                writer.WriteLine("<!-- RDEL-DOCOPS-SOURCE: " + sourceRelative + " -->");
                writer.WriteLine("<!-- RDEL-DOCOPS-UTC: " + DateTime.UtcNow.ToString("u") + " -->");
                writer.WriteLine();
                writer.WriteLine(appendText);
                writer.WriteLine();
            }

            appliedFiles.Add(targetRelative + " (DocOps append)");
            pane.WriteLine("DocOps appended: " + sourceRelative + " -> " + targetRelative);
        }

        private static void BackupTargetIfNeeded(string backupRoot, string targetRelative, string targetPath)
        {
            if (!File.Exists(targetPath))
            {
                return;
            }

            string backupPath = Path.Combine(backupRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(backupPath))
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
            File.Copy(targetPath, backupPath, true);
        }

        private static string CreateOperationId(string sourceRelative, string appendText)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sourceRelative + "\n" + appendText);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty).Substring(0, 16);
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
