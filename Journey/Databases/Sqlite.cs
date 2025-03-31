using System.Data.SQLite;
internal record Sqlite : IDatabase
{
    private SqlDialect _dialect = new SQliteDialect();
    private SQLiteConnection _connection;

    public async Task<IDatabase> Connect(string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);
        await _connection.OpenAsync();
        return this;
    }

    public async Task Execute(string query)
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CurrentVersion()
    {
        var command = _connection.CreateCommand();
        command.CommandText = _dialect.CurrentVersionQuery();
        try
        {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        }
        catch (SQLiteException ex) when (ex.Message.Contains("no such table"))
        {
            return -1;
        }
    }

    public async Task<List<Itinerary>> GetItinerary(int entries)
    {
        var history = new List<Itinerary>();
        var command = _connection.CreateCommand();
        command.CommandText = _dialect.HistoryQuery().Replace("[entries]", entries.ToString());
        var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
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

    public IDialect GetDialect()
    {
        return _dialect;
    }

    public void Dispose()
    {
        _connection.Close();
    }
}