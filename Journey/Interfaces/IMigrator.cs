public interface IMigrator {
    public Task Init(bool quiet);
    public Task Scaffold();
    public Task Validate(int version);
    public Task Migrate(int? target, bool? dryRun);
    public Task Rollback(int? target);
    public Task<string> History(int entries);
    public Task Update();
}