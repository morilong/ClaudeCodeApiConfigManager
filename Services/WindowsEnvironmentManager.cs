#if WINDOWS

using System.Runtime.InteropServices;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Windows 平台环境变量管理器
/// </summary>
public static class WindowsEnvironmentManager
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SendNotifyMessage(int hWnd, int Msg, nint wParam, string lParam);

    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.User);
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

        SendNotifyMessage(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment");
    }
}

#endif
