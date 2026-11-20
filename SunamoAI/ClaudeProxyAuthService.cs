namespace SunamoAI;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Login/bearer auth for ClaudeProxyService - holds the token until expiry, thread-safe refresh.
/// <summary>
/// Claude proxy auth service.
/// </summary>
/// <param name="httpClient">The http client.</param>
/// <param name="baseUrl">The base url.</param>
/// <param name="email">The email.</param>
/// <param name="password">The password.</param>
/// <returns>The claude proxy auth service result.</returns>
public class ClaudeProxyAuthService(HttpClient httpClient, string baseUrl, string email, string password)
{
    private string? _token;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Invalidate token.
    /// </summary>
    public void InvalidateToken() => _token = null;

    /// <summary>
    /// Get token async.
    /// </summary>
    /// <returns>The get token async result.</returns>
    public async Task<string?> GetTokenAsync()
    {
        if (_token != null && DateTime.UtcNow < _tokenExpiry)
            return _token;

        await _semaphore.WaitAsync();
        try
        {
            if (_token != null && DateTime.UtcNow < _tokenExpiry)
                return _token;

            var body = JsonSerializer.Serialize(new { Mail = email, Password = password });
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{baseUrl}/Account/LogIn", content);
            if (!response.IsSuccessStatusCode)
                return null;

            var raw = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LogInResponse>(raw, JsonOptions);
            if (string.IsNullOrEmpty(result?.Token))
                return null;

            _token = result.Token;
            _tokenExpiry = DateTime.UtcNow.AddHours(23);
            return _token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private sealed record LogInResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("message")] string Message);
}
