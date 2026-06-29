using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE80;
using Contollo.Rdel.ZipRunner.AI.Providers;
using Contollo.Rdel.ZipRunner.AI.Renderers;

namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal sealed class ContextAssemblyEngine
    {
        private readonly Dictionary<string, IContextProvider> providers = new Dictionary<string, IContextProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly IContextRenderer renderer = new MarkdownContextRenderer();

        public ContextAssemblyEngine()
        {
            Register(new ProtocolProvider()); Register(new AiContractProvider()); Register(new FailureContractProvider());
            Register(new ResponseContractProvider()); Register(new CapabilityMatrixProvider()); Register(new PackageSkeletonProvider());
            Register(new ProjectProvider()); Register(new ProjectDocsProvider()); Register(new MemoryProvider()); Register(new DecisionsProvider());
            Register(new ActiveDocumentProvider()); Register(new GitStatusProvider()); Register(new OutputPlaceholderProvider()); Register(new UserRequestProvider());
        }

        public string Assemble(AiSessionContextKind kind, DTE2 dte)
        {
            string solutionRoot = RdelSolutionLocator.GetSolutionRoot(dte);
            var request = new ContextAssemblyRequest { Kind = kind, Dte = dte, SolutionRoot = solutionRoot, SelectedProjectRoot = RdelSolutionLocator.GetSelectedProjectRoot(dte), RepositoryName = string.IsNullOrWhiteSpace(solutionRoot) ? "Unknown" : Path.GetFileName(solutionRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) };
            var template = ContextTemplateRegistry.Get(kind);
            var sections = new List<ContextSection>();
            foreach (string providerName in template.Providers)
            {
                try { IContextProvider p; sections.Add(providers.TryGetValue(providerName, out p) ? p.Build(request) : new ContextSection("Missing Provider: " + providerName, "Not registered.")); }
                catch (Exception ex) { sections.Add(new ContextSection("Provider Error: " + providerName, ex.ToString())); }
            }
            return renderer.Render(request, template, sections);
        }
        private void Register(IContextProvider provider) { providers[provider.Name] = provider; }
    }
}
