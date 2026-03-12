# Design: Spectre.Console 控制台输出改造

## Context

当前项目是一个跨平台 .NET 10 控制台应用，使用原生 `Console.Write*` 进行输出。项目已经引入了 Spectre.Console 0.54.0 包，但在 `InstallPromptService.cs` 中直接使用 `AnsiConsole` 静态类，其他 Service 文件仍使用原生 Console。

### 约束条件
- AOT 编译已启用，需要确保 Spectre.Console 与 AOT 兼容
- 现有 `IConsoleOutput` 接口用于解耦输出实现
- 跨平台支持（Windows/Linux/macOS）

## Goals / Non-Goals

### Goals
- 统一使用 Spectre.Console 进行控制台输出
- 扩展 `IConsoleOutput` 接口以支持 Spectre.Console 的核心功能
- 创建统一的样式常量管理
- 保持 AOT 兼容性

### Non-Goals
- 实现完整的 Spectre.Console 功能包装（仅包装核心需要的功能）
- 更改现有输出文本内容（仅改变显示样式）
- 添加复杂交互组件（如进度条、实时更新等，除非明确需要）

## Decisions

### 决策 1: 扩展 IConsoleOutput 接口而非直接使用 AnsiConsole

**选择**: 扩展现有 `IConsoleOutput` 接口，添加 Spectre.Console 特定方法

**原因**:
- 保持接口抽象，便于单元测试（可以 Mock）
- 便于未来切换或扩展输出实现
- 符合依赖倒置原则

**替代方案**:
- 直接使用 `AnsiConsole` 静态方法：简单但难以测试和扩展
- 创建全新接口：会破坏现有代码结构

### 决策 2: 接口方法设计

**添加的新方法**:
```csharp
void Markup(string markup);              // Spectre.Console 标记语法输出
void MarkupLine(string markup);          // 标记语法输出 + 换行
void Success(string message);            // 成功消息（绿色）
void Error(string message);              // 错误消息（红色）
void Warning(string message);            // 警告消息（黄色）
void Info(string message);               // 信息消息（蓝色）
void WriteTable(Table table);            // 表格输出
void WriteRule(string text);             // 分隔线
bool Confirm(string prompt, bool defaultValue); // 确认提示
```

**保留的原方法**:
```csharp
void Write(string message);
void WriteLine(string message);
void WriteLine();                 // 无参数，输出空行
void WriteError(string message);  // 改为调用 Error()
string? ReadLine();
```

### 决策 3: 统一样式管理

**创建 `ConsoleStyles` 静态类**:
```csharp
public static class ConsoleStyles
{
    public const string Success = "green";
    public const string Error = "red";
    public const string Warning = "yellow";
    public const string Info = "blue";
    public const string Dim = "dim";
    public const string Bold = "bold";

    // 预定义的标记模板
    public static string SuccessMessage(string text) => $"[{Success}]{text}[/]";
    public static string ErrorMessage(string text) => $"[{Error}]{text}[/]";
    public static string WarningMessage(string text) => $"[{Warning}]{text}[/]";
    public static string InfoMessage(string text) => $"[{Info}]{text}[/]";
}
```

### 决策 4: 实现类命名

**选择**: `SpectreConsoleOutput` 替代现有的 `ConsoleOutput`

**原因**:
- 清晰表明使用 Spectre.Console
- 保留现有 `ConsoleOutput` 作为简单实现（可选，用于调试）

### 决策 5: y/N 确认提示改造

**现状**: 代码中存在 5 处使用 `Console.ReadLine()` + 手动字符串判断的 y/N 确认模式

**改造方案**: 使用 `IConsoleOutput.Confirm(prompt, defaultValue)` 替代

**改造示例**:
```csharp
// 改造前
Console.Write("是否覆盖? [y/N]: ");
var response = Console.ReadLine()?.Trim().ToLower();
var result = response == "y" || response == "yes";

// 改造后
var result = _console.Confirm("是否覆盖?", false);
```

**优势**:
- Spectre.Console 的 Confirm 方法支持单键输入（无需回车）
- 自动显示 `[y/N]` 或 `[Y/n]` 样式（根据 defaultValue）
- 统一的确认交互体验
- 自动处理大小写不敏感

**受影响的代码位置**:
1. `InitService.cs:192` - `PromptOverwrite()` 方法
2. `InstallService.cs:240` - Windows 文件覆盖确认
3. `InstallService.cs:356` - Windows 删除配置文件确认
4. `InstallService.cs:518` - Unix 符号链接覆盖确认
5. `InstallService.cs:618` - Unix 删除配置文件确认

## Architecture

```
Services/
├── IConsoleOutput.cs          (扩展接口)
├── SpectreConsoleOutput.cs    (新增实现)
├── ConsoleStyles.cs           (新增样式常量)
└── [其他 Services...]         (通过 IConsoleOutput 输出)
```

## Migration Plan

### 步骤
1. 创建 `ConsoleStyles.cs` 样式常量类
2. 扩展 `IConsoleOutput.cs` 接口
3. 创建 `SpectreConsoleOutput.cs` 实现类
4. 更新依赖注入/实例创建代码
5. 逐个 Service 文件替换 `Console.Write*` 调用
6. 验证所有命令输出效果

### 回滚策略
- 保留 Git 提交历史，可随时回退
- `ConsoleOutput` 简单实现可作为备份

## Risks / Trade-offs

### 风险 1: Spectre.Console AOT 兼容性
**风险**: 某些 Spectre.Console 功能可能与 AOT 不兼容
**缓解**: Spectre.Console 0.54.0 已声明支持 AOT，仅使用基础功能

### 风险 2: 终端颜色支持差异
**风险**: 不同终端对 ANSI 颜色支持程度不同
**缓解**: Spectre.Console 自动检测终端能力并降级

### 权衡
- 更好的视觉效果 vs 简单的纯文本输出
- 接口复杂度增加 vs 代码可测试性和扩展性提升

## Open Questions

1. 是否需要在 `IConsoleOutput` 中添加更高级的 Spectre.Console 组件（如 Tree、Grid、Progress 等）？
   - 建议：按需添加，当前仅包装核心输出方法

2. 是否需要支持禁用颜色输出的模式？
   - 建议：通过环境变量 `NO_COLOR` 或 Spectre.Console 的自动检测处理
