namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// Shell 函数注入器
/// 在安装时自动注入 Shell wrapper 函数，实现 ccm use 的自动 eval
/// </summary>
public static class ShellFunctionInjector
{
    private static readonly IConsoleOutput Output = new ConsoleOutput();
    private static readonly string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    #region Shell 函数模板

    /// <summary>
    /// PowerShell 函数模板
    /// </summary>
    private const string PowerShellFunctionTemplate = """
# <ccm-init>
function ccm {
    $cmd = $args[0]
    if ($cmd -eq "use") {
        if ($args -contains "--help" -or $args -contains "-h") {
            & ccm.exe $args
        } elseif ($args -contains "--persist" -or $args -contains "-p") {
            & ccm.exe $args | Invoke-Expression
        } else {
            & ccm.exe $args --temp | Invoke-Expression
        }
    } else {
        & ccm.exe $args
    }
}

function ccm-claude {
    $skipPerms = $false
    $configName = $null
    $claudeArgs = @()

    foreach ($arg in $args) {
        if ($arg -eq "-y") {
            $skipPerms = $true
        } elseif ($null -eq $configName) {
            $configName = $arg
        } else {
            $claudeArgs += $arg
        }
    }

    if ($null -eq $configName) {
        Write-Host "Usage: ccm-claude <config-name> [-y] [claude-args...]" -ForegroundColor Red
        return
    }

    & ccm.exe use $configName --temp | Invoke-Expression
    if ($skipPerms) {
        & claude --dangerously-skip-permissions @claudeArgs
    } else {
        & claude @claudeArgs
    }
}

Set-Alias -Name ccm-c -Value ccm-claude
# </ccm-init>
""";

    /// <summary>
    /// Bash/Zsh 函数模板
    /// </summary>
    private static string BashFunctionTemplate = """
# <ccm-init>
ccm() {
    local cmd="$1"
    if [[ "$cmd" == "use" ]]; then
        if [[ "$*" == *"--help"* || "$*" == *"-h"* ]]; then
            command ccm "$@"
        elif [[ "$*" == *"--persist"* || "$*" == *"-p"* ]]; then
            eval "$(command ccm "$@")"
        else
            eval "$(command ccm "$@" --temp)"
        fi
    else
        command ccm "$@"
    fi
}

ccm-claude() {
    local skip_perms=false
    local config_name=""
    local claude_args=()

    while [[ $# -gt 0 ]]; do
        case "$1" in
            -y) skip_perms=true; shift ;;
            *)
                if [[ -z "$config_name" ]]; then
                    config_name="$1"
                else
                    claude_args+=("$1")
                fi
                shift ;;
        esac
    done

    if [[ -z "$config_name" ]]; then
        echo "Usage: ccm-claude <config-name> [-y] [claude-args...]" >&2
        return 1
    fi

    eval "$(command ccm use "$config_name" --temp)"
    if [[ "$skip_perms" == true ]]; then
        claude --dangerously-skip-permissions "${claude_args[@]}"
    else
        claude "${claude_args[@]}"
    fi
}

alias ccm-c='ccm-claude'
# </ccm-init>
""".Replace("\r\n", "\n");

    /// <summary>
    /// Fish 函数模板
    /// </summary>
    private static string FishFunctionTemplate = """
# <ccm-init>
function ccm
    set -l cmd $argv[1]
    if test "$cmd" = "use"
        if string match -qr -- '--help|-h' $argv
            command ccm $argv
        else if string match -q -- '*--persist*' $argv; or string match -q -- '*-p*' $argv
            eval (command ccm $argv)
        else
            eval (command ccm $argv --temp)
        end
    else
        command ccm $argv
    end
end

function ccm-claude
    set -l skip_perms false
    set -l config_name ""
    set -l claude_args

    for arg in $argv
        if test "$arg" = "-y"
            set skip_perms true
        else if test -z "$config_name"
            set config_name $arg
        else
            set -a claude_args $arg
        end
    end

    if test -z "$config_name"
        echo "Usage: ccm-claude <config-name> [-y] [claude-args...]" >&2
        return 1
    end

    eval (command ccm use $config_name --temp)
    if test "$skip_perms" = true
        claude --dangerously-skip-permissions $claude_args
    else
        claude $claude_args
    end
end

function ccm-c
    ccm-claude $argv
end
# </ccm-init>
""".Replace("\r\n", "\n");

    #endregion

    #region 注入方法

    /// <summary>
    /// 为指定 Shell 类型注入函数
    /// </summary>
    public static void Inject(ShellType shellType)
    {
        switch (shellType)
        {
            case ShellType.PowerShell:
                InjectPowerShell();
                break;
            case ShellType.GitBash:
                InjectGitBash();
                break;
            case ShellType.Bash:
                InjectBash();
                break;
            case ShellType.Zsh:
                InjectZsh();
                break;
            case ShellType.Fish:
                InjectFish();
                break;
            // CMD 不支持函数注入
            case ShellType.Cmd:
            default:
                break;
        }
    }

    /// <summary>
    /// 注入 PowerShell 函数
    /// 同时检查 Windows PowerShell 和 PowerShell Core 的配置文件
    /// </summary>
    private static void InjectPowerShell()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var injected = false;

        // Windows PowerShell 5.1 路径
        var winPsProfile = Path.Combine(documentsPath, "WindowsPowerShell", "Microsoft.PowerShell_profile.ps1");
        if (File.Exists(winPsProfile))
        {
            InjectToConfigFile(winPsProfile, PowerShellFunctionTemplate, "Windows PowerShell $PROFILE");
            injected = true;
        }

        // PowerShell Core (6+) 路径
        var psCoreProfile = Path.Combine(documentsPath, "PowerShell", "Microsoft.PowerShell_profile.ps1");
        if (File.Exists(psCoreProfile))
        {
            InjectToConfigFile(psCoreProfile, PowerShellFunctionTemplate, "PowerShell Core $PROFILE");
            injected = true;
        }

        // 如果都不存在，创建 PowerShell Core 配置文件
        if (!injected)
        {
            InjectToConfigFile(psCoreProfile, PowerShellFunctionTemplate, "PowerShell $PROFILE");
        }
    }

    /// <summary>
    /// 注入 Git Bash 函数
    /// </summary>
    private static void InjectGitBash()
    {
        // Git Bash 使用 Windows 用户主目录下的 .bashrc
        var bashrcPath = Path.Combine(HomeDir, Constants.Files.Bashrc);
        InjectToConfigFile(bashrcPath, BashFunctionTemplate, "Git Bash .bashrc");
    }

    /// <summary>
    /// 注入 Bash 函数 (Unix)
    /// </summary>
    private static void InjectBash()
    {
        var bashrcPath = Path.Combine(HomeDir, Constants.Files.Bashrc);
        var bashProfilePath = Path.Combine(HomeDir, Constants.Files.BashProfile);

        // 优先使用 .bashrc，不存在则使用 .bash_profile
        var targetPath = File.Exists(bashrcPath) ? bashrcPath : bashProfilePath;
        InjectToConfigFile(targetPath, BashFunctionTemplate, ".bashrc");
    }

    /// <summary>
    /// 注入 Zsh 函数
    /// </summary>
    private static void InjectZsh()
    {
        var zshrcPath = Path.Combine(HomeDir, Constants.Files.Zshrc);
        InjectToConfigFile(zshrcPath, BashFunctionTemplate, ".zshrc");
    }

    /// <summary>
    /// 注入 Fish 函数
    /// </summary>
    private static void InjectFish()
    {
        var fishConfigPath = Path.Combine(HomeDir, ".config", Constants.Dirs.FishConfigDir, Constants.Files.FishConfig);
        InjectToConfigFile(fishConfigPath, FishFunctionTemplate, "Fish config.fish");
    }

    /// <summary>
    /// 通用配置文件注入方法
    /// </summary>
    private static void InjectToConfigFile(string configPath, string functionContent, string configName)
    {
        // 确保目录存在
        var dir = Path.GetDirectoryName(configPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // 检查是否已包含注入内容
        if (File.Exists(configPath))
        {
            var content = File.ReadAllText(configPath);
            if (content.Contains("<ccm-init>"))
            {
                Output.WriteLine();
                Output.WriteLine($"{configName} 已包含 ccm 函数，跳过注入。");
                return;
            }
        }

        // 追加函数内容
        using var writer = File.AppendText(configPath);
        writer.WriteLine();
        writer.WriteLine(functionContent);

        Output.WriteLine();
        Output.Success($"已注入 ccm 函数到 {configName}");
    }

    #endregion

    #region 移除方法

    /// <summary>
    /// 从指定 Shell 类型移除函数
    /// </summary>
    public static void Remove(ShellType shellType)
    {
        switch (shellType)
        {
            case ShellType.PowerShell:
                RemoveFromPowerShell();
                break;
            case ShellType.GitBash:
                RemoveFromGitBash();
                break;
            case ShellType.Bash:
                RemoveFromBash();
                break;
            case ShellType.Zsh:
                RemoveFromZsh();
                break;
            case ShellType.Fish:
                RemoveFromFish();
                break;
            case ShellType.Cmd:
            default:
                break;
        }
    }

    /// <summary>
    /// 从 PowerShell 移除函数
    /// 同时检查 Windows PowerShell 和 PowerShell Core 的配置文件
    /// </summary>
    private static void RemoveFromPowerShell()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Windows PowerShell 5.1 路径
        var winPsProfile = Path.Combine(documentsPath, "WindowsPowerShell", "Microsoft.PowerShell_profile.ps1");
        if (File.Exists(winPsProfile))
        {
            RemoveFromConfigFile(winPsProfile, "Windows PowerShell $PROFILE");
        }

        // PowerShell Core (6+) 路径
        var psCoreProfile = Path.Combine(documentsPath, "PowerShell", "Microsoft.PowerShell_profile.ps1");
        if (File.Exists(psCoreProfile))
        {
            RemoveFromConfigFile(psCoreProfile, "PowerShell Core $PROFILE");
        }
    }

    /// <summary>
    /// 从 Git Bash 移除函数
    /// </summary>
    private static void RemoveFromGitBash()
    {
        var bashrcPath = Path.Combine(HomeDir, Constants.Files.Bashrc);
        RemoveFromConfigFile(bashrcPath, "Git Bash .bashrc");
    }

    /// <summary>
    /// 从 Bash 移除函数
    /// </summary>
    private static void RemoveFromBash()
    {
        var bashrcPath = Path.Combine(HomeDir, Constants.Files.Bashrc);
        var bashProfilePath = Path.Combine(HomeDir, Constants.Files.BashProfile);

        if (File.Exists(bashrcPath))
        {
            RemoveFromConfigFile(bashrcPath, ".bashrc");
        }
        if (File.Exists(bashProfilePath))
        {
            RemoveFromConfigFile(bashProfilePath, ".bash_profile");
        }
    }

    /// <summary>
    /// 从 Zsh 移除函数
    /// </summary>
    private static void RemoveFromZsh()
    {
        var zshrcPath = Path.Combine(HomeDir, Constants.Files.Zshrc);
        RemoveFromConfigFile(zshrcPath, ".zshrc");
    }

    /// <summary>
    /// 从 Fish 移除函数
    /// </summary>
    private static void RemoveFromFish()
    {
        var fishConfigPath = Path.Combine(HomeDir, ".config", Constants.Dirs.FishConfigDir, Constants.Files.FishConfig);
        RemoveFromConfigFile(fishConfigPath, "Fish config.fish");
    }

    /// <summary>
    /// 通用配置文件移除方法
    /// </summary>
    private static void RemoveFromConfigFile(string configPath, string configName)
    {
        if (!File.Exists(configPath))
        {
            return;
        }

        var content = File.ReadAllText(configPath);
        if (!content.Contains("<ccm-init>"))
        {
            return;
        }

        // 移除 <ccm-init> 到 </ccm-init> 之间的内容（包括标记）
        var lines = content.Split(Environment.NewLine);
        var newLines = new List<string>();
        var inCcmBlock = false;

        foreach (var line in lines)
        {
            if (line.Contains("<ccm-init>"))
            {
                inCcmBlock = true;
                continue;
            }
            if (line.Contains("</ccm-init>"))
            {
                inCcmBlock = false;
                continue;
            }
            if (!inCcmBlock)
            {
                newLines.Add(line);
            }
        }

        // 移除末尾的空行
        while (newLines.Count > 0 && string.IsNullOrWhiteSpace(newLines[^1]))
        {
            newLines.RemoveAt(newLines.Count - 1);
        }

        File.WriteAllText(configPath, string.Join(Environment.NewLine, newLines) + Environment.NewLine);
        Output.Success($"已从 {configName} 移除 ccm 函数");
    }

    #endregion

    #region 检测方法

    /// <summary>
    /// 检测所有可用的 Shell 并返回列表
    /// </summary>
    public static List<ShellType> DetectAvailableShells()
    {
        var shells = new List<ShellType>();

        if (Platform.IsWindows)
        {
            // Windows: 检测 PowerShell、Git Bash
            if (IsPowerShellAvailable())
            {
                shells.Add(ShellType.PowerShell);
            }
            if (IsGitBashAvailable())
            {
                shells.Add(ShellType.GitBash);
            }
        }
        else
        {
            // Unix: 检测 Bash、Zsh、Fish
            var shell = Environment.GetEnvironmentVariable("SHELL") ?? "";
            if (shell.Contains("fish"))
            {
                shells.Add(ShellType.Fish);
            }
            else if (shell.Contains("zsh"))
            {
                shells.Add(ShellType.Zsh);
            }
            else
            {
                shells.Add(ShellType.Bash);
            }
        }

        return shells;
    }

    /// <summary>
    /// 检测 PowerShell 是否可用
    /// </summary>
    private static bool IsPowerShellAvailable()
    {
        var psModulePath = Environment.GetEnvironmentVariable("PSModulePath");
        return !string.IsNullOrEmpty(psModulePath);
    }

    /// <summary>
    /// 检测 Git Bash 是否可用
    /// 在 Windows 上，始终返回 true，因为我们会在注入时创建 .bashrc（如果不存在）
    /// </summary>
    private static bool IsGitBashAvailable()
    {
        // Windows 上始终尝试注入 Git Bash 函数
        // 如果 .bashrc 不存在，InjectToConfigFile 会自动创建
        return Platform.IsWindows;
    }

    #endregion
}
