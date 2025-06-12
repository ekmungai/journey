/// <summary>
/// Represents an object which when executed effects a change on the Database.
/// </summary>
public interface IExecutable {
    /// <summary>
    /// Run the queries in the executable.
    /// </summary>
    /// <param name="verbose">Whether to print out queries as they are executed.</param>
    /// <returns cref="Task"></returns>
    public Task Execute(bool verbose);
}