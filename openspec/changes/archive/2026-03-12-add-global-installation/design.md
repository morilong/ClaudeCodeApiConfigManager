# Design: 全局安装和卸载功能

## 架构概述

本功能新增三个核心服务类来实现跨平台的安装和卸载功能：

```
Services/
├── InitService.cs          # 初始化服务（配置文件创建 + 全局安装）
├── InstallService.cs       # 平台特定的安装/卸载逻辑
└── Constants.cs (扩展)     # 新增配置模板相关常量
```

## 核心组件设计

### 1. InitService

初始化服务负责首次运行时的用户引导和配置设置。

**职责**:
- 检测是否需要初始化（配置文件是否存在）
- 显示欢迎信息和将要执行的操作
- 获取用户确认
- 协调配置文件创建和全局安装

**核心方法**:
```csharp
public static bool ShouldInitialize()
// 检查是否需要初始化（配置文件不存在或为空）

public static async Task<int> RunInitializeWizard()
// 运行初始化向导，返回退出码
```

**交互流程**:
```
1. 检查配置文件是否存在
2. 如果不存在：
   a. 显示欢迎信息
   b. 显示将要执行的操作：
      - 创建配置文件到 <配置目录>
      - 将可执行文件安装到 <目标目录>
   c. 询问用户是否继续 (y/n)
   d. 如果用户确认：
      - 调用 CreateDefaultConfigFile() 创建配置
      - 调用 InstallService.Install() 执行全局安装
   e. 显示成功信息和下一步提示
```

### 2. InstallService

平台特定的安装/卸载服务，使用条件编译分离 Windows 和 Unix 实现。

**职责**:
- 检测安装目标和环境
- 执行平台特定的安装操作
- 执行平台特定的卸载操作
- 提供安装状态查询

**核心方法**:
```csharp
public static InstallStatus DetectInstallStatus()
// 检测当前安装状态

public static async Task<bool> Install()
// 执行全局安装，返回是否成功

public static async Task<bool> Uninstall(bool removeConfig = false)
// 执行卸载，可选择是否删除配置文件
```

**Windows 实现**:
- 智能安装策略：
  1. 检测当前可执行文件所在盘符和目录内容
  2. 如果非 C 盘且目录只有 `ccm.exe` 和 `settings.json`（可选）：
     - 将当前目录添加到用户 PATH
     - 不复制文件
     - 配置文件放在当前目录（ccm.exe 同目录）
  3. 如果非 C 盘但目录有其他文件：
     - 直接复制 `ccm.exe` 到 `%USERPROFILE%\.ccm\`（无需用户确认）
     - 将 `%USERPROFILE%\.ccm` 添加到用户 PATH
     - 配置文件放在 `%USERPROFILE%\.ccm\`
  4. 如果是 C 盘：
     - 复制 `ccm.exe` 到 `%USERPROFILE%\.ccm\`
     - 将 `%USERPROFILE%\.ccm` 添加到用户 PATH
     - 配置文件放在 `%USERPROFILE%\.ccm\`
- 文件覆盖检测：
  - 复制文件前检查目标是否存在
  - 如果存在，询问用户是否覆盖
  - 创建配置文件前检查是否存在
  - 如果存在，询问用户是否覆盖
- PATH 修改: 使用 `Environment.SetEnvironmentVariable` 修改用户级 PATH
- 检测逻辑:
  - 检查当前盘符（通过 `Path.GetPathRoot()`）
  - 检查当前目录文件列表
  - 检查目录是否在用户 PATH 中
  - 检查目标位置是否已有文件

**Unix 实现**:
- 智能安装策略：
  1. 检测 `~/.local/bin` 是否在 PATH 环境变量中
  2. 如果在 PATH 中：创建符号链接 `~/.local/bin/ccm` → 当前可执行文件
  3. 如果不在 PATH 中：尝试创建符号链接 `/usr/local/bin/ccm`（需要 sudo）
- 安装方式: 创建符号链接 `ccm` → 当前可执行文件
- 文件覆盖检测：
  - 创建符号链接前检查目标是否存在
  - 如果存在，询问用户是否覆盖
- PATH 修改: 不自动修改（Unix 下通常由用户管理 PATH）
- 检测逻辑:
  - 检查 `~/.local/bin` 和 `/usr/local/bin` 是否在 PATH 中
  - 检查目录是否存在
  - 检查符号链接是否存在
  - 检测是否有创建权限（特别是 `/usr/local/bin`）

### 3. 配置模板管理

配置模板硬编码在 `Constants` 类中，避免依赖外部文件。

**实现方式**:
```csharp
public static class ConfigTemplates
{
    public const string DefaultSettingsJson = """
    {
      "configs": [
        {
          "name": "zhipu",
          "authToken": "",
          "baseUrl": "https://open.bigmodel.cn/api/anthropic",
          "model": "glm-4.7",
          "customParams": {}
        },
        // ... 其他配置
      ],
      "activeConfigName": null
    }
    """;
}
```

## Program.cs 修改

```csharp
static int Main(string[] args)
{
    try
    {
        // 检查是否请求显示版本
        if (VersionHelper.IsVersionRequest(args))
        {
            VersionHelper.PrintVersion();
            return 0;
        }

        // 无参数时运行初始化向导
        if (args.Length == 0)
        {
            return InitService.RunInitializeWizard().GetAwaiter().GetResult();
        }

        // ... 其余命令处理逻辑
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

## 新增 uninstall 命令

在 `CommandBuilder.cs` 中添加：

```csharp
public static Command CreateUninstallCommand()
{
    var removeConfigOption = new Option<bool>(
        "--remove-config",
        "同时删除配置文件和配置目录"
    );

    var command = new Command("uninstall", "卸载全局命令")
    {
        removeConfigOption
    };

    command.SetAction(parseResult =>
    {
        var removeConfig = parseResult.GetValue(removeConfigOption);
        InstallService.Uninstall(removeConfig);
    });

    return command;
}
```

## 用户交互设计

### 首次运行输出示例（Windows - 非 C 盘）

```
欢迎使用 Claude Code API 配置管理器 (ccm)!

检测到这是您首次运行 ccm。我们将执行以下操作：

1. 创建配置文件到: D:\tools\settings.json
   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding

2. 安装全局命令: 将当前目录 (D:\tools) 添加到 PATH
   安装后您可以在任何位置使用 'ccm' 命令

是否继续? [Y/n]:
```

### 首次运行输出示例（Windows - C 盘）

```
欢迎使用 Claude Code API 配置管理器 (ccm)!

检测到这是您首次运行 ccm。我们将执行以下操作：

1. 创建配置文件到: C:\Users\username\.ccm\settings.json
   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding

2. 安装全局命令: 复制 ccm.exe 到 C:\Users\username\.ccm\ 并添加到 PATH
   安装后您可以在任何位置使用 'ccm' 命令

是否继续? [Y/n]:
```

### 首次运行输出示例（Windows - 非 C 盘但目录有其他文件）

```
欢迎使用 Claude Code API 配置管理器 (ccm)!

检测到这是您首次运行 ccm。我们将执行以下操作：

1. 创建配置文件到: C:\Users\username\.ccm\settings.json
   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding

2. 安装全局命令: 复制 ccm.exe 到 C:\Users\username\.ccm\ 并添加到 PATH
   安装后您可以在任何位置使用 'ccm' 命令

是否继续? [Y/n]:
```

### 文件已存在时的提示

```
检测到 C:\Users\username\.ccm\ccm.exe 已存在。
是否覆盖? [y/N]:
```

### 首次运行输出示例（Unix - ~/.local/bin 在 PATH）

```
欢迎使用 Claude Code API 配置管理器 (ccm)!

检测到这是您首次运行 ccm。我们将执行以下操作：

1. 创建配置文件到: /home/user/.config/ClaudeCodeApiConfigManager/settings.json
   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding

2. 安装全局命令: 创建符号链接到 /home/user/.local/bin/ccm

是否继续? [Y/n]:
```

### 首次运行输出示例（Unix - ~/.local/bin 不在 PATH）

```
欢迎使用 Claude Code API 配置管理器 (ccm)!

检测到这是您首次运行 ccm。我们将执行以下操作：

1. 创建配置文件到: /home/user/.config/ClaudeCodeApiConfigManager/settings.json
   包含预设配置: zhipu, ds, mm, kimi, qwen3, qwen3-coding

2. 安装全局命令: 创建符号链接到 /usr/local/bin/ccm
   注意: 此操作需要 sudo 权限

是否继续? [Y/n]:
```

### 安装成功输出示例

```
✓ 配置文件已创建
✓ 全局命令已安装

下一步:
1. 运行 'ccm list' 查看可用配置
2. 使用 'ccm add <name> <token> <url> <model>' 添加配置的 Token
3. 使用 'ccm use <name>' 切换配置

提示: 如需卸载，运行 'ccm uninstall'
```

### 卸载输出示例（Windows）

```
检测到 ccm 安装位置: D:\tools\

这将卸载 ccm 全局命令。
- 从 PATH 环境变量中移除: D:\tools

是否同时删除配置文件? [y/N]:

正在卸载...
✓ 已从 PATH 移除

ccm 已卸载。如需重新安装，请再次运行程序。
```

### 卸载输出示例（Unix）

```
检测到 ccm 安装位置: /usr/local/bin/ccm

这将卸载 ccm 全局命令。
- 删除符号链接: /usr/local/bin/ccm
- 注意: 此操作需要 sudo 权限

是否同时删除配置文件? [y/N]:

正在卸载...
✓ 符号链接已删除

ccm 已卸载。如需重新安装，请再次运行程序。
```

## 错误处理

### Windows 安装失败场景

1. **C 盘目录无法创建**
   - 错误: "无法创建安装目录: %USERPROFILE%\.ccm"
   - 建议: "请手动创建目录或检查权限"

2. **复制文件失败**
   - 错误: "无法复制可执行文件到 %USERPROFILE%\.ccm\"
   - 建议: "请检查文件权限或磁盘空间"

3. **PATH 修改失败**
   - 错误: "无法修改用户 PATH 环境变量"
   - 建议: "请手动将目录添加到 PATH"

4. **目标文件已存在 - 用户拒绝覆盖**
   - 提示: "检测到文件已存在，用户取消覆盖"
   - 操作: "跳过文件复制，继续其他安装步骤"

### Unix 安装失败场景

1. **无法创建 ~/.local/bin 符号链接**
   - 错误: "无法创建符号链接: <原因>"
   - 建议: "请检查权限或目录是否存在"

2. **无法创建 /usr/local/bin 符号链接（权限不足）**
   - 错误: "无法创建 /usr/local/bin/ccm: 权限不足"
   - 建议: "请使用 sudo 运行此命令，或手动将 ~/.local/bin 添加到 PATH 后重新运行"

3. **~/.local/bin 不在 PATH 中且无法访问 /usr/local/bin**
   - 警告: "~/.local/bin 不在 PATH 中，且无法写入 /usr/local/bin"
   - 建议: "请先添加以下内容到 ~/.bashrc 或 ~/.zshrc:\nexport PATH=\"$HOME/.local/bin:$PATH\"\n然后重新运行安装"

4. **目标符号链接已存在 - 用户拒绝覆盖**
   - 提示: "检测到符号链接已存在，用户取消覆盖"
   - 操作: "跳过链接创建，继续其他安装步骤"

## AOT 兼容性考虑

1. **避免反射**: 使用 Source Generator 进行 JSON 序列化
2. **条件编译**: 使用 `#if WINDOWS` 和 `#if UNIX` 分离平台代码
3. **字符串处理**: 使用原始字符串字面量 (""") 存储配置模板
4. **异步处理**: 使用 `async/await` 但避免反射-heavy 的操作

## 测试策略

### 手动测试场景

1. **首次运行**:
   - Windows: 验证文件复制和 PATH 修改
   - Linux: 验证符号链接创建
   - macOS: 验证符号链接创建

2. **重复运行**:
   - 验证不会重复安装

3. **卸载**:
   - 验证文件/链接删除
   - 验证可选配置删除

4. **跨平台**:
   - 在每个平台上完整测试安装和使用流程
