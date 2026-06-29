namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal interface IContextProvider
    {
        string Name { get; }
        ContextSection Build(ContextAssemblyRequest request);
    }
}
