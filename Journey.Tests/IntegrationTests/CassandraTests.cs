
namespace Journey.Tests.IntegrationTests;

public class CassandraTest(CassandraFixture _container)
    : GenericDbTests<CassandraFixture>(_container)
{ }