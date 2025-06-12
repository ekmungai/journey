/// <summary>
/// Represents the orchestration of migration operations against the Database.
/// </summary>
public interface IMigrator {
    /// <summary>
    /// Initializes the Database for migrations by creating the versions table.
    /// </summary>
    /// <param name="quiet">Determines whether the user should be prompted before creating the versions table.</param>
    /// <returns cref="Task"></returns>
    public Task Init(bool quiet);
    /// <summary>
    /// Creates the basic structure of the migration for the next version of the Database.
    /// </summary>
    /// <returns cref="Task"></returns>
    public Task Scaffold();
    /// <summary>
    /// Checks that the contents of the migration file with the given file number has the required structure to be 
    /// successfully executed.
    /// </summary>
    /// <param name="version">The version whose file is to be validated.</param>
    /// <returns cref="Task"></returns>
    /// <returns>True if the file for the version is valid, false if not.</returns>
    public Task<bool> Validate(int version);
    /// <summary>
    /// Updates the Database up to the target version if one is provided, and to the highest available version if not. 
    /// </summary>
    /// <param name="target">The version at which to stop the migration.</param>
    /// <param name="dryRun">Whether or not to undo all migrations applied.</param>
    /// <returns cref="Task"></returns>
    public Task Migrate(int? target, bool? dryRun);
    /// <summary>
    /// Restores the Database down to the target version if one is provided, and to the highest available version if not.
    /// </summary>
    /// <param name="target">The version at which to stop the rollback.</param>
    /// <returns cref="Task"></returns>
    public Task Rollback(int? target);
    /// <summary>
    /// Retrieves the records of all migrations applied to the Database from the latest up to the given number of entries
    /// back.
    /// </summary>
    /// <param name="entries">The number of migration entries to retrieve.</param>
    /// <returns cref="Task"></returns>
    public Task<string> History(int entries);
    /// <summary>
    /// Migrates or rolls back the Database to the given target, or to the highest available version if none is specified.
    /// </summary>
    /// <returns cref="Task"></returns>
    public Task Update(int? target = null);
}