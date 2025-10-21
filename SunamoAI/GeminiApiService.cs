// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
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

            var config = new GenerationConfig
            {
                Temperature = temperature,
                MaxOutputTokens = maxOutputTokens,
                TopP = 1.0f,
                TopK = 1
            };

            if (_enableBasicLogging)
            {
                _logger.LogInformation($"Calling Gemini with prompt length: {prompt.Length}");
            }
            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Sending prompt to Gemini API (model: {model})");
            }

            GenerateContentResponse? response = null;
            try
            {
                response = await geminiModel.GenerateContent(prompt, config);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Gemini API call failed: {ex.Message}");

                // Check if it's a quota exceeded error
                if (ex.Message.Contains("TooManyRequests") || ex.Message.Contains("quota"))
                {
                    _logger.LogWarning("Gemini API quota exceeded");
                }
                return null;
            }

            if (_enableDetailedLogging)
            {
                _logger.LogInformation($"Gemini response: Text={response?.Text?.Length ?? 0} chars, Candidates={response?.Candidates?.Count ?? 0}");
            }

            var result = response?.Text;

            // If response.Text is empty, try to get text from candidates
            if (string.IsNullOrEmpty(result) && response?.Candidates != null && response.Candidates.Count > 0)
            {
                var firstCandidate = response.Candidates[0];
                if (_enableDetailedLogging)
                {
                    _logger.LogWarning($"response.Text is empty. First candidate: FinishReason={firstCandidate.FinishReason}, Content parts={firstCandidate.Content?.Parts?.Count ?? 0}");

                    // Log safety ratings to see if content was blocked
                    if (firstCandidate.SafetyRatings != null && firstCandidate.SafetyRatings.Count > 0)
                    {
                        foreach (var rating in firstCandidate.SafetyRatings)
                        {
                            _logger.LogWarning($"SafetyRating: Category={rating.Category}, Probability={rating.Probability}");
                        }
                    }
                }

                if (firstCandidate.Content?.Parts != null && firstCandidate.Content.Parts.Count > 0)
                {
                    var firstPart = firstCandidate.Content.Parts[0];
                    result = firstPart.Text;
                    if (_enableDetailedLogging)
                    {
                        _logger.LogInformation($"Extracted text from candidate.Content.Parts[0]: '{result}'");
                    }
                }
                else
                {
                    _logger.LogError("Content.Parts is null or empty - Gemini returned no usable response");
                }
            }
            else if (!string.IsNullOrEmpty(result) && _enableDetailedLogging)
            {
                _logger.LogInformation($"Successfully got response.Text: '{result}'");
            }

            if (string.IsNullOrEmpty(result))
            {
                if (_enableBasicLogging)
                {
                    _logger.LogWarning("Gemini returned empty response after checking all sources");
                }
                return null;
            }

            return result.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling Gemini API: {ex.Message}");
            return null;
        }
    }
}
