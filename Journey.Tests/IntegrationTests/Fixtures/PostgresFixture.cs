using Npgsql;
using Testcontainers.PostgreSql;

public class PostgresFixture : DatabaseFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .Build();

    public override IDatabase GetDatabase() => new Postgres();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(PostgresException);

    public override List<string> GetVersionEntries() => [
            """
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (1, 'Testing version insert number one', 'me', 'you');
            """,
            """
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (2, 'Testing version insert number two', 'they', 'them');
            """
        ];
}