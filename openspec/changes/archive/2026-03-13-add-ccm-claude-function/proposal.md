## Why

用户每次使用 Claude Code 时需要两步操作：先执行 `ccm use <config>` 切换配置，再执行 `claude` 启动 Claude。这个工作流可以简化为一个命令，提升使用效率。

## What Changes

- 新增 Shell 函数 `ccm-claude`（别名 `ccm-c`），自动完成配置切换 + 启动 Claude
- 支持 `-y` 参数，映射到 `claude --dangerously-skip-permissions`
- 在安装时自动注入到 Shell 配置文件（复用现有 ShellFunctionInjector 机制）

**使用示例**：
```bash
ccm-claude zhipu           # 等价于: ccm use zhipu && claude
ccm-claude zhipu -y        # 等价于: ccm use zhipu && claude --dangerously-skip-permissions
ccm-c zhipu                # ccm-claude 的简写
ccm-c zhipu -y             # 带 -y 参数
```

## Capabilities

### New Capabilities

- `claude-launcher`: 提供快捷命令 `ccm-claude`/`ccm-c`，一键切换配置并启动 Claude Code

### Modified Capabilities

- `shell-function-injection`: 扩展现有注入机制，新增 `ccm-claude`/`ccm-c` 函数的注入

## Impact

- **新增代码**: 扩展 `ShellFunctionInjector.cs`，添加新函数模板
- **Shell 配置**: 用户终端配置文件将新增函数定义
- **向后兼容**: 完全兼容，不影响现有功能
