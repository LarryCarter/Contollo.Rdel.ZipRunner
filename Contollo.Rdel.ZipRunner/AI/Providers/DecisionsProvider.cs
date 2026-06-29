using Contollo.Rdel.ZipRunner.AI.Engine;
namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class DecisionsProvider : IContextProvider
    {
        public string Name { get { return "Decisions"; } }
        public ContextSection Build(ContextAssemblyRequest request) { return new ContextSection("Architecture Decisions", ProviderText.ReadDocument(request.SolutionRoot, "docs/DECISIONS.md", 5000)); }
    }
}
