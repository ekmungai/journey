using Journey.Models;

namespace Journey.Interfaces
{
    public interface IReversible
    {
        public Task Rollback();

    }
}
