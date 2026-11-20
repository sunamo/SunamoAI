namespace SunamoAI;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

// Generic HTTP client for the sunamo.cz Claude proxy (cats.sunamo.cz/Claude/Ask) with bearer auth
// via ClaudeProxyAuthService - usable by any app that has access to this proxy
// (unlike ClaudeApiService, which calls api.anthropic.com directly with an API key).
public class ClaudeProxyService(HttpClient httpClient, ILogger logger, ClaudeProxyAuthService? authService = null)
{
    public static string BaseUrl = "https://cats.sunamo.cz";

    public async Task<string?> Ask(string prompt)
    {
        try
        {
            var (response, body) = await SendAsync(prompt);

            if (response.StatusCode == HttpStatusCode.Unauthorized && authService != null)
            {
                authService.InvalidateToken();
                (response, body) = await SendAsync(prompt);
            }

            response.EnsureSuccessStatusCode();
            // The server may return a plain string (hello) or a JSON-encoded string ("hello")
            return body.StartsWith("\"") ? JsonSerializer.Deserialize<string>(body) : body;
        }
        catch (Exception ex)
        {
            logger.LogError($"ClaudeProxyService.Ask failed: {ex.Message}");
            return null;
        }
    }

    private async Task<(HttpResponseMessage, string)> SendAsync(string prompt)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/Claude/Ask");
        request.Content = new StringContent(JsonSerializer.Serialize(prompt), Encoding.UTF8, "application/json");

        if (authService != null)
        {
            var token = await authService.GetTokenAsync();
            if (token != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return (response, body);
    }
}
