using System.CommandLine;
using ClaudeCodeApiConfigManager.Commands;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var rootCommand = new RootCommand("Claude Code API 配置管理工具")
            {
                Description = "跨平台 CLI 工具，用于管理 Claude Code API 配置。"
            };

            // 添加子命令
            rootCommand.AddCommand(CommandBuilder.CreateAddCommand());
            rootCommand.AddCommand(CommandBuilder.CreateListCommand());
            rootCommand.AddCommand(CommandBuilder.CreateUseCommand());
            rootCommand.AddCommand(CommandBuilder.CreateCurrentCommand());
            rootCommand.AddCommand(CommandBuilder.CreateRemoveCommand());
            rootCommand.AddCommand(CommandBuilder.CreateTestCommand());

            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"发生错误: {ex.Message}");
            return 1;
        }
    }
}
