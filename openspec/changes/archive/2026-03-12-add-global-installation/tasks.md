# Implementation Tasks

## 1. 项目基础设置和结构
- [x] 1.1 在 `Constants.cs` 中添加配置模板相关常量（ConfigTemplates 类）
- [x] 1.2 在 `Constants.cs` 中添加安装相关常量（目录名、文件名、消息等）

## 2. 配置模板实现
- [x] 2.1 将 `./settings.json` 的内容迁移到代码中的字符串常量
- [x] 2.2 验证配置模板 JSON 格式正确性
- [x] 2.3 确保配置模板兼容现有的 JsonSerializerContext

## 3. InitService 实现
- [x] 3.1 创建 `InitService.cs` 类
- [x] 3.2 实现 `ShouldInitialize()` 方法 - 检查配置文件是否存在
- [x] 3.3 实现 `ShowWelcomeMessage()` 方法 - 显示欢迎信息和操作列表
- [x] 3.4 实现 `ConfirmAction()` 方法 - 获取用户确认
- [x] 3.5 实现 `CreateDefaultConfigFile()` 方法 - 创建默认配置文件（检查是否已存在）
- [x] 3.6 实现 `PromptOverwrite()` 方法 - 文件已存在时询问是否覆盖
- [x] 3.7 实现 `RunInitializeWizard()` 方法 - 协调整个初始化流程

## 4. InstallService 基础结构
- [x] 4.1 创建 `InstallService.cs` 类
- [x] 4.2 定义 `InstallStatus` 枚举（NotInstalled、Installed、NeedsUpdate）
- [x] 4.3 定义 `InstallResult` 类（成功、失败、错误消息）

## 5. Windows 安装实现
- [x] 5.1 实现 `GetCurrentDriveLetter()` 方法 - 获取当前盘符
- [x] 5.2 实现 `IsDirectoryClean()` 方法 - 检查目录是否只包含 ccm.exe 和可选的 settings.json
- [x] 5.3 实现 `DetectWindowsInstallStatus()` 方法 - 检测 Windows 安装状态和方式
- [x] 5.4 实现 `DetermineConfigDirectory()` 方法 - 确定配置文件目录（当前目录或 .ccm 目录）
- [x] 5.5 实现 `IsDirectoryInPath()` 方法 - 检查目录是否在 PATH 中
- [x] 5.6 实现 `AddToUserPath()` 方法 - 添加目录到用户 PATH
- [x] 5.7 实现 `RemoveFromUserPath()` 方法 - 从用户 PATH 移除目录
- [x] 5.8 实现 `CopyExecutableToCcmDir()` 方法 - 复制可执行文件到 %USERPROFILE%\.ccm\（检查目标是否存在）
- [x] 5.9 实现 `WindowsInstall()` 方法 - Windows 智能安装主流程（判断 C 盘/非 C 盘）
- [x] 5.10 实现 `WindowsUninstall()` 方法 - Windows 卸载主流程（根据安装方式卸载）

## 6. Unix 安装实现
- [x] 6.1 实现 `IsLocalBinInPath()` 方法 - 检查 ~/.local/bin 是否在 PATH
- [x] 6.2 实现 `IsUsrLocalBinInPath()` 方法 - 检查 /usr/local/bin 是否在 PATH
- [x] 6.3 实现 `DetectUnixInstallStatus()` 方法 - 检测 Unix 安装状态和位置
- [x] 6.4 实现 `CreateSymbolicLink()` 方法 - 创建符号链接（检查目标是否存在并询问）
- [x] 6.5 实现 `RemoveSymbolicLink()` 方法 - 删除符号链接
- [x] 6.6 实现 `CheckSymlinkPermission()` 方法 - 检查是否有创建符号链接的权限
- [x] 6.7 实现 `UnixInstall()` 方法 - Unix 智能安装主流程（判断 PATH 中的目录）
- [x] 6.8 实现 `UnixUninstall()` 方法 - Unix 卸载主流程（根据安装位置卸载）

## 7. InstallService 公共接口
- [x] 7.1 实现 `DetectInstallStatus()` 方法 - 统一安装状态检测接口
- [x] 7.2 实现 `Install()` 方法 - 统一安装接口
- [x] 7.3 实现 `Uninstall()` 方法 - 统一卸载接口
- [x] 7.4 添加用户友好的错误消息处理

## 8. Program.cs 修改
- [x] 8.1 添加无参数时的处理逻辑
- [x] 8.2 调用 InitService.RunInitializeWizard() 处理首次运行
- [x] 8.3 确保异常处理覆盖初始化流程

## 9. uninstall 命令实现
- [x] 9.1 在 `CommandBuilder.cs` 中创建 `CreateUninstallCommand()` 方法
- [x] 9.2 添加 `--remove-config` 选项
- [x] 9.3 实现卸载命令的处理逻辑
- [x] 9.4 在 `Program.cs` 中注册 uninstall 命令

## 10. 用户交互和输出
- [x] 10.1 设计并实现欢迎信息输出
- [x] 10.2 设计并实现操作列表显示
- [x] 10.3 设计并实现确认提示
- [x] 10.4 设计并实现成功/失败消息输出
- [x] 10.5 设计并实现下一步操作建议

## 11. 错误处理
- [x] 11.1 处理配置文件创建失败的情况
- [x] 11.2 处理配置文件已存在时的用户确认
- [x] 11.3 处理 Windows C 盘目录创建失败的情况
- [x] 11.4 处理 Windows 文件复制失败的情况
- [x] 11.5 处理 Windows PATH 修改失败的情况
- [x] 11.6 处理 Windows 目标文件已存在时的用户确认
- [x] 11.7 处理 Unix ~/.local/bin 符号链接创建失败的情况
- [x] 11.8 处理 Unix /usr/local/bin 符号链接创建失败（权限不足）的情况
- [x] 11.9 处理 ~/.local/bin 不在 PATH 且无法访问 /usr/local/bin 的情况
- [x] 11.10 处理 Unix 目标符号链接已存在时的用户确认
- [x] 11.11 处理卸载时的权限问题

## 12. Windows 平台测试
- [ ] 12.1 测试首次运行初始化流程
- [ ] 12.2 测试配置文件创建到正确位置
- [ ] 12.3 测试非 C 盘且目录干净时的安装（添加当前目录到 PATH）
- [ ] 12.4 测试 C 盘时的安装（复制到 %USERPROFILE%\.ccm\，配置文件也放 .ccm）
- [ ] 12.5 测试非 C 盘但目录有其他文件时的安装（直接复制到 .ccm，无需提示）
- [ ] 12.6 测试配置文件已存在时的覆盖提示
- [ ] 12.7 测试目标文件已存在时的覆盖提示
- [ ] 12.8 测试 PATH 环境变量修改
- [ ] 12.9 测试重复运行时的行为
- [ ] 12.10 测试 uninstall 命令（PATH 方式和复制方式）
- [ ] 12.11 测试 `--remove-config` 选项
- [ ] 12.12 测试 AOT 编译是否正常工作

## 13. Unix 平台测试
- [ ] 13.1 测试首次运行初始化流程
- [ ] 13.2 测试配置文件创建到 `~/.config/ClaudeCodeApiConfigManager/`
- [ ] 13.3 测试配置文件已存在时的覆盖提示
- [ ] 13.4 测试 ~/.local/bin 在 PATH 时的安装
- [ ] 13.5 测试 ~/.local/bin 不在 PATH 时尝试 /usr/local/bin 的安装
- [ ] 13.6 测试符号链接已存在时的覆盖提示
- [ ] 13.7 测试权限不足时的错误处理
- [ ] 13.8 测试符号链接创建
- [ ] 13.9 测试重复运行时的行为
- [ ] 13.10 测试 uninstall 命令（~/.local/bin 和 /usr/local/bin）
- [ ] 13.11 测试符号链接删除
- [ ] 13.12 测试 AOT 编译是否正常工作

## 14. 集成测试
- [ ] 14.1 测试完整的安装 → 使用 → 卸载流程
- [ ] 14.2 测试配置文件预设配置的正确性
- [ ] 14.3 测试安装后使用 add、use、list 等命令
- [ ] 14.4 测试跨平台一致性（配置格式、命令行为）

## 15. 文档更新
- [ ] 15.1 更新 README.md 添加安装说明
- [ ] 15.2 更新命令参考文档添加 uninstall 命令
- [ ] 15.3 添加故障排除指南
