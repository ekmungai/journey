using Journey.Interfaces;

namespace Journey.Models;

/// <inheritdoc cref="DatabaseAction" />
public record Migration : DatabaseAction, IReversible {
    private readonly Rollback _rollback;

    public Migration(IDatabase database, Dictionary<string, List<string>> sections, Action<string> logger) : base(database, logger) {
        Queries = sections[Parser.Migration];
        _rollback = new Rollback(database, sections, logger);
    }

    /// Executes the queries of the migration on the database to apply it
    public async Task Migrate(bool verbose) {
        await Execute(verbose);
    }

    /// <inheritdoc/>
    public async Task Rollback(bool verbose) => await _rollback.Reverse(verbose);
}