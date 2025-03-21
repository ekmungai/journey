namespace Journey.Interfaces
{
    public interface IDatabase
    {
        public Task Connect(string ConnectionString);
        public Task Execute(IExecutable executable);
    }
}
