internal class Scaffold(Dialect dialect)
{
    private const string startMigration = "--start migration";
    private const string scaffoldMigration = "-- SCAFFOLDING: Enter your migration queries here ..";
    private const string endMigration = "-- -- end migration";
    private const string startRollback = "--start rollback";
    private const string scaffoldRollback = "-- SCAFFOLDING: Enter your rollback queries here ..";
    private const string endRollback = "-- -- end rollback";
    private readonly List<string> scaffolding { get; init; };

    public Scaffold()
    {
        scaffolding = new List<string>() {
            startMigration,
            dialect.StartTransaction,
            scaffoldMigration,
            dialect.EndTransaction,
            endMigration,
            startRollback,
            dialect.EndTransaction,
            scaffoldRollback,
            endRollback
        };
    }

    public ScaffoldInit()
    {
        Scaffold();
        scaffolding[0] = dialect.MigrateVersionsTable;
        scaffolding[7] = dialect.RollbackVersionsTable;
    }

    public string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var line in Scaffolding)
        {
            stringBuilder.AppendLine(line);
        }
        return stringBuilder.ToString();
    }
}