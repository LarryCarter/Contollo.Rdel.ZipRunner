namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal sealed class ContextDocument
    {
        public string RelativePath { get; set; }
        public string Title { get; set; }
        public ContextLevel Level { get; set; }

        public ContextDocument()
        {
        }

        public ContextDocument(string relativePath, string title, ContextLevel level)
        {
            RelativePath = relativePath;
            Title = title;
            Level = level;
        }
    }
}
