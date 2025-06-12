using System.IO.Abstractions;
/// <inheritdoc/>
public class JourneyFacade(
    string databaseType,
    string connectionString,
    string versionsDir,
    string? schema,
    ILogger? logger,
    bool? verbose
) : IJourneyFacade, IDisposable {
    internal Migrator _migrator;
    internal IDatabase _database;
    private ILogger _logger = logger ?? new Logger();

    public async Task Init(bool quiet, IFileSystem? _fileSystem = null) {
        _database = databaseType switch {
            "sqlite" => await new Sqlite().Connect(connectionString),
            "postgres" => await new Postgres().Connect(connectionString, schema!),
            "timescaledb" => await new TimescaleDb().Connect(connectionString, schema!),
            "cockroachdb" => await new CockroachDb().Connect(connectionString, schema!),
            "mysql" => await new Mysql().Connect(connectionString, schema!),
            "mariadb" => await new Mariadb().Connect(connectionString, schema!),
            "mssql" => await new Mssql().Connect(connectionString),
            _ => await new Sqlite().Connect(connectionString),
        };
        _migrator = new Migrator(new FileManager(versionsDir, _fileSystem ?? new FileSystem()), _database, _logger, verbose);
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
    /// <inheritdoc/>
    public IDatabase GetDatabase() => _database;
}