using System.Text.Json;
using System.Text.Json.Serialization;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 配置文件管理
/// </summary>
public class ConfigManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = SettingsContext.Default
    };

    private readonly string _settingsFilePath;

    public ConfigManager()
    {
        _settingsFilePath = ConfigDirectory.GetSettingsFilePath();
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    public SettingsConfig LoadSettings()
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
            Console.Error.WriteLine($"错误: 配置文件 '{_settingsFilePath}' 格式损坏。");
            Console.Error.WriteLine($"请检查文件或删除它后重试。");
            throw;
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public void SaveSettings(SettingsConfig settings)
    {
        var json = JsonSerializer.Serialize(settings, SettingsContext.Default.SettingsConfig);
        File.WriteAllText(_settingsFilePath, json);
    }

    /// <summary>
    /// 添加或更新配置
    /// </summary>
    public bool AddOrUpdateConfig(ApiConfig config, bool forceOverwrite = false)
    {
        var settings = LoadSettings();
        var existingIndex = settings.Configs.FindIndex(c => c.Name == config.Name);

        if (existingIndex >= 0)
        {
            if (!forceOverwrite)
            {
                Console.Write($"配置 '{config.Name}' 已存在。是否覆盖？(y/N): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y" && response != "yes")
                {
                    Console.WriteLine("操作已取消。");
                    return false;
                }
            }
            settings.Configs[existingIndex] = config;
        }
        else
        {
            settings.Configs.Add(config);
        }

        SaveSettings(settings);
        Console.WriteLine($"配置 '{config.Name}' 已成功添加。");
        return true;
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    public bool RemoveConfig(string name)
    {
        var settings = LoadSettings();
        var config = settings.Configs.Find(c => c.Name == name);

        if (config == null)
        {
            Console.Error.WriteLine($"错误: 配置 '{name}' 不存在。");
            return false;
        }

        settings.Configs.Remove(config);

        // 如果删除的是当前活动配置
        if (settings.ActiveConfigName == name)
        {
            settings.ActiveConfigName = null;
            Console.WriteLine($"已删除当前活动配置 '{name}'。");
        }
        else
        {
            Console.WriteLine($"配置 '{name}' 已删除。");
        }

        SaveSettings(settings);
        return true;
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    public List<ApiConfig> GetAllConfigs()
    {
        var settings = LoadSettings();
        return settings.Configs;
    }

    /// <summary>
    /// 获取指定配置
    /// </summary>
    public ApiConfig? GetConfig(string name)
    {
        var settings = LoadSettings();
        return settings.Configs.Find(c => c.Name == name);
    }

    /// <summary>
    /// 获取当前活动配置
    /// </summary>
    public ApiConfig? GetActiveConfig()
    {
        var settings = LoadSettings();
        if (string.IsNullOrEmpty(settings.ActiveConfigName))
        {
            return null;
        }
        return settings.Configs.Find(c => c.Name == settings.ActiveConfigName);
    }

    /// <summary>
    /// 设置活动配置
    /// </summary>
    public void SetActiveConfig(string name)
    {
        var settings = LoadSettings();
        if (settings.Configs.All(c => c.Name != name))
        {
            throw new ArgumentException($"配置 '{name}' 不存在。");
        }
        settings.ActiveConfigName = name;
        SaveSettings(settings);
    }

    /// <summary>
    /// 列出所有配置
    /// </summary>
    public void ListConfigs()
    {
        var settings = LoadSettings();
        var configs = settings.Configs;

        if (configs.Count == 0)
        {
            Console.WriteLine("暂无配置，使用 add 命令添加配置。");
            return;
        }

        var activeName = settings.ActiveConfigName;

        foreach (var config in configs)
        {
            var isActive = config.Name == activeName;
            var prefix = isActive ? "* " : "  ";
            Console.WriteLine($"{prefix}{config.Name} ({config.Model})");
        }
    }

    /// <summary>
    /// 显示当前配置
    /// </summary>
    public void ShowCurrentConfig()
    {
        var settings = LoadSettings();

        if (settings.Configs.Count == 0)
        {
            Console.WriteLine("暂无配置。");
            return;
        }

        if (string.IsNullOrEmpty(settings.ActiveConfigName))
        {
            Console.WriteLine("未设置当前配置，使用 use 命令切换配置。");
            return;
        }

        var config = settings.Configs.Find(c => c.Name == settings.ActiveConfigName);
        if (config == null)
        {
            Console.WriteLine("当前配置不存在，请使用 use 命令设置配置。");
            return;
        }

        Console.WriteLine($"当前配置:");
        Console.WriteLine($"  名称: {config.Name}");
        Console.WriteLine($"  模型: {config.Model}");
        Console.WriteLine($"  Base URL: {config.BaseUrl}");

        if (config.CustomParams.Count > 0)
        {
            Console.WriteLine($"  自定义参数:");
            foreach (var param in config.CustomParams)
            {
                Console.WriteLine($"    {param.Key}={param.Value}");
            }
        }
    }
}
