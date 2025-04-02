
using MySqlConnector;

internal record Mysql : IDatabase
{
    private readonly SqlDialect _dbDialect = new MysqlDialect();
    private string _connectionString;
    private string _schema;

    public async Task<IDatabase> Connect(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public async Task<IDatabase> Connect(string connectionString, string schema)
    {
        _schema = schema;
        return await Connect(connectionString);
    }

    public async Task Execute(string query)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _schema != null
            ? query.Replace("versions", _schema + ".versions")
            : query;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CurrentVersion()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _schema != null
        ? _dbDialect.CurrentVersionQuery().Replace("versions", _schema + ".versions")
        : _dbDialect.CurrentVersionQuery();
        try
        {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        }
        catch (MySqlException ex) when (ex.Message.Contains($"versions' doesn't exist"))
        {
            return -1;
        }
    }

    public async Task<List<Itinerary>> GetItinerary(int entries)
    {
        var history = new List<Itinerary>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.HistoryQuery().Replace("[entries]", entries.ToString());

        using var reader = await command.ExecuteReaderAsync();
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
        await reader.CloseAsync();
        return history;
    }

    public IDialect GetDialect()
    {
        return _dbDialect;
    }

    public void Dispose()
    {
        //
    }
}