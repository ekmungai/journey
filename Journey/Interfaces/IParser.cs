namespace Journey.Interfaces;

/// <summary>
/// Converts the contents of a migration file into an executable series of Database queries,
/// checking for the expected files structure along the way.
/// </summary>
public interface IParser {
    /// <summary>
    /// Converts the contents of a migration file into a series of executable SQL queries.
    /// </summary>
    /// <returns>Nothing, if the file is valid the result of the parsing is stored internally.</returns>
    public Queue<string>? ParseFile();
    /// <summary>
    /// Get the result of parsing a migration file.
    /// </summary>
    /// <returns>A dictionary of the migration and rollback sections from the migration file.</returns>
    public Dictionary<string, List<string>> GetResult();

}