namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Windows 平台环境变量管理器
/// </summary>
public static class WindowsEnvironmentManager
{
    private static readonly IConsoleOutput Output = new ConsoleOutput();

    public static void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.User);
        }

        Output.WriteLine(Constants.Messages.WindowsEnvUpdate);
        Output.WriteLine(Constants.Messages.WindowsRestartHint);
    }
}
