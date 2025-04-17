using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Logger : ILogger {
    public virtual void Debug(string message) => Console.WriteLine(message);
    public virtual void Error(string message) => Console.WriteLine(message);
    public virtual void Error(Exception ex, string message) => Console.WriteLine($"Message: {message}, Exception: {ex}");
    public virtual void Information(string message) => Console.WriteLine(message);
}