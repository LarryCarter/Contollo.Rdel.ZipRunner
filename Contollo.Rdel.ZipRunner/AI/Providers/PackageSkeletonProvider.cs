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
            b.AppendLine("rdel-1.4.2-contollo-rdel-ziprunner-provider-fix.zip");
            b.AppendLine();
            b.AppendLine("Canonical current-format contollo-rdel.json skeleton:");

            string[] manifestLines =
            {
                "{",
                "  \"Name\": \"RDEL 1.x.x - Project Name - Package Title\",",
                "  \"Description\": \"What this package does.\",",
                "  \"Target\": \"solution\",",
                "  \"ValidationProfile\": \"GitOnly\",",
                "  \"Commands\": [\"git status --porcelain\"],",
                "  \"PackageId\": \"contollo.rdel.package-name\",",
                "  \"PackageVersion\": \"1.x.x-preview\",",
                "  \"Author\": \"Contollo\",",
                "  \"Company\": \"Contollo\",",
                "  \"Category\": \"Feature\",",
                "  \"Tags\": [\"rdel\"],",
                "  \"SchemaVersion\": \"1.1-current-compatible\",",
                "  \"ManifestVersion\": \"1.3-preview\",",
                "  \"PackageType\": \"FeatureAndDocumentation\",",
                "  \"ChangeIntent\": \"Why this package exists.\",",
                "  \"TrustLevel\": \"ExternalGenerated\",",
                "  \"SourceProvider\": \"AI\",",
                "  \"HumanReadmePath\": \"README.md\",",
                "  \"AiContextPath\": \".rdel-docops/context/update.md\",",
                "  \"Compatibility\": {",
                "    \"MinimumRunnerVersion\": \"0.3.1-preview\",",
                "    \"RequiresDocOps\": true,",
                "    \"RequiresGit\": true",
                "  },",
                "  \"DocumentationOperations\": [",
                "    { \"Type\": \"Append\", \"Target\": \"docs/context.md\", \"Source\": \".rdel-docops/context/update.md\" },",
                "    { \"Type\": \"Append\", \"Target\": \"docs/memory.md\", \"Source\": \".rdel-docops/memory/update.md\" },",
                "    { \"Type\": \"Append\", \"Target\": \"docs/DECISIONS.md\", \"Source\": \".rdel-docops/decisions/ADR-000-update.md\" }",
                "  ]",
                "}"
            };

            foreach (string line in manifestLines)
            {
                b.AppendLine(line);
            }

            b.AppendLine();
            b.AppendLine("Package QA rule:");
            b.AppendLine("If an RDEL package changes C# files, verify generated C# parses before packaging.");
            b.AppendLine("If the package claims a rename, delete, refactor, or code change, the package diff must prove that change is present.");

            return new ContextSection("Canonical Package Skeleton", b.ToString());
        }
    }
}
