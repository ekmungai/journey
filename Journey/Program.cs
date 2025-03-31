using CommandLine;
using OptionParser = CommandLine.Parser;

var options = (Options)OptionParser.Default.ParseArguments<
    ValidateOptions,
    ScaffoldOptions,
    MigrateOptions,
    HistoryOptions,
    RollbackOptions>(args)
  .WithParsed<ValidateOptions>(ValidateOptions.RunOptions)
  .WithParsed<ScaffoldOptions>(ScaffoldOptions.RunOptions)
  .WithParsed<MigrateOptions>(MigrateOptions.RunOptions)
  .WithParsed<MigrateOptions>(HistoryOptions.RunOptions)
  .WithParsed<RollbackOptions>(RollbackOptions.RunOptions)
  .WithNotParsed(Options.HandleParseError).Value;

IDatabase database;
database = options.Database switch
{
    "sqlite" => await new Sqlite().Connect(options.Connection),
    _ => await new Sqlite().Connect(options.Connection),
};

var migrator = new Migrator(new FileManager(options.VersionsDir), database);
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