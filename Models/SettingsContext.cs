using System.Text.Json.Serialization;

namespace ClaudeCodeApiConfigManager.Models;

/// <summary>
/// JSON 序列化上下文 - 用于 AOT 编译
/// </summary>
[JsonSerializable(typeof(SettingsConfig))]
[JsonSerializable(typeof(ApiConfig))]
[JsonSerializable(typeof(List<ApiConfig>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class SettingsContext : JsonSerializerContext;
