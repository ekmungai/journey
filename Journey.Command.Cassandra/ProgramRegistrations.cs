using Journey.Cassandra;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyCassandraRegistration.Register();
    }
}
