using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class MemoryProvider : IContextProvider
    {
        public string Name { get { return "Memory"; } }

        public ContextSection Build(ContextAssemblyRequest request)
        {
            return new ContextSection(
                "Project Memory",
                ProviderText.ReadDocument(request.SolutionRoot, "docs/memory.md", ContextLevel.Summary));
        }
    }
}
