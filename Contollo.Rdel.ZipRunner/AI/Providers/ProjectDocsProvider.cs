using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ProjectDocsProvider : IContextProvider
    {
        public string Name { get { return "ProjectDocs"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("Docs to read for detail: docs/context.md, docs/RDEL-AI-OPERATOR-GUIDE.md, docs/RDEL-PACKAGE-AUTHORING-GUIDE.md, docs/RDEL-AI-SPEC.md, docs/ai-instructions.md.");
            b.AppendLine();
            b.AppendLine("Project context excerpt:");
            b.AppendLine(ProviderText.ReadDocument(request.SolutionRoot, "docs/context.md", 2500));
            b.AppendLine();
            b.AppendLine("Operator guide excerpt:");
            b.AppendLine(ProviderText.ReadDocument(request.SolutionRoot, "docs/RDEL-AI-OPERATOR-GUIDE.md", 2500));
            return new ContextSection("Project Documentation", b.ToString());
        }
    }
}
