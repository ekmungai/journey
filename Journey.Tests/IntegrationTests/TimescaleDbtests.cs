
namespace Journey.Tests.IntegrationTests;

public class TimescaleDbTest(TimescaleDbFixture _container)
    : GenericDbTests<TimescaleDbFixture>(_container) { }