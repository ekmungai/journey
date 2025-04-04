public class Rollback : DatabaseAction {
    public Rollback(IDatabase database, Dictionary<string, List<string>> sections) : base(database) {
        _queries = sections[Parser.Rollback];
    }

    public async Task Reverse(bool debug) {
        await Execute(debug);
    }
}