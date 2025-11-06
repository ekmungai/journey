# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Journey is a script-based database migration tool that decouples code and database versioning. It treats migrations as a journey with atomic steps that can be applied or rolled back independently. The tool supports both SQL databases (Postgres/CockroachDB/TimescaleDB, MySQL/MariaDB, MSSQL, SQLite) and NoSQL databases (Cassandra).

## Build and Test Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Build Journey.Command CLI (standalone executable)
dotnet build Journey.Command/Journey.Command.csproj

# Build Journey.Net tool (dotnet global tool)
dotnet build Journey.Net/Journey.Net.csproj

# Run specific test project
dotnet test Journey.Tests/Journey.Tests.csproj
```

## Solution Structure

The solution contains 4 projects:

- **Journey** - Core library (targets net9.0 and net8.0)
- **Journey.Command** - Standalone CLI executable
- **Journey.Net** - .NET global tool
- **Journey.Tests** - xUnit test suite with Testcontainers for integration tests

## Core Architecture

### Facade Pattern Entry Point

`JourneyFacade` is the main entry point for consumers. It:
- Takes database type, connection string, versions directory, optional schema, and verbosity settings
- Initializes the appropriate `IDatabase` implementation based on database type
- Creates a `Migrator` with a `FileManager` and the selected database
- Exposes methods: `Init()`, `Migrate()`, `Rollback()`, `Update()`, `Scaffold()`, `Validate()`, `History()`
- Supports custom logging via `UseSerilogLogging()` or `UseMicrosoftLogging()`

### Key Components

**Migrator** (`Journey/Migrator.cs`)
- Orchestrates the migration process
- Maintains internal state: `_map` (available versions), `_currentVersion` (database version)
- Calculates migration routes between versions
- Handles the "travel" metaphor: moving through waypoints (versions) along a route

**Parser** (`Journey/Parser.cs`)
- Parses migration files using recursive descent parsing
- Validates file structure (migration/rollback sections, transactions, queries)
- Returns structured `Dictionary<string, List<string>>` with parsed queries
- Handles multi-line query blocks and comments
- Enforces format rules: sections must be properly opened/closed, transactions must be complete

**FileManager** (`Journey/FileManager.cs`)
- Abstracts file system operations using `System.IO.Abstractions`
- Manages version files (format: `{version}.sql`)
- Returns "map" of available versions from the versions directory

**Migration/Rollback Models** (`Journey/Models/`)
- `Migration` executes migration queries and can rollback via embedded `Rollback` instance
- `Rollback` reverses migration changes
- Both inherit from `DatabaseAction` which handles query execution

**Scaffold** (`Journey/Models/Scaffold.cs`)
- Generates migration file templates
- Includes header with formatting rules, migration/rollback sections with transaction blocks
- Creates special initialization scaffold (version 0) to set up the versions table

### Database Abstraction

**IDatabase Interface**
- Defines contract: `Connect()`, `Execute()`, `CurrentVersion()`, `GetItinerary()`, `GetDialect()`
- Each database implementation in `Journey/Databases/` implements this interface

**IDialect Interface and SqlDialect Base**
- Defines database-specific SQL syntax via abstract methods
- Each database has a corresponding dialect in `Journey/Dialects/`
- Dialects provide: `MigrateVersionsTable()`, `InsertVersion()`, `DeleteVersion()`, transaction delimiters, comment syntax, query terminators
- Pattern: Database class instantiates its dialect and uses it throughout operations

### Adding New Database Support

To add a new database:

1. Create database class in `Journey/Databases/` implementing `IDatabase`
   - Add static `Name` constant
   - Instantiate appropriate dialect
   - Implement connection, execution, version tracking, history methods

2. Create dialect class in `Journey/Dialects/` inheriting `SqlDialect` or `IDialect`
   - Override `MigrateVersionsTable()` with CREATE TABLE statement
   - Override `InsertVersion()` with INSERT statement
   - Override transaction methods if database uses non-standard syntax

3. Add case to `JourneyFacade.Init()` switch statement to instantiate your database

## Migration File Format

Migration files follow strict formatting enforced by the Parser:

```sql
-- Header with rules
-- start migration
BEGIN;
-- migration queries here
INSERT INTO versions (...) VALUES ([versionNumber], ...);
END;
-- end migration

-- start rollback
BEGIN;
-- rollback queries here
DELETE FROM versions WHERE version = [versionNumber];
END;
-- end rollback
```

- Files can contain multiple transactions per section (migration or rollback)
- Parser validates sections are opened/closed, transactions are complete
- Multi-line queries are supported (accumulated until terminator is found)
- Comments (starting with `--`) are skipped

## Testing

Tests use xUnit with Testcontainers for integration tests:

- **UnitTests/** - Test Parser, FileManager, Migrator, Scaffold in isolation
- **IntegrationTests/** - Test against real databases running in Docker containers
- **Fixtures/** - Database fixtures set up Testcontainers and provide test helpers
- Pattern: Each database has a `{Database}Fixture` and `{Database}Tests` class

Integration tests inherit from `GenericDbTest<TFixture>` which provides common test scenarios.

## Logging

Journey uses a custom `ILogger` interface (not Microsoft.Extensions.Logging):
- Default: `ConsoleLogger` writes to console
- Adapters: `SerilogLogger`, `MicrosoftLogger` wrap external logging frameworks
- Migrator uses logger for informational messages, errors, and debug output (when verbose mode enabled)

## Async Patterns

Journey is fully async with `AsyncHelper.RunSync()` utility for synchronous wrappers (e.g., `InitSync()`, `UpdateSync()`).
