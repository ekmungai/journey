using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MsLogger = Microsoft.Extensions.Logging.ILogger;
/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class MicrosoftLogger(MsLogger logger) : ILogger {
    public MicrosoftLogger(ILoggerFactory loggerFactory) : this(loggerFactory.CreateLogger<JourneyFacade>()) {
    }

    /// <inheritdoc/>
    public virtual void Debug(string message) => logger.LogDebug(message);
    /// <inheritdoc/>
    public virtual void Error(Exception ex, string message) => logger.LogError(ex, $"Message: {message}, Exception: {ex}");
    /// <inheritdoc/>
    public virtual void Information(string message) => logger.LogInformation(message);
}