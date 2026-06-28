using System;
using System.Collections.Generic;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelRunRecord
    {
        public string RunId { get; set; }
        public string PackageName { get; set; }
        public string ZipPath { get; set; }
        public string PackageSha256 { get; set; }
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
        public RdelPackageMetadata PackageMetadata { get; set; }
        public RdelVerificationSummary Verification { get; set; }
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
        // Current RDEL fields. Keep these stable so existing packages keep working.
        public string Name { get; set; }
        public string Description { get; set; }
        public string Target { get; set; }
        public string[] Commands { get; set; }

        // Forward-compatible RDEL 2.0 metadata fields. These are optional and ignored by older packages.
        public string SchemaVersion { get; set; }
        public string PackageType { get; set; }
        public string ChangeIntent { get; set; }
        public string TrustLevel { get; set; }
        public string SourceProvider { get; set; }
        public string HumanReadmePath { get; set; }
        public string AiContextPath { get; set; }
        public RdelBaselineContract Baseline { get; set; }
        public RdelExpectedContract ExpectedAfterApply { get; set; }
        public string[] BlockedFilePatterns { get; set; }
        public string[] RequiredFiles { get; set; }

        public static readonly string[] DefaultCommands =
        {
            "dotnet restore",
            "dotnet build --no-restore",
            "dotnet test --no-build"
        };
    }

    internal sealed class RdelBaselineContract
    {
        public bool BuildMustAlreadyPass { get; set; }
        public int? TestsTotal { get; set; }
        public int? TestsPassed { get; set; }
        public string Notes { get; set; }
    }

    internal sealed class RdelExpectedContract
    {
        public bool BuildMustPass { get; set; }
        public int? TestsPassedMinimum { get; set; }
        public string[] RequiredCommandFragments { get; set; }
        public string[] ForbiddenChangedPaths { get; set; }
        public string Notes { get; set; }
    }

    internal sealed class RdelPackageMetadata
    {
        public bool HasManifest { get; set; }
        public string ManifestPath { get; set; }
        public bool HasHumanReadme { get; set; }
        public string HumanReadmePath { get; set; }
        public string HumanReadmePreview { get; set; }
        public bool HasAiContext { get; set; }
        public string AiContextPath { get; set; }
        public string AiContextPreview { get; set; }
    }

    internal sealed class RdelVerificationSummary
    {
        public bool CommandsSucceeded { get; set; }
        public bool ExpectedBuildPassed { get; set; }
        public bool ExpectedMinimumTestsSatisfied { get; set; }
        public bool ForbiddenChangedPathsClean { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
    }

    internal sealed class RdelApplyResult
    {
        public string TargetRoot { get; set; }
        public string BackupRoot { get; set; }
        public RdelManifest Manifest { get; set; }
        public RdelPackageMetadata PackageMetadata { get; set; }
        public List<string> AppliedFiles { get; set; } = new List<string>();
    }
}
