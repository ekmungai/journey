using Npgsql;
/// <inheritdoc/>
internal record Postgres : IDatabase {
    private readonly SqlDialect _dialect = new PostgresDialect();
    private string _connectionString = default!;
    private string _schema = default!;

    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        _connectionString = connectionString;
        return Task.FromResult((IDatabase)this);
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        _schema = schema;
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        command.CommandText = _schema != null
            ? query.Replace("versions", _schema + ".versions")
            : query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        command.CommandText = _schema != null
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
        using var dataSource = NpgsqlDataSource.Create(_connectionString);
        var command = dataSource.CreateCommand();
        var query = _dialect.HistoryQuery().Replace("[entries]", entries.ToString());
        command.CommandText = _schema != null
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