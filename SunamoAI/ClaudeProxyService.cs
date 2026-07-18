namespace SunamoAI;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

// Genericky HTTP klient na sunamo.cz Claude proxy (cats.sunamo.cz/Claude/Ask) s bearer auth
// pres ClaudeProxyAuthService - pouzitelny libovolnou app, ktera ma pristup k tomuto proxy
// (na rozdil od ClaudeApiService, ktery vola primo api.anthropic.com s api klicem).
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
            // Server muze vratit obycejny string (hello) nebo JSON-encoded string ("hello")
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
