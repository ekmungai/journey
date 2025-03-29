class FileManager(string versionsDir) : IFileManager
{
    public string VersionsDir { get; init; } = versionsDir;
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
        return Path.Combine(VersionsDir, fileNumber.ToString() + ".sql");
    }
}