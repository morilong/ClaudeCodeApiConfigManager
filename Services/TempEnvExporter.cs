namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 临时环境变量导出器
/// 根据不同的 Shell 类型输出对应格式的环境变量设置命令
/// </summary>
public static class TempEnvExporter
{
    /// <summary>
    /// 导出环境变量命令
    /// </summary>
    /// <param name="shellType">Shell 类型</param>
    /// <param name="variables">环境变量字典</param>
    /// <returns>可被 eval 执行的命令字符串</returns>
    public static string Export(ShellType shellType, Dictionary<string, string> variables)
    {
        return shellType switch
        {
            ShellType.PowerShell => ExportPowerShell(variables),
            ShellType.Cmd => ExportCmd(variables),
            ShellType.GitBash => ExportBash(variables),
            ShellType.Bash => ExportBash(variables),
            ShellType.Zsh => ExportBash(variables),
            ShellType.Fish => ExportFish(variables),
            _ => ExportBash(variables)
        };
    }

    /// <summary>
    /// PowerShell 格式: $env:VAR="value"
    /// </summary>
    private static string ExportPowerShell(Dictionary<string, string> variables)
    {
        var lines = new List<string>();
        foreach (var (key, value) in variables)
        {
            var escapedValue = EscapePowerShellValue(value);
            lines.Add($"$env:{key}=\"{escapedValue}\"");
        }
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Bash/Zsh/Git Bash 格式: export VAR="value"
    /// </summary>
    private static string ExportBash(Dictionary<string, string> variables)
    {
        var lines = new List<string>();
        foreach (var (key, value) in variables)
        {
            var escapedValue = EscapeBashValue(value);
            lines.Add($"export {key}=\"{escapedValue}\"");
        }
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Fish 格式: set -x VAR "value"
    /// </summary>
    private static string ExportFish(Dictionary<string, string> variables)
    {
        var lines = new List<string>();
        foreach (var (key, value) in variables)
        {
            var escapedValue = EscapeFishValue(value);
            lines.Add($"set -x {key} \"{escapedValue}\"");
        }
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// CMD 格式: set VAR1=val1 && set VAR2=val2
    /// 单行输出，方便复制粘贴
    /// </summary>
    private static string ExportCmd(Dictionary<string, string> variables)
    {
        var parts = new List<string>();
        foreach (var (key, value) in variables)
        {
            // CMD 的 set 命令不使用引号，值中的空格不需要特殊处理
            // 但需要处理特殊字符
            var escapedValue = EscapeCmdValue(value);
            parts.Add($"set {key}={escapedValue}");
        }
        return string.Join(" && ", parts);
    }

    #region 转义方法

    /// <summary>
    /// 转义 PowerShell 字符串值
    /// </summary>
    private static string EscapePowerShellValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // PowerShell 中双引号内的特殊字符:
        // $ ` " 需要转义，使用 ` (反引号)
        var result = value;
        result = result.Replace("`", "``");  // 反引号本身先转义
        result = result.Replace("$", "`$");  // $ 符号
        result = result.Replace("\"", "`\""); // 双引号
        result = result.Replace("\n", "`n");  // 换行
        result = result.Replace("\r", "`r");  // 回车
        return result;
    }

    /// <summary>
    /// 转义 Bash/Zsh 字符串值
    /// </summary>
    private static string EscapeBashValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Bash 双引号内的特殊字符: $ ` " \ 需要转义
        var result = value;
        result = result.Replace("\\", "\\\\"); // 反斜杠先转义
        result = result.Replace("\"", "\\\""); // 双引号
        result = result.Replace("$", "\\$");   // $ 符号
        result = result.Replace("`", "\\`");   // 反引号
        return result;
    }

    /// <summary>
    /// 转义 Fish 字符串值
    /// </summary>
    private static string EscapeFishValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Fish 中双引号内的特殊字符: $ " \ 需要转义
        var result = value;
        result = result.Replace("\\", "\\\\"); // 反斜杠先转义
        result = result.Replace("\"", "\\\""); // 双引号
        result = result.Replace("$", "\\$");   // $ 符号
        return result;
    }

    /// <summary>
    /// 转义 CMD 值
    /// CMD 的 set 命令比较特殊，不支持引号包裹
    /// </summary>
    private static string EscapeCmdValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // CMD set 命令的特殊处理:
        // 1. 值末尾的空格会被保留，但通常不期望
        // 2. 特殊字符如 & | < > ^ 需要转义
        // 3. 使用 ^ 转义符
        var result = value;
        result = result.Replace("^", "^^");  // ^ 本身先转义
        result = result.Replace("&", "^&");  // &
        result = result.Replace("|", "^|");  // |
        result = result.Replace("<", "^<");  // <
        result = result.Replace(">", "^>");  // >
        result = result.Replace("(", "^(");  // (
        result = result.Replace(")", "^)");  // )
        return result;
    }

    #endregion
}
