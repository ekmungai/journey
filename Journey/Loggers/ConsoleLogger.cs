using System.Diagnostics.CodeAnalysis;
using Journey.Interfaces;

namespace Journey.Loggers;

/// <inheritdoc/>
public sealed class ConsoleLogger : ILogger {
    /// <inheritdoc/>
    public void Debug(string message) => Console.WriteLine(message);
    /// <inheritdoc/>
    public void Error(Exception ex, string message) => Console.WriteLine($"Message: {message}, Exception: {ex}");
    /// <inheritdoc/>
    public void Information(string message) => Console.WriteLine(message);
}