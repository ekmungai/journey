using Testcontainers.MariaDb;

public class MariaDbFixture : MysqlFixture, IAsyncLifetime
{
    private readonly MariaDbContainer _container = new MariaDbBuilder()
     .Build();

    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

}