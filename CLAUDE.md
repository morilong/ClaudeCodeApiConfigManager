<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

**ClaudeCodeApiConfigManager (ccm)** 是一个跨平台的 .NET 10 控制台应用程序，用于管理 Claude Code API 配置。类似于 `nvm`（Node 版本管理器）和 `nrm`（npm 源管理器），该工具提供了简洁的 CLI 界面来管理多个 API 配置。

## 核心功能

- **多配置管理**: 添加、列出、删除多个 API 配置
- **快速切换**: 在不同配置间快速切换
- **环境变量设置**: 自动设置平台特定的环境变量
- **跨平台支持**: Windows、Linux、macOS
- **智能参数识别**: 自动识别 Token、Base URL 和模型参数
- **一键安装**: 支持交互式安装和静默安装 (`install -y`)
- **配置模板**: 预设常见 API 提供商配置（zhipu、ds、minimax、kimi、qwen3、qwen3-coding）

## 构建和运行

```bash
# 构建项目
dotnet build

# 运行项目
dotnet run

# 发布为 AOT 原生可执行文件
dotnet publish -c Release
```

## CLI 命令

```bash
# 添加新配置
ccm add <名称> <TOKEN> <BASE_URL> <MODEL> [自定义参数...]

# 列出所有配置
ccm list
ccm ls

# 修改指定配置的 API Token
ccm setToken <名称> <TOKEN>
ccm st <名称> <TOKEN>

# 切换到指定配置
ccm use <名称>

# 查看当前配置
ccm current
ccm c

# 删除配置
ccm remove <名称>
ccm rm <名称>
ccm del <名称>

# 卸载 ccm
ccm uninstall

# 查看版本
ccm v
```

## 项目配置

- **目标框架**: .NET 10.0
- **AOT 编译**: 已启用 (`PublishAot=true`)
- **全局化**: 固定 (`InvariantGlobalization=true`)
- **可空引用类型**: 已启用 (`Nullable=enable`)
- **隐式 using**: 已启用 (`ImplicitUsings=enable`)

## 依赖包

- **System.CommandLine** (v2.0.2) - CLI 命令解析
- **Spectre.Console** (v0.54.0) - 控制台美化和交互
- **System.Text.Json** - JSON 序列化（使用 Source Generation）

## 代码结构

```
ClaudeCodeApiConfigManager/
├── Program.cs                     # 应用入口点
├── Commands/
│   └── CommandBuilder.cs          # CLI 命令构建器
├── Models/
│   ├── ConfigModels.cs            # API 配置数据模型
│   └── SettingsContext.cs        # JSON 序列化上下文（Source Generator）
├── Services/
│   ├── ConfigRepository.cs        # 配置文件存储服务
│   ├── ConfigService.cs           # 配置业务逻辑服务
│   ├── CommandHelper.cs           # 命令参数解析辅助功能
│   ├── Constants.cs               # 全局常量定义
│   ├── VersionHelper.cs           # 版本信息管理
│   ├── IConsoleOutput.cs          # 控制台输出接口
│   ├── ConsoleStyles.cs           # 控制台样式常量
│   ├── Platform.cs                # 平台检测和路径管理
│   ├── WindowsEnvironmentManager.cs # Windows 环境变量管理器
│   ├── UnixEnvironmentManager.cs   # Unix 环境变量管理器
│   ├── InstallService.cs          # 安装/卸载服务
│   ├── InitService.cs             # 初始化向导服务
│   └── InstallPromptService.cs    # 安装提示服务
└── ClaudeCodeApiConfigManager.csproj
```

## 核心类说明

### Models 层

- **ApiConfig**: 单个 API 配置模型
  - `Name`: 配置名称
  - `AuthToken`: API 认证令牌
  - `BaseUrl`: API 基础 URL
  - `Model`: 模型名称
  - `CustomParams`: 自定义参数字典

- **SettingsConfig**: 配置集合模型
  - `Configs`: 所有 API 配置列表
  - `ActiveConfigName`: 当前活动配置名称

### Services 层

- **ConfigService**: 配置管理的核心业务逻辑
  - 配置的增删改查操作
  - 活动配置切换
  - 配置验证和冲突处理
  - Token 单独修改功能

- **ConfigRepository**: 配置文件持久化
  - 读取和写入 JSON 配置文件
  - 使用 Source Generator 优化的 JSON 序列化

- **CommandHelper**: 智能参数解析
  - 自动识别 HTTP/HTTPS URL
  - 识别 sk- 前缀的 API Token
  - 解析 KEY=VALUE 格式的自定义参数
  - 环境变量构建

- **Platform**: 跨平台支持
  - 检测运行平台（Windows/Unix）
  - 获取平台特定的配置目录路径
  - Windows: 可执行文件目录
  - Unix: `~/.config/ClaudeCodeApiConfigManager/`
  - PATH 环境变量检测

- **WindowsEnvironmentManager**: Windows 环境变量管理
  - 使用 `Environment.SetEnvironmentVariable` 设置用户级环境变量

- **UnixEnvironmentManager**: Unix 环境变量管理
  - 检测 Shell 类型（bash/zsh/fish）
  - 在 `~/.ccm/` 目录生成 Shell 脚本
  - 自动在 Shell 配置文件中添加初始化代码

- **InstallService**: 安装和卸载服务
  - 跨平台安装逻辑（Windows 复制文件/Unix 符号链接）
  - PATH 环境变量管理
  - 卸载时清理文件和环境变量
  - 检测目录是否适合安装到 PATH

- **InitService**: 初始化向导服务
  - 首次运行时的配置文件创建
  - 交互式安装向导
  - 默认配置模板生成

- **InstallPromptService**: 安装提示服务
  - 安装目录选择
  - 安装计划确认
  - 自定义路径输入

- **ConsoleStyles**: 控制台样式常量
  - 统一管理 Spectre.Console 的颜色和样式标记

- **VersionHelper**: 版本管理
  - 从程序集属性读取版本号
  - 版本选项创建和参数检测

### Commands 层

- **CommandBuilder**: CLI 命令定义
  - 使用 System.CommandLine 构建命令树
  - 定义 add、list、use、current、remove、setToken、uninstall、v 命令

## 配置文件

配置文件位置：
- **Windows**: 可执行文件目录下的 `settings.json`
- **Linux/macOS**: `~/.config/ClaudeCodeApiConfigManager/settings.json`

配置文件格式：
```json
{
  "configs": [
    {
      "name": "配置名称",
      "authToken": "sk-xxx...",
      "baseUrl": "https://api.example.com",
      "model": "模型名称",
      "customParams": {
        "KEY": "VALUE"
      }
    }
  ],
  "activeConfigName": "当前活动配置名称"
}
```

**默认配置模板**（首次安装时自动创建）：
- `zhipu` - 智谱AI (glm-4.7)
- `ds` - DeepSeek (deepseek-chat)
- `minimax` - MiniMax (MiniMax-M2.1)
- `kimi` - Moonshot (kimi-k2.5)
- `qwen3` - 通义千问 (qwen3-coder-plus)
- `qwen3-coding` - 通义千问 Coding Plan (qwen3-coder-plus)

## 环境变量

工具设置以下环境变量：
- `ANTHROPIC_AUTH_TOKEN`: API 认证令牌
- `ANTHROPIC_BASE_URL`: API 基础 URL
- `ANTHROPIC_MODEL`: 模型名称
- 自定义参数作为额外的环境变量

## 开发注意事项

### AOT 编译限制

该项目配置为 AOT 发布，代码将在编译时完全编译为本机代码：
- 不支持运行时反射（大部分）
- 不支持动态加载程序集
- 所有代码必须在编译时可确定

### JSON 序列化

使用 `JsonSerializerContext` 和 Source Generation 来优化性能并兼容 AOT：
- 在 `SettingsContext.cs` 中定义序列化上下文
- 使用 `[JsonSerializable]` 特性标记可序列化类型

### 平台特定代码

- 使用条件编译（`#if WINDOWS`）处理平台差异
- `Platform` 类统一处理平台检测和路径获取

### Shell 检测（Unix 平台）

Unix 环境管理器会：
1. 通过 `$SHELL` 环境变量检测用户 Shell
2. 支持 bash、zsh、fish 三种主流 Shell
3. 自动在 Shell 配置文件中添加 source 语句：
   - Bash: `~/.bashrc` 或 `~/.bash_profile`
   - Zsh: `~/.zshrc`
   - Fish: `~/.config/fish/config.fish`

## 扩展开发

添加新命令时：
1. 在 `CommandBuilder.cs` 中定义命令
2. 在 `ConfigService.cs` 或新的 Service 类中实现业务逻辑
3. 保持 AOT 兼容性，避免使用反射
4. 使用 `IConsoleOutput` 接口进行输出，便于测试

### Spectre.Console 使用

项目使用 Spectre.Console 进行控制台美化和交互：
- 使用 `AnsiConsole` 进行标记语法输出
- 使用 `Table` 显示表格数据
- 使用 `SelectionPrompt` 创建交互式选择
- 使用 `Confirm` 显示确认提示

参考 `ConsoleStyles.cs` 和 `IConsoleOutput.cs` 了解输出接口规范。

### install 命令

- 无参数：交互式安装向导
- `install -y`：静默安装，使用默认选项无需确认
