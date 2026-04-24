using Journey.Mssql;

namespace Journey.Net;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyMssqlRegistration.Register();
    }
}
