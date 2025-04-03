public abstract class DatabaseFixture : IAsyncLifetime
{
    public abstract IDatabase GetDatabase();
    public abstract Task InitializeAsync();

    public abstract Task DisposeAsync();

    public abstract string GetConnectionString();
    public abstract Type GetDatabaseException();
    public abstract List<string> GetVersionEntries();
}