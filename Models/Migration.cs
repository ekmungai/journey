
using Journey.Interfaces;

namespace Journey.Models
{
    public class Migration(IDatabase database, List<string> queries, Rollback rollback) :
    DatabaseAction(database, queries), IReversible
    {
        private readonly Rollback rollback = rollback;

        public async Task Rollback()
        {
            await rollback.Execute();
        }
    }

}