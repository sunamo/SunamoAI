namespace SunamoAI;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents the claude api service type.
/// </summary>
public class ClaudeApiService
{
    private readonly ILogger logger;
    private readonly string apiKey;
    private readonly bool isVerboseLoggingEnabled;
    private readonly bool isDetailedLoggingEnabled;

    /// <summary>
    /// Initializes a new instance of the claude api service class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiKey">The api key.</param>
    /// <param name="isVerboseLoggingEnabled">The is verbose logging enabled.</param>
    /// <param name="isDetailedLoggingEnabled">The is detailed logging enabled.</param>
    public ClaudeApiService(
        ILogger logger,
        string apiKey,
        bool isVerboseLoggingEnabled = false,
        bool isDetailedLoggingEnabled = false)
    {
        this.logger = logger;
        this.apiKey = apiKey;
        this.isVerboseLoggingEnabled = isVerboseLoggingEnabled;
        this.isDetailedLoggingEnabled = isDetailedLoggingEnabled;
    }

    /// <summary>
    /// Call claude api.
    /// </summary>
    /// <param name="prompt">The prompt.</param>
    /// <param name="model">The model.</param>
    /// <param name="maxTokens">The max tokens.</param>
    /// <param name="temperature">The temperature.</param>
    /// <returns>The call claude api result.</returns>
    public async Task<string?> CallClaudeApi(
        string prompt,
        string model = "claude-sonnet-4-20250514",
        int maxTokens = 1024,
        double temperature = 0.0)
    {
        try
        {
            if (isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Sending prompt to Claude API (length: {prompt.Length} chars)");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = model,
                max_tokens = maxTokens,
                temperature = temperature,
                messages = new[] { new { role = "user", content = prompt } }
            };

            var requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            if (isDetailedLoggingEnabled)
            {
                logger.LogInformation($"Calling Claude API with model {model}");
            }

            var claudeResponse = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", httpContent);
            var responseBody = await claudeResponse.Content.ReadAsStringAsync();

            if (!claudeResponse.IsSuccessStatusCode)
            {
                logger.LogWarning($"Claude API call failed with status {claudeResponse.StatusCode}: {responseBody}");
                return null;
            }

            string? result = null;
            using var jsonDocument = System.Text.Json.JsonDocument.Parse(responseBody);
            var rootElement = jsonDocument.RootElement;
            if (rootElement.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
            {
                var firstContent = contentArray[0];
                if (firstContent.TryGetProperty("text", out var textElement))
                {
                    result = textElement.GetString();
                }
            }

            if (result == null)
            {
                logger.LogWarning("Claude API returned no text content");
                return null;
            }

            if (isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Claude API response received: {result.Length} chars");
            }

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError($"Error calling Claude API: {exception.Message}");
            return null;
        }
    }
}
