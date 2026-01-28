namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 控制台输出抽象接口
/// </summary>
public interface IConsoleOutput
{
    /// <summary>
    /// 写入文本（不换行）
    /// </summary>
    void Write(string message);

    /// <summary>
    /// 写入文本并换行
    /// </summary>
    void WriteLine(string message);

    /// <summary>
    /// 写入错误信息
    /// </summary>
    void WriteError(string message);

    /// <summary>
    /// 读取用户输入
    /// </summary>
    string? ReadLine();
}

/// <summary>
/// 默认控制台输出实现
/// </summary>
public class ConsoleOutput : IConsoleOutput
{
    public void Write(string message) => Console.Write(message);
    public void WriteLine(string message) => Console.WriteLine(message);
    public void WriteError(string message) => Console.Error.WriteLine(message);
    public string? ReadLine() => Console.ReadLine();
}
