# Change: 添加全局安装和卸载功能

## Why

当前 ccm 工具需要在本地目录运行，用户需要手动将可执行文件添加到系统路径才能全局使用。这降低了工具的易用性。类似于 `nvm` 和 `npm` 等工具，ccm 应该提供简化的安装和卸载机制，让用户能够：

1. **一键全局安装** - 通过运行可执行文件即可完成全局配置，无需手动设置环境变量
2. **自动初始化配置** - 首次运行时自动创建配置文件并预设常用配置模板
3. **便捷卸载** - 提供卸载命令清理全局配置

## What Changes

### 1. 首次运行初始化功能

当用户无参数启动程序时，经用户确认后执行以下操作：

- **配置文件初始化**：
  - 检查配置目录是否已存在 `settings.json`
  - 如果不存在，创建包含预设配置模板的 `settings.json`
  - 如果存在，询问用户是否覆盖
  - 配置目录：
    - Windows: 可执行文件目录
    - Linux/macOS: `~/.config/ClaudeCodeApiConfigManager/`

- **全局安装**：
  - **Windows**: 智能安装策略
    - 如果当前目录非 C 盘且目录下只有 `ccm.exe` 和 `settings.json`（可选）：将当前目录添加到 PATH
    - 如果当前目录非 C 盘但目录有其他文件：复制 `ccm.exe` 到 `%USERPROFILE%\.ccm\` 并将该目录添加到 PATH（无需确认）
    - 如果当前目录是 C 盘：复制 `ccm.exe` 到 `%USERPROFILE%\.ccm\` 并将该目录添加到 PATH
    - 复制文件前检查目标是否存在，如果存在则询问是否覆盖
  - **Linux/macOS**: 智能安装策略
    - 如果 `~/.local/bin` 在 PATH 中：创建符号链接到 `~/.local/bin/ccm`
    - 如果 `~/.local/bin` 不在 PATH 中：创建符号链接到 `/usr/local/bin/ccm`（需要 sudo）
    - 创建链接前检查目标是否存在，如果存在则询问是否覆盖

### 2. 卸载命令

添加 `uninstall` 命令，执行以下操作：

- **Windows**:
  - 检测安装位置（原始目录或 `%USERPROFILE%\.ccm\`）
  - 删除可执行文件（如果是复制的）
  - 从用户 PATH 环境变量中移除对应路径

- **Linux/macOS**:
  - 检测安装位置（`~/.local/bin/ccm` 或 `/usr/local/bin/ccm`）
  - 删除符号链接（可能需要 sudo）

- **可选**：询问用户是否同时删除配置文件和配置目录

### 3. 新增和修改的命令

- **无参数启动**: 进入初始化向导
- **`ccm uninstall`**: 卸载全局命令

## Impact

- **受影响的规范**: 新增 `global-installation` 规范，修改 `cli-config-manager` 规范
- **受影响的代码**:
  - `Program.cs` - 添加无参数时的处理逻辑
  - 新增 `InitService.cs` - 处理初始化和安装逻辑
  - 新增 `InstallService.cs` - 平台特定的安装/卸载逻辑
  - 新增 `Constants` - 配置模板常量
  - `CommandBuilder.cs` - 添加 uninstall 命令

## Breaking Changes

无 - 新增功能，不影响现有功能

## Dependencies

- Windows: 需要 .NET 10.0 AOT 运行时
- Unix: 需要有创建符号链接的权限（安装到 `/usr/local/bin` 时需要 sudo）

## Platform Support

| 平台 | 全局安装方式 | 卸载方式 | AOT 支持 |
|------|-------------|---------|----------|
| Windows | 非 C 盘：添加当前目录到 PATH；C 盘：复制到 %USERPROFILE%\.ccm\ 并添加到 PATH | 从 PATH 移除路径，删除复制的文件 | 支持 |
| Linux | ~/.local/bin 在 PATH：链接到 ~/.local/bin；否则链接到 /usr/local/bin（sudo） | 删除符号链接 | 支持 |
| macOS | ~/.local/bin 在 PATH：链接到 ~/.local/bin；否则链接到 /usr/local/bin（sudo） | 删除符号链接 | 支持 |

## Additional Notes

### 配置模板

配置模板将硬编码在代码中，包含以下预设配置：
- zhipu (智谱 AI)
- ds (DeepSeek)
- mm (MiniMax)
- kimi (Moonshot)
- qwen3 (通义千问)
- qwen3-coding (通义千问编程版)

所有配置的 `authToken` 初始为空字符串，用户需要自行添加。

### 安装检测逻辑

- **Windows**:
  - 检测当前可执行文件所在盘符（是否为 C 盘）
  - 检测当前目录下的文件列表（是否只有 ccm.exe 和可选的 settings.json）
  - 非 C 盘 + 目录干净：将当前目录添加到 PATH
  - 非 C 盘 + 目录有其他文件：复制到 `%USERPROFILE%\.ccm\` 并添加到 PATH
  - C 盘：复制到 `%USERPROFILE%\.ccm\` 并添加到 PATH，配置文件也放在该目录
  - 所有文件写入前检查是否存在，存在则询问是否覆盖

- **Unix**:
  - 检测 `~/.local/bin` 是否在 PATH 环境变量中
  - 如果在，创建符号链接到 `~/.local/bin/ccm`
  - 如果不在，尝试创建符号链接到 `/usr/local/bin/ccm`（需要 sudo 权限）
  - 创建链接前检查是否存在，存在则询问是否覆盖

### AOT 兼容性

- 配置模板使用 `JsonSerializerContext` Source Generator
- 避免使用运行时反射
- 使用条件编译处理平台差异
