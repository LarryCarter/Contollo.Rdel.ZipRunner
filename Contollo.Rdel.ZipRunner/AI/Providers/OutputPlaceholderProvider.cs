using System.IO;
using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class OutputPlaceholderProvider : IContextProvider
    {
        public string Name { get { return "OutputPlaceholder"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("Paste relevant Visual Studio Output, Build, Test, Git, Terminal, or Contollo RDEL output below if needed.");
            b.AppendLine("[Paste output here]");
            if (!string.IsNullOrWhiteSpace(request.SolutionRoot)) b.AppendLine("RDEL runs: " + Path.Combine(request.SolutionRoot, ".contollo", "rdel", "runs"));
            return new ContextSection("Output Window / Error Context", b.ToString());
        }
    }
}
