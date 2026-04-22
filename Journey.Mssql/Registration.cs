using System.Runtime.CompilerServices;
using MssqlDb = Journey.Databases.Mssql;

namespace Journey.Mssql;

internal static class JourneyMssqlRegistration {
    [ModuleInitializer]
    internal static void Register() {
        JourneyFacade.RegisterDatabase(
            MssqlDb.Name,
            async (cs, _) => await new MssqlDb().Connect(cs));
    }
}
