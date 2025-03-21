
using Journey.Interfaces;
namespace Journey
{
    class Database : IDatabase
    {
        public Task Connect(string ConnectionString)
        {
            throw new NotImplementedException();
        }

        public Task Execute(IExecutable executable)
        {
            throw new NotImplementedException();
        }
    }
}