using System.Text.Json;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 配置文件仓储
/// </summary>
public class ConfigRepository
{
    private readonly string _settingsFilePath;

    public ConfigRepository(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    public SettingsConfig Load()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new SettingsConfig();
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize(json, SettingsContext.Default.SettingsConfig);
            return settings ?? new SettingsConfig();
        }
        catch (JsonException)
        {
            throw new ConfigFileCorruptedException(_settingsFilePath);
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public void Save(SettingsConfig settings)
    {
        var json = JsonSerializer.Serialize(settings, SettingsContext.Default.SettingsConfig);
        File.WriteAllText(_settingsFilePath, json);
    }
}

/// <summary>
/// 配置文件损坏异常
/// </summary>
public class ConfigFileCorruptedException : Exception
{
    public string FilePath { get; }

    public ConfigFileCorruptedException(string filePath)
        : base($"配置文件 '{filePath}' 格式损坏。")
    {
        FilePath = filePath;
    }
}
