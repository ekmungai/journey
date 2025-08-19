
namespace Journey.Tests.IntegrationTests;

public class CassandraTest(CassandraFixture container)
    : GenericDbTests<CassandraFixture>(container)
{ }