using Journey.Exceptions;
using PgDb = Journey.Databases.Postgres;

namespace Journey.Tests.UnitTests;

public class PostgresConnectionStringTest {

    [Theory]
    [InlineData("postgres://user:pass@localhost/mydb")]
    [InlineData("postgresql://user:pass@localhost/mydb")]
    [InlineData("POSTGRES://user:pass@localhost/mydb")]
    [InlineData("POSTGRESQL://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_UriScheme_ConvertsToKeyValue(string uri) {
        var result = PgDb.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Host=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Username=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithPort_PreservesPort() {
        var result = PgDb.NormalizeConnectionString("postgres://user:pass@localhost:5433/mydb");

        Assert.Contains("Port=5433", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithoutPort_OmitsPort() {
        var result = PgDb.NormalizeConnectionString("postgres://user:pass@localhost/mydb");

        Assert.DoesNotContain("Port=5433", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithSpecialCharsInPassword_DecodesCorrectly() {
        var result = PgDb.NormalizeConnectionString("postgres://user:p%40ss%21@localhost/mydb");

        Assert.Contains("Password=p@ss!", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_KeyValueFormat_PassesThroughUnchanged() {
        const string kvString = "Host=localhost;Database=mydb;Username=user;Password=pass";

        var result = PgDb.NormalizeConnectionString(kvString);

        Assert.Equal(kvString, result);
    }

    [Theory]
    [InlineData("Host=localhost;Database=mydb;Username=user;Password=pass")]
    [InlineData("Server=db.example.com;Database=prod;User Id=admin;Password=secret;")]
    public void NormalizeConnectionString_NonUriStrings_ReturnUnchanged(string kvString) {
        var result = PgDb.NormalizeConnectionString(kvString);

        Assert.Equal(kvString, result);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithRemoteHost_PreservesHost() {
        var result = PgDb.NormalizeConnectionString("postgres://user:pass@db.example.com/production");

        Assert.Contains("Host=db.example.com", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=production", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithQueryParams_AppliesParams() {
        var result = PgDb.NormalizeConnectionString(
            "postgres://user:pass@localhost/mydb?Command Timeout=90&Pooling=false");

        Assert.Contains("Command Timeout=90", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Pooling=False", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithLibpqParams_TranslatesToNpgsqlKeywords() {
        var result = PgDb.NormalizeConnectionString(
            "postgres://user:pass@localhost/mydb?sslmode=Require&connect_timeout=30&application_name=journey");

        Assert.Contains("SSL Mode=Require", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Timeout=30", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Application Name=journey", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithEscapedParamValue_DecodesCorrectly() {
        var result = PgDb.NormalizeConnectionString(
            "postgres://user:pass@localhost/mydb?application_name=my%20app");

        Assert.Contains("Application Name=\"my app\"", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeConnectionString_UriWithUnknownParam_Throws() {
        Assert.Throws<InvalidConnectionStringParameterException>(() =>
            PgDb.NormalizeConnectionString("postgres://user:pass@localhost/mydb?not_a_real_option=1"));
    }

    [Theory]
    [InlineData("cockroachdb://user:pass@localhost/mydb")]
    [InlineData("COCKROACHDB://user:pass@localhost/mydb")]
    public void NormalizeConnectionString_CockroachDbScheme_ConvertsToKeyValue(string uri) {
        var result = PgDb.NormalizeConnectionString(uri);

        Assert.DoesNotContain("://", result);
        Assert.Contains("Host=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=mydb", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Username=user", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", result, StringComparison.OrdinalIgnoreCase);
    }
}
