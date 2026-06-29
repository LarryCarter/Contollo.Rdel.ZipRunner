using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class FailureContractProvider : IContextProvider
    {
        public string Name { get { return "FailureContract"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("If required information is missing or ambiguous:");
            b.AppendLine("- do not invent files, classes, namespaces, methods, or dependencies");
            b.AppendLine("- state what is missing");
            b.AppendLine("- list assumptions explicitly");
            b.AppendLine("- mark uncertain work as: Unverified - Human Review Required");
            b.AppendLine("- ask for clarification when uncertainty changes implementation");
            return new ContextSection("Failure Contract", b.ToString());
        }
    }
}
