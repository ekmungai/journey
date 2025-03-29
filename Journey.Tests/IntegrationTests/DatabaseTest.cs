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
}