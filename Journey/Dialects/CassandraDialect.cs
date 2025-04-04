internal record CassandraDialect() : SqlDialect {
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
    public override string InsertVersion() => """
            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES ([versionNumber], '', '', '');
            """;
    public override string StartTransaction() => "";
    public override string HistoryQuery() => "SELECT * FROM versions LIMIT [entries];";

    public override string EndTransaction() => "";

    public string CreateKeySpace() => """
            CREATE KEYSPACE IF NOT EXISTS [key_space]
                WITH REPLICATION = { 
                'class' : 'SimpleStrategy', 
                'replication_factor' : 1 
                };
            """;
}