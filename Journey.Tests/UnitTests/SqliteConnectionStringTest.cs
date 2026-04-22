using SqliteDb = Journey.Databases.Sqlite;

namespace Journey.Tests.UnitTests;

public class SqliteConnectionStringTest {

    [Fact]
    public void NormalizeConnectionString_FileUriRelativePath_ConvertsToDataSource() {
        var result = SqliteDb.NormalizeConnectionString("file:mydb.sqlite");

        Assert.Equal("Data Source=mydb.sqlite", result);
    }

    [Fact]
    public void NormalizeConnectionString_FileUriAbsolutePath_ConvertsToDataSource() {
        var result = SqliteDb.NormalizeConnectionString("file:/home/user/mydb.sqlite");

        Assert.Equal("Data Source=/home/user/mydb.sqlite", result);
    }

    [Fact]
    public void NormalizeConnectionString_FileUriTripleSlash_ConvertsToDataSource() {
        var result = SqliteDb.NormalizeConnectionString("file:///home/user/mydb.sqlite");

        Assert.Equal("Data Source=/home/user/mydb.sqlite", result);
    }

    [Fact]
    public void NormalizeConnectionString_FileUriWithAuthority_ConvertsToDataSource() {
        var result = SqliteDb.NormalizeConnectionString("file://localhost/home/user/mydb.sqlite");

        Assert.Equal("Data Source=/home/user/mydb.sqlite", result);
    }

    [Fact]
    public void NormalizeConnectionString_InMemoryUri_ConvertsToDataSource() {
        var result = SqliteDb.NormalizeConnectionString("file::memory:");

        Assert.Equal("Data Source=:memory:", result);
    }

    [Fact]
    public void NormalizeConnectionString_FileUriWithQueryParams_StripsQueryParams() {
        var result = SqliteDb.NormalizeConnectionString("file:mydb.sqlite?cache=shared&mode=rwc");

        Assert.Equal("Data Source=mydb.sqlite", result);
    }

    [Fact]
    public void NormalizeConnectionString_FileUriCaseInsensitive_Converts() {
        var result = SqliteDb.NormalizeConnectionString("FILE:mydb.sqlite");

        Assert.Equal("Data Source=mydb.sqlite", result);
    }

    [Theory]
    [InlineData("Data Source=mydb.sqlite")]
    [InlineData("Data Source=:memory:")]
    [InlineData("Data Source=/home/user/mydb.sqlite;Version=3;")]
    public void NormalizeConnectionString_KeyValueFormat_PassesThroughUnchanged(string kvString) {
        var result = SqliteDb.NormalizeConnectionString(kvString);

        Assert.Equal(kvString, result);
    }
}
