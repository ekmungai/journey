using Testcontainers.PostgreSql;

public class TimescaleDbFixture : PostgresFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("timescale/timescaledb:latest")
        .Build();

    public override IDatabase GetDatabase() => new TimescaleDb();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();
}