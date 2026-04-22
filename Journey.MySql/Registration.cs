using System.Runtime.CompilerServices;
using Journey.Databases;

namespace Journey.MySql;

internal static class JourneyMySqlRegistration {
    [ModuleInitializer]
    internal static void Register() {
        JourneyFacade.RegisterDatabase(
            Mysql.Name,
            async (cs, _) => await new Mysql().Connect(cs));
        JourneyFacade.RegisterDatabase(
            Mariadb.Name,
            async (cs, _) => await new Mariadb().Connect(cs));
    }
}
