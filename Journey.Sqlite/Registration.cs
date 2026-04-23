using System.Runtime.CompilerServices;
using SqliteDb = Journey.Databases.Sqlite;

namespace Journey.Sqlite;

public static class JourneySqliteRegistration {
    [ModuleInitializer]
    internal static void AutoRegister() => Register();

    public static void Register() {
        JourneyFacade.RegisterDatabase(
            SqliteDb.Name,
            async (cs, _) => await new SqliteDb().Connect(cs));
    }
}
