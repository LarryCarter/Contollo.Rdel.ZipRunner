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