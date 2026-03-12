using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Shell 类型枚举
/// </summary>
public enum ShellType
{
    /// <summary>
    /// Windows PowerShell
    /// </summary>
    PowerShell,

    /// <summary>
    /// Windows CMD
    /// </summary>
    Cmd,

    /// <summary>
    /// Git Bash (Windows)
    /// </summary>
    GitBash,

    /// <summary>
    /// Bash (Unix)
    /// </summary>
    Bash,

    /// <summary>
    /// Zsh (Unix)
    /// </summary>
    Zsh,

    /// <summary>
    /// Fish (Unix)
    /// </summary>
    Fish
}

/// <summary>
/// Shell 类型检测器
/// </summary>
public static class ShellDetector
{
    /// <summary>
    /// 检测当前 Shell 类型
    /// 按优先级检测: Git Bash > Fish > Zsh > Bash > PowerShell > CMD
    /// </summary>
    public static ShellType Detect()
    {
        // 检测 Git Bash (Windows 上运行，但使用 Bash 语法)
        var msystem = Environment.GetEnvironmentVariable("MSYSTEM");
        if (!string.IsNullOrEmpty(msystem))
        {
            return ShellType.GitBash;
        }

        // 检测 Unix Shell (通过 $SHELL 环境变量)
        var shell = Environment.GetEnvironmentVariable("SHELL") ?? "";
        if (shell.Contains("fish", StringComparison.OrdinalIgnoreCase))
        {
            return ShellType.Fish;
        }
        if (shell.Contains("zsh", StringComparison.OrdinalIgnoreCase))
        {
            return ShellType.Zsh;
        }
        if (shell.Contains("bash", StringComparison.OrdinalIgnoreCase))
        {
            return ShellType.Bash;
        }

        // Windows 平台检测
        if (OperatingSystem.IsWindows())
        {
            // PowerShell 检测: 通过父进程名称判断
            if (IsRunningInPowerShell())
            {
                return ShellType.PowerShell;
            }

            // 默认为 CMD
            return ShellType.Cmd;
        }

        // Unix 默认返回 Bash
        return ShellType.Bash;
    }

    /// <summary>
    /// 获取 Shell 类型名称（用于显示）
    /// </summary>
    public static string GetDisplayName(ShellType shellType)
    {
        return shellType switch
        {
            ShellType.PowerShell => "PowerShell",
            ShellType.Cmd => "CMD",
            ShellType.GitBash => "Git Bash",
            ShellType.Bash => "Bash",
            ShellType.Zsh => "Zsh",
            ShellType.Fish => "Fish",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// 判断是否为 Windows 平台的 Shell
    /// </summary>
    public static bool IsWindowsShell(ShellType shellType)
    {
        return shellType == ShellType.PowerShell ||
               shellType == ShellType.Cmd ||
               shellType == ShellType.GitBash;
    }

    /// <summary>
    /// 判断是否为 Bash 兼容的 Shell (使用 export 语法)
    /// </summary>
    public static bool IsBashCompatible(ShellType shellType)
    {
        return shellType == ShellType.Bash ||
               shellType == ShellType.Zsh ||
               shellType == ShellType.GitBash;
    }

    /// <summary>
    /// 检测当前进程是否在 PowerShell 中运行
    /// 通过父进程名称来判断
    /// </summary>
    public static bool IsRunningInPowerShell()
    {
        var parentProcessName = GetParentProcessName();
        if (!string.IsNullOrEmpty(parentProcessName))
        {
            // PowerShell 进程名: powershell (Windows PowerShell), pwsh (PowerShell Core/7+)
            return parentProcessName.Contains("powershell", StringComparison.OrdinalIgnoreCase) ||
                   parentProcessName.Contains("pwsh", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    /// <summary>
    /// 获取父进程名称
    /// 使用 NtQueryInformationProcess API
    /// </summary>
    private static string? GetParentProcessName()
    {
        try
        {
            var parentProcessId = GetParentProcessId();
            if (parentProcessId > 0)
            {
                using var parentProcess = Process.GetProcessById(parentProcessId);
                return parentProcess.ProcessName;
            }
            else
            {
                Console.Error.WriteLine("获取父进程ID失败");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("获取父进程名称异常：" + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// 使用 NtQueryInformationProcess 获取父进程 ID
    /// </summary>
    private static int GetParentProcessId()
    {
        using var process = Process.GetCurrentProcess();
        var pbi = new PROCESS_BASIC_INFORMATION();
        var status = NtQueryInformationProcess(
            process.Handle,
            0, // ProcessBasicInformation
            ref pbi,
            Marshal.SizeOf(pbi),
            out int _
        );
        if (status == 0) // STATUS_SUCCESS
        {
            return pbi.InheritedFromUniqueProcessId;
        }
        return -1;
    }

    #region Windows API

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(
        IntPtr processHandle,
        int processInformationClass,
        ref PROCESS_BASIC_INFORMATION processInformation,
        int processInformationLength,
        out int returnLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public int InheritedFromUniqueProcessId;
    }

    #endregion
}
