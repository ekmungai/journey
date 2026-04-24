using Journey.Mssql;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyMssqlRegistration.Register();
    }
}
