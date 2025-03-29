
internal class FileManager(string versionsDir) : IFileManager
{
    public bool FileExists(int fileNumber)
    => File.Exists(Path.Combine(versionsDir, fileNumber.ToString() + ".sql"));

    public List<int> GetMap()
    {
        var versionNumbers = new List<int>();
        foreach (var file in Directory.GetFiles(versionsDir, "*.sql"))
        {
            var fileName = Path.GetFileName(file);
            versionNumbers.Add(int.Parse(fileName.Split('.')[0]));
        }
        return versionNumbers;
    }

    public async Task<string[]> ReadFile(int fileNumber)
    {
        return await File.ReadAllLinesAsync(GetPath(fileNumber));
    }
    public async Task WriteFile(int fileNumber, string content)
    {
        await File.WriteAllTextAsync(GetPath(fileNumber), content);
    }

    private string GetPath(int fileNumber)
    {
        return Path.Combine(versionsDir, fileNumber.ToString() + ".sql");
    }
}