
using System.Data.SqlClient;

internal record Mssql : IDatabase {
    private readonly SqlDialect _dbDialect = new MssqlDialect();
    private string _connectionString;

    public async Task<IDatabase> Connect(string connectionString) {
        _connectionString = connectionString;
        return this;
    }

    public async Task<IDatabase> Connect(string connectionString, string schema) {
        return await Connect(connectionString);
    }


    public async Task Execute(string query) {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

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

    public IDialect GetDialect() {
        return _dbDialect;
    }

    public void Dispose() {
        //
    }
}