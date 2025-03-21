

using Journey.Interfaces;

namespace Journey.Models
{
    public abstract class DatabaseAction(IDatabase database, List<string> queries) : IExecutable
    {
        protected readonly List<string> _queries = queries;
        private readonly IDatabase _database = database;

        public virtual async Task Execute()
        {
            await _database.Execute(this);
        }
    }
}