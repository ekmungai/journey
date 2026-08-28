
using Journey.Models;

namespace Journey.Interfaces;

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
    /// Executes all the queries of a single migration or rollback, honouring the transactions
    /// declared in the migration file so that a failure part of the way through leaves nothing of
    /// the migration behind.
    /// </summary>
    /// <param name="queries">The queries to execute, including the transaction delimiters.</param>
    /// <param name="onQuery">Called with each query before it is executed, if given.</param>
    /// <returns cref="Task"></returns>
    public async Task ExecuteAll(IReadOnlyList<string> queries, Action<string>? onQuery = null) {
        // Databases without multi statement transactions, such as Cassandra, keep this fallback.
        foreach (var query in queries) {
            onQuery?.Invoke(query);
            await Execute(query.Trim());
        }
    }
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
    /// Creates the Database if it doesn't exist yet.
    /// </summary>
    /// <returns cref="IDialect"></returns>
    public IDialect GetDialect();
    /// <summary>
    /// Checks if the Database exists.
    /// </summary>
    public Task<bool> CheckDatabase();
    /// <summary>
    /// Initialize the Database.
    /// </summary>
    public Task InitDatabase();
}