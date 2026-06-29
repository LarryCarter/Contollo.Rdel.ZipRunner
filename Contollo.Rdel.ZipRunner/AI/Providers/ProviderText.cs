using System;
using System.IO;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal static class ProviderText
    {
        public static string ReadDocument(string solutionRoot, string relativePath, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(solutionRoot)) return "Solution root not available.";
            string path = Path.Combine(solutionRoot, relativePath);
            if (!File.Exists(path)) return "Not found locally: " + relativePath;
            string text = File.ReadAllText(path);
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + Environment.NewLine + "[Truncated]";
        }
        public static string NullIfEmpty(string value) { return string.IsNullOrWhiteSpace(value) ? "(not available)" : value; }
    }
}
