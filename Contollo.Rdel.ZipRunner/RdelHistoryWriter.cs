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
                writer.WriteLine("Package SHA256: " + record.PackageSha256);
                writer.WriteLine("Target: " + record.TargetRoot);
                writer.WriteLine("Succeeded: " + record.Succeeded);
                writer.WriteLine();

                if (record.Manifest != null)
                {
                    writer.WriteLine("Manifest:");
                    writer.WriteLine("  Name: " + record.Manifest.Name);
                    writer.WriteLine("  Description: " + record.Manifest.Description);
                    writer.WriteLine("  Target: " + record.Manifest.Target);
                    writer.WriteLine("  SchemaVersion: " + record.Manifest.SchemaVersion);
                    writer.WriteLine("  PackageType: " + record.Manifest.PackageType);
                    writer.WriteLine("  ChangeIntent: " + record.Manifest.ChangeIntent);
                    writer.WriteLine("  TrustLevel: " + record.Manifest.TrustLevel);
                    writer.WriteLine("  SourceProvider: " + record.Manifest.SourceProvider);
                    writer.WriteLine();
                }

                if (record.PackageMetadata != null)
                {
                    writer.WriteLine("Package Metadata:");
                    writer.WriteLine("  HasManifest: " + record.PackageMetadata.HasManifest);
                    writer.WriteLine("  HasHumanReadme: " + record.PackageMetadata.HasHumanReadme);
                    writer.WriteLine("  HasAiContext: " + record.PackageMetadata.HasAiContext);
                    writer.WriteLine();
                }

                writer.WriteLine("Applied Files:");
                foreach (string file in record.AppliedFiles)
                {
                    writer.WriteLine("  " + file);
                }
                writer.WriteLine();

                writer.WriteLine("Commands:");
                foreach (var command in record.Commands)
                {
                    writer.WriteLine("COMMAND: " + command.Command);
                    writer.WriteLine("WORKING DIRECTORY: " + command.WorkingDirectory);
                    writer.WriteLine("EXIT CODE: " + command.ExitCode);
                    writer.WriteLine(command.Output);
                    writer.WriteLine();
                }

                if (record.Verification != null)
                {
                    writer.WriteLine("Verification:");
                    writer.WriteLine("  CommandsSucceeded: " + record.Verification.CommandsSucceeded);
                    writer.WriteLine("  ExpectedBuildPassed: " + record.Verification.ExpectedBuildPassed);
                    writer.WriteLine("  ExpectedMinimumTestsSatisfied: " + record.Verification.ExpectedMinimumTestsSatisfied);
                    writer.WriteLine("  ForbiddenChangedPathsClean: " + record.Verification.ForbiddenChangedPathsClean);
                    foreach (string note in record.Verification.Notes)
                    {
                        writer.WriteLine("  - " + note);
                    }
                    writer.WriteLine();
                }

                if (!string.IsNullOrWhiteSpace(record.Error)) { writer.WriteLine("ERROR:"); writer.WriteLine(record.Error); }
            }
            File.AppendAllText(indexPath, JsonConvert.SerializeObject(new
            {
                record.RunId,
                record.PackageName,
                record.PackageSha256,
                record.StartedUtc,
                record.CompletedUtc,
                record.Succeeded,
                record.TargetRoot,
                TrustLevel = record.Manifest != null ? record.Manifest.TrustLevel : null,
                PackageType = record.Manifest != null ? record.Manifest.PackageType : null,
                ChangeIntent = record.Manifest != null ? record.Manifest.ChangeIntent : null,
                HistoryPath = historyPath
            }) + System.Environment.NewLine);
        }
    }
}
