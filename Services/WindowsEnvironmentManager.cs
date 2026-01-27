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

        Console.WriteLine("环境变量已更新。请重启 Claude Code 以使更改生效。");
    }
}

#endif
