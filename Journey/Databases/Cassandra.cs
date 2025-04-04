
using Cassandra;

internal record CassandraDb : IDatabase {
    private readonly CassandraDialect _dialect = new();
    private string _key_space;
    private ISession _session;

    public async Task<IDatabase> Connect(string connectionString) {
        var cluster = Cluster.Builder()
                     .WithConnectionString(connectionString)
                     .Build();
        var systemSesion = cluster.Connect("system");
        systemSesion.Execute(_dialect.CreateKeySpace().Replace("[key_space]", _key_space));
        systemSesion.Dispose();
        _session = cluster.Connect(_key_space);
        return this;
    }

    public async Task<IDatabase> Connect(string connectionString, string key_space) {
        _key_space = key_space;
        return await Connect(connectionString);
    }

    public async Task Execute(string query) {
        var statement = new SimpleStatement(query);
        await _session.ExecuteAsync(statement);
    }

    public async Task<int> CurrentVersion() {
        var statement = new SimpleStatement(_dialect.CurrentVersionQuery().Replace("versions", _key_space + ".versions"));
        try {
            var rowSet = await _session.ExecuteAsync(statement);
            var version = rowSet.GetRows().FirstOrDefault()!.GetValue<long>("version");
            return int.Parse(version.ToString() ?? "");
        } catch (InvalidQueryException ex) when (ex.Message.Contains("table versions does not exist")) {
            return -1;
        }
    }

    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();
        var statement = new SimpleStatement(_dialect.HistoryQuery()
            .Replace("versions", _key_space + ".versions")
            .Replace("[entries]", entries.ToString()));
        var result = await _session.ExecuteAsync(statement);
        foreach (var row in result) {
            history.Add(new Itinerary(
                row.GetValue<int>("version").ToString(),
                row.GetValue<string>("description"),
                DateTime.SpecifyKind(row.GetValue<DateTime>("run_time"), DateTimeKind.Utc),
                row.GetValue<string>("run_by"),
                row.GetValue<string>("author")
            ));
        }
        return history;
    }

    public IDialect GetDialect() {
        return _dialect;
    }

    public void Dispose() {
        _session.Dispose();
    }
}