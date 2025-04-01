using System.Data.SQLite;

namespace Journey.Tests.IntegrationTests;

public class SqliteTest
{
    private readonly Sqlite _database = new();
    private readonly string _connectionString = "Data Source=:memory:";

    [Fact]
    public async Task TestConnect()
    {
        Assert.IsType<Sqlite>(await _database.Connect(_connectionString));
    }

    [Fact]
    public async Task TestGetCurrentVersionUninitialized()
    {
        await _database.Connect(_connectionString);
        Assert.Equal(-1, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestGetCurrentVersionInitialized()
    {
        await _database.Connect(_connectionString);
        await SetupVersionsTable();

        Assert.Equal(0, await _database.CurrentVersion());

        await _database.Execute("""
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (1, 'Testing version insert number one', 'me', 'you');
            """);

        Assert.Equal(1, await _database.CurrentVersion());

        await _database.Execute("""
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (2, 'Testing version insert number two', 'they', 'them');
            """);

        Assert.Equal(2, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestExecute()
    {
        await _database.Connect(_connectionString);
        var query = "CREATE TABLE test (column TEST)";
        await _database.Execute(query);
    }

    [Fact]
    public async Task TestExecuteThrows()
    {
        await _database.Connect(_connectionString);
        var query = "CREATE TABLES test (column TEST)";
        await Assert.ThrowsAsync<SQLiteException>(async () => await _database.Execute(query));
    }

    [Fact]
    public async Task TestGetItinerary()
    {
        await _database.Connect(_connectionString);
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
        Assert.Equal(now, history[0].RunTime, TimeSpan.FromSeconds(1));
        Assert.Equal("2", history[1].Version);
        Assert.Equal("Testing version insert number two", history[1].Description);
        Assert.Equal("they", history[1].RunBy);
        Assert.Equal("them", history[1].Author);
        Assert.Equal(now, history[1].RunTime, TimeSpan.FromSeconds(1));
    }

    private async Task SetupVersionsTable()
    => await _database.Execute(_database.GetDialect().MigrateVersionsTable());
}