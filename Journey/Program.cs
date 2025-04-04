using System.Diagnostics.CodeAnalysis;
using CommandLine;
using OptionParser = CommandLine.Parser;

[ExcludeFromCodeCoverage]
internal class Program {
    private static async Task Main(string[] args) {
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

        var journey = new JourneyFacade(
            options.Database,
            options.Connection,
            options.VersionsDir,
            options.Schema
        );
        await journey.Init(options.Quiet);

        Console.WriteLine(
            options switch {
                ValidateOptions => await journey.Validate(options.Target ?? 0),
                ScaffoldOptions => await journey.Scaffold(),
                MigrateOptions => await journey.Migrate(options.Target, options.Debug, options.DryRun),
                RollbackOptions => await journey.Rollback(options.Target, options.Debug),
                HistoryOptions => await journey.History(options.Entries),
                UpdateOptions => await journey.Update(options.Debug),
                _ => throw new InvalidOperationException(),
            }
        );
    }
}