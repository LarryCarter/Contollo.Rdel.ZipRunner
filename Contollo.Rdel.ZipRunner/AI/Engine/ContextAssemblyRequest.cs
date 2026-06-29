using EnvDTE80;

namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal sealed class ContextAssemblyRequest
    {
        public AiSessionContextKind Kind { get; set; }
        public DTE2 Dte { get; set; }
        public string SolutionRoot { get; set; }
        public string SelectedProjectRoot { get; set; }
        public string RepositoryName { get; set; }
    }
}
