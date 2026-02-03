
# 一个跨平台的 Claude Code API 配置管理工具，类似 nvm 和 nrm。

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 简介

**ccm** (Claude Code API Config Manager) 是一个命令行工具，帮助开发者快速管理和切换多个 Claude Code API 配置。如果你经常在不同的 API 提供商（如 DeepSeek、智谱AI、MiniMax、Kimi、通义千问 等）之间切换，或者需要为不同项目使用不同的 API 配置，这个工具将大大提高你的效率。

## 功能特性

- **多配置管理** - 添加、列出、删除多个 API 配置
- **快速切换** - 一条命令在不同配置间切换
- **环境变量自动设置** - 自动配置平台特定的环境变量
- **智能参数识别** - 自动识别 Token、Base URL 和模型参数
- **跨平台支持** - Windows、Linux、macOS
- **AOT 编译** - 发布为原生可执行文件，启动快速
- **一键安装** - 支持交互式安装和静默安装 (`install -y`)


## 安装

### 一键安装/更新

#### 国内用户（Gitee 源）

**Linux / macOS**
```bash
curl -fsSL https://gitee.com/morilong/claude-code-api-config-manager/raw/master/scripts/install.sh | bash
```

**Windows (PowerShell)**
```powershell
irm https://gitee.com/morilong/claude-code-api-config-manager/raw/master/scripts/install.ps1 | iex
```

**Windows (CMD)**
```cmd
curl -fsSL https://gitee.com/morilong/claude-code-api-config-manager/raw/master/scripts/install.cmd -o install.cmd && install.cmd && del install.cmd
```

#### 国外用户（GitHub 源）

**Linux / macOS**
```bash
curl -fsSL https://raw.githubusercontent.com/morilong/ClaudeCodeApiConfigManager/master/scripts/install-github.sh | bash
```

**Windows (PowerShell)**
```powershell
irm https://raw.githubusercontent.com/morilong/ClaudeCodeApiConfigManager/master/scripts/install-github.ps1 | iex
```

**Windows (CMD)**
```cmd
curl -fsSL https://raw.githubusercontent.com/morilong/ClaudeCodeApiConfigManager/master/scripts/install-github.cmd -o install-github.cmd && install-github.cmd && del install-github.cmd
```

### 手动安装

从 [Releases](https://github.com/morilong/ClaudeCodeApiConfigManager/releases) 页面下载对应平台的压缩包：

| 平台 | 架构 | 下载链接 |
|------|------|----------|
| Windows | x64 | [ccm-win-x64.zip](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-win-x64.zip) |
| Windows | x86 | [ccm-win-x86.zip](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-win-x86.zip) |
| Windows | ARM64 | [ccm-win-arm64.zip](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-win-arm64.zip) |
| Linux | x64 | [ccm-linux-x64.tar.gz](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-linux-x64.tar.gz) |
| Linux | ARM64 | [ccm-linux-arm64.tar.gz](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-linux-arm64.tar.gz) |
| macOS | x64 | [ccm-osx-x64.tar.gz](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-osx-x64.tar.gz) |
| macOS | ARM64 | [ccm-osx-arm64.tar.gz](https://gitee.com/morilong/claude-code-api-config-manager/releases/download/latest/ccm-osx-arm64.tar.gz) |

**解压后执行安装**
- Windows：双击 `ccm.exe`
- Linux/macOS：`./ccm`

### 从源码构建

```bash
# 克隆仓库
git clone https://gitee.com/morilong/claude-code-api-config-manager.git
cd claude-code-api-config-manager

或

git clone https://github.com/morilong/ClaudeCodeApiConfigManager.git
cd ClaudeCodeApiConfigManager

# 发布为可执行程序
dotnet publish -c Release
```

---

## 使用方法

### 列出所有配置
```bash
ccm ls
```

### 修改指定配置的 API Token
默认自带了 DeepSeek、智谱AI、MiniMax、Kimi、通义千问 的配置模板，只需修改 API Token 即可使用：
```
ccm st <name> <token>

# 示例
ccm st ds sk-xxx
```

### 切换配置
```bash
ccm use ds
```

### 添加新配置

```bash
# 智谱AI
ccm add zhipu xxx https://open.bigmodel.cn/api/anthropic glm-4.7

# DeepSeek
ccm add ds sk-xxx https://api.deepseek.com/anthropic deepseek-chat

# MiniMax
ccm add minimax xxx https://api.minimaxi.com/anthropic MiniMax-M2.1

# Kimi
ccm add kimi sk-xxx https://api.moonshot.cn/anthropic kimi-k2.5

# 通义千问Coder
ccm add qwen3 sk-xxx https://dashscope.aliyuncs.com/apps/anthropic qwen3-coder-plus

# 通义千问Coder Coding Plan
ccm add qwen3-coding sk-xxx https://coding.dashscope.aliyuncs.com/apps/anthropic qwen3-coder-plus
```

配置自定义参数：
```
ccm add ds sk-xxx https://api.deepseek.com/anthropic deepseek-chat API_TIMEOUT_MS=600000
```
- 格式：KEY=VALUE（如：API_TIMEOUT_MS=600000）

### 查看当前配置
```bash
ccm c
```

### 删除配置
```bash
ccm rm test1
```

### 查看版本
```bash
ccm v
```

## 命令参考

| 命令 | 描述 | 使用示例 |
|-----|------|---------|
| `list` / `ls` | 列出所有已保存的配置 | `ccm ls` |
| `setToken` / `st` | 修改指定配置的 API Token | `ccm st zhipu xxx` |
| `add` | 添加新的 API 配置 | `ccm add zhipu xxx https://open.bigmodel.cn/api/anthropic glm-4.7` |
| `use` | 切换到指定配置（设置环境变量） | `ccm use zhipu` |
| `current` / `c` | 查看当前使用的配置详情 | `ccm c` |
| `remove` / `rm` / `del` | 删除指定配置 | `ccm rm test1` |
| `uninstall` | 卸载 ccm 软件本身 | `ccm uninstall` |
| `v` | 查看版本信息 | `ccm v` |

---

## 环境变量

工具会设置以下环境变量：

| 环境变量 | 说明 |
|---------|------|
| `ANTHROPIC_AUTH_TOKEN` | API 认证令牌 |
| `ANTHROPIC_BASE_URL` | API 基础 URL |
| `ANTHROPIC_MODEL` | 模型名称 |
| 自定义参数 | 通过 `KEY=VALUE` 添加的额外环境变量 |

## 配置文件

配置文件位置：

| 平台 | 位置 |
|-----|------|
| Windows | `可执行文件目录/settings.json` |
| Linux/macOS | `~/.config/ClaudeCodeApiConfigManager/settings.json` |

配置文件格式：

```json
{
  "configs": [
    {
      "name": "zhipu",
      "authToken": "your_zhipu_api_key",
      "baseUrl": "https://open.bigmodel.cn/api/anthropic",
      "model": "glm-4.7",
      "customParams": {}
    },
    {
      "name": "ds",
      "authToken": "sk-xxx",
      "baseUrl": "https://api.deepseek.com/anthropic",
      "model": "deepseek-chat",
      "customParams": {
        "API_TIMEOUT_MS": 600000
      }
    }
  ],
  "activeConfigName": "zhipu"
}
```

## 平台支持

### Windows

使用用户级环境变量，设置后需要重启程序或打开新的终端窗口才能生效。

### Linux/macOS

1. 自动检测用户的 Shell（bash/zsh/fish）
2. 在 `~/.ccm/` 目录生成环境变量脚本
3. 自动在 Shell 配置文件中添加初始化代码
4. 执行 `source` 命令重新加载配置

支持的 Shell：
- Bash: `~/.bashrc` 或 `~/.bash_profile`
- Zsh: `~/.zshrc`
- Fish: `~/.config/fish/config.fish`

## 技术栈

- **.NET 10.0** - 跨平台框架
- **System.CommandLine** (v2.0.2) - CLI 命令解析
- **Spectre.Console** (v0.54.0) - 控制台美化和交互
- **System.Text.Json** - JSON 序列化（Source Generation）
- **AOT 编译** - 提前编译为本机代码

## 开发

```bash
# 构建项目
dotnet build

# 运行项目
dotnet run -- add test sk-xxx https://api.example.com model-name

# 发布为可执行文件
dotnet publish -c Release
```

## 项目结构

```
ClaudeCodeApiConfigManager/
├── Program.cs                     # 应用入口点
├── Commands/
│   └── CommandBuilder.cs          # CLI 命令构建器
├── Models/
│   ├── ConfigModels.cs            # API 配置数据模型
│   └── SettingsContext.cs        # JSON 序列化上下文
├── Services/
│   ├── ConfigRepository.cs        # 配置文件存储服务
│   ├── ConfigService.cs           # 配置业务逻辑服务
│   ├── CommandHelper.cs           # 命令参数解析辅助
│   ├── Constants.cs               # 全局常量
│   ├── VersionHelper.cs           # 版本信息管理
│   ├── IConsoleOutput.cs          # 控制台输出接口
│   ├── ConsoleStyles.cs           # 控制台样式常量
│   ├── Platform.cs                # 平台检测和路径管理
│   ├── WindowsEnvironmentManager.cs # Windows 环境变量管理
│   ├── UnixEnvironmentManager.cs   # Unix 环境变量管理
│   ├── InstallService.cs          # 安装/卸载服务
│   ├── InitService.cs             # 初始化向导服务
│   └── InstallPromptService.cs    # 安装提示服务
└── ClaudeCodeApiConfigManager.csproj
```

## 常见问题

### Q: Windows 上切换配置后环境变量没有生效？
A: Windows 使用用户级环境变量，设置后需要重启程序或打开新的终端窗口。

### Q: Linux/macOS 上切换配置后环境变量没有生效？
A: 需要重新加载 Shell 配置文件，例如：`source ~/.bashrc` 或 `source ~/.zshrc`。

### Q: 如何备份我的配置？
A: 直接复制配置文件即可。
- Windows 在可执行文件目录
- Linux/macOS 在 `~/.config/ClaudeCodeApiConfigManager/`

### Q: 解决 Claude Code 报错：Unable to connect to Anthropic services
A: 编辑 `.claude.json` 文件：
- Windows：`C:\Users\%username%\.claude.json`
- Linux：`~/.claude.json`
>
在第一个 `{` 后面添加一行：
```
"hasCompletedOnboarding": true,
```

### Q: win7/win8报错：`无法启动此程序，因为计算机中丢失 api-ms-win-crt-*.dll。`
A: https://gitee.com/morilong/windows-sdk-ucrt-redistributable-dlls
