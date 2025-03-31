
using Moq;
using Moq.AutoMock;

namespace Journey.Tests.UnitTests;

public class MigratorTest
{
    private readonly IMigrator _migrator;
    private readonly AutoMocker _mocker = new(MockBehavior.Loose); // testing strings is such a pain >_<

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
    public async Task TestMigrateSingleStep()
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

        SetupQueries(queries);

        var result = await _migrator.Migrate(null);
        Assert.Equal($"{Environment.NewLine}The database was succesfully migrated to Version: 0{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestMigrateMultipleSteps()
    {
        string[] migration1 = [
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

        string[] migration2 = [
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
            "CREATE TABLE test (column TEST);", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE test;", "",
            "END;", "",
            "-- end rollback", "",
            ];

        string[] migration3 = [
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
            "ALTER TABLE test ADD COLUMN (column2 TEST);", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "ALTER TABLE test DROP COLUMN (column2 TEST);", "",
            "END;", "",
            "-- end rollback", "",
            ];

        string[] migration1Queries = [
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

        string[] migration2Queries = [
            "BEGIN;",
            "CREATE TABLE test (column TEST);",
            "END;"
        ];

        string[] migration3Queries = [
            "BEGIN;",
            "ALTER TABLE test ADD COLUMN (column2 TEST);",
            "END;"
        ];

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([0, 1, 2]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(-1);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(0))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(1))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(2))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(0))
        .ReturnsAsync(migration1);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(1))
        .ReturnsAsync(migration2);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(2))
        .ReturnsAsync(migration3);

        SetupQueries(migration1Queries);
        SetupQueries(migration2Queries);
        SetupQueries(migration3Queries);

        var result = await _migrator.Migrate(2);
        Assert.Equal($"{Environment.NewLine}The database was succesfully migrated to Version: 2{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestLowerVersionMigrationThrows()
    {
        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(9);

        var ex = await Assert.ThrowsAsync<InvalidMigrationException>(async () => await _migrator.Migrate(4));
        Assert.Equal("Cannot migrate to a lower version. Target: 4 < Current: 9", ex.Message);
    }

    [Fact]
    public async Task TestHistoryDefaultEntries()
    {
        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        var timeOne = DateTimeOffset.Now.AddDays(-7);
        var timeTwo = DateTimeOffset.Now;

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetItinerary(10))
        .ReturnsAsync([
            new Itinerary("1", "Test Migration", timeOne, "me", "you"),
            new Itinerary("2", "Test Migration 2", timeTwo, "they", "them"),
        ]);


        var history = await _migrator.History(10);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {timeOne} \t| Test Migration \t| me \t| you", history);
        Assert.Contains($"2 \t| {timeTwo} \t| Test Migration 2 \t| they \t| them", history);
    }

    [Fact]
    public async Task TestHistoryLimitedEntries()
    {
        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        var timeOne = DateTimeOffset.Now.AddDays(-7);
        var timeTwo = DateTimeOffset.Now;

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetItinerary(1))
        .ReturnsAsync([
            new Itinerary("1", "Test Migration", timeOne, "me", "you"),
        ]);

        var history = await _migrator.History(1);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {timeOne} \t| Test Migration \t| me \t| you", history);
        Assert.DoesNotContain($"2 \t| {timeTwo} \t| Test Migration 2 \t| they \t| them", history);
    }

    [Fact]
    public async Task TestRollbackSingleStep()
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
            DROP TABLE versions;
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
        .ReturnsAsync(0);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(0))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(0))
        .ReturnsAsync(content);

        SetupQueries(queries);

        var result = await _migrator.Rollback(null);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to Version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestRollbackMultipleSteps()
    {
        string[] migration1 = [
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

        string[] migration2 = [
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
            "CREATE TABLE test (column TEST);", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE test;", "",
            "END;", "",
            "-- end rollback", "",
            ];

        string[] migration3 = [
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
            "ALTER TABLE test ADD COLUMN (column2 TEST);", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "ALTER TABLE test DROP COLUMN (column2 TEST);", "",
            "END;", "",
            "-- end rollback", "",
            ];

        string[] migration1Queries = [
            "BEGIN;",
            """
            DROP TABLE versions;
            """,
            "END;"
        ];

        string[] migration2Queries = [
            "BEGIN;",
            "DROP TABLE test;",
            "END;"
        ];

        string[] migration3Queries = [
            "BEGIN;",
            "ALTER TABLE test DROP COLUMN (column2 TEST);",
            "END;"
        ];

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([0, 1, 2]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(2);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(0))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(1))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.FileExists(2))
        .Returns(true);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(0))
        .ReturnsAsync(migration1);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(1))
        .ReturnsAsync(migration2);

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.ReadFile(2))
        .ReturnsAsync(migration3);

        SetupQueries(migration1Queries);
        SetupQueries(migration2Queries);
        SetupQueries(migration3Queries);

        var result = await _migrator.Rollback(-1);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to Version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestHigherVersionRollbackThrows()
    {
        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(1);

        var ex = await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _migrator.Rollback(2));
        Assert.Equal("Cannot rollback to a higher version. Target: 2 > Current: 1", ex.Message);
    }

    [Fact]
    public async Task TestImpossibleRollbackThrows()
    {
        _mocker.GetMock<IDatabase>()
        .Setup(m => m.GetDialect())
        .Returns(new SQliteDialect());

        _mocker.GetMock<IFileManager>()
        .Setup(m => m.GetMap())
        .Returns([]);

        _mocker.GetMock<IDatabase>()
        .Setup(m => m.CurrentVersion())
        .ReturnsAsync(-1);

        await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _migrator.Rollback(null));
    }

    private bool AssertFragments(string source, List<string> fragments)
    => fragments.TrueForAll(source.Contains);

    private void SetupQueries(string[] queries)
    {
        var database = _mocker.GetMock<IDatabase>();
        foreach (var query in queries)
        {
            database.Setup(d => d.Execute(query.Trim()));
        }
    }
}