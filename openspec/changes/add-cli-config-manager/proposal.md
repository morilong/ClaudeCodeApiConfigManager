# Change: 添加 Claude Code API 配置管理 CLI 工具

## Why

用户在使用 Claude Code 时可能需要在不同的 API 配置之间切换（例如：官方 API、代理 API、不同模型的配置等）。目前没有便捷的工具来管理这些配置，用户需要手动修改系统环境变量，操作繁琐且容易出错。

参考 nvm（Node Version Manager）和 nrm（NPM Registry Manager）的设计理念，提供一个简单易用的跨平台 CLI 工具来管理 Claude Code 的 API 配置。

## What Changes

- 添加跨平台配置管理命令行工具，支持以下命令：
  - `add` - 添加新的 API 配置
  - `list` / `ls` - 列出所有已保存的配置
  - `use` - 切换到指定配置（修改用户级环境变量）
  - `current` / `c` - 查看当前使用的配置
  - `remove` / `del` - 删除指定配置
  - `test` - 手动测试当前配置的 API 连接

- 配置持久化到平台特定的用户配置目录：
  - Windows: `%APPDATA%\ClaudeCodeApiConfigManager\settings.json`
  - Linux/macOS: `~/.config/ClaudeCodeApiConfigManager/settings.json`

- 支持灵活的参数格式（TOKEN 和 URL 位置可自动识别）
- 支持自定义参数（如 API_TIMEOUT_MS、ANTHROPIC_SMALL_FAST_MODEL 等）
- 跨平台环境变量持久化：
  - Windows: 通过注册表 `HKEY_CURRENT_USER\Environment`
  - Linux/macOS: 通过生成 shell 初始化脚本（`~/.claude-config/env.sh`）
- 配置名称冲突时提示用户确认是否覆盖

## Impact

- **受影响的规范**: 新增 `cli-config-manager` 规范
- **受影响的代码**:
  - `Program.cs` - 需要重写为 CLI 命令解析器
  - 新增配置管理模块（ConfigManager.cs）
  - 新增跨平台环境变量管理接口和实现（IEnvironmentManager、WindowsEnvironmentManager、UnixEnvironmentManager）
  - 新增平台检测模块（PlatformDetector.cs）
  - 新增 API 测试模块（ApiTester.cs）
  - 新增配置数据模型（ConfigModels.cs）

## Breaking Changes

无 - 这是一个全新功能，现有代码仅为 "Hello World" 示例。

## Dependencies

- 需要使用 System.CommandLine 进行命令行参数解析（需要添加 NuGet 包）
- 需要使用 System.Text.Json 进行 JSON 配置文件读写
- 需要使用 Microsoft.Win32.Registry 访问 Windows 注册表以修改用户环境变量（Windows 平台专用）

## Platform Support

| 平台 | 环境变量持久化方式 | AOT 支持 |
|------|-------------------|----------|
| Windows | 注册表 HKEY_CURRENT_USER\Environment | 支持 |
| Linux | Shell 初始化脚本 (~/.claude-config/env.sh) | 支持 |
| macOS | Shell 初始化脚本 (~/.claude-config/env.sh) | 支持 |

## Additional Notes

- Linux/macOS 需要在 shell 配置文件中添加初始化代码（首次运行时自动添加）
- 支持 Bash、Zsh、Fish 三种 shell
- 使用条件编译（#if WINDOWS、#if UNIX）确保 AOT 兼容性
