using Journey.Dialects;
using Journey.Exceptions;

namespace Journey.Tests.UnitTests;

public class ParserTest {

    [Fact]
    public void TestParseSingleTransactionFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            """
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(1000) NOT NULL,
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
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();
        var printOut = parser.ToString();
        Assert.True(result.ContainsKey("Migration"));
        Assert.True(result.ContainsKey("Rollback"));
        Assert.Equal(3, result["Migration"].Count);
        Assert.Equal(3, result["Rollback"].Count);
        Assert.Contains("BEGIN;", result["Migration"]);
        Assert.Contains("""
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(1000) NOT NULL,
                author varchar(100)
            );
            """, result["Migration"]);
        Assert.Contains("END;", result["Migration"]);
        Assert.Contains("BEGIN;", result["Rollback"]);
        Assert.Contains("DROP TABLE versions;", result["Rollback"]);
        Assert.Contains("END;", result["Rollback"]);
        Assert.Contains(
            """
            Migration
            ----------------
            BEGIN;
            """, printOut);
        Assert.Contains(
            """
            Rollback
            ----------------
            BEGIN;
            """, printOut);
    }

    [Fact]
    public void TestParseMultipleTransactionsFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            """
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(1000) NOT NULL,
                author varchar(100)
            );
            """, "",
            "END;", "",
            "BEGIN;", "",
            "ALTER TABLE versions ADD COLUMN commit varchar(100);", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "ALTER TABLE versions DROP COLUMN commit;", "",
            "END;", "",
            "BEGIN;", "",
            "DROP TABLE versions;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();
        var printOut = parser.ToString();
        Assert.True(result.ContainsKey("Migration"));
        Assert.True(result.ContainsKey("Rollback"));
        Assert.Equal(6, result["Migration"].Count);
        Assert.Equal(6, result["Rollback"].Count);
        Assert.Contains("BEGIN;", result["Migration"]);
        Assert.Contains(
            "ALTER TABLE versions ADD COLUMN commit varchar(100);",
            result["Migration"]
        );
        Assert.Contains("""
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(1000) NOT NULL,
                author varchar(100)
            );
            """, result["Migration"]);
        Assert.Contains("END;", result["Migration"]);
        Assert.Contains("BEGIN;", result["Rollback"]);
        Assert.Contains("DROP TABLE versions;", result["Rollback"]);
        Assert.Contains("ALTER TABLE versions DROP COLUMN commit;", result["Rollback"]);
        Assert.Contains("END;", result["Rollback"]);
        Assert.Contains(
            """
            Migration
            ----------------
            BEGIN;
            """, printOut);
        Assert.Contains(
            """
            Rollback
            ----------------
            BEGIN;
            """, printOut);
    }

    [Fact]
    public void TestParseDefaultSectionOrderFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();
        Assert.True(result.ContainsKey("Migration"));
        Assert.True(result.ContainsKey("Rollback"));
        Assert.Equal(2, result["Migration"].Count);
        Assert.Equal(2, result["Rollback"].Count);
    }

    [Fact]
    public void TestParseReversedSectionOrderFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();
        Assert.True(result.ContainsKey("Migration"));
        Assert.True(result.ContainsKey("Rollback"));
        Assert.Equal(2, result["Migration"].Count);
        Assert.Equal(2, result["Rollback"].Count);
    }

    [Fact]
    public void TestParseFileMissingMigration() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var ex = Assert.Throws<MissingSectionException>(() => new Parser(0, content, new SQliteDialect()));
        Assert.Equal("The migration file for version 0 is missing a Migration section", ex.Message);
    }

    [Fact]
    public void TestParseFileMissingRollback() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            ];

        var ex = Assert.Throws<MissingSectionException>(() => new Parser(0, content, new SQliteDialect()));
        Assert.Equal("The migration file for version 0 is missing a Rollback section", ex.Message);
    }

    [Fact]
    public void TestParseOpenSectionFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var ex = Assert.Throws<OpenSectionException>(() => new Parser(0, content, new SQliteDialect()));
        Assert.Equal("The migration file for version 0 has an unclosed Migration section", ex.Message);
    }

    [Fact]
    public void TestParseOpenTransactionFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            """
                CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMPTZ DEFAULT NOW() NOT NULL,
                description varchar(1000) NOT NULL,
                author varchar(100)
            );
            """, "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE versions;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        Assert.Throws<OpenTransactionException>(parser.ParseFile);
    }

    [Fact]
    public void TestParseInvalidFormatMissingMigrationBeginFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var parser = new Parser(0, content, new SQliteDialect());
        var ex = Assert.Throws<InvalidFormatException>(parser.ParseFile);
        Assert.Equal("The migration file for version 0 is malformed at: END;", ex.Message);
    }

    [Fact]
    public void TestParseInvalidFormatMissingRollbackBeginFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var parser = new Parser(0, content, new SQliteDialect());
        var ex = Assert.Throws<InvalidFormatException>(parser.ParseFile);
        Assert.Equal("The migration file for version 0 is malformed at: END;", ex.Message);
    }

    [Fact]
    public void TestParseInvalidFormatNestedMigrationTransactionFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "BEGIN;", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var parser = new Parser(0, content, new SQliteDialect());
        var ex = Assert.Throws<InvalidFormatException>(parser.ParseFile);
        Assert.Equal("The migration file for version 0 is malformed at: BEGIN;", ex.Message);
    }

    [Fact]
    public void TestParseInvalidFormatNestedRollbackTransactionFile() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your migration queries here ..", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "-- SCAFFOLDING: Enter your rollback queries here ..", "",
            "BEGIN;", "",
            "END;", "",
            "-- end rollback", "",
            ];

        var parser = new Parser(0, content, new SQliteDialect());
        var ex = Assert.Throws<InvalidFormatException>(parser.ParseFile);
        Assert.Equal("The migration file for version 0 is malformed at: BEGIN;", ex.Message);
    }

    [Fact]
    public void TestParseSemicolonInTrailingComment() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "CREATE TABLE IF NOT EXISTS chart_definitions (",
            "    id TEXT PRIMARY KEY,",
            "    kind VARCHAR(16) NOT NULL,   -- 'chart' | 'tile'; never changes for a slug",
            "    slug TEXT NOT NULL",
            ");", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE chart_definitions;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();

        Assert.Equal(3, result["Migration"].Count);
        var statement = result["Migration"][1];
        Assert.Contains("slug TEXT NOT NULL", statement);
        Assert.EndsWith(");", statement);
    }

    [Fact]
    public void TestParseSemicolonInCommentInsideStatement() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "CREATE TABLE IF NOT EXISTS audit (",
            "    id TEXT PRIMARY KEY,",
            "    -- one row per change; rows are never updated",
            "    changed_at TEXT NOT NULL",
            ");", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DROP TABLE audit;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();

        Assert.Equal(3, result["Migration"].Count);
        var statement = result["Migration"][1];
        Assert.Contains("changed_at TEXT NOT NULL", statement);
        Assert.EndsWith(");", statement);
    }

    [Fact]
    public void TestParseSemicolonInStringLiteral() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "INSERT INTO versions (version, description, run_by, author)",
            "VALUES (1, 'charts; then tiles', 'me', 'you');", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "DELETE FROM versions WHERE version = 1;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();

        Assert.Equal(3, result["Migration"].Count);
        var statement = result["Migration"][1];
        Assert.Contains("'charts; then tiles'", statement);
        Assert.EndsWith(");", statement);
    }

    [Fact]
    public void TestParseSemicolonInBlockComment() {
        string[] content = [
            """
            ------------------------------------------------------------------
                                   -- | Migration file formatting rules.                               |
                                   -- | 1. There must be one and only one migration and one and only   |
                                   -- |    one rollback section.                                       |
                                   -- | 2. Apart from the default transaction, you can add as many     |
                                   -- | others as you need.                                            | 
                                   -- | 3. The two sections and all transactions must be properly      |
                                   -- | closed.                                                        |
                                   -- ******************************************************************
            """, "",
            "-- start migration", "",
            "BEGIN;", "",
            "/* the tile kind was dropped in v2;",
            "   see the migration notes */",
            "ALTER TABLE chart_definitions ADD COLUMN theme TEXT;", "",
            "END;", "",
            "-- end migration", "",
            "-- start rollback", "",
            "BEGIN;", "",
            "ALTER TABLE chart_definitions DROP COLUMN theme;", "",
            "END;", "",
            "-- end rollback", "",
            ];
        var parser = new Parser(0, content, new SQliteDialect());
        parser.ParseFile();
        var result = parser.GetResult();

        Assert.Equal(3, result["Migration"].Count);
        var statement = result["Migration"][1];
        Assert.Contains("ALTER TABLE chart_definitions ADD COLUMN theme TEXT;", statement);
    }
}
