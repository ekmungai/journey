namespace Journey.Interfaces
{
    public interface IFileManager
    {
        public Task ReadFile(string path);
        public Task WriteFile(string path, string content);

    }
}
