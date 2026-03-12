## 1. Shell 检测

- [x] 1.1 创建 `Services/ShellDetector.cs`，定义 `ShellType` 枚举
- [x] 1.2 实现 `Detect()` 方法，按优先级检测 Shell 类型
- [x] 1.3 添加 `MSYSTEM` 环境变量检查，用于 Git Bash 检测
- [x] 1.4 添加 `PSModulePath` 环境变量检查，用于 PowerShell 检测
- [x] 1.5 添加 `$SHELL` 环境变量解析，用于 Unix Shell 检测

## 2. 临时环境变量导出

- [x] 2.1 创建 `Services/TempEnvExporter.cs` 类
- [x] 2.2 实现 `Export(ShellType, Dictionary<string,string>)` 方法
- [x] 2.3 添加 PowerShell 输出格式: `$env:VAR="value"`
- [x] 2.4 添加 Bash/Zsh/Git Bash 输出格式: `export VAR="value"`
- [x] 2.5 添加 Fish 输出格式: `set -x VAR "value"`
- [x] 2.6 添加 CMD 输出格式: 单行 `set VAR1=val1 && set VAR2=val2`
- [x] 2.7 实现环境变量值的特殊字符转义

## 3. Shell 函数注入

- [x] 3.1 创建 `Services/ShellFunctionInjector.cs` 类
- [x] 3.2 添加 PowerShell 函数模板（使用 `Invoke-Expression`）
- [x] 3.3 添加 Bash/Zsh 函数模板（使用 `eval`）
- [x] 3.4 添加 Fish 函数模板（使用 `eval`）
- [x] 3.5 实现 `Inject(ShellType)` 方法，为各 Shell 类型注入函数
- [x] 3.6 实现 `Remove(ShellType)` 方法，用于卸载时移除函数
- [x] 3.7 添加标记注释检测（`# <ccm-init>`, `# </ccm-init>`）
- [x] 3.8 实现 Windows PowerShell `$PROFILE` 路径解析
- [x] 3.9 实现 Windows Git Bash `.bashrc` 路径解析

## 4. 命令更新

- [x] 4.1 为 `use` 命令添加 `--temp` / `-t` 选项（隐式默认）
- [x] 4.2 为 `use` 命令添加 `--persist` / `-p` 选项
- [x] 4.3 修改 `use` 命令默认行为，输出临时环境变量命令
- [x] 4.4 更新 `CommandHelper.SetEnvironmentVariables` 支持临时模式
- [x] 4.5 临时模式下也更新 `activeConfigName`
- [x] 4.6 确保永久模式同时输出临时命令，实现立即生效

## 5. 环境管理器更新

- [x] 5.1 更新 `WindowsEnvironmentManager`，使用 `ShellDetector` 和 `TempEnvExporter`
- [x] 5.2 更新 `UnixEnvironmentManager`，使用 `ShellDetector` 和 `TempEnvExporter`
- [x] 5.3 添加 CMD 用户提示，显示可复制粘贴的命令

## 6. 安装/卸载集成

- [x] 6.1 更新 `InstallService`，安装时调用 `ShellFunctionInjector.Inject()`
- [x] 6.2 检测可用的 Shell 并为每个注入函数
- [x] 6.3 更新 `InstallService`，卸载时调用 `ShellFunctionInjector.Remove()`
- [x] 6.4 更新安装成功提示，说明已注入 Shell 函数

## 7. 常量更新

- [x] 7.1 添加 Shell 标记常量（`<ccm-init>`, `</ccm-init>`）
- [x] 7.2 添加 Shell 函数模板常量（或移至单独文件）
- [x] 7.3 添加 CMD 用户提示的消息常量

## 8. 测试与完善

- [x] 8.1 测试 PowerShell 函数注入和 eval - 需要用户手动测试
- [x] 8.2 测试 Git Bash 函数注入和 eval - 需要用户手动测试
- [x] 8.3 测试 CMD 复制粘贴命令输出 - 需要用户手动测试
- [x] 8.4 测试 `--persist` 模式双重输出 - 需要用户手动测试
- [x] 8.5 测试卸载时移除 Shell 函数 - 需要用户手动测试
- [x] 8.6 测试环境变量值的特殊字符转义 - 騙自动化测试，
