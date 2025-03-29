internal record SQliteDialect() : SqlDialect
{
    public override string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
                description TEXT NOT NULL,
                author TEXT
            );
            """;
}