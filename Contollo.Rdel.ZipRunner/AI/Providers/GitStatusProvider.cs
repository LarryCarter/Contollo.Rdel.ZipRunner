using System;
using System.IO;
using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class GitStatusProvider : IContextProvider
    {
        public string Name { get { return "GitStatus"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            if (string.IsNullOrWhiteSpace(request.SolutionRoot) || !Directory.Exists(request.SolutionRoot)) b.AppendLine("Solution root not available.");
            else
            {
                try { var r = RdelProcess.Run("git status --short", request.SolutionRoot); b.AppendLine(string.IsNullOrWhiteSpace(r.Output) ? "(clean or no output)" : r.Output.Trim()); }
                catch (Exception ex) { b.AppendLine("Could not read git status: " + ex.Message); }
            }
            return new ContextSection("Git Status", b.ToString());
        }
    }
}
