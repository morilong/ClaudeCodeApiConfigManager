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

        Console.WriteLine(Constants.Messages.WindowsEnvUpdate);
        Console.WriteLine(Constants.Messages.WindowsRestartHint);
    }
}

#endif
