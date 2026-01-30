## ADDED Requirements

### Requirement: 首次运行初始化

The system SHALL provide an initialization wizard when the user runs the program without arguments for the first time. 系统应在用户首次无参数运行程序时提供初始化向导。

#### Scenario: 首次运行检测

- **GIVEN** 用户运行 `ccm`（无任何参数）
- **AND** 配置文件 `settings.json` 不存在
- **WHEN** 程序启动
- **THEN** 系统应显示欢迎信息
- **AND** 系统应显示将要执行的操作列表
- **AND** 系统应询问用户是否继续

#### Scenario: 用户确认初始化

- **GIVEN** 用户看到了初始化向导
- **WHEN** 用户输入 `y` 或 `Y` 确认
- **THEN** 系统应创建配置文件
- **AND** 系统应执行全局安装
- **AND** 系统应显示成功信息

#### Scenario: 用户取消初始化

- **GIVEN** 用户看到了初始化向导
- **WHEN** 用户输入 `n` 或 `N` 取消
- **THEN** 系统应显示退出信息
- **AND** 系统应不执行任何操作
- **AND** 程序应退出

#### Scenario: 配置文件已存在

- **GIVEN** 用户运行 `ccm`（无任何参数）
- **AND** 配置文件 `settings.json` 已存在
- **WHEN** 程序启动
- **THEN** 系统应显示帮助信息（等同于 `--help`）
- **AND** 不应显示初始化向导

---

### Requirement: 默认配置文件创建

The system SHALL create a default configuration file with predefined configuration templates. 系统应创建包含预设配置模板的默认配置文件。

#### Scenario: 创建 Windows 配置文件

- **GIVEN** 当前平台是 Windows
- **AND** 配置文件不存在
- **WHEN** 执行初始化
- **THEN** 系统应在可执行文件目录创建 `settings.json`
- **AND** 文件应包含所有预设配置（zhipu、ds、mm、kimi、qwen3、qwen3-coding）
- **AND** 所有配置的 `authToken` 应为空字符串

#### Scenario: 配置文件已存在时询问

- **GIVEN** 配置文件已存在
- **WHEN** 执行初始化
- **THEN** 系统应询问用户是否覆盖现有配置文件
- **AND** 如果用户确认，应覆盖文件
- **AND** 如果用户拒绝，应保留现有文件

#### Scenario: 创建 Unix 配置文件

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 配置文件不存在
- **WHEN** 执行初始化
- **THEN** 系统应在 `~/.config/ClaudeCodeApiConfigManager/` 创建 `settings.json`
- **AND** 文件应包含所有预设配置
- **AND** 所有配置的 `authToken` 应为空字符串

#### Scenario: 配置文件格式验证

- **GIVEN** 配置文件已创建
- **WHEN** 读取配置文件
- **THEN** JSON 格式应有效
- **AND** 应包含 `configs` 数组
- **AND** 应包含 `activeConfigName` 字段（值为 null）
- **AND** 每个 config 应包含 name、authToken、baseUrl、model、customParams 字段

---

### Requirement: Windows 全局安装

The system SHALL support global installation on Windows using an intelligent strategy based on the executable location and directory contents. 系统应支持在 Windows 上使用基于可执行文件位置和目录内容的智能策略来实现全局安装。

#### Scenario: 检测安装环境

- **GIVEN** 当前平台是 Windows
- **WHEN** 执行安装前检测
- **THEN** 系统应检查当前可执行文件所在盘符
- **AND** 系统应检查当前目录下的文件列表
- **AND** 系统应检查当前目录是否在用户 PATH 中

#### Scenario: 非 C 盘且目录干净时的安装

- **GIVEN** 当前平台是 Windows
- **AND** 可执行文件不在 C 盘（如 D:\tools\ccm.exe）
- **AND** 当前目录只包含 ccm.exe 和可选的 settings.json
- **WHEN** 执行安装
- **THEN** 系统应将当前目录添加到用户 PATH
- **AND** 系统应不复制文件
- **AND** 系统应显示 "已将当前目录添加到 PATH" 的消息

#### Scenario: C 盘时的安装

- **GIVEN** 当前平台是 Windows
- **AND** 可执行文件在 C 盘
- **WHEN** 执行安装
- **THEN** 系统应创建目录 `%USERPROFILE%\.ccm\`
- **AND** 系统应复制 `ccm.exe` 到该目录
- **AND** 系统应将该目录添加到用户 PATH
- **AND** 系统应将配置文件也放在 `%USERPROFILE%\.ccm\`
- **AND** 系统应显示安装位置

#### Scenario: 非 C 盘但目录有其他文件时的安装

- **GIVEN** 当前平台是 Windows
- **AND** 可执行文件不在 C 盘
- **AND** 当前目录包含 ccm.exe 以外的其他文件（除了 settings.json）
- **WHEN** 执行安装
- **THEN** 系统应直接复制 `ccm.exe` 到 `%USERPROFILE%\.ccm\`（无需用户确认）
- **AND** 系统应将 `%USERPROFILE%\.ccm` 添加到用户 PATH
- **AND** 系统应将配置文件放在 `%USERPROFILE%\.ccm\`
- **AND** 系统应显示安装位置

#### Scenario: 目标文件已存在时询问

- **GIVEN** 当前平台是 Windows
- **AND** 需要复制文件到目标位置
- **AND** 目标位置已存在 ccm.exe
- **WHEN** 执行安装
- **THEN** 系统应询问用户是否覆盖现有文件
- **AND** 如果用户确认，应覆盖文件
- **AND** 如果用户拒绝，应跳过文件复制并继续其他步骤

#### Scenario: 目录已在 PATH 中

- **GIVEN** 当前目录（或目标目录）已在用户 PATH 中
- **WHEN** 执行安装
- **THEN** 系统应跳过 PATH 修改步骤
- **AND** 系统应显示 "目录已在 PATH 中" 的消息

#### Scenario: 安装成功反馈

- **GIVEN** 安装操作成功完成
- **WHEN** 安装完成
- **THEN** 系统应显示成功消息
- **AND** 应显示安装方式（添加 PATH 或复制文件）
- **AND** 应显示下一步操作提示

---

### Requirement: Unix 全局安装

The system SHALL support global installation on Unix-like systems (Linux/macOS) by creating a symbolic link in the appropriate directory based on PATH availability. 系统应支持在类 Unix 系统（Linux/macOS）上通过根据 PATH 可用性在适当目录中创建符号链接来实现全局安装。

#### Scenario: 检测安装环境

- **GIVEN** 当前平台是 Linux 或 macOS
- **WHEN** 执行安装前检测
- **THEN** 系统应检查 `~/.local/bin` 是否在 PATH 环境变量中
- **AND** 系统应检查 `/usr/local/bin` 是否在 PATH 环境变量中
- **AND** 系统应检查这些目录是否存在
- **AND** 系统应检查是否有创建权限

#### Scenario: ~/.local/bin 在 PATH 时的安装

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** `~/.local/bin` 在 PATH 环境变量中
- **WHEN** 执行安装
- **THEN** 系统应确保 `~/.local/bin` 目录存在
- **AND** 系统应检查 `~/.local/bin/ccm` 是否已存在
- **AND** 如果已存在，应询问用户是否覆盖
- **AND** 系统应创建符号链接 `~/.local/bin/ccm` 指向当前可执行文件
- **AND** 应显示安装位置

#### Scenario: 符号链接已存在时询问

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 目标位置的符号链接已存在
- **WHEN** 执行安装
- **THEN** 系统应询问用户是否覆盖现有符号链接
- **AND** 如果用户确认，应删除并重新创建链接
- **AND** 如果用户拒绝，应跳过链接创建并继续其他步骤

#### Scenario: ~/.local/bin 不在 PATH 时尝试 /usr/local/bin

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** `~/.local/bin` 不在 PATH 中
- **AND** `/usr/local/bin` 在 PATH 中
- **WHEN** 执行安装
- **THEN** 系统应尝试创建符号链接 `/usr/local/bin/ccm`
- **AND** 如果权限不足，应提示需要 sudo
- **AND** 应显示安装位置

#### Scenario: 两个目录都不在 PATH 时

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** `~/.local/bin` 不在 PATH 中
- **AND** `/usr/local/bin` 不在 PATH 中
- **WHEN** 执行安装
- **THEN** 系统应优先尝试创建符号链接到 `~/.local/bin/ccm`
- **AND** 系统应显示警告信息
- **AND** 系统应显示如何将 `~/.local/bin` 添加到 PATH 的说明

#### Scenario: 创建符号链接失败（权限不足）

- **GIVEN** 系统无法创建符号链接到目标位置（权限不足）
- **WHEN** 尝试创建链接
- **THEN** 系统应显示错误信息
- **AND** 应建议用户将 `~/.local/bin` 添加到 PATH 后重新运行
- **AND** 应提供添加 PATH 的命令示例

#### Scenario: 安装成功反馈

- **GIVEN** 安装操作成功完成
- **WHEN** 安装完成
- **THEN** 系统应显示成功消息
- **AND** 应显示符号链接位置
- **AND** 应显示下一步操作提示

---

### Requirement: 卸载命令

The system SHALL provide an `uninstall` command to remove the global installation and optionally clean up configuration files. 系统应提供 `uninstall` 命令来删除全局安装并可选地清理配置文件。

#### Scenario: Windows 卸载流程（已添加 PATH 的目录）

- **GIVEN** 当前平台是 Windows
- **AND** 安装方式是将目录添加到 PATH（如 D:\tools\）
- **WHEN** 用户执行 `ccm uninstall`
- **THEN** 系统应显示将要执行的操作（从 PATH 移除目录）
- **AND** 系统应询问用户是否确认
- **AND** 如果用户确认，应从用户 PATH 中移除该目录
- **AND** 系统应不删除可执行文件

#### Scenario: Windows 卸载流程（复制的文件）

- **GIVEN** 当前平台是 Windows
- **AND** 安装方式是复制到 `%USERPROFILE%\.ccm\`
- **WHEN** 用户执行 `ccm uninstall`
- **THEN** 系统应显示将要执行的操作（删除文件和从 PATH 移除）
- **AND** 系统应询问用户是否确认
- **AND** 如果用户确认，应删除 `%USERPROFILE%\.ccm\ccm.exe`
- **AND** 应从用户 PATH 中移除 `%USERPROFILE%\.ccm`
- **AND** 如果目录为空，应删除目录

#### Scenario: Unix 卸载流程（~/.local/bin）

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 符号链接 `~/.local/bin/ccm` 已存在
- **WHEN** 用户执行 `ccm uninstall`
- **THEN** 系统应显示将要执行的操作（删除符号链接）
- **AND** 系统应询问用户是否确认
- **AND** 如果用户确认，应删除符号链接

#### Scenario: Unix 卸载流程（/usr/local/bin）

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 符号链接 `/usr/local/bin/ccm` 已存在
- **WHEN** 用户执行 `ccm uninstall`
- **THEN** 系统应显示将要执行的操作
- **AND** 系统应提示此操作需要 sudo 权限
- **AND** 系统应询问用户是否确认
- **AND** 如果用户确认，应尝试删除符号链接

#### Scenario: 卸载时同时删除配置

- **GIVEN** 用户执行 `ccm uninstall --remove-config`
- **AND** 用户确认删除配置
- **WHEN** 执行卸载
- **THEN** 系统应删除全局命令
- **AND** 系统应删除配置目录及其中的所有文件

#### Scenario: 卸载未安装的程序

- **GIVEN** 系统未执行过全局安装
- **WHEN** 用户执行 `ccm uninstall`
- **THEN** 系统应显示 "未检测到全局安装"
- **AND** 不应执行任何删除操作

---

### Requirement: 配置模板管理

The system SHALL store default configuration templates in code to avoid external file dependencies. 系统应将默认配置模板存储在代码中以避免外部文件依赖。

#### Scenario: 配置模板完整性

- **GIVEN** 程序初始化
- **WHEN** 读取配置模板
- **THEN** 模板应包含 6 个预设配置
- **AND** 每个配置应包含 name、baseUrl、model 字段
- **AND** 所有 authToken 应为空字符串

#### Scenario: 预设配置列表

- **GIVEN** 配置模板被加载
- **THEN** 应包含以下配置：
  - zhipu: https://open.bigmodel.cn/api/anthropic, glm-4.7
  - ds: https://api.deepseek.com/anthropic, deepseek-chat
  - mm: https://api.minimaxi.com/anthropic, MiniMax-M2.1
  - kimi: https://api.moonshot.cn/anthropic, kimi-k2.5
  - qwen3: https://dashscope.aliyuncs.com/apps/anthropic, qwen3-coder-plus
  - qwen3-coding: https://coding.dashscope.aliyuncs.com/apps/anthropic, qwen3-coder-plus

---

### Requirement: AOT 兼容性

The system SHALL maintain AOT compatibility when implementing global installation features. 系统在实现全局安装功能时应保持 AOT 兼容性。

#### Scenario: 配置模板序列化

- **GIVEN** 配置模板存储为字符串常量
- **WHEN** 反序列化为配置对象
- **THEN** 应使用 JsonSerializerContext Source Generator
- **AND** 不应使用运行时反射

#### Scenario: 平台检测

- **GIVEN** 代码需要根据平台执行不同逻辑
- **WHEN** 区分 Windows 和 Unix 平台
- **THEN** 应使用条件编译 (#if WINDOWS, #if UNIX)
- **AND** 不应使用运行时平台检测和动态加载

---

### Requirement: 用户交互

The system SHALL provide clear and friendly user interactions during installation and uninstallation. 系统应在安装和卸载过程中提供清晰友好的用户交互。

#### Scenario: 欢迎信息

- **GIVEN** 用户首次运行程序
- **WHEN** 显示初始化向导
- **THEN** 应显示 "欢迎使用 Claude Code API 配置管理器 (ccm)!"
- **AND** 应列出将要执行的操作
- **AND** 应提示用户输入 y/n 确认

#### Scenario: 错误提示

- **GIVEN** 安装或卸载过程中发生错误
- **WHEN** 捕获到异常
- **THEN** 应显示明确的错误信息
- **AND** 应提供解决建议
- **AND** 不应显示原始异常堆栈（除非是调试模式）

#### Scenario: 成功反馈

- **GIVEN** 操作成功完成
- **WHEN** 安装或卸载完成
- **THEN** 应显示带勾标记的成功信息
- **AND** 应显示下一步操作建议

#### Scenario: 进度提示

- **GIVEN** 操作包含多个步骤
- **WHEN** 执行每个步骤
- **THEN** 应显示当前步骤的简短描述
- **AND** 关键步骤完成后应显示确认信息

---

### Requirement: 安装状态检测

The system SHALL be able to detect the current installation status and location to avoid duplicate installations and properly handle uninstallation. 系统应能够检测当前安装状态和位置，以避免重复安装并正确处理卸载。

#### Scenario: 检测已安装状态（Windows - PATH 方式）

- **GIVEN** Windows 平台
- **AND** 某个目录（如 D:\tools\）已被添加到 PATH 作为 ccm 安装目录
- **WHEN** 执行安装前检测
- **THEN** 系统应识别为已安装
- **AND** 系统应识别安装方式（添加 PATH）
- **AND** 应询问用户是否重新安装

#### Scenario: 检测已安装状态（Windows - 复制方式）

- **GIVEN** Windows 平台
- **AND** `ccm.exe` 已存在于 `%USERPROFILE%\.ccm\`
- **AND** 该目录在 PATH 中
- **WHEN** 执行安装前检测
- **THEN** 系统应识别为已安装
- **AND** 系统应识别安装方式（复制到 .ccm）
- **AND** 应询问用户是否重新安装

#### Scenario: 检测已安装状态（Unix - ~/.local/bin）

- **GIVEN** Unix 平台
- **AND** 符号链接 `~/.local/bin/ccm` 已存在
- **WHEN** 执行安装前检测
- **THEN** 系统应识别为已安装
- **AND** 系统应记录安装位置
- **AND** 应询问用户是否重新安装

#### Scenario: 检测已安装状态（Unix - /usr/local/bin）

- **GIVEN** Unix 平台
- **AND** 符号链接 `/usr/local/bin/ccm` 已存在
- **WHEN** 执行安装前检测
- **THEN** 系统应识别为已安装
- **AND** 系统应记录安装位置
- **AND** 应询问用户是否重新安装

#### Scenario: 检测未安装状态

- **GIVEN** 系统未执行过全局安装
- **WHEN** 执行安装前检测
- **THEN** 系统应识别为未安装
- **AND** 应继续执行安装流程
