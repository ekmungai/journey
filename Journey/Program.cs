using CommandLine;
using OptionParser = CommandLine.Parser;

var options = (Options)OptionParser.Default.ParseArguments<
    ValidateOptions,
    ScaffoldOptions,
    MigrateOptions,
    RollbackOptions>(args)
  .WithParsed<ValidateOptions>(ValidateOptions.RunOptions)
  .WithParsed<ScaffoldOptions>(ScaffoldOptions.RunOptions)
  .WithParsed<MigrateOptions>(MigrateOptions.RunOptions)
  .WithParsed<RollbackOptions>(RollbackOptions.RunOptions)
  .WithNotParsed(Options.HandleParseError).Value;

IDatabase database;
database = options.Database switch
{
    "sqlite" => new Sqlite().Connect(options.Connection),
    "postgres" => new Postgres(options.Schema!).Connect(options.Connection),
    _ => new Sqlite().Connect(options.Connection),
};

var migrator = new Migrator(new FileManager(options.VersionsDir), database);
var currentVersion = await database.CurrentVersion();

if (currentVersion == 0)
{
    await migrator.Init(!string.IsNullOrWhiteSpace(options.Quiet));
}
var result = options switch
{
    ValidateOptions => await migrator.Validate(options.Target ?? 0),
    ScaffoldOptions => await migrator.Scaffold(currentVersion + 1),
    MigrateOptions => await migrator.Scaffold(currentVersion + 1),
    RollbackOptions => await migrator.Scaffold(currentVersion + 1),
    _ => throw new InvalidOperationException(),
};
Console.WriteLine(result);