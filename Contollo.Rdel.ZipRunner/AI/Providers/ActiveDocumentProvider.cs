using System;
using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class ActiveDocumentProvider : IContextProvider
    {
        public string Name { get { return "ActiveDocument"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            var b = new StringBuilder();
            try
            {
                if (request.Dte != null && request.Dte.ActiveDocument != null)
                {
                    b.AppendLine("Name: " + request.Dte.ActiveDocument.Name);
                    b.AppendLine("Path: " + request.Dte.ActiveDocument.FullName);
                }
                else b.AppendLine("No active document detected.");
            }
            catch (Exception ex) { b.AppendLine("Could not read active document: " + ex.Message); }
            return new ContextSection("Active Document", b.ToString());
        }
    }
}
