using System.Data.SQLite;
internal record Sqlite : IDatabase, IDisposable
{
    private SqlDialect _dialect = new SqlDialect();
    private SQLiteConnection _connection;

    public IDatabase Connect(string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);
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
        catch
        {
            return 0;
        }
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