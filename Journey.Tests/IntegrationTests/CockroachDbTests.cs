
namespace Journey.Tests.IntegrationTests;

public class CockroachDbTest(CockroachDbFixture _container)
    : GenericDbTests<CockroachDbFixture>(_container)
{ }