public interface IDialect
{
    public string Terminator();
    public string Comment();
    public string StartTransaction();
    public string EndTransaction();
    public string MigrateVersionsTable();
    public string RollbackVersionsTable();
    public string InsertVersion();
    public string DeleteVersion();
}