using System.Data.SQLite;
/// <inheritdoc/>
internal record Sqlite : IDatabase {
    private readonly SqlDialect _dialect = new SQliteDialect();
    private SQLiteConnection _connection = default!;

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString) {
        _connection = new SQLiteConnection(connectionString);
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

    /// <summary>
    /// Closes the connection to the database
    /// </summary>
    public void Dispose() {
        _connection.Close();
    }
}