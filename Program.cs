using System.CommandLine;
using System.Text;
using ClaudeCodeApiConfigManager.Commands;
using ClaudeCodeApiConfigManager.Services;

namespace ClaudeCodeApiConfigManager;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;

            // 检查是否请求显示版本
            if (VersionHelper.IsVersionRequest(args))
            {
                VersionHelper.PrintVersion();
                return 0;
            }

            // 无参数时运行初始化向导
            if (args.Length == 0)
            {
                var result = InitService.RunInitializeWizard();
                if (result != -1)
                {
                    return result;
                }
                // 已安装，正常使用
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
            rootCommand.Subcommands.Add(CommandBuilder.CreateSetTokenCommand());
            rootCommand.Subcommands.Add(CommandBuilder.CreateUninstallCommand());

            return rootCommand.Parse(args).Invoke();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("发生错误：");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }
}
