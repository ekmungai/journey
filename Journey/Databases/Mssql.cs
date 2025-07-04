
using System.Data.SqlClient;
/// <inheritdoc/>
internal record Mssql : IDatabase {
    public const string Name = "mssql";
    private readonly SqlDialect _dbDialect = new MssqlDialect();
    private string _connectionString = default!;

    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        _connectionString = connectionString;
        return Task.FromResult((IDatabase)this);
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'dbo.Versions'")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.HistoryQuery().Replace("[entries]", entries.ToString());

        using var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) {
            history.Add(new Itinerary(
                reader["Version"].ToString()!,
                (string)reader["Description"],
                DateTime.SpecifyKind((DateTime)reader["RunTime"], DateTimeKind.Utc),
                (string)reader["RunBy"],
                (string)reader["Author"]
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