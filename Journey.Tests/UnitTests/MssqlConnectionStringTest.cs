using Journey.Exceptions;
using MssqlDb = Journey.Databases.Mssql;

namespace Journey.Tests.UnitTests;

public class MssqlConnectionStringTest {

    [Theory]
    [InlineData("sqlserver://user:pass@localhost/mydb")]
    [InlineData("SQLSERVER://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_SqlServerScheme_ConvertsToKeyValue(string uri) {
        var result = MssqlDb.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Data Source=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Initial Catalog=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User ID=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("mssql://user:pass@localhost/mydb")]
    [InlineData("MSSQL://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_MssqlScheme_ConvertsToKeyValue(string uri) {
        var result = MssqlDb.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Data Source=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Initial Catalog=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User ID=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithPort_IncludesPortInDataSource() {
        var result = MssqlDb.NormalizeConnectionString("sqlserver://user:pass@localhost:1434/mydb");

        Assert.Contains("Data Source=localhost,1434", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithSpecialCharsInPassword_DecodesCorrectly() {
        var result = MssqlDb.NormalizeConnectionString("sqlserver://user:p%40ss%21@localhost/mydb");

        Assert.Contains("Password=p@ss!", result, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Server=localhost;Database=mydb;User Id=user;Password=pass;")]
    [InlineData("Data Source=db.example.com;Initial Catalog=prod;User ID=admin;Password=secret;")]
    public void NormalizeConnectionString_KeyValueFormat_PassesThroughUnchanged(string kvString) {
        var result = MssqlDb.NormalizeConnectionString(kvString);

        Assert.Equal(kvString, result);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithQueryParams_AppliesParams() {
        var result = MssqlDb.NormalizeConnectionString(
            "sqlserver://user:pass@localhost/mydb?Command Timeout=90&Encrypt=false");

        Assert.Contains("Command Timeout=90", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Encrypt=False", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Initial Catalog=mydb", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithAliasedParams_TranslatesToSqlClientKeywords() {
        var result = MssqlDb.NormalizeConnectionString(
            "sqlserver://user:pass@localhost/mydb?connect_timeout=30&application_name=journey");

        Assert.Contains("Connect Timeout=30", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Application Name=journey", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithUnknownParam_Throws() {
        Assert.Throws<InvalidConnectionStringParameterException>(() =>
            MssqlDb.NormalizeConnectionString("sqlserver://user:pass@localhost/mydb?not_a_real_option=1"));
    }

    [Fact]
    public void NormalizeConnectionString_UriWithRemoteHost_PreservesHost() {
        var result = MssqlDb.NormalizeConnectionString("sqlserver://user:pass@db.example.com/production");

        Assert.Contains("Data Source=db.example.com", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Initial Catalog=production", result, StringComparison.OrdinalIgnoreCase);
    }
}
