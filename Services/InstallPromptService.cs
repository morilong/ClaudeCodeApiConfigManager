using Spectre.Console;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 安装选项模型
/// </summary>
public class InstallOption
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Directory { get; set; }

    public override string ToString() => Name;
}

/// <summary>
/// 安装选择提示服务
/// </summary>
public static class InstallPromptService
{
    /// <summary>
    /// 获取可用的安装选项列表
    /// </summary>
    public static List<InstallOption> GetInstallOptions(InstallPlan defaultPlan)
    {
        var options = new List<InstallOption>();

        if (Platform.IsWindows)
        {
            var currentDir = Directory.GetCurrentDirectory().TrimEnd('\\', '/');
            var ccmDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Constants.Install.WinCcmDir);

            // 当前目录选项（如果干净）
            if (InstallService.IsDirectoryClean(currentDir))
            {
                options.Add(new InstallOption
                {
                    Name = "当前目录",
                    Description = $"将 {currentDir} 添加到 PATH",
                    Directory = currentDir
                });
            }

            // 用户目录 .ccm 选项
            options.Add(new InstallOption
            {
                Name = "用户目录",
                Description = $"复制到 {ccmDir} 并添加到 PATH",
                Directory = ccmDir
            });

            // 自定义路径选项
            options.Add(new InstallOption
            {
                Name = "自定义路径",
                Description = "手动输入安装目录",
                Directory = ""
            });
        }
        else // Unix
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var localBinPath = Path.Combine(homeDir, Constants.Install.LocalBinDir);
            var usrLocalBinPath = Constants.Install.UsrLocalBinDir;

            // ~/.local/bin 选项
            options.Add(new InstallOption
            {
                Name = "用户目录 (~/.local/bin)",
                Description = $"创建符号链接到 {localBinPath}/ccm",
                Directory = localBinPath
            });

            // /usr/local/bin 选项
            options.Add(new InstallOption
            {
                Name = "系统目录 (/usr/local/bin)",
                Description = "创建符号链接到 /usr/local/bin/ccm (需要 sudo)",
                Directory = usrLocalBinPath
            });

            // 自定义路径选项
            options.Add(new InstallOption
            {
                Name = "自定义路径",
                Description = "手动输入安装目录",
                Directory = ""
            });
        }

        return options;
    }

    /// <summary>
    /// 显示安装目录选择提示
    /// </summary>
    public static InstallOption? PromptInstallDirectory(InstallPlan defaultPlan)
    {
        var options = GetInstallOptions(defaultPlan);

        // 找到默认选中的选项（与检测到的安装计划匹配的）
        int defaultIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            // 比较目录路径（标准化后比较）
            var normalizedOption = options[i].Directory.TrimEnd('\\', '/');
            var normalizedDefault = defaultPlan.InstallDirectory.TrimEnd('\\', '/');

            if (!string.IsNullOrEmpty(normalizedOption) &&
                normalizedOption.Equals(normalizedDefault, StringComparison.OrdinalIgnoreCase))
            {
                defaultIndex = i;
                break;
            }
        }

        // 使用 Spectre.Console 创建选择提示
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[blue]选择安装目录[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<InstallOption>()
                .Title("[blue]请选择安装目录：[/]")
                .PageSize(10)
                .AddChoices(options)
                .UseConverter(o => $"{o.Name} [dim]({o.Directory})[/]")
                .HighlightStyle(new Style().Foreground(Color.Green).Background(Color.Grey23))
        );

        // 如果选择自定义路径，提示用户输入
        if (selected.Name == "自定义路径")
        {
            AnsiConsole.WriteLine();
            var customPath = AnsiConsole.Ask<string>("[green]请输入安装目录路径：[/]");

            if (string.IsNullOrWhiteSpace(customPath))
            {
                AnsiConsole.MarkupLine("[red]路径不能为空，将使用默认选项。[/]");
                return options[defaultIndex];
            }

            // 验证路径
            try
            {
                // 规范化路径
                customPath = Path.GetFullPath(customPath);

                // 确保目录存在
                if (!Directory.Exists(customPath))
                {
                    var createDir = AnsiConsole.Confirm($"目录 {customPath} 不存在，是否创建？", true);
                    if (createDir)
                    {
                        Directory.CreateDirectory(customPath);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]已取消，将使用默认选项。[/]");
                        return options[defaultIndex];
                    }
                }

                selected.Directory = customPath;
                selected.Description = $"自定义安装到 {customPath}";
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]无效的路径: {ex.Message}[/]");
                AnsiConsole.MarkupLine("[yellow]将使用默认选项。[/]");
                return options[defaultIndex];
            }
        }

        return selected;
    }

    /// <summary>
    /// 显示安装计划摘要并确认
    /// </summary>
    public static bool ConfirmInstallPlan(InstallOption selectedOption, InstallPlan originalPlan)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[blue]安装计划[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);
        table.AddColumn("[bold]项目[/]");
        table.AddColumn("[bold]详情[/]");

        table.AddRow("安装目录", selectedOption.Directory);
        table.AddRow("配置目录", originalPlan.ConfigDirectory);
        table.AddRow("安装方式", selectedOption.Description);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        return AnsiConsole.Confirm("[green]是否继续安装？[/]", true);
    }
}
