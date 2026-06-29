using System;
using System.IO;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal sealed class OutputPlaceholderProvider : IContextProvider
    {
        private const int MaxPaneCharacters = 12000;

        public string Name { get { return "OutputPlaceholder"; } }

        public ContextSection Build(ContextAssemblyRequest request)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Captured Visual Studio Output Window content.");
            builder.AppendLine("If a pane cannot be read, paste that pane manually below the generated session context.");
            builder.AppendLine();

            AppendOutputWindowPanes(builder, request.Dte);
            AppendRdelRunHistoryPath(builder, request.SolutionRoot);

            return new ContextSection("Output Window / Error Context", builder.ToString());
        }

        private static void AppendOutputWindowPanes(StringBuilder builder, DTE2 dte)
        {
            try
            {
                if (dte == null || dte.ToolWindows == null || dte.ToolWindows.OutputWindow == null)
                {
                    builder.AppendLine("Output Window unavailable.");
                    builder.AppendLine();
                    return;
                }

                OutputWindow outputWindow = dte.ToolWindows.OutputWindow;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;

                if (panes == null || panes.Count == 0)
                {
                    builder.AppendLine("No Output Window panes found.");
                    builder.AppendLine();
                    return;
                }

                for (int i = 1; i <= panes.Count; i++)
                {
                    OutputWindowPane pane = panes.Item(i);
                    AppendPane(builder, pane);
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine("Could not capture Output Window panes: " + ex.Message);
                builder.AppendLine();
            }
        }

        private static void AppendPane(StringBuilder builder, OutputWindowPane pane)
        {
            if (pane == null)
            {
                return;
            }

            string name = SafePaneName(pane);
            string text = ReadPaneText(pane);

            builder.AppendLine("### Output Pane: " + name);
            builder.AppendLine();

            if (string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine("(empty)");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("```text");
            builder.AppendLine(Trim(text, MaxPaneCharacters));
            builder.AppendLine("```");
            builder.AppendLine();
        }

        private static string SafePaneName(OutputWindowPane pane)
        {
            try
            {
                return string.IsNullOrWhiteSpace(pane.Name) ? "(unnamed)" : pane.Name;
            }
            catch
            {
                return "(unknown)";
            }
        }

        private static string ReadPaneText(OutputWindowPane pane)
        {
            try
            {
                TextDocument document = pane.TextDocument;
                if (document == null)
                {
                    return string.Empty;
                }

                EditPoint start = document.StartPoint.CreateEditPoint();
                return start.GetText(document.EndPoint);
            }
            catch (Exception ex)
            {
                return "Could not read pane text: " + ex.Message;
            }
        }

        private static string Trim(string text, int maxCharacters)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (text.Length <= maxCharacters)
            {
                return text.TrimEnd();
            }

            return text.Substring(text.Length - maxCharacters).TrimEnd()
                + Environment.NewLine
                + "[Output truncated to last " + maxCharacters + " characters]";
        }

        private static void AppendRdelRunHistoryPath(StringBuilder builder, string solutionRoot)
        {
            if (string.IsNullOrWhiteSpace(solutionRoot))
            {
                return;
            }

            builder.AppendLine("RDEL run history path, if present:");
            builder.AppendLine("```text");
            builder.AppendLine(Path.Combine(solutionRoot, ".contollo", "rdel", "runs"));
            builder.AppendLine("```");
            builder.AppendLine();
        }
    }
}
