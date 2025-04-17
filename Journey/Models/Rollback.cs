public record Rollback : DatabaseAction {
    public Rollback(IDatabase database, Dictionary<string, List<string>> sections, Action<string> logger) : base(database, logger) {
        _queries = sections[Parser.Rollback];
    }

    public async Task Reverse() {
        await Execute();
    }
}