using System;
using System.IO;
using Newtonsoft.Json;

namespace Contollo.Rdel.ZipRunner.AI.Settings
{
    internal static class AiSessionSettingsService
    {
        private const string SettingsFileName = "ai-session-settings.json";

        public static AiSessionSettings Load()
        {
            try
            {
                string path = GetSettingsPath();
                if (!File.Exists(path))
                {
                    var defaults = new AiSessionSettings();
                    Save(defaults);
                    return defaults;
                }

                return JsonConvert.DeserializeObject<AiSessionSettings>(File.ReadAllText(path)) ?? new AiSessionSettings();
            }
            catch
            {
                return new AiSessionSettings();
            }
        }

        public static void Save(AiSessionSettings settings)
        {
            string path = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Contollo", "RDEL", SettingsFileName);
        }
    }
}
