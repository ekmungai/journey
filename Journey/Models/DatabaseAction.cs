/// <inheritdoc/>
public abstract record DatabaseAction(IDatabase database, Action<string> logger) : IExecutable {
    protected List<string> _queries = default!;
    protected readonly IDatabase _database = database;

    /// <inheritdoc/>
    public async Task Execute(bool verbose) {
        foreach (var query in _queries) {
            if (verbose) {
                logger($"> {query}");
            }
            await database.Execute(query.Trim());
        }
    }
}