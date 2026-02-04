namespace SunamoAI;

using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

/// <summary>
/// Generic service for calling Google Gemini API
/// </summary>
public class GeminiApiService
{
    private readonly ILogger _logger;
    private readonly string _apiKey;
    private readonly bool _enableBasicLogging;
    private readonly bool _enableVerboseLogging;
    private readonly bool _enableDetailedLogging;
    private GoogleAI? _geminiClient;

    /// <summary>
    /// Initializes a new instance of the GeminiApiService class
    /// </summary>
    /// <param name="logger">Logger instance for logging operations</param>
    /// <param name="apiKey">Google API key for authentication</param>
    /// <param name="enableBasicLogging">Enable basic logging</param>
    /// <param name="enableVerboseLogging">Enable verbose logging for debugging</param>
    /// <param name="enableDetailedLogging">Enable detailed logging for API calls</param>
    public GeminiApiService(
        ILogger logger,
        string apiKey,
        bool enableBasicLogging = false,
        bool enableVerboseLogging = false,
        bool enableDetailedLogging = false)
    {
        _logger = logger;
        _apiKey = apiKey;
        _enableBasicLogging = enableBasicLogging;
        _enableVerboseLogging = enableVerboseLogging;
        _enableDetailedLogging = enableDetailedLogging;
    }

    private void InitializeClient()
    {
        if (_geminiClient == null && _apiKey != "PUT_YOUR_GEMINI_API_KEY_HERE")
        {
            _geminiClient = new GoogleAI(_apiKey);
        }
    }

    /// <summary>
    /// Calls Gemini API with a prompt and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to Gemini</param>
    /// <param name="model">Gemini model to use (default: gemini-2.5-flash)</param>
    /// <param name="temperature">Temperature for response generation (default: 0.0)</param>
    /// <param name="maxOutputTokens">Maximum tokens in response (default: 8192)</param>
    /// <returns>Gemini's response text, or null if failed</returns>
    public async Task<string?> CallGeminiApi(
        string prompt,
        string model = "gemini-2.5-flash",
        float temperature = 0.0f,
        int maxOutputTokens = 8192)
    {
        InitializeClient();

        if (_geminiClient == null)
        {
            _logger.LogWarning("Gemini client not initialized - API key not set");
            return null;
        }

        try
        {
            var geminiModel = _geminiClient.GenerativeModel(model: model);

            if (_enableBasicLogging)
            {
                _logger.LogInformation($"Calling Gemini with prompt length: {prompt.Length}");
            }
            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Sending prompt to Gemini API (model: {model})");
            }

            var request = new GenerateContentRequest(prompt)
            {
                GenerationConfig = new GenerationConfig
                {
                    Temperature = temperature,
                    MaxOutputTokens = maxOutputTokens,
                    TopP = 1.0f,
                    TopK = 1
                }
            };

            GenerateContentResponse? response = null;
            try
            {
                response = await geminiModel.GenerateContent(request);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Gemini API call failed: {exception.Message}");

                // Check if it's a quota exceeded error
                if (exception.Message.Contains("TooManyRequests") || exception.Message.Contains("quota"))
                {
                    _logger.LogWarning("Gemini API quota exceeded");
                }
                return null;
            }

            if (_enableDetailedLogging)
            {
                _logger.LogInformation($"Gemini response: Text={response?.Text?.Length ?? 0} chars");
            }

            var result = response?.Text;

            if (string.IsNullOrEmpty(result))
            {
                if (_enableBasicLogging)
                {
                    _logger.LogWarning("Gemini returned empty response");
                }
                return null;
            }

            return result.Trim();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error calling Gemini API: {exception.Message}");
            return null;
        }
    }
}