using Npgsql;
internal record Postgres : IDatabase
{
    private readonly SqlDialect _dialect = new SQliteDialect();
    private NpgsqlDataSource _dataSource;
    private string _schema;

    public async Task<IDatabase> Connect(string connectionString)
    {
        var dataSource = NpgsqlDataSource.Create(connectionString);
        _dataSource = dataSource;
        return this;
    }

    public async Task<IDatabase> Connect(string connectionString, string schema)
    {
        _schema = schema;
        return await Connect(connectionString);
    }

    public async Task Execute(string query)
    {
        var command = _dataSource.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CurrentVersion()
    {
        var command = _dataSource.CreateCommand();
        command.CommandText = _dialect.CurrentVersionQuery().Replace("versions", _schema + ".versions");
        try
        {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        }
        catch (NpgsqlException ex) when (ex.Message.Contains($"42P01: relation \"{_schema}.versions\" does not exist"))
        {
            return -1;
        }
    }

    public async Task<List<Itinerary>> GetItinerary(int entries)
    {
        var history = new List<Itinerary>();
        var command = _dataSource.CreateCommand();
        command.CommandText = _dialect.HistoryQuery().Replace("versions", _schema + ".versions").Replace("[entries]", entries.ToString());
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
        _dataSource.Dispose();
    }
}