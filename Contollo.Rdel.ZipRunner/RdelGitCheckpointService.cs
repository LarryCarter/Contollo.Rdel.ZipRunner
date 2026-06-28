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
                    string tempMessagePath = WriteCommitMessageFile(commitMessage, null);

                    try
                    {
                        var commit = RunGit(repositoryRoot, "commit -F \"" + tempMessagePath + "\"");
                        checkpoint.Output = commit.Output;
                        checkpoint.CommitCreated = commit.ExitCode == 0;
                        pane.WriteLine(commit.Output);
                    }
                    finally
                    {
                        TryDelete(tempMessagePath);
                    }
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

                string tempMessagePath = WriteCommitMessageFile(subject, body);

                try
                {
                    pane.WriteLine("Creating post-apply RDEL commit.");
                    var commit = RunGit(repositoryRoot, "commit -F \"" + tempMessagePath + "\"");
                    checkpoint.Output = commit.Output;
                    checkpoint.CommitCreated = commit.ExitCode == 0;
                    pane.WriteLine(commit.Output);
                }
                finally
                {
                    TryDelete(tempMessagePath);
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
            builder.AppendLine("- PackageSha256: " + record.PackageSha256);
            builder.AppendLine("- ApplySucceeded: " + record.ApplySucceeded);
            builder.AppendLine("- ValidationSucceeded: " + record.ValidationSucceeded);
            builder.AppendLine("- Succeeded: " + record.Succeeded);

            if (record.Verification != null && record.Verification.Notes != null && record.Verification.Notes.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Verification:");
                foreach (string note in record.Verification.Notes)
                {
                    builder.AppendLine("- " + note);
                }
            }

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

        private static string WriteCommitMessageFile(string subject, string body)
        {
            string tempBodyPath = Path.Combine(Path.GetTempPath(), "contollo-rdel-commit-" + Guid.NewGuid().ToString("N") + ".txt");

            var builder = new StringBuilder();
            builder.AppendLine(string.IsNullOrWhiteSpace(subject) ? "RDEL commit" : subject.Trim());

            if (!string.IsNullOrWhiteSpace(body))
            {
                builder.AppendLine();
                builder.AppendLine(body.Trim());
            }

            File.WriteAllText(tempBodyPath, builder.ToString(), Encoding.UTF8);
            return tempBodyPath;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }

        private static RdelCommandResult RunGit(string workingDirectory, string arguments)
        {
            return RdelProcess.Run("git " + arguments, workingDirectory);
        }
    }
}
