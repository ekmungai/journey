
/// <summary>
/// A representation of a Database.
/// </summary>
public interface IDatabase : IDisposable {
    /// <summary>
    /// Connect to the Database.
    /// </summary>
    /// <param name="connectionString">The connection string to connect with.</param>
    /// <returns cref="IDatabase">An instance of the Database.</returns>
    public Task<IDatabase> Connect(string connectionString);
    /// <summary>
    /// Connect to the Database, specifying a schema.
    /// </summary>
    /// <param name="connectionString">The connection string to connect with.</param>
    /// <param name="schema">The schema to apply migrations on.</param>
    /// <returns cref="IDatabase">An instance of the Database.</returns>
    public Task<IDatabase> Connect(string connectionString, string schema);
    /// <summary>
    /// Executes the provided query against the Database.
    /// </summary>
    /// <param name="query">The query to execute against the Database.</param>
    /// <returns cref="Task"></returns>
    public Task Execute(string query);
    /// <summary>
    /// Gets the current version of the Database.
    /// </summary>
    /// <returns cref="Task"></returns>
    public Task<int> CurrentVersion();
    /// <summary>
    /// Retrieves the descriptions of applied migrations chronologically. 
    /// </summary>
    /// <param name="entries">The maximum number of migration entries to retrieve.</param>
    /// <returns cref="Task"></returns>
    public Task<List<Itinerary>> GetItinerary(int entries);
    /// <summary>
    /// Returns the Dialect of the Database.
    /// </summary>
    /// <returns cref="IDialect"></returns>
    public IDialect GetDialect();
}