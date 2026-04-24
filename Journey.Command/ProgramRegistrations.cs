using Journey.Cassandra;
using Journey.Mssql;
using Journey.MySql;
using Journey.Postgres;
using Journey.Sqlite;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyPostgresRegistration.Register();
        JourneyMySqlRegistration.Register();
        JourneySqliteRegistration.Register();
        JourneyMssqlRegistration.Register();
        JourneyCassandraRegistration.Register();
    }
}
