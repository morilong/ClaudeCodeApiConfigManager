
# 一个跨平台的 Claude Code API 配置管理工具，类似 nvm 和 nrm。

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 简介

**ccm** (Claude Code API Config Manager) 是一个命令行工具，帮助开发者快速管理和切换多个 Claude Code API 配置。如果你经常在不同的 API 提供商（如 Anthropic、DeepSeek、智谱AI 等）之间切换，或者需要为不同项目使用不同的 API 配置，这个工具将大大提高你的效率。

## 功能特性

- 多配置管理 - 添加、列出、删除多个 API 配置
- 快速切换 - 一条命令在不同配置间切换
- 环境变量自动设置 - 自动配置平台特定的环境变量
- 跨平台支持 - Windows、Linux、macOS
- 智能参数识别 - 自动识别 Token、Base URL 和模型参数
- AOT 编译 - 发布为原生可执行文件，启动快速

## 安装

### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/morilong/ClaudeCodeApiConfigManager.git
cd ClaudeCodeApiConfigManager
或
git clone https://gitee.com/morilong/claude-code-api-config-manager.git
cd claude-code-api-config-manager

# 发布为单文件可执行程序
dotnet publish -c Release
```

### 直接使用预编译版本

下载对应平台的可执行文件，将其添加到系统 PATH 中。

## 使用方法

### 添加配置

```bash
# 智谱AI
ccm add zhipu xxx.xxx https://open.bigmodel.cn/api/anthropic glm-4.7

#DeepSeek
ccm add ds sk-xxx https://api.deepseek.com/anthropic deepseek-chat
```

工具会智能识别参数：
- 以 `sk-` 开头的识别为 API Token
- 以 `http://` 或 `https://` 开头的识别为 Base URL
- `KEY=VALUE` 格式的识别为自定义参数
- 剩余参数作为模型名称

### 列出所有配置

```bash
ccm list
# 或
ccm ls
```

输出示例：
```
* zhipu - glm-4.7
  ds - deepseek-chat
```

`*` 标记表示当前活动的配置。

### 切换配置

```bash
ccm use zhipu
```

切换后会自动设置环境变量。在 Unix 平台上，可能需要重新加载 Shell 配置：

```bash
source ~/.bashrc  # bash
source ~/.zshrc   # zsh
source ~/.config/fish/config.fish  # fish
```

### 查看当前配置

```bash
ccm current
# 或
ccm c
```

输出示例：
```
当前配置：
  名称: zhipu
  模型: glm-4.7
  BaseURL: https://open.bigmodel.cn/api/anthropic
```

### 删除配置

```bash
ccm remove test1
# 或
ccm del test1
```

### 查看版本

```bash
ccm v
```

## 命令参考

| 命令 | 描述 | 使用示例 |
|-----|------|---------|
| `add` | 添加新的 API 配置 | `ccm add zhipu xxx.xxx https://open.bigmodel.cn/api/anthropic glm-4.7` |
| `list` / `ls` | 列出所有已保存的配置 | `ccm ls` |
| `use` | 切换到指定配置（设置环境变量） | `ccm use zhipu` |
| `current` / `c` | 查看当前使用的配置详情 | `ccm c` |
| `remove` / `del` | 删除指定配置 | `ccm del test1` |
| `v` | 查看版本信息 | `ccm v` |

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
| Windows | 可执行文件目录/`settings.json` |
| Linux/macOS | `~/.config/ClaudeCodeApiConfigManager/settings.json` |

配置文件格式：

```json
{
  "configs": [
    {
      "name": "zhipu",
      "authToken": "xxx.xxx",
      "baseUrl": "https://open.bigmodel.cn/api/anthropic",
      "model": "glm-4.7",
      "customParams": {}
    },
    {
      "name": "ds",
      "authToken": "sk-xxx",
      "baseUrl": "https://api.deepseek.com/anthropic",
      "model": "deepseek-chat",
      "customParams": {}
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
- **System.CommandLine 2.0.2** - CLI 命令解析
- **System.Text.Json** - JSON 序列化（Source Generation）
- **AOT 编译** - 提前编译为本机代码

## 开发

```bash
# 构建项目
dotnet build

# 运行项目
dotnet run -- add test sk-xxx https://api.example.com model-name

# 发布为 AOT 可执行文件
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
│   ├── Platform.cs                # 平台检测
│   ├── WindowsEnvironmentManager.cs # Windows 环境变量管理
│   └── UnixEnvironmentManager.cs   # Unix 环境变量管理
└── ClaudeCodeApiConfigManager.csproj
```

## 常见问题

### Q: Windows 上切换配置后环境变量没有生效？

A: Windows 使用用户级环境变量，设置后需要重启程序或打开新的终端窗口。

### Q: Linux/macOS 上切换配置后环境变量没有生效？

A: 需要重新加载 Shell 配置文件，例如：`source ~/.zshrc` 或 `source ~/.bashrc`。

### Q: 如何备份我的配置？

A: 直接复制配置文件即可。Windows 上在可执行文件目录，Unix 上在 `~/.config/ClaudeCodeApiConfigManager/`。

## License

MIT License
