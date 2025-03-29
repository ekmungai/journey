internal record PostgresDialect() : SqlDialect
{
    public override string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                author varchar(100)
            );
            """;
}