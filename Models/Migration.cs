
using Journey.Interfaces;

namespace Journey.Models
{
    public class Migration(IDatabase database, List<string> queries, Rollback rollback) :
    DatabaseAction(database, queries), IReversible
    {
        public Rollback rollback { set => throw new NotImplementedException(); }

        public Task Rollback()
        {
            throw new rollback.Execute();
        }
    }

}