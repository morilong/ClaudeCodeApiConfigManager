#if UNIX

using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Unix 平台（Linux/macOS）环境变量管理器
/// </summary>
public static class UnixEnvironmentManager
{
    private readonly static string _homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly static string _ccmDir = Path.Combine(_homeDir, ".ccm");

    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        // 确保 .ccm 目录存在
        if (!Directory.Exists(_ccmDir))
        {
            Directory.CreateDirectory(_ccmDir);
        }

        // 检测用户 shell
        var shellType = DetectShell();

        // 根据类型生成对应的环境变量脚本
        switch (shellType)
        {
            case ShellType.Fish:
                WriteFishEnvScript(variables);
                EnsureFishInitScript();
                break;
            case ShellType.Bash:
            case ShellType.Zsh:
            default:
                WriteBashEnvScript(variables);
                EnsureBashInitScript(shellType);
                break;
        }

        Console.WriteLine();
        Console.WriteLine("环境变量已更新。");
        Console.WriteLine($"请运行以下命令使更改生效：");
        if (shellType == ShellType.Fish)
        {
            Console.WriteLine("  source ~/.config/fish/config.fish");
        }
        else
        {
            Console.WriteLine("  source ~/.bashrc  # 或 ~/.zshrc");
        }
    }

    /// <summary>
    /// Shell 类型
    /// </summary>
    private enum ShellType
    {
        Bash,
        Zsh,
        Fish,
        Unknown
    }

    /// <summary>
    /// 检测用户使用的 shell
    /// </summary>
    private static ShellType DetectShell()
    {
        var shellPath = Environment.GetEnvironmentVariable("SHELL") ?? "";

        if (string.IsNullOrEmpty(shellPath))
        {
            return ShellType.Bash; // 默认
        }

        return shellPath switch
        {
            var s when s.Contains("fish") => ShellType.Fish,
            var s when s.Contains("zsh") => ShellType.Zsh,
            var s when s.Contains("bash") => ShellType.Bash,
            _ => ShellType.Unknown
        };
    }

    /// <summary>
    /// 写入 Bash/Zsh 环境变量脚本
    /// </summary>
    private static void WriteBashEnvScript(Dictionary<string, string> variables)
    {
        var envFile = Path.Combine(_ccmDir, "env.sh");
        using var writer = new StreamWriter(envFile);

        writer.WriteLine("# ~/.ccm/env.sh");
        writer.WriteLine("# 此文件由 ccm 自动生成，请勿手动编辑");
        writer.WriteLine();

        foreach (var variable in variables)
        {
            writer.WriteLine($"export {variable.Key}=\"{variable.Value}\"");
        }
    }

    /// <summary>
    /// 写入 Fish 环境变量脚本
    /// </summary>
    private static void WriteFishEnvScript(Dictionary<string, string> variables)
    {
        var envFile = Path.Combine(_ccmDir, "env.fish");
        using var writer = new StreamWriter(envFile);

        writer.WriteLine("# ~/.ccm/env.fish");
        writer.WriteLine("# 此文件由 ccm 自动生成，请勿手动编辑");
        writer.WriteLine();

        foreach (var variable in variables)
        {
            writer.WriteLine($"set -x {variable.Key} \"{variable.Value}\"");
        }
    }

    /// <summary>
    /// 确保 Bash/Zsh 配置文件中有初始化代码
    /// </summary>
    private static void EnsureBashInitScript(ShellType shellType)
    {
        string configFile;
        string marker = "# ccm - Claude Code Config Manager";

        if (shellType == ShellType.Zsh)
        {
            configFile = Path.Combine(_homeDir, ".zshrc");
        }
        else
        {
            // 优先使用 .bashrc，如果不存在则使用 .bash_profile
            var bashrc = Path.Combine(_homeDir, ".bashrc");
            configFile = File.Exists(bashrc) ? bashrc : Path.Combine(_homeDir, ".bash_profile");
        }

        // 检查是否已经包含初始化代码
        if (File.Exists(configFile) && File.ReadAllText(configFile).Contains(marker))
        {
            return; // 已经存在
        }

        // 添加初始化代码
        using var writer = File.AppendText(configFile);
        writer.WriteLine();
        writer.WriteLine($"# {marker}");
        writer.WriteLine("export CCM_HOME=\"$HOME/.ccm\"");
        writer.WriteLine("if [[ -f \"$CCM_HOME/env.sh\" ]]; then");
        writer.WriteLine("    source \"$CCM_HOME/env.sh\"");
        writer.WriteLine("fi");

        Console.WriteLine($"已添加初始化代码到 {configFile}");
    }

    /// <summary>
    /// 确保 Fish 配置文件中有初始化代码
    /// </summary>
    private static void EnsureFishInitScript()
    {
        var fishConfigDir = Path.Combine(_homeDir, ".config", "fish");
        var configFile = Path.Combine(fishConfigDir, "config.fish");
        string marker = "# ccm - Claude Code Config Manager";

        // 确保 fish 配置目录存在
        if (!Directory.Exists(fishConfigDir))
        {
            Directory.CreateDirectory(fishConfigDir);
        }

        // 检查是否已经包含初始化代码
        if (File.Exists(configFile) && File.ReadAllText(configFile).Contains(marker))
        {
            return; // 已经存在
        }

        // 添加初始化代码
        using var writer = File.AppendText(configFile);
        writer.WriteLine();
        writer.WriteLine($"# {marker}");
        writer.WriteLine("set -x CCM_HOME \"$HOME/.ccm\"");
        writer.WriteLine("if test -f \"$CCM_HOME/env.fish\"");
        writer.WriteLine("    source \"$CCM_HOME/env.fish\"");
        writer.WriteLine("end");

        Console.WriteLine($"已添加初始化代码到 {configFile}");
    }
}

#endif
