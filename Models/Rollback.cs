
using Journey.Interfaces;

namespace Journey.Models
{
    public class Rollback(IDatabase database, List<string> queries) :
    DatabaseAction(database, queries)
    { }
}