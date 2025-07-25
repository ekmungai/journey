using Cassandra;
using Journey.Databases;
using Journey.Interfaces;
using Testcontainers.Cassandra;

public class CassandraFixture : DatabaseFixture, IAsyncLifetime {
    private readonly CassandraContainer _container = new CassandraBuilder()
        .Build();

    public override IDatabase GetDatabase() => new CassandraDb();
    public override string GetValidQuery() => $"CREATE TABLE {GetSchema()}.test (column1 text PRIMARY KEY)";
    public override string GetInValidQuery() => $"CREATE TABLE {GetSchema()}.test (column1 text)";
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(InvalidQueryException);
    public override List<string> GetVersionEntries() {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return
        [
            $"""
            INSERT INTO versions (
                version,
                run_time,
                description,
                run_by,
                author)
            VALUES (1, '{now}', 'Testing version insert number one', 'me', 'you');
            """,
            $"""
            INSERT INTO versions (
                version,
                run_time,
                description,
                run_by,
                author)
            VALUES (2, '{now}','Testing version insert number two', 'they', 'them');
            """
        ];
    }
}