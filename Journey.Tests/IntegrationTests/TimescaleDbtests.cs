
namespace Journey.Tests.IntegrationTests;

public class TimescaleDbTest(CockroachDbFixture _container)
    : GenericDbTests<CockroachDbFixture>(_container)
{ }