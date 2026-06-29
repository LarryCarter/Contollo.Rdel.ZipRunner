using Contollo.Rdel.ZipRunner.AI.Engine;
namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class UserRequestProvider : IContextProvider
    {
        public string Name { get { return "UserRequest"; } }
        public ContextSection Build(ContextAssemblyRequest request)
        {
            return new ContextSection("User Request / Current Task",
                request.Kind == AiSessionContextKind.Initialize
                    ? "First, confirm you understand RDEL and the required current-format package workflow. Then wait for the specific task."
                    : "[Paste or type the current task here.]");
        }
    }
}
