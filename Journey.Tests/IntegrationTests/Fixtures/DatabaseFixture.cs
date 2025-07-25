using Journey.Interfaces;

public abstract class DatabaseFixture : IAsyncLifetime {
    public abstract IDatabase GetDatabase();
    public virtual string? GetSchema() => "public";
    public abstract Task InitializeAsync();

    public abstract Task DisposeAsync();
    public abstract string GetConnectionString();
    public virtual string GetValidQuery() => "CREATE TABLE test (column1 varchar(100) NOT NULL)";
    public virtual string GetInValidQuery() => "CREATE TABLES test (column1 varchar(100) NOT NULL)";
    public abstract Type GetDatabaseException();
    public virtual List<string> GetVersionEntries() => [
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
}