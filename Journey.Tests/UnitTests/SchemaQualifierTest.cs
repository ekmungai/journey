using Journey.Helpers;

namespace Journey.Tests.UnitTests;

public class SchemaQualifierTest {

    [Fact]
    public void TestQualifiesTheVersionsTable() {
        Assert.Equal(
            "INSERT INTO public.versions (version) VALUES (1);",
            SchemaQualifier.Qualify("INSERT INTO versions (version) VALUES (1);", "public")
        );
        Assert.Equal(
            "SELECT COUNT(*) as version FROM public.versions;",
            SchemaQualifier.Qualify("SELECT COUNT(*) as version FROM versions;", "public")
        );
        Assert.Equal(
            "DELETE FROM reporting.versions WHERE version = 3;",
            SchemaQualifier.Qualify("DELETE FROM versions WHERE version = 3;", "reporting")
        );
    }

    [Fact]
    public void TestLeavesTablesThatMerelyContainTheWordAlone() {
        Assert.Equal(
            "CREATE TABLE IF NOT EXISTS chart_definition_versions (id UUID PRIMARY KEY);",
            SchemaQualifier.Qualify("CREATE TABLE IF NOT EXISTS chart_definition_versions (id UUID PRIMARY KEY);", "public")
        );
        Assert.Equal(
            "SELECT * FROM versions_backup;",
            SchemaQualifier.Qualify("SELECT * FROM versions_backup;", "public")
        );
    }

    [Fact]
    public void TestLeavesAlreadyQualifiedReferencesAlone() {
        Assert.Equal(
            "SELECT * FROM reporting.versions;",
            SchemaQualifier.Qualify("SELECT * FROM reporting.versions;", "public")
        );
    }

    [Fact]
    public void TestLeavesCommentsAndStringLiteralsAlone() {
        Assert.Equal(
            "INSERT INTO public.versions (description) VALUES ('created the versions table'); -- versions",
            SchemaQualifier.Qualify(
                "INSERT INTO versions (description) VALUES ('created the versions table'); -- versions",
                "public")
        );
    }

    [Fact]
    public void TestWithoutASchemaTheQueryIsUnchanged() {
        Assert.Equal(
            "SELECT * FROM versions;",
            SchemaQualifier.Qualify("SELECT * FROM versions;", "")
        );
    }
}
