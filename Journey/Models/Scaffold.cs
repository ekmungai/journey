
using System.Text;
internal class Scaffold
{
    private const string _header = """
        ------------------------------------------------------------------
        -- | Migration file formatting rules.                               |
        -- | 1. There must be one and only one migration and one and only   |
        -- |    one rollback section.                                       |
        -- | 2. Only change the section between transaction blocks.         | 
        -- | 3. Each migration and rollback must have only one transaction. |                                       |
        -- ******************************************************************
        """;
    private const string _startMigration = "start migration";
    private const string _scaffoldMigration = "SCAFFOLDING: Enter your migration queries here ..";
    private const string _endMigration = "end migration";
    private const string _startRollback = "start rollback";
    private const string _scaffoldRollback = "SCAFFOLDING: Enter your rollback queries here ..";
    private const string _endRollback = "end rollback";
    private readonly IDialect _dialect;

    private List<string> _scaffolding { get; set; }

    public List<string> Scaffolding { get { return _scaffolding; } }


    public Scaffold(IDialect dialect, int? version)
    {
        _scaffolding = [.. new List<string>() {
            _header,
            _startMigration,
            _scaffoldMigration,
            _endMigration,
            _startRollback,
            _scaffoldRollback,
            _endRollback
        }.Select(s => dialect.Comment() + " " + s)];

        _scaffolding.Insert(2, dialect.StartTransaction());
        _scaffolding.Insert(4, dialect.InsertVersion().Replace("[versionNumber]", version.ToString()));
        _scaffolding.Insert(5, dialect.EndTransaction());
        _scaffolding.Insert(8, dialect.StartTransaction());
        _scaffolding.Insert(10, dialect.DeleteVersion().Replace("[versionNumber]", version.ToString()));
        _scaffolding.Insert(11, dialect.EndTransaction());
        _dialect = dialect;
    }

    public void ScaffoldInit()
    {
        _scaffolding[3] = _dialect.MigrateVersionsTable();
        _scaffolding.RemoveAt(4);
        _scaffolding[8] = _dialect.RollbackVersionsTable();
        _scaffolding.RemoveAt(9);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var line in _scaffolding)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(line);
        }
        return stringBuilder.ToString().Trim();
    }
}