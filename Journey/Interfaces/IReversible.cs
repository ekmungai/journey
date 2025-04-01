public interface IReversible
{
    public Task Rollback(bool debug);
}