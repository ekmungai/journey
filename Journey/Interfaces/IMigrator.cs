public interface IMigrator
{
    public Task Init(bool quiet);
    public Task<string> Validate(int version);
    public Task<string> Scaffold(int version);
    public Task<string> Migrate(int? target);
    public Task<string> Rollback(int? target);
}