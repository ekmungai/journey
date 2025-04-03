namespace Journey.Tests.IntegrationTests;

public class MssqlTests(MssqlFixture _container)
    : GenericDbTests<MssqlFixture>(_container)
{ }