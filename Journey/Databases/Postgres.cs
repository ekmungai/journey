using Journey.Dialects;
using Journey.Interfaces;
using Journey.Models;
using Npgsql;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Postgres : IDatabase {
    public const string Name = "postgres";
    private readonly SqlDialect _dialect = new PostgresDialect();
    private string _connectionString = null!;
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

    public void Dispose() {
        //
    }
}