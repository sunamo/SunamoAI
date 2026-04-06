namespace SunamoAI;

using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

/// <summary>
/// Service for calling Google Gemini API with configurable logging levels.
/// </summary>
public class GeminiApiService
{
    private readonly ILogger logger;
    private readonly string apiKey;
    private readonly bool isBasicLoggingEnabled;
    private readonly bool isVerboseLoggingEnabled;
    private readonly bool isDetailedLoggingEnabled;
    private GoogleAI? geminiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging operations.</param>
    /// <param name="apiKey">Google API key for authentication.</param>
    /// <param name="isBasicLoggingEnabled">Whether basic logging is enabled.</param>
    /// <param name="isVerboseLoggingEnabled">Whether verbose logging is enabled for debugging.</param>
    /// <param name="isDetailedLoggingEnabled">Whether detailed logging is enabled for API calls.</param>
    public GeminiApiService(
        ILogger logger,
        string apiKey,
        bool isBasicLoggingEnabled = false,
        bool isVerboseLoggingEnabled = false,
        bool isDetailedLoggingEnabled = false)
    {
        this.logger = logger;
        this.apiKey = apiKey;
        this.isBasicLoggingEnabled = isBasicLoggingEnabled;
        this.isVerboseLoggingEnabled = isVerboseLoggingEnabled;
        this.isDetailedLoggingEnabled = isDetailedLoggingEnabled;
    }

    /// <summary>
    /// Initializes the Gemini client if not already initialized and the API key is valid.
    /// </summary>
    private void InitializeClient()
    {
        if (geminiClient == null && apiKey != "PUT_YOUR_GEMINI_API_KEY_HERE")
        {
            geminiClient = new GoogleAI(apiKey);
        }
    }

    /// <summary>
    /// Calls Gemini API with a prompt and returns the response.
    /// </summary>
    /// <param name="prompt">The prompt to send to Gemini.</param>
    /// <param name="model">Gemini model to use (default: gemini-2.5-flash).</param>
    /// <param name="temperature">Temperature for response generation (default: 0.0).</param>
    /// <param name="maxOutputTokens">Maximum tokens in response (default: 8192).</param>
    /// <returns>Gemini's response text, or null if the call failed.</returns>
    public async Task<string?> CallGeminiApi(
        string prompt,
        string model = "gemini-2.5-flash",
        float temperature = 0.0f,
        int maxOutputTokens = 8192)
    {
        InitializeClient();

        if (geminiClient == null)
        {
            logger.LogWarning("Gemini client not initialized - API key not set");
            return null;
        }

        try
        {
            var geminiModel = geminiClient.GenerativeModel(model: model);

            if (isBasicLoggingEnabled)
            {
                logger.LogInformation($"Calling Gemini with prompt length: {prompt.Length}");
            }
            if (isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Sending prompt to Gemini API (model: {model})");
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
                logger.LogError($"Gemini API call failed: {exception.Message}");

                if (exception.Message.Contains("TooManyRequests") || exception.Message.Contains("quota"))
                {
                    logger.LogWarning("Gemini API quota exceeded");
                }
                return null;
            }

            if (isDetailedLoggingEnabled)
            {
                logger.LogInformation($"Gemini response: Text={response?.Text?.Length ?? 0} chars");
            }

            var result = response?.Text;

            if (string.IsNullOrEmpty(result))
            {
                if (isBasicLoggingEnabled)
                {
                    logger.LogWarning("Gemini returned empty response");
                }
                return null;
            }

            return result.Trim();
        }
        catch (Exception exception)
        {
            logger.LogError($"Error calling Gemini API: {exception.Message}");
            return null;
        }
    }
}
