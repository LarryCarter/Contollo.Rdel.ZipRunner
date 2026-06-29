using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;
using Contollo.Rdel.ZipRunner.AI.Settings;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ProjectDocsProvider : IContextProvider
    {
        public string Name { get { return "ProjectDocs"; } }

        public ContextSection Build(ContextAssemblyRequest request)
        {
            var settings = AiSessionSettingsService.Load();
            ContextLevel level = request.Kind == AiSessionContextKind.Initialize
                ? settings.InitializeDocumentLevel
                : request.Kind == AiSessionContextKind.Rehydrate
                    ? settings.RehydrateDocumentLevel
                    : settings.ContinueDocumentLevel;

            var builder = new StringBuilder();
            builder.AppendLine("Context Level: " + level);
            Append(builder, request, "docs/context.md", "Project Context", level);
            Append(builder, request, "docs/RDEL-AI-OPERATOR-GUIDE.md", "AI Operator Guide", level);
            Append(builder, request, "docs/RDEL-PACKAGE-AUTHORING-GUIDE.md", "Package Authoring Guide", level);
            Append(builder, request, "docs/RDEL-AI-SPEC.md", "AI Specification", level);
            Append(builder, request, "docs/ai-instructions.md", "AI Instructions", level);
            return new ContextSection("Project Documentation", builder.ToString());
        }

        private static void Append(StringBuilder builder, ContextAssemblyRequest request, string path, string title, ContextLevel level)
        {
            builder.AppendLine();
            builder.AppendLine("### " + title);
            builder.AppendLine("Source: " + path);
            builder.AppendLine("Level: " + level);
            builder.AppendLine(ProviderText.ReadDocument(request.SolutionRoot, path, level));
        }
    }
}
