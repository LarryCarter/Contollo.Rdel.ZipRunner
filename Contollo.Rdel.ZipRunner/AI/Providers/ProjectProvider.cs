using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ProjectProvider : IContextProvider
    {
        public string Name { get { return "Project"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("Repository Name: " + ProviderText.NullIfEmpty(request.RepositoryName));
            b.AppendLine("Solution Root: " + ProviderText.NullIfEmpty(request.SolutionRoot));
            b.AppendLine("Selected Project Root: " + ProviderText.NullIfEmpty(request.SelectedProjectRoot));
            b.AppendLine("Public context URLs:");
            b.AppendLine("- https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/context.md");
            b.AppendLine("- https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-OPERATOR-GUIDE.md");
            b.AppendLine("- https://larrycarter.github.io/Contollo.Rdel.ZipRunner/docs/RDEL-AI-SPEC.md");
            b.AppendLine("Current Epic: Epic 1.2 — Package Identity, Compatibility, Discovery, AI Communication, and Context Assembly.");
            return new ContextSection("Project", b.ToString());
        }
    }
}
