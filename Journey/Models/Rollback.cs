public class Rollback : DatabaseAction
{
    public Rollback(IDatabase database, List<string> queries) : base(database)
    {
        _queries = queries;
    }

    public async Task Reverse()
    {
        Console.WriteLine("Begin Rollback ...");
        await Execute();
        Console.WriteLine();
        Console.WriteLine("End Rollback ...");
    }
}