using System.CommandLine;
using System.Reflection;
using ClaudeCodeApiConfigManager.Commands;

namespace ClaudeCodeApiConfigManager;

class Program
{
    static int Main(string[] args)
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

            rootCommand.Options.Add(new Option<bool>("-v") { Description = "显示版本号" });

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
