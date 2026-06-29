namespace Contollo.Rdel.ZipRunner.AI.Engine
{
    internal static class ContextTemplateRegistry
    {
        public static ContextTemplate Get(AiSessionContextKind kind)
        {
            if (kind == AiSessionContextKind.Initialize) return new ContextTemplate { Name = "Initialize", Providers = new[] { "Protocol", "AIContract", "FailureContract", "ResponseContract", "CapabilityMatrix", "PackageSkeleton", "Project", "ProjectDocs", "UserRequest" } };
            if (kind == AiSessionContextKind.Rehydrate) return new ContextTemplate { Name = "Rehydrate", Providers = new[] { "Protocol", "Project", "Memory", "Decisions", "CapabilityMatrix", "GitStatus", "UserRequest" } };
            return new ContextTemplate { Name = "Continue", Providers = new[] { "Project", "ActiveDocument", "GitStatus", "OutputPlaceholder", "UserRequest" } };
        }
    }
}
