using System;
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
            string normalized = Normalize(relativePath);

            return normalized.StartsWith(".git/")
                || normalized.StartsWith(".vs/")
                || normalized.StartsWith("bin/")
                || normalized.StartsWith("obj/")
                || normalized.StartsWith(".contollo/")
                || normalized.StartsWith("__pycache__/")
                || normalized.Contains("/.git/")
                || normalized.Contains("/.vs/")
                || normalized.Contains("/bin/")
                || normalized.Contains("/obj/")
                || normalized.Contains("/.contollo/")
                || normalized.Contains("/__pycache__/")
                || IsSensitiveFile(normalized)
                || IsCompiledOrCacheFile(normalized);
        }

        public static bool IsPackageMetadataPath(string relativePath)
        {
            string normalized = Normalize(relativePath);
            return normalized.Equals("contollo-rdel.json", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("contollo-rdel.txt", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("README.md", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("context.md", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("manifest.json", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith(".rdel-docops/", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("rdel-package/", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("verification/", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("rollback/", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("audit/", StringComparison.OrdinalIgnoreCase);
        }

        public static string Normalize(string relativePath)
        {
            if (relativePath == null)
            {
                return string.Empty;
            }

            return relativePath.Replace('\\', '/').TrimStart('/');
        }

        private static bool IsCompiledOrCacheFile(string normalized)
        {
            return normalized.EndsWith(".pyc", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith(".cache", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSensitiveFile(string normalized)
        {
            string fileName = Path.GetFileName(normalized);

            return fileName.Equals(".env", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("secrets.json", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("appsettings.Production.json", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("appsettings.prod.json", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("appsettings.Release.json", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".pem", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".key", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".cer", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".crt", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".snk", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("/secrets/")
                || normalized.Contains("/certificates/")
                || normalized.Contains("/private/");
        }
    }
}
