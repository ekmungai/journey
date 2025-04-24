using System.IO.Abstractions;
/// <inheritdoc/>
internal class FileManager(string versionsDir, IFileSystem _fileSystem) : IFileManager {
    public bool FileExists(int versionNumber)
    => _fileSystem.File.Exists(Path.Combine(versionsDir, versionNumber.ToString() + ".sql"));
    /// <inheritdoc/>
    public List<int> GetMap() {
        var versionNumbers = new List<int>();
        foreach (var file in _fileSystem.Directory.GetFiles(versionsDir, "*.sql")) {
            var fileName = Path.GetFileName(file);
            versionNumbers.Add(int.Parse(fileName.Split('.')[0]));
        }
        return versionNumbers;
    }
    /// <inheritdoc/>
    public async Task<string[]> ReadFile(int versionNumber) {
        return await _fileSystem.File.ReadAllLinesAsync(GetPath(versionNumber));
    }
    /// <inheritdoc/>
    public async Task WriteFile(int versionNumber, string content) {
        await _fileSystem.File.WriteAllTextAsync(GetPath(versionNumber), content);
    }
    /// <inheritdoc/>
    private string GetPath(int versionNumber) {
        return Path.Combine(versionsDir, versionNumber.ToString() + ".sql");
    }
}