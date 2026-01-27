# Implementation Tasks

## 1. 项目基础设置
- [x] 1.1 添加 NuGet 包依赖（System.CommandLine、System.Text.Json）
- [x] 1.2 添加 Windows 平台 NuGet 包依赖（Microsoft.Win32.Registry）
- [x] 1.3 创建项目基础目录结构（Models/、Services/、Commands/）
- [x] 1.4 更新 .csproj 文件添加平台特定编译符号和条件引用
- [x] 1.5 配置跨平台 AOT 编译设置

## 2. 数据模型定义
- [x] 2.1 创建 `ApiConfig` 模型类（包含名称、Token、BaseUrl、Model、自定义参数）
- [x] 2.2 创建 `SettingsConfig` 模型类（包含配置列表和当前活动配置标识）
- [x] 2.3 添加 JSON 序列化配置和转换器（JsonSerializerContext for AOT）

## 3. 平台检测
- [x] 3.1 实现 `Platform` 静态类 - 检测当前操作系统
- [x] 3.2 实现 `ConfigDirectory` 静态类 - 获取平台特定的配置目录路径
- [ ] 3.3 添加平台检测单元测试（跳过，保持简单）

## 4. 配置管理服务
- [x] 4.1 实现 `ConfigManager` 类 - 负责配置文件的读写
- [x] 4.2 实现配置添加逻辑（包括名称冲突检测和用户确认）
- [x] 4.3 实现配置删除逻辑
- [x] 4.4 实现配置列表展示逻辑（格式：名称（模型））
- [x] 4.5 实现当前配置获取逻辑
- [x] 4.6 实现配置文件损坏时的错误处理

## 5. 环境变量管理服务 - 接口和通用实现
- [x] 5.1 定义 `IEnvironmentManager` 接口（已改为使用静态类）
- [x] 5.2 实现读取当前环境变量的通用逻辑（已简化）
- [x] 5.3 实现环境变量字典构建逻辑（CommandHelper）

## 6. 环境变量管理服务 - Windows 平台
- [x] 6.1 实现 `WindowsEnvironmentManager` 类（条件编译 #if WINDOWS）
- [x] 6.2 实现通过 Windows 注册表修改用户环境变量的逻辑
- [x] 6.3 添加环境变量修改后的通知机制（发送 WM_SETTINGCHANGE 消息）
- [x] 6.4 测试 Windows 平台环境变量修改功能（已通过基本测试）

## 7. 环境变量管理服务 - Unix 平台（Linux/macOS）
- [x] 7.1 实现 `UnixEnvironmentManager` 类（条件编译 #if UNIX）
- [x] 7.2 实现用户 shell 检测逻辑（bash、zsh、fish）
- [x] 7.3 实现环境变量脚本生成（~/.ccm/env.sh、env.fish）
- [x] 7.4 实现在 shell 配置文件中添加初始化代码的逻辑
- [x] 7.5 实现初始化代码存在性检测
- [x] 7.6 添加首次配置时的用户引导提示
- [ ] 7.7 测试 Linux/macOS 平台环境变量修改功能（需要 Unix 环境）

## 8. API 测试服务
- [x] 8.1 实现 `ApiTester` 类 - 负责测试 API 连接
- [x] 8.2 实现简单的 API 健康检查（如调用 /models 或简单的 API 请求）
- [x] 8.3 添加友好的测试结果输出（成功/失败、响应时间）

## 9. CLI 命令实现
- [x] 9.1 创建 `AddCommand` - 实现 add 命令（支持参数自动识别）
- [x] 9.2 创建 `ListCommand` - 实现 list/ls 命令
- [x] 9.3 创建 `UseCommand` - 实现 use 命令
- [x] 9.4 创建 `CurrentCommand` - 实现 current/c 命令
- [x] 9.5 创建 `RemoveCommand` - 实现 remove/del 命令
- [x] 9.6 创建 `TestCommand` - 实现 test 命令

## 10. 主程序集成
- [x] 10.1 重写 `Program.cs` - 设置命令行根命令和子命令
- [x] 10.2 添加全局异常处理和用户友好的错误消息
- [x] 10.3 实现命令帮助文档
- [x] 10.4 添加平台特定的使用提示

## 11. Windows 平台验证和测试
- [x] 11.1 手动测试 add 命令（包括参数位置调换和自定义参数）
- [x] 11.2 手动测试 list/ls 命令
- [x] 11.3 手动测试 use 命令（验证注册表环境变量是否正确修改）
- [x] 11.4 手动测试 current/c 命令
- [x] 11.5 手动测试 remove/del 命令
- [ ] 11.6 手动测试 test 命令（需要有效的 API 凭证）
- [ ] 11.7 测试 Windows AOT 编译是否正常工作
- [x] 11.8 测试配置文件在不同场景下的正确性（空配置、单配置、多配置）

## 12. Linux/macOS 平台验证和测试
- [ ] 12.1 在 Linux 环境手动测试 add 命令
- [ ] 12.2 在 Linux 环境手动测试 use 命令（验证 shell 脚本生成）
- [ ] 12.3 在 Linux 环境测试 shell 初始化代码自动添加
- [ ] 12.4 在 macOS 环境重复上述测试
- [ ] 12.5 测试不同 shell（bash、zsh、fish）的兼容性
- [ ] 12.6 测试 Linux/macOS AOT 编译是否正常工作
