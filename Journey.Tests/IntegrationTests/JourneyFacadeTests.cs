using System.Data.SQLite;
using System.IO.Abstractions.TestingHelpers;

namespace Journey.Tests.IntegrationTests;

public class JourneyFacadeTest {
    private readonly JourneyFacade _journeyFacade;
    private readonly string _versionsDir = "versions";
    private readonly string[] versions = [
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************

            -- start migration

            BEGIN;

            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
                description TEXT NOT NULL,
                run_by TEXT NOT NULL,
                author TEXT NOT NULL
            );

            END;

            -- end migration

            -- start rollback

            BEGIN;

            DROP TABLE versions;

            END;

            -- end rollback
            """,
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************

            -- start migration

            BEGIN;

            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (1, 'Testing version insert', 'me', 'you');

            END;

            -- end migration

            -- start rollback

            BEGIN;

            DELETE FROM versions WHERE version = 1;

            END;

            -- end rollback
            """,
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************

            -- start migration

            BEGIN;

            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (2, 'Testing version insert number two', 'they', 'them');

            END;

            -- end migration

            -- start rollback

            BEGIN;

            DELETE FROM versions WHERE version = 2;

            END;

            -- end rollback
            """,
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************

            -- start migration

            BEGIN;

            -- SCAFFOLDING: Enter your migration queries here ..

            INSERT INTO versions (
                version,
                description,
                run_by,
                author)
            VALUES (3, 'Testing version insert number three', 'they', 'them');

            END;

            -- end migration

            -- start rollback

            BEGIN;

            -- SCAFFOLDING: Enter your rollback queries here ..

            DELETE FROM versions WHERE version = 3;

            END;

            -- end rollback
            """
        ];

    public JourneyFacadeTest() {
        _journeyFacade = new JourneyFacade(
            "sqlite",
            "Data Source=:memory:",
            _versionsDir,
            null
        );
    }

    [Fact]
    public async Task TestValidateValidFile() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));

        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Validate(0);
        Assert.Contains($"{Environment.NewLine}File for version 0 is valid with the queries: {Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestValidateInvalidFile() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(
            """
            -- ------------------------------------------------------------------
            -- | Migration file formatting rules.                               |
            -- | 1. There must be one and only one migration and one and only   |
            -- |    one rollback section.                                       |
            -- | 2. Only change the section between transaction blocks.         | 
            -- | 3. Each migration and rollback must have only one transaction. |                                       |
            -- ******************************************************************

            -- start migration

            BEGIN;

            CREATE TABLE IF NOT EXISTS versions (
                version INTEGER NOT NULL,
                run_time TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,
                description TEXT NOT NULL,
                run_by TEXT NOT NULL,
                author TEXT NOT NULL
            );

            BEGIN;
            END;

            -- end migration

            -- start rollback

            BEGIN;

            DROP TABLE versions;

            END;

            -- end rollback
            """
            ));

        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Validate(0);
        Assert.Contains($"File for version 0 is invalid with error: 'The migration file is malformed at: BEGIN;'", result);
    }

    [Fact]
    public async Task TestMigrateSingleStep() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Migrate(null, false, false);
        Assert.Equal($"{Environment.NewLine}The database was succesfully migrated to version: 0{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestMigrateMultipleSteps() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));

        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Migrate(2, false, false);
        Assert.Equal($"{Environment.NewLine}The database was succesfully migrated to version: 2{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestMigrateUpToDateDatabase() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var result = await _journeyFacade.Migrate(0, false, false);
        Assert.Equal($"{Environment.NewLine}The database is up to date at Version: 0{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestDryRunMigrateSingleStep() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Migrate(null, false, true);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestDryRunMigrateMultipleSteps() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));

        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Migrate(2, false, true);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestMissingMigrationFileThrows() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData("queries"));
        await _journeyFacade.Init(true, _fileSystem);
        var ex = await Assert.ThrowsAsync<MissingMigrationFileException>(async () => await _journeyFacade.Migrate(1, false, false));
        Assert.Equal("Migration file for version 1 was not found", ex.Message);
    }

    [Fact]
    public async Task TestLowerVersionMigrationThrows() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));

        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Update(false);

        var ex = await Assert.ThrowsAsync<InvalidMigrationException>(async () => await _journeyFacade.Migrate(1, false, false));
        Assert.Equal("Cannot migrate to a lower version. Target: 1 < Current: 2", ex.Message);
    }

    [Fact]
    public async Task TestHistoryDefaultEntries() {
        var now = DateTimeOffset.UtcNow;
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));

        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var history = await _journeyFacade.History(10);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {now} \t| Testing version insert \t| me \t| you", history);
        Assert.Contains($"2 \t| {now} \t| Testing version insert number two \t| they \t| them", history);
    }

    [Fact]
    public async Task TestHistoryLimitedEntries() {
        var now = DateTimeOffset.UtcNow;
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var history = await _journeyFacade.History(1);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {now} \t| Testing version insert \t| me \t| you", history);
        Assert.DoesNotContain($"2 \t| {now} \t| Testing version insert number two \t| they \t| them", history);
    }

    [Fact]
    public async Task TestUpdateMigrateDatabase() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(versions[3]));
        await _journeyFacade.Init(true, _fileSystem);
        var result = await _journeyFacade.Update(false);
        Assert.Equal($"{Environment.NewLine}The database was succesfully migrated to version: 3{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestRollbackSingleStep() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Migrate(null, false, false);
        var result = await _journeyFacade.Rollback(null, false);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestRollbackMultipleSteps() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(versions[2]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(versions[3]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var result = await _journeyFacade.Rollback(-1, false);
        Assert.Equal($"{Environment.NewLine}The database was succesfully rolled back to version: -1{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestRollbackUpToDateDatabase() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var result = await _journeyFacade.Rollback(0, false);
        Assert.Equal($"{Environment.NewLine}The database is up to date at Version: 0{Environment.NewLine}", result);
    }

    [Fact]
    public async Task TestHigherVersionRollbackThrows() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(versions[0]));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(versions[1]));
        await _journeyFacade.Init(true, _fileSystem);
        await _journeyFacade.Update(false);

        var ex = await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _journeyFacade.Rollback(2, false));
        Assert.Equal("Cannot rollback to a higher version. Target: 2 > Current: 1", ex.Message);
    }

    [Fact]
    public async Task TestImpossibleRollbackThrows() {
        var _fileSystem = new MockFileSystem();
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData("content"));
        await _journeyFacade.Init(true, _fileSystem);
        await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _journeyFacade.Rollback(null, false));
    }

}