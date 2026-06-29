using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class PackageSkeletonProvider : IContextProvider
    {
        public string Name { get { return "PackageSkeleton"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("Canonical current-format contollo-rdel.json skeleton:");
            b.AppendLine("{");
            b.AppendLine("  \"Name\": \"Package Name\",");
            b.AppendLine("  \"Description\": \"What this package does.\",");
            b.AppendLine("  \"Target\": \"solution\",");
            b.AppendLine("  \"ValidationProfile\": \"GitOnly\",");
            b.AppendLine("  \"Commands\": [\"git status --porcelain\"],");
            b.AppendLine("  \"SchemaVersion\": \"1.1-current-compatible\",");
            b.AppendLine("  \"ManifestVersion\": \"1.2-preview\",");
            b.AppendLine("  \"PackageVersion\": \"1.2.x-preview\",");
            b.AppendLine("  \"PackageType\": \"FeatureAndDocumentation\",");
            b.AppendLine("  \"ChangeIntent\": \"Why this package exists.\",");
            b.AppendLine("  \"TrustLevel\": \"ExternalGenerated\",");
            b.AppendLine("  \"SourceProvider\": \"AI\",");
            b.AppendLine("  \"HumanReadmePath\": \"README.md\",");
            b.AppendLine("  \"AiContextPath\": \"docs/context.md\"");
            b.AppendLine("}");
            return new ContextSection("Canonical Package Skeleton", b.ToString());
        }
    }
}
