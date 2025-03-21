using Journey.Models;

namespace Journey.Interfaces
{
    public interface IReversible
    {
        Rollback rollback { set; }
        public Task Rollback();

    }
}
