internal record Dialect(string schema = "public")
{
    public const string StartTransaction = "BEGIN;";
    public const string EndTransaction = "END;";
    public const string MigrateVersionsTable = $@"""
        CREATE TABLE IF NOT EXISTS {schema}.versions (
        run_time TIMESTAMPTZ NOT NULL,
        description varchar(100) NOT NULL,
        author varchar(100)
    );
    """;
    public const string RollbackVersionsTable = $"DROP TABLE {schema}.versions;";
}