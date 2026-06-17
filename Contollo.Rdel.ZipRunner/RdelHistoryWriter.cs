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
                writer.WriteLine("Target: " + record.TargetRoot);
                writer.WriteLine("Succeeded: " + record.Succeeded);
                writer.WriteLine();
                foreach (var command in record.Commands)
                {
                    writer.WriteLine("COMMAND: " + command.Command);
                    writer.WriteLine("WORKING DIRECTORY: " + command.WorkingDirectory);
                    writer.WriteLine("EXIT CODE: " + command.ExitCode);
                    writer.WriteLine(command.Output);
                    writer.WriteLine();
                }
                if (!string.IsNullOrWhiteSpace(record.Error)) { writer.WriteLine("ERROR:"); writer.WriteLine(record.Error); }
            }
            File.AppendAllText(indexPath, JsonConvert.SerializeObject(new { record.RunId, record.PackageName, record.StartedUtc, record.CompletedUtc, record.Succeeded, record.TargetRoot, HistoryPath = historyPath }) + System.Environment.NewLine);
        }
    }
}