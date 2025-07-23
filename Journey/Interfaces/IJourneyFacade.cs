/// <inheritdoc />
public interface IJourneyFacade : IMigrator {
    /// <summary>
    /// Sets Serilog as the logger for the migrator.
    /// </summary>
    /// <param name="logger">The Serilog logger instance to use.</param>
    public void UseSerilogLogging(Serilog.ILogger logger);
    /// <summary>
    /// Sets Microsoft as the logger for the migrator.
    /// </summary>
    /// <param name="logger">The Microsoft logger instance to use.</param>
    public void UseMicrosoftLogging(Microsoft.Extensions.Logging.ILogger logger);
    /// <summary>
    /// Sets Microsoft as the logger for the migrator, using a logger factory.
    /// </summary>
    /// <param name="logger">The Microsoft logger factory with which to create a logger instance to use.</param>
    public void UseMicrosoftLogging(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory);
}