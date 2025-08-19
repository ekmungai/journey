
using Journey.Interfaces;

namespace Journey.Tests.IntegrationTests;

public abstract class GenericDbTests<T>(T container) : IClassFixture<T> where T : DatabaseFixture, new() {
    private readonly IDatabase _database = container.GetDatabase();

    [Fact]
    public async Task TestGetCurrentVersionUninitialized() {
        await _database.Connect(container.GetConnectionString(), container.GetSchema()!);
        await ClearVersionsTable();
        Assert.Equal(-1, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestGetCurrentVersionInitialized() {
        await _database.Connect(container.GetConnectionString(), container.GetSchema()!);
        await ClearVersionsTable();
        await SetupVersionsTable();

        Assert.Equal(0, await _database.CurrentVersion());

        List<string> queries = container.GetVersionEntries();

        await _database.Execute(queries[0]);

        Assert.Equal(1, await _database.CurrentVersion());

        await _database.Execute(queries[1]);

        Assert.Equal(2, await _database.CurrentVersion());
    }

    [Fact]
    public async Task TestExecute() {
        await _database.Connect(container.GetConnectionString(), container.GetSchema()!);
        await _database.Execute(container.GetValidQuery());
    }

    [Fact]
    public async Task TestExecuteThrows() {
        await _database.Connect(container.GetConnectionString(), container.GetSchema()!);
        try {
            await _database.Execute(container.GetInValidQuery());
        } catch (Exception e) {
            Assert.Equal(e.GetType(), container.GetDatabaseException());
        }
    }

    [Fact]
    public async Task TestGetItinerary() {
        await _database.Connect(container.GetConnectionString(), container.GetSchema()!);
        await SetupVersionsTable();

        var queries = container.GetVersionEntries();

        queries.ForEach(async void (q) => await _database.Execute(q));
        //_database.Dispose();

        var time = 5;
        var history = await _database.GetItinerary(10);
        while (time > 0 && history.Count < 2) {
            history = await _database.GetItinerary(10);
            await Task.Delay(1000);
            time--;
        }

        Assert.Contains(history,
            h => h is { Version: "1", Description: "Testing version insert number one", RunBy: "me", Author: "you" });

        Assert.Contains(history,
            h => h is { Version: "2", Description: "Testing version insert number two", RunBy: "they", Author: "them" });
    }

    private async Task SetupVersionsTable() => await _database.Execute(_database.GetDialect().MigrateVersionsTable());
    private async Task ClearVersionsTable() {
        try {
            await _database.Execute(_database.GetDialect().RollbackVersionsTable());
        } catch {
            // ignored
        }
    }
}