using Journey.Databases;
using Journey.Interfaces;
using Npgsql;
using Testcontainers.CockroachDb;

public class CockroachDbFixture : DatabaseFixture, IAsyncLifetime
{
    private readonly CockroachDbContainer _container = new CockroachDbBuilder()
        .Build();

    public override IDatabase GetDatabase() => new CockroachDb();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(PostgresException);
}