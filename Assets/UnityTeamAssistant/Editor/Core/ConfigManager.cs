using System.IO;
using UnityEngine;

namespace UnityTeamAssistant.Core
{
    [System.Serializable]
    public class PluginConfig
    {
        public bool StrictMode = true;
        public int StaleTimeoutDays = 7;
        public string CommitMessagePrefix = "[UTA]";
        public string UserDisplayName = "";
        public bool AutoCommitUnlock = true;
        public bool AutoPullDashboard = true;
    }

    public static class ConfigManager
    {
        private static string ConfigPath => Path.Combine(Application.dataPath, "UnityTeamAssistant/Config/settings.json");
        private static PluginConfig cachedConfig;

        static ConfigManager()
        {
            LoadConfig();
        }

        public static PluginConfig LoadConfig()
        {
            if (cachedConfig != null) return cachedConfig;

            string dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                cachedConfig = JsonUtility.FromJson<PluginConfig>(json) ?? new PluginConfig();
            }
            else
            {
                cachedConfig = new PluginConfig();
                SaveConfig();
            }
            return cachedConfig;
        }

        public static void SaveConfig()
        {
            string dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonUtility.ToJson(cachedConfig, true);
            File.WriteAllText(ConfigPath, json);
        }

        public static string GetUserName()
        {
            var config = LoadConfig();
            if (!string.IsNullOrEmpty(config.UserDisplayName))
                return config.UserDisplayName;
            return GitService.GetUserName();
        }
    }
}