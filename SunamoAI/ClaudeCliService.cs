// variables names: ok
namespace SunamoAI;

using Microsoft.Extensions.Logging;

/// <summary>
/// Generic service for calling Claude CLI (claude.cmd) with retry logic and rate limit handling
/// </summary>
public class ClaudeCliService
{
    private readonly ILogger _logger;
    private readonly bool _enableVerboseLogging;
    private readonly bool _enableDetailedLogging;

    /// <summary>
    /// Initializes a new instance of the ClaudeCliService class
    /// </summary>
    /// <param name="logger">Logger instance for logging operations</param>
    /// <param name="enableVerboseLogging">Enable verbose logging for debugging</param>
    /// <param name="enableDetailedLogging">Enable detailed logging for CLI calls</param>
    public ClaudeCliService(
        ILogger logger,
        bool enableVerboseLogging = false,
        bool enableDetailedLogging = false)
    {
        _logger = logger;
        _enableVerboseLogging = enableVerboseLogging;
        _enableDetailedLogging = enableDetailedLogging;
    }

    /// <summary>
    /// Calls Claude CLI with a prompt and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to Claude</param>
    /// <param name="retryCount">Internal retry counter (starts at 0)</param>
    /// <returns>Claude's response text, or null if failed</returns>
    public async Task<string?> CallClaudeCli(string prompt, int retryCount = 0)
    {
        try
        {
            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Sending prompt to Claude CLI (length: {prompt.Length} chars)");
            }

            if (_enableDetailedLogging)
            {
                _logger.LogInformation($"Calling Claude CLI with prompt length: {prompt.Length}");
            }

            // Create process to run claude CLI
            // IMPORTANT: Use --print to make claude output the result and exit immediately (not interactive mode)
            // Pass prompt via stdin to avoid escaping issues with quotes
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c claude --dangerously-skip-permissions --disallowedTools \"\" --permission-mode bypassPermissions --model sonnet --print",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();

            // Write prompt to stdin and close it
            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();

            var result = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Always log when we get output or errors for debugging
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning($"Claude CLI stderr: {error}");
            }
            if (!string.IsNullOrEmpty(result) && _enableVerboseLogging)
            {
                _logger.LogInformation($"Claude CLI stdout: {result.Substring(0, Math.Min(200, result.Length))}...");
            }

            if (process.ExitCode != 0)
            {
                _logger.LogWarning($"Claude CLI failed with exit code {process.ExitCode}");

                // Check if error contains rate limit message
                if (error.Contains("rate_limit_error") || error.Contains("TooManyRequests") || error.Contains("rate limit"))
                {
                    if (retryCount < 3) // Limit retries to prevent infinite loop
                    {
                        _logger.LogWarning($"Claude CLI rate limit exceeded. Waiting 65 seconds before retry (attempt {retryCount + 1}/3)...");

                        // Countdown from 65 to 1
                        for (int secondsRemaining = 65; secondsRemaining > 0; secondsRemaining--)
                        {
                            Console.Write($"\rRate limit - waiting: {secondsRemaining}s remaining...  ");
                            await Task.Delay(1000);
                        }
                        Console.WriteLine("\rRetrying Claude CLI request...                    ");

                        // Retry the request
                        return await CallClaudeCli(prompt, retryCount + 1);
                    }
                    else
                    {
                        var errorMessage = $"Claude CLI rate limit exceeded after 3 retries. Exiting program. Error: {error}";
                        _logger.LogError(errorMessage);
                        Console.WriteLine($"\n\n{errorMessage}");
                        Environment.Exit(1); // Exit with error code 1
                    }
                }

                _logger.LogWarning($"Claude CLI failed with exit code {process.ExitCode}: {error}");
                return null;
            }

            if (string.IsNullOrEmpty(result))
            {
                _logger.LogWarning($"Claude CLI returned empty response. Exit code: {process.ExitCode}, stderr length: {error?.Length ?? 0}");
                return null;
            }

            if (_enableVerboseLogging)
            {
                _logger.LogInformation($"Claude CLI response received: {result.Length} chars");
            }

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error calling Claude CLI: {exception.Message}");
            return null;
        }
    }
}