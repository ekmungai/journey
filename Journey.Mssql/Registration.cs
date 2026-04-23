using System.Runtime.CompilerServices;
using MssqlDb = Journey.Databases.Mssql;

namespace Journey.Mssql;

public static class JourneyMssqlRegistration {
    [ModuleInitializer]
    internal static void AutoRegister() => Register();

    public static void Register() {
        JourneyFacade.RegisterDatabase(
            MssqlDb.Name,
            async (cs, _) => await new MssqlDb().Connect(cs));
    }
}
