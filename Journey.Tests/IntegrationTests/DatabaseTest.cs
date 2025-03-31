using System.Data.SQLite;

namespace Journey.Tests.IntegrationTests;

public class DatabaseTest
{
    private readonly IDatabase _database = new Sqlite();
    private readonly string _connectionString = "Data Source=:memory:";

    [Fact]
    public async Task TestConnect()
    {
        Assert.IsType<Sqlite>(await _database.Connect(_connectionString));
    }

    [Fact]
    public async Task TestGetCurrentVersion()
    {
        await _database.Connect(_connectionString);
        Assert.Equal(-1, await _database.CurrentVersion());
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
            VALUES (1, 'Testing version insert number two', 'they', 'them');
            """
        ];

        queries.ForEach(async q => await _database.Execute(q));
        var history = await _database.GetItinerary(10);
        Assert.Equal("1", history[0].Version);
    }

    private async Task SetupVersionsTable() => await _database.Execute(
        """
        CREATE TABLE IF NOT EXISTS versions (
            version INTEGER NOT NULL,
            run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
            description TEXT NOT NULL,
            run_by TEXT NOT NULL,
            author TEXT NOT NULL
        );
        """);
}