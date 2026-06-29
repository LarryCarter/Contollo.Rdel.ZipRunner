using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ResponseContractProvider : IContextProvider
    {
        public string Name { get { return "ResponseContract"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("When implementing changes, respond with:");
            b.AppendLine("1. Summary");
            b.AppendLine("2. Affected files and status for each file");
            b.AppendLine("3. Architecture impact");
            b.AppendLine("4. Documentation updates");
            b.AppendLine("5. Validation steps");
            b.AppendLine("6. Rollback expectations");
            b.AppendLine("7. Assumptions and missing information");
            b.AppendLine("8. Complete current-format RDEL package unless instructed otherwise");
            return new ContextSection("Response Contract", b.ToString());
        }
    }
}
