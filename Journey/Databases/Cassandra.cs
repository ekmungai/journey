
using Cassandra;
using Journey.Dialects;
using Journey.Interfaces;
using Journey.Models;

namespace Journey.Databases;

/// <inheritdoc/>
internal record CassandraDb : IDatabase {
    public const string Name = "cassandra";
    private readonly CassandraDialect _dialect = new();
    private string _keySpace = null!;
    private ISession _session = null!;
    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        var cluster = Cluster.Builder()
            .WithConnectionString(connectionString)
            .Build();
        var systemSesion = cluster.Connect("system");
        systemSesion.Execute(_dialect.CreateKeySpace().Replace("[key_space]", _keySpace));
        systemSesion.Dispose();
        _session = cluster.Connect(_keySpace);
        return Task.FromResult<IDatabase>(this);
    }
    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string keySpace) {
        _keySpace = keySpace;
        return await Connect(connectionString);
    }
    /// <inheritdoc/>
    public async Task Execute(string query) {
        var statement = new SimpleStatement(query);
        await _session.ExecuteAsync(statement);
    }
    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        var statement = new SimpleStatement(_dialect.CurrentVersionQuery().Replace("versions", _keySpace + ".versions"));
        try {
            var rowSet = await _session.ExecuteAsync(statement);
            var version = rowSet.GetRows().FirstOrDefault()!.GetValue<long>("version");
            return int.Parse(version.ToString());
        } catch (InvalidQueryException ex) when (ex.Message.Contains("table versions does not exist")) {
            return -1;
        }
    }
    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();
        var statement = new SimpleStatement(_dialect.HistoryQuery()
            .Replace("versions", _keySpace + ".versions")
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
    /// <inheritdoc/>
    public IDialect GetDialect() {
        return _dialect;
    }
    /// <summary>
    /// Disposes the database session
    /// </summary>
    public void Dispose() {
        _session.Dispose();
    }
}