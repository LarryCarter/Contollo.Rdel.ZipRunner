namespace Contollo.Rdel.ZipRunner
{
    internal sealed class RdelCommandRunner
    {
        public RdelCommandResult Run(string command, string workingDirectory, RdelOutputPane pane)
        {
            pane.WriteLine("Running: " + command);
            var result = RdelProcess.Run(command, workingDirectory);
            pane.WriteLine(result.Output);
            pane.WriteLine("Exit code: " + result.ExitCode);
            return result;
        }
    }
}