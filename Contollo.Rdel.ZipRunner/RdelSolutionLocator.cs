using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelSolutionLocator
    {
        public static string GetSolutionRoot(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (dte == null || dte.Solution == null || string.IsNullOrWhiteSpace(dte.Solution.FullName)) { return null; }
            return Path.GetDirectoryName(dte.Solution.FullName);
        }

        public static string GetSelectedProjectRoot(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (dte == null || dte.ToolWindows == null) { return null; }
            UIHierarchy solutionExplorer = dte.ToolWindows.SolutionExplorer;
            object[] selectedItems = solutionExplorer.SelectedItems as object[];
            if (selectedItems == null || selectedItems.Length == 0) { return null; }

            foreach (object item in selectedItems)
            {
                UIHierarchyItem hierarchyItem = item as UIHierarchyItem;
                Project project = hierarchyItem?.Object as Project;
                if (project == null)
                {
                    ProjectItem projectItem = hierarchyItem?.Object as ProjectItem;
                    project = projectItem?.ContainingProject;
                }
                if (project != null && !string.IsNullOrWhiteSpace(project.FullName)) { return Path.GetDirectoryName(project.FullName); }
            }
            return null;
        }
    }
}