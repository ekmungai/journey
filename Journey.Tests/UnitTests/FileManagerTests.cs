using Moq.AutoMock;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;

namespace Journey.Tests.UnitTests;

public class FileManagerTest
{
    private readonly AutoMocker _mocker = new(Moq.MockBehavior.Strict);
    private readonly MockFileSystem _fileSystem = new();
    private readonly string _versionsDir = ".";

    [Fact]
    public void TestFileExists()
    {
        // Arrange
        var fileManager = new FileManager(_versionsDir, _fileSystem);
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData("queries"));

        // Act
        var exists = fileManager.FileExists(0);
        var missing = fileManager.FileExists(1);

        // Assert
        Assert.True(exists);
        Assert.False(missing);
    }

    [Fact]
    public void TestGetMap()
    {
        // Arrange
        var fileManager = new FileManager(_versionsDir, _fileSystem);
        _fileSystem.AddFile(Path.Combine(_versionsDir, "0.sql"), new MockFileData("queries"));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "1.sql"), new MockFileData("queries2"));
        _fileSystem.AddFile(Path.Combine(_versionsDir, "2.sql"), new MockFileData("queries3"));

        // Act
        var map = fileManager.GetMap();

        // Assert
        Assert.Equal([0, 1, 2], map);
    }

    [Theory]
    [InlineData(0, "queries1", "queries3")]
    [InlineData(1, "queries2", "queries5")]
    [InlineData(2, "queries3", "queries7")]
    public async Task TestReadAndWriteFile(int fileNumber, string originalContent, string newContent)
    {
        // Arrange
        var fileManager = new FileManager(_versionsDir, _fileSystem);
        _fileSystem.AddFile(Path.Combine(_versionsDir, $"{fileNumber}.sql"), new MockFileData(originalContent));

        // Act
        await fileManager.WriteFile(fileNumber, newContent);

        // Assert
        var fileContent = await fileManager.ReadFile(fileNumber);
        Assert.Equal([newContent], fileContent);
    }
}
