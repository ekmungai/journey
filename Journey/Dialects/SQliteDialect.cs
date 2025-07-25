namespace Journey.Dialects;

/// <inheritdoc/>
internal record SQliteDialect : SqlDialect {
    /// <inheritdoc/>
    public override string MigrateVersionsTable() => """
                                                     CREATE TABLE IF NOT EXISTS versions (
                                                         version INTEGER NOT NULL,
                                                         run_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                                         description TEXT NOT NULL,
                                                         run_by TEXT NOT NULL,
                                                         author TEXT NOT NULL
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
    public override string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber];";

}