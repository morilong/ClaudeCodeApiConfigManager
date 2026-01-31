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
            if (Platform.IsWindows &&
                File.Exists(Path.Combine(configDir, Constants.Files.Settings)) &&
                Platform.IsDirectoryInPath(configDir))
            {
                return ShowInstalled();
            }

            // 获取安装计划
            var installPlan = InstallService.DetectInstallPlan();

            // 确定配置文件和安装位置
            var settingsPath = Path.Combine(installPlan.ConfigDirectory, Constants.Files.Settings);

            // 检测状态
            var isInstalled = InstallService.IsInstalled();

            // 如果已安装，不需要初始化
            if (isInstalled)
            {
                return ShowInstalled();
            }

            try
            {
                // 显示欢迎信息
                ShowWelcomeMessage();

                // 让用户选择安装目录
                var selectedOption = InstallPromptService.PromptInstallDirectory(installPlan);
                if (selectedOption == null)
                {
                    Console.WriteLine("安装已取消。");
                    return 0;
                }

                // 更新安装计划
                installPlan.InstallDirectory = selectedOption.Directory;
                installPlan.Description = selectedOption.Description;
                if (Platform.IsWindows)
                {
                    // 配置目录始终跟随安装目录
                    installPlan.ConfigDirectory = selectedOption.Directory;
                    // 重新计算配置文件路径
                    settingsPath = Path.Combine(installPlan.ConfigDirectory, Constants.Files.Settings);
                }

                // 确认安装计划
                if (!InstallPromptService.ConfirmInstallPlan(selectedOption, installPlan))
                {
                    Console.WriteLine("安装已取消。");
                    return 0;
                }

                // 执行全局安装（如果 PATH 不存在）
                if (!isInstalled)
                {
                    var installSuccess = InstallService.Install();
                    if (!installSuccess)
                    {
                        Console.WriteLine("安装失败。");
                        return 1;
                    }
                }

                // 创建配置文件（如果不存在）
                var configExists = File.Exists(settingsPath);
                if (!configExists)
                {
                    if (!CreateDefaultConfigFile(settingsPath))
                    {
                        return 1;
                    }
                }

                // 显示成功信息
                ShowSuccessMessage(configExists, isInstalled);
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
            Console.Error.WriteLine("初始化失败：");
            Console.Error.WriteLine(ex.ToString());
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
            Console.Error.WriteLine("创建配置文件失败：");
            Console.Error.WriteLine(ex.ToString());
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
