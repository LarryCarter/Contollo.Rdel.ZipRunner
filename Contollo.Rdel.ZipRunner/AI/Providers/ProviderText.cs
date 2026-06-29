using System;
using System.IO;
using System.Text;
using Contollo.Rdel.ZipRunner.AI.Engine;

namespace Contollo.Rdel.ZipRunner.AI.Providers
{
    internal static class ProviderText
    {
        public static string ReadDocument(string solutionRoot, string relativePath, ContextLevel level)
        {
            if (string.IsNullOrWhiteSpace(solutionRoot)) return "Solution root not available.";
            string path = Path.Combine(solutionRoot, relativePath);
            if (!File.Exists(path)) return "Not found locally: " + relativePath;
            if (level == ContextLevel.Reference) return "Reference only: " + relativePath;

            string text = File.ReadAllText(path);
            if (level == ContextLevel.Full) return text;

            return SummarizeDocument(relativePath, text);
        }

        public static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(not available)" : value;
        }

        private static string SummarizeDocument(string relativePath, string text)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Summary of: " + relativePath);
            using (var reader = new StringReader(text))
            {
                string line;
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        builder.AppendLine(line);
                        count++;
                        if (count >= 18) break;
                    }
                }
            }
            builder.AppendLine();
            builder.AppendLine("Full document available locally at: " + relativePath);
            return builder.ToString();
        }
    }
}
