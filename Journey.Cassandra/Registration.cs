using System.Runtime.CompilerServices;
using Journey.Databases;

namespace Journey.Cassandra;

public static class JourneyCassandraRegistration {
    [ModuleInitializer]
    internal static void AutoRegister() => Register();

    public static void Register() {
        JourneyFacade.RegisterDatabase(
            CassandraDb.Name,
            async (cs, keySpace) => await new CassandraDb().Connect(cs, keySpace ?? "journey"));
    }
}
