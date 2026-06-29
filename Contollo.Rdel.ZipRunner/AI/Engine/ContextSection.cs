namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal sealed class ContextSection
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public ContextSection() {}
        public ContextSection(string title, string content) { Title = title; Content = content; }
    }
}
