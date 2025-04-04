
public interface IDatabase : IDisposable
{
    public Task<IDatabase> Connect(string connectionString);
    public Task<IDatabase> Connect(string connectionString, string schema);
    public Task Execute(string query);
    public Task<int> CurrentVersion();
    public Task<List<Itinerary>> GetItinerary(int entries);
    public IDialect GetDialect();
}