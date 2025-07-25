
using Journey.Dialects;
using Journey.Interfaces;
using Journey.Models;
using MySqlConnector;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Mysql : IDatabase {
    public const string Name = "mysql";
    private readonly SqlDialect _dbDialect = new MysqlDialect();
    private string _connectionString = null!;

    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        _connectionString = connectionString;
        return Task.FromResult<IDatabase>(this);
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (MySqlException ex) when (ex.Message.Contains($"versions' doesn't exist")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.HistoryQuery().Replace("[entries]", entries.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) {
            history.Add(new Itinerary(
                reader["version"].ToString()!,
                (string)reader["description"],
                DateTime.SpecifyKind((DateTime)reader["run_time"], DateTimeKind.Utc),
                (string)reader["run_by"],
                (string)reader["author"]
            ));
        }
        await reader.CloseAsync();
        return history;
    }

    /// <inheritdoc/>
    public IDialect GetDialect() {
        return _dbDialect;
    }

    public void Dispose() {
        //
    }
}