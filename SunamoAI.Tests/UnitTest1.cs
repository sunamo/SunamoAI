// variables names: ok
namespace SunamoAI.Tests;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class UnitTest1
{
    [Fact]
    public void ClaudeCliService_Constructor_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new ClaudeCliService(logger);

        Assert.NotNull(service);
    }

    [Fact]
    public void ClaudeCliService_ConstructorWithAllParameters_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new ClaudeCliService(logger, isVerboseLoggingEnabled: true, isDetailedLoggingEnabled: true);

        Assert.NotNull(service);
    }

    [Fact]
    public void ClaudeApiService_Constructor_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new ClaudeApiService(logger, "test-api-key");

        Assert.NotNull(service);
    }

    [Fact]
    public void ClaudeApiService_ConstructorWithAllParameters_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new ClaudeApiService(logger, "test-api-key", isVerboseLoggingEnabled: true, isDetailedLoggingEnabled: true);

        Assert.NotNull(service);
    }

    [Fact]
    public void GeminiApiService_Constructor_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new GeminiApiService(logger, "test-api-key");

        Assert.NotNull(service);
    }

    [Fact]
    public void GeminiApiService_ConstructorWithAllParameters_CreatesInstance()
    {
        var logger = NullLogger.Instance;

        var service = new GeminiApiService(logger, "test-api-key", isBasicLoggingEnabled: true, isVerboseLoggingEnabled: true, isDetailedLoggingEnabled: true);

        Assert.NotNull(service);
    }
}
