using System.Diagnostics.CodeAnalysis;
/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class SerilogLogger(Serilog.ILogger logger) : ILogger {
    /// <inheritdoc/>
    public virtual void Debug(string message) => logger.Debug(message);
    /// <inheritdoc/>
    public virtual void Error(Exception ex, string message) => logger.Error(ex, $"Message: {message}, Exception: {ex}");
    /// <inheritdoc/>
    public virtual void Information(string message) => logger.Information(message);
}