internal record SQliteDialect() : SqlDialect
{
    public override string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
                description TEXT NOT NULL,
                run_by TEXT NOT NULL,
                author TEXT NOT NULL
            );
            """;
    public override string InsertVersion() => """
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES ([versionNumber], '', '', '');
            """;

    public override string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber];";

}