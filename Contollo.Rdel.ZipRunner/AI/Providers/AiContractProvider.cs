using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class AiContractProvider : IContextProvider
    {
        public string Name { get { return "AIContract"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("Do not merely suggest code. Account for code.");
            b.AppendLine("Produce current-format RDEL packages when asked for changes.");
            b.AppendLine("Preserve exact file paths. Never invent missing files, methods, or dependencies.");
            b.AppendLine("State assumptions and missing information.");
            b.AppendLine("Update docs/context.md, docs/DECISIONS.md, and docs/memory.md when appropriate.");
            b.AppendLine("Use GitOnly validation for documentation-only packages.");
            b.AppendLine("Avoid plain dotnet build for this VSIX repository unless explicitly requested.");
            b.AppendLine("Avoid blocked paths and secrets. Use full-file updates until patch/hunk mode exists.");
            return new ContextSection("AI Contract", b.ToString());
        }
    }
}
