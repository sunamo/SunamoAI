namespace SunamoAI;

using Microsoft.Extensions.Logging;

/// <summary>
/// Generic service for calling Claude API (Anthropic HTTP API)
/// </summary>
public class ClaudeApiService
{
    private readonly ILogger _logger;
    private readonly string _apiKey;
    private readonly bool _enableVerboseLogging;
    private readonly bool _enableDetailedLogging;

    /// <summary>
    /// Initializes a new instance of the ClaudeApiService class
    /// </summary>
    /// <param name="logger">Logger instance for logging operations</param>
    /// <param name="apiKey">Anthropic API key for authentication</param>
    /// <param name="enableVerboseLogging">Enable verbose logging for debugging</param>
    /// <param name="enableDetailedLogging">Enable detailed logging for API calls</param>
    public ClaudeApiService(
        ILogger logger,
        string apiKey,
        bool enableVerboseLogging = false,
        bool enableDetailedLogging = false)
    {
        _logger = logger;
        _apiKey = apiKey;
        _enableVerboseLogging = enableVerboseLogging;
        _enableDetailedLogging = enableDetailedLogging;
    }

    /// <summary>
    /// Calls Claude API with a prompt and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to Claude</param>
    /// <param name="model">Claude model to use (default: claude-sonnet-4-20250514)</param>
    /// <param name="maxTokens">Maximum tokens in response (default: 1024)</param>
    /// <param name="temperature">Temperature for response generation (default: 0.0)</param>
    /// <returns>Claude's response text, or null if failed</returns>
    public async Task<string?> CallClaudeApi(
        string prompt,
        string model = "claude-sonnet-4-20250514",
        int maxTokens = 1024,
        double temperature = 0.0)
    {
        try
        {
            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Sending prompt to Claude API (length: {prompt.Length} chars)");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = model,
                max_tokens = maxTokens,
                temperature = temperature,
                messages = new[] { new { role = "user", content = prompt } }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            if (_enableDetailedLogging)
            {
                _logger.LogInformation($"Calling Claude API with model {model}");
            }

            var claudeResponse = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseBody = await claudeResponse.Content.ReadAsStringAsync();

            if (!claudeResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Claude API call failed with status {claudeResponse.StatusCode}: {responseBody}");
                return null;
            }

            string? result = null;
            using var jsonDocument = System.Text.Json.JsonDocument.Parse(responseBody);
            var root = jsonDocument.RootElement;
            if (root.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
            {
                var firstContent = contentArray[0];
                if (firstContent.TryGetProperty("text", out var textElement))
                {
                    result = textElement.GetString();
                }
            }

            if (result == null)
            {
                _logger.LogWarning("Claude API returned no text content");
                return null;
            }

            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Claude API response received: {result.Length} chars");
            }

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error calling Claude API: {exception.Message}");
            return null;
        }
    }
}