using System.IO.Abstractions;
public class JourneyFacade(
    string databaseType,
    string connectionString,
    string versionsDir,
    string? schema
) : IJourneyFacade, IDisposable {
    internal Migrator _migrator;
    internal IDatabase _database;

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
        _migrator = new Migrator(new FileManager(versionsDir, _fileSystem ?? new FileSystem()), _database);
        await _migrator.Init(quiet);
    }

    public async Task<string> History(int entries) => await _migrator.History(entries);
    public async Task<string> Migrate(int? target, bool? debug, bool? dryRun) => await _migrator.Migrate(target, debug, dryRun);
    public async Task<string> Rollback(int? target, bool? debug) => await _migrator.Rollback(target, debug);
    public async Task<string> Scaffold() => await _migrator.Scaffold();
    public async Task<string> Update(bool? debug) => await _migrator.Update(debug);
    public async Task<string> Validate(int version) => await _migrator.Validate(version);

    public void Dispose() => _database.Dispose();
    public IDatabase GetDatabase() => _database;
}