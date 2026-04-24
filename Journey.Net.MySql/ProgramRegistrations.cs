using Journey.MySql;

namespace Journey.Net;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyMySqlRegistration.Register();
    }
}
