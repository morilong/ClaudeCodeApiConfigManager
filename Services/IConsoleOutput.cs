using Spectre.Console;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 控制台输出抽象接口
/// </summary>
public interface IConsoleOutput
{
    // ========== 基础方法（保留兼容性） ==========

    /// <summary>
    /// 写入文本（不换行）
    /// </summary>
    void Write(string message);

    /// <summary>
    /// 写入文本并换行
    /// </summary>
    void WriteLine(string message);

    /// <summary>
    /// 写入空行
    /// </summary>
    void WriteLine();

    // ========== Spectre.Console 扩展方法 ==========

    /// <summary>
    /// 使用 Spectre.Console 标记语法输出
    /// </summary>
    void Markup(string markup);

    /// <summary>
    /// 使用 Spectre.Console 标记语法输出并换行
    /// </summary>
    void MarkupLine(string markup);

    /// <summary>
    /// 输出成功消息（绿色）
    /// </summary>
    void Success(string message);

    /// <summary>
    /// 输出错误消息（红色）
    /// </summary>
    void Error(string message);

    /// <summary>
    /// 输出警告消息（黄色）
    /// </summary>
    void Warn(string message);

    /// <summary>
    /// 输出信息消息（蓝色）
    /// </summary>
    void Info(string message);

    /// <summary>
    /// 输出表格
    /// </summary>
    void WriteTable(Table table);

    /// <summary>
    /// 输出分隔线
    /// </summary>
    void WriteRule(string text);

    /// <summary>
    /// 显示确认提示
    /// </summary>
    /// <param name="prompt">提示文本</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>用户选择结果</returns>
    bool Confirm(string prompt, bool defaultValue = false);
}

/// <summary>
/// 默认控制台输出实现（原生 Console，保留用于简单场景或调试）
/// </summary>
public class ConsoleOutput : IConsoleOutput
{
    public virtual void Write(string message) => Console.Write(message);
    public virtual void WriteLine(string message) => Console.WriteLine(message);
    public virtual void WriteLine() => Console.WriteLine();

    // Spectre.Console 方法默认实现（使用 AnsiConsole 静态方法）
    public virtual void Markup(string markup) => AnsiConsole.Markup(markup);
    public virtual void MarkupLine(string markup) => AnsiConsole.MarkupLine(markup);
    public virtual void Success(string message) => AnsiConsole.MarkupLine($"[green]{Spectre.Console.Markup.Escape(message)}[/]");
    public virtual void Error(string message) => AnsiConsole.MarkupLine($"[red]{Spectre.Console.Markup.Escape(message)}[/]");
    public virtual void Warn(string message) => AnsiConsole.MarkupLine($"[yellow]{Spectre.Console.Markup.Escape(message)}[/]");
    public virtual void Info(string message) => AnsiConsole.MarkupLine($"[blue]{Spectre.Console.Markup.Escape(message)}[/]");
    public virtual void WriteTable(Table table) => AnsiConsole.Write(table);
    public virtual void WriteRule(string text) => AnsiConsole.Write(new Rule(text));
    public virtual bool Confirm(string prompt, bool defaultValue = false) => AnsiConsole.Confirm(prompt, defaultValue);
}
