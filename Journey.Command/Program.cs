using CommandLine;
using OptionParser = CommandLine.Parser;

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
        .WithParsed<RollbackOptions>(Options.RunOptions)
        .WithParsed<UpdateOptions>(Options.RunOptions)
        .WithNotParsed(Options.HandleParseError).Value;

        var journey = new JourneyFacade(
            options.Database,
            options.Connection,
            options.VersionsDir,
            options.Schema,
            null,
            options.Verbose
        );
        await journey.Init(options.Quiet);

        switch (options) {
            case ValidateOptions:
                await journey.Validate(options.Target ?? 0);
                break;
            case ScaffoldOptions:
                await journey.Scaffold();
                break;
            case MigrateOptions:
                await journey.Migrate(options.Target ?? 0, options.DryRun);
                break;
            case RollbackOptions:
                await journey.Rollback(options.Target ?? 0);
                break;
            case HistoryOptions:
                await journey.History(options.Entries);
                break;
            case UpdateOptions:
                await journey.Update(options.Target ?? 0);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}