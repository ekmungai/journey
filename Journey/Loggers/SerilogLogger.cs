using System.Diagnostics.CodeAnalysis;
using Journey.Interfaces;

namespace Journey.Loggers;

/// <inheritdoc/>
public sealed class SerilogLogger(Serilog.ILogger logger) : ILogger {
    /// <inheritdoc/>
    public void Debug(string message) => logger.Debug(message);
    /// <inheritdoc/>
    public void Error(Exception ex, string message) => logger.Error(ex, $"Message: {message}, Exception: {ex}");
    /// <inheritdoc/>
    public void Information(string message) => logger.Information("{message}", message);
}