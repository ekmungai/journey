public interface IJourneyFacade
{
    public Task<string> Scaffold();
    public Task<string> Validate(int version);
    public Task<string> Migrate(int? target, bool? debug, bool? dryRun);
    public Task<string> Rollback(int? target, bool? debug);
    public Task<string> History(int entries);
    public Task<string> Update(bool? debug);
}