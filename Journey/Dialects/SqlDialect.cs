internal abstract record SqlDialect() : IDialect
{
    public string StartTransaction() => "BEGIN;";
    public string EndTransaction() => "END;";
    public abstract string MigrateVersionsTable();
    public string RollbackVersionsTable() => "DROP TABLE versions;";
    public string CurrentVersionQuery() => "SELECT COUNT(*) as version FROM versions;";
    public string Comment() => "--";
    public string Terminator() => ";";
}