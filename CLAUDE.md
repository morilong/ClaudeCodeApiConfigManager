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

# 切换到指定配置
ccm use <名称>

# 查看当前配置
ccm current
ccm c

# 删除配置
ccm remove <名称>
ccm del <名称>

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
│   ├── Platform.cs                # 平台检测和路径管理
│   ├── WindowsEnvironmentManager.cs # Windows 环境变量管理器
│   └── UnixEnvironmentManager.cs   # Unix 环境变量管理器
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

- **ConfigRepository**: 配置文件持久化
  - 读取和写入 JSON 配置文件
  - 使用 Source Generator 优化的 JSON 序列化

- **CommandHelper**: 智能参数解析
  - 自动识别 HTTP/HTTPS URL
  - 识别 sk- 前缀的 API Token
  - 解析 KEY=VALUE 格式的自定义参数

- **Platform**: 跨平台支持
  - 检测运行平台（Windows/Unix）
  - 获取平台特定的配置目录路径
  - Windows: 可执行文件目录
  - Unix: `~/.config/ClaudeCodeApiConfigManager/`

- **WindowsEnvironmentManager**: Windows 环境变量管理
  - 使用 `Environment.SetEnvironmentVariable` 设置用户级环境变量

- **UnixEnvironmentManager**: Unix 环境变量管理
  - 检测 Shell 类型（bash/zsh/fish）
  - 在 `~/.ccm/` 目录生成 Shell 脚本
  - 自动在 Shell 配置文件中添加初始化代码

- **VersionHelper**: 版本管理
  - 从程序集属性读取版本号

### Commands 层

- **CommandBuilder**: CLI 命令定义
  - 使用 System.CommandLine 构建命令树
  - 定义 add、list、use、current、remove、v 命令

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
