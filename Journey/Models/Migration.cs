/// <inheritdoc/>
public record Migration : DatabaseAction, IReversible {
    private readonly Rollback _rollback;

    public Migration(IDatabase database, Dictionary<string, List<string>> sections, Action<string> logger) : base(database, logger) {
        _queries = sections[Parser.Migration];
        _rollback = new Rollback(database, sections, logger);
    }

    /// <inheritdoc/>
    public async Task Migrate() {
        await Execute();
    }

    /// <inheritdoc/>
    public async Task Rollback() => await _rollback.Reverse();
}