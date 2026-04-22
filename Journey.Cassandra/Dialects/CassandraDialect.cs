namespace Journey.Dialects;

/// <inheritdoc/>
internal record CassandraDialect : SqlDialect {
    /// <inheritdoc/>
    public override string MigrateVersionsTable() => """
                                                     CREATE TABLE IF NOT EXISTS versions (
                                                         version int,
                                                         run_time timestamp,
                                                         description text,
                                                         run_by text,
                                                         author text,
                                                         PRIMARY KEY (version)
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
    public override string StartTransaction() => "";
    /// <inheritdoc/>
    public override string HistoryQuery() => "SELECT * FROM versions LIMIT [entries];";
    /// <inheritdoc/>
    public override string[] EndTransaction() => [""];
    /// The SQL query for creating a key space in cassandra
    public string CreateKeySpace() => """
                                      CREATE KEYSPACE IF NOT EXISTS [key_space]
                                          WITH REPLICATION = { 
                                          'class' : 'SimpleStrategy', 
                                          'replication_factor' : 1 
                                          };
                                      """;
}