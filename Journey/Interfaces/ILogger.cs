/// <summary>
/// Represents a means of reporting the state of the library.
/// </summary>
public interface ILogger {
    /// <summary>
    /// Provides information about events in the operation of the library.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Information(string message);
    /// <summary>
    /// Provides diagnostic details about events in the operation of the library.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Debug(string message);
    /// <summary>
    /// Reports errors in the operation of the library, including details of the exception that was raised.
    /// </summary>
    /// <param name="ex">The exception that was raised.</param>
    /// <param name="message">The error to report.</param>
    void Error(Exception ex, string message);
}