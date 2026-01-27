#if WINDOWS

using System.Runtime.InteropServices;
using Microsoft.Win32;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Windows 平台环境变量管理器
/// </summary>
public static class WindowsEnvironmentManager
{
    private const string EnvironmentKeyPath = @"Environment";

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SendNotifyMessage(int hWnd, int Msg, nint wParam, string lParam);

    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        // 打开当前用户的环境变量注册表项
        using var key = Registry.CurrentUser.OpenSubKey(EnvironmentKeyPath, true);
        if (key == null)
        {
            Console.Error.WriteLine("错误: 无法打开环境变量注册表项。");
            return;
        }

        foreach (var variable in variables)
        {
            key.SetValue(variable.Key, variable.Value);
        }

        // 通知系统环境变量已更改
        NotifyEnvironmentChange();

        Console.WriteLine("环境变量已更新。请重启 Claude Code 以使更改生效。");
    }

    /// <summary>
    /// 发送 WM_SETTINGCHANGE 消息通知所有窗口
    /// </summary>
    private static void NotifyEnvironmentChange()
    {
        const int HWND_BROADCAST = 0xFFFF;
        const int WM_SETTINGCHANGE = 0x1A;

        SendNotifyMessage(HWND_BROADCAST, WM_SETTINGCHANGE, (nint)IntPtr.Zero, "Environment");
    }
}

#endif
