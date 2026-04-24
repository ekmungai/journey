using Journey.MySql;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyMySqlRegistration.Register();
    }
}
