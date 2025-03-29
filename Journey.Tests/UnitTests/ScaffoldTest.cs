using Moq.AutoMock;

namespace Journey.Tests.UnitTests;

public class ScaffoldTest
{
    private readonly IDatabase _database;
    private readonly AutoMocker _mocker = new(Moq.MockBehavior.Strict);
    public ScaffoldTest()
    {
        _database = _mocker.GetMock<IDatabase>().Object;
        _mocker.GetMock<IDatabase>()
        .Setup(d => d.GetDialect())
        .Returns(new SQliteDialect());
    }

    [Fact]
    public void TestSqlDialectScaffold()
    {
        // Arrange
        var scaffold = new Scaffold(_database.GetDialect());

        // Act
        var output = scaffold.ToString();

        // Assert
        #region migration
        Assert.Contains("Migration file formatting rules.", output);
        Assert.Contains("BEGIN;", output);
        Assert.Contains("-- start migration", output);
        Assert.Contains("-- SCAFFOLDING: Enter your migration queries here ..", output);
        Assert.Contains("END;", output);
        Assert.Contains("-- end migration", output);
        #endregion

        #region rollback
        Assert.Contains("BEGIN;", output);
        Assert.Contains("-- start rollback", output);
        Assert.Contains("-- SCAFFOLDING: Enter your rollback queries here ..", output);
        Assert.Contains("END;", output);
        Assert.Contains("-- end rollback", output);
        #endregion
    }

    [Fact]
    public void TestSqliteInitScaffold()
    {
        // Arrange
        var scaffold = new Scaffold(_database.GetDialect());
        scaffold.ScaffoldInit();

        // Act
        var output = scaffold.ToString();

        // Assert
        #region migration
        Assert.Contains("Migration file formatting rules.", output);
        Assert.Contains("BEGIN;", output);
        Assert.Contains("-- start migration", output);
        Assert.Contains("""
            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
                description TEXT NOT NULL,
                author TEXT
            );
            """, output);
        Assert.Contains("END;", output);
        Assert.Contains("-- end migration", output);
        #endregion

        #region rollback
        Assert.Contains("BEGIN;", output);
        Assert.Contains("-- start rollback", output);
        Assert.Contains("DROP TABLE versions;", output);
        Assert.Contains("END;", output);
        Assert.Contains("-- end rollback", output);
        #endregion
    }
}
