using Journey.Cassandra;
using Journey.MySql;
using Journey.Postgres;
using Journey.Sqlite;
using Journey.Mssql;

namespace Journey.Tests.UnitTests;

/// <summary>
/// Verifies that each package's public Register() method registers the expected
/// database types — the explicit-registration path that clients must use when
/// [ModuleInitializer] does not fire (e.g. isolated ALCs, certain test hosts).
/// </summary>
public class RegistrationTest {
    [Fact]
    public void PostgresRegistration_RegistersAllThreeDatabaseTypes() {
        JourneyPostgresRegistration.Register();

        Assert.True(JourneyFacade.IsRegistered("postgres"));
        Assert.True(JourneyFacade.IsRegistered("timescaledb"));
        Assert.True(JourneyFacade.IsRegistered("cockroachdb"));
    }

    [Fact]
    public void MySqlRegistration_RegistersBothDatabaseTypes() {
        JourneyMySqlRegistration.Register();

        Assert.True(JourneyFacade.IsRegistered("mysql"));
        Assert.True(JourneyFacade.IsRegistered("mariadb"));
    }

    [Fact]
    public void SqliteRegistration_RegistersDatabaseType() {
        JourneySqliteRegistration.Register();

        Assert.True(JourneyFacade.IsRegistered("sqlite"));
    }

    [Fact]
    public void MssqlRegistration_RegistersDatabaseType() {
        JourneyMssqlRegistration.Register();

        Assert.True(JourneyFacade.IsRegistered("mssql"));
    }

    [Fact]
    public void CassandraRegistration_RegistersDatabaseType() {
        JourneyCassandraRegistration.Register();

        Assert.True(JourneyFacade.IsRegistered("cassandra"));
    }

    [Fact]
    public void UnregisteredDatabaseType_IsNotRegistered() {
        Assert.False(JourneyFacade.IsRegistered("nonexistent-db"));
    }
}
