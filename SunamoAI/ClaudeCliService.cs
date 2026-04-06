namespace SunamoAI;

using Microsoft.Extensions.Logging;

/// <summary>
/// Service for calling Claude CLI (claude.cmd) with retry logic and rate limit handling.
/// </summary>
public class ClaudeCliService
{
    private readonly ILogger logger;
    private readonly bool isVerboseLoggingEnabled;
    private readonly bool isDetailedLoggingEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaudeCliService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging operations.</param>
    /// <param name="isVerboseLoggingEnabled">Whether verbose logging is enabled for debugging.</param>
    /// <param name="isDetailedLoggingEnabled">Whether detailed logging is enabled for CLI calls.</param>
    public ClaudeCliService(
        ILogger logger,
        bool isVerboseLoggingEnabled = false,
        bool isDetailedLoggingEnabled = false)
    {
        this.logger = logger;
        this.isVerboseLoggingEnabled = isVerboseLoggingEnabled;
        this.isDetailedLoggingEnabled = isDetailedLoggingEnabled;
    }

    /// <summary>
    /// Calls Claude CLI with a prompt and returns the response.
    /// Includes automatic retry logic for rate limit errors (up to 3 retries with 65-second waits).
    /// </summary>
    /// <param name="prompt">The prompt to send to Claude.</param>
    /// <param name="retryCount">Internal retry counter (starts at 0).</param>
    /// <returns>Claude's response text, or null if the call failed.</returns>
    public async Task<string?> CallClaudeCli(string prompt, int retryCount = 0)
    {
        try
        {
            if (isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Sending prompt to Claude CLI (length: {prompt.Length} chars)");
            }

            if (isDetailedLoggingEnabled)
            {
                logger.LogInformation($"Calling Claude CLI with prompt length: {prompt.Length}");
            }

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

            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();

            var result = await process.StandardOutput.ReadToEndAsync();
            var standardError = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(standardError))
            {
                logger.LogWarning($"Claude CLI stderr: {standardError}");
            }
            if (!string.IsNullOrEmpty(result) && isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Claude CLI stdout: {result.Substring(0, Math.Min(200, result.Length))}...");
            }

            if (process.ExitCode != 0)
            {
                logger.LogWarning($"Claude CLI failed with exit code {process.ExitCode}");

                if (standardError.Contains("rate_limit_error") || standardError.Contains("TooManyRequests") || standardError.Contains("rate limit"))
                {
                    if (retryCount < 3)
                    {
                        logger.LogWarning($"Claude CLI rate limit exceeded. Waiting 65 seconds before retry (attempt {retryCount + 1}/3)...");

                        for (int secondsRemaining = 65; secondsRemaining > 0; secondsRemaining--)
                        {
                            Console.Write($"\rRate limit - waiting: {secondsRemaining}s remaining...  ");
                            await Task.Delay(1000);
                        }
                        Console.WriteLine("\rRetrying Claude CLI request...                    ");

                        return await CallClaudeCli(prompt, retryCount + 1);
                    }
                    else
                    {
                        var errorMessage = $"Claude CLI rate limit exceeded after 3 retries. Exiting program. Error: {standardError}";
                        logger.LogError(errorMessage);
                        Console.WriteLine($"\n\n{errorMessage}");
                        Environment.Exit(1);
                    }
                }

                logger.LogWarning($"Claude CLI failed with exit code {process.ExitCode}: {standardError}");
                return null;
            }

            if (string.IsNullOrEmpty(result))
            {
                logger.LogWarning($"Claude CLI returned empty response. Exit code: {process.ExitCode}, stderr length: {standardError?.Length ?? 0}");
                return null;
            }

            if (isVerboseLoggingEnabled)
            {
                logger.LogInformation($"Claude CLI response received: {result.Length} chars");
            }

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError($"Error calling Claude CLI: {exception.Message}");
            return null;
        }
    }
}
