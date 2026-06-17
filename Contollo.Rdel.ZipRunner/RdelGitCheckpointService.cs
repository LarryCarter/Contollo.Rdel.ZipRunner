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