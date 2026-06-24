namespace SunamoAI;

using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

public class GeminiApiService
{
    private readonly ILogger logger;
    private readonly string apiKey;
    private readonly bool isBasicLoggingEnabled;
    private readonly bool isVerboseLoggingEnabled;
    private readonly bool isDetailedLoggingEnabled;
    private GoogleAI? geminiClient;

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

    private void InitializeClient()
    {
        if (geminiClient == null && apiKey != "PUT_YOUR_GEMINI_API_KEY_HERE")
        {
            geminiClient = new GoogleAI(apiKey);
        }
    }

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
