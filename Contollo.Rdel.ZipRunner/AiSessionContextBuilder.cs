using EnvDTE80;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class AiSessionContextBuilder
    {
        private readonly ContextAssemblyEngine engine = new ContextAssemblyEngine();
        public string Build(AiSessionContextKind kind, DTE2 dte) { return engine.Assemble(kind, dte); }
    }
}
