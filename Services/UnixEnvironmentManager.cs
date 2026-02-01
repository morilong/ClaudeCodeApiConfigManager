namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Unix 平台（Linux/macOS）环境变量管理器
/// </summary>
public static class UnixEnvironmentManager
{
    private static readonly IConsoleOutput Output = new ConsoleOutput();
    private static readonly string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string CcmDir = Path.Combine(HomeDir, Constants.Dirs.CcmDir);

    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        EnsureCcmDirectoryExists();

        var shellType = DetectShell();

        // 根据类型生成对应的环境变量脚本
        switch (shellType)
        {
            case ShellType.Fish:
                WriteFishEnvScript(variables);
                EnsureInitScript(GetFishConfigPath(), GetFishInitScriptContent(), Constants.Files.FishConfig);
                break;
            case ShellType.Bash:
            case ShellType.Zsh:
            default:
                WriteBashEnvScript(variables);
                var bashConfigPath = GetBashConfigPath(shellType);
                EnsureInitScript(bashConfigPath, GetBashInitScriptContent(), Path.GetFileName(bashConfigPath));
                break;
        }

        PrintReloadInstructions(shellType);
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
    /// 确保 .ccm 目录存在
    /// </summary>
    private static void EnsureCcmDirectoryExists()
    {
        if (!Directory.Exists(CcmDir))
        {
            Directory.CreateDirectory(CcmDir);
        }
    }

    /// <summary>
    /// 获取 Bash 配置文件路径
    /// </summary>
    private static string GetBashConfigPath(ShellType shellType)
    {
        if (shellType == ShellType.Zsh)
        {
            return Path.Combine(HomeDir, Constants.Files.Zshrc);
        }

        // 优先使用 .bashrc，如果不存在则使用 .bash_profile
        var bashrc = Path.Combine(HomeDir, Constants.Files.Bashrc);
        return File.Exists(bashrc) ? bashrc : Path.Combine(HomeDir, Constants.Files.BashProfile);
    }

    /// <summary>
    /// 获取 Fish 配置文件路径
    /// </summary>
    private static string GetFishConfigPath()
    {
        return Path.Combine(HomeDir, ".config", Constants.Dirs.FishConfigDir, Constants.Files.FishConfig);
    }

    /// <summary>
    /// 获取 Bash 初始化脚本内容
    /// </summary>
    private static string GetBashInitScriptContent()
    {
        return string.Join(Environment.NewLine,
            $"# {Constants.ShellMarkers.CcmMarker}",
            $"export {Constants.EnvVars.CcmHome}=\"$HOME/{Constants.Dirs.CcmDir}\"",
            $"if [[ -f \"${Constants.EnvVars.CcmHome}/{Constants.Files.BashEnv}\" ]]; then",
            $"    source \"${Constants.EnvVars.CcmHome}/{Constants.Files.BashEnv}\"",
            "fi"
        );
    }

    /// <summary>
    /// 获取 Fish 初始化脚本内容
    /// </summary>
    private static string GetFishInitScriptContent()
    {
        return string.Join(Environment.NewLine,
            $"# {Constants.ShellMarkers.CcmMarker}",
            $"set -x {Constants.EnvVars.CcmHome} \"$HOME/{Constants.Dirs.CcmDir}\"",
            $"if test -f \"${Constants.EnvVars.CcmHome}/{Constants.Files.FishEnv}\"",
            $"    source \"${Constants.EnvVars.CcmHome}/{Constants.Files.FishEnv}\"",
            "end"
        );
    }

    /// <summary>
    /// 确保配置文件中有初始化代码
    /// </summary>
    private static void EnsureInitScript(string configPath, string initScriptContent, string configFileName)
    {
        // 确保配置目录存在
        var configDir = Path.GetDirectoryName(configPath);
        if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        // 检查是否已经包含初始化代码
        if (File.Exists(configPath) && File.ReadAllText(configPath).Contains(Constants.ShellMarkers.CcmMarker))
        {
            return; // 已经存在
        }

        // 添加初始化代码
        using var writer = File.AppendText(configPath);
        writer.WriteLine();
        writer.WriteLine(initScriptContent);

        Output.Success($"已添加初始化代码到：{configPath}");
    }

    /// <summary>
    /// 写入 Bash/Zsh 环境变量脚本
    /// </summary>
    private static void WriteBashEnvScript(Dictionary<string, string> variables)
    {
        var envFile = Path.Combine(CcmDir, Constants.Files.BashEnv);
        using var writer = new StreamWriter(envFile);

        writer.WriteLine($"# ~/{Constants.Dirs.CcmDir}/{Constants.Files.BashEnv}");
        writer.WriteLine(Constants.Messages.FileAutoGenerated);
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
        var envFile = Path.Combine(CcmDir, Constants.Files.FishEnv);
        using var writer = new StreamWriter(envFile);

        writer.WriteLine($"# ~/{Constants.Dirs.CcmDir}/{Constants.Files.FishEnv}");
        writer.WriteLine(Constants.Messages.FileAutoGenerated);
        writer.WriteLine();

        foreach (var variable in variables)
        {
            writer.WriteLine($"set -x {variable.Key} \"{variable.Value}\"");
        }
    }

    /// <summary>
    /// 打印重新加载环境的说明
    /// </summary>
    private static void PrintReloadInstructions(ShellType shellType)
    {
        Output.WriteLine();
        Output.WriteLine(Constants.Messages.UnixEnvUpdate);
        Output.WriteLine();
        Output.WriteLine(Constants.Messages.UnixRunCmdEffective);

        string sourceCommand;
        if (shellType == ShellType.Fish)
        {
            sourceCommand = "source ~/.config/fish/config.fish";
        }
        else if (shellType == ShellType.Zsh)
        {
            sourceCommand = $"source ~/{Constants.Files.Zshrc}";
        }
        else
        {
            var bashrc = Path.Combine(HomeDir, Constants.Files.Bashrc);
            var configFile = File.Exists(bashrc) ? $"~/{Constants.Files.Bashrc}" : $"~/{Constants.Files.BashProfile}";
            sourceCommand = $"source {configFile}";
        }

        Output.Success(sourceCommand);
        Output.WriteLine();
        Output.WriteLine(Constants.Messages.UnixOpenNewWindowEffective);
    }
}
