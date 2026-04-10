using Journey.Databases;

namespace Journey.Tests.UnitTests;

public class MysqlConnectionStringTest {

    [Theory]
    [InlineData("mysql://user:pass@localhost/mydb")]
    [InlineData("MYSQL://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_MysqlScheme_ConvertsToKeyValue(string uri) {
        var result = Mysql.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Server=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User ID=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("mysql+tcp://user:pass@localhost/mydb")]
    [InlineData("MYSQL+TCP://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_MysqlTcpScheme_ConvertsToKeyValue(string uri) {
        var result = Mysql.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Server=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("mariadb://user:pass@localhost/mydb")]
    [InlineData("MARIADB://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_MariaDbScheme_ConvertsToKeyValue(string uri) {
        var result = Mysql.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Server=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User ID=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithPort_PreservesPort() {
        var result = Mysql.NormalizeConnectionString("mysql://user:pass@localhost:3307/mydb");

        Assert.Contains("Port=3307", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithSpecialCharsInPassword_DecodesCorrectly() {
        var result = Mysql.NormalizeConnectionString("mysql://user:p%40ss%21@localhost/mydb");

        Assert.Contains("Password=p@ss!", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Server=localhost;Database=mydb;User ID=user;Password=pass;")]
    [InlineData("Server=db.example.com;Database=prod;User ID=admin;Password=secret;")]
    public void NormalizeConnectionString_KeyValueFormat_PassesThroughUnchanged(string kvString) {
        var result = Mysql.NormalizeConnectionString(kvString);

        Assert.Equal(kvString, result);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithRemoteHost_PreservesHost() {
        var result = Mysql.NormalizeConnectionString("mysql://user:pass@db.example.com/production");

        Assert.Contains("Server=db.example.com", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=production", result, StringComparison.OrdinalIgnoreCase);
    }
}
