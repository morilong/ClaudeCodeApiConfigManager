# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个 .NET 控制台应用程序（ClaudeCodeApiConfigManager），用于管理 Claude API 配置。

## 构建和运行

```bash
# 构建项目
dotnet build

# 运行项目
dotnet run

# 发布为 AOT（Ahead-of-Time）原生可执行文件
dotnet publish -c Release
```

## 项目配置

- **目标框架**: .NET 10.0
- **输出类型**: 控制台应用程序 (Exe)
- **AOT 编译**: 已启用 (`PublishAot=true`)
- **全局化**: 固定 (`InvariantGlobalization=true`)
- **可空引用类型**: 已启用 (`Nullable=enable`)
- **隐式 using**: 已启用 (`ImplicitUsings=enable`)

## 代码结构

- `Program.cs` - 主入口点
- `ClaudeCodeApiConfigManager.csproj` - 项目配置文件

## 开发注意事项

该项目配置为 AOT 发布，这意味着代码将在编译时完全编译为本机代码，而不是运行时通过 JIT 编译。这会带来以下限制：
- 不支持运行时反射（大部分）
- 不支持动态加载程序集
- 所有代码必须在编译时可确定
