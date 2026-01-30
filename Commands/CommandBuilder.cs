using System.CommandLine;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager.Commands;

/// <summary>
/// 命令构建器
/// </summary>
public static class CommandBuilder
{
    private static readonly ConfigRepository ConfigRepository = new(Platform.GetSettingsFilePath());
    private static readonly IConsoleOutput ConsoleOutput = new ConsoleOutput();
    private static readonly ConfigService ConfigService = new(ConfigRepository, ConsoleOutput);

    /// <summary>
    /// 创建 add 命令
    /// </summary>
    public static Command CreateAddCommand()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "配置名称"
        };
        var argsArgument = new Argument<string[]>("args")
        {
            Description = "API 配置参数 [TOKEN] [BASE_URL] [MODEL] [自定义参数...]",
            Arity = ArgumentArity.OneOrMore
        };

        var command = new Command("add", "添加新的 API 配置")
        {
            nameArgument,
            argsArgument
        };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArgument)!;
            var args = parseResult.GetValue(argsArgument)!;
            var config = CommandHelper.ParseAddArguments(name, args);

            // 验证必填字段
            if (string.IsNullOrEmpty(config.AuthToken))
            {
                ConsoleOutput.WriteError("错误: 未找到 API Token。");
                ConsoleOutput.WriteError("用法: ccm add <name> <TOKEN> <BASE_URL> <MODEL> [自定义参数...]");
                return;
            }

            if (string.IsNullOrEmpty(config.BaseUrl))
            {
                ConsoleOutput.WriteError("错误: 未找到 Base URL（必须以 http:// 或 https:// 开头）。");
                return;
            }

            if (string.IsNullOrEmpty(config.Model))
            {
                ConsoleOutput.WriteError("错误: 未指定模型名称。");
                return;
            }

            ConfigService.AddOrUpdateConfig(config);
        });

        return command;
    }

    /// <summary>
    /// 创建 list/ls 命令
    /// </summary>
    public static Command CreateListCommand()
    {
        var command = new Command("list", "列出所有已保存的配置");
        command.Aliases.Add("ls");

        command.SetAction(_ =>
        {
            ConfigService.ListConfigs();
        });

        return command;
    }

    /// <summary>
    /// 创建 use 命令
    /// </summary>
    public static Command CreateUseCommand()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "配置名称"
        };

        var command = new Command("use", "切换到指定配置")
        {
            nameArgument
        };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArgument)!;
            var config = ConfigService.GetConfig(name);
            if (config == null)
            {
                ConsoleOutput.WriteError($"错误: 配置 '{name}' 不存在。");
                return;
            }

            try
            {
                CommandHelper.SetEnvironmentVariables(config);
                ConfigService.SetActiveConfig(name);
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteError($"错误: {ex.Message}");
            }
        });

        return command;
    }

    /// <summary>
    /// 创建 current/c 命令
    /// </summary>
    public static Command CreateCurrentCommand()
    {
        var command = new Command("current", "查看当前使用的配置");
        command.Aliases.Add("c");

        command.SetAction(_ =>
        {
            ConfigService.ShowCurrentConfig();
        });

        return command;
    }

    /// <summary>
    /// 创建 remove/del 命令
    /// </summary>
    public static Command CreateRemoveCommand()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "配置名称"
        };

        var command = new Command("remove", "删除指定配置")
        {
            nameArgument
        };
        command.Aliases.Add("del");

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArgument)!;
            ConfigService.RemoveConfig(name);
        });

        return command;
    }

    /// <summary>
    /// 创建 uninstall 命令
    /// </summary>
    public static Command CreateUninstallCommand()
    {
        var removeConfigOption = new Option<bool>(
            "--remove-config",
            "同时删除配置文件和配置目录"
        );

        var command = new Command("uninstall", "卸载全局命令")
        {
            removeConfigOption
        };

        command.SetAction(parseResult =>
        {
            var removeConfig = parseResult.GetValue(removeConfigOption);
            InstallService.Uninstall(removeConfig);
        });

        return command;
    }
}
