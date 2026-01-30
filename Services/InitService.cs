using System.Diagnostics;
using System.Text.Json;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 初始化服务，处理首次运行时的配置文件创建和全局安装
/// </summary>
public static class InitService
{
    /// <summary>
    /// 检查是否需要初始化（配置文件不存在）
    /// </summary>
    public static bool ShouldInitialize()
    {
        var settingsPath = Platform.GetSettingsFilePath();
        return !File.Exists(settingsPath);
    }

    /// <summary>
    /// 运行初始化向导
    /// </summary>
    public static int RunInitializeWizard()
    {
        try
        {
            var configDir = Platform.GetConfigDirectory();
            if (File.Exists(Path.Combine(configDir, Constants.Files.Settings)) &&
                Platform.IsDirectoryInPath(configDir))
            {
                return ShowInstalled();
            }

            // 获取安装计划
            var installPlan = InstallService.DetectInstallPlan();

            // 确定配置文件和安装位置
            var settingsPath = Path.Combine(installPlan.ConfigDirectory, Constants.Files.Settings);

            // 检测状态
            var configExists = File.Exists(settingsPath);
            var pathExists = InstallService.IsInstallPathInPath();

            // 如果配置文件已存在且 PATH 已设置，不需要初始化
            if (configExists && pathExists)
            {
                return ShowInstalled();
            }

            try
            {
                // 显示欢迎信息
                ShowWelcomeMessage();

                // 根据状态显示需要执行的操作
                int stepNumber = 1;
                if (!configExists)
                {
                    Console.WriteLine($"\n{stepNumber}. 创建配置文件到: {settingsPath}");
                    Console.WriteLine("   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding");
                    stepNumber++;
                }
                if (!pathExists)
                {
                    Console.WriteLine($"\n{stepNumber}. {installPlan.Description}");
                }

                // 询问用户是否继续
                if (!ConfirmAction())
                {
                    Console.WriteLine("安装已取消。");
                    return 0;
                }

                // 创建配置文件（如果不存在）
                if (!configExists)
                {
                    if (!CreateDefaultConfigFile(settingsPath))
                    {
                        return 1;
                    }
                }

                // 执行全局安装（如果 PATH 不存在）
                if (!pathExists)
                {
                    var installSuccess = InstallService.Install();
                    if (!installSuccess)
                    {
                        Console.WriteLine("安装失败。");
                        return 1;
                    }
                }

                // 显示成功信息
                ShowSuccessMessage(configExists, pathExists);
            }
            finally
            {
                if (Platform.IsWindows)
                {
                    // 防止 Windows 控制台窗口闪退
                    Console.ReadKey();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"初始化失败: {ex.Message}");
            return 1;
        }
    }

    private static int ShowInstalled()
    {
        if (Platform.IsWindows &&
            Console.Title?.Contains(Constants.Install.WinExeName, StringComparison.OrdinalIgnoreCase) == true)
        {
            Console.WriteLine("ccm 已安装，不支持双击 ccm.exe 使用。");
            Console.WriteLine("请打开新终端使用 'ccm -h' 查看可用命令。");
            Console.ReadKey();
            return 0;
        }
        return -1;
    }

    /// <summary>
    /// 显示欢迎信息
    /// </summary>
    private static void ShowWelcomeMessage()
    {
        Console.WriteLine();
        Console.WriteLine(Constants.Messages.WelcomeMessage);
        Console.WriteLine();
        Console.WriteLine(Constants.Messages.FirstRunDetected);
    }

    /// <summary>
    /// 获取用户确认
    /// </summary>
    private static bool ConfirmAction()
    {
        Console.Write($"\n{Constants.Messages.AskContinue} ");
        var response = Console.ReadLine()?.Trim().ToLower();
        return string.IsNullOrEmpty(response) || response == "y" || response == "yes";
    }

    /// <summary>
    /// 创建默认配置文件
    /// </summary>
    private static bool CreateDefaultConfigFile(string settingsPath)
    {
        // 检查文件是否已存在
        if (File.Exists(settingsPath))
        {
            if (!PromptOverwrite(settingsPath))
            {
                Console.WriteLine("跳过配置文件创建。");
                return true;
            }
        }

        try
        {
            // 确保目录存在
            var directory = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 写入配置文件
            File.WriteAllText(settingsPath, Constants.ConfigTemplates.DefaultSettingsJson);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"创建配置文件失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 文件已存在时询问是否覆盖
    /// </summary>
    private static bool PromptOverwrite(string filePath)
    {
        Console.Write($"检测到 {filePath} {Constants.Messages.FileAlreadyExists}。");
        Console.Write($" {Constants.Messages.AskOverwrite} ");
        var response = Console.ReadLine()?.Trim().ToLower();
        return response == "y" || response == "yes";
    }

    /// <summary>
    /// 显示成功信息
    /// </summary>
    private static void ShowSuccessMessage(bool configExists, bool pathExists)
    {
        Console.WriteLine();
        if (!configExists)
        {
            Console.WriteLine($"✓ {Constants.Messages.ConfigFileCreated}");
        }
        if (!pathExists)
        {
            Console.WriteLine($"✓ {Constants.Messages.GlobalCommandInstalled}");
        }
        Console.WriteLine();
        Console.WriteLine("下一步:");
        Console.WriteLine("1. 运行 'ccm list' 查看可用配置");
        Console.WriteLine("2. 使用 'ccm add <name> <token> <url> <model>' 添加配置的 Token");
        Console.WriteLine("3. 使用 'ccm use <name>' 切换配置");
        Console.WriteLine();
        Console.WriteLine("提示: 如需卸载，运行 'ccm uninstall'");
    }
}
