public abstract class DatabaseAction(IDatabase database) : IExecutable
{
    protected List<string> _queries;
    protected readonly IDatabase _database = database;

    public async Task Execute()
    {
        foreach (var query in _queries)
        {
            Console.WriteLine($"> {query}");
            await database.Execute(query.Trim());
        }
    }
}