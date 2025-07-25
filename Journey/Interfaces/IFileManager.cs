namespace Journey.Interfaces;

/// <summary>
/// Represents the file system within migration scripts are located.
/// </summary>
public interface IFileManager {
    /// <summary>
    /// Gets the numbers of all version scripts available.
    /// </summary>
    /// <returns>A list of version numbers</returns>
    public List<int> GetMap();
    /// <summary>
    /// Checks if the file for the given version exists on the file system.
    /// </summary>
    /// <param name="versionNumber">The version number to check.</param>
    /// <returns>True if file for exists, false otherwise.</returns>
    public bool FileExists(int versionNumber);
    /// <summary>
    /// Retrieves the contents of the file with the given file number.
    /// </summary>
    /// <param name="versionNumber">The version number of the file to read.</param>
    /// <returns>An array of strings representing the lines in the file.</returns>
    public Task<string[]> ReadFile(int versionNumber);
    /// <summary>
    /// Adds content to the file with the given file number.
    /// </summary>
    /// <param name="versionNumber">The version number of the file to read.</param>
    /// <param name="content">The content to write to the the file.</param>
    /// <returns cref="Task"></returns>
    public Task WriteFile(int versionNumber, string content);

}