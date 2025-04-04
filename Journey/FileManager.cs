using System.IO.Abstractions;

internal class FileManager(string versionsDir, IFileSystem _fileSystem) : IFileManager
{
    public bool FileExists(int fileNumber)
    => _fileSystem.File.Exists(Path.Combine(versionsDir, fileNumber.ToString() + ".sql"));

    public List<int> GetMap() {
        var versionNumbers = new List<int>();
        foreach (var file in _fileSystem.Directory.GetFiles(versionsDir, "*.sql"))
        {
            var fileName = Path.GetFileName(file);
            versionNumbers.Add(int.Parse(fileName.Split('.')[0]));
        }
        return versionNumbers;
    }

    public async Task<string[]> ReadFile(int fileNumber) {
        return await _fileSystem.File.ReadAllLinesAsync(GetPath(fileNumber));
    }
    public async Task WriteFile(int fileNumber, string content) {
        await _fileSystem.File.WriteAllTextAsync(GetPath(fileNumber), content);
    }

    private string GetPath(int fileNumber) {
        return Path.Combine(versionsDir, fileNumber.ToString() + ".sql");
    }
}