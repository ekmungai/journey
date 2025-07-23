
internal class Program {
    private static async Task<int> Main(string[] args) {

        var command = new JourneyCommand();

        return await command.Build(
            RunScaffold,
            RunMigrate,
            RunRollback,
            RunValidate,
            RunHistory,
            RunUpdate
        ).InvokeAsync(args);
    }

    private static async Task RunScaffold(string path, string connection, string database, string schema, bool quiet, bool verbose) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.Scaffold();
    }

    private static async Task RunMigrate(string path, string connection, string database, string schema, bool quiet, int? target, bool verbose, bool dryRun) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.Migrate(target, dryRun);
    }

    private static async Task RunRollback(string path, string connection, string database, string schema, bool quiet, int? target, bool verbose) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.Rollback(target);
    }

    private static async Task RunValidate(string path, string connection, string database, string schema, bool quiet, int? target, bool verbose) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.Validate(target ?? 0);
    }

    private static async Task RunHistory(string path, string connection, string database, string schema, bool quiet, int entries, bool verbose) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.History(entries);
    }

    private static async Task RunUpdate(string path, string connection, string database, string schema, bool quiet, int? target, bool verbose) {
        ValidateOptions(path);
        var journey = new JourneyFacade(database, connection, path, schema, verbose);
        await journey.Init(quiet);
        await journey.Update(target);
    }

    private static void ValidateOptions(string path) {
        if (!Directory.Exists(path)) {
            throw new DirectoryNotFoundException("Versions directory is invalid");
        }
    }
}
