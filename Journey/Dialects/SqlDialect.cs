internal abstract record SqlDialect() : IDialect {
    public abstract string MigrateVersionsTable();
    public abstract string InsertVersion();
    public virtual string StartTransaction() => "BEGIN;";
    public virtual string EndTransaction() => "END;";
    public virtual string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber]";
    public virtual string RollbackVersionsTable() => "DROP TABLE versions;";
    public virtual string CurrentVersionQuery() => "SELECT COUNT(*) as version FROM versions;";
    public virtual string HistoryQuery() => "SELECT * FROM versions ORDER BY version LIMIT [entries];";
    public string Comment() => "--";
    public string Terminator() => ";";
}