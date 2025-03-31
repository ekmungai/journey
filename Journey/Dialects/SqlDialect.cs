internal abstract record SqlDialect() : IDialect
{
    public string StartTransaction() => "BEGIN;";
    public string EndTransaction() => "END;";
    public abstract string MigrateVersionsTable();
    public abstract string InsertVersion();
    public abstract string DeleteVersion();
    public string RollbackVersionsTable() => "DROP TABLE versions;";
    public string CurrentVersionQuery() => "SELECT COUNT(*) as version FROM versions;";
    public string HistoryQuery() => "SELECT * FROM versions ORDER BY version ASC LIMIT [entries];";
    public string Comment() => "--";
    public string Terminator() => ";";
}