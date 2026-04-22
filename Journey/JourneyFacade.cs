using System.Collections.Concurrent;
using System.IO.Abstractions;
using Journey.Helpers;
using Journey.Interfaces;
using Journey.Loggers;
using Microsoft.Extensions.Logging;
using ILogger = Journey.Interfaces.ILogger;

namespace Journey;

/// <inheritdoc cref="IJourneyFacade" />
public class JourneyFacade(
    string databaseType,
    string connectionString,
    string versionsDir,
    string? schema,
    bool? verbose
) : IJourneyFacade, IDisposable {
    private static readonly ConcurrentDictionary<string, Func<string, string?, Task<IDatabase>>> _registry = new();
    private Migrator _migrator = null!;
    private IDatabase _database = null!;

    /// <summary>Registers a database factory for a given database type name.</summary>
    public static void RegisterDatabase(string name, Func<string, string?, Task<IDatabase>> factory)
        => _registry[name] = factory;

    private void SetLogger(ILogger logger) {
        _migrator.SetLogger(logger);
    }

    public async Task Init(bool quiet, IFileSystem? fileSystem = null) {
        if (!_registry.TryGetValue(databaseType, out var factory))
            throw new NotSupportedException(
                $"Database type '{databaseType}' is not registered. " +
                $"Add a reference to the Ekmungai.Journey.* package for your database.");
        _database = await factory(connectionString, schema);
        _migrator = new Migrator(new FileManager(versionsDir, fileSystem ?? new FileSystem()), _database, verbose);
        await _migrator.Init(quiet);
    }
    /// <inheritdoc/>
    public async Task<string> History(int entries) => await _migrator.History(entries);
    /// <inheritdoc/>
    public async Task Migrate(int? target, bool? dryRun) => await _migrator.Migrate(target, dryRun);
    /// <inheritdoc/>
    public async Task Rollback(int? target) => await _migrator.Rollback(target);
    /// <inheritdoc/>
    public async Task Scaffold() => await _migrator.Scaffold();
    /// <inheritdoc/>
    public async Task Update(int? target) => await _migrator.Update(target);
    /// <inheritdoc/>
    public async Task<bool> Validate(int version) => await _migrator.Validate(version);
    /// <inheritdoc/>
    public Task Init(bool quiet) => Init(quiet, new FileSystem());
    /// <inheritdoc/>
    public void Dispose() => _database.Dispose();
    /// Get the database associated with the facade
    internal IDatabase GetDatabase() => _database;

    public void UseSerilogLogging(Serilog.ILogger logger) {
        SetLogger(new SerilogLogger(logger));
    }

    public void UseMicrosoftLogging(Microsoft.Extensions.Logging.ILogger logger) {
        SetLogger(new MicrosoftLogger(logger));
    }

    public void UseMicrosoftLogging(ILoggerFactory loggerFactory) {
        SetLogger(new MicrosoftLogger(loggerFactory));
    }

    public JourneyFacade InitSync() {
        AsyncHelper.RunSync(async () => await Init(true));
        return this;
    }

    public void UpdateSync(int? target = null) => AsyncHelper.RunSync(async () => await Update(target));
}