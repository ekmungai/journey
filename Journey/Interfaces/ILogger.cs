public interface ILogger {
    void Information(string message);
    void Debug(string message);
    void Error(string message);
    void Error(Exception ex, string message);
}