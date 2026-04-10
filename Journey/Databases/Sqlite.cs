using System.Data.SQLite;
using System.Text.RegularExpressions;
using Journey.Dialects;
using Journey.Interfaces;
using Journey.Models;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Sqlite : IDatabase {
    public const string Name = "sqlite";
    private readonly SqlDialect _dialect = new SQliteDialect();
    private SQLiteConnection _connection = null!;

    internal static string NormalizeConnectionString(string connectionString) {
        if (!connectionString.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        if (connectionString.StartsWith("file::memory:", StringComparison.OrdinalIgnoreCase))
            return "Data Source=:memory:";

        // Strip "file:" prefix and optional authority ("//host"), keeping the path
        var path = Regex.Replace(connectionString, @"(?i)^file:(//[^/]*)?", "");
        // Drop any SQLite URI query parameters (e.g. ?cache=shared)
        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0) path = path[..queryIndex];

        return $"Data Source={path}";
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString) {
        _connection = new SQLiteConnection(NormalizeConnectionString(connectionString));
        await _connection.OpenAsync();
        return this;
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        var command = _connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        var command = _connection.CreateCommand();
        command.CommandText = _dialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (SQLiteException ex) when (ex.Message.Contains("no such table")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();
        var command = _connection.CreateCommand();
        command.CommandText = _dialect.HistoryQuery().Replace("[entries]", entries.ToString());
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
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

    public async Task<bool> CheckDatabase() => await Task.FromResult(true);

    public Task InitDatabase() {
        // Not necessary as sqlite automatically creates database if it doesn't exist
        return Task.CompletedTask;
    }

    /// <summary>
    /// Closes the connection to the database
    /// </summary>
    public void Dispose() {
        _connection.Close();
    }
}