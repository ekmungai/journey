
using Journey.Interfaces;
namespace Journey
{
    class Database : IDatabase
    {
        public Task Connect(string ConnectionString)
        {
            throw new NotImplementedException();
        }

        public Task Execute(string query)
        {
            throw new NotImplementedException();
        }
    }
}