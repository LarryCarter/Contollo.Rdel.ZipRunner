using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class CapabilityMatrixProvider : IContextProvider
    {
        public string Name { get { return "CapabilityMatrix"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("| Capability | Status |");
            b.AppendLine("|---|---|");
            b.AppendLine("| New files | Supported |");
            b.AppendLine("| Updated files | Supported |");
            b.AppendLine("| Delete files | Not supported |");
            b.AppendLine("| Rename files | Not supported |");
            b.AppendLine("| Patch/hunk updates | Not supported |");
            b.AppendLine("| Metadata export | Planned |");
            b.AppendLine("| Package signatures | Planned |");
            b.AppendLine("| Dependency enforcement | Planned |");
            return new ContextSection("Runner Capability Matrix", b.ToString());
        }
    }
}
