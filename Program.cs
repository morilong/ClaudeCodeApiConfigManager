using System.CommandLine;
using ClaudeCodeApiConfigManager.Commands;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            // 检查是否请求显示版本
            if (VersionHelper.IsVersionRequest(args))
            {
                VersionHelper.PrintVersion();
                return 0;
            }

            var rootCommand = new RootCommand
            {
                Description = "一个跨平台的 Claude Code API 配置管理工具。"
            };

            // 添加版本选项
            rootCommand.Options.Add(VersionHelper.CreateVersionOption());

            // 添加子命令
            rootCommand.Subcommands.Add(CommandBuilder.CreateAddCommand());
            rootCommand.Subcommands.Add(CommandBuilder.CreateListCommand());
            rootCommand.Subcommands.Add(CommandBuilder.CreateUseCommand());
            rootCommand.Subcommands.Add(CommandBuilder.CreateCurrentCommand());
            rootCommand.Subcommands.Add(CommandBuilder.CreateRemoveCommand());

            return rootCommand.Parse(args).Invoke();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"发生错误: {ex.Message}");
            return 1;
        }
    }
}
