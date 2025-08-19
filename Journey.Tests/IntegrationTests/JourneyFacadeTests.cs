using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Journey.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Journey.Tests.IntegrationTests;

public class JourneyFacadeTest : IDisposable {
    private readonly JourneyFacade _journeyFacade;
    private readonly string _versionsDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../../versions";
    private readonly AutoMocker _mocker = new(MockBehavior.Loose); // testing strings is such a pain >_<
    private readonly string[] _versions = [
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
            null,
            true
        );
    }

    [Fact]
    public async Task TestSerilogLoggerInformation() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<Serilog.ILogger>().Object;
        _journeyFacade.UseSerilogLogging(logger);

        _mocker.GetMock<Serilog.ILogger>()
            .Setup(m => m.Information(It.Is<string>(
                log => log.Contains("{message}")
            ), It.Is<string>(
                log => log.Contains("File for version 0 is valid with the queries:")
            )));

        Assert.True(await _journeyFacade.Validate(0));
    }

    [Fact]
    public async Task TestSerilogLoggerError() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(
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
            
            BEGIN;

            DELETE FROM versions WHERE version = 1;

            END;

            -- end rollback
            """
        ));

        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<Serilog.ILogger>().Object;
        _journeyFacade.UseSerilogLogging(logger);
        _mocker.GetMock<Serilog.ILogger>()
            .Setup(m => m.Error(It.IsAny<InvalidFormatException>(),
                It.Is<string>(
                log => log.Contains("File for version 1 is invalid with error: 'The migration file is malformed at: BEGIN;'")
                )));

        Assert.False(await _journeyFacade.Validate(1));
    }

    [Fact]
    public async Task TestSerilogLoggerDebug() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<Serilog.ILogger>().Object;
        _journeyFacade.UseSerilogLogging(logger);

        _mocker.GetMock<Serilog.ILogger>()
            .Setup(m => m.Debug(It.Is<string>(
                log => log.Contains("> BEGIN;")
            )));

        await _journeyFacade.Migrate(null, false);

        await AssertDatabaseVersion(1);
    }

    [Fact]
    public async Task TestMicrosoftLoggerInformation() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<ILogger>().Object;
        _journeyFacade.UseMicrosoftLogging(logger);

        Assert.True(await _journeyFacade.Validate(0));

        // Cannot verify extension methods, so we check the contents of the invocations list
        var invocation = _mocker.GetMock<ILogger>()
            .Invocations[0];
        Assert.Equal(LogLevel.Information, invocation.Arguments[0]);
    }

    [Fact]
    public async Task TestMicrosoftLoggerError() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));

        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(
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

            BEGIN;

            DELETE FROM versions WHERE version = 1;

            END;

            -- end rollback
            """
        ));

        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<ILogger>().Object;
        _journeyFacade.UseMicrosoftLogging(logger);

        Assert.False(await _journeyFacade.Validate(1));

        // Cannot verify extension methods, so we check the contents of the invocations list
        var invocation = _mocker.GetMock<ILogger>()
            .Invocations[0];
        Assert.Equal(LogLevel.Error, invocation.Arguments[0]);
    }

    [Fact]
    public async Task TestMicrosoftLoggerDebug() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(_versionsDir);
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);

        var logger = _mocker.GetMock<ILogger>().Object;
        _journeyFacade.UseMicrosoftLogging(logger);

        await _journeyFacade.Migrate(null, false);

        // Cannot verify extension methods, so we check the contents of the invocations list
        var invocation = _mocker.GetMock<ILogger>()
            .Invocations[1];
        Assert.Equal(LogLevel.Debug, invocation.Arguments[0]);
    }

    [Fact]
    public async Task TestValidateValidFile() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));

        await _journeyFacade.Init(true, fileSystem);
        Assert.True(await _journeyFacade.Validate(0));
    }

    [Fact]
    public async Task TestValidateInvalidFile() {
        var fileSystem = new MockFileSystem();
        
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(
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

            BEGIN;

            DELETE FROM versions WHERE version = 1;

            END;

            -- end rollback
            """
        ));

        await _journeyFacade.Init(true, fileSystem);

        Assert.False(await _journeyFacade.Validate(1));
    }

    [Fact]
    public async Task TestMigrateSingleStep() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Migrate(1, false);

        await AssertDatabaseVersion(1);
    }

    [Fact]
    public async Task TestMigrateMultipleSteps() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Migrate(2, false);

        await AssertDatabaseVersion(2);
    }

    [Fact]
    public async Task TestMigrateUpToDateDatabase() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        await _journeyFacade.Migrate(0, false);

        await AssertDatabaseVersion(0);
    }

    [Fact]
    public async Task TestDryRunMigrateSingleStep() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Migrate(null, true);

        await AssertDatabaseVersion(0);
    }

    [Fact]
    public async Task TestDryRunMigrateMultipleSteps() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Migrate(2, true);

        await AssertDatabaseVersion(0);
    }

    [Fact]
    public async Task TestMissingMigrationFileThrows() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);
        var ex = await Assert.ThrowsAsync<MissingMigrationFileException>(async () => await _journeyFacade.Migrate(1, false));
        Assert.Equal("Migration file for version 1 was not found", ex.Message);
    }

    [Fact]
    public async Task TestLowerVersionMigrationThrows() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        var ex = await Assert.ThrowsAsync<InvalidMigrationException>(async () => await _journeyFacade.Migrate(1, false));
        Assert.Equal("Cannot migrate to a lower version. Target: 1 < Current: 2", ex.Message);
    }

    [Fact]
    public async Task TestHistoryDefaultEntries() {
        var now = DateTimeOffset.UtcNow;
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        var history = await _journeyFacade.History(10);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {now} \t| Testing version insert \t| me \t| you", history);
        Assert.Contains($"2 \t| {now} \t| Testing version insert number two \t| they \t| them", history);
    }

    [Fact]
    public async Task TestHistoryLimitedEntries() {
        var now = DateTimeOffset.UtcNow;
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        var history = await _journeyFacade.History(1);
        Assert.Contains($"{Environment.NewLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author", history);
        Assert.Contains($"1 \t| {now} \t| Testing version insert \t| me \t| you", history);
        Assert.DoesNotContain($"2 \t| {now} \t| Testing version insert number two \t| they \t| them", history);
    }

    [Fact]
    public async Task TestUpdateDatabaseUpgradeToLatest() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(_versions[3]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        await AssertDatabaseVersion(3);
    }

    [Fact]
    public async Task TestUpdateDatabaseUpgradeToTarget() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(_versions[3]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(2);

        await AssertDatabaseVersion(2);
    }

    [Fact]
    public async Task TestUpdateDatabaseDowngradeToTarget() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(_versions[3]));

        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        await _journeyFacade.Update(2);

        await AssertDatabaseVersion(2);
    }

    [Fact]
    public async Task TestRollbackSingleStep() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Migrate(null, false);
        await _journeyFacade.Rollback(null);

        await AssertDatabaseVersion(0);
    }

    [Fact]
    public async Task TestRollbackMultipleSteps() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData(_versions[2]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "3.sql"), new MockFileData(_versions[3]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        await _journeyFacade.Rollback(-1);

        await AssertDatabaseVersion(-1);
    }

    [Fact]
    public async Task TestRollbackUpToDateDatabase() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        await _journeyFacade.Rollback(0);

        await AssertDatabaseVersion(0);
    }

    [Fact]
    public async Task TestHigherVersionRollbackThrows() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData(_versions[1]));
        await _journeyFacade.Init(true, fileSystem);
        await _journeyFacade.Update(null);

        var ex = await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _journeyFacade.Rollback(2));
        Assert.Equal("Cannot rollback to a higher version. Target: 2 > Current: 1", ex.Message);
    }

    [Fact]
    public async Task TestImpossibleRollbackThrows() {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData(_versions[0]));
        await _journeyFacade.Init(true, fileSystem);
        await Assert.ThrowsAsync<InvalidRollbackException>(async () => await _journeyFacade.Rollback(1));
    }

    private async Task AssertDatabaseVersion(int version) {
        var db = _journeyFacade.GetDatabase();
        var newVersion = await db.CurrentVersion();
        Assert.Equal(version, newVersion);
    }

    public void Dispose() {
        _journeyFacade.Dispose();
        _mocker.VerifyAll();
    }
}