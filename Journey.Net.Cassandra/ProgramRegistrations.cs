using Journey.Cassandra;

namespace Journey.Net;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyCassandraRegistration.Register();
    }
}
