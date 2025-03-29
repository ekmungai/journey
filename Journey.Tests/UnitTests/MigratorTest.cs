
using Moq;
using Moq.AutoMock;

namespace Journey.Tests.UnitTests;

public class MigratorTest
{
    private readonly IMigrator _migrator;
    private readonly AutoMocker _mocker = new(Moq.MockBehavior.Loose); // testing string is a pain >_<

    public MigratorTest()
    {
        _migrator = new Migrator(
            _mocker.GetMock<IFileManager>().Object,
            _mocker.GetMock<IDatabase>().Object);
    }

    [Fact]
    public async Task TestInit()
    {
        List<string> fragments = [
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************
            """,
            "-- start migration",
            "BEGIN;",
            """
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                author varchar(100)
            );
            """,
            "END;",
            "-- end migration",
            "-- start rollback",
            "BEGIN;",
            "DROP TABLE versions;",
            "END;",
            "-- end rollback",
            ];

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(0))
        .Returns(false);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.WriteFile(0, It.Is<string>(s =>
        AssertFragments(s, fragments))))
        .Returns(Task.CompletedTask);

        await _migrator.Init(true);
    }

    [Fact]
    public async Task TestMigrateNoTarget()
    {
        string[] content = [
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            """
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                author varchar(100)
            );
            """, "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE versions;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        string[] queries = [
            "BEGIN;",
            """
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(100) NOT NULL,
                author varchar(100)
            );
            """,
            "END;"
        ];

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([0]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(-1);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(0))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(0))
        .ReturnsAsync(content);

        SetupQuerys(queries);

        var result = await _migrator.Migrate(null);
        Assert.Equal($"The database was succesfully migrated to Version: 0{Environment.NewLine}", result);
    }

    private bool AssertFragments(string source, List<string> fragments)
    => fragments.TrueForAll(source.Contains);

    private void SetupQuerys(string[] queries)
    {
        var database = _mocker.GetMock<IDatabase>();
        foreach (var query in queries)
        {
            database.Setup(d => d.Execute(query.Trim()));
        }
    }
}