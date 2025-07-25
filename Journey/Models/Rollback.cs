using Journey.Interfaces;

namespace Journey.Models;

/// <inheritdoc/>
public record Rollback : DatabaseAction {
    public Rollback(IDatabase database, Dictionary<string, List<string>> sections, Action<string> logger) : base(database, logger) {
        _queries = sections[Parser.Rollback];
    }

    /// Executes the queries of the rollback on the database to apply it
    public async Task Reverse(bool verbose) {
        await Execute(verbose);
    }
}