/// <summary>
/// Represents an object which when executed effects a change on the Database.
/// </summary>
public interface IExecutable {
    /// <summary>
    /// Run the queries in the executable.
    /// </summary>
    /// <returns cref="Task"></returns>
    public Task Execute();
}