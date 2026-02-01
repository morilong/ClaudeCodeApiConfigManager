namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 控制台输出样式常量
/// 统一管理 Spectre.Console 的颜色和样式标记
/// </summary>
public static class ConsoleStyles
{
    /// <summary>
    /// 颜色标记
    /// </summary>
    public const string Success = "green";
    public const string Error = "red";
    public const string Warning = "yellow";
    public const string Info = "blue";
    public const string Dim = "dim";
    public const string Bold = "bold";

    /// <summary>
    /// 预定义的标记模板方法
    /// </summary>
    public static string SuccessMessage(string text) => $"[{Success}]{text}[/]";
    public static string ErrorMessage(string text) => $"[{Error}]{text}[/]";
    public static string WarningMessage(string text) => $"[{Warning}]{text}[/]";
    public static string InfoMessage(string text) => $"[{Info}]{text}[/]";
}
