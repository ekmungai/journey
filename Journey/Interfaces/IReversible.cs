/// <summary>
/// Represents an executable whose effects on the Database can be reversed.
/// </summary>
public interface IReversible {
    /// <summary>
    /// Undos the effects that the executable applied on the database.
    /// </summary>
    /// <returns cref="Task"></returns>
    public Task Rollback();
}