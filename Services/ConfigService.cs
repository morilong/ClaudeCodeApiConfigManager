using ClaudeCodeApiConfigManager.Models;
using Spectre.Console;

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
                if (!_output.Confirm($"配置 '{config.Name}' 已存在。是否覆盖？", false))
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
        _output.Success($"配置 '{config.Name}' 已成功添加。");
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
            _output.Error($"错误: 配置 '{name}' 不存在。");
            return false;
        }

        settings.Configs.Remove(config);

        // 如果删除的是当前活动配置
        if (settings.ActiveConfigName == name)
        {
            settings.ActiveConfigName = null;
            _output.Success($"已删除当前活动配置 '{name}'。");
        }
        else
        {
            _output.Success($"配置 '{name}' 已删除。");
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

        var table = new Table()
            .BorderColor(Color.Grey)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]●[/]").Centered())
            .AddColumn(new TableColumn("[bold]名称[/]"))
            .AddColumn(new TableColumn("[bold]模型[/]"))
            .AddColumn(new TableColumn("[bold]BaseURL[/]"));

        var activeName = settings.ActiveConfigName;

        foreach (var config in configs)
        {
            var isActive = config.Name == activeName;
            var marker = isActive ? "[green]●[/]" : " ";
            var nameStyle = isActive ? "[green]" : "";
            var nameStyleEnd = isActive ? "[/]" : "";
            table.AddRow(
                marker,
                $"{nameStyle}{Markup.Escape(config.Name)}{nameStyleEnd}",
                $"{nameStyle}{Markup.Escape(config.Model)}{nameStyleEnd}",
                $"{nameStyle}{Markup.Escape(config.BaseUrl)}{nameStyleEnd}"
            );
        }

        _output.WriteTable(table);
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
            _output.Warn("当前配置不存在，请使用 use 命令设置配置。");
            return;
        }

        // 主配置信息表格
        var configTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]属性[/]"))
            .AddColumn(new TableColumn("[bold]值[/]"));

        configTable.AddRow("名称", Markup.Escape(config.Name));
        configTable.AddRow("模型", Markup.Escape(config.Model));
        configTable.AddRow("BaseURL", Markup.Escape(config.BaseUrl));

        _output.WriteTable(configTable);

        // 自定义参数表格
        if (config.CustomParams.Count > 0)
        {
            _output.WriteLine();
            var paramTable = new Table()
                .BorderColor(Color.Grey)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[bold]自定义参数[/]"))
                .AddColumn(new TableColumn("[bold]值[/]"));

            foreach (var param in config.CustomParams)
            {
                paramTable.AddRow(Markup.Escape(param.Key), Markup.Escape(param.Value?.ToString() ?? ""));
            }

            _output.WriteTable(paramTable);
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
            _output.Error($"错误: 配置 '{name}' 不存在。");
            return false;
        }

        config.AuthToken = token;
        _repository.Save(settings);
        _output.Success($"配置 '{name}' 的 Token 已更新。");
        return true;
    }
}
