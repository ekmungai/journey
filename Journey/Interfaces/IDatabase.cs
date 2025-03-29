
public interface IDatabase
{
    public Task<IDatabase> Connect(string connectionString);
    public Task Execute(string query);
    public Task<int> CurrentVersion();
    public IDialect GetDialect();
}