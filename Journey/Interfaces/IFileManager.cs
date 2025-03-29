public interface IFileManager
{
    public Task<string[]> ReadFile(int fileNumber);
    public Task WriteFile(int fileNumber, string content);

}