#if WINDOWS

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Windows 平台环境变量管理器
/// </summary>
public static class WindowsEnvironmentManager
{
    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.User);
        }

        Console.WriteLine("环境变量已更新。");
        Console.WriteLine("打开新终端窗口 或 重启ClaudeCode程序 使更改生效。");
    }
}

#endif
