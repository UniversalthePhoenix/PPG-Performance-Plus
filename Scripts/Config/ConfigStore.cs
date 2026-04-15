using System.IO;
using UnityEngine;

namespace PPGPerformancePlusMod
{
    public static class ConfigStore
    {
        public static string GetConfigPath()
        {
            return Path.Combine(GetRootDirectory(), "config.json");
        }

        public static string GetSessionReportPath()
        {
            return Path.Combine(GetRootDirectory(), "session-report.txt");
        }

        public static ModConfig Load()
        {
            var path = GetConfigPath();
            EnsureDirectory();

            if (!File.Exists(path))
            {
                var defaultConfig = new ModConfig();
                Save(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(path);
                var loaded = JsonUtility.FromJson<ModConfig>(json);
                return loaded ?? new ModConfig();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[PPG Performance+] Failed to load config: " + exception.Message);
                return new ModConfig();
            }
        }

        public static void Save(ModConfig config)
        {
            var path = GetConfigPath();
            EnsureDirectory();

            try
            {
                var json = JsonUtility.ToJson(config, true);
                File.WriteAllText(path, json);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[PPG Performance+] Failed to save config: " + exception.Message);
            }
        }

        public static void SaveSessionReport(string report)
        {
            var path = GetSessionReportPath();
            EnsureDirectory();

            try
            {
                File.WriteAllText(path, report);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[PPG Performance+] Failed to save session report: " + exception.Message);
            }
        }

        private static string GetRootDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "PPGPerformancePlus");
        }

        private static void EnsureDirectory()
        {
            var root = GetRootDirectory();
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
        }
    }
}
