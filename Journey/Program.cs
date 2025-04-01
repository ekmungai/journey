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
            RollbackOptions,
            UpdateOptions>(args)
        .WithParsed<ValidateOptions>(Options.RunOptions)
        .WithParsed<ScaffoldOptions>(Options.RunOptions)
        .WithParsed<MigrateOptions>(Options.RunOptions)
        .WithParsed<MigrateOptions>(Options.RunOptions)
        .WithParsed<RollbackOptions>(Options.RunOptions)
        .WithParsed<UpdateOptions>(Options.RunOptions)
        .WithNotParsed(Options.HandleParseError).Value;

        IDatabase database;
        database = options.Database switch
        {
            "sqlite" => await new Sqlite().Connect(options.Connection),
            "postgres" => await new Postgres().Connect(options.Connection, options.Schema),
            "mysql" => await new Mysql().Connect(options.Connection, options.Schema),
            _ => await new Sqlite().Connect(options.Connection),
        };

        var migrator = new Migrator(new FileManager(options.VersionsDir, new FileSystem()), database);
        await migrator.Init(options.Quiet);

        var result = options switch
        {
            ValidateOptions => await migrator.Validate(options.Target ?? 0),
            ScaffoldOptions => await migrator.Scaffold(),
            MigrateOptions => await migrator.Migrate(options.Target, options.Debug),
            RollbackOptions => await migrator.Rollback(options.Target, options.Debug),
            HistoryOptions => await migrator.History(options.Entries),
            UpdateOptions => await migrator.Update(options.Debug),
            _ => throw new InvalidOperationException(),
        };
        Console.WriteLine(result);
        database.Dispose();
    }
}