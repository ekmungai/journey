using System.CommandLine;

namespace Journey.Command;

internal class Command {
    private readonly RootCommand _root = new("Journey - Database Migration Tool");
    private readonly System.CommandLine.Command _scaffold;
    private readonly System.CommandLine.Command _migrate;
    private readonly System.CommandLine.Command _rollback;
    private readonly System.CommandLine.Command _validate;
    private readonly System.CommandLine.Command _history;
    private readonly System.CommandLine.Command _update;
    private readonly Option<string> _path = new Option<string>(
        aliases: ["-p", "--path"],
        description: "The path to the versions directory."
    ) { IsRequired = true };
    private readonly Option<string> _connection = new Option<string>(
        aliases: ["-c", "--connection"],
        description: "The connection string for connecting to the database"
    ) { IsRequired = true };
    private readonly Option<string> _database = new Option<string>(
        aliases: ["-d", "--database"],
        description: "The type of the database to migrate."
    ) { IsRequired = true };
    private readonly Option<string> _schema = new Option<string>(
        aliases: ["-s", "--schema"],
        description: "The schema to apply the migration to.",
        getDefaultValue: () => "public"
    );
    private readonly Option<bool> _quiet = new Option<bool>(
        aliases: ["-q", "--quiet"],
        description: "Run the action without prompts.",
        getDefaultValue: () => false
    );
    private readonly Option<int> _entries = new Option<int>(
        aliases: ["-e", "--entries"],
        description: "The number of versions back to retrieve.",
        getDefaultValue: () => 10
    );
    private readonly Option<int?> _target = new Option<int?>(
        aliases: ["-t", "--target"],
        description: "The target version."
    );
    private readonly Option<bool> _verbose = new Option<bool>(
        aliases: ["-v", "--verbose"],
        description: "Print out migration queries as they are executed.",
        getDefaultValue: () => false
    );
    private readonly Option<bool> _dryRun = new Option<bool>(
        aliases: ["-r", "--dry-run"],
        description: "Immediately rollback migrations after they have been executed.",
        getDefaultValue: () => false
    );
    public Command() {
        _scaffold = new System.CommandLine.Command("scaffold", "Scaffold the next version migration file."){
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _verbose
        };
        _migrate = new System.CommandLine.Command("migrate", "Migrate the database to a target version.") {
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _target,
            _verbose,
            _dryRun
        };
        _rollback = new System.CommandLine.Command("rollback", "Rollback the database to a target version.") {
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _target,
            _verbose
        };
        _validate = new System.CommandLine.Command("validate", "Validate the migration files.") {
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _target,
            _verbose
        };
        _history = new System.CommandLine.Command("history", "Show migration history.") {
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _entries,
            _verbose
        };
        _update = new System.CommandLine.Command("update", "Update the database to the latest version.")
        {
            _path,
            _connection,
            _database,
            _schema,
            _quiet,
            _target,
            _verbose
        };
    }

    public Command Build(
        Func<string, string, string, string, bool, bool, Task> scaffoldHandler,
        Func<string, string, string, string, bool, int?, bool, bool, Task> migrateHandler,
        Func<string, string, string, string, bool, int?, bool, Task> rollbackHandler,
        Func<string, string, string, string, bool, int?, bool, Task> validateHandler,
        Func<string, string, string, string, bool, int, bool, Task> historyHandler,
        Func<string, string, string, string, bool, int?, bool, Task> updateHandler
    ) {

        //Set Handlers
        _scaffold.SetHandler(scaffoldHandler, _path, _connection, _database, _schema, _quiet, _verbose);
        _migrate.SetHandler(migrateHandler, _path, _connection, _database, _schema, _quiet, _target, _verbose, _dryRun);
        _rollback.SetHandler(rollbackHandler, _path, _connection, _database, _schema, _quiet, _target, _verbose);
        _validate.SetHandler(validateHandler, _path, _connection, _database, _schema, _quiet, _target, _verbose);
        _history.SetHandler(historyHandler, _path, _connection, _database, _schema, _quiet, _entries, _verbose);
        _validate.SetHandler(validateHandler, _path, _connection, _database, _schema, _quiet, _target, _verbose);
        _update.SetHandler(updateHandler, _path, _connection, _database, _schema, _quiet, _target, _verbose);

        // Add Commands
        _root.AddCommand(_scaffold);
        _root.AddCommand(_migrate);
        _root.AddCommand(_rollback);
        _root.AddCommand(_validate);
        _root.AddCommand(_history);
        _root.AddCommand(_update);
        return this;
    }

    public async Task<int> InvokeAsync(string[] args) {
        return await _root.InvokeAsync(args);
    }
}