using System.IO.Abstractions;
using Journey.Databases;
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
    private Migrator _migrator = null!;
    private IDatabase _database = null!;

    private void SetLogger(ILogger logger) {
        _migrator.SetLogger(logger);
    }

    public async Task Init(bool quiet, IFileSystem? fileSystem = null) {
        _database = databaseType switch {
            Sqlite.Name => await new Sqlite().Connect(connectionString),
            Postgres.Name => await new Postgres().Connect(connectionString, schema!),
            TimescaleDb.Name => await new TimescaleDb().Connect(connectionString, schema!),
            CockroachDb.Name => await new CockroachDb().Connect(connectionString, schema!),
            Mysql.Name => await new Mysql().Connect(connectionString, schema!),
            Mariadb.Name => await new Mariadb().Connect(connectionString, schema!),
            Mssql.Name => await new Mssql().Connect(connectionString),
            CassandraDb.Name => await new CassandraDb().Connect(connectionString),
            _ => await new Sqlite().Connect(connectionString),
        };
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