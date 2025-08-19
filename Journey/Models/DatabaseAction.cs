using Journey.Interfaces;

namespace Journey.Models;

/// Represents an action applied to the database 
public abstract record DatabaseAction(IDatabase Database, Action<string> Logger) : IExecutable {
    protected List<string> Queries = null!;

    /// Executes the queries of the action on the database to apply it
    public async Task Execute(bool verbose) {
        foreach (var query in Queries) {
            if (verbose) {
                Logger($"> {query}");
            }
            await Database.Execute(query.Trim());
        }
    }
}