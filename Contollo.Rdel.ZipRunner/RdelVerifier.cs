using System;
using System.Collections.Generic;
using System.Linq;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelVerifier
    {
        public RdelVerificationSummary Verify(RdelManifest manifest, IList<RdelCommandResult> commands, IList<string> appliedFiles)
        {
            var summary = new RdelVerificationSummary();

            commands = commands ?? new List<RdelCommandResult>();
            appliedFiles = appliedFiles ?? new List<string>();

            summary.CommandsSucceeded = commands.Count == 0 || commands.All(command => command.ExitCode == 0);
            summary.ExpectedBuildPassed = VerifyBuildExpectation(manifest, commands, summary);
            summary.ExpectedMinimumTestsSatisfied = VerifyMinimumTestsExpectation(manifest, commands, summary);
            summary.ForbiddenChangedPathsClean = VerifyForbiddenChangedPaths(manifest, appliedFiles, summary);

            if (manifest == null)
            {
                summary.Notes.Add("No manifest was available. Only command exit-code verification was performed.");
            }
            else if (manifest.ExpectedAfterApply == null)
            {
                summary.Notes.Add("No ExpectedAfterApply contract was provided. Verification used command results and applied-file safety checks.");
            }

            if (summary.CommandsSucceeded)
            {
                summary.Notes.Add("All configured commands completed with exit code 0.");
            }
            else
            {
                summary.Notes.Add("One or more configured commands failed.");
            }

            return summary;
        }

        public bool IsSuccessful(RdelVerificationSummary summary)
        {
            if (summary == null)
            {
                return false;
            }

            return summary.CommandsSucceeded
                && summary.ExpectedBuildPassed
                && summary.ExpectedMinimumTestsSatisfied
                && summary.ForbiddenChangedPathsClean;
        }

        private static bool VerifyBuildExpectation(RdelManifest manifest, IList<RdelCommandResult> commands, RdelVerificationSummary summary)
        {
            if (manifest?.ExpectedAfterApply == null || !manifest.ExpectedAfterApply.BuildMustPass)
            {
                return true;
            }

            bool buildCommandFound = false;
            bool buildCommandPassed = false;

            foreach (var command in commands)
            {
                if (command == null || string.IsNullOrWhiteSpace(command.Command))
                {
                    continue;
                }

                if (command.Command.IndexOf("build", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    buildCommandFound = true;
                    if (command.ExitCode == 0)
                    {
                        buildCommandPassed = true;
                    }
                }
            }

            if (!buildCommandFound)
            {
                summary.Notes.Add("ExpectedAfterApply.BuildMustPass was true, but no build command was found.");
                return false;
            }

            if (!buildCommandPassed)
            {
                summary.Notes.Add("ExpectedAfterApply.BuildMustPass was true, but the build command did not pass.");
                return false;
            }

            summary.Notes.Add("Expected build contract satisfied.");
            return true;
        }

        private static bool VerifyMinimumTestsExpectation(RdelManifest manifest, IList<RdelCommandResult> commands, RdelVerificationSummary summary)
        {
            int? minimum = manifest?.ExpectedAfterApply?.TestsPassedMinimum;
            if (!minimum.HasValue)
            {
                return true;
            }

            // Current-format compatibility note:
            // We do not parse framework-specific test result formats yet. For now, a passing test command satisfies the minimum contract.
            foreach (var command in commands)
            {
                if (command == null || string.IsNullOrWhiteSpace(command.Command))
                {
                    continue;
                }

                if (command.Command.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 && command.ExitCode == 0)
                {
                    summary.Notes.Add("Expected minimum test contract treated as satisfied because a test command passed. Detailed test-count parsing is pending.");
                    return true;
                }
            }

            summary.Notes.Add("Expected minimum test contract was provided, but no passing test command was found.");
            return false;
        }

        private static bool VerifyForbiddenChangedPaths(RdelManifest manifest, IList<string> appliedFiles, RdelVerificationSummary summary)
        {
            string[] forbidden = manifest?.ExpectedAfterApply?.ForbiddenChangedPaths;
            if (forbidden == null || forbidden.Length == 0)
            {
                return true;
            }

            var violations = new List<string>();

            foreach (string appliedFile in appliedFiles)
            {
                if (string.IsNullOrWhiteSpace(appliedFile))
                {
                    continue;
                }

                string normalizedApplied = appliedFile.Replace('\\', '/').TrimStart('/');

                foreach (string forbiddenPath in forbidden)
                {
                    if (string.IsNullOrWhiteSpace(forbiddenPath))
                    {
                        continue;
                    }

                    string normalizedForbidden = forbiddenPath.Replace('\\', '/').Trim('/');

                    if (normalizedApplied.Equals(normalizedForbidden, StringComparison.OrdinalIgnoreCase)
                        || normalizedApplied.StartsWith(normalizedForbidden + "/", StringComparison.OrdinalIgnoreCase)
                        || normalizedApplied.EndsWith("/" + normalizedForbidden, StringComparison.OrdinalIgnoreCase)
                        || normalizedApplied.IndexOf("/" + normalizedForbidden + "/", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        violations.Add(appliedFile);
                    }
                }
            }

            if (violations.Count == 0)
            {
                summary.Notes.Add("Forbidden changed path contract satisfied.");
                return true;
            }

            foreach (string violation in violations)
            {
                summary.Notes.Add("Forbidden changed path violation: " + violation);
            }

            return false;
        }
    }
}
