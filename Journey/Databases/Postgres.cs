using System.Text.RegularExpressions;
using Journey.Dialects;
using Journey.Exceptions;
using Journey.Interfaces;
using Journey.Models;
using Npgsql;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Postgres : IDatabase {
    public const string Name = "postgres";
    private readonly SqlDialect _dialect = new PostgresDialect();
    private string _connectionString = null!;
    private const string DatabaseNameRegex = "(?i)(database|db)=([^;]+)";
    private string _schema = "";

    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        _connectionString = $"{connectionString};CommandTimeout=300;Timeout=300;";
        return Task.FromResult<IDatabase>(this);
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        _schema = schema;
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        command.CommandText = _schema != ""
            ? query.Replace("versions", _schema + ".versions")
            : query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        command.CommandText = _schema != ""
            ? _dialect.CurrentVersionQuery().Replace("versions", _schema + ".versions")
            : _dialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (NpgsqlException ex) when (ex.Message.Contains($"versions\" does not exist")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();
        await using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        var query = _dialect.HistoryQuery().Replace("[entries]", entries.ToString());
        command.CommandText = _schema != ""
            ? query.Replace("versions", _schema + ".versions")
            : query;
        var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) {
            history.Add(new Itinerary(
                reader["version"].ToString()!,
                (string)reader["description"],
                DateTime.SpecifyKind((DateTime)reader["run_time"], DateTimeKind.Utc),
                (string)reader["run_by"],
                (string)reader["author"]
            ));
        }
        return history;
    }

    /// <inheritdoc/>
    public IDialect GetDialect() {
        return _dialect;
    }

    public async Task<bool> CheckDatabase() {
        var databaseName = GetDatabaseName();

        // Connect to 'postgres' maintenance database to check if target database exists
        var maintenanceConnectionString = Regex.Replace(_connectionString, DatabaseNameRegex, "Database=postgres");

        await using var dataSource = NpgsqlDataSource.Create(maintenanceConnectionString);
        await using var fetchCommand = dataSource.CreateCommand();
        fetchCommand.CommandText = $"SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = '{databaseName}')";
        return (bool)(await fetchCommand.ExecuteScalarAsync())!;
    }

    /// <inheritdoc/>
    public async Task InitDatabase() {
        var databaseName = GetDatabaseName();

        var maintenanceConnectionString = Regex.Replace(_connectionString, DatabaseNameRegex, "Database=postgres");

        await using var dataSource = NpgsqlDataSource.Create(maintenanceConnectionString);

        await using var command = dataSource.CreateCommand();
        command.CommandText = $"CREATE DATABASE {databaseName}";
        command.ExecuteNonQuery();
    }

    public void Dispose() {
        //
    }

    private string GetDatabaseName() {
        var name = Regex.Match(_connectionString, DatabaseNameRegex).Groups[2].Value;
        return !string.IsNullOrWhiteSpace(name) ? name : throw new MissingDatabaseNameException();
    }
}