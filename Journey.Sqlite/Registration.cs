using System.Runtime.CompilerServices;
using SqliteDb = Journey.Databases.Sqlite;

namespace Journey.Sqlite;

internal static class JourneySqliteRegistration {
    [ModuleInitializer]
    internal static void Register() {
        JourneyFacade.RegisterDatabase(
            SqliteDb.Name,
            async (cs, _) => await new SqliteDb().Connect(cs));
    }
}
