using System.IO;
using System.Text.RegularExpressions;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelPath
    {
        public static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "package";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '-');
            }

            return Regex.Replace(value, @"\s+", "-");
        }

        public static bool IsBlockedPath(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimStart('/');

            return normalized.StartsWith(".git/")
                || normalized.StartsWith(".vs/")
                || normalized.StartsWith("bin/")
                || normalized.StartsWith("obj/")
                || normalized.StartsWith(".contollo/")
                || normalized.StartsWith("__pycache__/")
                || normalized.EndsWith(".pyc")
                || normalized.Contains("/.git/")
                || normalized.Contains("/.vs/")
                || normalized.Contains("/bin/")
                || normalized.Contains("/obj/")
                || normalized.Contains("/.contollo/")
                || normalized.Contains("/__pycache__/");
        }
    }
}