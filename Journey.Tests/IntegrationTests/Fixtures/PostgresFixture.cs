using Npgsql;
using Testcontainers.PostgreSql;

public class PostgresFixture : DatabaseFixture, IAsyncLifetime {
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .Build();

    public override IDatabase GetDatabase() => new Postgres();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => $"{_container.GetConnectionString()};CommandTimeout=100";

    public override Type GetDatabaseException() => typeof(PostgresException);
}