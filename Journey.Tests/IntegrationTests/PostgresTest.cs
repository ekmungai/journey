using System.Data.SQLite;
using Testcontainers.PostgreSql;

namespace Journey.Tests.IntegrationTests;

public class PostgresTest : IAsyncLifetime
{
    private readonly Postgres _database = new Postgres();
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
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
        Assert.IsType<Postgres>(await _database.Connect(_container.GetConnectionString()));
    }

    [Fact]
    public async Task TestGetCurrentVersion()
    {
        await _database.Connect(_container.GetConnectionString(), "public");
        Assert.Equal(-1, await _database.CurrentVersion());
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
        await Assert.ThrowsAsync<Npgsql.PostgresException>(async () => await _database.Execute(query));
    }

    [Fact]
    public async Task TestGetItinerary()
    {
        await _database.Connect(_container.GetConnectionString(), "public");
        await SetupVersionsTable();
        var now = DateTime.UtcNow;
        List<string> queries = [
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

        queries.ForEach(async q => await _database.Execute(q));
        var history = await _database.GetItinerary(10);
        Assert.Equal("1", history[0].Version);
        Assert.Equal("Testing version insert number one", history[0].Description);
        Assert.Equal("me", history[0].RunBy);
        Assert.Equal("you", history[0].Author);
        Assert.Equal(now, history[0].RunTime, TimeSpan.FromSeconds(10));
        if (history.Count > 1)
        {
            Assert.Equal("2", history[1].Version);
            Assert.Equal("Testing version insert number two", history[1].Description);
            Assert.Equal("they", history[1].RunBy);
            Assert.Equal("them", history[1].Author);
            Assert.Equal(now, history[1].RunTime, TimeSpan.FromSeconds(10));
        }

    }

    private async Task SetupVersionsTable() => await _database.Execute(
        """
        CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW(),
                description varchar(100) NOT NULL,
                run_by varchar(100) NOT NULL,
                author varchar(100) NOT NULL
            );
        """);
}