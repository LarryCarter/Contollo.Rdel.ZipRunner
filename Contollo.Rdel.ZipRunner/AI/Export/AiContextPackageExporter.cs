using System;
using System.IO;
using System.IO.Compression;
using Contollo.Rdel.ZipRunner.AI.Settings;

namespace Contollo.Rdel.ZipRunner.AI.Export
{
    internal sealed class AiContextPackageExporter
    {
        public string Export(string solutionRoot)
        {
            var settings = AiSessionSettingsService.Load();
            Directory.CreateDirectory(settings.OutputDirectory);
            string zipPath = Path.Combine(settings.OutputDirectory, "rdel-ai-context-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".zip");
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                AddText(archive, "manifest.json", "{\n  \"PackageType\": \"RDEL AI Context Package\",\n  \"Protocol\": \"RDEL Session Protocol\",\n  \"Version\": \"1.2-preview\"\n}");
                AddFile(archive, solutionRoot, "docs/context.md"); AddFile(archive, solutionRoot, "docs/memory.md"); AddFile(archive, solutionRoot, "docs/DECISIONS.md");
                AddFile(archive, solutionRoot, "docs/ai-instructions.md"); AddFile(archive, solutionRoot, "docs/RDEL-AI-SPEC.md"); AddFile(archive, solutionRoot, "docs/RDEL-AI-OPERATOR-GUIDE.md");
                AddFile(archive, solutionRoot, "docs/RDEL-PACKAGE-AUTHORING-GUIDE.md"); AddFile(archive, solutionRoot, "docs/RDEL-CONTEXT-ASSEMBLY-ENGINE.md"); AddFile(archive, solutionRoot, "docs/RDEL-SESSION-PROTOCOL.md");
            }
            return zipPath;
        }
        private static void AddFile(ZipArchive archive, string root, string rel) { if (string.IsNullOrWhiteSpace(root)) return; string path = Path.Combine(root, rel); if (File.Exists(path)) archive.CreateEntryFromFile(path, rel.Replace('\\','/')); }
        private static void AddText(ZipArchive archive, string name, string text) { var e = archive.CreateEntry(name); using (var w = new StreamWriter(e.Open())) w.Write(text); }
    }
}
