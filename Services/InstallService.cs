using System.Diagnostics;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 安装状态枚举
/// </summary>
public enum InstallStatus
{
    NotInstalled,
    Installed,
    NeedsUpdate
}

/// <summary>
/// 安装结果类
/// </summary>
public class InstallResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? InstallPath { get; set; }
    public string? InstallMethod { get; set; }

    public static InstallResult Ok(string? installPath = null, string? installMethod = null) =>
        new() { Success = true, InstallPath = installPath, InstallMethod = installMethod };

    public static InstallResult Fail(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// 安装计划类
/// </summary>
public class InstallPlan
{
    public string Description { get; set; } = string.Empty;
    public string ConfigDirectory { get; set; } = string.Empty;
    public string InstallDirectory { get; set; } = string.Empty;
    public string InstallMethod { get; set; } = string.Empty;
}

/// <summary>
/// 平台特定的安装/卸载服务
/// </summary>
public static class InstallService
{
    private static readonly IConsoleOutput Output = new ConsoleOutput();
    private static InstallPlan? CachedPlan;

    /// <summary>
    /// 检测安装计划（不执行安装）
    /// </summary>
    public static InstallPlan DetectInstallPlan()
    {
        if (CachedPlan != null)
            return CachedPlan;

        return Platform.IsWindows ?
            CachedPlan = DetectWindowsInstallPlan() :
            CachedPlan = DetectUnixInstallPlan();
    }

    /// <summary>
    /// 检查是否已安装
    /// </summary>
    public static bool IsInstalled()
    {
        if (Platform.IsWindows)
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var pathEntries = (pathEnv ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in pathEntries)
            {
                var exePath = Path.Combine(path, Constants.Install.WinExeName);
                if (File.Exists(exePath))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return ExecuteOnUnix("ccm", "-v").Code == 0;
        }
    }

    /// <summary>
    /// 执行全局安装
    /// </summary>
    public static bool Install(bool isForce)
    {
        try
        {
            return Platform.IsWindows ? WindowsInstall(isForce) : UnixInstall();
        }
        catch (Exception ex)
        {
            Output.Error("安装失败：");
            Output.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// 执行卸载
    /// </summary>
    public static void Uninstall(bool removeConfig = false)
    {
        try
        {
            if (Platform.IsWindows)
            {
                WindowsUninstall(removeConfig);
            }
            else
            {
                UnixUninstall(removeConfig);
            }
        }
        catch (Exception ex)
        {
            Output.Error("卸载失败：");
            Output.Error(ex.ToString());
        }
    }

    #region Windows 安装实现

    private static string GetWinUserProfileCcmDir()
    {
        var profileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(profileDir, Constants.Install.WinCcmDir);
    }

    /// <summary>
    /// 检测 Windows 安装计划
    /// </summary>
    private static InstallPlan DetectWindowsInstallPlan()
    {
        var currentDir = Directory.GetCurrentDirectory().TrimEnd('\\', '/');

        string configDir;
        string installPath;
        string installMethod;

        if (IsDirectoryClean(currentDir))
        {
            // 非 C 盘且目录干净：添加当前目录到 PATH
            configDir = currentDir;
            installPath = currentDir;
            installMethod = $"将当前目录 {currentDir} 添加到环境变量 PATH";
        }
        else
        {
            // 复制到 .ccm 目录
            var ccmDir = GetWinUserProfileCcmDir();
            configDir = ccmDir;
            installPath = ccmDir;
            installMethod = $"复制 ccm.exe 到 {ccmDir} 并添加到环境变量 PATH";
        }

        return new InstallPlan
        {
            Description = installMethod,
            ConfigDirectory = configDir,
            InstallDirectory = installPath,
            InstallMethod = installMethod
        };
    }

    /// <summary>
    /// 检查目录是否只包含 ccm.exe 和其他少量非 .exe 文件
    /// </summary>
    public static bool IsDirectoryClean(string directory)
    {
        try
        {
            var fileOrDirNames = Directory.EnumerateFileSystemEntries(directory)
                .Select(path => Path.GetFileName(path))
                .Where(fileName =>
                {
                    // 排除 api-ms-win-*.dll
                    var isMsApiDll = fileName.StartsWith("api-ms-win-", StringComparison.OrdinalIgnoreCase) &&
                        fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                    return !isMsApiDll;
                }).ToArray();
            if (fileOrDirNames.Length > 10)
            {
                return false; // 文件过多
            }
            foreach (var fileName in fileOrDirNames)
            {
                if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Equals(Constants.Install.WinExeName, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // 发现其他.exe
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Windows 安装主流程
    /// </summary>
    private static bool WindowsInstall(bool isForce)
    {
        var plan = DetectInstallPlan();
        var baseDir = AppContext.BaseDirectory;
        var isCDriveInstall = plan.InstallDirectory.Contains(Constants.Install.WinCcmDir);

        // 如果需要复制文件
        if (isCDriveInstall || !IsDirectoryClean(baseDir))
        {
            // 创建目标目录
            if (!Directory.Exists(plan.InstallDirectory))
            {
                Directory.CreateDirectory(plan.InstallDirectory);
            }

            // 查找可执行文件（可能是 .exe 或 .dll）
            var exeSource = FindExecutableFile(baseDir);
            if (exeSource == null)
            {
                Output.Error("找不到可执行文件。");
                return false;
            }

            var exeDest = Path.Combine(plan.InstallDirectory, Constants.Install.WinExeName);

            if (File.Exists(exeDest))
            {
                if (isForce || Output.Confirm($"检测到 {exeDest} 已存在。是否覆盖？", true))
                {
                    File.Copy(exeSource, exeDest, true);
                }
                else
                {
                    Output.WriteLine("跳过文件复制。");
                }
            }
            else
            {
                File.Copy(exeSource, exeDest);
            }
        }

        // 添加到环境变量 PATH
        if (!Platform.IsDirectoryInPath(plan.InstallDirectory))
        {
            AddToUserPath(plan.InstallDirectory);
        }

        return true;
    }

    /// <summary>
    /// 查找可执行文件（.exe 或 .dll）
    /// </summary>
    private static string? FindExecutableFile(string directory)
    {
        // 首先尝试找 .exe 文件
        var exePath = Path.Combine(directory, Constants.Install.WinExeName);
        if (File.Exists(exePath))
        {
            return exePath;
        }

        // 如果没有 .exe，尝试找同名的 .dll 文件（用于开发环境）
        var dllName = Path.GetFileNameWithoutExtension(Constants.Install.WinExeName) + ".dll";
        var dllPath = Path.Combine(directory, dllName);
        if (File.Exists(dllPath))
        {
            return dllPath;
        }

        // 查找任何 .exe 或 .dll 文件
        var files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLower();
            if (ext == ".exe" || ext == ".dll")
            {
                return file;
            }
        }

        return null;
    }

    /// <summary>
    /// 添加目录到用户 PATH
    /// </summary>
    private static void AddToUserPath(string directory)
    {
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var pathEntries = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!pathEntries.Any(p => p.Equals(directory, StringComparison.OrdinalIgnoreCase)))
            {
                pathEntries.Add(directory);
                var newPath = string.Join(Path.PathSeparator, pathEntries);
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Output.Success($"已将 {directory} 添加到环境变量 PATH。");
                Output.WriteLine(Constants.Messages.WindowsRestartHint);
            }
        }
        catch (Exception ex)
        {
            Output.Error("无法修改 PATH 环境变量：");
            Output.Error(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Windows 卸载主流程
    /// </summary>
    private static void WindowsUninstall(bool removeConfig)
    {
        var installDir = AppContext.BaseDirectory.TrimEnd('\\', '/');
        var exeFilePath = Path.Combine(installDir, Constants.Install.WinExeName);
        var existExeFile = File.Exists(exeFilePath);

        if (existExeFile)
        {
            Output.WriteLine($"检测到 ccm 安装目录: {installDir}");
        }
        Output.WriteLine();
        Output.WriteLine("这将卸载 ccm 全局命令：");

        if (existExeFile)
        {
            Output.Warn($"- 从 PATH 环境变量中移除: {installDir}");
            Output.Warn($"- 删除: {exeFilePath}");
        }

        // 询问是否删除配置文件
        if (removeConfig)
        {
            Output.Warn($"- 删除配置文件: {Platform.GetSettingsFilePath()}");
        }
        else
        {
            Output.WriteLine();
            removeConfig = Output.Confirm(Constants.Messages.AskRemoveConfig, false);
        }

        Output.WriteLine();
        Output.WriteLine("正在卸载...");

        // 从 PATH 移除
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var pathEntries = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathEntries.RemoveAll(p => p.Equals(installDir, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                var newPath = string.Join(Path.PathSeparator, pathEntries);
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Output.Success($"✓ {Constants.Messages.RemovedFromPath}: {installDir}");
            }
        }
        catch (Exception ex)
        {
            Output.Error("无法从 PATH 移除目录：");
            Output.Error(ex.ToString());
        }

        // 删除配置文件
        if (removeConfig)
        {
            try
            {
                var settingsPath = Path.Combine(installDir, Constants.Files.Settings);
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                    Output.Success($"✓ {Constants.Messages.ConfigFileDeleted}：{settingsPath}");
                }
            }
            catch (Exception ex)
            {
                Output.Error("删除配置文件失败：");
                Output.Error(ex.ToString());
            }
        }

        // 启动独立 cmd 进程执行删除复制的.exe文件 + 删除空的安装目录
        try
        {
            if (existExeFile)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C ping 127.0.0.1 -n 2 >nul & del \"{exeFilePath}\" & rd \"{installDir}\" 2>nul",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Output.Success($"✓ 已删除：{exeFilePath}");
                Output.WriteLine();
                Output.WriteLine(Constants.Messages.UninstallComplete);

                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Output.Error($"删除 {Constants.Install.WinExeName} 失败：");
            Output.Error(ex.ToString());
        }
    }

    #endregion

    #region Unix 安装实现

    /// <summary>
    /// 检测 Unix 安装计划
    /// </summary>
    private static InstallPlan DetectUnixInstallPlan()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var localBinPath = Path.Combine(homeDir, Constants.Install.LocalBinDir);
        var usrLocalBinPath = Constants.Install.UsrLocalBinDir;

        string installPath;
        string installMethod;

        // 检查 PATH 环境变量
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathEntries = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        var localBinInPath = pathEntries.Any(p => p.Equals(localBinPath, StringComparison.OrdinalIgnoreCase));
        var usrLocalBinInPath = pathEntries.Any(p => p.Equals(usrLocalBinPath, StringComparison.OrdinalIgnoreCase));

        if (localBinInPath)
        {
            installPath = localBinPath;
            installMethod = $"创建符号链接到 {localBinPath}/ccm";
        }
        else if (usrLocalBinInPath)
        {
            installPath = usrLocalBinPath;
            installMethod = $"创建符号链接到 {usrLocalBinPath}/ccm (需要 sudo 权限)";
        }
        else
        {
            installPath = localBinPath;
            installMethod = $"创建符号链接到 {localBinPath}/ccm";
        }

        return new InstallPlan
        {
            Description = installMethod,
            ConfigDirectory = Path.Combine(homeDir, ".config", Constants.Dirs.ConfigDir),
            InstallDirectory = installPath,
            InstallMethod = installMethod
        };
    }

    /// <summary>
    /// 检查指定目录是否在 PATH
    /// </summary>
    private static bool IsInPath(string dir)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathEntries = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return pathEntries.Any(p => p.Equals(dir, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取 Unix 永久安装目录（用于存放实际可执行文件）
    /// </summary>
    private static string GetUnixInstallBinDir()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".local", "share", "ccm");
    }

    /// <summary>
    /// 检查路径是否为临时目录
    /// </summary>
    private static bool IsTempDirectory(string path)
    {
        var normalizedPath = path.Replace('\\', '/');
        return normalizedPath.Contains("/tmp/") ||
               normalizedPath.StartsWith("/tmp/") ||
               normalizedPath.Contains("ccm-install-");
    }

    /// <summary>
    /// Unix 安装主流程
    /// </summary>
    private static bool UnixInstall()
    {
        var plan = DetectInstallPlan();
        var targetDir = plan.InstallDirectory;
        var needSudo = targetDir.Equals(Constants.Install.UsrLocalBinDir, StringComparison.OrdinalIgnoreCase);
        var currentExe = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];

        // 如果当前可执行文件在临时目录，先复制到永久位置
        if (IsTempDirectory(currentExe))
        {
            var installBinDir = GetUnixInstallBinDir();
            Directory.CreateDirectory(installBinDir);

            // 从文件名提取版本号（如 1.0.0）
            var version = Path.GetFileName(currentExe);

            // 直接用版本号作为文件名，支持多版本共存
            var destExePath = Path.Combine(installBinDir, version);

            File.Copy(currentExe, destExePath, true);
            Output.Success($"已复制 ccm v{version} 到目录: {installBinDir}/");

            // 设置可执行权限
            ExecuteCommand("chmod", $"+x \"{destExePath}\"");
            currentExe = destExePath;
        }

        // 确保目标目录存在
        if (!Directory.Exists(targetDir))
        {
            if (needSudo)
            {
                Output.WriteLine($"需要创建目录 {targetDir}，请输入 sudo 密码：");
                var result = ExecuteCommand("sudo", $"mkdir -p {targetDir}");
                if (result != 0)
                {
                    Output.Error($"无法创建目录 {targetDir}");
                    return false;
                }
            }
            else
            {
                Directory.CreateDirectory(targetDir);
            }
        }

        var linkPath = Path.Combine(targetDir, Constants.Install.UnixExeName);

        // 检查符号链接是否已存在
        if (File.Exists(linkPath) || Directory.Exists(linkPath))
        {
            // 删除现有链接
            if (needSudo)
            {
                ExecuteCommand("sudo", $"rm -f {linkPath}");
            }
            else
            {
                if (File.Exists(linkPath))
                {
                    File.Delete(linkPath);
                }
                else if (Directory.Exists(linkPath))
                {
                    Directory.Delete(linkPath);
                }
            }
        }

        // 创建符号链接
        if (needSudo)
        {
            Output.WriteLine($"创建符号链接需要 sudo 权限，请输入密码：");
            var result = ExecuteCommand("sudo", $"ln -sf \"{currentExe}\" \"{linkPath}\"");
            if (result != 0)
            {
                Output.Error($"无法创建符号链接: {linkPath}");
                return false;
            }
        }
        else
        {
            File.CreateSymbolicLink(linkPath, currentExe);
        }

        // 如果目录不在 PATH 中，显示警告
        if (!IsInPath(targetDir))
        {
            Output.WriteLine();
            Output.Warn($"注意: {targetDir} 不在 PATH 中。");
            Output.Warn("请添加以下内容到 ~/.bashrc 或 ~/.zshrc:");
            Output.Warn($"  export PATH=\"{targetDir}:$PATH\"");
        }

        return true;
    }

    /// <summary>
    /// Unix 卸载主流程
    /// </summary>
    private static void UnixUninstall(bool removeConfig)
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var localBinLink = Path.Combine(homeDir, Constants.Install.LocalBinDir, Constants.Install.UnixExeName);
        var usrLocalBinLink = Path.Combine(Constants.Install.UsrLocalBinDir, Constants.Install.UnixExeName);

        string? installedPath = null;
        bool needSudo = false;

        if (File.Exists(localBinLink))
        {
            installedPath = localBinLink;
        }
        else if (File.Exists(usrLocalBinLink))
        {
            installedPath = usrLocalBinLink;
            needSudo = true;
        }

        if (installedPath == null)
        {
            Output.WriteLine("未检测到全局安装。");
            return;
        }

        Output.WriteLine($"检测到 ccm 安装位置: {installedPath}");
        Output.WriteLine();
        Output.WriteLine("这将卸载 ccm 全局命令。");
        Output.Warn($"- 删除符号链接: {installedPath}");

        if (needSudo)
        {
            Output.Warn("- 注意: 此操作需要 sudo 权限");
        }

        // 询问是否删除配置文件
        if (removeConfig)
        {
            Output.Warn($"- 删除配置文件: {Platform.GetSettingsFilePath()}");
        }
        else
        {
            Output.WriteLine();
            removeConfig = Output.Confirm(Constants.Messages.AskRemoveConfig, false);
        }

        Output.WriteLine();
        Output.WriteLine("正在卸载...");

        // 删除符号链接
        try
        {
            if (needSudo)
            {
                Output.WriteLine("删除符号链接需要 sudo 权限，请输入密码：");
                ExecuteCommand("sudo", $"rm -f \"{installedPath}\"");
            }
            else
            {
                File.Delete(installedPath);
            }
            Output.Success($"✓ {Constants.Messages.SymlinkDeleted}：{installedPath}");
        }
        catch (Exception ex)
        {
            Output.Error("删除符号链接失败：");
            Output.Error(ex.ToString());
        }

        // 删除配置文件
        if (removeConfig)
        {
            try
            {
                var settingsPath = Platform.GetSettingsFilePath();
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                    Output.Success($"✓ {Constants.Messages.ConfigFileDeleted}：{settingsPath}");
                }

                var configDir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(configDir) && Directory.Exists(configDir) &&
                    !Directory.EnumerateFileSystemEntries(configDir).Any())
                {
                    Directory.Delete(configDir);
                    Output.Success($"✓ {Constants.Messages.ConfigDirDeleted}：{configDir}");
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        Output.WriteLine();
        Output.WriteLine(Constants.Messages.UninstallComplete);
    }

    /// <summary>
    /// 执行 shell 命令
    /// </summary>
    private static int ExecuteCommand(string command, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch
        {
            return -1;
        }
    }

    private static (int Code, string Output, string Error) ExecuteOnUnix(string command, string arguments)
    {
        string fullCommand = $"{command} {arguments} 2>&1";
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"{fullCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return (-1, "", "启动进程失败。");
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, output, error);
    }

    #endregion

}
