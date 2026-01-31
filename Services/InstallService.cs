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
    private static InstallPlan? _cachedPlan;

    /// <summary>
    /// 检测安装计划（不执行安装）
    /// </summary>
    public static InstallPlan DetectInstallPlan()
    {
        if (_cachedPlan != null)
            return _cachedPlan;

        return Platform.IsWindows ?
            _cachedPlan = DetectWindowsInstallPlan() :
            _cachedPlan = DetectUnixInstallPlan();
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
    public static bool Install()
    {
        try
        {
            return Platform.IsWindows ? WindowsInstall() : UnixInstall();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("安装失败：");
            Console.Error.WriteLine(ex.ToString());
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
            Console.Error.WriteLine("卸载失败：");
            Console.Error.WriteLine(ex.ToString());
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
    private static bool WindowsInstall()
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
                Console.Error.WriteLine("找不到可执行文件。");
                return false;
            }

            var exeDest = Path.Combine(plan.InstallDirectory, Constants.Install.WinExeName);

            if (File.Exists(exeDest))
            {
                Console.Write($"检测到 {exeDest} {Constants.Messages.FileAlreadyExists}。");
                Console.Write($" {Constants.Messages.AskOverwrite} ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y" && response != "yes")
                {
                    Console.WriteLine("跳过文件复制。");
                }
                else
                {
                    File.Copy(exeSource, exeDest, true);
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
                Console.WriteLine($"已将 {directory} 添加到环境变量 PATH。");
                Console.WriteLine(Constants.Messages.WindowsRestartHint);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("无法修改 PATH 环境变量：");
            Console.Error.WriteLine(ex.ToString());
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
            Console.WriteLine($"检测到 ccm 安装目录: {installDir}");
        }
        Console.WriteLine();
        Console.WriteLine("这将卸载 ccm 全局命令：");

        if (existExeFile)
        {
            Console.WriteLine($"- 从 PATH 环境变量中移除: {installDir}");
            Console.WriteLine($"- 删除: {exeFilePath}");
        }

        // 询问是否删除配置文件
        if (removeConfig)
        {
            Console.WriteLine($"- 删除配置文件: {Platform.GetSettingsFilePath()}");
        }
        else
        {
            Console.WriteLine();
            Console.Write($"{Constants.Messages.AskRemoveConfig} ");
            var response = Console.ReadLine()?.Trim().ToLower();
            removeConfig = response == "y" || response == "yes";
        }

        Console.WriteLine();
        Console.WriteLine("正在卸载...");

        // 从 PATH 移除
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var pathEntries = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathEntries.RemoveAll(p => p.Equals(installDir, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                var newPath = string.Join(Path.PathSeparator, pathEntries);
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Console.WriteLine($"✓ {Constants.Messages.RemovedFromPath}: {installDir}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("无法从 PATH 移除目录：");
            Console.Error.WriteLine(ex.ToString());
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
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("删除配置文件失败：");
                Console.Error.WriteLine(ex.ToString());
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
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"删除 {Constants.Install.WinExeName} 失败：");
            Console.Error.WriteLine(ex.ToString());
        }

        Console.WriteLine();
        Console.WriteLine(Constants.Messages.UninstallComplete);
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
    /// Unix 安装主流程
    /// </summary>
    private static bool UnixInstall()
    {
        var plan = DetectInstallPlan();
        var targetDir = plan.InstallDirectory;
        var needSudo = targetDir.Equals(Constants.Install.UsrLocalBinDir, StringComparison.OrdinalIgnoreCase);
        var currentExe = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];

        // 确保目标目录存在
        if (!Directory.Exists(targetDir))
        {
            if (needSudo)
            {
                Console.WriteLine($"需要创建目录 {targetDir}，请输入 sudo 密码：");
                var result = ExecuteCommand("sudo", $"mkdir -p {targetDir}");
                if (result != 0)
                {
                    Console.Error.WriteLine($"无法创建目录 {targetDir}");
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
            Console.Write($"检测到 {linkPath} {Constants.Messages.FileAlreadyExists}。");
            Console.Write($" {Constants.Messages.AskOverwrite} ");
            var response = Console.ReadLine()?.Trim().ToLower();
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("跳过符号链接创建。");
                return true;
            }

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
            Console.WriteLine($"创建符号链接需要 sudo 权限，请输入密码：");
            var result = ExecuteCommand("sudo", $"ln -sf \"{currentExe}\" \"{linkPath}\"");
            if (result != 0)
            {
                Console.Error.WriteLine($"无法创建符号链接: {linkPath}");
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
            Console.WriteLine();
            Console.WriteLine($"注意: {targetDir} 不在 PATH 中。");
            Console.WriteLine("请添加以下内容到 ~/.bashrc 或 ~/.zshrc:");
            Console.WriteLine($"  export PATH=\"{targetDir}:$PATH\"");
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
            Console.WriteLine("未检测到全局安装。");
            return;
        }

        Console.WriteLine($"检测到 ccm 安装位置: {installedPath}");
        Console.WriteLine();
        Console.WriteLine("这将卸载 ccm 全局命令。");
        Console.WriteLine($"- 删除符号链接: {installedPath}");

        if (needSudo)
        {
            Console.WriteLine("- 注意: 此操作需要 sudo 权限");
        }

        // 询问是否删除配置文件
        if (removeConfig)
        {
            Console.WriteLine($"- 删除配置文件: {Platform.GetSettingsFilePath()}");
        }
        else
        {
            Console.WriteLine();
            Console.Write($"{Constants.Messages.AskRemoveConfig} ");
            var response = Console.ReadLine()?.Trim().ToLower();
            removeConfig = response == "y" || response == "yes";
        }

        Console.WriteLine();
        Console.WriteLine("正在卸载...");

        // 删除符号链接
        try
        {
            if (needSudo)
            {
                Console.WriteLine("删除符号链接需要 sudo 权限，请输入密码：");
                ExecuteCommand("sudo", $"rm -f \"{installedPath}\"");
            }
            else
            {
                File.Delete(installedPath);
            }
            Console.WriteLine($"✓ {Constants.Messages.SymlinkDeleted}: {installedPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("删除符号链接失败：");
            Console.Error.WriteLine(ex.ToString());
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
                }

                var configDir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(configDir) && Directory.Exists(configDir) &&
                    !Directory.EnumerateFileSystemEntries(configDir).Any())
                {
                    Directory.Delete(configDir);
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        Console.WriteLine();
        Console.WriteLine(Constants.Messages.UninstallComplete);
    }

    /// <summary>
    /// 执行 shell 命令
    /// </summary>
    private static int ExecuteCommand(string command, string arguments)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
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
