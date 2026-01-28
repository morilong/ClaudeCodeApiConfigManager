using System.Reflection;
using System.CommandLine;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 版本信息辅助类
/// </summary>
public static class VersionHelper
{
    /// <summary>
    /// 获取当前程序版本号
    /// </summary>
    public static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return informationalVersion?.Split('+')[0]
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }

    /// <summary>
    /// 打印版本号到控制台
    /// </summary>
    public static void PrintVersion()
    {
        Console.WriteLine(GetVersion());
    }

    /// <summary>
    /// 创建版本选项（用于添加到 RootCommand）
    /// </summary>
    public static Option<bool> CreateVersionOption()
    {
        return new Option<bool>("-v") { Description = "显示版本号" };
    }

    /// <summary>
    /// 检查参数是否为版本请求
    /// </summary>
    public static bool IsVersionRequest(string[] args)
    {
        return args.Length == 1 && args[0] == "-v";
    }
}
