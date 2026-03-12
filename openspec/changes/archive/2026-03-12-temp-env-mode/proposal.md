## Why

当前 `ccm use xxx` 设置永久环境变量，用户需要重启终端或手动 source 才能生效，体验不够流畅。用户期望执行命令后环境变量能**立即在当前终端生效**，同时保留永久设置的选项。

此外，Windows 平台需要支持更多 Shell 类型（PowerShell、Git Bash、CMD），每种 Shell 的环境变量语法和 eval 机制不同，需要统一处理。

## What Changes

- **默认行为变更** `ccm use xxx` → 临时生效（仅当前终端），同时更新 `activeConfigName`
- **新增参数** `--persist` / `-p` → 永久生效（当前终端 + 新终端都生效）
- **新增 Shell 函数注入** → 安装时自动在 Shell 配置中注入函数，实现自动 eval
- **新增 Shell 检测** → 自动识别 PowerShell / Git Bash / CMD / Bash / Zsh / Fish
- **CMD 特殊处理** → 输出单条 `set && set && set` 格式命令，方便用户复制粘贴执行
- **永久模式优化** → 设置永久环境变量的同时也输出临时命令，确保当前终端立即生效

**BREAKING**: `ccm use xxx` 默认行为从"永久生效"改为"临时生效"

## Capabilities

### New Capabilities

- `temp-env-export`: 临时环境变量导出，根据 Shell 类型输出正确格式的环境变量设置命令
- `shell-detection`: Shell 类型自动检测，支持 PowerShell、CMD、Git Bash、Bash、Zsh、Fish
- `shell-function-injection`: Shell 函数注入，安装时自动配置各 Shell 的 wrapper 函数

### Modified Capabilities

- `env-management`: 环境变量管理行为变更，默认临时生效，`--persist` 永久生效

## Impact

**新增文件**:
- `Services/ShellDetector.cs` - Shell 类型检测
- `Services/TempEnvExporter.cs` - 临时环境变量输出器
- `Services/ShellFunctionInjector.cs` - Shell 函数注入器

**修改文件**:
- `Commands/CommandBuilder.cs` - use 命令添加 `--temp`/`--persist` 参数
- `Services/CommandHelper.cs` - 环境变量设置逻辑重构
- `Services/WindowsEnvironmentManager.cs` - 支持 PowerShell/Git Bash 函数注入
- `Services/UnixEnvironmentManager.cs` - 整合 shell 函数注入
- `Services/InstallService.cs` - 安装时调用注入器
- `Services/Constants.cs` - 新增常量
