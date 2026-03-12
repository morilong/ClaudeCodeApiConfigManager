## Context

当前 `ccm use xxx` 的实现：
- **Windows**: 调用 `Environment.SetEnvironmentVariable` 设置用户级永久环境变量
- **Unix**: 生成 `~/.ccm/env.sh` 脚本，通过 source 方式加载

问题：
1. 子进程无法修改父进程（Shell）的环境变量
2. 用户需要重启终端或手动 source 才能生效
3. Windows 平台有多种 Shell（PowerShell、Git Bash、CMD），处理方式不同

## Goals / Non-Goals

**Goals:**
- `ccm use xxx` 默认临时生效，环境变量立即在当前终端生效
- `ccm use xxx -p` 永久生效，当前终端 + 新终端都生效
- 支持 PowerShell、Git Bash、Bash、Zsh、Fish 自动生效
- CMD 用户可复制粘贴单条命令执行
- 安装时自动注入 Shell 函数，用户无感知

**Non-Goals:**
- 不支持 CMD 自动生效（技术限制）
- 不支持 Windows CMD 的 doskey 宏方案（过于复杂）

## Decisions

### 1. 默认行为改为临时生效

**决定**: `ccm use xxx` 默认临时生效，`-p`/`--persist` 永久生效

**理由**:
- 用户最常用的场景是切换配置后立即使用
- 永久生效是特殊需求，应该显式指定
- 与 nvm、conda 等工具的行为一致

**替代方案**:
- 保持默认永久，添加 `--temp` 参数 → 用户需要多打字，体验差

### 2. Shell 函数包装 + eval 模式

**决定**: 安装时注入 Shell 函数，函数内部使用 eval 执行 ccm 输出的命令

**理由**:
- 外部程序无法直接修改父进程环境变量
- eval 模式是业界标准做法（nvm、conda、direnv 都用此方案）
- 用户体验最好，无感知

**实现**:
```
用户执行: ccm use xxx
Shell 函数拦截 → 调用 ccm.exe use xxx --temp → eval 输出命令 → 环境变量生效
```

### 3. Shell 检测策略

**决定**: 按优先级检测环境变量

| 优先级 | Shell | 检测条件 |
|--------|-------|----------|
| 1 | Git Bash | `MSYSTEM` 环境变量存在 |
| 2 | Fish | `$SHELL` 包含 "fish" |
| 3 | Zsh | `$SHELL` 包含 "zsh" |
| 4 | Bash | `$SHELL` 包含 "bash" |
| 5 | PowerShell | Windows + `PSModulePath` 存在 |
| 6 | CMD | Windows + 以上都不满足 |

**理由**:
- Git Bash 在 Windows 上运行但使用 Bash 语法，需要优先检测
- PowerShell 和 CMD 只在 Windows 上检测

### 4. CMD 输出格式

**决定**: 输出单条 `set && set && set` 格式命令

**理由**:
- 方便用户一次性复制粘贴执行
- 不需要生成临时文件
- 避免复杂的 for /f 解析

**示例**:
```
set ANTHROPIC_BASE_URL=https://xxx && set ANTHROPIC_API_KEY=sk-xxx && set ANTHROPIC_MODEL=xxx
```

### 5. 永久模式双重输出

**决定**: `--persist` 模式同时设置永久环境变量 + 输出临时命令

**理由**:
- 确保当前终端立即生效
- 新终端通过永久环境变量自动生效
- 用户体验一致

## Risks / Trade-offs

| 风险 | 缓解措施 |
|------|----------|
| Shell 函数注入失败 | 检测失败时提示用户手动添加 |
| 环境变量值包含特殊字符 | 对值进行转义处理（引号、$、\等） |
| 用户已有同名 ccm 函数 | 使用 marker 注释，检测已存在则跳过 |
| CMD 用户需要手动复制 | 清晰提示，建议使用 PowerShell/Git Bash |

## Open Questions

- [ ] 是否需要支持 `--shell=xxx` 手动指定 Shell 类型？（暂不实现，按需添加）
