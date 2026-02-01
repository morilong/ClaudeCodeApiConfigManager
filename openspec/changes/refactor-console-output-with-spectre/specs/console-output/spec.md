# Console Output Specification

## ADDED Requirements

### Requirement: Spectre.Console 输出接口

系统 MUST 提供 `IConsoleOutput` 接口，支持 Spectre.Console 的核心输出功能。

#### Scenario: 基本文本输出

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 调用 `WriteLine("Hello")` 方法
- **THEN** 输出 "Hello" 并换行

#### Scenario: 空行输出

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 调用 `WriteLine()` 方法（无参数）
- **THEN** 输出一个空行

#### Scenario: 标记语法输出

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 调用 `Markup("[green]Success[/]")` 方法
- **THEN** 输出绿色的 "Success" 文本

#### Scenario: 预定义样式输出

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 调用 `Success("操作成功")` 方法
- **THEN** 输出绿色的 "操作成功" 文本
- **AND** 调用 `Error("操作失败")` 输出红色文本
- **AND** 调用 `Warning("警告信息")` 输出黄色文本
- **AND** 调用 `Info("提示信息")` 输出蓝色文本

#### Scenario: 表格输出

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 创建 Table 并调用 `WriteTable(table)` 方法
- **THEN** 输出格式化的表格

#### Scenario: 确认提示

- **GIVEN** IConsoleOutput 接口已注入
- **WHEN** 调用 `Confirm("是否继续?", true)` 方法
- **THEN** 显示确认提示并返回用户选择结果

#### Scenario: y/N 确认替换

- **GIVEN** 现有代码使用 `Console.ReadLine()` + 手动判断 `response == "y" || response == "yes"`
- **WHEN** 改造为调用 `IConsoleOutput.Confirm(prompt, defaultValue)`
- **THEN** 输出 Spectre.Console 风格的确认提示 `[y/N]` 或 `[Y/n]`
- **AND** 用户只需输入 y 或 n（不区分大小写），无需回车即可确认
- **AND** 返回布尔值而非字符串

---

### Requirement: 统一样式管理

系统 MUST 提供 `ConsoleStyles` 常量类，统一管理控制台输出样式。

#### Scenario: 样式常量定义

- **GIVEN** ConsoleStyles 类存在
- **WHEN** 访问 `ConsoleStyles.Success`
- **THEN** 返回 "green" 样式标记
- **AND** `ConsoleStyles.Error` 返回 "red"
- **AND** `ConsoleStyles.Warning` 返回 "yellow"
- **AND** `ConsoleStyles.Info` 返回 "blue"

#### Scenario: 样式模板方法

- **GIVEN** ConsoleStyles 类存在
- **WHEN** 调用 `ConsoleStyles.SuccessMessage("成功")`
- **THEN** 返回 "[green]成功[/]" 格式化字符串
