
namespace Journey.Tests.IntegrationTests;

public class MariaDbTest(MysqlFixture _container)
    : GenericDbTests<MysqlFixture>(_container)
{

}