public class Migration : DatabaseAction, IReversible
{
    private readonly Rollback _rollback;

    public Migration(IDatabase database, Dictionary<string, List<string>> sections) : base(database)
    {
        _queries = sections[Parser.Migration];
        _rollback = new Rollback(database, sections[Parser.Rollback]);
    }

    public async Task Migrate()
    {
        Console.WriteLine($"Begin Migration ...{Environment.NewLine}");
        await Execute();
        Console.WriteLine($"{Environment.NewLine}End Migration ...");
    }

    public async Task Rollback() => await _rollback.Reverse();
}