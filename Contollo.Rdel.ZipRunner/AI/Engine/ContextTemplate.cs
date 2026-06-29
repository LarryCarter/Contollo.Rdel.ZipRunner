namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal sealed class ContextTemplate
    {
        public string Name { get; set; }
        public string[] Providers { get; set; }
        public ContextTemplate() { Providers = new string[0]; }
    }
}
