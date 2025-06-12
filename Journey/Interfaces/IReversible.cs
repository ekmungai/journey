/// <summary>
/// Represents an executable whose effects on the Database can be reversed.
/// </summary>
public interface IReversible {
    /// <summary>
    /// Undos the effects that the executable applied on the database.
    /// </summary>
    /// <param name="verbose">Whether to print out queries as they are executed.</param>
    /// <returns cref="Task"></returns>
    public Task Rollback(bool verbose);
}