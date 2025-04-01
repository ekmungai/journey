using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
internal class Options
{
    // Required options
    [Option('p', "path", Required = true, HelpText = "The path to the versions directory.")]
    public required string VersionsDir { get; init; }

    [Option('c', "connection", Required = true, HelpText = "The connection string for connecting to the database")]
    public required string Connection { get; init; }

    [Option('d', "database", HelpText = "The type of the database to migrate.")]
    public required string Database { get; init; }

    // optional options
    [Option('s', "schema", Default = "public", HelpText = "The schema to apply the migration to.")]
    public string Schema { get; init; }

    [Option('q', "quiet", Default = false, HelpText = "Run the action without prompts.")]
    public bool Quiet { get; init; }

    [Option('t', "target", HelpText = "The target version.")]
    public int? Target { get; init; }

    [Option('e', "entries", Default = 10, HelpText = "The number of versions back to retieve.")]
    public int Entries { get; init; }

    [Option('b', "debug", Default = false, HelpText = "Print out migration queries as they are executed.")]
    public bool Debug { get; init; }

    public static void RunOptions(Options opts)
    {
        if (!Directory.Exists(opts.VersionsDir))
        {
            throw new DirectoryNotFoundException("Versions directory is invalid");
        }
    }

    public static void HandleParseError(IEnumerable<Error> errs)
    {
        Environment.Exit(-1);
    }
}