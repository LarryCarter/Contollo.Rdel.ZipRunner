using System.Collections.Generic;

namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal interface IContextRenderer
    {
        string Render(ContextAssemblyRequest request, ContextTemplate template, IList<ContextSection> sections);
    }
}
