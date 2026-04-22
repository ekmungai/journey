using System.Runtime.CompilerServices;
using Journey.Databases;

namespace Journey.Cassandra;

internal static class JourneyCassandraRegistration {
    [ModuleInitializer]
    internal static void Register() {
        JourneyFacade.RegisterDatabase(
            CassandraDb.Name,
            async (cs, keySpace) => await new CassandraDb().Connect(cs, keySpace ?? "journey"));
    }
}
