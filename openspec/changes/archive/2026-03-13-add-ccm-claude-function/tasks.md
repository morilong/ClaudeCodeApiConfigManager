## 1. Shell 函数模板

- [x] 1.1 添加 PowerShell `ccm-claude`/`ccm-c` 函数模板到 `ShellFunctionInjector.cs`
- [x] 1.2 添加 Bash/Zsh `ccm-claude`/`ccm-c` 函数模板
- [x] 1.3 添加 Fish `ccm-claude`/`ccm-c` 函数模板
- [x] 1.4 添加 Git Bash `ccm-claude`/`ccm-c` 函数模板（复用 Bash 模板）

## 2. 注入逻辑

- [x] 2.1 修改 `Inject()` 方法，在注入 `ccm` 函数时同时注入 `ccm-claude`/`ccm-c`（使用相同 `<ccm-init>` 标记，无需修改）
- [x] 2.2 修改 `Remove()` 方法，卸载时移除 `ccm-claude`/`ccm-c` 函数（使用相同 `<ccm-init>` 标记，无需修改）

## 3. 测试验证

- [x] 3.1 测试 `ccm-claude <config>` 正常启动 Claude
- [x] 3.2 测试 `ccm-claude <config> -y` 启动 Claude 并跳过权限
- [x] 3.3 测试 `ccm-c` 别名工作正常
- [x] 3.4 测试卸载时正确移除函数
- [x] 3.5 测试各 Shell（PowerShell、Bash、Zsh、Fish、Git Bash）
