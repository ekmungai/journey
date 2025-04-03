using Testcontainers.MySql;

public class MysqlFixture : DatabaseFixture, IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .Build();

    public override IDatabase GetDatabase() => new Mysql();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(MySqlConnector.MySqlException);

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