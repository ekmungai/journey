using System.Diagnostics.CodeAnalysis;
/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class ConsoleLogger : ILogger {
    /// <inheritdoc/>
    public virtual void Debug(string message) => Console.WriteLine(message);
    /// <inheritdoc/>
    public virtual void Error(Exception ex, string message) => Console.WriteLine($"Message: {message}, Exception: {ex}");
    /// <inheritdoc/>
    public virtual void Information(string message) => Console.WriteLine(message);
}