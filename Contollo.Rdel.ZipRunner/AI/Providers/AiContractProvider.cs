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
            b.AppendLine("Produce current-format RDEL ZIP packages when asked for changes.");
            b.AppendLine("Name RDEL ZIP files using: rdel-{version}-{project}-{description}.zip.");
            b.AppendLine("Include project name and package version in the package title and ZIP filename.");
            b.AppendLine("Preserve exact file paths. Never invent missing files, methods, or dependencies.");
            b.AppendLine("State assumptions and missing information.");
            b.AppendLine("For cumulative docs, use DocOps instead of direct full-file overwrites.");
            b.AppendLine("Do not directly overwrite docs/context.md, docs/memory.md, or docs/DECISIONS.md unless explicitly instructed.");
            b.AppendLine("For existing projects without enough context, request or generate project intake first.");
            b.AppendLine("Use GitOnly validation for documentation-only packages.");
            b.AppendLine("Avoid plain dotnet build for this VSIX repository unless explicitly requested.");
            b.AppendLine("Avoid blocked paths and secrets. Use full-file updates until patch/hunk mode exists.");

            return new ContextSection("AI Contract", b.ToString());
        }
    }
}
