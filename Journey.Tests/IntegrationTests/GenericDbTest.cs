
namespace Journey.Tests.IntegrationTests;

public abstract class GenericDbTests<T>(T _container) : IClassFixture<T> where T : DatabaseFixture, new()
{
    private readonly IDatabase _database = _container.GetDatabase();

    [Fact]
    public async Task TestGetCurrentVersionUninitialized()
    {
        await _database.Connect(_container.GetConnectionString());
        await ClearVersionsTable();
        Assert.Equal(-1, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestGetCurrentVersionInitialized()
    {
        await _database.Connect(_container.GetConnectionString());
        await ClearVersionsTable();
        await SetupVersionsTable();

        Assert.Equal(0, await _database.CurrentVersion());

        List<string> queries = _container.GetVersionEntries();

        await _database.Execute(queries[0]);

        Assert.Equal(1, await _database.CurrentVersion());

        await _database.Execute(queries[1]);

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
        try
        {
            await _database.Execute(query);
        }
        catch (Exception e)
        {
            Assert.Equal(e.GetType(), _container.GetDatabaseException());
        }
    }

    [Fact]
    public async Task TestGetItinerary()
    {
        await _database.Connect(_container.GetConnectionString());
        await SetupVersionsTable();
        var now = DateTime.UtcNow;
        List<string> queries = _container.GetVersionEntries();

        queries.ForEach(async q => await _database.Execute(q));
        _database.Dispose();

        var time = 5;
        var history = await _database.GetItinerary(10);
        while (time > 0 && history.Count < 2)
        {
            history = await _database.GetItinerary(10);
            await Task.Delay(1000);
            time--;
        }

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
    private async Task ClearVersionsTable()
    {
        try
        {
            await _database.Execute(_database.GetDialect().RollbackVersionsTable());
        }
        catch { }
    }
}