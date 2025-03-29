
internal record Postgres : IDatabase, IDisposable
{
    private SqlDialect _dialect = new SqlDialect();
    private string _connection;
    private string _schema;
    public Postgres(string schema)
    {
        _schema = schema;
    }
    public IDatabase Connect(string connectionString)
    {
        _connection = connectionString;
        return this;
    }

    public async Task Execute(string query)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CurrentVersion()
    {
        throw new NotImplementedException();
    }

    public IDialect GetDialect()
    {
        return _dialect;
    }

    public void Dispose()
    {
        return;
    }
}