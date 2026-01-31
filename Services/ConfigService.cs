using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 配置服务
/// </summary>
public class ConfigService
{
    private readonly ConfigRepository _repository;
    private readonly IConsoleOutput _output;

    public ConfigService(ConfigRepository repository, IConsoleOutput output)
    {
        _repository = repository;
        _output = output;
    }

    /// <summary>
    /// 添加或更新配置
    /// </summary>
    public bool AddOrUpdateConfig(ApiConfig config, bool forceOverwrite = false)
    {
        var settings = _repository.Load();
        var existingIndex = settings.Configs.FindIndex(c => c.Name == config.Name);

        if (existingIndex >= 0)
        {
            if (!forceOverwrite)
            {
                _output.Write($"配置 '{config.Name}' 已存在。是否覆盖？(y/N): ");
                var response = _output.ReadLine()?.Trim().ToLower();
                if (response != "y" && response != "yes")
                {
                    _output.WriteLine("操作已取消。");
                    return false;
                }
            }
            settings.Configs[existingIndex] = config;
        }
        else
        {
            settings.Configs.Add(config);
        }

        _repository.Save(settings);
        _output.WriteLine($"配置 '{config.Name}' 已成功添加。");
        return true;
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    public bool RemoveConfig(string name)
    {
        var settings = _repository.Load();
        var config = settings.Configs.Find(c => c.Name == name);

        if (config == null)
        {
            _output.WriteError($"错误: 配置 '{name}' 不存在。");
            return false;
        }

        settings.Configs.Remove(config);

        // 如果删除的是当前活动配置
        if (settings.ActiveConfigName == name)
        {
            settings.ActiveConfigName = null;
            _output.WriteLine($"已删除当前活动配置 '{name}'。");
        }
        else
        {
            _output.WriteLine($"配置 '{name}' 已删除。");
        }

        _repository.Save(settings);
        return true;
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    public List<ApiConfig> GetAllConfigs()
    {
        var settings = _repository.Load();
        return settings.Configs;
    }

    /// <summary>
    /// 获取指定配置
    /// </summary>
    public ApiConfig? GetConfig(string name)
    {
        var settings = _repository.Load();
        return settings.Configs.Find(c => c.Name == name);
    }

    /// <summary>
    /// 获取当前活动配置
    /// </summary>
    public ApiConfig? GetActiveConfig()
    {
        var settings = _repository.Load();
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
        var settings = _repository.Load();
        if (settings.Configs.All(c => c.Name != name))
        {
            throw new ArgumentException($"配置 '{name}' 不存在。");
        }
        settings.ActiveConfigName = name;
        _repository.Save(settings);
    }

    /// <summary>
    /// 列出所有配置
    /// </summary>
    public void ListConfigs()
    {
        var settings = _repository.Load();
        var configs = settings.Configs;

        if (configs.Count == 0)
        {
            _output.WriteLine("暂无配置，使用 add 命令添加配置。");
            return;
        }

        var activeName = settings.ActiveConfigName;

        foreach (var config in configs)
        {
            var isActive = config.Name == activeName;
            var prefix = isActive ? "* " : "  ";
            _output.WriteLine($"{prefix}{config.Name} ({config.Model})");
        }
    }

    /// <summary>
    /// 显示当前配置
    /// </summary>
    public void ShowCurrentConfig()
    {
        var settings = _repository.Load();

        if (settings.Configs.Count == 0)
        {
            _output.WriteLine("暂无配置。");
            return;
        }

        if (string.IsNullOrEmpty(settings.ActiveConfigName))
        {
            _output.WriteLine("未设置当前配置，使用 use 命令切换配置。");
            return;
        }

        var config = settings.Configs.Find(c => c.Name == settings.ActiveConfigName);
        if (config == null)
        {
            _output.WriteLine("当前配置不存在，请使用 use 命令设置配置。");
            return;
        }

        _output.WriteLine("当前配置:");
        _output.WriteLine($"  名称: {config.Name}");
        _output.WriteLine($"  模型: {config.Model}");
        _output.WriteLine($"  BaseURL: {config.BaseUrl}");

        if (config.CustomParams.Count > 0)
        {
            _output.WriteLine("  自定义参数:");
            foreach (var param in config.CustomParams)
            {
                _output.WriteLine($"    {param.Key}={param.Value}");
            }
        }
    }

    /// <summary>
    /// 修改指定配置的 Token
    /// </summary>
    public bool SetToken(string name, string token)
    {
        var settings = _repository.Load();
        var config = settings.Configs.Find(c => c.Name == name);

        if (config == null)
        {
            _output.WriteError($"错误: 配置 '{name}' 不存在。");
            return false;
        }

        config.AuthToken = token;
        _repository.Save(settings);
        _output.WriteLine($"配置 '{name}' 的 Token 已更新。");
        return true;
    }
}
