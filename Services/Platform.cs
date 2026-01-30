namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 平台检测
/// </summary>
public static class Platform
{
    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsUnix => IsLinux || IsMacOS;

    /// <summary>
    /// 获取配置目录路径
    /// </summary>
    public static string GetConfigDirectory()
    {
        string baseDir;

        if (IsWindows)
        {
            // Windows: 使用可执行文件所在目录
            baseDir = AppContext.BaseDirectory;
        }
        else
        {
            // Linux/macOS: ~/.config/ClaudeCodeApiConfigManager
            baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                Constants.Dirs.ConfigDir
            );
        }

        // 确保目录存在
        if (!Directory.Exists(baseDir))
        {
            Directory.CreateDirectory(baseDir);
        }

        return baseDir;
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetConfigDirectory(), Constants.Files.Settings);
    }

    /// <summary>
    /// 检查目录是否在 PATH 中
    /// </summary>
    public static bool IsDirectoryInPath(string dir)
    {
        var pathEnv = IsWindows ?
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) :
            Environment.GetEnvironmentVariable("PATH");
        var pathEntries = (pathEnv ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return pathEntries.Any(p => p.TrimEnd('\\', '/').Equals(dir.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase));
    }

}
