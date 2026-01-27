using System.Diagnostics;
using ClaudeCodeApiConfigManager.Models;

namespace ClaudeCodeApiConfigManager.Services;

/// <summary>
/// API 测试服务
/// </summary>
public class ApiTester
{
    /// <summary>
    /// 测试 API 连接
    /// </summary>
    public async Task<bool> TestAsync(ApiConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", config.AuthToken);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var stopwatch = Stopwatch.StartNew();

            // 使用 /v1/messages 端点进行健康检查
            var request = new HttpRequestMessage(HttpMethod.Post, config.BaseUrl.TrimEnd('/') + "/v1/messages");
            var payload = $$"""{"model": "{{config.Model}}", "max_tokens": 1, "messages": [{"role": "user", "content": "Hi"}]}""";
            request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cancellationToken);

            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"连接成功！响应时间: {stopwatch.ElapsedMilliseconds}ms");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"连接失败: {response.StatusCode}");
                Console.WriteLine($"错误详情: {error}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"连接失败: 网络错误");
            Console.WriteLine($"错误详情: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("连接失败: 请求超时");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"连接失败: {ex.Message}");
            return false;
        }
    }
}
