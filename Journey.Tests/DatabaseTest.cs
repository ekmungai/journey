namespace Journey.Tests;

public class DatabaseTest
{
    private readonly IDatabase _database = new Sqlite();
    private readonly string _connectionString = "Data Source=:memory:";

    [Fact]
    public void TestConnect()
    {
        Assert.IsType<Sqlite>(_database.Connect(_connectionString));
    }

    [Fact]
    public async Task TestGetCurrentVersion()
    {
        _database.Connect(_connectionString);
        Assert.Equal(0, await _database.CurrentVersion());
    }

    [Fact]
    public void TestExecute()
    {
        Assert.True(true);
    }
}