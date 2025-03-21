
using Journey.Interfaces;

namespace Journey.Implrementations
{
    class FileManager : IFileManager
    {
        public async Task ReadFile(string path)
        {
            await File.ReadAllTextAsync(path);

        }
        public async Task WriteFile(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }
    }
}