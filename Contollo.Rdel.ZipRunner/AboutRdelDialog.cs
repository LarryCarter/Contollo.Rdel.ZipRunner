using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class AboutRdelDialog : Form
    {
        public AboutRdelDialog(string solutionRoot)
        {
            Text = "About Contollo RDEL";
            StartPosition = FormStartPosition.CenterParent;
            Width = 680;
            Height = 520;
            MinimumSize = new Size(680, 520);

            var title = new Label
            {
                Text = "Contollo RDEL Zip Runner",
                Left = 16,
                Top = 16,
                Width = 620,
                Height = 30,
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold)
            };

            var subtitle = new Label
            {
                Text = "Repository Delta Evolution Loop",
                Left = 16,
                Top = 48,
                Width = 620,
                Height = 24
            };

            var textBox = new TextBox
            {
                Left = 16,
                Top = 82,
                Width = 630,
                Height = 340,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                Text = BuildAboutText(solutionRoot)
            };

            var closeButton = new Button
            {
                Text = "Close",
                Left = 556,
                Top = 435,
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.OK
            };

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(textBox);
            Controls.Add(closeButton);

            AcceptButton = closeButton;
            CancelButton = closeButton;
        }

        private static string BuildAboutText(string solutionRoot)
        {
            var builder = new StringBuilder();

            JObject version = LoadVersion(solutionRoot);

            builder.AppendLine("Product");
            builder.AppendLine("-------");
            builder.AppendLine(Get(version, "ProductName", "Contollo RDEL Zip Runner"));
            builder.AppendLine();

            builder.AppendLine("Protocol");
            builder.AppendLine("--------");
            builder.AppendLine(Get(version, "ProtocolName", "Repository Delta Evolution Loop"));
            builder.AppendLine();

            builder.AppendLine("Versions");
            builder.AppendLine("--------");
            builder.AppendLine("ProductVersion: " + Get(version, "ProductVersion", "0.3.0-preview"));
            builder.AppendLine("PackageFormatVersion: " + Get(version, "PackageFormatVersion", "1.1-current-compatible"));
            builder.AppendLine("ManifestVersion: " + Get(version, "ManifestVersion", "1.2-preview"));
            builder.AppendLine("RSP Version: " + Get(version, "RdelSessionProtocolVersion", "1.2-preview"));
            builder.AppendLine();

            builder.AppendLine("Current Phase");
            builder.AppendLine("-------------");
            builder.AppendLine(Get(version, "CurrentPhase", "Phase 1.3 / AI Context Manager Foundation"));
            builder.AppendLine();

            builder.AppendLine("Baseline Includes");
            builder.AppendLine("-----------------");
            AppendArray(builder, version, "CurrentBaselineIncludes");
            builder.AppendLine();

            builder.AppendLine("Known Missing");
            builder.AppendLine("-------------");
            AppendArray(builder, version, "KnownMissing");
            builder.AppendLine();

            builder.AppendLine("Solution Root");
            builder.AppendLine("-------------");
            builder.AppendLine(string.IsNullOrWhiteSpace(solutionRoot) ? "(not available)" : solutionRoot);
            builder.AppendLine();

            builder.AppendLine("Version Source");
            builder.AppendLine("--------------");
            builder.AppendLine(GetVersionPath(solutionRoot));

            return builder.ToString();
        }

        private static JObject LoadVersion(string solutionRoot)
        {
            try
            {
                string path = GetVersionPath(solutionRoot);
                if (!File.Exists(path))
                {
                    return new JObject();
                }

                return JObject.Parse(File.ReadAllText(path));
            }
            catch
            {
                return new JObject();
            }
        }

        private static string GetVersionPath(string solutionRoot)
        {
            if (string.IsNullOrWhiteSpace(solutionRoot))
            {
                return "docs/VERSION.json";
            }

            return Path.Combine(solutionRoot, "docs", "VERSION.json");
        }

        private static string Get(JObject source, string propertyName, string fallback)
        {
            JToken token;
            if (source != null && source.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out token))
            {
                string value = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return fallback;
        }

        private static void AppendArray(StringBuilder builder, JObject source, string propertyName)
        {
            JToken token;
            if (source != null && source.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out token) && token is JArray array)
            {
                foreach (JToken item in array)
                {
                    builder.AppendLine("- " + item.ToString());
                }

                return;
            }

            builder.AppendLine("(not available)");
        }
    }
}
