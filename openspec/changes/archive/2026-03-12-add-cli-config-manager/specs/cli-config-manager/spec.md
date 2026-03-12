## ADDED Requirements

### Requirement: CLI 命令解析

The system SHALL provide a command-line interface to parse and execute user commands. 系统必须提供命令行界面来解析和执行用户输入的命令。

#### Scenario: 识别根命令

- **GIVEN** 用户启动应用程序
- **WHEN** 用户输入 `ccm <command> [arguments]`
- **THEN** 系统应识别并执行对应的命令

#### Scenario: 显示帮助信息

- **GIVEN** 用户输入 `ccm --help` 或 `ccm -h`
- **WHEN** 请求帮助信息
- **THEN** 系统应显示所有可用命令及其用法

#### Scenario: 处理未知命令

- **GIVEN** 用户输入不存在的命令
- **WHEN** 系统无法识别命令
- **THEN** 系统应显示错误信息并列出可用命令

---

### Requirement: 添加配置

The system SHALL provide an `add` command to add new API configurations. 系统必须提供 `add` 命令来添加新的 API 配置。

#### Scenario: 添加配置 - 基本格式

- **GIVEN** 用户输入 `add official sk-ant-xxx https://api.anthropic.com claude-3-5-sonnet-20241022`
- **WHEN** 执行添加命令
- **THEN** 系统应保存配置到 settings.json
- **AND** 配置应包含名称、Token、BaseUrl、Model

#### Scenario: 添加配置 - Token 和 URL 位置调换

- **GIVEN** 用户输入 `add proxy1 https://proxy.example.com sk-ant-yyy claude-3-5-haiku-20241022`
- **WHEN** 执行添加命令
- **THEN** 系统应正确识别 URL 和 Token（基于 http/https 前缀）
- **AND** 配置应正确保存

#### Scenario: 添加配置 - 带自定义参数

- **GIVEN** 用户输入 `add custom sk-ant-zzz https://api.example.com claude-3-5-sonnet-20241022 API_TIMEOUT_MS=3000000 ANTHROPIC_SMALL_FAST_MODEL=glm-4.7`
- **WHEN** 执行添加命令
- **THEN** 系统应解析并保存自定义参数到配置的 CustomParams 字典
- **AND** 自定义参数应包含 `API_TIMEOUT_MS` 和 `ANTHROPIC_SMALL_FAST_MODEL`

#### Scenario: 配置名称冲突 - 用户选择覆盖

- **GIVEN** 配置名称 "official" 已存在
- **WHEN** 用户再次添加名为 "official" 的配置并确认覆盖
- **THEN** 系统应更新现有配置
- **AND** 不应创建重复的配置项

#### Scenario: 配置名称冲突 - 用户选择不覆盖

- **GIVEN** 配置名称 "official" 已存在
- **WHEN** 用户再次添加名为 "official" 的配置并选择不覆盖
- **THEN** 系统应取消添加操作
- **AND** 原配置应保持不变

#### Scenario: 添加成功反馈

- **GIVEN** 用户成功添加配置
- **WHEN** 添加操作完成
- **THEN** 系统应显示成功消息，如 "配置 'official' 已成功添加"

---

### Requirement: 列出配置

The system SHALL provide `list` and `ls` commands to list all saved configurations. 系统必须提供 `list` 和 `ls` 命令来列出所有已保存的配置。

#### Scenario: 列出所有配置

- **GIVEN** 系统中存在多个配置（official、proxy1、custom）
- **WHEN** 用户执行 `list` 或 `ls` 命令
- **THEN** 系统应以 "名称（模型）" 格式显示所有配置
- **AND** 当前使用的配置应有标记（如 `*` 前缀）

#### Scenario: 空配置列表

- **GIVEN** 系统中没有任何配置
- **WHEN** 用户执行 `list` 命令
- **THEN** 系统应显示 "暂无配置，使用 add 命令添加配置"

---

### Requirement: 切换配置

The system SHALL provide a `use` command to switch the currently active API configuration. 系统必须提供 `use` 命令来切换当前使用的 API 配置。

#### Scenario: 切换到指定配置（Windows）

- **GIVEN** 系统中存在配置 "proxy1"
- **AND** 当前平台是 Windows
- **WHEN** 用户执行 `use proxy1`
- **THEN** 系统应修改注册表 `HKEY_CURRENT_USER\Environment` 中的环境变量
- **AND** 应更新 settings.json 中的 ActiveConfigName
- **AND** 应发送 WM_SETTINGCHANGE 消息通知系统
- **AND** 应显示成功消息

#### Scenario: 切换到指定配置（Linux/macOS）

- **GIVEN** 系统中存在配置 "proxy1"
- **AND** 当前平台是 Linux 或 macOS
- **WHEN** 用户执行 `use proxy1`
- **THEN** 系统应生成/更新 `~/.claude-config/env.sh` 或 `env.fish` 脚本
- **AND** 应更新 settings.json 中的 ActiveConfigName
- **AND** 如果是首次配置，应在 shell 配置文件中添加初始化代码
- **AND** 应提示用户运行 `source ~/.bashrc` 或重启终端

#### Scenario: 切换到不存在的配置

- **GIVEN** 用户尝试切换到不存在的配置 "nonexistent"
- **WHEN** 用户执行 `use nonexistent`
- **THEN** 系统应显示错误消息 "配置 'nonexistent' 不存在"
- **AND** 不应修改任何环境变量或脚本

#### Scenario: 切换带自定义参数的配置

- **GIVEN** 配置 "custom" 包含自定义参数 API_TIMEOUT_MS=3000000
- **WHEN** 用户执行 `use custom`
- **THEN** 系统应同时设置自定义参数对应的环境变量或脚本
- **AND** 所有环境变量（标准 + 自定义）都应被正确设置

---

### Requirement: 查看当前配置

The system SHALL provide `current` and `c` commands to display the currently active configuration. 系统必须提供 `current` 和 `c` 命令来查看当前使用的配置。

#### Scenario: 显示当前配置

- **GIVEN** 当前使用的配置是 "official"
- **WHEN** 用户执行 `current` 或 `c` 命令
- **THEN** 系统应显示当前配置的详细信息
- **AND** 信息应包括：名称、Model、BaseUrl

#### Scenario: 无当前配置

- **GIVEN** 系统中有配置但未设置当前配置
- **WHEN** 用户执行 `current` 命令
- **THEN** 系统应显示 "未设置当前配置，使用 use 命令切换配置"

#### Scenario: 配置为空时查看当前配置

- **GIVEN** 系统中没有任何配置
- **WHEN** 用户执行 `current` 命令
- **THEN** 系统应显示 "暂无配置"

---

### Requirement: 删除配置

The system SHALL provide `remove` and `del` commands to delete specified configurations. 系统必须提供 `remove` 和 `del` 命令来删除指定配置。

#### Scenario: 删除指定配置

- **GIVEN** 系统中存在配置 "proxy1"
- **WHEN** 用户执行 `remove proxy1` 或 `del proxy1`
- **THEN** 系统应从配置列表中移除该配置
- **AND** 应显示成功消息

#### Scenario: 删除不存在的配置

- **GIVEN** 系统中不存在配置 "nonexistent"
- **WHEN** 用户执行 `remove nonexistent`
- **THEN** 系统应显示错误消息 "配置 'nonexistent' 不存在"

#### Scenario: 删除当前活动配置

- **GIVEN** 配置 "official" 是当前活动配置
- **WHEN** 用户执行 `remove official`
- **THEN** 系统应删除该配置
- **AND** 应将 ActiveConfigName 设置为 null
- **AND** 应提示用户 "已删除当前活动配置"

---

### Requirement: 测试配置

The system SHALL provide a `test` command to manually test API connectivity. 系统必须提供 `test` 命令来手动测试 API 连接。

#### Scenario: 测试当前配置成功

- **GIVEN** 当前配置的 API 可访问
- **WHEN** 用户执行 `test` 命令
- **THEN** 系统应向 API 发送测试请求
- **AND** 应显示 "连接成功" 消息
- **AND** 应显示响应时间

#### Scenario: 测试当前配置失败

- **GIVEN** 当前配置的 API 不可访问或认证失败
- **WHEN** 用户执行 `test` 命令
- **THEN** 系统应显示 "连接失败" 消息
- **AND** 应显示错误原因（如网络错误、认证失败）

#### Scenario: 无配置时测试

- **GIVEN** 系统中没有当前活动配置
- **WHEN** 用户执行 `test` 命令
- **THEN** 系统应显示错误消息 "未设置当前配置，无法测试"

---

### Requirement: 配置文件管理

The system SHALL persist configurations to a settings.json file in a platform-specific user configuration directory. 系统必须将配置持久化到平台特定的用户配置目录中的 settings.json 文件。

#### Scenario: 配置文件不存在时创建

- **GIVEN** 首次运行应用程序
- **WHEN** 用户添加第一个配置
- **THEN** 系统应在平台特定的配置目录下创建 settings.json：
  - Windows: `%APPDATA%\ClaudeCodeApiConfigManager\settings.json`
  - Linux/macOS: `~/.config/ClaudeCodeApiConfigManager/settings.json`
- **AND** 文件应包含正确的 JSON 结构

#### Scenario: 配置文件已存在时读取

- **GIVEN** settings.json 已存在并包含配置
- **WHEN** 用户执行任何需要读取配置的命令
- **THEN** 系统应从文件中读取现有配置

#### Scenario: 配置文件损坏时处理

- **GIVEN** settings.json 文件格式损坏
- **WHEN** 用户执行需要读取配置的命令
- **THEN** 系统应显示友好的错误消息
- **AND** 应建议用户检查或删除配置文件

---

### Requirement: 跨平台环境变量管理

The system MUST be able to persistently modify user-level environment variables on Windows, Linux, and macOS platforms. 系统必须能够在 Windows、Linux 和 macOS 平台上持久化修改用户级环境变量。

#### Scenario: Windows 平台 - 修改环境变量

- **GIVEN** 当前平台是 Windows
- **WHEN** 用户执行 `use` 命令切换配置
- **THEN** 系统应修改注册表 `HKEY_CURRENT_USER\Environment` 中的：
  - ANTHROPIC_AUTH_TOKEN
  - ANTHROPIC_BASE_URL
  - ANTHROPIC_MODEL
  - 自定义参数对应的环境变量
- **AND** 应发送 WM_SETTINGCHANGE 消息通知系统
- **AND** 不应需要管理员权限

#### Scenario: Linux/macOS 平台 - 检测 Shell 类型

- **GIVEN** 当前平台是 Linux 或 macOS
- **WHEN** 用户首次执行 `use` 命令
- **THEN** 系统应检测用户的默认 shell（通过 $SHELL 环境变量）
- **AND** 系统应支持 bash、zsh 和 fish shell

#### Scenario: Linux/macOS 平台 - 生成环境变量脚本

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 用户使用 bash 或 zsh
- **WHEN** 用户执行 `use` 命令切换配置
- **THEN** 系统应生成/更新 `~/.claude-config/env.sh` 文件
- **AND** 文件应包含所有必需的 export 语句
- **AND** 应包含文件头注释说明这是自动生成的文件

#### Scenario: Linux/macOS 平台 - Fish Shell 支持

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 用户使用 fish shell
- **WHEN** 用户执行 `use` 命令切换配置
- **THEN** 系统应生成/更新 `~/.claude-config/env.fish` 文件
- **AND** 文件应使用 fish 的 `set -x` 语法而非 export

#### Scenario: Linux/macOS 平台 - 添加 Shell 初始化代码

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 用户的 shell 配置文件中尚未添加初始化代码
- **WHEN** 用户首次执行 `use` 命令
- **THEN** 系统应在相应的 shell 配置文件中添加初始化代码：
  - bash: `~/.bashrc` 或 `~/.bash_profile`
  - zsh: `~/.zshrc`
  - fish: `~/.config/fish/config.fish`
- **AND** 初始化代码应 source `~/.claude-config/env.sh` 或 `env.fish`
- **AND** 应提示用户运行 `source ~/.bashrc` 或重启终端

#### Scenario: Linux/macOS 平台 - 初始化代码已存在

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 用户的 shell 配置文件中已包含初始化代码
- **WHEN** 用户执行 `use` 命令
- **THEN** 系统不应重复添加初始化代码
- **AND** 系统应只更新 `~/.claude-config/env.sh` 文件

#### Scenario: Linux/macOS 平台 - 不支持的 Shell

- **GIVEN** 当前平台是 Linux 或 macOS
- **AND** 用户使用不支持的 shell（如 csh、tcsh）
- **WHEN** 用户执行 `use` 命令
- **THEN** 系统应显示警告消息 "当前 shell 暂不支持，请手动配置环境变量"
- **AND** 系统应列出需要设置的环境变量及其值
- **AND** 系统应继续生成通用格式的 env.sh 文件供用户参考

---

### Requirement: 平台检测

The system MUST accurately detect the current operating system and use the appropriate environment variable persistence mechanism. 系统必须准确检测当前操作系统并使用相应的环境变量持久化机制。

#### Scenario: 检测 Windows 平台

- **GIVEN** 应用程序在 Windows 上运行
- **WHEN** 系统初始化
- **THEN** 系统应检测到 Windows 平台
- **AND** 应使用注册表方式修改环境变量

#### Scenario: 检测 Linux 平台

- **GIVEN** 应用程序在 Linux 上运行
- **WHEN** 系统初始化
- **THEN** 系统应检测到 Linux 平台
- **AND** 应使用 shell 脚本方式修改环境变量

#### Scenario: 检测 macOS 平台

- **GIVEN** 应用程序在 macOS 上运行
- **WHEN** 系统初始化
- **THEN** 系统应检测到 macOS 平台
- **AND** 应使用 shell 脚本方式修改环境变量
