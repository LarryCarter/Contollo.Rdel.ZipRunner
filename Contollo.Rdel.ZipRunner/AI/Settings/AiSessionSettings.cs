using System;
using System.IO;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Settings
{
    internal sealed class AiSessionSettings
    {
        public ContextLevel InitializeDocumentLevel { get; set; }
        public ContextLevel RehydrateDocumentLevel { get; set; }
        public ContextLevel ContinueDocumentLevel { get; set; }
        public string OutputDirectory { get; set; }

        public AiSessionSettings()
        {
            InitializeDocumentLevel = ContextLevel.Reference;
            RehydrateDocumentLevel = ContextLevel.Summary;
            ContinueDocumentLevel = ContextLevel.Reference;
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Contollo", "RDEL", "ContextPackages");
        }
    }
}
