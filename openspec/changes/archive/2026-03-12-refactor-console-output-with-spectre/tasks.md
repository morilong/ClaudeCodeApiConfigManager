# Implementation Tasks

## 1. 核心接口和样式基础设施
- [x] 1.1 创建 `ConsoleStyles.cs` 常量类，定义统一的颜色和样式标记
- [x] 1.2 扩展 `IConsoleOutput.cs` 接口，添加 Spectre.Console 支持方法（含 Confirm 方法）
- [x] 1.3 创建 `SpectreConsoleOutput.cs` 实现类（含 Confirm 实现）
- [x] 1.4 更新实例创建代码，使用新的 SpectreConsoleOutput

## 2. 改造 Program.cs 入口点
- [x] 2.1 替换 `Console.Error.WriteLine` 错误输出为 `IConsoleOutput.Error()`（2处）

## 3. 改造安装相关服务
- [x] 3.1 改造 `InstallService.cs` - 替换所有 `Console.Write*` 和 `Console.Error.WriteLine` 调用（约20+处标准输出 + 10+处错误输出）
- [x] 3.2 改造 `InstallService.cs` - 替换 4 处 `Console.ReadLine()` y/N 确认为 `Confirm()` 方法
- [x] 3.3 改造 `InstallPromptService.cs` - **跳过**，该服务使用 Spectre.Console 高级组件（SelectionPrompt、Table、Rule），继续直接使用 AnsiConsole

## 4. 改造初始化服务
- [x] 4.1 改造 `InitService.cs` - 替换所有 `Console.Write*` 和 `Console.Error.WriteLine` 调用（约10+处标准输出 + 3处错误输出）
- [x] 4.2 改造 `InitService.cs` - 替换 1 处 `Console.ReadLine()` y/N 确认为 `Confirm()` 方法

## 5. 改造环境管理服务
- [x] 5.1 改造 `WindowsEnvironmentManager.cs` - 替换 `Console.WriteLine`
- [x] 5.2 改造 `UnixEnvironmentManager.cs` - 替换 `Console.WriteLine`

## 6. 改造其他服务
- [x] 6.1 改造 `VersionHelper.cs` - 替换 `Console.WriteLine`

## 7. 验证和测试
- [ ] 7.1 运行 `ccm list` 验证配置列表显示效果
- [ ] 7.2 运行 `ccm add/use/remove` 验证命令输出效果
- [ ] 7.3 运行 `ccm install` 验证安装流程输出效果（含 y/N 确认）
- [ ] 7.4 运行 `ccm uninstall` 验证卸载流程输出效果（含 y/N 确认）
- [ ] 7.5 运行 `ccm v` 验证版本号显示效果
- [ ] 7.6 验证错误输出（如触发异常场景）显示红色样式
- [ ] 7.7 验证 y/N 确认提示支持单键输入（无需回车）
