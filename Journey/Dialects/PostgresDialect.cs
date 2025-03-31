internal record PostgresDialect() : SqlDialect
{
    public override string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                run_by varchar(100) NOT NULL,
                author varchar(100) NOT NULL
            );
            """;
    public override string InsertVersion() => """
            INSERT INTO versions (
                version,
                run_time,
                description,
                run_by,
                author)
            VALUES ("", "", "", "", ""));
            """;

    public override string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber]";
}