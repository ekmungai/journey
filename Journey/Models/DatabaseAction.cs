using Journey.Interfaces;

namespace Journey.Models;

/// Represents an action applied to the database 
public abstract record DatabaseAction(IDatabase database, Action<string> logger) : IExecutable {
    protected List<string> _queries = null!;
    protected readonly IDatabase _database = database;

    /// Executes the queries of the action on the database to apply it
    public async Task Execute(bool verbose) {
        foreach (var query in _queries) {
            if (verbose) {
                logger($"> {query}");
            }
            await database.Execute(query.Trim());
        }
    }
}