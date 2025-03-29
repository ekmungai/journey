internal record SqlDialect() : IDialect
{
    public string StartTransaction() => "BEGIN;";
    public string EndTransaction() => "END;";
    public string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                author varchar(100)
            );
            """;
    public string RollbackVersionsTable() => "DROP TABLE versions;";
    public string CurrentVersionQuery() => "SELECT COUNT(*) as version FROM versions;";

    public string Comment() => "--";

    public string Terminator() => ";";
}