public class Migration : DatabaseAction
{
    public Migration(IDatabase database, Dictionary<string, List<string>> sections) : base(database)
    {
        _queries = sections[Parser.Migration];
    }

    public async Task Migrate()
    {
        Console.WriteLine($"Begin Migration ...{Environment.NewLine}");
        await Execute();
        Console.WriteLine($"{Environment.NewLine}End Migration ...");
    }
}