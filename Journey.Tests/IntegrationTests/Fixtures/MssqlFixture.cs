using System.Data.SqlClient;
using Journey.Databases;
using Journey.Interfaces;
using Testcontainers.MsSql;

public class MssqlFixture : DatabaseFixture, IAsyncLifetime {
    private readonly MsSqlContainer _container = new MsSqlBuilder()
       .Build();

    public override IDatabase GetDatabase() => new Mssql();
    public override Task InitializeAsync() => _container.StartAsync();

    public override Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public override string GetConnectionString() => _container.GetConnectionString();

    public override Type GetDatabaseException() => typeof(SqlException);

    public override List<string> GetVersionEntries() => [
            """
            INSERT INTO Versions (
                Version,
                Description,
                RunBy,
                Author)
            VALUES (1, 'Testing version insert number one', 'me', 'you');
            """,
            """
            INSERT INTO versions (
                Version,
                Description,
                RunBy,
                Author)
            VALUES (2, 'Testing version insert number two', 'they', 'them');
            """
        ];
}