using System;
using System.Diagnostics;
using System.Text;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelProcess
    {
        public static RdelCommandResult Run(string command, string workingDirectory)
        {
            var result = new RdelCommandResult { Command = command, WorkingDirectory = workingDirectory, StartedUtc = DateTime.UtcNow };
            var output = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) => { if (args.Data != null) { output.AppendLine(args.Data); } };
                process.ErrorDataReceived += (sender, args) => { if (args.Data != null) { output.AppendLine(args.Data); } };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                result.ExitCode = process.ExitCode;
                result.CompletedUtc = DateTime.UtcNow;
                result.Output = output.ToString();
            }
            return result;
        }
    }
}