# Technical Design

## Context

这是一个 .NET 10 跨平台控制台应用程序，需要实现类似 nvm/nrm 的配置管理功能。项目启用了 AOT 编译，这限制了运行时反射和动态加载的使用。

**关键约束：**
- 目标平台：Windows、Linux、macOS
- AOT 编译：所有代码必须在编译时可确定
- .NET 10：使用最新的 .NET 特性

## Goals / Non-Goals

**Goals：**
- 提供简洁直观的 CLI 界面管理 Claude Code API 配置
- 跨平台支持（Windows/Linux/macOS）
- 持久化环境变量到用户级别
- 支持灵活的参数格式
- 配置持久化到本地 JSON 文件

**Non-Goals：**
- 不支持配置文件加密
- 不支持配置同步到云端
- 不提供 GUI 界面

## Decisions

### 1. 命令行解析库选择

**决策：** 使用 `System.CommandLine`

**理由：**
- 官方库，维护良好
- 支持 AOT 编译
- 提供类型安全的参数绑定
- 自动生成帮助文档

**替代方案考虑：**
- `CommandLineParser`：第三方库，功能类似但非官方
- 手动解析：增加复杂度，容易出错

### 2. 配置文件存储位置

**决策：** 使用平台特定的用户配置目录

| 平台 | 配置目录路径 |
|------|-------------|
| Windows | `%APPDATA%\ClaudeCodeApiConfigManager\settings.json` |
| Linux | `~/.config/ClaudeCodeApiConfigManager/settings.json` |
| macOS | `~/.config/ClaudeCodeApiConfigManager/settings.json` |

**理由：**
- 遵循各平台的 XDG 配置目录规范
- 配置文件不会与可执行文件混在一起
- 便于用户备份和迁移配置

### 3. 跨平台环境变量持久化策略

**决策：** 平台特定的环境变量持久化机制

#### Windows 实现

```csharp
// 使用 Microsoft.Win32.Registry（条件编译）
#if WINDOWS
using Microsoft.Win32.Registry;

var userEnv = Registry.CurrentUser.OpenSubKey("Environment", true);
userEnv?.SetValue("ANTHROPIC_AUTH_TOKEN", token);
// 发送 WM_SETTINGCHANGE 消息
#endif
```

#### Linux/macOS 实现

**策略：** 类似 nvm 的设计，使用 shell 初始化脚本

**实现步骤：**

1. **检测用户的默认 shell**
   ```bash
   echo $SHELL  # /bin/bash, /bin/zsh, /bin/fish, etc.
   ```

2. **生成环境变量设置脚本**
   - 创建 `~/.ccm/env.sh`（Bash/Zsh）
   - 创建 `~/.ccm/env.fish`（Fish）

3. **在 shell 配置文件中添加初始化代码**
   - Bash: `~/.bashrc` 或 `~/.bash_profile`
   - Zsh: `~/.zshrc`
   - Fish: `~/.config/fish/config.fish`

4. **初始化代码示例**
   ```bash
   # 在 ~/.bashrc 中添加
   export CLAUDE_CONFIG_HOME="$HOME/.ccm"
   if [[ -f "$CLAUDE_CONFIG_HOME/env.sh" ]]; then
       source "$CLAUDE_CONFIG_HOME/env.sh"
   fi
   ```

5. **切换配置时更新环境变量脚本**
   ```bash
   # ~/.ccm/env.sh 内容
   export ANTHROPIC_AUTH_TOKEN="sk-ant-xxx"
   export ANTHROPIC_BASE_URL="https://api.anthropic.com"
   export ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"
   ```

**实现类设计：**

```csharp
public interface IEnvironmentManager
{
    void SetEnvironmentVariables(Dictionary<string, string> variables);
    Dictionary<string, string> GetEnvironmentVariables();
}

// Windows 实现
#if WINDOWS
public class WindowsEnvironmentManager : IEnvironmentManager { }
#endif

// Unix 实现
#if UNIX
public class UnixEnvironmentManager : IEnvironmentManager
{
    private readonly string _configDir;
    private readonly string _shellType;

    public void SetEnvironmentVariables(Dictionary<string, string> variables)
    {
        // 检测 shell 类型
        DetectShell();

        // 生成环境变量脚本
        WriteEnvScript(variables);

        // 检查是否已添加初始化代码
        if (!HasInitScript())
        {
            AddInitScript();
            Console.WriteLine("请运行 'source ~/.bashrc' 或重启终端使配置生效");
        }
    }
}
#endif
```

### 4. 参数识别策略

**决策：** 基于 URL 模式匹配自动识别 TOKEN 和 URL

```csharp
bool IsUrl(string value) => value.StartsWith("http://") || value.StartsWith("https://");

// 解析参数
string token = null;
string baseUrl = null;
foreach (var arg in args)
{
    if (IsUrl(arg)) baseUrl = arg;
    else token = arg;
}
```

**理由：**
- API Token 通常不含 http:// 或 https://
- Base URL 必须是 HTTP/HTTPS 地址
- 这种启发式方法对大多数场景有效

### 5. 项目结构

```
ClaudeCodeApiConfigManager/
├── Program.cs                         # 入口点，设置命令路由
├── Models/                            # 数据模型
│   └── ConfigModels.cs                # ApiConfig, SettingsConfig
├── Services/                          # 核心服务
│   ├── ConfigManager.cs               # 配置文件管理
│   ├── IEnvironmentManager.cs         # 环境变量管理接口
│   ├── WindowsEnvironmentManager.cs   # Windows 环境变量管理
│   ├── UnixEnvironmentManager.cs      # Unix 环境变量管理
│   ├── PlatformDetector.cs            # 平台检测
│   └── ApiTester.cs                   # API 测试
└── Commands/                          # CLI 命令
    ├── AddCommand.cs
    ├── ListCommand.cs
    ├── UseCommand.cs
    ├── CurrentCommand.cs
    ├── RemoveCommand.cs
    └── TestCommand.cs
```

## Data Models

```csharp
// API 配置项
public class ApiConfig
{
    public string Name { get; set; }
    public string AuthToken { get; set; }
    public string BaseUrl { get; set; }
    public string Model { get; set; }
    public Dictionary<string, string> CustomParams { get; set; }
}

// 配置文件结构
public class SettingsConfig
{
    public List<ApiConfig> Configs { get; set; } = new();
    public string? ActiveConfigName { get; set; }
}
```

## Platform Detection

```csharp
public static class Platform
{
    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsUnix => IsLinux || IsMacOS;
}

public static class ConfigDirectory
{
    public static string GetConfigDirectory()
    {
        if (Platform.IsWindows)
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClaudeCodeApiConfigManager"
            );
        else
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "ClaudeCodeApiConfigManager"
            );
    }
}
```

## Risks / Trade-offs

### 风险 1：环境变量生效时机

**风险描述：**
- Windows：修改注册表后需要重启应用才能生效
- Linux/macOS：需要重新加载 shell 配置或重启终端

**缓解措施：**
- Windows：发送 WM_SETTINGCHANGE 消息
- Linux/macOS：提示用户运行 `source ~/.bashrc` 或重启终端

### 风险 2：Shell 检测不准确

**风险描述：** 用户可能使用非标准 shell 或切换 shell。

**缓解措施：**
- 检测多个常见 shell（bash、zsh、fish）
- 提供手动配置选项
- 在文档中说明如何手动配置

### 风险 3：AOT 限制

**风险描述：** 条件编译和平台特定代码可能影响 AOT。

**缓解措施：**
- 使用编译时常量（`#if WINDOWS`、`#if UNIX`）
- 避免运行时类型检测
- 为每个平台单独编译 AOT 可执行文件

### 风险 4：配置文件兼容性

**风险描述：** settings.json 在不同平台间的兼容性。

**缓解措施：**
- 使用跨平台的 JSON 格式
- 路径使用相对路径或平台占位符
- 路径在运行时动态解析

## Migration Plan

无需迁移 - 这是全新功能。

## Open Questions

1. **问：** 是否需要支持配置导入/导出功能？
   **答：** 初期不支持，可在后续版本中根据需求添加。

2. **问：** 是否需要支持配置模板？
   **答：** 初期不支持，保持简单。

3. **问：** test 命令应该调用什么 API 端点？
   **答：** 调用 Anthropic API 的 `/models` 端点或简单的 `messages` 端点进行健康检查。

4. **问：** Linux/macOS 下如何处理不同 shell？
   **答：** 检测 $SHELL 环境变量，为支持的 shell（bash、zsh、fish）生成对应脚本，其他 shell 用户需要手动配置。

## Dependencies

| 包名 | 版本 | 用途 | 平台 |
|------|------|------|------|
| System.CommandLine | 2.x | 命令行参数解析 | 全平台 |
| System.Text.Json | 8.x+ | JSON 序列化 | 全平台 |
| Microsoft.Win32.Registry | 8.x+ | 注册表访问 | Windows only |

## Build Configuration

```xml
<!-- ClaudeCodeApiConfigManager.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>

    <!-- 平台特定定义 -->
    <DefineConstants Condition="$([MSBuild]::IsOSPlatform('Windows'))">$(DefineConstants);WINDOWS</DefineConstants>
    <DefineConstants Condition="$([MSBuild]::IsOSPlatform('Linux'))">$(DefineConstants);UNIX;LINUX</DefineConstants>
    <DefineConstants Condition="$([MSBuild]::IsOSPlatform('OSX'))">$(DefineConstants);UNIX;MACOS</DefineConstants>
  </PropertyGroup>

  <!-- Windows 特定引用 -->
  <ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
    <PackageReference Include="Microsoft.Win32.Registry" Version="8.*" />
  </ItemGroup>

  <!-- 全平台引用 -->
  <ItemGroup>
    <PackageReference Include="System.CommandLine" Version="2.*" />
  </ItemGroup>
</Project>
```

## Shell 初始化脚本示例

### Bash/Zsh 初始化代码

```bash
#~/.ccm/env.sh
# 此文件由 ccm 自动生成，请勿手动编辑

export ANTHROPIC_AUTH_TOKEN="sk-ant-xxx"
export ANTHROPIC_BASE_URL="https://api.anthropic.com"
export ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"
export API_TIMEOUT_MS="3000000"
```

### Fish 初始化代码

```fish
#~/.ccm/env.fish
# 此文件由 ccm 自动生成，请勿手动编辑

set -x ANTHROPIC_AUTH_TOKEN "sk-ant-xxx"
set -x ANTHROPIC_BASE_URL "https://api.anthropic.com"
set -x ANTHROPIC_MODEL "claude-3-5-sonnet-20241022"
set -x API_TIMEOUT_MS "3000000"
```

### Shell 配置文件中的初始化

```bash
# ~/.bashrc 或 ~/.zshrc

# Claude Code API 配置管理
export CLAUDE_CONFIG_HOME="$HOME/.ccm"
if [[ -f "$CLAUDE_CONFIG_HOME/env.sh" ]]; then
    source "$CLAUDE_CONFIG_HOME/env.sh"
fi
```

```fish
# ~/.config/fish/config.fish

# Claude Code API 配置管理
set -x CLAUDE_CONFIG_HOME "$HOME/.ccm"
if test -f "$CLAUDE_CONFIG_HOME/env.fish"
    source "$CLAUDE_CONFIG_HOME/env.fish"
end
```
