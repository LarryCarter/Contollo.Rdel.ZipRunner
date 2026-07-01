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

            b.AppendLine("Required ZIP filename format:");
            b.AppendLine("rdel-{version}-{project}-{description}.zip");
            b.AppendLine();
            b.AppendLine("Example:");
            b.AppendLine("rdel-1.4.1-contollo-rdel-ziprunner-package-naming-ai-docs.zip");
            b.AppendLine();
            b.AppendLine("Canonical current-format contollo-rdel.json skeleton:");
            b.AppendLine("{");
            b.AppendLine("  "Name": "RDEL 1.x.x — Project Name — Package Title",");
            b.AppendLine("  "Description": "What this package does.",");
            b.AppendLine("  "Target": "solution",");
            b.AppendLine("  "ValidationProfile": "GitOnly",");
            b.AppendLine("  "Commands": ["git status --porcelain"],");
            b.AppendLine("  "PackageId": "contollo.rdel.package-name",");
            b.AppendLine("  "PackageVersion": "1.x.x-preview",");
            b.AppendLine("  "Author": "Contollo",");
            b.AppendLine("  "Company": "Contollo",");
            b.AppendLine("  "Category": "Feature",");
            b.AppendLine("  "Tags": ["rdel"],");
            b.AppendLine("  "SchemaVersion": "1.1-current-compatible",");
            b.AppendLine("  "ManifestVersion": "1.3-preview",");
            b.AppendLine("  "PackageType": "FeatureAndDocumentation",");
            b.AppendLine("  "ChangeIntent": "Why this package exists.",");
            b.AppendLine("  "TrustLevel": "ExternalGenerated",");
            b.AppendLine("  "SourceProvider": "AI",");
            b.AppendLine("  "HumanReadmePath": "README.md",");
            b.AppendLine("  "AiContextPath": ".rdel-docops/context/update.md",");
            b.AppendLine("  "Compatibility": {");
            b.AppendLine("    "MinimumRunnerVersion": "0.3.1-preview",");
            b.AppendLine("    "RequiresDocOps": true,");
            b.AppendLine("    "RequiresGit": true");
            b.AppendLine("  },");
            b.AppendLine("  "DocumentationOperations": [");
            b.AppendLine("    { "Type": "Append", "Target": "docs/context.md", "Source": ".rdel-docops/context/update.md" },");
            b.AppendLine("    { "Type": "Append", "Target": "docs/memory.md", "Source": ".rdel-docops/memory/update.md" },");
            b.AppendLine("    { "Type": "Append", "Target": "docs/DECISIONS.md", "Source": ".rdel-docops/decisions/ADR-000-update.md" }");
            b.AppendLine("  ]");
            b.AppendLine("}");

            return new ContextSection("Canonical Package Skeleton", b.ToString());
        }
    }
}
