using Microsoft.Extensions.Logging;
using ILogger = Journey.Interfaces.ILogger;
using MsLogger = Microsoft.Extensions.Logging.ILogger;
namespace Journey.Loggers;

/// <inheritdoc/>
public sealed class MicrosoftLogger(MsLogger logger) : ILogger {
    public MicrosoftLogger(ILoggerFactory loggerFactory) : this(loggerFactory.CreateLogger<JourneyFacade>()) {
    }

    /// <inheritdoc/>
    public void Debug(string message) => logger.LogDebug(message);
    /// <inheritdoc/>
    public void Error(Exception ex, string message) => logger.LogError(ex, "Message: {message}, Exception: {ex}", message, ex);
    /// <inheritdoc/>
    public void Information(string message) => logger.LogInformation("{message}", message);
}