using Journey.Databases;
using Journey.Interfaces;
using Npgsql;
using Testcontainers.PostgreSql;

public class TimescaleDbFixture : DatabaseFixture, IAsyncLifetime {
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("timescale/timescaledb:latest-pg14")
        .Build();

    public override IDatabase GetDatabase() => new TimescaleDb();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();
    public override Type GetDatabaseException() => typeof(PostgresException);
}