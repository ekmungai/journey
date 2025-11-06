using System.Text;
using Journey.Interfaces;

namespace Journey.Models;

/// <summary>
/// Represents the structure of an empty migration file, ready to be filled with 
/// queries that make changes to the database.
/// </summary>
internal record Scaffold {
    private const string Header = """
                                   ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
                                   """;
    private const string StartMigration = "start migration";
    private const string ScaffoldMigration = "SCAFFOLDING: Enter your migration queries here ..";
    private const string EndMigration = "end migration";
    private const string StartRollback = "start rollback";
    private const string ScaffoldRollback = "SCAFFOLDING: Enter your rollback queries here ..";
    private const string EndRollback = "end rollback";
    private readonly IDialect _dialect;

    private List<string> Scaffolding { get; }

    public List<string> GetScaffolding() => Scaffolding;


    /// <summary>
    /// Prepares the sections of an ordinary migration file.
    /// </summary>
    /// <param name="dialect">The dialect for which the migration should be scaffolded.</param>
    /// <param name="version">The version of the migration should be scaffolded.</param>
    public Scaffold(IDialect dialect, int? version) {
        Scaffolding = [.. new List<string> {
            Header,
            StartMigration,
            ScaffoldMigration,
            EndMigration,
            StartRollback,
            ScaffoldRollback,
            EndRollback
        }.Select(s => dialect.Comment() + " " + s)];

        dialect.DeleteVersion();
        Scaffolding.Insert(2, dialect.StartTransaction());
        Scaffolding.Insert(4, dialect.InsertVersion().Replace("[versionNumber]", version.ToString()));
        Scaffolding.Insert(5, dialect.EndTransaction()[0]);
        Scaffolding.Insert(8, dialect.StartTransaction());
        Scaffolding.Insert(10, dialect.DeleteVersion().Replace("[versionNumber]", version.ToString()));
        Scaffolding.Insert(11, dialect.EndTransaction()[0]);
        _dialect = dialect;
    }

    /// <summary>
    /// Scaffold for the first migration file, which prepares the database for use with the journey tool.
    /// </summary>
    public void ScaffoldInit() {
        Scaffolding[3] = _dialect.MigrateVersionsTable();
        Scaffolding.RemoveAt(4);
        Scaffolding[8] = _dialect.RollbackVersionsTable();
        Scaffolding.RemoveAt(9);
    }

    /// <summary>
    /// Represents the contents of the scaffold as a string.
    /// </summary>
    /// <returns>A string representation of the contents of the file to be scaffolded.</returns>
    public override string ToString() {
        var stringBuilder = new StringBuilder();

        foreach (var line in Scaffolding) {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(line);
        }
        return stringBuilder.ToString().Trim();
    }
}