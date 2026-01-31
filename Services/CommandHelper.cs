using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// 命令辅助类
/// </summary>
public static class CommandHelper
{
    /// <summary>
    /// 解析 add 命令的参数
    /// </summary>
    public static ApiConfig ParseAddArguments(string name, string[] args)
    {
        string token = string.Empty;
        string baseUrl = string.Empty;
        string model = string.Empty;
        var customParams = new Dictionary<string, string>();

        // 自动识别 URL（基于 http:// 或 https:// 前缀）
        foreach (var arg in args)
        {
            if (arg.StartsWith(Constants.UrlPrefixes.Http, StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith(Constants.UrlPrefixes.Https, StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = arg;
                break;
            }
        }

        // 处理剩余参数（非 URL、非自定义参数的）
        var regularArgs = new List<string>();
        foreach (var arg in args)
        {
            if (arg.StartsWith(Constants.UrlPrefixes.Http, StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith(Constants.UrlPrefixes.Https, StringComparison.OrdinalIgnoreCase))
            {
                continue; // 跳过 URL
            }
            if (arg.Contains('='))
            {
                // 自定义参数 KEY=VALUE
                var parts = arg.Split('=', 2);
                if (parts.Length == 2)
                {
                    customParams[parts[0]] = parts[1];
                }
            }
            else
            {
                regularArgs.Add(arg);
            }
        }

        // regularArgs 应该包含 [TOKEN, MODEL] 或 [MODEL, TOKEN]
        // API Token 通常以 sk- 开头或较长，模型通常包含数字
        if (regularArgs.Count >= 2)
        {
            // 判断哪个是 token：通常以 sk- 开头，或者比另一个长
            var arg0 = regularArgs[0];
            var arg1 = regularArgs[1];

            bool arg0IsToken = arg0.StartsWith(Constants.ApiTokenPrefix) || arg0.Length > arg1.Length;
            bool arg1IsToken = arg1.StartsWith(Constants.ApiTokenPrefix) || arg1.Length > arg0.Length;

            if (arg0IsToken && !arg1IsToken)
            {
                token = arg0;
                model = arg1;
            }
            else if (arg1IsToken && !arg0IsToken)
            {
                token = arg1;
                model = arg0;
            }
            else
            {
                // 都像 token 或都不像，默认顺序
                token = arg0;
                model = arg1;
            }
        }
        else if (regularArgs.Count == 1)
        {
            // 只有一个参数，可能是 token 或 model
            // 如果它看起来像 token，就是 token，否则是 model
            if (regularArgs[0].StartsWith(Constants.ApiTokenPrefix))
            {
                token = regularArgs[0];
            }
            else
            {
                model = regularArgs[0];
            }
        }

        return new ApiConfig
        {
            Name = name,
            AuthToken = token,
            BaseUrl = baseUrl,
            Model = model,
            CustomParams = customParams
        };
    }

    /// <summary>
    /// 设置环境变量
    /// </summary>
    public static void SetEnvironmentVariables(ApiConfig config)
    {
        var variables = BuildEnvironmentVariables(config);

        if (Platform.IsWindows)
        {
            WindowsEnvironmentManager.SetEnvironmentVariables(variables);
        }
        else if (Platform.IsUnix)
        {
            UnixEnvironmentManager.SetEnvironmentVariables(variables);
        }
        else
        {
            throw new PlatformNotSupportedException("当前平台不支持");
        }
    }

    /// <summary>
    /// 从配置构建环境变量字典
    /// </summary>
    public static Dictionary<string, string> BuildEnvironmentVariables(ApiConfig config)
    {
        var variables = new Dictionary<string, string>
        {
            [Constants.EnvVars.AuthToken] = config.AuthToken,
            [Constants.EnvVars.BaseUrl] = config.BaseUrl,
            [Constants.EnvVars.Model] = config.Model
        };

        // 添加自定义参数
        foreach (var param in config.CustomParams)
        {
            variables[param.Key] = param.Value;
        }

        return variables;
    }
}
