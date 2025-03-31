using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using CommandLine;
using OptionParser = CommandLine.Parser;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static async Task Main(string[] args)
    {
        var options = (Options)OptionParser.Default.ParseArguments<
            ValidateOptions,
            ScaffoldOptions,
            MigrateOptions,
            HistoryOptions,
            RollbackOptions>(args)
        .WithParsed<ValidateOptions>(Options.RunOptions)
        .WithParsed<ScaffoldOptions>(Options.RunOptions)
        .WithParsed<MigrateOptions>(Options.RunOptions)
        .WithParsed<MigrateOptions>(Options.RunOptions)
        .WithParsed<RollbackOptions>(Options.RunOptions)
        .WithNotParsed(Options.HandleParseError).Value;

        IDatabase database;
        database = options.Database switch
        {
            "sqlite" => await new Sqlite().Connect(options.Connection),
            "postgres" => await new Postgres().Connect(options.Connection, options.Schema),
            _ => await new Sqlite().Connect(options.Connection),
        };

        var migrator = new Migrator(new FileManager(options.VersionsDir, new FileSystem()), database);
        var currentVersion = await database.CurrentVersion();

        if (currentVersion == -1)
        {
            await migrator.Init(!string.IsNullOrWhiteSpace(options.Quiet));
        }
        var result = options switch
        {
            ValidateOptions => await migrator.Validate(options.Target ?? 0),
            ScaffoldOptions => await migrator.Scaffold(currentVersion + 1),
            MigrateOptions => await migrator.Migrate(options.Target),
            RollbackOptions => await migrator.Rollback(options.Target),
            HistoryOptions => await migrator.History(options.Entries),
            _ => throw new InvalidOperationException(),
        };
        Console.WriteLine(result);
        database.Dispose();
    }
}