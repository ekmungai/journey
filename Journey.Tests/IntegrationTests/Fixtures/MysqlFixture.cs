using Journey.Databases;
using Journey.Interfaces;
using Testcontainers.MySql;

public class MysqlFixture : DatabaseFixture, IAsyncLifetime {
    private readonly MySqlContainer _container = new MySqlBuilder()
        .Build();

    public override IDatabase GetDatabase() => new Mysql();
    public override string? GetSchema() => null;
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(MySqlConnector.MySqlException);
}