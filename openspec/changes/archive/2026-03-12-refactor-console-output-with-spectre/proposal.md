# Change: 使用 Spectre.Console 改造控制台输出

## Why

当前项目使用原生 `Console.Write*` 方法进行控制台输出，存在以下问题：
1. 输出样式单调，缺乏颜色、格式化等增强功能
2. 错误信息、成功消息、警告等缺乏视觉区分
3. 项目已引入 Spectre.Console 包（0.54.0），但仅在 `InstallPromptService.cs` 中使用，其他 Service 文件仍使用原生 Console
4. 现有的 `IConsoleOutput` 接口设计未充分利用 Spectre.Console 的功能

通过统一使用 Spectre.Console，可以获得：
- 丰富的样式支持（颜色、粗体、斜体等）
- 更好的用户体验（表格、树形结构、进度条等）
- 统一的输出风格
- 跨平台兼容的终端输出

## What Changes

- **扩展 IConsoleOutput 接口**：添加支持 Spectre.Console 功能的方法（如 Markup、WriteTable 等）
- **创建 SpectreConsoleOutput 实现类**：实现扩展后的 IConsoleOutput 接口
- **创建 ConsoleStyles 常量类**：统一管理颜色和样式标记
- **改造现有输出代码**：将所有 `Console.Write*` 调用替换为通过 IConsoleOutput 的调用
- **移除直接使用 Console 和 AnsiConsole 的代码**：所有输出都通过 IConsoleOutput 接口

## Impact

- Affected specs: `console-output` (新增)
- Affected code:
  - `Services/IConsoleOutput.cs` - 扩展接口
  - `Services/ConsoleStyles.cs` - 新增样式常量类
  - `Services/SpectreConsoleOutput.cs` - 新增实现类
  - `Program.cs` - 替换 `Console.Error.WriteLine` 错误输出（2处）
  - `Services/InstallPromptService.cs` - 改造为使用 IConsoleOutput（替代直接 AnsiConsole 调用）
  - `Services/InstallService.cs` - 替换所有 `Console.Write*` 和 `Console.Error.WriteLine` 调用（约20+处标准输出 + 10+处错误输出）
  - `Services/InitService.cs` - 替换所有 `Console.Write*` 和 `Console.Error.WriteLine` 调用（约10+处标准输出 + 3处错误输出）
  - `Services/VersionHelper.cs` - 替换 `Console.WriteLine`
  - `Services/WindowsEnvironmentManager.cs` - 替换 `Console.WriteLine`
  - `Services/UnixEnvironmentManager.cs` - 替换 `Console.WriteLine`
