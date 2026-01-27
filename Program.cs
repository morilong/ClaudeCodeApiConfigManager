using System.CommandLine;
using System.Reflection;
using ClaudeCodeApiConfigManager.Commands;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // 检查是否请求显示版本（-v 短选项）
            if (args.Length == 1 && args[0] == "-v")
            {
                var assembly = Assembly.GetExecutingAssembly();
                var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                var version = informationalVersion?.Split('+')[0] ?? assembly.GetName().Version?.ToString() ?? "unknown";
                Console.WriteLine(version);
                return 0;
            }

            var rootCommand = new RootCommand
            {
                Description = "跨平台 CLI 工具，用于管理 Claude Code API 配置。"
            };

            rootCommand.AddOption(new Option<bool>(
                aliases: new[] { "-v" },
                description: "显示版本号"
            ));

            // 添加子命令
            rootCommand.AddCommand(CommandBuilder.CreateAddCommand());
            rootCommand.AddCommand(CommandBuilder.CreateListCommand());
            rootCommand.AddCommand(CommandBuilder.CreateUseCommand());
            rootCommand.AddCommand(CommandBuilder.CreateCurrentCommand());
            rootCommand.AddCommand(CommandBuilder.CreateRemoveCommand());

            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"发生错误: {ex.Message}");
            return 1;
        }
    }
}
