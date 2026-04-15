using System.Text.Json;

namespace PPGPerformancePlus.Config;

public sealed class ConfigManager
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ConfigManager(string configPath)
    {
        ConfigPath = configPath;
    }

    public string ConfigPath { get; }

    public ModConfig Load()
    {
        var directory = Path.GetDirectoryName(ConfigPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = new ModConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(ConfigPath);
        var config = JsonSerializer.Deserialize<ModConfig>(json, SerializerOptions);
        return config ?? new ModConfig();
    }

    public void Save(ModConfig config)
    {
        var json = JsonSerializer.Serialize(config, SerializerOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
