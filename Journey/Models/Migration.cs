public class Migration : DatabaseAction, IReversible
{
    private readonly Rollback _rollback;

    public Migration(IDatabase database, Dictionary<string, List<string>> sections) : base(database)
    {
        _queries = sections[Parser.Migration];
        _rollback = new Rollback(database, sections);
    }

    public async Task Migrate()
    {
        await Execute();
    }

    public async Task Rollback() => await _rollback.Reverse();
}