using System.Data.SqlClient;
using Testcontainers.MsSql;

namespace Journey.Tests.IntegrationTests;

public class MssqlTest : IAsyncLifetime
{
    private readonly Mssql _database = new();
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .Build();


    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task TestConnect()
    {
        Assert.IsType<Mssql>(await _database.Connect(_container.GetConnectionString()));
    }

    [Fact]
    public async Task TestGetCurrentVersionUninitialized()
    {
        await _database.Connect(_container.GetConnectionString());
        Assert.Equal(-1, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestGetCurrentVersionInitialized()
    {
        await _database.Connect(_container.GetConnectionString());
        await SetupVersionsTable();

        Assert.Equal(0, await _database.CurrentVersion());

        await _database.Execute("""
            INSERT INTO Versions (
                 Version,
                Description,
                RunBy,
                Author)
            VALUES (1, 'Testing version insert number one', 'me', 'you');
            """);

        Assert.Equal(1, await _database.CurrentVersion());

        await _database.Execute("""
            INSERT INTO versions (
                 Version,
                Description,
                RunBy,
                Author)
            VALUES (2, 'Testing version insert number two', 'they', 'them');
            """);

        Assert.Equal(2, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestExecute()
    {
        await _database.Connect(_container.GetConnectionString());
        var query = "CREATE TABLE test (column1 varchar(100) NOT NULL)";
        await _database.Execute(query);
    }

    [Fact]
    public async Task TestExecuteThrows()
    {
        await _database.Connect(_container.GetConnectionString());
        var query = "CREATE TABLES test (column1 varchar(100) NOT NULL)";
        await Assert.ThrowsAsync<SqlException>(async () => await _database.Execute(query));
    }

    [Fact]
    public async Task TestGetItinerary()
    {
        await _database.Connect(_container.GetConnectionString());
        await SetupVersionsTable();
        var now = DateTime.UtcNow;
        List<string> queries = [
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

        queries.ForEach(async q => await _database.Execute(q));
        _database.Dispose();

        await Task.Delay(5000);
        await _database.Connect(_container.GetConnectionString());
        var history = await _database.GetItinerary(10);
        Assert.Equal("1", history[0].Version);
        Assert.Equal("Testing version insert number one", history[0].Description);
        Assert.Equal("me", history[0].RunBy);
        Assert.Equal("you", history[0].Author);
        Assert.Equal(now, history[0].RunTime, TimeSpan.FromSeconds(10));
        Assert.Equal("2", history[1].Version);
        Assert.Equal("Testing version insert number two", history[1].Description);
        Assert.Equal("they", history[1].RunBy);
        Assert.Equal("them", history[1].Author);
        Assert.Equal(now, history[1].RunTime, TimeSpan.FromSeconds(10));
    }

    private async Task SetupVersionsTable()
    => await _database.Execute(_database.GetDialect().MigrateVersionsTable());
}