using System.Text.Json.Serialization;

namespace ClaudeCodeApiConfigManager.Models;

/// <summary>
/// API 配置项
/// </summary>
public class ApiConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("authToken")]
    public string AuthToken { get; set; } = string.Empty;

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("customParams")]
    public Dictionary<string, string> CustomParams { get; set; } = new();
}

/// <summary>
/// 配置文件结构
/// </summary>
public class SettingsConfig
{
    [JsonPropertyName("configs")]
    public List<ApiConfig> Configs { get; set; } = new();

    [JsonPropertyName("activeConfigName")]
    public string? ActiveConfigName { get; set; }
}
