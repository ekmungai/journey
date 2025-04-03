
namespace Journey.Tests.IntegrationTests;
public class MySqlTest(MysqlFixture _container)
    : GenericDbTests<MysqlFixture>(_container)
{ }