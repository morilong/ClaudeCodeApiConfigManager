## Context

当前 ccm 已通过 `ShellFunctionInjector` 在安装时注入 Shell wrapper 函数，实现 `ccm use` 的自动 eval。用户需要进一步简化工作流，将配置切换和 Claude 启动合并为一条命令。

## Goals / Non-Goals

**Goals:**
- 新增 `ccm-claude` 和 `ccm-c` Shell 函数，一键切换配置并启动 Claude
- 支持 `-y` 参数映射到 `--dangerously-skip-permissions`
- 复用现有的 `ShellFunctionInjector` 机制，最小化代码改动

**Non-Goals:**
- 不支持执行其他命令（如 `ccm-claude zhipu -- node script.js`）
- 不修改 ccm CLI 核心逻辑

## Decisions

### 1. 函数命名

**决定**: 使用 `ccm-claude` 作为主命令，`ccm-c` 作为别名

**理由**:
- `ccm-claude` 语义明确，一看就知道是启动 Claude
- `ccm-c` 简短，适合高频使用
- 遵循 ccm 命名前缀约定

### 2. `-y` 参数设计

**决定**: `-y` 作为函数参数，映射到 `--dangerously-skip-permissions`

**理由**:
- `-y` 是业界惯例（apt、yum、choco 都用 `-y` 表示 yes/auto）
- 用户无需记忆长参数名
- 只需一个函数，通过参数区分模式

**替代方案**:
- `--skip`: 更明确但较长
- `-d`: 太短，容易混淆

### 3. 实现位置

**决定**: 在 `ShellFunctionInjector.cs` 中添加新函数模板，与现有 `ccm` 函数并存

**理由**:
- 复用现有注入机制
- 代码改动最小
- 使用相同 marker `# <ccm-init>` 管理

### 4. 函数实现策略

**决定**: 函数内部调用 `ccm use` + `claude`，而非重新实现环境变量设置

**理由**:
- 避免重复代码
- 自动继承 `ccm use` 的所有改进
- 保持单一职责

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| 函数名与用户现有别名冲突 | 使用 ccm 前缀，降低冲突概率 |
| `-y` 参数位置敏感（必须在配置名之后） | 文档说明，函数内做参数解析 |
| 用户可能期望支持更多命令 | 明确 Non-Goals，未来可扩展为 `ccm exec` |
