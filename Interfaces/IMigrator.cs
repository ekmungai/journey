using Journey.Models;
namespace Journey.Interfaces
{
    public interface IMigrator
    {
        public Task Migrate(Migration migration);
        public Task Rollback(Migration migration);
    }
}
