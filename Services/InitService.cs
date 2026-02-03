using System.Diagnostics;
using System.Text.Json;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 初始化服务，处理首次运行时的配置文件创建和全局安装
/// </summary>
public static class InitService
{
    private static readonly IConsoleOutput Output = new ConsoleOutput();

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
    public static int RunInitializeWizard(bool isForce)
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
                var selectedOption = InstallPromptService.PromptInstallDirectory(installPlan, isForce);
                if (selectedOption == null)
                {
                    Output.WriteLine("安装已取消。");
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
                if (!InstallPromptService.ConfirmInstallPlan(selectedOption, installPlan, isForce))
                {
                    Output.WriteLine("安装已取消。");
                    return 0;
                }

                // 执行全局安装（如果 PATH 不存在）
                if (!isInstalled)
                {
                    var installSuccess = InstallService.Install();
                    if (!installSuccess)
                    {
                        Output.Error("安装失败。");
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
            Output.Error("初始化失败：");
            Output.Error(ex.ToString());
            return 1;
        }
    }

#pragma warning disable CA1416 // 仅在 Windows 上支持
    private static int ShowInstalled()
    {
        if (Platform.IsWindows &&
            Console.Title?.Contains(Constants.Install.WinExeName, StringComparison.OrdinalIgnoreCase) == true)
        {
            Output.Warn("ccm 已安装，不支持双击 ccm.exe 使用。");
            Output.WriteLine("请打开新终端使用 'ccm -h' 查看可用命令。");
            Console.ReadKey();
            return 0;
        }
        return -1;
    }
#pragma warning restore CA1416

    /// <summary>
    /// 显示欢迎信息
    /// </summary>
    private static void ShowWelcomeMessage()
    {
        Output.WriteLine();
        Output.WriteLine(Constants.Messages.WelcomeMessage);
    }

    /// <summary>
    /// 创建默认配置文件
    /// </summary>
    private static bool CreateDefaultConfigFile(string settingsPath)
    {
        // 检查文件是否已存在
        if (File.Exists(settingsPath))
        {
            if (!Output.Confirm($"检测到 {settingsPath} {Constants.Messages.FileAlreadyExists}。是否覆盖?", false))
            {
                Output.WriteLine("跳过配置文件创建。");
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
            Output.Error("创建配置文件失败：");
            Output.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// 显示成功信息
    /// </summary>
    private static void ShowSuccessMessage(bool configExists, bool pathExists)
    {
        Output.WriteLine();
        if (!configExists)
        {
            Output.Success($"✓ {Constants.Messages.ConfigFileCreated}");
        }
        if (!pathExists)
        {
            Output.Success($"✓ {Constants.Messages.GlobalCommandInstalled}");
        }
        Output.WriteLine();
        Output.WriteLine("下一步:");
        Output.WriteLine("1. 运行 'ccm list' 查看可用配置");
        Output.WriteLine("2. 运行 'ccm setToken <name> <token>' 修改指定配置的 API Token");
        Output.WriteLine("3. 使用 'ccm add <name> <token> <url> <model>' 添加新配置");
        Output.WriteLine("4. 使用 'ccm use <name>' 切换配置");
        Output.WriteLine();
        Output.WriteLine("提示: 如需卸载，运行 'ccm uninstall'");
    }
}
