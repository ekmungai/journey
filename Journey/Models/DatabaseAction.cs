/// <inheritdoc/>
public abstract record DatabaseAction(IDatabase database, Action<string> logger) : IExecutable {
    protected List<string> _queries;
    protected readonly IDatabase _database = database;

    /// <inheritdoc/>
    public async Task Execute() {
        foreach (var query in _queries) {
#if DEBUG
            logger($"> {query}");
#endif
            await database.Execute(query.Trim());
        }
    }
}