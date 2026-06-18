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

        public bool ApplySucceeded { get; set; }
        public bool ValidationSucceeded { get; set; }
        public bool Succeeded { get; set; }

        public string Error { get; set; }
        public RdelGitCheckpoint Git { get; set; }
        public RdelGitCheckpoint PostApplyGit { get; set; }
        public RdelManifest Manifest { get; set; }
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
        public string Name { get; set; }
        public string Description { get; set; }
        public string Target { get; set; }
        public string[] Commands { get; set; }

        public static readonly string[] DefaultCommands =
        {
            "dotnet restore",
            "dotnet build --no-restore",
            "dotnet test --no-build"
        };
    }

    internal sealed class RdelApplyResult
    {
        public string TargetRoot { get; set; }
        public string BackupRoot { get; set; }
        public RdelManifest Manifest { get; set; }
        public List<string> AppliedFiles { get; set; } = new List<string>();
    }
}