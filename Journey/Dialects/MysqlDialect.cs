/// <inheritdoc/>
internal record MysqlDialect() : SqlDialect {
    /// <inheritdoc/>
    public override string MigrateVersionsTable() => """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                description varchar(1000) NOT NULL,
                run_by varchar(100) NOT NULL,
                author varchar(100) NOT NULL
            );
            """;
    /// <inheritdoc/>
    public override string InsertVersion() => """
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES ([versionNumber], '', '', '');
            """;

    /// <inheritdoc/>
    public override string EndTransaction() => "COMMIT;";
}