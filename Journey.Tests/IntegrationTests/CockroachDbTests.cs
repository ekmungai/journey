
namespace Journey.Tests.IntegrationTests;

public class CockroachDbTest(CockroachDbFixture container)
    : GenericDbTests<CockroachDbFixture>(container)
{ }