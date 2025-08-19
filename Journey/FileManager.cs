using System.IO.Abstractions;
using Journey.Interfaces;

namespace Journey;

/// <inheritdoc/>
internal class FileManager(string versionsDir, IFileSystem fileSystem) : IFileManager {
    public bool FileExists(int versionNumber)
        => fileSystem.File.Exists(Path.Combine(versionsDir, versionNumber + ".sql"));
    /// <inheritdoc/>
    public List<int> GetMap() {
        var versionNumbers = new List<int>();
        foreach (var file in fileSystem.Directory.GetFiles(versionsDir, "*.sql")) {
            var fileName = Path.GetFileName(file);
            versionNumbers.Add(int.Parse(fileName.Split('.')[0]));
        }
        return versionNumbers;
    }
    /// <inheritdoc/>
    public async Task<string[]> ReadFile(int versionNumber) {
        return await fileSystem.File.ReadAllLinesAsync(GetPath(versionNumber));
    }
    /// <inheritdoc/>
    public async Task WriteFile(int versionNumber, string content) {
        await fileSystem.File.WriteAllTextAsync(GetPath(versionNumber), content);
    }
    /// Retrieve the fully qualified path of the given version number
    private string GetPath(int versionNumber) {
        return Path.Combine(versionsDir, $"{versionNumber}.sql");
    }
}