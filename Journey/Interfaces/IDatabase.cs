
public interface IDatabase
{
    public IDatabase Connect(string connectionString);
    public Task Execute(string query);
    public Task<int> CurrentVersion();
    public IDialect GetDialect();
}