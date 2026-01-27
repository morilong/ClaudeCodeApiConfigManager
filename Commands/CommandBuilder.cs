using System.CommandLine;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager.Commands;

/// <summary>
/// 命令构建器
/// </summary>
public static class CommandBuilder
{
    private static readonly ConfigManager ConfigManager = new();
    private static readonly ApiTester ApiTester = new();

    /// <summary>
    /// 创建 add 命令
    /// </summary>
    public static Command CreateAddCommand()
    {
        var nameArgument = new Argument<string>("name", "配置名称");
        var argsArgument = new Argument<string[]>("args", "API 配置参数 [TOKEN] [BASE_URL] [MODEL] [自定义参数...]")
        {
            Arity = ArgumentArity.OneOrMore
        };

        var command = new Command("add", "添加新的 API 配置")
        {
            nameArgument,
            argsArgument
        };

        command.SetHandler((name, args) =>
        {
            var config = CommandHelper.ParseAddArguments(name, args);

            // 验证必填字段
            if (string.IsNullOrEmpty(config.AuthToken))
            {
                Console.Error.WriteLine("错误: 未找到 API Token。");
                Console.Error.WriteLine("用法: ccm add <name> <TOKEN> <BASE_URL> <MODEL> [自定义参数...]");
                return;
            }

            if (string.IsNullOrEmpty(config.BaseUrl))
            {
                Console.Error.WriteLine("错误: 未找到 Base URL（必须以 http:// 或 https:// 开头）。");
                return;
            }

            if (string.IsNullOrEmpty(config.Model))
            {
                Console.Error.WriteLine("错误: 未指定模型名称。");
                return;
            }

            ConfigManager.AddOrUpdateConfig(config);
        }, nameArgument, argsArgument);

        return command;
    }

    /// <summary>
    /// 创建 list/ls 命令
    /// </summary>
    public static Command CreateListCommand()
    {
        var command = new Command("list", "列出所有已保存的配置");
        command.AddAlias("ls");

        command.SetHandler(() =>
        {
            ConfigManager.ListConfigs();
        });

        return command;
    }

    /// <summary>
    /// 创建 use 命令
    /// </summary>
    public static Command CreateUseCommand()
    {
        var nameArgument = new Argument<string>("name", "配置名称");

        var command = new Command("use", "切换到指定配置")
        {
            nameArgument
        };

        command.SetHandler((name) =>
        {
            var config = ConfigManager.GetConfig(name);
            if (config == null)
            {
                Console.Error.WriteLine($"错误: 配置 '{name}' 不存在。");
                return;
            }

            try
            {
                CommandHelper.SetEnvironmentVariables(config);
                ConfigManager.SetActiveConfig(name);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"错误: {ex.Message}");
            }
        }, nameArgument);

        return command;
    }

    /// <summary>
    /// 创建 current/c 命令
    /// </summary>
    public static Command CreateCurrentCommand()
    {
        var command = new Command("current", "查看当前使用的配置");
        command.AddAlias("c");

        command.SetHandler(() =>
        {
            ConfigManager.ShowCurrentConfig();
        });

        return command;
    }

    /// <summary>
    /// 创建 remove/del 命令
    /// </summary>
    public static Command CreateRemoveCommand()
    {
        var nameArgument = new Argument<string>("name", "配置名称");

        var command = new Command("remove", "删除指定配置")
        {
            nameArgument
        };
        command.AddAlias("del");

        command.SetHandler((name) =>
        {
            ConfigManager.RemoveConfig(name);
        }, nameArgument);

        return command;
    }

    /// <summary>
    /// 创建 test 命令
    /// </summary>
    public static Command CreateTestCommand()
    {
        var command = new Command("test", "测试当前配置的 API 连接");

        command.SetHandler(async () =>
        {
            var config = ConfigManager.GetActiveConfig();
            if (config == null)
            {
                Console.Error.WriteLine("错误: 未设置当前配置，无法测试。");
                Console.Error.WriteLine("请使用 'ccm use <name>' 命令设置配置。");
                return;
            }

            Console.WriteLine($"正在测试配置 '{config.Name}'...");
            await ApiTester.TestAsync(config);
        });

        return command;
    }
}
