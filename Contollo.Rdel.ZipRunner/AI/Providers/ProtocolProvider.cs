using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ProtocolProvider : IContextProvider
    {
        public string Name { get { return "Protocol"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            b.AppendLine("RDEL Protocol Version: 1.2-preview");
            b.AppendLine("Session Mode: " + request.Kind);
            b.AppendLine("Neuro Commander Studio is the broader AI development workbench; RDEL is the current package/change engine.");
            b.AppendLine("RDEL means Repository Delta Evolution Loop: portable, traceable, verifiable AI-assisted change packages.");
            b.AppendLine("Current package root files: contollo-rdel.json and README.md.");
            b.AppendLine("Use docs/context.md for repository AI context updates.");
            b.AppendLine("Commands are authoritative. ValidationProfile is documented but not fully implemented.");
            b.AppendLine("Supported: new/update files, safe extraction, blocked paths, hash, validation commands, Git checkpoints, dry run, rollback.");
            b.AppendLine("Not supported: delete, rename, patch/hunk, metadata export, signatures, dependency enforcement.");
            return new ContextSection("Protocol", b.ToString());
        }
    }
}
