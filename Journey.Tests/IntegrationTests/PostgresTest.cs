
namespace Journey.Tests.IntegrationTests;

public class PostgresTest(PostgresFixture _container)
    : GenericDbTests<PostgresFixture>(_container)
{ }