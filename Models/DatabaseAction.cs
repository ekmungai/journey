

using Journey.Interfaces;

namespace Journey.Models
{
    public abstract class DatabaseAction(IDatabase database, IParser parser, int number) : IExecutable
    {
        protected readonly List<string> _queries = queries;
        private readonly IDatabase _database = database;
        private readonly IParser _parser = parser;
        protected async Task Load()
        {
            _queries = _parser.GetQueries(Number);
        }

        public virtual async Task Execute()
        {
            foreach (var query in queries)
            {
                await _database.Execute(this);
            }
        }
    }
}