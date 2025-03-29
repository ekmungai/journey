public interface IFileManager
{
    public List<int> GetMap();
    public bool FileExists(int fileNumber);
    public Task<string[]> ReadFile(int fileNumber);
    public Task WriteFile(int fileNumber, string content);

}